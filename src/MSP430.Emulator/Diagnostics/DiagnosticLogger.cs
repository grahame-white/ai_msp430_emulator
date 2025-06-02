using System.Collections.Concurrent;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Diagnostics;

/// <summary>
/// A logger wrapper that maintains a circular buffer of recent log entries for diagnostic reporting.
/// </summary>
public class DiagnosticLogger : ILogger, IDisposable
{
    private readonly ILogger _innerLogger;
    private readonly ConcurrentQueue<LogEntry> _recentEntries;
    private readonly int _maxEntries;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the DiagnosticLogger class.
    /// </summary>
    /// <param name="innerLogger">The underlying logger to wrap.</param>
    /// <param name="maxRecentEntries">Maximum number of recent entries to keep in memory (default: 100).</param>
    public DiagnosticLogger(ILogger innerLogger, int maxRecentEntries = 100)
    {
        _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
        _maxEntries = maxRecentEntries;
        _recentEntries = new ConcurrentQueue<LogEntry>();
    }

    /// <inheritdoc/>
    public LogLevel MinimumLevel 
    { 
        get => _innerLogger.MinimumLevel; 
        set => _innerLogger.MinimumLevel = value; 
    }

    /// <inheritdoc/>
    public void Log(LogLevel level, string message)
    {
        Log(level, message, null);
    }

    /// <inheritdoc/>
    public void Log(LogLevel level, string message, object? context)
    {
        if (_disposed) return;

        // Log to underlying logger
        _innerLogger.Log(level, message, context);
        
        // Store in recent entries buffer
        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            Context = context
        };
        
        _recentEntries.Enqueue(entry);
        
        // Maintain buffer size
        while (_recentEntries.Count > _maxEntries)
        {
            _recentEntries.TryDequeue(out _);
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
    public bool IsEnabled(LogLevel level) => _innerLogger.IsEnabled(level);

    /// <summary>
    /// Gets the recent log entries for diagnostic reporting.
    /// </summary>
    /// <param name="maxEntries">Maximum number of entries to return (0 for all).</param>
    /// <returns>An array of recent log entries, ordered chronologically.</returns>
    public LogEntry[] GetRecentEntries(int maxEntries = 0)
    {
        var entries = _recentEntries.ToArray();
        
        if (maxEntries > 0 && entries.Length > maxEntries)
        {
            // Return the most recent entries
            return entries.Skip(entries.Length - maxEntries).ToArray();
        }
        
        return entries;
    }

    /// <summary>
    /// Formats recent log entries as a string suitable for diagnostic reports.
    /// </summary>
    /// <param name="maxEntries">Maximum number of entries to include (0 for all).</param>
    /// <returns>A formatted string containing recent log entries.</returns>
    public string FormatRecentEntries(int maxEntries = 50)
    {
        var entries = GetRecentEntries(maxEntries);
        if (entries.Length == 0)
        {
            return "No recent log entries available.";
        }

        var formatted = new System.Text.StringBuilder();
        formatted.AppendLine($"Recent Log Entries (last {entries.Length} entries):");
        formatted.AppendLine("```");
        
        foreach (var entry in entries)
        {
            string formattedEntry = LogEntryFormatter.FormatLogEntry(entry.Level, entry.Message, entry.Context);
            formatted.AppendLine(formattedEntry);
        }
        
        formatted.AppendLine("```");
        
        return formatted.ToString();
    }

    /// <summary>
    /// Releases all resources used by the DiagnosticLogger.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (_innerLogger is IDisposable disposableLogger)
            {
                disposableLogger.Dispose();
            }
            _disposed = true;
        }
    }
}

/// <summary>
/// Represents a log entry with timestamp and context.
/// </summary>
public class LogEntry
{
    /// <summary>
    /// Gets or sets the timestamp when the log entry was created.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the log message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional context data.
    /// </summary>
    public object? Context { get; set; }
}