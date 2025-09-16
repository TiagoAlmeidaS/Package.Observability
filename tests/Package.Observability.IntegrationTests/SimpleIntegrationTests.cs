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
/// Testes de integração simplificados para verificar funcionalidades básicas
/// </summary>
public class SimpleIntegrationTests : IClassFixture<ExampleApiFactory>
{
    private readonly ExampleApiFactory _factory;

    public SimpleIntegrationTests(ExampleApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Application_ShouldStart_WithBasicConfiguration()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Basic",
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
        var response = await client.GetAsync("/WeatherForecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Metrics_ShouldBeExposed_WhenEnabled()
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
        var response = await client.GetAsync("/WeatherForecast");
        var metricsResponse = await client.GetAsync("/metrics");
        var metrics = await metricsResponse.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();
        metrics.Should().NotBeEmpty();
        metrics.Should().Contain("# TYPE");
    }

    [Fact]
    public async Task HealthChecks_ShouldWork_WhenEnabled()
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
    }

    [Fact]
    public async Task LoggingOnly_ShouldWork_WithoutLoki()
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
                        ["Observability:LokiUrl"] = ""
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/WeatherForecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
