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
/// Testes de integração para cenários sem Loki (apenas console logging)
/// </summary>
public class WithoutLokiScenariosTests
{
    [Fact]
    public async Task ConsoleOnlyLogging_ShouldWorkWithoutLoki()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-ConsoleOnly",
                        ["Observability:EnableMetrics"] = "false",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:LokiUrl"] = "", // Sem Loki
                        ["Observability:OtlpEndpoint"] = "", // Sem OTLP
                        ["Observability:MinimumLogLevel"] = "Debug"
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

        // 3. Verificar se logging está configurado (console)
        var logger = factory.Services.GetRequiredService<ILogger<WithoutLokiScenariosTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task ConsoleAndFileLogging_ShouldWorkWithoutLoki()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-ConsoleFile",
                        ["Observability:EnableMetrics"] = "false",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:LokiUrl"] = "", // Sem Loki
                        ["Observability:OtlpEndpoint"] = "", // Sem OTLP
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

        // 2. Verificar se logging está configurado
        var logger = factory.Services.GetRequiredService<ILogger<WithoutLokiScenariosTests>>();
        logger.Should().NotBeNull();

        // 3. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task MetricsWithConsoleLogging_ShouldWorkWithoutLoki()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-MetricsConsole",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:LokiUrl"] = "", // Sem Loki
                        ["Observability:OtlpEndpoint"] = "", // Sem OTLP
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

        // 2. Verificar se métricas estão disponíveis (endpoint deve retornar 200)
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var metrics = await metricsResponse.Content.ReadAsStringAsync();
        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");

        // 3. Verificar se logging está configurado (console)
        var logger = factory.Services.GetRequiredService<ILogger<WithoutLokiScenariosTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task TracingWithConsoleLogging_ShouldWorkWithoutLoki()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-TracingConsole",
                        ["Observability:EnableMetrics"] = "false",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:LokiUrl"] = "", // Sem Loki
                        ["Observability:OtlpEndpoint"] = "http://otel-collector:4317" // Com OTLP
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

        // 3. Verificar se logging está configurado (console)
        var logger = factory.Services.GetRequiredService<ILogger<WithoutLokiScenariosTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task CompleteWithoutLoki_ShouldWorkWithAllComponentsExceptLoki()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-CompleteNoLoki",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:LokiUrl"] = "", // Sem Loki
                        ["Observability:OtlpEndpoint"] = "http://jaeger:4317", // Com OTLP
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
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

        // 2. Verificar se métricas estão disponíveis (endpoint deve retornar 200)
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var metrics = await metricsResponse.Content.ReadAsStringAsync();
        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");

        // 3. Verificar se logging está configurado (console)
        var logger = factory.Services.GetRequiredService<ILogger<WithoutLokiScenariosTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se health checks estão disponíveis
        var healthResponse = await client.GetAsync("/health");
        healthResponse.IsSuccessStatusCode.Should().BeTrue();

        // 5. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task DevelopmentScenario_ShouldUseConsoleLoggingOnly()
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
                        ["Observability:LokiUrl"] = "", // Sem Loki para desenvolvimento
                        ["Observability:OtlpEndpoint"] = "", // Sem OTLP para desenvolvimento
                        ["Observability:MinimumLogLevel"] = "Debug",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:AdditionalLabels:environment"] = "development"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas estão disponíveis (endpoint deve retornar 200)
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var metrics = await metricsResponse.Content.ReadAsStringAsync();
        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");

        // 3. Verificar se logging está configurado (console)
        var logger = factory.Services.GetRequiredService<ILogger<WithoutLokiScenariosTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task SimpleApplication_ShouldWorkWithMinimalConfiguration()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Simple",
                        ["Observability:EnableMetrics"] = "false",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:LokiUrl"] = "", // Sem Loki
                        ["Observability:OtlpEndpoint"] = "" // Sem OTLP
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

        // 3. Verificar se logging está configurado (console)
        var logger = factory.Services.GetRequiredService<ILogger<WithoutLokiScenariosTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task ErrorHandling_ShouldWorkWithoutLoki()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Error",
                        ["Observability:EnableMetrics"] = "false",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:LokiUrl"] = "", // Sem Loki
                        ["Observability:OtlpEndpoint"] = "" // Sem OTLP
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se erro retorna 500
        var errorResponse = await client.GetAsync("/WeatherForecast/error");
        errorResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        // 3. Verificar se logging está configurado (console)
        var logger = factory.Services.GetRequiredService<ILogger<WithoutLokiScenariosTests>>();
        logger.Should().NotBeNull();
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
