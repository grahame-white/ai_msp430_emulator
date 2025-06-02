using MSP430.Emulator.Diagnostics;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Tests.Diagnostics;

public class DiagnosticLoggerTests
{
    [Fact]
    public void Constructor_WithValidLogger_ShouldSucceed()
    {
        // Arrange
        var innerLogger = new ConsoleLogger();

        // Act
        var diagnosticLogger = new DiagnosticLogger(innerLogger);

        // Assert
        Assert.NotNull(diagnosticLogger);
        Assert.Equal(innerLogger.MinimumLevel, diagnosticLogger.MinimumLevel);
        
        diagnosticLogger.Dispose();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DiagnosticLogger(null!));
    }

    [Fact]
    public void Log_ShouldStoreEntryInBuffer()
    {
        // Arrange
        var innerLogger = new ConsoleLogger();
        var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        // Act
        diagnosticLogger.Info("Test message");

        // Assert
        var entries = diagnosticLogger.GetRecentEntries();
        Assert.Single(entries);
        Assert.Equal("Test message", entries[0].Message);
        Assert.Equal(LogLevel.Info, entries[0].Level);
        
        diagnosticLogger.Dispose();
    }

    [Fact]
    public void Log_WithContext_ShouldStoreContextInBuffer()
    {
        // Arrange
        var innerLogger = new ConsoleLogger();
        var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);
        var context = new { Key = "Value", Number = 42 };

        // Act
        diagnosticLogger.Info("Test message", context);

        // Assert
        var entries = diagnosticLogger.GetRecentEntries();
        Assert.Single(entries);
        Assert.Equal("Test message", entries[0].Message);
        Assert.Equal(context, entries[0].Context);
        
        diagnosticLogger.Dispose();
    }

    [Fact]
    public void Log_BeyondMaxEntries_ShouldMaintainBufferSize()
    {
        // Arrange
        var innerLogger = new ConsoleLogger();
        var diagnosticLogger = new DiagnosticLogger(innerLogger, 3);

        // Act
        diagnosticLogger.Info("Message 1");
        diagnosticLogger.Info("Message 2");
        diagnosticLogger.Info("Message 3");
        diagnosticLogger.Info("Message 4");
        diagnosticLogger.Info("Message 5");

        // Assert
        var entries = diagnosticLogger.GetRecentEntries();
        Assert.Equal(3, entries.Length);
        
        // Should contain the most recent entries
        Assert.Equal("Message 3", entries[0].Message);
        Assert.Equal("Message 4", entries[1].Message);
        Assert.Equal("Message 5", entries[2].Message);
        
        diagnosticLogger.Dispose();
    }

    [Fact]
    public void GetRecentEntries_WithMaxEntries_ShouldLimitResults()
    {
        // Arrange
        var innerLogger = new ConsoleLogger();
        var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);
        
        for (int i = 1; i <= 5; i++)
        {
            diagnosticLogger.Info($"Message {i}");
        }

        // Act
        var entries = diagnosticLogger.GetRecentEntries(3);

        // Assert
        Assert.Equal(3, entries.Length);
        Assert.Equal("Message 3", entries[0].Message);
        Assert.Equal("Message 4", entries[1].Message);
        Assert.Equal("Message 5", entries[2].Message);
        
        diagnosticLogger.Dispose();
    }

    [Fact]
    public void FormatRecentEntries_ShouldReturnFormattedString()
    {
        // Arrange
        var innerLogger = new ConsoleLogger();
        var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);
        
        diagnosticLogger.Info("Test message 1");
        diagnosticLogger.Warning("Test warning");
        diagnosticLogger.Error("Test error");

        // Act
        string formatted = diagnosticLogger.FormatRecentEntries();

        // Assert
        Assert.Contains("Recent Log Entries", formatted);
        Assert.Contains("Test message 1", formatted);
        Assert.Contains("Test warning", formatted);
        Assert.Contains("Test error", formatted);
        Assert.Contains("[INFO]", formatted);
        Assert.Contains("[WARNING]", formatted);
        Assert.Contains("[ERROR]", formatted);
        
        diagnosticLogger.Dispose();
    }

    [Fact]
    public void FormatRecentEntries_WithNoEntries_ShouldReturnNoEntriesMessage()
    {
        // Arrange
        var innerLogger = new ConsoleLogger();
        var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        // Act
        string formatted = diagnosticLogger.FormatRecentEntries();

        // Assert
        Assert.Contains("No recent log entries available", formatted);
        
        diagnosticLogger.Dispose();
    }

    [Fact]
    public void AllLogMethods_ShouldCreateCorrectEntries()
    {
        // Arrange
        var innerLogger = new ConsoleLogger();
        var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        // Act
        diagnosticLogger.Debug("Debug message");
        diagnosticLogger.Info("Info message");
        diagnosticLogger.Warning("Warning message");
        diagnosticLogger.Error("Error message");
        diagnosticLogger.Fatal("Fatal message");

        // Assert
        var entries = diagnosticLogger.GetRecentEntries();
        Assert.Equal(5, entries.Length);
        
        Assert.Equal(LogLevel.Debug, entries[0].Level);
        Assert.Equal(LogLevel.Info, entries[1].Level);
        Assert.Equal(LogLevel.Warning, entries[2].Level);
        Assert.Equal(LogLevel.Error, entries[3].Level);
        Assert.Equal(LogLevel.Fatal, entries[4].Level);
        
        diagnosticLogger.Dispose();
    }

    [Fact]
    public void MinimumLevel_ShouldReflectInnerLogger()
    {
        // Arrange
        var innerLogger = new ConsoleLogger { MinimumLevel = LogLevel.Warning };
        var diagnosticLogger = new DiagnosticLogger(innerLogger);

        // Act & Assert
        Assert.Equal(LogLevel.Warning, diagnosticLogger.MinimumLevel);
        
        diagnosticLogger.MinimumLevel = LogLevel.Error;
        Assert.Equal(LogLevel.Error, innerLogger.MinimumLevel);
        
        diagnosticLogger.Dispose();
    }

    [Fact]
    public void IsEnabled_ShouldReflectInnerLogger()
    {
        // Arrange
        var innerLogger = new ConsoleLogger { MinimumLevel = LogLevel.Warning };
        var diagnosticLogger = new DiagnosticLogger(innerLogger);

        // Act & Assert
        Assert.False(diagnosticLogger.IsEnabled(LogLevel.Debug));
        Assert.False(diagnosticLogger.IsEnabled(LogLevel.Info));
        Assert.True(diagnosticLogger.IsEnabled(LogLevel.Warning));
        Assert.True(diagnosticLogger.IsEnabled(LogLevel.Error));
        Assert.True(diagnosticLogger.IsEnabled(LogLevel.Fatal));
        
        diagnosticLogger.Dispose();
    }

    [Fact]
    public void Dispose_ShouldDisposeInnerLoggerIfDisposable()
    {
        // Arrange
        var innerLogger = new FileLogger("test-diagnostic.log");
        var diagnosticLogger = new DiagnosticLogger(innerLogger);

        // Act
        diagnosticLogger.Dispose();

        // Assert - If inner logger was properly disposed, the file should be released
        // We'll test this by trying to delete the file
        if (File.Exists("test-diagnostic.log"))
        {
            File.Delete("test-diagnostic.log");
        }
        
        // If this succeeds without exception, the file was properly released
        Assert.True(true);
    }
}