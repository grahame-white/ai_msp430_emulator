namespace MSP430.Emulator.Logging;

/// <summary>
/// Defines the contract for logging functionality in the MSP430 emulator.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Gets or sets the minimum log level that will be written.
    /// </summary>
    LogLevel MinimumLevel { get; set; }

    /// <summary>
    /// Logs a message at the specified level.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The message to log.</param>
    void Log(LogLevel level, string message);

    /// <summary>
    /// Logs a message at the specified level with structured context.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Additional structured context data.</param>
    void Log(LogLevel level, string message, object? context);

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Debug(string message);

    /// <summary>
    /// Logs a debug message with context.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Additional structured context data.</param>
    void Debug(string message, object? context);

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Info(string message);

    /// <summary>
    /// Logs an info message with context.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Additional structured context data.</param>
    void Info(string message, object? context);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Warning(string message);

    /// <summary>
    /// Logs a warning message with context.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Additional structured context data.</param>
    void Warning(string message, object? context);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Error(string message);

    /// <summary>
    /// Logs an error message with context.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Additional structured context data.</param>
    void Error(string message, object? context);

    /// <summary>
    /// Logs a fatal message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Fatal(string message);

    /// <summary>
    /// Logs a fatal message with context.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="context">Additional structured context data.</param>
    void Fatal(string message, object? context);

    /// <summary>
    /// Determines if logging is enabled for the specified level.
    /// </summary>
    /// <param name="level">The log level to check.</param>
    /// <returns>True if logging is enabled for the level, false otherwise.</returns>
    bool IsEnabled(LogLevel level);
}
