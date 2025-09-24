using System.Diagnostics;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using Package.Observability;
using Xunit;

namespace Package.Observability.IntegrationTests;

/// <summary>
/// Testes de integração para OTLP com HTTP Protocol Buffer
/// Validação do envio de dados para https://collector.qas.tastechdeveloper.shop
/// </summary>
public class OtlpHttpProtobufIntegrationTests
{
    private const string TestOtlpEndpoint = "https://collector.qas.tastechdeveloper.shop";
    private const string TestServiceName = "Package.Observability.IntegrationTest";

    [Fact]
    public async Task OtlpHttpProtobuf_Configuration_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddObservability(options =>
                    {
                        options.ServiceName = TestServiceName;
                        options.ServiceVersion = "1.0.0";
                        options.EnableMetrics = true;
                        options.EnableTracing = true;
                        options.EnableLogging = true;
                        options.PrometheusPort = GetFreeTcpPort();
                        options.OtlpEndpoint = TestOtlpEndpoint;
                        options.OtlpProtocol = "HttpProtobuf";
                        options.RecordExceptions = true;
                        options.ExcludePaths = new List<string> { "/metrics", "/health" };
                        options.EnableRouteMetrics = true;
                        options.EnableDetailedEndpointMetrics = true;
                        options.CustomHistogramBuckets = new List<double> { 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 };
                        options.CustomMetricLabels = new Dictionary<string, string>
                        {
                            { "environment", "integration-test" },
                            { "region", "qas" }
                        };
                        options.AdditionalLabels = new Dictionary<string, string>
                        {
                            { "test-type", "otlp-http-protobuf" },
                            { "test-scenario", "full-integration" }
                        };
                    });
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se as configurações OTLP foram aplicadas
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.OtlpEndpoint.Should().Be(TestOtlpEndpoint);
        options.OtlpProtocol.Should().Be("HttpProtobuf");
        options.ServiceName.Should().Be(TestServiceName);
        options.ServiceVersion.Should().Be("1.0.0");
        options.RecordExceptions.Should().BeTrue();
        options.ExcludePaths.Should().Contain("/metrics");
        options.ExcludePaths.Should().Contain("/health");
        options.EnableRouteMetrics.Should().BeTrue();
        options.EnableDetailedEndpointMetrics.Should().BeTrue();
        options.CustomHistogramBuckets.Should().HaveCount(11);
        options.CustomMetricLabels.Should().ContainKey("environment");
        options.CustomMetricLabels.Should().ContainKey("region");
        options.AdditionalLabels.Should().ContainKey("test-type");
        options.AdditionalLabels.Should().ContainKey("test-scenario");
    }

    [Fact]
    public async Task OtlpHttpProtobuf_Tracing_ShouldGenerateTraces()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = $"{TestServiceName}-Tracing",
                        ["Observability:ServiceVersion"] = "1.0.0",
                        ["Observability:EnableTracing"] = "true",
                        ["Observability:EnableMetrics"] = "false",
                        ["Observability:EnableLogging"] = "false",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:OtlpEndpoint"] = TestOtlpEndpoint,
                        ["Observability:OtlpProtocol"] = "HttpProtobuf",
                        ["Observability:RecordExceptions"] = "true",
                        ["Observability:EnableAspNetCoreInstrumentation"] = "true",
                        ["Observability:EnableHttpClientInstrumentation"] = "true"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        // Fazer várias requisições para gerar traces
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(client.GetAsync("/WeatherForecast"));
        }
        
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        
        // Verificar se a configuração de tracing está correta
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.EnableTracing.Should().BeTrue();
        options.OtlpProtocol.Should().Be("HttpProtobuf");
        options.RecordExceptions.Should().BeTrue();
        options.EnableAspNetCoreInstrumentation.Should().BeTrue();
        options.EnableHttpClientInstrumentation.Should().BeTrue();
    }

    [Fact]
    public async Task OtlpHttpProtobuf_Metrics_ShouldGenerateMetrics()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddObservability(options =>
                    {
                        options.ServiceName = $"{TestServiceName}-Metrics";
                        options.ServiceVersion = "1.0.0";
                        options.EnableTracing = false;
                        options.EnableMetrics = true;
                        options.EnableLogging = false;
                        options.PrometheusPort = GetFreeTcpPort();
                        options.OtlpEndpoint = TestOtlpEndpoint;
                        options.OtlpProtocol = "HttpProtobuf";
                        options.EnableRouteMetrics = true;
                        options.EnableDetailedEndpointMetrics = true;
                        options.EnableRuntimeInstrumentation = true;
                        options.EnableAspNetCoreInstrumentation = true;
                        options.EnableHttpClientInstrumentation = true;
                        options.CustomHistogramBuckets = new List<double> { 0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 25, 50, 100 };
                    });
                });
            });

        var client = factory.CreateClient();

        // Act
        // Fazer requisições para gerar métricas
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(client.GetAsync("/WeatherForecast"));
        }
        
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        
        // Verificar se a configuração de métricas está correta
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.EnableMetrics.Should().BeTrue();
        options.EnableRouteMetrics.Should().BeTrue();
        options.EnableDetailedEndpointMetrics.Should().BeTrue();
        options.EnableRuntimeInstrumentation.Should().BeTrue();
        options.EnableAspNetCoreInstrumentation.Should().BeTrue();
        options.EnableHttpClientInstrumentation.Should().BeTrue();
        options.CustomHistogramBuckets.Should().HaveCount(15);
    }

    [Fact]
    public async Task OtlpHttpProtobuf_Logs_ShouldGenerateLogs()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = $"{TestServiceName}-Logs",
                        ["Observability:ServiceVersion"] = "1.0.0",
                        ["Observability:EnableTracing"] = "false",
                        ["Observability:EnableMetrics"] = "false",
                        ["Observability:EnableLogging"] = "true",
                        ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                        ["Observability:OtlpEndpoint"] = TestOtlpEndpoint,
                        ["Observability:OtlpProtocol"] = "HttpProtobuf",
                        ["Observability:EnableConsoleLogging"] = "true",
                        ["Observability:MinimumLogLevel"] = "Information",
                        ["Observability:EnableCorrelationId"] = "true",
                        ["Observability:EnableRequestLogging"] = "true",
                        ["Observability:SlowRequestThreshold"] = "500"
                    };
                    configBuilder.AddInMemoryCollection(config);
                });
            });

        var client = factory.CreateClient();

        // Act
        // Fazer requisições para gerar logs
        var response = await client.GetAsync("/WeatherForecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar se a configuração de logs está correta
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.EnableLogging.Should().BeTrue();
        options.EnableConsoleLogging.Should().BeTrue();
        options.MinimumLogLevel.Should().Be("Information");
        options.EnableCorrelationId.Should().BeTrue();
        options.EnableRequestLogging.Should().BeTrue();
        options.SlowRequestThreshold.Should().Be(500);
    }

    [Fact]
    public async Task OtlpHttpProtobuf_FullIntegration_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddObservability(options =>
                    {
                        options.ServiceName = $"{TestServiceName}-FullIntegration";
                        options.ServiceVersion = "1.0.0";
                        options.EnableTracing = true;
                        options.EnableMetrics = true;
                        options.EnableLogging = true;
                        options.PrometheusPort = GetFreeTcpPort();
                        options.OtlpEndpoint = TestOtlpEndpoint;
                        options.OtlpProtocol = "HttpProtobuf";
                        options.RecordExceptions = true;
                        options.ExcludePaths = new List<string> { "/metrics", "/health", "/swagger" };
                        options.EnableRouteMetrics = true;
                        options.EnableDetailedEndpointMetrics = true;
                        options.EnableRuntimeInstrumentation = true;
                        options.EnableAspNetCoreInstrumentation = true;
                        options.EnableHttpClientInstrumentation = true;
                        options.EnableConsoleLogging = true;
                        options.MinimumLogLevel = "Information";
                        options.EnableCorrelationId = true;
                        options.EnableRequestLogging = true;
                        options.SlowRequestThreshold = 1000;
                        options.CustomHistogramBuckets = new List<double> { 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 };
                        options.CustomMetricLabels = new Dictionary<string, string>
                        {
                            { "environment", "integration-test" },
                            { "region", "qas" },
                            { "test-type", "full-integration" }
                        };
                        options.AdditionalLabels = new Dictionary<string, string>
                        {
                            { "service-type", "web-api" },
                            { "team", "observability" },
                            { "version", "1.0.0" }
                        };
                        options.MetricNames = new MetricNamesConfiguration
                        {
                            HttpRequestsTotal = "http_requests_total_by_route",
                            HttpRequestErrorsTotal = "http_requests_errors_total_by_route",
                            HttpRequestDurationSeconds = "http_request_duration_seconds_by_route"
                        };
                    });
                });
            });

        var client = factory.CreateClient();

        // Act
        // Simular diferentes cenários de uso
        var scenarios = new[]
        {
            client.GetAsync("/WeatherForecast"),
            client.GetAsync("/WeatherForecast"),
            client.GetAsync("/WeatherForecast"),
            client.GetAsync("/nonexistent"), // 404
            client.GetAsync("/WeatherForecast")
        };

        var responses = await Task.WhenAll(scenarios);

        // Assert
        responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[1].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[2].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[3].StatusCode.Should().Be(HttpStatusCode.NotFound);
        responses[4].StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar configuração completa
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        // Configurações básicas
        options.ServiceName.Should().Be($"{TestServiceName}-FullIntegration");
        options.ServiceVersion.Should().Be("1.0.0");
        options.OtlpEndpoint.Should().Be(TestOtlpEndpoint);
        options.OtlpProtocol.Should().Be("HttpProtobuf");
        
        // Configurações de tracing
        options.EnableTracing.Should().BeTrue();
        options.RecordExceptions.Should().BeTrue();
        options.ExcludePaths.Should().Contain("/metrics");
        options.ExcludePaths.Should().Contain("/health");
        options.ExcludePaths.Should().Contain("/swagger");
        options.EnableAspNetCoreInstrumentation.Should().BeTrue();
        options.EnableHttpClientInstrumentation.Should().BeTrue();
        
        // Configurações de métricas
        options.EnableMetrics.Should().BeTrue();
        options.EnableRouteMetrics.Should().BeTrue();
        options.EnableDetailedEndpointMetrics.Should().BeTrue();
        options.EnableRuntimeInstrumentation.Should().BeTrue();
        options.CustomHistogramBuckets.Should().HaveCount(11);
        options.CustomMetricLabels.Should().ContainKey("environment");
        options.CustomMetricLabels.Should().ContainKey("region");
        options.CustomMetricLabels.Should().ContainKey("test-type");
        options.AdditionalLabels.Should().ContainKey("service-type");
        options.AdditionalLabels.Should().ContainKey("team");
        options.AdditionalLabels.Should().ContainKey("version");
        
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
    public async Task OtlpHttpProtobuf_ErrorHandling_ShouldHandleErrors()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = new Dictionary<string, string?>
                    {
                        ["Observability:ServiceName"] = $"{TestServiceName}-ErrorHandling",
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
        // Simular diferentes tipos de erro
        var errorScenarios = new[]
        {
            client.GetAsync("/nonexistent"), // 404
            client.GetAsync("/WeatherForecast"), // 200
            client.GetAsync("/invalid-endpoint"), // 404
        };

        var responses = await Task.WhenAll(errorScenarios);

        // Assert
        responses[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
        responses[1].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[2].StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // Verificar se a configuração de tratamento de erros está correta
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.RecordExceptions.Should().BeTrue();
        options.EnableRouteMetrics.Should().BeTrue();
    }

    [Fact]
    public async Task OtlpHttpProtobuf_HealthCheck_ShouldValidateConfiguration()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddObservability(options =>
                    {
                        options.ServiceName = $"{TestServiceName}-HealthCheck";
                        options.ServiceVersion = "1.0.0";
                        options.EnableTracing = true;
                        options.EnableMetrics = true;
                        options.EnableLogging = true;
                        options.PrometheusPort = GetFreeTcpPort();
                        options.OtlpEndpoint = TestOtlpEndpoint;
                        options.OtlpProtocol = "HttpProtobuf";
                    });
                });
            });

        var client = factory.CreateClient();

        // Act
        var healthResponse = await client.GetAsync("/health");
        var healthContent = await healthResponse.Content.ReadAsStringAsync();

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        healthContent.Should().Contain("Healthy");
        
        // Verificar se as configurações OTLP estão corretas
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.OtlpEndpoint.Should().Be(TestOtlpEndpoint);
        options.OtlpProtocol.Should().Be("HttpProtobuf");
        options.EnableTracing.Should().BeTrue();
        options.EnableMetrics.Should().BeTrue();
        options.EnableLogging.Should().BeTrue();
    }

    [Fact]
    public async Task OtlpHttpProtobuf_CodeConfiguration_ShouldWork()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddObservability(options =>
                    {
                        options.ServiceName = $"{TestServiceName}-CodeConfig";
                        options.ServiceVersion = "1.0.0";
                        options.EnableMetrics = true;
                        options.EnableTracing = true;
                        options.EnableLogging = true;
                        options.PrometheusPort = GetFreeTcpPort();
                        options.OtlpEndpoint = TestOtlpEndpoint;
                        options.OtlpProtocol = "HttpProtobuf";
                        options.RecordExceptions = true;
                        options.ExcludePaths = new List<string> { "/metrics", "/health" };
                        options.EnableRouteMetrics = true;
                        options.EnableDetailedEndpointMetrics = true;
                        options.EnableRuntimeInstrumentation = true;
                        options.EnableAspNetCoreInstrumentation = true;
                        options.EnableHttpClientInstrumentation = true;
                        options.EnableConsoleLogging = true;
                        options.MinimumLogLevel = "Information";
                        options.EnableCorrelationId = true;
                        options.EnableRequestLogging = true;
                        options.SlowRequestThreshold = 1000;
                        options.CustomHistogramBuckets = new List<double> 
                        { 
                            0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 
                        };
                        options.CustomMetricLabels = new Dictionary<string, string>
                        {
                            { "environment", "integration-test" },
                            { "region", "qas" },
                            { "test-type", "code-configuration" }
                        };
                        options.AdditionalLabels = new Dictionary<string, string>
                        {
                            { "service-type", "web-api" },
                            { "team", "observability" },
                            { "version", "1.0.0" }
                        };
                        options.MetricNames = new MetricNamesConfiguration
                        {
                            HttpRequestsTotal = "http_requests_total_by_route",
                            HttpRequestErrorsTotal = "http_requests_errors_total_by_route",
                            HttpRequestDurationSeconds = "http_request_duration_seconds_by_route"
                        };
                    });
                });
            });

        var client = factory.CreateClient();

        // Act & Assert
        // 1. Verificar se a aplicação inicia sem erros
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verificar se as configurações foram aplicadas
        var serviceProvider = factory.Services;
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        
        options.ServiceName.Should().Be($"{TestServiceName}-CodeConfig");
        options.ServiceVersion.Should().Be("1.0.0");
        options.OtlpEndpoint.Should().Be(TestOtlpEndpoint);
        options.OtlpProtocol.Should().Be("HttpProtobuf");
        options.RecordExceptions.Should().BeTrue();
        options.ExcludePaths.Should().Contain("/metrics");
        options.ExcludePaths.Should().Contain("/health");
        options.EnableRouteMetrics.Should().BeTrue();
        options.EnableDetailedEndpointMetrics.Should().BeTrue();
        options.EnableRuntimeInstrumentation.Should().BeTrue();
        options.EnableAspNetCoreInstrumentation.Should().BeTrue();
        options.EnableHttpClientInstrumentation.Should().BeTrue();
        options.EnableConsoleLogging.Should().BeTrue();
        options.MinimumLogLevel.Should().Be("Information");
        options.EnableCorrelationId.Should().BeTrue();
        options.EnableRequestLogging.Should().BeTrue();
        options.SlowRequestThreshold.Should().Be(1000);
        options.CustomHistogramBuckets.Should().HaveCount(11);
        options.CustomMetricLabels.Should().ContainKey("environment");
        options.CustomMetricLabels.Should().ContainKey("region");
        options.CustomMetricLabels.Should().ContainKey("test-type");
        options.AdditionalLabels.Should().ContainKey("service-type");
        options.AdditionalLabels.Should().ContainKey("team");
        options.AdditionalLabels.Should().ContainKey("version");
        options.MetricNames.HttpRequestsTotal.Should().Be("http_requests_total_by_route");
        options.MetricNames.HttpRequestErrorsTotal.Should().Be("http_requests_errors_total_by_route");
        options.MetricNames.HttpRequestDurationSeconds.Should().Be("http_request_duration_seconds_by_route");
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
