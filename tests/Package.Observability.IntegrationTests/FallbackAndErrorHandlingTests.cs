using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Package.Observability;
using Package.Observability.Exceptions;
using Xunit;

namespace Package.Observability.IntegrationTests;

/// <summary>
/// Testes de integração para cenários de fallback e tratamento de erros
/// </summary>
public class FallbackAndErrorHandlingTests
{
    [Fact]
    public async Task InvalidConfiguration_ShouldThrowConfigurationException()
    {
        // Arrange & Act
        var exception = await Assert.ThrowsAsync<ObservabilityConfigurationException>(async () =>
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
    public async Task MissingServiceName_ShouldThrowConfigurationException()
    {
        // Arrange & Act
        var exception = await Assert.ThrowsAsync<ObservabilityConfigurationException>(async () =>
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        var config = new Dictionary<string, string?>
                        {
                            // ServiceName ausente deve causar erro
                            ["Observability:EnableMetrics"] = "true"
                        };
                        configBuilder.AddInMemoryCollection(config);
                    });
                });

            var client = factory.CreateClient();
            await client.GetAsync("/WeatherForecast");
        });

        // Assert
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task InvalidPort_ShouldThrowConfigurationException()
    {
        // Arrange & Act
        var exception = await Assert.ThrowsAsync<ObservabilityConfigurationException>(async () =>
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        var config = new Dictionary<string, string?>
                        {
                            ["Observability:ServiceName"] = "TestService",
                            ["Observability:PrometheusPort"] = "-1" // Porta inválida
                        };
                        configBuilder.AddInMemoryCollection(config);
                    });
                });

            var client = factory.CreateClient();
            await client.GetAsync("/WeatherForecast");
        });

        // Assert
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task InvalidLokiUrl_ShouldThrowConfigurationException()
    {
        // Arrange & Act
        var exception = await Assert.ThrowsAsync<ObservabilityConfigurationException>(async () =>
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        var config = new Dictionary<string, string?>
                        {
                            ["Observability:ServiceName"] = "TestService",
                            ["Observability:EnableLogging"] = "true",
                            ["Observability:LokiUrl"] = "invalid-url" // URL inválida
                        };
                        configBuilder.AddInMemoryCollection(config);
                    });
                });

            var client = factory.CreateClient();
            await client.GetAsync("/WeatherForecast");
        });

        // Assert
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task InvalidOtlpEndpoint_ShouldThrowConfigurationException()
    {
        // Arrange & Act
        var exception = await Assert.ThrowsAsync<ObservabilityConfigurationException>(async () =>
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        var config = new Dictionary<string, string?>
                        {
                            ["Observability:ServiceName"] = "TestService",
                            ["Observability:EnableTracing"] = "true",
                            ["Observability:OtlpEndpoint"] = "invalid-endpoint" // Endpoint inválido
                        };
                        configBuilder.AddInMemoryCollection(config);
                    });
                });

            var client = factory.CreateClient();
            await client.GetAsync("/WeatherForecast");
        });

        // Assert
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task InvalidLogLevel_ShouldThrowConfigurationException()
    {
        // Arrange & Act
        var exception = await Assert.ThrowsAsync<ObservabilityConfigurationException>(async () =>
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        var config = new Dictionary<string, string?>
                        {
                            ["Observability:ServiceName"] = "TestService",
                            ["Observability:EnableLogging"] = "true",
                            ["Observability:MinimumLogLevel"] = "InvalidLevel" // Nível inválido
                        };
                        configBuilder.AddInMemoryCollection(config);
                    });
                });

            var client = factory.CreateClient();
            await client.GetAsync("/WeatherForecast");
        });

        // Assert
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidConfiguration_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Valid",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
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

        // 3. Verificar se logging está configurado
        var logger = factory.Services.GetRequiredService<ILogger<FallbackAndErrorHandlingTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task PartialConfiguration_ShouldUseDefaults()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Partial"
                        // Outras configurações usarão defaults
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

        // 3. Verificar se logging está configurado (padrão)
        var logger = factory.Services.GetRequiredService<ILogger<FallbackAndErrorHandlingTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task EmptyConfiguration_ShouldUseDefaults()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        // Configuração vazia - deve usar defaults
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

        // 3. Verificar se logging está configurado (padrão)
        var logger = factory.Services.GetRequiredService<ILogger<FallbackAndErrorHandlingTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task ConfigurationWithWarnings_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Warnings",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:PrometheusPort"] = "1", // Porta baixa (warning)
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:LokiUrl"] = "", // URL vazia com logging habilitado (warning)
                        ["Observability:OtlpEndpoint"] = "" // Endpoint vazio com tracing habilitado (warning)
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros (apenas warnings)
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas estão disponíveis
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();

        // 3. Verificar se logging está configurado
        var logger = factory.Services.GetRequiredService<ILogger<FallbackAndErrorHandlingTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task ConfigurationWithInvalidLabels_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-InvalidLabels",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:AdditionalLabels:invalid-label"] = "value", // Label inválida (warning)
                        ["Observability:LokiLabels:invalid-loki-label"] = "value" // Label Loki inválida (warning)
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros (apenas warnings)
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas estão disponíveis
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();

        // 3. Verificar se logging está configurado
        var logger = factory.Services.GetRequiredService<ILogger<FallbackAndErrorHandlingTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
    }

    [Fact]
    public async Task ConfigurationWithLongServiceName_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-With-Very-Long-Name-That-Should-Generate-Warning-But-Should-Still-Work",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros (apenas warnings)
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se métricas estão disponíveis
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();

        // 3. Verificar se logging está configurado
        var logger = factory.Services.GetRequiredService<ILogger<FallbackAndErrorHandlingTests>>();
        logger.Should().NotBeNull();

        // 4. Verificar se a aplicação funciona normalmente
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC");
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
