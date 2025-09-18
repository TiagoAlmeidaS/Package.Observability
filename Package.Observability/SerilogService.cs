using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.File;
using Serilog.Sinks.Seq;
using Package.Observability.Exceptions;

namespace Package.Observability;

/// <summary>
/// Service for managing Serilog configuration and operations
/// </summary>
public class SerilogService : ISerilogService
{
    private readonly ObservabilityOptions _options;
    private readonly ILogger<SerilogService> _logger;
    private Serilog.ILogger? _serilogLogger;
    private bool _isConfigured = false;

    public SerilogService(IOptions<ObservabilityOptions> options, ILogger<SerilogService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Gets whether Serilog is currently configured and ready
    /// </summary>
    public bool IsConfigured => _isConfigured;

    /// <summary>
    /// Gets the current Serilog logger instance
    /// </summary>
    public Serilog.ILogger? SerilogLogger => _serilogLogger;

    /// <summary>
    /// Configures Serilog with the current options
    /// </summary>
    /// <returns>True if configuration was successful, false otherwise</returns>
    public bool Configure()
    {
        try
        {
            if (!_options.EnableLogging)
            {
                _logger.LogWarning("Logging is disabled in configuration");
                return false;
            }

            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", _options.ServiceName);

            // Set minimum log level
            if (Enum.TryParse<LogEventLevel>(_options.MinimumLogLevel, true, out var logLevel))
            {
                loggerConfiguration.MinimumLevel.Is(logLevel);
            }

            // Add enrichers
            loggerConfiguration
                .Enrich.WithEnvironmentName()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId();

            // Add correlation ID if enabled
            if (_options.EnableCorrelationId)
            {
                loggerConfiguration.Enrich.WithProperty("CorrelationId", Guid.NewGuid().ToString());
            }

            // Console sink
            if (_options.EnableConsoleLogging)
            {
                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {ServiceName} {Message:lj}{NewLine}{Exception}");
            }

            // Loki sink
            if (!string.IsNullOrWhiteSpace(_options.LokiUrl))
            {
                var lokiLabels = new List<LokiLabel>
                {
                    new() { Key = "service", Value = _options.ServiceName },
                    new() { Key = "level", Value = "{Level}" }
                };

                // Add custom Loki labels
                foreach (var label in _options.LokiLabels)
                {
                    lokiLabels.Add(new LokiLabel { Key = label.Key, Value = label.Value });
                }

                loggerConfiguration.WriteTo.GrafanaLoki(
                    uri: _options.LokiUrl,
                    labels: lokiLabels);
            }

            // File sink (if configured)
            if (_options.EnableFileLogging)
            {
                var filePath = _options.FileLoggingPath ?? $"Logs/{_options.ServiceName}-.log";
                loggerConfiguration.WriteTo.File(
                    filePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {ServiceName} {Message:lj}{NewLine}{Exception}");
            }

            // Additional sinks based on configuration
            ConfigureAdditionalSinks(loggerConfiguration);

            _serilogLogger = loggerConfiguration.CreateLogger();
            _isConfigured = true;

            _logger.LogInformation("Serilog configured successfully with {SinkCount} sinks", 
                GetSinkCount());

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Serilog");
            _isConfigured = false;
            return false;
        }
    }

    /// <summary>
    /// Logs a structured message using Serilog
    /// </summary>
    /// <param name="level">Log level</param>
    /// <param name="messageTemplate">Message template</param>
    /// <param name="args">Message arguments</param>
    public void Log(LogEventLevel level, string messageTemplate, params object[] args)
    {
        if (!_isConfigured || _serilogLogger == null)
        {
            _logger.LogWarning("Serilog is not configured, falling back to default logger");
            return;
        }

        _serilogLogger.Write(level, messageTemplate, args);
    }

    public void Log(LogEventLevel level, Exception exception, string messageTemplate, params object[] args)
    {
        if (!_isConfigured || _serilogLogger == null)
        {
            _logger.LogWarning("Serilog is not configured, falling back to default logger");
            return;
        }

        _serilogLogger.Write(level, exception, messageTemplate, args);
    }

    /// <summary>
    /// Logs an exception with context
    /// </summary>
    /// <param name="level">Log level</param>
    /// <param name="exception">Exception to log</param>
    /// <param name="messageTemplate">Message template</param>
    /// <param name="args">Message arguments</param>
    public void LogException(LogEventLevel level, Exception exception, string messageTemplate, params object[] args)
    {
        if (!_isConfigured || _serilogLogger == null)
        {
            _logger.LogWarning("Serilog is not configured, falling back to default logger");
            return;
        }

        _serilogLogger.Write(level, exception, messageTemplate, args);
    }

    /// <summary>
    /// Creates a scoped logger with additional properties
    /// </summary>
    /// <param name="properties">Additional properties to include</param>
    /// <returns>Scoped logger instance</returns>
        public Serilog.ILogger CreateScopedLogger(Dictionary<string, object> properties)
        {
            if (!_isConfigured || _serilogLogger == null)
            {
                // Return a mock Serilog logger that wraps the Microsoft logger
                return new SerilogLoggerWrapper(_logger);
            }

            // Convert Dictionary to ILogEventEnricher
            var enricher = _serilogLogger;
            foreach (var property in properties)
            {
                enricher = enricher.ForContext(property.Key, property.Value);
            }
            return enricher;
        }

    /// <summary>
    /// Flushes any pending log events
    /// </summary>
        public void Flush()
        {
            if (_serilogLogger != null)
            {
                // Serilog ILogger doesn't have Dispose, but we can flush
                Serilog.Log.CloseAndFlush();
            }
        }

    /// <summary>
    /// Gets the current configuration status
    /// </summary>
    /// <returns>Configuration status information</returns>
    public SerilogConfigurationStatus GetConfigurationStatus()
    {
        return new SerilogConfigurationStatus
        {
            IsConfigured = _isConfigured,
            IsLoggingEnabled = _options.EnableLogging,
            IsConsoleLoggingEnabled = _options.EnableConsoleLogging,
            IsFileLoggingEnabled = _options.EnableFileLogging,
            IsLokiEnabled = !string.IsNullOrWhiteSpace(_options.LokiUrl),
            MinimumLogLevel = _options.MinimumLogLevel,
            ServiceName = _options.ServiceName,
            SinkCount = GetSinkCount()
        };
    }

    private void ConfigureAdditionalSinks(LoggerConfiguration loggerConfiguration)
    {
        // Configure additional sinks based on options
        // This can be extended for other sinks like Seq, Elasticsearch, etc.
        
        if (_options.EnableSeqLogging && !string.IsNullOrWhiteSpace(_options.SeqUrl))
        {
            loggerConfiguration.WriteTo.Seq(_options.SeqUrl);
        }

        if (_options.EnableElasticsearchLogging && !string.IsNullOrWhiteSpace(_options.ElasticsearchUrl))
        {
            // Note: This would require Serilog.Sinks.Elasticsearch package
            // loggerConfiguration.WriteTo.Elasticsearch(_options.ElasticsearchUrl);
        }
    }

    private int GetSinkCount()
    {
        if (!_isConfigured)
        {
            return 0;
        }

        int count = 0;
        if (_options.EnableConsoleLogging) count++;
        if (!string.IsNullOrWhiteSpace(_options.LokiUrl)) count++;
        if (_options.EnableFileLogging) count++;
        if (_options.EnableSeqLogging && !string.IsNullOrWhiteSpace(_options.SeqUrl)) count++;
        if (_options.EnableElasticsearchLogging && !string.IsNullOrWhiteSpace(_options.ElasticsearchUrl)) count++;
        return count;
    }
}

/// <summary>
/// Configuration status information for Serilog
/// </summary>
public class SerilogConfigurationStatus
{
    public bool IsConfigured { get; set; }
    public bool IsLoggingEnabled { get; set; }
    public bool IsConsoleLoggingEnabled { get; set; }
    public bool IsFileLoggingEnabled { get; set; }
    public bool IsLokiEnabled { get; set; }
    public string MinimumLogLevel { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int SinkCount { get; set; }
}

/// <summary>
/// Wrapper to convert Microsoft.Extensions.Logging.ILogger to Serilog.ILogger
/// </summary>
internal class SerilogLoggerWrapper : Serilog.ILogger
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public SerilogLoggerWrapper(Microsoft.Extensions.Logging.ILogger logger)
    {
        _logger = logger;
    }

    public void Write(LogEvent logEvent)
    {
        if (logEvent == null) return;

        var level = logEvent.Level switch
        {
            LogEventLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Trace,
            LogEventLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogEventLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            LogEventLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogEventLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogEventLevel.Fatal => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };

        _logger.Log(level, logEvent.Exception, logEvent.MessageTemplate.Text, logEvent.Properties.Values.ToArray());
    }

    public void Write(LogEventLevel level, string messageTemplate, params object?[]? propertyValues)
    {
        var msLevel = level switch
        {
            LogEventLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Trace,
            LogEventLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogEventLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            LogEventLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogEventLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogEventLevel.Fatal => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };

        _logger.Log(msLevel, messageTemplate, propertyValues);
    }

    public void Write(LogEventLevel level, Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        var msLevel = level switch
        {
            LogEventLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Trace,
            LogEventLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogEventLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            LogEventLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogEventLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogEventLevel.Fatal => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };

        _logger.Log(msLevel, exception, messageTemplate, propertyValues);
    }

    public bool IsEnabled(LogEventLevel level) => true;

    public Serilog.ILogger ForContext(ILogEventEnricher enricher) => this;
    public Serilog.ILogger ForContext(IEnumerable<ILogEventEnricher> enrichers) => this;
    public Serilog.ILogger ForContext(string propertyName, object? value, bool destructureObjects = false) => this;
    public Serilog.ILogger ForContext<TSource>() => this;
    public Serilog.ILogger ForContext(Type source) => this;
}

