using Microsoft.AspNetCore.Builder;
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
using Serilog.Sinks.File;
using Serilog.Sinks.Seq;
using Package.Observability.Exceptions;
using Package.Observability.HealthChecks;
using Package.Observability.Telemetry;

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
        try
        {
            // Bind configuration
            services.Configure<ObservabilityOptions>(configuration.GetSection(sectionName));
            var options = configuration.GetSection(sectionName).Get<ObservabilityOptions>() ?? new ObservabilityOptions();

            // Validar configuração
            var validationResult = ConfigurationValidator.Validate(options);
            if (!validationResult.IsValid)
            {
                throw new ObservabilityConfigurationException(
                    $"Configuração de observabilidade inválida:\n{validationResult}");
            }

            // Log de avisos se houver
            if (validationResult.Warnings.Count > 0)
            {
                // Em uma implementação real, você usaria um logger apropriado
                System.Diagnostics.Debug.WriteLine($"Avisos na configuração de observabilidade: {string.Join(", ", validationResult.Warnings)}");
            }

            // Registrar ResourceManager
            services.AddSingleton<ResourceManager>();

            // Registrar SerilogService
            services.AddSingleton<ISerilogService, SerilogService>();

               // Configurar ObservabilityMetrics
               System.Diagnostics.Debug.WriteLine($"Setting ObservabilityMetrics.EnableMetrics to: {options.EnableMetrics}");
               ObservabilityMetrics.SetMetricsEnabled(options.EnableMetrics);
               System.Diagnostics.Debug.WriteLine($"ObservabilityMetrics.IsMetricsEnabled is now: {ObservabilityMetrics.IsMetricsEnabled}");
               
               // Debug adicional para verificar se a configuração está sendo aplicada
               System.Diagnostics.Debug.WriteLine($"Configuration values - EnableMetrics: {options.EnableMetrics}, EnableTracing: {options.EnableTracing}, EnableLogging: {options.EnableLogging}");

            // Configure OpenTelemetry
            var openTelemetryBuilder = services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName: options.ServiceName)
                    .AddAttributes(options.AdditionalLabels.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value))));

            // Only configure metrics if enabled
            if (options.EnableMetrics)
            {
                openTelemetryBuilder.WithMetrics(metrics => ConfigureMetrics(metrics, options));
            }

            // Only configure tracing if enabled
            if (options.EnableTracing)
            {
                openTelemetryBuilder.WithTracing(tracing => ConfigureTracing(tracing, options));
            }

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

            // Configure SerilogService
            if (options.EnableLogging)
            {
                // Configure SerilogService after DI container is built
                services.AddHostedService<SerilogServiceInitializer>();
            }

            // Adicionar Health Checks
            AddHealthChecks(services, options);

            return services;
        }
        catch (Exception ex) when (!(ex is ObservabilityConfigurationException))
        {
            throw new ObservabilityConfigurationException(
                "Erro ao configurar observabilidade", ex);
        }
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
            opt.TempoEndpoint = options.TempoEndpoint;
            opt.CollectorEndpoint = options.CollectorEndpoint;
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
                .AddAttributes(options.AdditionalLabels.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value))))
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

        metrics.AddMeter(options.ServiceName);

        if (options.EnableRuntimeInstrumentation)
            metrics.AddRuntimeInstrumentation();

        if (options.EnableAspNetCoreInstrumentation)
            metrics.AddAspNetCoreInstrumentation();

        if (options.EnableHttpClientInstrumentation)
            metrics.AddHttpClientInstrumentation();

        metrics.AddPrometheusExporter();
    }

    private static void ConfigureTracing(TracerProviderBuilder tracing, ObservabilityOptions options)
    {
        if (!options.EnableTracing) return;

        tracing.AddSource(options.ServiceName);

        if (options.EnableAspNetCoreInstrumentation)
            tracing.AddAspNetCoreInstrumentation();

        if (options.EnableHttpClientInstrumentation)
            tracing.AddHttpClientInstrumentation();

        var endpoint = !string.IsNullOrEmpty(options.CollectorEndpoint) 
            ? options.CollectorEndpoint 
            : options.OtlpEndpoint;
            
        if (!string.IsNullOrEmpty(endpoint))
        {
            tracing.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(endpoint);
            });
        }
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
            var consoleTemplate = options.ConsoleOutputTemplate ?? 
                "[{Timestamp:HH:mm:ss} {Level:u3}] {ServiceName} {Message:lj}{NewLine}{Exception}";
            loggerConfiguration.WriteTo.Console(outputTemplate: consoleTemplate);
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

        // File sink
        if (options.EnableFileLogging)
        {
            var filePath = options.FileLoggingPath ?? $"Logs/{options.ServiceName}-.log";
            var fileTemplate = options.FileOutputTemplate ?? 
                "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {ServiceName} {Message:lj}{NewLine}{Exception}";
            
            loggerConfiguration.WriteTo.File(
                filePath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: fileTemplate);
        }

        // Add custom properties
        foreach (var property in options.CustomProperties)
        {
            loggerConfiguration.Enrich.WithProperty(property.Key, property.Value);
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }

    /// <summary>
    /// Adiciona Health Checks para observabilidade
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="options">Opções de observabilidade</param>
    private static void AddHealthChecks(IServiceCollection services, ObservabilityOptions options)
    {
        services.AddHealthChecks()
            .AddCheck<ObservabilityHealthCheck>("observability", tags: new[] { "observability", "general" });

        if (options.EnableMetrics)
        {
            services.AddHealthChecks()
                .AddCheck<MetricsHealthCheck>("metrics", tags: new[] { "observability", "metrics" });
        }

        if (options.EnableTracing)
        {
            services.AddHealthChecks()
                .AddCheck<TracingHealthCheck>("tracing", tags: new[] { "observability", "tracing" });
        }

        if (options.EnableLogging)
        {
            services.AddHealthChecks()
                .AddCheck<LoggingHealthCheck>("logging", tags: new[] { "observability", "logging" })
                .AddCheck<SerilogHealthCheck>("serilog", tags: new[] { "observability", "logging", "serilog" });
        }

        // Register telemetry services
        // services.AddSingleton<ITelemetryService, TelemetryService>();
        // services.AddSingleton<TelemetryInterceptor>();
        
        // Register auto-instrumentation services (zero configuration)
        // services.AddSingleton<AutoMethodInterceptor>();
        // services.AddHostedService<AutoDiscovery>();
    }

    /// <summary>
    /// Adiciona o middleware de telemetria automática
    /// Similar ao Application Insights, mas usando OpenTelemetry
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseObservabilityTelemetry(this IApplicationBuilder app)
    {
        // return app.UseMiddleware<TelemetryMiddleware>();
        return app;
    }

    /// <summary>
    /// Adiciona o middleware de telemetria automática (zero configuração)
    /// Similar ao Tempo, Loki e Prometheus - funciona automaticamente
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseAutoObservabilityTelemetry(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ZeroConfigTelemetryMiddleware>();
    }
}