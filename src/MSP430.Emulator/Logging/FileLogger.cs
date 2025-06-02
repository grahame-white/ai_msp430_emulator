using System.Text.Json;

namespace MSP430.Emulator.Logging;

/// <summary>
/// A logger implementation that writes log messages to a file.
/// </summary>
public class FileLogger : ILogger, IDisposable
{
    private readonly string _filePath;
    private readonly StreamWriter _writer;
    private readonly object _lock = new object();
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the FileLogger class.
    /// </summary>
    /// <param name="filePath">The path to the log file.</param>
    /// <param name="append">Whether to append to existing file or overwrite.</param>
    public FileLogger(string filePath, bool append = true)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

        // Ensure directory exists
        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(_filePath, append) { AutoFlush = true };
    }

    /// <inheritdoc/>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

    /// <inheritdoc/>
    public void Log(LogLevel level, string message)
    {
        Log(level, message, null);
    }

    /// <inheritdoc/>
    public void Log(LogLevel level, string message, object? context)
    {
        if (!IsEnabled(level) || _disposed)
        {
            return;
        }

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

        lock (_lock)
        {
            if (!_disposed)
            {
                _writer.WriteLine(logEntry);
            }
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

    /// <summary>
    /// Releases all resources used by the FileLogger.
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
            lock (_lock)
            {
                _writer?.Dispose();
                _disposed = true;
            }
        }
    }
}
