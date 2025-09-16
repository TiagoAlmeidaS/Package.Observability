namespace Package.Observability.Exceptions;

/// <summary>
/// Exceção base para erros relacionados à observabilidade
/// </summary>
public class ObservabilityException : Exception
{
    public ObservabilityException(string message) : base(message)
    {
    }

    public ObservabilityException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exceção lançada quando há problemas de configuração
/// </summary>
public class ObservabilityConfigurationException : ObservabilityException
{
    public string? ConfigurationKey { get; }
    public object? ConfigurationValue { get; }

    public ObservabilityConfigurationException(string message) : base(message)
    {
    }

    public ObservabilityConfigurationException(string message, string configurationKey, object? configurationValue) 
        : base(message)
    {
        ConfigurationKey = configurationKey;
        ConfigurationValue = configurationValue;
    }

    public ObservabilityConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ObservabilityConfigurationException(string message, string configurationKey, object? configurationValue, Exception innerException) 
        : base(message, innerException)
    {
        ConfigurationKey = configurationKey;
        ConfigurationValue = configurationValue;
    }
}

/// <summary>
/// Exceção lançada quando há problemas com métricas
/// </summary>
public class MetricsException : ObservabilityException
{
    public string? MetricName { get; }
    public string? ServiceName { get; }

    public MetricsException(string message) : base(message)
    {
    }

    public MetricsException(string message, string metricName, string serviceName) : base(message)
    {
        MetricName = metricName;
        ServiceName = serviceName;
    }

    public MetricsException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public MetricsException(string message, string metricName, string serviceName, Exception innerException) 
        : base(message, innerException)
    {
        MetricName = metricName;
        ServiceName = serviceName;
    }
}

/// <summary>
/// Exceção lançada quando há problemas com tracing
/// </summary>
public class TracingException : ObservabilityException
{
    public string? ActivityName { get; }
    public string? ServiceName { get; }

    public TracingException(string message) : base(message)
    {
    }

    public TracingException(string message, string activityName, string serviceName) : base(message)
    {
        ActivityName = activityName;
        ServiceName = serviceName;
    }

    public TracingException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TracingException(string message, string activityName, string serviceName, Exception innerException) 
        : base(message, innerException)
    {
        ActivityName = activityName;
        ServiceName = serviceName;
    }
}

/// <summary>
/// Exceção lançada quando há problemas com logging
/// </summary>
public class LoggingException : ObservabilityException
{
    public string? LogLevel { get; }
    public string? SinkName { get; }

    public LoggingException(string message) : base(message)
    {
    }

    public LoggingException(string message, string logLevel, string sinkName) : base(message)
    {
        LogLevel = logLevel;
        SinkName = sinkName;
    }

    public LoggingException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public LoggingException(string message, string logLevel, string sinkName, Exception innerException) 
        : base(message, innerException)
    {
        LogLevel = logLevel;
        SinkName = sinkName;
    }
}

/// <summary>
/// Exceção lançada quando há problemas de validação
/// </summary>
public class ValidationException : ObservabilityException
{
    public string? PropertyName { get; }
    public object? PropertyValue { get; }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, string propertyName, object? propertyValue) : base(message)
    {
        PropertyName = propertyName;
        PropertyValue = propertyValue;
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ValidationException(string message, string propertyName, object? propertyValue, Exception innerException) 
        : base(message, innerException)
    {
        PropertyName = propertyName;
        PropertyValue = propertyValue;
    }
}
