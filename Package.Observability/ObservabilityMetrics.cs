using System.Diagnostics.Metrics;

namespace Package.Observability;

/// <summary>
/// Factory for creating and managing custom metrics
/// </summary>
public static class ObservabilityMetrics
{
    private static readonly Dictionary<string, Meter> _meters = new();
    private static readonly object _lock = new();
    private static bool _metricsEnabled = true;

    /// <summary>
    /// Sets whether metrics are enabled
    /// </summary>
    /// <param name="enabled">True to enable metrics, false to disable</param>
    public static void SetMetricsEnabled(bool enabled)
    {
        _metricsEnabled = enabled;
        
        // Se desabilitando métricas, limpar todas as métricas existentes
        if (!enabled)
        {
            DisposeAll();
        }
    }

    /// <summary>
    /// Gets whether metrics are currently enabled
    /// </summary>
    public static bool IsMetricsEnabled => _metricsEnabled;

    /// <summary>
    /// Gets or creates a Meter for the specified service name
    /// </summary>
    /// <param name="serviceName">The name of the service</param>
    /// <param name="version">Optional version of the service</param>
    /// <returns>Meter instance</returns>
    public static Meter GetOrCreateMeter(string serviceName, string? version = null)
    {
        if (!_metricsEnabled)
            throw new InvalidOperationException("Metrics are disabled");

        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));

        var key = $"{serviceName}:{version ?? "1.0.0"}";

        if (_meters.TryGetValue(key, out var existingMeter))
            return existingMeter;

        lock (_lock)
        {
            if (_meters.TryGetValue(key, out existingMeter))
                return existingMeter;

            var meter = new Meter(serviceName, version ?? "1.0.0");
            _meters[key] = meter;
            return meter;
        }
    }

    /// <summary>
    /// Creates a counter metric
    /// </summary>
    /// <typeparam name="T">The type of the counter value</typeparam>
    /// <param name="serviceName">The service name</param>
    /// <param name="name">The name of the counter</param>
    /// <param name="unit">Optional unit of measurement</param>
    /// <param name="description">Optional description</param>
    /// <returns>Counter instance</returns>
    public static Counter<T> CreateCounter<T>(
        string serviceName,
        string name,
        string? unit = null,
        string? description = null) where T : struct
    {
        var meter = GetOrCreateMeter(serviceName);
        return meter.CreateCounter<T>(name, unit, description);
    }

    /// <summary>
    /// Creates a histogram metric
    /// </summary>
    /// <typeparam name="T">The type of the histogram value</typeparam>
    /// <param name="serviceName">The service name</param>
    /// <param name="name">The name of the histogram</param>
    /// <param name="unit">Optional unit of measurement</param>
    /// <param name="description">Optional description</param>
    /// <returns>Histogram instance</returns>
    public static Histogram<T> CreateHistogram<T>(
        string serviceName,
        string name,
        string? unit = null,
        string? description = null) where T : struct
    {
        var meter = GetOrCreateMeter(serviceName);
        return meter.CreateHistogram<T>(name, unit, description);
    }

    /// <summary>
    /// Creates a gauge metric
    /// </summary>
    /// <typeparam name="T">The type of the gauge value</typeparam>
    /// <param name="serviceName">The service name</param>
    /// <param name="name">The name of the gauge</param>
    /// <param name="unit">Optional unit of measurement</param>
    /// <param name="description">Optional description</param>
    /// <returns>ObservableGauge instance</returns>
    public static ObservableGauge<T> CreateObservableGauge<T>(
        string serviceName,
        string name,
        Func<T> observeValue,
        string? unit = null,
        string? description = null) where T : struct
    {
        var meter = GetOrCreateMeter(serviceName);
        return meter.CreateObservableGauge(name, observeValue, unit, description);
    }

    /// <summary>
    /// Creates an up-down counter metric
    /// </summary>
    /// <typeparam name="T">The type of the counter value</typeparam>
    /// <param name="serviceName">The service name</param>
    /// <param name="name">The name of the counter</param>
    /// <param name="unit">Optional unit of measurement</param>
    /// <param name="description">Optional description</param>
    /// <returns>UpDownCounter instance</returns>
    public static UpDownCounter<T> CreateUpDownCounter<T>(
        string serviceName,
        string name,
        string? unit = null,
        string? description = null) where T : struct
    {
        var meter = GetOrCreateMeter(serviceName);
        return meter.CreateUpDownCounter<T>(name, unit, description);
    }

    /// <summary>
    /// Disposes all created Meter instances
    /// </summary>
    public static void DisposeAll()
    {
        lock (_lock)
        {
            foreach (var meter in _meters.Values)
            {
                meter.Dispose();
            }
            _meters.Clear();
        }
    }
}