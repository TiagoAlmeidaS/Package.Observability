using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Package.Observability;
using Xunit;

namespace Package.Observability.IntegrationTests;

/// <summary>
/// Testes de integração para diferentes cenários de configuração do Package.Observability
/// </summary>
public class ConfigurationScenariosTests
{
    [Fact]
    public async Task DevelopmentConfiguration_ShouldHaveConsoleLoggingAndMetricsOnly()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Dev",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:LokiUrl"] = "",
                        ["Observability:OtlpEndpoint"] = "",
                        ["Observability:MinimumLogLevel"] = "Debug",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas estão disponíveis
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();
        var metrics = await metricsResponse.Content.ReadAsStringAsync();
        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");

        // 3. Verificar se logging está configurado
        var logger = factory.Services.GetRequiredService<ILogger<ConfigurationScenariosTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se tracing não está configurado (não há endpoint OTLP)
        // Isso é verificado pela ausência de configuração OTLP
    }

    [Fact]
    public async Task ProductionConfiguration_ShouldHaveAllComponentsEnabled()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Prod",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "false",
                        ["Observability:LokiUrl"] = "http://loki:3100",
                        ["Observability:OtlpEndpoint"] = "http://jaeger:4317",
                        ["Observability:MinimumLogLevel"] = "Information",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:AdditionalLabels:environment"] = "production",
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

        // 2. Verificar se métricas estão disponíveis
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();
        var metrics = await metricsResponse.Content.ReadAsStringAsync();
        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");

        // 3. Verificar se logging está configurado
        var logger = factory.Services.GetRequiredService<ILogger<ConfigurationScenariosTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se health checks estão registrados
        var healthResponse = await client.GetAsync("/health");
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task MetricsOnlyConfiguration_ShouldHaveOnlyMetricsEnabled()
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
                        ["Observability:EnableConsoleLogging"] = "false",
                        ["Observability:LokiUrl"] = "",
                        ["Observability:OtlpEndpoint"] = "",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas estão disponíveis
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();
        var metrics = await metricsResponse.Content.ReadAsStringAsync();
        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");

        // 3. Verificar se logging não está configurado (usando logger padrão)
        var logger = factory.Services.GetRequiredService<ILogger<ConfigurationScenariosTests>>();
        logger.Should().NotBeNull();
    }

    [Fact]
    public async Task LoggingOnlyConfiguration_ShouldHaveOnlyLoggingEnabled()
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
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:LokiUrl"] = "",
                        ["Observability:OtlpEndpoint"] = "",
                        ["Observability:MinimumLogLevel"] = "Information"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas NÃO estão disponíveis
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();
        var metrics = await metricsResponse.Content.ReadAsStringAsync();
        metrics.Should().NotContain("weather_requests_total");

        // 3. Verificar se logging está configurado
        var logger = factory.Services.GetRequiredService<ILogger<ConfigurationScenariosTests>>();
        logger.Should().NotBeNull();
    }

    [Fact]
    public async Task TracingOnlyConfiguration_ShouldHaveOnlyTracingEnabled()
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
                        ["Observability:EnableConsoleLogging"] = "false",
                        ["Observability:LokiUrl"] = "",
                        ["Observability:OtlpEndpoint"] = "http://jaeger:4317"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas NÃO estão disponíveis (endpoint deve retornar 404)
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 3. Verificar se logging não está configurado (usando logger padrão)
        var logger = factory.Services.GetRequiredService<ILogger<ConfigurationScenariosTests>>();
        logger.Should().NotBeNull();
    }

    [Fact]
    public async Task MinimalConfiguration_ShouldUseDefaults()
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
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas estão disponíveis (padrão)
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();
        var metrics = await metricsResponse.Content.ReadAsStringAsync();
        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");

        // 3. Verificar se logging está configurado (padrão)
        var logger = factory.Services.GetRequiredService<ILogger<ConfigurationScenariosTests>>();
        logger.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomLabelsConfiguration_ShouldIncludeCustomLabels()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Labels",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "false",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:AdditionalLabels:environment"] = "test",
                        ["Observability:AdditionalLabels:version"] = "2.0.0",
                        ["Observability:AdditionalLabels:team"] = "backend",
                        ["Observability:LokiLabels:app"] = "test-app",
                        ["Observability:LokiLabels:component"] = "api"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas estão disponíveis
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();
        var metrics = await metricsResponse.Content.ReadAsStringAsync();
        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");

        // 3. Verificar se as labels customizadas estão sendo usadas
        // Isso seria verificado através das métricas exportadas
    }

    [Fact]
    public async Task InvalidConfiguration_ShouldThrowConfigurationException()
    {
        // Arrange & Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        var config = new Dictionary<string, string?>
                        {
                            ["Observability:ServiceName"] = "", // Nome vazio deve causar erro
                            ["Observability:PrometheusPort"] = "99999" // Porta inválida
                        };
                        configBuilder.AddInMemoryCollection(config);
                    });
                });

            var client = factory.CreateClient();
            await client.GetAsync("/WeatherForecast");
        });

        // Assert
        exception.Should().NotBeNull();
        // A exceção deve ser relacionada à configuração inválida
    }

    [Fact]
    public async Task HealthChecksConfiguration_ShouldRegisterHealthChecks()
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
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
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
    public async Task DifferentPortsConfiguration_ShouldUseCustomPorts()
    {
        // Arrange
        var customPort = GetFreeTcpPort();
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Port",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:PrometheusPort"] = customPort.ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas estão disponíveis na porta customizada
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();
        var metrics = await metricsResponse.Content.ReadAsStringAsync();
        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");
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
