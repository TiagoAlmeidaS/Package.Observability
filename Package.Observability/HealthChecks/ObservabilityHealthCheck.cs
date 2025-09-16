using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Package.Observability.HealthChecks;

/// <summary>
/// Health check para verificar o status dos componentes de observabilidade
/// </summary>
public class ObservabilityHealthCheck : IHealthCheck
{
    private readonly ObservabilityOptions _options;
    private readonly ILogger<ObservabilityHealthCheck> _logger;

    public ObservabilityHealthCheck(IOptions<ObservabilityOptions> options, ILogger<ObservabilityHealthCheck> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                ["ServiceName"] = _options.ServiceName,
                ["EnableMetrics"] = _options.EnableMetrics,
                ["EnableTracing"] = _options.EnableTracing,
                ["EnableLogging"] = _options.EnableLogging,
                ["PrometheusPort"] = _options.PrometheusPort
            };

            var issues = new List<string>();

            // Verificar configuração básica
            if (string.IsNullOrWhiteSpace(_options.ServiceName))
            {
                issues.Add("ServiceName não configurado");
            }

            // Verificar se pelo menos uma funcionalidade está habilitada
            if (!_options.EnableMetrics && !_options.EnableTracing && !_options.EnableLogging)
            {
                issues.Add("Nenhuma funcionalidade de observabilidade está habilitada");
            }

            // Verificar configuração de métricas
            if (_options.EnableMetrics)
            {
                if (_options.PrometheusPort < 1 || _options.PrometheusPort > 65535)
                {
                    issues.Add($"Porta do Prometheus inválida: {_options.PrometheusPort}");
                }
            }

            // Verificar configuração de logging
            if (_options.EnableLogging)
            {
                if (!string.IsNullOrEmpty(_options.LokiUrl) && !IsValidUrl(_options.LokiUrl))
                {
                    issues.Add($"URL do Loki inválida: {_options.LokiUrl}");
                }
            }

            // Verificar configuração de tracing
            if (_options.EnableTracing)
            {
                if (!string.IsNullOrEmpty(_options.OtlpEndpoint) && !IsValidUrl(_options.OtlpEndpoint))
                {
                    issues.Add($"Endpoint OTLP inválido: {_options.OtlpEndpoint}");
                }
            }

            // Verificar se há recursos registrados (simulação)
            data["RegisteredResources"] = 0;

            if (issues.Count > 0)
            {
                data["Issues"] = issues;
                return HealthCheckResult.Degraded(
                    "Observabilidade com problemas de configuração",
                    data: data);
            }

            return HealthCheckResult.Healthy("Observabilidade funcionando corretamente", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar saúde da observabilidade");
            return HealthCheckResult.Unhealthy("Erro ao verificar observabilidade", ex);
        }
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Health check específico para métricas
/// </summary>
public class MetricsHealthCheck : IHealthCheck
{
    private readonly ObservabilityOptions _options;
    private readonly ILogger<MetricsHealthCheck> _logger;

    public MetricsHealthCheck(IOptions<ObservabilityOptions> options, ILogger<MetricsHealthCheck> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.EnableMetrics)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Métricas desabilitadas"));
            }

            var data = new Dictionary<string, object>
            {
                ["PrometheusPort"] = _options.PrometheusPort,
                ["EnableRuntimeInstrumentation"] = _options.EnableRuntimeInstrumentation,
                ["EnableHttpClientInstrumentation"] = _options.EnableHttpClientInstrumentation,
                ["EnableAspNetCoreInstrumentation"] = _options.EnableAspNetCoreInstrumentation
            };

            // Verificar se a porta está disponível (simulação)
            if (_options.PrometheusPort < 1024)
            {
                data["Warning"] = "Porta abaixo de 1024 pode requerer privilégios de administrador";
            }

            return Task.FromResult(HealthCheckResult.Healthy("Métricas configuradas corretamente", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar saúde das métricas");
            return Task.FromResult(HealthCheckResult.Unhealthy("Erro ao verificar métricas", ex));
        }
    }
}

/// <summary>
/// Health check específico para tracing
/// </summary>
public class TracingHealthCheck : IHealthCheck
{
    private readonly ObservabilityOptions _options;
    private readonly ILogger<TracingHealthCheck> _logger;

    public TracingHealthCheck(IOptions<ObservabilityOptions> options, ILogger<TracingHealthCheck> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.EnableTracing)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Tracing desabilitado"));
            }

            var data = new Dictionary<string, object>
            {
                ["OtlpEndpoint"] = _options.OtlpEndpoint ?? "Não configurado",
                ["EnableHttpClientInstrumentation"] = _options.EnableHttpClientInstrumentation,
                ["EnableAspNetCoreInstrumentation"] = _options.EnableAspNetCoreInstrumentation
            };

            var issues = new List<string>();

            if (string.IsNullOrEmpty(_options.OtlpEndpoint))
            {
                issues.Add("Endpoint OTLP não configurado");
            }
            else if (!IsValidUrl(_options.OtlpEndpoint))
            {
                issues.Add($"Endpoint OTLP inválido: {_options.OtlpEndpoint}");
            }

            if (issues.Count > 0)
            {
                data["Issues"] = issues;
                return Task.FromResult(HealthCheckResult.Degraded("Tracing com problemas de configuração", data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Tracing configurado corretamente", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar saúde do tracing");
            return Task.FromResult(HealthCheckResult.Unhealthy("Erro ao verificar tracing", ex));
        }
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Health check específico para logging
/// </summary>
public class LoggingHealthCheck : IHealthCheck
{
    private readonly ObservabilityOptions _options;
    private readonly ILogger<LoggingHealthCheck> _logger;

    public LoggingHealthCheck(IOptions<ObservabilityOptions> options, ILogger<LoggingHealthCheck> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.EnableLogging)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Logging desabilitado"));
            }

            var data = new Dictionary<string, object>
            {
                ["LokiUrl"] = _options.LokiUrl ?? "Não configurado",
                ["EnableConsoleLogging"] = _options.EnableConsoleLogging,
                ["MinimumLogLevel"] = _options.MinimumLogLevel,
                ["EnableCorrelationId"] = _options.EnableCorrelationId
            };

            var issues = new List<string>();

            if (string.IsNullOrEmpty(_options.LokiUrl))
            {
                issues.Add("URL do Loki não configurada");
            }
            else if (!IsValidUrl(_options.LokiUrl))
            {
                issues.Add($"URL do Loki inválida: {_options.LokiUrl}");
            }

            if (!string.IsNullOrEmpty(_options.MinimumLogLevel) && !IsValidLogLevel(_options.MinimumLogLevel))
            {
                issues.Add($"Nível de log inválido: {_options.MinimumLogLevel}");
            }

            if (issues.Count > 0)
            {
                data["Issues"] = issues;
                return Task.FromResult(HealthCheckResult.Degraded("Logging com problemas de configuração", data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Logging configurado corretamente", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar saúde do logging");
            return Task.FromResult(HealthCheckResult.Unhealthy("Erro ao verificar logging", ex));
        }
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool IsValidLogLevel(string logLevel)
    {
        var validLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical", "Fatal" };
        return validLevels.Contains(logLevel, StringComparer.OrdinalIgnoreCase);
    }
}
