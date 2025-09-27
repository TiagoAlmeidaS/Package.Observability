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
/// Testes de cenários reais com controllers e serviços
/// Demonstra telemetria automática em situações práticas
/// </summary>
public class RealWorldScenariosTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RealWorldScenariosTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task WeatherService_CompleteWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "WeatherService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                // A observabilidade já está registrada no Program.cs
                // Não precisamos registrar novamente para evitar duplicação
            });
        }).CreateClient();

        // Act & Assert - Fluxo completo de trabalho
        // 1. Obter previsão para múltiplos dias
        var forecastResponse = await client.GetAsync("/api/AutoWeatherService/forecast/7");
        forecastResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Obter previsão para data específica
        var specificDate = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd");
        var specificDateResponse = await client.GetAsync($"/api/AutoWeatherService/forecast/date/{specificDate}");
        specificDateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Validar previsão
        var forecast = new
        {
            Date = specificDate,
            TemperatureC = 25,
            Summary = "Warm"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(forecast);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var validateResponse = await client.PostAsync("/api/AutoWeatherService/validate", content);
        validateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Criar nova previsão
        var createResponse = await client.PostAsync("/api/AutoWeatherService/create", content);
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        // 5. Atualizar previsão
        var updateResponse = await client.PutAsync("/api/AutoWeatherService/update/1", content);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Excluir previsão
        var deleteResponse = await client.DeleteAsync("/api/AutoWeatherService/delete/1");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task WeatherService_ErrorHandling_ShouldWorkCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "WeatherService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                // A observabilidade já está registrada no Program.cs
                // Não precisamos registrar novamente para evitar duplicação
            });
        }).CreateClient();

        // Act & Assert - Cenários de erro
        // 1. Dias inválidos (negativo)
        var negativeDaysResponse = await client.GetAsync("/api/AutoWeatherService/forecast/-1");
        negativeDaysResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 2. Dias inválidos (muito alto)
        var tooManyDaysResponse = await client.GetAsync("/api/AutoWeatherService/forecast/100");
        tooManyDaysResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 3. Data passada
        var pastDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        var pastDateResponse = await client.GetAsync($"/api/AutoWeatherService/forecast/date/{pastDate}");
        pastDateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 4. Validação com dados inválidos
        var invalidForecast = new
        {
            Date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
            TemperatureC = 999, // Temperatura inválida
            Summary = "" // Summary vazio
        };

        var invalidJson = System.Text.Json.JsonSerializer.Serialize(invalidForecast);
        var invalidContent = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");

        var invalidValidateResponse = await client.PostAsync("/api/AutoWeatherService/validate", invalidContent);
        invalidValidateResponse.StatusCode.Should().Be(HttpStatusCode.OK); // Deve retornar OK mas com valid=false

        // 5. ID inválido para atualização
        var invalidUpdateResponse = await client.PutAsync("/api/AutoWeatherService/update/0", invalidContent);
        invalidUpdateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 6. ID inválido para exclusão
        var invalidDeleteResponse = await client.DeleteAsync("/api/AutoWeatherService/delete/0");
        invalidDeleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WeatherService_PerformanceTest_ShouldHandleLoad()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "WeatherService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                // A observabilidade já está registrada no Program.cs
                // Não precisamos registrar novamente para evitar duplicação
            });
        }).CreateClient();

        // Act - Teste de performance com múltiplas requisições
        var tasks = new List<Task<HttpResponseMessage>>();
        var startTime = DateTime.UtcNow;

        // Fazer 50 requisições simultâneas
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(client.GetAsync("/api/AutoWeather"));
        }

        var responses = await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        responses.Should().HaveCount(50);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
        duration.TotalSeconds.Should().BeLessThan(10); // Deve completar em menos de 10 segundos
    }

    [Fact]
    public async Task WeatherService_ConcurrentOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "WeatherService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                // A observabilidade já está registrada no Program.cs
                // Não precisamos registrar novamente para evitar duplicação
            });
        }).CreateClient();

        // Act - Operações concorrentes
        var tasks = new List<Task<HttpResponseMessage>>();

        // Múltiplas operações diferentes simultâneas
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(client.GetAsync("/api/AutoWeather")); // GET
            tasks.Add(client.GetAsync($"/api/AutoWeatherService/forecast/{i + 1}")); // GET com parâmetro
            tasks.Add(client.GetAsync("/api/AutoWeatherService/forecast/date/2024-12-25")); // GET com data
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().HaveCount(30);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task WeatherService_DataConsistency_ShouldMaintainCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "WeatherService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                // A observabilidade já está registrada no Program.cs
                // Não precisamos registrar novamente para evitar duplicação
            });
        }).CreateClient();

        // Act & Assert - Teste de consistência de dados
        // 1. Obter previsão para 5 dias
        var forecastResponse = await client.GetAsync("/api/AutoWeatherService/forecast/5");
        forecastResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var forecastContent = await forecastResponse.Content.ReadAsStringAsync();
        forecastContent.Should().NotBeNullOrEmpty();

        // 2. Verificar se a resposta contém dados válidos
        var forecastData = System.Text.Json.JsonSerializer.Deserialize<object[]>(forecastContent);
        forecastData.Should().NotBeNull();
        forecastData.Should().HaveCount(5);

        // 3. Obter previsão para data específica
        var specificDate = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd");
        var specificDateResponse = await client.GetAsync($"/api/AutoWeatherService/forecast/date/{specificDate}");
        specificDateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var specificDateContent = await specificDateResponse.Content.ReadAsStringAsync();
        specificDateContent.Should().NotBeNullOrEmpty();

        // 4. Verificar se a resposta contém dados válidos
        var specificDateData = System.Text.Json.JsonSerializer.Deserialize<object>(specificDateContent);
        specificDateData.Should().NotBeNull();
    }

    [Fact]
    public async Task WeatherService_LoggingVerification_ShouldLogCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "WeatherService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                // A observabilidade já está registrada no Program.cs
                // Não precisamos registrar novamente para evitar duplicação
            });
        }).CreateClient();

        // Act - Fazer requisições que devem gerar logs
        var responses = new List<HttpResponseMessage>
        {
            await client.GetAsync("/api/AutoWeather"),
            await client.GetAsync("/api/AutoWeatherService/forecast/3"),
            await client.GetAsync("/api/AutoWeatherService/forecast/date/2024-12-25"),
            await client.GetAsync("/api/AutoWeather/0") // Deve gerar erro
        };

        // Assert
        responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[1].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[2].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[3].StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Em um cenário real, você poderia verificar os logs aqui
    }

    [Fact]
    public async Task WeatherService_MetricsVerification_ShouldExposeMetrics()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "WeatherService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                // A observabilidade já está registrada no Program.cs
                // Não precisamos registrar novamente para evitar duplicação
            });
        }).CreateClient();

        // Act - Fazer algumas requisições para gerar métricas
        await client.GetAsync("/api/AutoWeather");
        await client.GetAsync("/api/AutoWeatherService/forecast/3");
        await client.GetAsync("/api/AutoWeatherService/forecast/date/2024-12-25");

        // Verificar se o endpoint de métricas está acessível
        var metricsResponse = await client.GetAsync("/metrics");

        // Assert
        metricsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var metricsContent = await metricsResponse.Content.ReadAsStringAsync();
        metricsContent.Should().NotBeNullOrEmpty();
        metricsContent.Should().Contain("http_server_request_duration_seconds"); // Deve conter métricas HTTP
    }

    [Fact]
    public async Task WeatherService_HealthCheck_ShouldBeHealthy()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "WeatherService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                // A observabilidade já está registrada no Program.cs
                // Não precisamos registrar novamente para evitar duplicação
            });
        }).CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");

        // Assert
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        healthContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task WeatherService_StressTest_ShouldHandleHighLoad()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "WeatherService",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "true",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                });
            });

            builder.ConfigureServices((context, services) =>
            {
                // A observabilidade já está registrada no Program.cs
                // Não precisamos registrar novamente para evitar duplicação
            });
        }).CreateClient();

        // Act - Teste de estresse com 100 requisições
        var tasks = new List<Task<HttpResponseMessage>>();
        var startTime = DateTime.UtcNow;

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(client.GetAsync("/api/AutoWeather"));
        }

        var responses = await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        responses.Should().HaveCount(100);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
        duration.TotalSeconds.Should().BeLessThan(30); // Deve completar em menos de 30 segundos
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
