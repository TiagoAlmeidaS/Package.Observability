using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Package.Observability.Telemetry;

/// <summary>
/// Sistema de telemetria automática - ZERO CONFIGURAÇÃO
/// Similar ao Tempo, Loki e Prometheus - funciona automaticamente
/// </summary>
public static class ZeroConfigTelemetry
{
    private static readonly Dictionary<string, Counter<long>> _counters = new();
    private static readonly Dictionary<string, Histogram<double>> _histograms = new();
    private static readonly object _lock = new();
    private static Meter? _meter;
    private static bool _initialized = false;

    /// <summary>
    /// Inicializa a telemetria automática
    /// </summary>
    public static void Initialize(string serviceName)
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;

            _meter = new Meter(serviceName, "1.0.0");
            _initialized = true;
        }
    }

    /// <summary>
    /// Registra uma requisição HTTP automaticamente
    /// </summary>
    public static void RecordHttpRequest(string method, string path, int statusCode, double duration)
    {
        if (!_initialized || _meter == null) return;

        try
        {
            // Registrar contador de requisições
            var counterKey = "http_requests_total";
            if (!_counters.ContainsKey(counterKey))
            {
                lock (_lock)
                {
                    if (!_counters.ContainsKey(counterKey))
                    {
                        _counters[counterKey] = _meter.CreateCounter<long>(counterKey, "count", "Total HTTP requests");
                    }
                }
            }
            _counters[counterKey].Add(1, new KeyValuePair<string, object?>("method", method), 
                new KeyValuePair<string, object?>("path", path), 
                new KeyValuePair<string, object?>("status_code", statusCode.ToString()));

            // Registrar duração da requisição
            var histogramKey = "http_request_duration_seconds";
            if (!_histograms.ContainsKey(histogramKey))
            {
                lock (_lock)
                {
                    if (!_histograms.ContainsKey(histogramKey))
                    {
                        _histograms[histogramKey] = _meter.CreateHistogram<double>(histogramKey, "seconds", "HTTP request duration");
                    }
                }
            }
            _histograms[histogramKey].Record(duration, new KeyValuePair<string, object?>("method", method), 
                new KeyValuePair<string, object?>("path", path), 
                new KeyValuePair<string, object?>("status_code", statusCode.ToString()));

            // Registrar erros se status >= 400
            if (statusCode >= 400)
            {
                var errorCounterKey = "http_requests_errors_total";
                if (!_counters.ContainsKey(errorCounterKey))
                {
                    lock (_lock)
                    {
                        if (!_counters.ContainsKey(errorCounterKey))
                        {
                            _counters[errorCounterKey] = _meter.CreateCounter<long>(errorCounterKey, "count", "Total HTTP request errors");
                        }
                    }
                }
                _counters[errorCounterKey].Add(1, new KeyValuePair<string, object?>("method", method), 
                    new KeyValuePair<string, object?>("path", path), 
                    new KeyValuePair<string, object?>("status_code", statusCode.ToString()));
            }
        }
        catch (Exception)
        {
            // Silently ignore errors in auto-telemetry
        }
    }

    /// <summary>
    /// Registra uma chamada de método automaticamente
    /// </summary>
    public static void RecordMethodCall(string className, string methodName, double duration, bool success, string? error = null)
    {
        if (!_initialized || _meter == null) return;

        try
        {
            var counterKey = $"app_{className.ToLowerInvariant()}_{methodName.ToLowerInvariant()}_calls_total";
            var durationKey = $"app_{className.ToLowerInvariant()}_{methodName.ToLowerInvariant()}_duration_seconds";

            var tags = new List<KeyValuePair<string, object?>>
            {
                new("class", className),
                new("method", methodName),
                new("success", success.ToString())
            };

            if (!string.IsNullOrEmpty(error))
            {
                tags.Add(new("error", error));
            }

            // Registrar contador de chamadas
            if (!_counters.ContainsKey(counterKey))
            {
                lock (_lock)
                {
                    if (!_counters.ContainsKey(counterKey))
                    {
                        _counters[counterKey] = _meter.CreateCounter<long>(counterKey, "count", $"Total calls to {className}.{methodName}");
                    }
                }
            }
            _counters[counterKey].Add(1, tags.ToArray());

            // Registrar duração
            if (!_histograms.ContainsKey(durationKey))
            {
                lock (_lock)
                {
                    if (!_histograms.ContainsKey(durationKey))
                    {
                        _histograms[durationKey] = _meter.CreateHistogram<double>(durationKey, "seconds", $"Duration of {className}.{methodName}");
                    }
                }
            }
            _histograms[durationKey].Record(duration, tags.ToArray());
        }
        catch (Exception)
        {
            // Silently ignore errors in auto-telemetry
        }
    }
}

/// <summary>
/// Middleware de telemetria automática - ZERO CONFIGURAÇÃO
/// </summary>
public class ZeroConfigTelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ZeroConfigTelemetryMiddleware> _logger;
    private readonly ObservabilityOptions _options;

    public ZeroConfigTelemetryMiddleware(
        RequestDelegate next,
        ILogger<ZeroConfigTelemetryMiddleware> logger,
        IOptions<ObservabilityOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;

        // Inicializar telemetria automática
        ZeroConfigTelemetry.Initialize(_options.ServiceName);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.EnableMetrics && !_options.EnableTracing && !_options.EnableLogging)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;
        var statusCode = 200; // Default

        try
        {
            // Log automático se habilitado
            if (_options.EnableLogging)
            {
                _logger.LogInformation(
                    "Request starting {Method} {Path} from {RemoteIp}",
                    method, requestPath, context.Connection.RemoteIpAddress);
            }

            await _next(context);

            statusCode = context.Response.StatusCode;
        }
        catch (Exception ex)
        {
            statusCode = 500;
            
            // Log automático de erro se habilitado
            if (_options.EnableLogging)
            {
                _logger.LogError(ex,
                    "Request failed {Method} {Path} with error: {ErrorMessage}",
                    method, requestPath, ex.Message);
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalSeconds;

            // Métricas automáticas se habilitadas
            if (_options.EnableMetrics)
            {
                ZeroConfigTelemetry.RecordHttpRequest(method, requestPath, statusCode, duration);
            }

            // Log automático de conclusão se habilitado
            if (_options.EnableLogging)
            {
                _logger.LogInformation(
                    "Request completed {Method} {Path} {StatusCode} in {Duration}ms",
                    method, requestPath, statusCode, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
