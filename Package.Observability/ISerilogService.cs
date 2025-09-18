using Serilog;
using Serilog.Events;

namespace Package.Observability;

/// <summary>
/// Interface for SerilogService to enable mocking in unit tests
/// </summary>
public interface ISerilogService
{
    /// <summary>
    /// Gets whether the service is configured
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Gets the Serilog logger instance
    /// </summary>
    Serilog.ILogger? SerilogLogger { get; }

    /// <summary>
    /// Configures the Serilog service
    /// </summary>
    /// <returns>True if configuration was successful</returns>
    bool Configure();

    /// <summary>
    /// Logs a message with the specified level
    /// </summary>
    /// <param name="level">Log level</param>
    /// <param name="message">Message template</param>
    /// <param name="args">Message arguments</param>
    void Log(LogEventLevel level, string message, params object[] args);

    /// <summary>
    /// Logs an exception with the specified level
    /// </summary>
    /// <param name="level">Log level</param>
    /// <param name="exception">Exception to log</param>
    /// <param name="message">Message template</param>
    /// <param name="args">Message arguments</param>
    void Log(LogEventLevel level, Exception exception, string message, params object[] args);

    /// <summary>
    /// Creates a scoped logger with additional properties
    /// </summary>
    /// <param name="properties">Additional properties to include</param>
    /// <returns>Scoped logger instance</returns>
    Serilog.ILogger CreateScopedLogger(Dictionary<string, object> properties);

    /// <summary>
    /// Flushes any pending log events
    /// </summary>
    void Flush();

    /// <summary>
    /// Gets the current configuration status
    /// </summary>
    /// <returns>Configuration status information</returns>
    SerilogConfigurationStatus GetConfigurationStatus();
}

