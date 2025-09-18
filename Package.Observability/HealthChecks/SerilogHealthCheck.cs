using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Package.Observability;

namespace Package.Observability.HealthChecks;

/// <summary>
/// Health check específico para o SerilogService
/// </summary>
public class SerilogHealthCheck : IHealthCheck
{
    private readonly ISerilogService _serilogService;
    private readonly ObservabilityOptions _options;
    private readonly ILogger<SerilogHealthCheck> _logger;

    public SerilogHealthCheck(
        ISerilogService serilogService, 
        IOptions<ObservabilityOptions> options, 
        ILogger<SerilogHealthCheck> logger)
    {
        _serilogService = serilogService;
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

            var status = _serilogService.GetConfigurationStatus();
            var data = new Dictionary<string, object>
            {
                ["IsConfigured"] = status.IsConfigured,
                ["IsLoggingEnabled"] = status.IsLoggingEnabled,
                ["IsConsoleLoggingEnabled"] = status.IsConsoleLoggingEnabled,
                ["IsFileLoggingEnabled"] = status.IsFileLoggingEnabled,
                ["IsLokiEnabled"] = status.IsLokiEnabled,
                ["MinimumLogLevel"] = status.MinimumLogLevel,
                ["ServiceName"] = status.ServiceName,
                ["SinkCount"] = status.SinkCount,
                ["LokiUrl"] = _options.LokiUrl ?? "Não configurado",
                ["SeqUrl"] = _options.SeqUrl ?? "Não configurado",
                ["FileLoggingPath"] = _options.FileLoggingPath ?? "Padrão",
                ["EnableRequestLogging"] = _options.EnableRequestLogging,
                ["SlowRequestThreshold"] = _options.SlowRequestThreshold ?? 0
            };

            var issues = new List<string>();

            // Verificar se o Serilog está configurado
            if (!status.IsConfigured)
            {
                issues.Add("SerilogService não está configurado");
            }

            // Verificar se pelo menos um sink está habilitado
            if (status.SinkCount == 0)
            {
                issues.Add("Nenhum sink de logging está habilitado");
            }

            // Verificar configuração do Loki
            if (status.IsLokiEnabled)
            {
                if (string.IsNullOrEmpty(_options.LokiUrl))
                {
                    issues.Add("Loki habilitado mas URL não configurada");
                }
                else if (!IsValidUrl(_options.LokiUrl))
                {
                    issues.Add($"URL do Loki inválida: {_options.LokiUrl}");
                }
            }

            // Verificar configuração do Seq
            if (_options.EnableSeqLogging)
            {
                if (string.IsNullOrEmpty(_options.SeqUrl))
                {
                    issues.Add("Seq habilitado mas URL não configurada");
                }
                else if (!IsValidUrl(_options.SeqUrl))
                {
                    issues.Add($"URL do Seq inválida: {_options.SeqUrl}");
                }
            }

            // Verificar configuração do Elasticsearch
            if (_options.EnableElasticsearchLogging)
            {
                if (string.IsNullOrEmpty(_options.ElasticsearchUrl))
                {
                    issues.Add("Elasticsearch habilitado mas URL não configurada");
                }
                else if (!IsValidUrl(_options.ElasticsearchUrl))
                {
                    issues.Add($"URL do Elasticsearch inválida: {_options.ElasticsearchUrl}");
                }
            }

            // Verificar configuração de file logging
            if (_options.EnableFileLogging)
            {
                if (!string.IsNullOrEmpty(_options.FileLoggingPath))
                {
                    var directory = Path.GetDirectoryName(_options.FileLoggingPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        try
                        {
                            Directory.CreateDirectory(directory);
                            data["DirectoryCreated"] = true;
                        }
                        catch (Exception ex)
                        {
                            issues.Add($"Não foi possível criar diretório de logs: {ex.Message}");
                        }
                    }
                }
            }

            // Verificar nível de log válido
            if (!IsValidLogLevel(_options.MinimumLogLevel))
            {
                issues.Add($"Nível de log inválido: {_options.MinimumLogLevel}");
            }

            // Verificar threshold de slow requests
            if (_options.SlowRequestThreshold.HasValue && _options.SlowRequestThreshold.Value < 0)
            {
                issues.Add($"Threshold de slow requests inválido: {_options.SlowRequestThreshold}");
            }

            // Teste de logging (se configurado)
            if (status.IsConfigured)
            {
                try
                {
                    _serilogService.Log(Serilog.Events.LogEventLevel.Information, 
                        "Health check test log from {ServiceName} at {Timestamp}", 
                        _options.ServiceName, DateTime.UtcNow);
                    data["LoggingTest"] = "Success";
                }
                catch (Exception ex)
                {
                    issues.Add($"Falha no teste de logging: {ex.Message}");
                    data["LoggingTest"] = $"Failed: {ex.Message}";
                }
            }

            if (issues.Count > 0)
            {
                data["Issues"] = issues;
                return Task.FromResult(HealthCheckResult.Degraded(
                    "SerilogService com problemas de configuração",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("SerilogService funcionando corretamente", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar saúde do SerilogService");
            return Task.FromResult(HealthCheckResult.Unhealthy("Erro ao verificar SerilogService", ex));
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
