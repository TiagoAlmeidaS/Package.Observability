using System.Diagnostics;

namespace Package.Observability;

/// <summary>
/// Factory for creating and managing ActivitySource instances for distributed tracing
/// </summary>
public static class ActivitySourceFactory
{
    private static readonly Dictionary<string, ActivitySource> _activitySources = new();
    private static readonly object _lock = new();

    /// <summary>
    /// Gets or creates an ActivitySource for the specified service name
    /// </summary>
    /// <param name="serviceName">The name of the service</param>
    /// <param name="version">Optional version of the service</param>
    /// <returns>ActivitySource instance</returns>
    public static ActivitySource GetOrCreate(string serviceName, string? version = null)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));

        var key = $"{serviceName}:{version ?? "1.0.0"}";

        if (_activitySources.TryGetValue(key, out var existingSource))
            return existingSource;

        lock (_lock)
        {
            if (_activitySources.TryGetValue(key, out existingSource))
                return existingSource;

            var activitySource = new ActivitySource(serviceName, version ?? "1.0.0");
            _activitySources[key] = activitySource;
            return activitySource;
        }
    }

    /// <summary>
    /// Creates a new activity with the specified name and kind
    /// </summary>
    /// <param name="serviceName">The service name</param>
    /// <param name="activityName">The name of the activity</param>
    /// <param name="kind">The kind of activity</param>
    /// <param name="parentContext">Optional parent activity context</param>
    /// <returns>Activity instance or null if not sampled</returns>
    public static Activity? StartActivity(
        string serviceName, 
        string activityName, 
        ActivityKind kind = ActivityKind.Internal,
        ActivityContext parentContext = default)
    {
        var source = GetOrCreate(serviceName);
        return source.StartActivity(activityName, kind, parentContext);
    }

    /// <summary>
    /// Disposes all created ActivitySource instances
    /// </summary>
    public static void DisposeAll()
    {
        lock (_lock)
        {
            foreach (var source in _activitySources.Values)
            {
                source.Dispose();
            }
            _activitySources.Clear();
        }
    }
}