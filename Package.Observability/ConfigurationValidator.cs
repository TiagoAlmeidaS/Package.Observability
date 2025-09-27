using System.Text.RegularExpressions;

namespace Package.Observability;

/// <summary>
/// Validador de configuração para o pacote de observabilidade
/// </summary>
public static class ConfigurationValidator
{
    private static readonly Regex UrlRegex = new(
        @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PortRegex = new(
        @"^([1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$",
        RegexOptions.Compiled);

    /// <summary>
    /// Valida as opções de observabilidade
    /// </summary>
    /// <param name="options">Opções a serem validadas</param>
    /// <returns>Resultado da validação</returns>
    public static ValidationResult Validate(ObservabilityOptions options)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validação do nome do serviço
        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            errors.Add("ServiceName não pode ser nulo ou vazio");
        }
        else if (options.ServiceName.Length > 50)
        {
            warnings.Add("ServiceName muito longo (máximo recomendado: 50 caracteres)");
        }

        // Validação da porta do Prometheus
        if (options.PrometheusPort < 1 || options.PrometheusPort > 65535)
        {
            errors.Add($"PrometheusPort deve estar entre 1 e 65535. Valor atual: {options.PrometheusPort}");
        }
        else if (options.PrometheusPort < 1024)
        {
            warnings.Add("Portas abaixo de 1024 podem requerer privilégios de administrador");
        }

        // Validação da URL do Loki
        if (!string.IsNullOrEmpty(options.LokiUrl) && !IsValidUrl(options.LokiUrl))
        {
            errors.Add($"LokiUrl inválida: {options.LokiUrl}");
        }

        // Validação dos endpoints de tracing apenas se tracing estiver habilitado
        if (options.EnableTracing)
        {
            // Validação do endpoint OTLP
            if (!string.IsNullOrEmpty(options.OtlpEndpoint) && !IsValidUrl(options.OtlpEndpoint))
            {
                errors.Add($"OtlpEndpoint inválido: {options.OtlpEndpoint}");
            }

            // Validação do endpoint do Tempo
            if (!string.IsNullOrEmpty(options.TempoEndpoint) && !IsValidUrl(options.TempoEndpoint))
            {
                errors.Add($"TempoEndpoint inválido: {options.TempoEndpoint}");
            }

            // Validação do endpoint do Collector
            if (!string.IsNullOrEmpty(options.CollectorEndpoint) && !IsValidUrl(options.CollectorEndpoint))
            {
                errors.Add($"CollectorEndpoint inválido: {options.CollectorEndpoint}");
            }
        }

        // Validação do nível de log
        if (!string.IsNullOrEmpty(options.MinimumLogLevel))
        {
            var validLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical", "Fatal" };
            if (!validLevels.Contains(options.MinimumLogLevel, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"MinimumLogLevel inválido: {options.MinimumLogLevel}. Valores válidos: {string.Join(", ", validLevels)}");
            }
        }

        // Validação dos labels adicionais
        if (options.AdditionalLabels != null)
        {
            foreach (var label in options.AdditionalLabels)
            {
                if (string.IsNullOrWhiteSpace(label.Key))
                {
                    errors.Add("Chave de label adicional não pode ser nula ou vazia");
                }
                else if (label.Key.Length > 50)
                {
                    warnings.Add($"Chave de label '{label.Key}' muito longa (máximo recomendado: 50 caracteres)");
                }

                if (string.IsNullOrWhiteSpace(label.Value))
                {
                    warnings.Add($"Valor do label '{label.Key}' está vazio");
                }
                else if (label.Value.Length > 200)
                {
                    warnings.Add($"Valor do label '{label.Key}' muito longo (máximo recomendado: 200 caracteres)");
                }
            }
        }

        // Validação dos labels do Loki
        if (options.LokiLabels != null)
        {
            foreach (var label in options.LokiLabels)
            {
                if (string.IsNullOrWhiteSpace(label.Key))
                {
                    errors.Add("Chave de label do Loki não pode ser nula ou vazia");
                }
                else if (label.Key.Length > 50)
                {
                    warnings.Add($"Chave de label do Loki '{label.Key}' muito longa (máximo recomendado: 50 caracteres)");
                }

                if (string.IsNullOrWhiteSpace(label.Value))
                {
                    warnings.Add($"Valor do label do Loki '{label.Key}' está vazio");
                }
                else if (label.Value.Length > 200)
                {
                    warnings.Add($"Valor do label do Loki '{label.Key}' muito longo (máximo recomendado: 200 caracteres)");
                }
            }
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Valida se uma string é uma URL válida
    /// </summary>
    /// <param name="url">URL a ser validada</param>
    /// <returns>True se a URL for válida</returns>
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Valida se uma porta é válida
    /// </summary>
    /// <param name="port">Porta a ser validada</param>
    /// <returns>True se a porta for válida</returns>
    public static bool IsValidPort(int port)
    {
        return port >= 1 && port <= 65535;
    }

    /// <summary>
    /// Valida se um nível de log é válido
    /// </summary>
    /// <param name="logLevel">Nível de log a ser validado</param>
    /// <returns>True se o nível for válido</returns>
    public static bool IsValidLogLevel(string logLevel)
    {
        if (string.IsNullOrWhiteSpace(logLevel))
            return false;

        var validLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical", "Fatal" };
        return validLevels.Contains(logLevel, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Resultado da validação de configuração
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indica se a configuração é válida
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Lista de erros encontrados
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Lista de avisos encontrados
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Retorna uma representação em string do resultado da validação
    /// </summary>
    public override string ToString()
    {
        var result = new List<string>();
        
        if (IsValid)
        {
            result.Add("✅ Configuração válida");
        }
        else
        {
            result.Add("❌ Configuração inválida");
        }

        if (Errors.Count > 0)
        {
            result.Add($"\nErros ({Errors.Count}):");
            foreach (var error in Errors)
            {
                result.Add($"  • {error}");
            }
        }

        if (Warnings.Count > 0)
        {
            result.Add($"\nAvisos ({Warnings.Count}):");
            foreach (var warning in Warnings)
            {
                result.Add($"  ⚠️ {warning}");
            }
        }

        return string.Join("\n", result);
    }
}
