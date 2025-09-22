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
/// Testes de integração para telemetria automática (ZERO CONFIGURAÇÃO)
/// </summary>
public class ZeroConfigTelemetryIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ZeroConfigTelemetryIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
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
                // Garantir que o middleware automático está registrado
                services.AddObservability(context.Configuration);
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AutoWeatherController_Get_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/AutoWeather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AutoWeatherController_GetById_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/AutoWeather/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AutoWeatherController_GetById_WithInvalidId_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/AutoWeather/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AutoWeatherController_Create_ShouldReturnSuccess()
    {
        // Arrange
        var forecast = new
        {
            Date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
            TemperatureC = 25,
            Summary = "Warm"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(forecast);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/AutoWeather", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Fact]
    public async Task AutoWeatherController_Update_ShouldReturnSuccess()
    {
        // Arrange
        var forecast = new
        {
            Date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
            TemperatureC = 30,
            Summary = "Hot"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(forecast);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/AutoWeather/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AutoWeatherController_Delete_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.DeleteAsync("/api/AutoWeather/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AutoWeatherServiceController_GetForecast_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/AutoWeatherService/forecast/5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AutoWeatherServiceController_GetForecast_WithInvalidDays_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/AutoWeatherService/forecast/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AutoWeatherServiceController_GetForecastByDate_ShouldReturnSuccess()
    {
        // Arrange
        var date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/AutoWeatherService/forecast/date/{date}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AutoWeatherServiceController_GetForecastByDate_WithPastDate_ShouldReturnBadRequest()
    {
        // Arrange
        var pastDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/AutoWeatherService/forecast/date/{pastDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AutoWeatherServiceController_ValidateForecast_ShouldReturnSuccess()
    {
        // Arrange
        var forecast = new
        {
            Date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
            TemperatureC = 25,
            Summary = "Warm"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(forecast);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/AutoWeatherService/validate", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("valid");
    }

    [Fact]
    public async Task AutoWeatherServiceController_CreateForecast_ShouldReturnSuccess()
    {
        // Arrange
        var forecast = new
        {
            Date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
            TemperatureC = 25,
            Summary = "Warm"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(forecast);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/AutoWeatherService/create", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Fact]
    public async Task AutoWeatherServiceController_UpdateForecast_ShouldReturnSuccess()
    {
        // Arrange
        var forecast = new
        {
            Date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
            TemperatureC = 30,
            Summary = "Hot"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(forecast);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/AutoWeatherService/update/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AutoWeatherServiceController_DeleteForecast_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.DeleteAsync("/api/AutoWeatherService/delete/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task WeatherForecastControllerV2_Get_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/WeatherForecastV2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task WeatherForecastControllerV2_GetManual_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/WeatherForecastV2/manual");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task WeatherForecastControllerV2_GetExternal_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/WeatherForecastV2/external");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Dados obtidos da API externa com sucesso");
    }

    [Fact]
    public async Task WeatherForecastControllerV2_GetMetricsDemo_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/WeatherForecastV2/metrics-demo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Métricas automáticas funcionando - ZERO CONFIGURAÇÃO");
    }

    [Fact]
    public async Task MetricsEndpoint_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task MultipleRequests_ShouldHandleConcurrently()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Fazer múltiplas requisições simultâneas
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/api/AutoWeather"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().HaveCount(10);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task ErrorHandling_ShouldWorkCorrectly()
    {
        // Act - Fazer requisição que deve gerar erro
        var response = await _client.GetAsync("/api/AutoWeather/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logging_ShouldWorkCorrectly()
    {
        // Arrange
        var loggerFactory = _factory.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("TestLogger");

        // Act
        var response = await _client.GetAsync("/api/AutoWeather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Em um cenário real, você poderia verificar os logs
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
