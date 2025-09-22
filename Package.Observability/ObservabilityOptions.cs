namespace Package.Observability;

/// <summary>
/// Configuration options for observability features
/// </summary>
public class ObservabilityOptions
{
    /// <summary>
    /// Name of the service for identification in metrics, logs, and traces
    /// </summary>
    public string ServiceName { get; set; } = "DefaultService";

    /// <summary>
    /// Port for Prometheus metrics endpoint
    /// </summary>
    public int PrometheusPort { get; set; } = 9090;

    /// <summary>
    /// Enable metrics collection and export
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Enable distributed tracing
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Enable structured logging
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Grafana Loki endpoint URL for log aggregation
    /// </summary>
    public string LokiUrl { get; set; } = "http://localhost:3100";

    /// <summary>
    /// OpenTelemetry Protocol (OTLP) endpoint for traces
    /// </summary>
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// Tempo endpoint URL for trace storage and querying
    /// </summary>
    public string TempoEndpoint { get; set; } = "http://localhost:3200";

    /// <summary>
    /// OpenTelemetry Collector endpoint for traces, metrics, and logs
    /// </summary>
    public string CollectorEndpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// Enable console logging output
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// Minimum log level for Serilog
    /// </summary>
    public string MinimumLogLevel { get; set; } = "Information";

    /// <summary>
    /// Additional labels to be added to all metrics
    /// </summary>
    public Dictionary<string, string> AdditionalLabels { get; set; } = new();

    /// <summary>
    /// Custom Loki labels for log categorization
    /// </summary>
    public Dictionary<string, string> LokiLabels { get; set; } = new();

    /// <summary>
    /// Enable automatic correlation ID generation and propagation
    /// </summary>
    public bool EnableCorrelationId { get; set; } = true;

    /// <summary>
    /// Enable runtime instrumentation for .NET metrics
    /// </summary>
    public bool EnableRuntimeInstrumentation { get; set; } = true;

    /// <summary>
    /// Enable HTTP client instrumentation for outbound requests
    /// </summary>
    public bool EnableHttpClientInstrumentation { get; set; } = true;

    /// <summary>
    /// Enable ASP.NET Core instrumentation for web applications
    /// </summary>
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;

    // Serilog-specific configurations

    /// <summary>
    /// Enable file logging output
    /// </summary>
    public bool EnableFileLogging { get; set; } = false;

    /// <summary>
    /// Path for file logging (default: Logs/{ServiceName}-.log)
    /// </summary>
    public string? FileLoggingPath { get; set; }

    /// <summary>
    /// Enable Seq logging integration
    /// </summary>
    public bool EnableSeqLogging { get; set; } = false;

    /// <summary>
    /// Seq endpoint URL for log aggregation
    /// </summary>
    public string? SeqUrl { get; set; }

    /// <summary>
    /// Enable Elasticsearch logging integration
    /// </summary>
    public bool EnableElasticsearchLogging { get; set; } = false;

    /// <summary>
    /// Elasticsearch endpoint URL for log aggregation
    /// </summary>
    public string? ElasticsearchUrl { get; set; }

    /// <summary>
    /// Custom output template for console logging
    /// </summary>
    public string? ConsoleOutputTemplate { get; set; }

    /// <summary>
    /// Custom output template for file logging
    /// </summary>
    public string? FileOutputTemplate { get; set; }

    /// <summary>
    /// Enable request logging middleware
    /// </summary>
    public bool EnableRequestLogging { get; set; } = true;

    /// <summary>
    /// Enable performance logging for slow requests (in milliseconds)
    /// </summary>
    public int? SlowRequestThreshold { get; set; } = 1000;

    /// <summary>
    /// Additional Serilog enrichers to include
    /// </summary>
    public List<string> AdditionalEnrichers { get; set; } = new();

    /// <summary>
    /// Custom properties to add to all log events
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}