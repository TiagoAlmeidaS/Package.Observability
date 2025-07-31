using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

namespace Package.Observability;

/// <summary>
/// Extension methods for configuring observability services
/// </summary>
public static class ObservabilityStartupExtensions
{
    /// <summary>
    /// Adds comprehensive observability services (metrics, logging, tracing) to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="sectionName">Configuration section name (default: "Observability")</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddObservability(
        this IServiceCollection services, 
        IConfiguration configuration, 
        string sectionName = "Observability")
    {
        // Bind configuration
        services.Configure<ObservabilityOptions>(configuration.GetSection(sectionName));
        var options = configuration.GetSection(sectionName).Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        // Configure OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: options.ServiceName)
                .AddAttributes(options.AdditionalLabels))
            .WithMetrics(metrics => ConfigureMetrics(metrics, options))
            .WithTracing(tracing => ConfigureTracing(tracing, options));

        // Configure Serilog
        if (options.EnableLogging)
        {
            ConfigureSerilog(options);
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });
        }

        return services;
    }

    /// <summary>
    /// Adds observability services with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure observability options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        Action<ObservabilityOptions> configureOptions)
    {
        var options = new ObservabilityOptions();
        configureOptions(options);

        services.Configure<ObservabilityOptions>(opt =>
        {
            opt.ServiceName = options.ServiceName;
            opt.PrometheusPort = options.PrometheusPort;
            opt.EnableMetrics = options.EnableMetrics;
            opt.EnableTracing = options.EnableTracing;
            opt.EnableLogging = options.EnableLogging;
            opt.LokiUrl = options.LokiUrl;
            opt.OtlpEndpoint = options.OtlpEndpoint;
            opt.EnableConsoleLogging = options.EnableConsoleLogging;
            opt.MinimumLogLevel = options.MinimumLogLevel;
            opt.AdditionalLabels = options.AdditionalLabels;
            opt.LokiLabels = options.LokiLabels;
            opt.EnableCorrelationId = options.EnableCorrelationId;
            opt.EnableRuntimeInstrumentation = options.EnableRuntimeInstrumentation;
            opt.EnableHttpClientInstrumentation = options.EnableHttpClientInstrumentation;
            opt.EnableAspNetCoreInstrumentation = options.EnableAspNetCoreInstrumentation;
        });

        // Configure OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: options.ServiceName)
                .AddAttributes(options.AdditionalLabels))
            .WithMetrics(metrics => ConfigureMetrics(metrics, options))
            .WithTracing(tracing => ConfigureTracing(tracing, options));

        // Configure Serilog
        if (options.EnableLogging)
        {
            ConfigureSerilog(options);
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });
        }

        return services;
    }

    private static void ConfigureMetrics(MeterProviderBuilder metrics, ObservabilityOptions options)
    {
        if (!options.EnableMetrics) return;

        if (options.EnableRuntimeInstrumentation)
            metrics.AddRuntimeInstrumentation();

        if (options.EnableAspNetCoreInstrumentation)
            metrics.AddAspNetCoreInstrumentation();

        if (options.EnableHttpClientInstrumentation)
            metrics.AddHttpClientInstrumentation();

        // Add Prometheus exporter
        metrics.AddPrometheusExporter(prometheusOptions =>
        {
            prometheusOptions.StartHttpListener = true;
            prometheusOptions.HttpListenerPrefixes = new[] { $"http://*:{options.PrometheusPort}/" };
        });
    }

    private static void ConfigureTracing(TracerProviderBuilder tracing, ObservabilityOptions options)
    {
        if (!options.EnableTracing) return;

        if (options.EnableAspNetCoreInstrumentation)
            tracing.AddAspNetCoreInstrumentation();

        if (options.EnableHttpClientInstrumentation)
            tracing.AddHttpClientInstrumentation();

        // Add OTLP exporter
        tracing.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
        });
    }

    private static void ConfigureSerilog(ObservabilityOptions options)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", options.ServiceName);

        // Set minimum log level
        if (Enum.TryParse<LogEventLevel>(options.MinimumLogLevel, true, out var logLevel))
        {
            loggerConfiguration.MinimumLevel.Is(logLevel);
        }

        // Add enrichers
        loggerConfiguration
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithThreadId();

        // Add correlation ID if enabled
        if (options.EnableCorrelationId)
        {
            loggerConfiguration.Enrich.WithProperty("CorrelationId", Guid.NewGuid().ToString());
        }

        // Console sink
        if (options.EnableConsoleLogging)
        {
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {ServiceName} {Message:lj}{NewLine}{Exception}");
        }

        // Loki sink
        if (!string.IsNullOrWhiteSpace(options.LokiUrl))
        {
            var lokiLabels = new List<LokiLabel>
            {
                new() { Key = "service", Value = options.ServiceName },
                new() { Key = "level", Value = "{Level}" }
            };

            // Add custom Loki labels
            foreach (var label in options.LokiLabels)
            {
                lokiLabels.Add(new LokiLabel { Key = label.Key, Value = label.Value });
            }

            loggerConfiguration.WriteTo.GrafanaLoki(
                uri: options.LokiUrl,
                labels: lokiLabels);
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }
}