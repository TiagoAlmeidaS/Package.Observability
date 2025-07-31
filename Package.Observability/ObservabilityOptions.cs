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
}