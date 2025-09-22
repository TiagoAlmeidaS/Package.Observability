using System.Diagnostics.Metrics;

namespace Package.Observability.Telemetry;

/// <summary>
/// Atributo para marcar métodos que devem ser instrumentados automaticamente
/// Similar ao Application Insights, mas usando OpenTelemetry
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class TelemetryAttribute : Attribute
{
    /// <summary>
    /// Nome da métrica personalizada
    /// </summary>
    public string? MetricName { get; set; }

    /// <summary>
    /// Descrição da métrica
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tipo da métrica (Counter, Histogram, Gauge)
    /// </summary>
    public MetricType Type { get; set; } = MetricType.Counter;

    /// <summary>
    /// Unidade da métrica
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Tags adicionais para a métrica
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Se deve capturar duração do método
    /// </summary>
    public bool CaptureDuration { get; set; } = true;

    /// <summary>
    /// Se deve capturar exceções
    /// </summary>
    public bool CaptureExceptions { get; set; } = true;
}

/// <summary>
/// Tipos de métricas suportadas
/// </summary>
public enum MetricType
{
    Counter,
    Histogram,
    Gauge
}
