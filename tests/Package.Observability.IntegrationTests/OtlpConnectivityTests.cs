using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Package.Observability;
using Xunit;

namespace Package.Observability.IntegrationTests;

/// <summary>
/// Testes de conectividade para OTLP endpoint real
/// Validação da conectividade com https://collector.qas.tastechdeveloper.shop
/// </summary>
public class OtlpConnectivityTests
{
    private const string TestOtlpEndpoint = "https://collector.qas.tastechdeveloper.shop";
    private const string TestServiceName = "Package.Observability.ConnectivityTest";

    [Fact]
    public async Task OtlpEndpoint_Connectivity_ShouldBeReachable()
    {
        // Arrange
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Act
        try
        {
            // Tentar fazer uma requisição HEAD para verificar conectividade
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, TestOtlpEndpoint));
            
            // Assert
            // O endpoint deve estar acessível (pode retornar 404, 405, etc., mas não deve dar timeout)
            response.Should().NotBeNull();
            response.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
            
            // Log do status para debug
            Console.WriteLine($"OTLP Endpoint Status: {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            // Se der erro de conectividade, falhar o teste
            Assert.True(false, $"OTLP Endpoint não está acessível: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            // Se der timeout, falhar o teste
            Assert.True(false, $"OTLP Endpoint timeout: {ex.Message}");
        }
    }

    [Fact]
    public async Task OtlpHttpProtobuf_Configuration_ShouldNotThrowExceptions()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = TestServiceName,
                        ["Observability:ServiceVersion"] = "1.0.0",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:OtlpEndpoint"] = TestOtlpEndpoint,
                        ["Observability:OtlpProtocol"] = "HttpProtobuf",
                        ["Observability:RecordExceptions"] = "true",
                        ["Observability:EnableRouteMetrics"] = "true"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        // Act & Assert
        // A aplicação deve iniciar sem exceções, mesmo que o endpoint não esteja acessível
        var client = factory.CreateClient();
        var response = await client.GetAsync("/WeatherForecast");
        
        // A resposta deve ser OK (a aplicação funciona independente da conectividade OTLP)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar se as configurações foram aplicadas corretamente
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.OtlpEndpoint.Should().Be(TestOtlpEndpoint);
        options.OtlpProtocol.Should().Be("HttpProtobuf");
        options.ServiceName.Should().Be(TestServiceName);
    }

    [Fact]
    public async Task OtlpHttpProtobuf_WithCustomLabels_ShouldConfigureCorrectly()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = TestServiceName,
                        ["Observability:ServiceVersion"] = "1.0.0",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:OtlpEndpoint"] = TestOtlpEndpoint,
                        ["Observability:OtlpProtocol"] = "HttpProtobuf",
                        ["Observability:AdditionalLabels:environment"] = "qas",
                        ["Observability:AdditionalLabels:region"] = "us-east-1",
                        ["Observability:AdditionalLabels:service-type"] = "web-api",
                        ["Observability:AdditionalLabels:team"] = "observability",
                        ["Observability:AdditionalLabels:version"] = "1.0.0",
                        ["Observability:CustomMetricLabels:test-scenario"] = "connectivity",
                        ["Observability:CustomMetricLabels:test-type"] = "integration"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/WeatherForecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar se todos os labels foram configurados
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.AdditionalLabels.Should().ContainKey("environment");
        options.AdditionalLabels.Should().ContainKey("region");
        options.AdditionalLabels.Should().ContainKey("service-type");
        options.AdditionalLabels.Should().ContainKey("team");
        options.AdditionalLabels.Should().ContainKey("version");
        options.AdditionalLabels["environment"].Should().Be("qas");
        options.AdditionalLabels["region"].Should().Be("us-east-1");
        options.AdditionalLabels["service-type"].Should().Be("web-api");
        options.AdditionalLabels["team"].Should().Be("observability");
        options.AdditionalLabels["version"].Should().Be("1.0.0");
        
        options.CustomMetricLabels.Should().ContainKey("test-scenario");
        options.CustomMetricLabels.Should().ContainKey("test-type");
        options.CustomMetricLabels["test-scenario"].Should().Be("connectivity");
        options.CustomMetricLabels["test-type"].Should().Be("integration");
    }

    [Fact]
    public async Task OtlpHttpProtobuf_WithAdvancedConfiguration_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = TestServiceName,
                        ["Observability:ServiceVersion"] = "1.0.0",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:OtlpEndpoint"] = TestOtlpEndpoint,
                        ["Observability:OtlpProtocol"] = "HttpProtobuf",
                        ["Observability:RecordExceptions"] = "true",
                        ["Observability:ExcludePaths:0"] = "/metrics",
                        ["Observability:ExcludePaths:1"] = "/health",
                        ["Observability:ExcludePaths:2"] = "/swagger",
                        ["Observability:ExcludePaths:3"] = "/favicon.ico",
                        ["Observability:EnableRouteMetrics"] = "true",
                        ["Observability:EnableDetailedEndpointMetrics"] = "true",
                        ["Observability:EnableRuntimeInstrumentation"] = "true",
                        ["Observability:EnableAspNetCoreInstrumentation"] = "true",
                        ["Observability:EnableHttpClientInstrumentation"] = "true",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:MinimumLogLevel"] = "Information",
                        ["Observability:EnableCorrelationId"] = "true",
                        ["Observability:EnableRequestLogging"] = "true",
                        ["Observability:SlowRequestThreshold"] = "1000",
                        ["Observability:CustomHistogramBuckets:0"] = "0.001",
                        ["Observability:CustomHistogramBuckets:1"] = "0.005",
                        ["Observability:CustomHistogramBuckets:2"] = "0.01",
                        ["Observability:CustomHistogramBuckets:3"] = "0.025",
                        ["Observability:CustomHistogramBuckets:4"] = "0.05",
                        ["Observability:CustomHistogramBuckets:5"] = "0.1",
                        ["Observability:CustomHistogramBuckets:6"] = "0.25",
                        ["Observability:CustomHistogramBuckets:7"] = "0.5",
                        ["Observability:CustomHistogramBuckets:8"] = "1",
                        ["Observability:CustomHistogramBuckets:9"] = "2.5",
                        ["Observability:CustomHistogramBuckets:10"] = "5",
                        ["Observability:CustomHistogramBuckets:11"] = "10",
                        ["Observability:CustomHistogramBuckets:12"] = "25",
                        ["Observability:CustomHistogramBuckets:13"] = "50",
                        ["Observability:CustomHistogramBuckets:14"] = "100",
                        ["Observability:CustomHistogramBuckets:15"] = "250",
                        ["Observability:CustomHistogramBuckets:16"] = "500",
                        ["Observability:CustomHistogramBuckets:17"] = "1000",
                        ["Observability:MetricNames:HttpRequestsTotal"] = "http_requests_total_by_route",
                        ["Observability:MetricNames:HttpRequestErrorsTotal"] = "http_requests_errors_total_by_route",
                        ["Observability:MetricNames:HttpRequestDurationSeconds"] = "http_request_duration_seconds_by_route"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        // Simular diferentes tipos de requisições
        var tasks = new[]
        {
            client.GetAsync("/WeatherForecast"),
            client.GetAsync("/WeatherForecast"),
            client.GetAsync("/nonexistent"), // 404
            client.GetAsync("/WeatherForecast")
        };

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[1].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[2].StatusCode.Should().Be(HttpStatusCode.NotFound);
        responses[3].StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar configuração avançada
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        // Configurações básicas
        options.ServiceName.Should().Be(TestServiceName);
        options.ServiceVersion.Should().Be("1.0.0");
        options.OtlpEndpoint.Should().Be(TestOtlpEndpoint);
        options.OtlpProtocol.Should().Be("HttpProtobuf");
        
        // Configurações de tracing
        options.EnableTracing.Should().BeTrue();
        options.RecordExceptions.Should().BeTrue();
        options.ExcludePaths.Should().Contain("/metrics");
        options.ExcludePaths.Should().Contain("/health");
        options.ExcludePaths.Should().Contain("/swagger");
        options.ExcludePaths.Should().Contain("/favicon.ico");
        options.EnableAspNetCoreInstrumentation.Should().BeTrue();
        options.EnableHttpClientInstrumentation.Should().BeTrue();
        
        // Configurações de métricas
        options.EnableMetrics.Should().BeTrue();
        options.EnableRouteMetrics.Should().BeTrue();
        options.EnableDetailedEndpointMetrics.Should().BeTrue();
        options.EnableRuntimeInstrumentation.Should().BeTrue();
        options.CustomHistogramBuckets.Should().HaveCount(18);
        
        // Configurações de logs
        options.EnableLogging.Should().BeTrue();
        options.EnableConsoleLogging.Should().BeTrue();
        options.MinimumLogLevel.Should().Be("Information");
        options.EnableCorrelationId.Should().BeTrue();
        options.EnableRequestLogging.Should().BeTrue();
        options.SlowRequestThreshold.Should().Be(1000);
        
        // Configurações de nomes de métricas
        options.MetricNames.HttpRequestsTotal.Should().Be("http_requests_total_by_route");
        options.MetricNames.HttpRequestErrorsTotal.Should().Be("http_requests_errors_total_by_route");
        options.MetricNames.HttpRequestDurationSeconds.Should().Be("http_request_duration_seconds_by_route");
    }

    [Fact]
    public async Task OtlpHttpProtobuf_HealthCheck_ShouldShowCorrectConfiguration()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = TestServiceName,
                        ["Observability:ServiceVersion"] = "1.0.0",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:OtlpEndpoint"] = TestOtlpEndpoint,
                        ["Observability:OtlpProtocol"] = "HttpProtobuf"
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
        // O health check deve retornar "Healthy" quando tudo está configurado corretamente
        healthContent.Should().Contain("Healthy");
        
        // Verificar se a aplicação funciona normalmente
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OtlpHttpProtobuf_ErrorScenarios_ShouldHandleGracefully()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = TestServiceName,
                        ["Observability:ServiceVersion"] = "1.0.0",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableMetrics"] = "true",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:OtlpEndpoint"] = TestOtlpEndpoint,
                        ["Observability:OtlpProtocol"] = "HttpProtobuf",
                        ["Observability:RecordExceptions"] = "true",
                        ["Observability:EnableRouteMetrics"] = "true"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        // Simular diferentes cenários de erro
        var errorScenarios = new[]
        {
            client.GetAsync("/nonexistent"), // 404
            client.GetAsync("/invalid-endpoint"), // 404
            client.GetAsync("/WeatherForecast"), // 200
            client.GetAsync("/another-nonexistent"), // 404
        };

        var responses = await Task.WhenAll(errorScenarios);

        // Assert
        responses[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
        responses[1].StatusCode.Should().Be(HttpStatusCode.NotFound);
        responses[2].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[3].StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // A aplicação deve continuar funcionando mesmo com erros
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.RecordExceptions.Should().BeTrue();
        options.EnableRouteMetrics.Should().BeTrue();
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
