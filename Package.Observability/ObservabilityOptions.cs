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

    // OpenTelemetry Tracing Advanced Configuration

    /// <summary>
    /// OTLP export protocol (Grpc or HttpProtobuf)
    /// </summary>
    public string OtlpProtocol { get; set; } = "Grpc";

    /// <summary>
    /// Service version for OpenTelemetry resource
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Enable exception recording in ASP.NET Core instrumentation
    /// </summary>
    public bool RecordExceptions { get; set; } = true;

    /// <summary>
    /// Paths to exclude from tracing (e.g., /metrics, /health)
    /// </summary>
    public List<string> ExcludePaths { get; set; } = new() { "/metrics", "/health" };

    // Custom Metrics Configuration

    /// <summary>
    /// Enable custom route-based metrics
    /// </summary>
    public bool EnableRouteMetrics { get; set; } = true;

    /// <summary>
    /// Custom histogram buckets for request duration metrics
    /// </summary>
    public List<double> CustomHistogramBuckets { get; set; } = new();

    /// <summary>
    /// Custom metric labels to include in all metrics
    /// </summary>
    public Dictionary<string, string> CustomMetricLabels { get; set; } = new();

    /// <summary>
    /// Enable detailed endpoint information in metrics
    /// </summary>
    public bool EnableDetailedEndpointMetrics { get; set; } = true;

    /// <summary>
    /// Custom metric names configuration
    /// </summary>
    public MetricNamesConfiguration MetricNames { get; set; } = new();
}

/// <summary>
/// Configuration for custom metric names
/// </summary>
public class MetricNamesConfiguration
{
    /// <summary>
    /// Name for HTTP requests counter metric
    /// </summary>
    public string HttpRequestsTotal { get; set; } = "http_requests_total_by_route";

    /// <summary>
    /// Name for HTTP request errors counter metric
    /// </summary>
    public string HttpRequestErrorsTotal { get; set; } = "http_requests_errors_total_by_route";

    /// <summary>
    /// Name for HTTP request duration histogram metric
    /// </summary>
    public string HttpRequestDurationSeconds { get; set; } = "http_request_duration_seconds_by_route";

    /// <summary>
    /// Description for HTTP requests counter metric
    /// </summary>
    public string HttpRequestsTotalDescription { get; set; } = "Total HTTP requests, labeled by method, endpoint name, and route template.";

    /// <summary>
    /// Description for HTTP request errors counter metric
    /// </summary>
    public string HttpRequestErrorsTotalDescription { get; set; } = "Total HTTP request errors (5xx or unhandled exceptions), labeled by method, endpoint name, and route template.";

    /// <summary>
    /// Description for HTTP request duration histogram metric
    /// </summary>
    public string HttpRequestDurationSecondsDescription { get; set; } = "HTTP request duration in seconds, labeled by method, endpoint name, and route template.";
}