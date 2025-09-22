using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Package.Observability;
using Xunit;

namespace Package.Observability.IntegrationTests;

/// <summary>
/// Testes de integração para Tempo e OpenTelemetry Collector
/// </summary>
public class TempoCollectorIntegrationTests
{
    [Fact]
    public async Task TempoEndpoint_Configuration_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Tempo",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:TempoEndpoint"] = "http://localhost:3200",
                        ["Observability:CollectorEndpoint"] = "http://localhost:4317",
                        ["Observability:LokiUrl"] = "http://localhost:3100"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se as configurações foram aplicadas
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.TempoEndpoint.Should().Be("http://localhost:3200");
        options.CollectorEndpoint.Should().Be("http://localhost:4317");
        options.ServiceName.Should().Be("TestService-Tempo");
    }

    [Fact]
    public async Task CollectorEndpoint_Priority_ShouldBeUsedOverOtlpEndpoint()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Collector-Priority",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:OtlpEndpoint"] = "http://old-jaeger:4317",
                        ["Observability:CollectorEndpoint"] = "http://new-collector:4317",
                        ["Observability:TempoEndpoint"] = "http://tempo:3200"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se CollectorEndpoint tem prioridade
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.CollectorEndpoint.Should().Be("http://new-collector:4317");
        options.OtlpEndpoint.Should().Be("http://old-jaeger:4317");
        options.TempoEndpoint.Should().Be("http://tempo:3200");
    }

    [Fact]
    public async Task TempoEndpoint_InvalidUrl_ShouldBeValidated()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Invalid-Tempo",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:TempoEndpoint"] = "invalid-url"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        // Act & Assert
        // Deve lançar exceção durante a configuração
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var client = factory.CreateClient();
            await client.GetAsync("/WeatherForecast");
        });

        exception.Message.Should().Contain("TempoEndpoint inválido");
    }

    [Fact]
    public async Task CollectorEndpoint_InvalidUrl_ShouldBeValidated()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Invalid-Collector",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:CollectorEndpoint"] = "not-a-valid-url"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        // Act & Assert
        // Deve lançar exceção durante a configuração
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var client = factory.CreateClient();
            await client.GetAsync("/WeatherForecast");
        });

        exception.Message.Should().Contain("CollectorEndpoint inválido");
    }

    [Fact]
    public async Task HealthChecks_WithTempoAndCollector_ShouldIncludeNewEndpoints()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Health-Tempo",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:TempoEndpoint"] = "http://localhost:3200",
                        ["Observability:CollectorEndpoint"] = "http://localhost:4317",
                        ["Observability:LokiUrl"] = "http://localhost:3100"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");
        var healthContent = await healthResponse.Content.ReadAsStringAsync();

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        healthContent.Should().Contain("TempoEndpoint");
        healthContent.Should().Contain("CollectorEndpoint");
        healthContent.Should().Contain("http://localhost:3200");
        healthContent.Should().Contain("http://localhost:4317");
    }

    [Fact]
    public async Task TracingHealthCheck_WithTempoAndCollector_ShouldValidateEndpoints()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Tracing-Health",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:TempoEndpoint"] = "http://localhost:3200",
                        ["Observability:CollectorEndpoint"] = "http://localhost:4317"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");
        var healthContent = await healthResponse.Content.ReadAsStringAsync();

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        healthContent.Should().Contain("TempoEndpoint");
        healthContent.Should().Contain("CollectorEndpoint");
        healthContent.Should().Contain("http://localhost:3200");
        healthContent.Should().Contain("http://localhost:4317");
    }

    [Fact]
    public async Task CodeConfiguration_WithTempoAndCollector_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddObservability(options =>
                    {
                        options.ServiceName = "TestService-Code-Tempo";
                        options.EnableMetrics = true;
                        options.EnableTracing = true;
                        options.EnableLogging = true;
                        options.PrometheusPort = GetFreeTcpPort();
                        options.TempoEndpoint = "http://tempo:3200";
                        options.CollectorEndpoint = "http://collector:4317";
                        options.LokiUrl = "http://loki:3100";
                        options.AdditionalLabels.Add("environment", "test");
                        options.AdditionalLabels.Add("component", "tempo-collector");
                    });
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se as configurações foram aplicadas
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.TempoEndpoint.Should().Be("http://tempo:3200");
        options.CollectorEndpoint.Should().Be("http://collector:4317");
        options.LokiUrl.Should().Be("http://loki:3100");
        options.AdditionalLabels.Should().ContainKey("environment");
        options.AdditionalLabels.Should().ContainKey("component");
    }

    [Fact]
    public async Task FallbackToOtlpEndpoint_WhenCollectorEndpointIsEmpty_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Fallback",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:CollectorEndpoint"] = "", // Vazio
                        ["Observability:OtlpEndpoint"] = "http://fallback-jaeger:4317",
                        ["Observability:TempoEndpoint"] = "http://tempo:3200"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se OtlpEndpoint é usado como fallback
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.CollectorEndpoint.Should().Be("");
        options.OtlpEndpoint.Should().Be("http://fallback-jaeger:4317");
        options.TempoEndpoint.Should().Be("http://tempo:3200");
    }

    [Fact]
    public async Task NoTracingEndpoints_ShouldShowWarningInHealthCheck()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-No-Endpoints",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:CollectorEndpoint"] = "", // Vazio
                        ["Observability:OtlpEndpoint"] = "", // Vazio
                        ["Observability:TempoEndpoint"] = "http://tempo:3200"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");
        var healthContent = await healthResponse.Content.ReadAsStringAsync();

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        healthContent.Should().Contain("Nenhum endpoint de tracing configurado");
    }

    [Fact]
    public async Task TempoCollector_ProductionConfiguration_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Production",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:TempoEndpoint"] = "http://tempo.monitoring.svc.cluster.local:3200",
                        ["Observability:CollectorEndpoint"] = "http://otel-collector.monitoring.svc.cluster.local:4317",
                        ["Observability:LokiUrl"] = "http://loki.monitoring.svc.cluster.local:3100",
                        ["Observability:AdditionalLabels:environment"] = "production",
                        ["Observability:AdditionalLabels:cluster"] = "k8s-prod",
                        ["Observability:AdditionalLabels:version"] = "1.0.0"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se as configurações de produção foram aplicadas
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.TempoEndpoint.Should().Be("http://tempo.monitoring.svc.cluster.local:3200");
        options.CollectorEndpoint.Should().Be("http://otel-collector.monitoring.svc.cluster.local:4317");
        options.LokiUrl.Should().Be("http://loki.monitoring.svc.cluster.local:3100");
        options.AdditionalLabels.Should().ContainKey("environment");
        options.AdditionalLabels.Should().ContainKey("cluster");
        options.AdditionalLabels.Should().ContainKey("version");
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
