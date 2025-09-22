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
/// Testes de integração para middleware de telemetria automática
/// </summary>
public class ZeroConfigMiddlewareIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ZeroConfigMiddlewareIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Middleware_WithObservabilityEnabled_ShouldProcessRequests()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/AutoWeather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Middleware_WithObservabilityDisabled_ShouldStillProcessRequests()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "false",
                    ["Observability:EnableTracing"] = "false",
                    ["Observability:EnableLogging"] = "false",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/AutoWeather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Middleware_WithMetricsOnly_ShouldProcessRequests()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "false",
                    ["Observability:EnableLogging"] = "false",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/AutoWeather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Middleware_WithLoggingOnly_ShouldProcessRequests()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "false",
                    ["Observability:EnableTracing"] = "false",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/AutoWeather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Middleware_WithTracingOnly_ShouldProcessRequests()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "false",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "false",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/AutoWeather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("GET", "/api/AutoWeather")]
    [InlineData("POST", "/api/AutoWeather")]
    [InlineData("PUT", "/api/AutoWeather/1")]
    [InlineData("DELETE", "/api/AutoWeather/1")]
    public async Task Middleware_WithDifferentHttpMethods_ShouldProcessCorrectly(string method, string path)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/api/AutoWeather")]
    [InlineData("/api/AutoWeather/1")]
    [InlineData("/health")]
    [InlineData("/metrics")]
    [InlineData("/swagger")]
    public async Task Middleware_WithDifferentPaths_ShouldProcessCorrectly(string path)
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync(path);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Middleware_WithConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act - Fazer múltiplas requisições simultâneas
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(client.GetAsync("/api/AutoWeather"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().HaveCount(20);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task Middleware_WithErrorResponse_ShouldHandleCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act - Fazer requisição que deve gerar erro
        var response = await client.GetAsync("/api/AutoWeather/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Middleware_WithLargePayload_ShouldHandleCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Arrange - Criar payload grande
        var largeData = new string('A', 10000);
        var forecast = new
        {
            Date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
            TemperatureC = 25,
            Summary = largeData
        };

        var json = System.Text.Json.JsonSerializer.Serialize(forecast);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/AutoWeather", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Middleware_WithSlowRequest_ShouldHandleCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act - Fazer requisição que pode demorar
        var response = await client.GetAsync("/api/AutoWeatherService/forecast/30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Middleware_WithCustomServiceName_ShouldWorkCorrectly()
    {
        // Arrange
        var customServiceName = "CustomTestService";
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = customServiceName,
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/AutoWeather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Middleware_WithAdditionalLabels_ShouldWorkCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                    ["Observability:AdditionalLabels:environment"] = "test",
                    ["Observability:AdditionalLabels:version"] = "1.0.0"
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddObservability(context.Configuration);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/AutoWeather");

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
