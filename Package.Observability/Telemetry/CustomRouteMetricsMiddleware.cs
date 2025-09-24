using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing.Matching;

namespace Package.Observability.Telemetry;

/// <summary>
/// Middleware for custom route-based metrics similar to the provided example
/// </summary>
public class CustomRouteMetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomRouteMetricsMiddleware> _logger;
    private readonly ObservabilityOptions _options;
    private readonly Counter<long> _requestByRouteCounter;
    private readonly Counter<long> _errorByRouteCounter;
    private readonly Histogram<double> _requestDurationByRoute;

    public CustomRouteMetricsMiddleware(
        RequestDelegate next,
        ILogger<CustomRouteMetricsMiddleware> logger,
        IOptions<ObservabilityOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;

        // Create custom metrics based on configuration
        var meter = new Meter(_options.ServiceName, _options.ServiceVersion);
        
        _requestByRouteCounter = meter.CreateCounter<long>(
            _options.MetricNames.HttpRequestsTotal,
            "count",
            _options.MetricNames.HttpRequestsTotalDescription);

        _errorByRouteCounter = meter.CreateCounter<long>(
            _options.MetricNames.HttpRequestErrorsTotal,
            "count",
            _options.MetricNames.HttpRequestErrorsTotalDescription);

        // Use custom buckets if provided, otherwise use default exponential buckets
        double[] buckets;
        if (_options.CustomHistogramBuckets?.Any() == true)
        {
            buckets = _options.CustomHistogramBuckets.ToArray();
        }
        else
        {
            // Default exponential buckets: 5ms .. ~163s
            buckets = new double[] { 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 25, 50, 100, 163 };
        }

        _requestDurationByRoute = meter.CreateHistogram<double>(
            _options.MetricNames.HttpRequestDurationSeconds,
            "seconds",
            _options.MetricNames.HttpRequestDurationSecondsDescription);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip counting the scrape itself to avoid noise
        if (context.Request.Path.StartsWithSegments("/metrics"))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();

        // Try to capture endpoint/route as early as possible
        var ep = context.GetEndpoint();
        string endpointName = ep?.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName
                              ?? ep?.DisplayName
                              ?? "unknown";
        
        var routeEp = ep as RouteEndpoint;
        string routeTemplate = routeEp?.RoutePattern?.RawText
                               ?? context.Request.Path.Value
                               ?? "/";

        // Add custom labels if configured
        var labels = new Dictionary<string, string>
        {
            { "method", context.Request.Method },
            { "endpoint", endpointName },
            { "route", routeTemplate }
        };

        // Add custom metric labels
        foreach (var customLabel in _options.CustomMetricLabels)
        {
            labels[customLabel.Key] = customLabel.Value;
        }

        try
        {
            await _next(context);

            // Consider 5xx as error
            if (context.Response.StatusCode >= 500)
            {
                RecordErrorMetric(labels);
            }
        }
        catch
        {
            // Count unhandled exception as error
            RecordErrorMetric(labels);
            throw;
        }
        finally
        {
            sw.Stop();
            
            // Record duration metric
            _requestDurationByRoute.Record(sw.Elapsed.TotalSeconds, 
                new KeyValuePair<string, object?>("method", labels["method"]),
                new KeyValuePair<string, object?>("endpoint", labels["endpoint"]),
                new KeyValuePair<string, object?>("route", labels["route"]));

            // Record request counter
            _requestByRouteCounter.Add(1,
                new KeyValuePair<string, object?>("method", labels["method"]),
                new KeyValuePair<string, object?>("endpoint", labels["endpoint"]),
                new KeyValuePair<string, object?>("route", labels["route"]));
        }
    }

    private void RecordErrorMetric(Dictionary<string, string> labels)
    {
        _errorByRouteCounter.Add(1,
            new KeyValuePair<string, object?>("method", labels["method"]),
            new KeyValuePair<string, object?>("endpoint", labels["endpoint"]),
            new KeyValuePair<string, object?>("route", labels["route"]));
    }
}
