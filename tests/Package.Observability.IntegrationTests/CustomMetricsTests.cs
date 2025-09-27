using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Package.Observability;
using System.Diagnostics.Metrics;
using Xunit;

namespace Package.Observability.IntegrationTests;

/// <summary>
/// Testes de integração para métricas customizadas do Package.Observability
/// </summary>
public class CustomMetricsTests
{
    [Fact]
    public async Task CustomMetrics_ShouldBeExposed_WhenCreated()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-CustomMetrics",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "false",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });

                builder.ConfigureServices(services =>
                {
                    // Registrar serviço que cria métricas customizadas
                    services.AddTransient<CustomMetricsService>();
                });
            });

        var client = factory.CreateClient();

        // Act
        // 1. Fazer requisições para gerar métricas
        var response1 = await client.GetAsync("/WeatherForecast");
        var response2 = await client.GetAsync("/WeatherForecast");
        var response3 = await client.GetAsync("/WeatherForecast/error");
        
        // 2. Chamar o serviço de métricas customizadas
        var customMetricsService = factory.Services.GetRequiredService<CustomMetricsService>();
        await customMetricsService.ProcessRequestAsync("test");
        
        // Aguardar um pouco para garantir que as métricas sejam processadas
        await Task.Delay(100);

        // 2. Obter métricas
        var metricsResponse = await client.GetAsync("/metrics");
        var metrics = await metricsResponse.Content.ReadAsStringAsync();

        // Assert
        // 1. Verificar se as requisições funcionaram
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response3.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        // 2. Verificar se métricas estão disponíveis
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();

        // 3. Verificar se métricas customizadas estão presentes
        metrics.Should().Contain("custom_requests_total");
        metrics.Should().Contain("custom_request_duration_seconds");
        metrics.Should().Contain("custom_items_in_progress");
        metrics.Should().Contain("custom_active_connections");

        // 4. Verificar se métricas automáticas estão presentes
        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");
        metrics.Should().Contain("http_server_request_duration_seconds");
    }

    [Fact]
    public async Task CustomMetrics_ShouldHaveCorrectLabels_WhenCreated()
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
                        ["Observability:AdditionalLabels:version"] = "1.0.0"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });

                builder.ConfigureServices(services =>
                {
                    services.AddTransient<CustomMetricsService>();
                });
            });

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/WeatherForecast");
        
        // Chamar o serviço de métricas customizadas
        var customMetricsService = factory.Services.GetRequiredService<CustomMetricsService>();
        await customMetricsService.ProcessRequestAsync("test");
        
        var metricsResponse = await client.GetAsync("/metrics");
        var metrics = await metricsResponse.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();

        // Verificar se métricas customizadas estão presentes
        metrics.Should().Contain("custom_requests_total");
        metrics.Should().Contain("custom_request_duration_seconds");
    }

    [Fact]
    public async Task CustomMetrics_ShouldWork_WithDifferentServiceNames()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Different",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "false",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });

                builder.ConfigureServices(services =>
                {
                    services.AddTransient<CustomMetricsService>();
                });
            });

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/WeatherForecast");
        
        // Chamar o serviço de métricas customizadas
        var customMetricsService = factory.Services.GetRequiredService<CustomMetricsService>();
        await customMetricsService.ProcessRequestAsync("test");
        
        var metricsResponse = await client.GetAsync("/metrics");
        var metrics = await metricsResponse.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();

        // Verificar se métricas customizadas estão presentes
        metrics.Should().Contain("custom_requests_total");
        metrics.Should().Contain("custom_request_duration_seconds");
    }

    [Fact]
    public async Task CustomMetrics_ShouldWork_WithConcurrentRequests()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Concurrent",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "false",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });

                builder.ConfigureServices(services =>
                {
                    services.AddTransient<CustomMetricsService>();
                });
            });

        var client = factory.CreateClient();

        // Act
        // Fazer múltiplas requisições concorrentes
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(client.GetAsync("/WeatherForecast"));
        }

        await Task.WhenAll(tasks);
        
        // Chamar o serviço de métricas customizadas
        var customMetricsService = factory.Services.GetRequiredService<CustomMetricsService>();
        await customMetricsService.ProcessRequestAsync("concurrent");

        var metricsResponse = await client.GetAsync("/metrics");
        var metrics = await metricsResponse.Content.ReadAsStringAsync();

        // Assert
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();

        // Verificar se métricas customizadas estão presentes
        metrics.Should().Contain("custom_requests_total");
        metrics.Should().Contain("custom_request_duration_seconds");
    }

    [Fact]
    public async Task CustomMetrics_ShouldWork_WithErrorScenarios()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = "TestService-Errors",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableLogging"] = "false",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString()
                    };
                    configBuilder.AddInMemoryCollection(config);
                });

                builder.ConfigureServices(services =>
                {
                    services.AddTransient<CustomMetricsService>();
                });
            });

        var client = factory.CreateClient();

        // Act
        // Fazer requisições com sucesso e erro
        var successResponse = await client.GetAsync("/WeatherForecast");
        var errorResponse = await client.GetAsync("/WeatherForecast/error");
        
        // Chamar o serviço de métricas customizadas
        var customMetricsService = factory.Services.GetRequiredService<CustomMetricsService>();
        await customMetricsService.ProcessRequestAsync("success");
        try
        {
            await customMetricsService.ProcessRequestAsync("error");
        }
        catch
        {
            // Esperado para testar cenários de erro
        }

        var metricsResponse = await client.GetAsync("/metrics");
        var metrics = await metricsResponse.Content.ReadAsStringAsync();

        // Assert
        successResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        errorResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();

        // Verificar se métricas customizadas estão presentes
        metrics.Should().Contain("custom_requests_total");
        metrics.Should().Contain("custom_request_duration_seconds");
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

/// <summary>
/// Serviço que demonstra o uso de métricas customizadas
/// </summary>
public class CustomMetricsService
{
    private readonly ILogger<CustomMetricsService> _logger;
    private readonly Meter _meter;
    
    // Métricas customizadas
    private readonly Counter<int> _requestCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly ObservableGauge<int> _itemsInProgress;
    private readonly UpDownCounter<int> _activeConnections;
    
    private int _itemsInProgressCount = 0;

    public CustomMetricsService(ILogger<CustomMetricsService> logger)
    {
        _logger = logger;
        // Usar o ObservabilityMetrics para garantir que o Meter seja registrado no OpenTelemetry
        _meter = ObservabilityMetrics.GetOrCreateMeter("CustomMetrics", "1.0.0");
        
        // Criar métricas usando o Meter
        _requestCounter = _meter.CreateCounter<int>("custom_requests_total", "count", "Total de requisições customizadas");
        _requestDuration = _meter.CreateHistogram<double>("custom_request_duration_seconds", "seconds", "Duração das requisições customizadas");
        _itemsInProgress = _meter.CreateObservableGauge<int>("custom_items_in_progress", () => _itemsInProgressCount, "items", "Itens em processamento");
        _activeConnections = _meter.CreateUpDownCounter<int>("custom_active_connections", "connections", "Conexões ativas");
    }

    public async Task ProcessRequestAsync(string requestType)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Processando requisição customizada: {RequestType}", requestType);
            
            // Simular processamento
            await Task.Delay(100);
            
            // Incrementar contador de sucesso
            _requestCounter.Add(1, new KeyValuePair<string, object?>("status", "success"));
            _requestCounter.Add(1, new KeyValuePair<string, object?>("type", requestType));
            
            // Incrementar conexões ativas
            _activeConnections.Add(1);
            
            // Incrementar itens em processamento
            Interlocked.Increment(ref _itemsInProgressCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar requisição customizada: {RequestType}", requestType);
            
            // Incrementar contador de erro
            _requestCounter.Add(1, new KeyValuePair<string, object?>("status", "error"));
            _requestCounter.Add(1, new KeyValuePair<string, object?>("type", requestType));
            
            throw;
        }
        finally
        {
            // Registrar duração
            _requestDuration.Record(stopwatch.Elapsed.TotalSeconds);
            
            // Decrementar conexões ativas
            _activeConnections.Add(-1);
            
            // Decrementar itens em processamento
            Interlocked.Decrement(ref _itemsInProgressCount);
        }
    }
}
