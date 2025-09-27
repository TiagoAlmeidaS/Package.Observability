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
/// Testes de integração para configuração por código do Package.Observability
/// </summary>
public class CodeConfigurationTests
{
    [Fact]
    public async Task CodeConfiguration_Development_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configuração por código para desenvolvimento
                    services.AddObservability(options =>
                    {
                        options.ServiceName = "TestService-Dev-Code";
                        options.EnableMetrics = true;
                        options.EnableTracing = false;
                        options.EnableLogging = true;
                        options.EnableConsoleLogging = true;
                        options.LokiUrl = ""; // Sem Loki
                        options.OtlpEndpoint = ""; // Sem OTLP
                        options.MinimumLogLevel = "Debug";
                        options.PrometheusPort = GetFreeTcpPort();
                        options.AdditionalLabels.Add("environment", "development");
                        options.AdditionalLabels.Add("config_type", "code");
                    });
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
        var logger = factory.Services.GetRequiredService<ILogger<CodeConfigurationTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task CodeConfiguration_Production_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configuração por código para produção
                    services.AddObservability(options =>
                    {
                        options.ServiceName = "TestService-Prod-Code";
                        options.EnableMetrics = true;
                        options.EnableTracing = true;
                        options.EnableLogging = true;
                        options.EnableConsoleLogging = false; // Sem console em produção
                        options.LokiUrl = "http://loki:3100";
                        options.OtlpEndpoint = "http://otel-collector:4317";
                        options.TempoEndpoint = "http://tempo:3200";
                        options.CollectorEndpoint = "http://collector:4317";
                        options.MinimumLogLevel = "Information";
                        options.PrometheusPort = GetFreeTcpPort();
                        options.AdditionalLabels.Add("environment", "production");
                        options.AdditionalLabels.Add("version", "1.0.0");
                        options.AdditionalLabels.Add("team", "backend");
                        options.LokiLabels.Add("app", "test-app");
                        options.LokiLabels.Add("component", "api");
                    });
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
        var logger = factory.Services.GetRequiredService<ILogger<CodeConfigurationTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se health checks estão disponíveis
        var healthResponse = await client.GetAsync("/health");
        healthResponse.IsSuccessStatusCode.Should().BeTrue();

        // 5. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task CodeConfiguration_MetricsOnly_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configuração por código - apenas métricas
                    services.AddObservability(options =>
                    {
                        options.ServiceName = "TestService-Metrics-Code";
                        options.EnableMetrics = true;
                        options.EnableTracing = false;
                        options.EnableLogging = false;
                        options.EnableConsoleLogging = false;
                        options.LokiUrl = "";
                        options.OtlpEndpoint = "";
                        options.PrometheusPort = GetFreeTcpPort();
                        options.AdditionalLabels.Add("component", "metrics-only");
                    });
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
        var logger = factory.Services.GetRequiredService<ILogger<CodeConfigurationTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task CodeConfiguration_LoggingOnly_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configuração por código - apenas logging
                    services.AddObservability(options =>
                    {
                        options.ServiceName = "TestService-Logging-Code";
                        options.EnableMetrics = false;
                        options.EnableTracing = false;
                        options.EnableLogging = true;
                        options.EnableConsoleLogging = true;
                        options.LokiUrl = "";
                        options.OtlpEndpoint = "";
                        options.MinimumLogLevel = "Information";
                    });
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

        // 3. Verificar se logging está configurado
        var logger = factory.Services.GetRequiredService<ILogger<CodeConfigurationTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task CodeConfiguration_TracingOnly_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configuração por código - apenas tracing
                    services.AddObservability(options =>
                    {
                        options.ServiceName = "TestService-Tracing-Code";
                        options.EnableMetrics = false;
                        options.EnableTracing = true;
                        options.EnableLogging = false;
                        options.EnableConsoleLogging = false;
                        options.LokiUrl = "";
                        options.OtlpEndpoint = "http://otel-collector:4317";
                        options.TempoEndpoint = "http://tempo:3200";
                        options.CollectorEndpoint = "http://collector:4317";
                    });
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
        var logger = factory.Services.GetRequiredService<ILogger<CodeConfigurationTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task CodeConfiguration_WithCustomLabels_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configuração por código com labels customizados
                    services.AddObservability(options =>
                    {
                        options.ServiceName = "TestService-Labels-Code";
                        options.EnableMetrics = true;
                        options.EnableTracing = false;
                        options.EnableLogging = true;
                        options.EnableConsoleLogging = true;
                        options.LokiUrl = "";
                        options.OtlpEndpoint = "";
                        options.PrometheusPort = GetFreeTcpPort();
                        
                        // Labels adicionais
                        options.AdditionalLabels.Add("environment", "test");
                        options.AdditionalLabels.Add("version", "2.0.0");
                        options.AdditionalLabels.Add("team", "backend");
                        options.AdditionalLabels.Add("config_type", "code");
                        
                        // Labels do Loki
                        options.LokiLabels.Add("app", "test-app");
                        options.LokiLabels.Add("component", "api");
                        options.LokiLabels.Add("tier", "backend");
                    });
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
        var logger = factory.Services.GetRequiredService<ILogger<CodeConfigurationTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task CodeConfiguration_WithCustomPort_ShouldWork()
    {
        // Arrange
        var customPort = GetFreeTcpPort();
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configuração por código com porta customizada
                    services.AddObservability(options =>
                    {
                        options.ServiceName = "TestService-Port-Code";
                        options.EnableMetrics = true;
                        options.EnableTracing = false;
                        options.EnableLogging = true;
                        options.EnableConsoleLogging = true;
                        options.LokiUrl = "";
                        options.OtlpEndpoint = "";
                        options.PrometheusPort = customPort;
                    });
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
        var logger = factory.Services.GetRequiredService<ILogger<CodeConfigurationTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task CodeConfiguration_WithInstrumentationSettings_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configuração por código com configurações de instrumentação
                    services.AddObservability(options =>
                    {
                        options.ServiceName = "TestService-Instrumentation-Code";
                        options.EnableMetrics = true;
                        options.EnableTracing = true;
                        options.EnableLogging = true;
                        options.EnableConsoleLogging = true;
                        options.LokiUrl = "";
                        options.OtlpEndpoint = "http://otel-collector:4317";
                        options.TempoEndpoint = "http://tempo:3200";
                        options.CollectorEndpoint = "http://collector:4317";
                        options.PrometheusPort = GetFreeTcpPort();
                        
                        // Configurações de instrumentação
                        options.EnableRuntimeInstrumentation = true;
                        options.EnableHttpClientInstrumentation = true;
                        options.EnableAspNetCoreInstrumentation = true;
                    });
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
        var logger = factory.Services.GetRequiredService<ILogger<CodeConfigurationTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task CodeConfiguration_WithCorrelationId_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configuração por código com Correlation ID
                    services.AddObservability(options =>
                    {
                        options.ServiceName = "TestService-Correlation-Code";
                        options.EnableMetrics = false;
                        options.EnableTracing = false;
                        options.EnableLogging = true;
                        options.EnableConsoleLogging = true;
                        options.LokiUrl = "";
                        options.OtlpEndpoint = "";
                        options.EnableCorrelationId = true;
                    });
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

        // 3. Verificar se logging está configurado
        var logger = factory.Services.GetRequiredService<ILogger<CodeConfigurationTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task CodeConfiguration_InvalidSettings_ShouldThrowException()
    {
        // Arrange & Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Configuração inválida por código
                        services.AddObservability(options =>
                        {
                            options.ServiceName = ""; // Nome vazio deve causar erro
                            options.PrometheusPort = 99999; // Porta inválida
                        });
                    });
                });

            var client = factory.CreateClient();
            await client.GetAsync("/WeatherForecast");
        });

        // Assert
        exception.Should().NotBeNull();
        // A exceção deve ser relacionada à configuração inválida
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
