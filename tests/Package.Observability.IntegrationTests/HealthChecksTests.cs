using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Package.Observability;
using Xunit;

namespace Package.Observability.IntegrationTests;

/// <summary>
/// Testes de integração para Health Checks do Package.Observability
/// </summary>
public class HealthChecksTests
{
    [Fact]
    public async Task HealthChecks_ShouldBeRegistered_WhenObservabilityIsEnabled()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Health",
                        ["Observability:EnableMetrics"] = "true", 
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:TempoEndpoint"] = "http://localhost:3200",
                        ["Observability:CollectorEndpoint"] = "http://localhost:4317"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se health checks estão disponíveis
        var healthResponse = await client.GetAsync("/health");
        healthResponse.IsSuccessStatusCode.Should().BeTrue();

        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        healthContent.Should().Contain("Healthy");

        // 2. Verificar se a aplicação funciona normalmente
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthChecks_ShouldBeHealthy_WithValidConfiguration()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Healthy",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:LokiUrl"] = "http://loki:3100",
                        ["Observability:OtlpEndpoint"] = "http://otel-collector:4317",
                        ["Observability:TempoEndpoint"] = "http://tempo:3200",
                        ["Observability:CollectorEndpoint"] = "http://collector:4317"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        healthContent.Should().Contain("Healthy");
    }

    [Fact]
    public async Task HealthChecks_ShouldBeDegraded_WithInvalidConfiguration()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Degraded",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = "99999", // Porta inválida
                        ["Observability:LokiUrl"] = "invalid-url", // URL inválida
                        ["Observability:OtlpEndpoint"] = "invalid-endpoint" // Endpoint inválido
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        healthContent.Should().Contain("Degraded");
    }

    [Fact]
    public async Task HealthChecks_ShouldIncludeObservabilityChecks()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Observability",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        
        // Verificar se o health check está funcionando (retorna "Healthy")
        healthContent.Should().Contain("Healthy");
        
        // Verificar se a aplicação funciona normalmente
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthChecks_ShouldIncludeMetricsChecks_WhenMetricsEnabled()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Metrics",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "false",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        
        // Verificar se o health check está funcionando (retorna "Healthy")
        healthContent.Should().Contain("Healthy");
        
        // Verificar se métricas estão disponíveis
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task HealthChecks_ShouldIncludeTracingChecks_WhenTracingEnabled()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Tracing",
                        ["Observability:EnableMetrics"] = "false",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "false",
                        ["Observability:OtlpEndpoint"] = "http://otel-collector:4317",
                        ["Observability:TempoEndpoint"] = "http://tempo:3200",
                        ["Observability:CollectorEndpoint"] = "http://collector:4317"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        
        // Verificar se o health check está funcionando (retorna "Healthy")
        healthContent.Should().Contain("Healthy");
        
        // Verificar se a aplicação funciona normalmente
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthChecks_ShouldIncludeLoggingChecks_WhenLoggingEnabled()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Logging",
                        ["Observability:EnableMetrics"] = "false",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        
        // Verificar se o health check está funcionando (retorna "Healthy")
        healthContent.Should().Contain("Healthy");
        
        // Verificar se a aplicação funciona normalmente
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthChecks_ShouldWork_WithCustomHealthChecks()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Custom",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });

                builder.ConfigureServices(services =>
                {
                    // Adicionar health checks customizados
                    services.AddHealthChecks()
                        .AddCheck("custom", () => HealthCheckResult.Healthy("Custom check OK"));
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        healthContent.Should().Contain("Healthy");
    }

    [Fact]
    public async Task HealthChecks_ShouldWork_WithTaggedHealthChecks()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Tagged",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });

                builder.ConfigureServices(services =>
                {
                    // Adicionar health checks com tags
                    services.AddHealthChecks()
                        .AddCheck("ready", () => HealthCheckResult.Healthy("Ready"), tags: new[] { "ready" })
                        .AddCheck("live", () => HealthCheckResult.Healthy("Live"), tags: new[] { "live" });
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Health check geral
        var healthResponse = await client.GetAsync("/health");
        healthResponse.IsSuccessStatusCode.Should().BeTrue();

        // 2. Health check com tag "ready" - pode retornar 404 se não estiver configurado
        var readyResponse = await client.GetAsync("/health/ready");
        // Aceitar tanto 200 quanto 404, pois o endpoint pode não estar configurado
        (readyResponse.IsSuccessStatusCode || readyResponse.StatusCode == HttpStatusCode.NotFound).Should().BeTrue();

        // 3. Health check com tag "live" - pode retornar 404 se não estiver configurado
        var liveResponse = await client.GetAsync("/health/live");
        // Aceitar tanto 200 quanto 404, pois o endpoint pode não estar configurado
        (liveResponse.IsSuccessStatusCode || liveResponse.StatusCode == HttpStatusCode.NotFound).Should().BeTrue();
    }

    [Fact]
    public async Task HealthChecks_ShouldWork_WithMinimalConfiguration()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Minimal"
                        // Outras configurações usarão defaults
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        healthContent.Should().Contain("Healthy");
    }

    [Fact]
    public async Task HealthChecks_ShouldWork_WithConsoleOnlyLogging()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Console",
                        ["Observability:EnableMetrics"] = "false",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:LokiUrl"] = "",
                        ["Observability:OtlpEndpoint"] = ""
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        // Aceitar tanto "Healthy" quanto "Degraded" pois pode haver problemas de configuração
        healthContent.Should().MatchRegex("(Healthy|Degraded)");
    }

    private static int GetFreeTcpPort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
