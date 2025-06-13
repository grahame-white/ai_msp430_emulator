using System.Collections.Generic;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Tests.TestUtilities;

/// <summary>
/// Test implementation of ILogger for use in unit tests.
/// Captures log entries for verification and provides standard test logging behavior.
/// </summary>
public class TestLogger : ILogger
{
    public LogLevel MinimumLevel { get; set; } = LogLevel.Warning;
    public List<LogEntry> LogEntries { get; } = new();

    public void Log(LogLevel level, string message)
    {
        if (IsEnabled(level))
        {
            LogEntries.Add(new LogEntry(level, message, null));
        }
    }

    public void Log(LogLevel level, string message, object? context)
    {
        if (IsEnabled(level))
        {
            LogEntries.Add(new LogEntry(level, message, context));
        }
    }

    public void Debug(string message) => Log(LogLevel.Debug, message);
    public void Debug(string message, object? context) => Log(LogLevel.Debug, message, context);
    public void Info(string message) => Log(LogLevel.Info, message);
    public void Info(string message, object? context) => Log(LogLevel.Info, message, context);
    public void Warning(string message) => Log(LogLevel.Warning, message);
    public void Warning(string message, object? context) => Log(LogLevel.Warning, message, context);
    public void Error(string message) => Log(LogLevel.Error, message);
    public void Error(string message, object? context) => Log(LogLevel.Error, message, context);
    public void Fatal(string message) => Log(LogLevel.Fatal, message);
    public void Fatal(string message, object? context) => Log(LogLevel.Fatal, message, context);

    public bool IsEnabled(LogLevel level) => level >= MinimumLevel;
}

/// <summary>
/// Represents a single log entry captured during testing.
/// </summary>
/// <param name="Level">The log level of the entry.</param>
/// <param name="Message">The log message.</param>
/// <param name="Context">Optional context object associated with the log entry.</param>
public record LogEntry(LogLevel Level, string Message, object? Context);
