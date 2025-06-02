using System.Text.Json;

namespace MSP430.Emulator.Logging;

/// <summary>
/// Provides shared formatting functionality for log entries.
/// </summary>
public static class LogEntryFormatter
{
    /// <summary>
    /// Formats a log entry with timestamp, level, message, and optional context.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="context">Optional context data to serialize as JSON.</param>
    /// <returns>A formatted log entry string.</returns>
    public static string FormatLogEntry(LogLevel level, string message, object? context = null)
    {
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        string levelString = level.ToString().ToUpper();

        string logEntry = $"[{timestamp}] [{levelString}] {message}";

        if (context != null)
        {
            string contextJson = JsonSerializer.Serialize(context, new JsonSerializerOptions
            {
                WriteIndented = false
            });
            logEntry += $" Context: {contextJson}";
        }

        return logEntry;
    }
}
