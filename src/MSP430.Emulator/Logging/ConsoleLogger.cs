using System;

namespace MSP430.Emulator.Logging;

/// <summary>
/// A logger implementation that writes log messages to the console.
/// </summary>
public class ConsoleLogger : ILogger
{
    /// <inheritdoc/>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Gets or sets whether to redirect error-level messages to stdout instead of stderr.
    /// This is useful in test environments where stderr output may cause CI failures.
    /// </summary>
    public bool RedirectErrorsToStdout { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to suppress all console output.
    /// This is useful in CI environments where log content may be interpreted as CI failures.
    /// </summary>
    public bool IsOutputSuppressed { get; set; } = false;

    /// <inheritdoc/>
    public void Log(LogLevel level, string message)
    {
        Log(level, message, null);
    }

    /// <inheritdoc/>
    public virtual void Log(LogLevel level, string message, object? context)
    {
        if (!IsEnabled(level))
        {
            return;
        }

        // If output is suppressed (e.g., in CI environments), skip console output entirely
        if (IsOutputSuppressed)
        {
            return;
        }

        string logEntry = LogEntryFormatter.FormatLogEntry(level, message, context);

        // Write to appropriate output stream based on level
        // In test environments, redirect errors to stdout to avoid CI failures
        if (level >= LogLevel.Error && !RedirectErrorsToStdout)
        {
            Console.Error.WriteLine(logEntry);
        }
        else
        {
            Console.WriteLine(logEntry);
        }
    }

    /// <inheritdoc/>
    public void Debug(string message) => Log(LogLevel.Debug, message);

    /// <inheritdoc/>
    public void Debug(string message, object? context) => Log(LogLevel.Debug, message, context);

    /// <inheritdoc/>
    public void Info(string message) => Log(LogLevel.Info, message);

    /// <inheritdoc/>
    public void Info(string message, object? context) => Log(LogLevel.Info, message, context);

    /// <inheritdoc/>
    public void Warning(string message) => Log(LogLevel.Warning, message);

    /// <inheritdoc/>
    public void Warning(string message, object? context) => Log(LogLevel.Warning, message, context);

    /// <inheritdoc/>
    public void Error(string message) => Log(LogLevel.Error, message);

    /// <inheritdoc/>
    public void Error(string message, object? context) => Log(LogLevel.Error, message, context);

    /// <inheritdoc/>
    public void Fatal(string message) => Log(LogLevel.Fatal, message);

    /// <inheritdoc/>
    public void Fatal(string message, object? context) => Log(LogLevel.Fatal, message, context);

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel level) => level >= MinimumLevel;
}
