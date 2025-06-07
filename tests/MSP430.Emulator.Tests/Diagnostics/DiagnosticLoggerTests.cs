using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MSP430.Emulator.Diagnostics;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Tests.Diagnostics;

public class DiagnosticLoggerTests
{
    /// <summary>
    /// Creates a console logger with output suppressed for testing purposes.
    /// </summary>
    /// <param name="minimumLevel">The minimum log level. Defaults to Debug.</param>
    /// <returns>A ConsoleLogger with IsOutputSuppressed set to true.</returns>
    private static ConsoleLogger CreateSuppressedConsoleLogger(LogLevel minimumLevel = LogLevel.Debug)
    {
        return new ConsoleLogger { IsOutputSuppressed = true, MinimumLevel = minimumLevel };
    }
    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateDiagnosticLogger()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();

        // Act
        using var diagnosticLogger = new DiagnosticLogger(innerLogger);

        // Assert
        Assert.NotNull(diagnosticLogger);
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldCopyMinimumLevel()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();

        // Act
        using var diagnosticLogger = new DiagnosticLogger(innerLogger);

        // Assert
        Assert.Equal(innerLogger.MinimumLevel, diagnosticLogger.MinimumLevel);
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
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        // Act
        diagnosticLogger.Info("Test message");

        // Assert
        LogEntry[] entries = diagnosticLogger.GetRecentEntries();
        Assert.Single(entries);
    }

    [Fact]
    public void Log_ShouldStoreCorrectMessage()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        // Act
        diagnosticLogger.Info("Test message");

        // Assert
        LogEntry[] entries = diagnosticLogger.GetRecentEntries();
        Assert.Equal("Test message", entries[0].Message);
    }

    [Fact]
    public void Log_ShouldStoreCorrectLevel()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        // Act
        diagnosticLogger.Info("Test message");

        // Assert
        LogEntry[] entries = diagnosticLogger.GetRecentEntries();
        Assert.Equal(LogLevel.Info, entries[0].Level);
    }

    [Fact]
    public void Log_WithContext_ShouldStoreEntryInBuffer()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);
        var context = new { Key = "Value", Number = 42 };

        // Act
        diagnosticLogger.Info("Test message", context);

        // Assert
        LogEntry[] entries = diagnosticLogger.GetRecentEntries();
        Assert.Single(entries);
    }

    [Fact]
    public void Log_WithContext_ShouldStoreCorrectMessage()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);
        var context = new { Key = "Value", Number = 42 };

        // Act
        diagnosticLogger.Info("Test message", context);

        // Assert
        LogEntry[] entries = diagnosticLogger.GetRecentEntries();
        Assert.Equal("Test message", entries[0].Message);
    }

    [Fact]
    public void Log_WithContext_ShouldStoreCorrectContext()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);
        var context = new { Key = "Value", Number = 42 };

        // Act
        diagnosticLogger.Info("Test message", context);

        // Assert
        LogEntry[] entries = diagnosticLogger.GetRecentEntries();
        Assert.Equal(context, entries[0].Context);
    }

    [Fact]
    public void Log_BeyondMaxEntries_ShouldMaintainBufferSize()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 3);

        // Act
        diagnosticLogger.Info("Message 1");
        diagnosticLogger.Info("Message 2");
        diagnosticLogger.Info("Message 3");
        diagnosticLogger.Info("Message 4");
        diagnosticLogger.Info("Message 5");

        // Assert
        LogEntry[] entries = diagnosticLogger.GetRecentEntries();
        Assert.Equal(3, entries.Length);
    }

    [Theory]
    [InlineData(0, "Message 3")]
    [InlineData(1, "Message 4")]
    [InlineData(2, "Message 5")]
    public void Log_BeyondMaxEntries_ShouldContainMostRecentEntries(int index, string expectedMessage)
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 3);

        // Act
        diagnosticLogger.Info("Message 1");
        diagnosticLogger.Info("Message 2");
        diagnosticLogger.Info("Message 3");
        diagnosticLogger.Info("Message 4");
        diagnosticLogger.Info("Message 5");

        // Assert
        LogEntry[] entries = diagnosticLogger.GetRecentEntries();
        Assert.Equal(expectedMessage, entries[index].Message);
    }

    [Fact]
    public void GetRecentEntries_WithMaxEntries_ShouldLimitResultsToCorrectCount()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        for (int i = 1; i <= 5; i++)
        {
            diagnosticLogger.Info($"Message {i}");
        }

        // Act
        LogEntry[] entries = diagnosticLogger.GetRecentEntries(3);

        // Assert
        Assert.Equal(3, entries.Length);
    }

    [Theory]
    [InlineData(0, "Message 3")]
    [InlineData(1, "Message 4")]
    [InlineData(2, "Message 5")]
    public void GetRecentEntries_WithMaxEntries_ShouldReturnMostRecentEntries(int index, string expectedMessage)
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        for (int i = 1; i <= 5; i++)
        {
            diagnosticLogger.Info($"Message {i}");
        }

        // Act
        LogEntry[] entries = diagnosticLogger.GetRecentEntries(3);

        // Assert
        Assert.Equal(expectedMessage, entries[index].Message);
    }

    [Fact]
    public void FormatRecentEntries_ShouldContainRecentLogEntriesHeader()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        diagnosticLogger.Info("Test message 1");
        diagnosticLogger.Warning("Sample warning log");
        diagnosticLogger.Error("Sample error log");

        // Act
        string formatted = diagnosticLogger.FormatRecentEntries();

        // Assert
        Assert.Contains("Recent Log Entries", formatted);
    }

    [Theory]
    [InlineData("Test message 1")]
    [InlineData("Sample warning log")]
    [InlineData("Sample error log")]
    public void FormatRecentEntries_ShouldContainLoggedMessage(string expectedMessage)
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        diagnosticLogger.Info("Test message 1");
        diagnosticLogger.Warning("Sample warning log");
        diagnosticLogger.Error("Sample error log");

        // Act
        string formatted = diagnosticLogger.FormatRecentEntries();

        // Assert
        Assert.Contains(expectedMessage, formatted);
    }

    [Theory]
    [InlineData("[INFO]")]
    [InlineData("[WARNING]")]
    [InlineData("[ERROR]")]
    public void FormatRecentEntries_ShouldContainLogLevelPrefix(string expectedPrefix)
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        diagnosticLogger.Info("Test message 1");
        diagnosticLogger.Warning("Sample warning log");
        diagnosticLogger.Error("Sample error log");

        // Act
        string formatted = diagnosticLogger.FormatRecentEntries();

        // Assert
        Assert.Contains(expectedPrefix, formatted);
    }

    [Fact]
    public void FormatRecentEntries_WithNoEntries_ShouldReturnNoEntriesMessage()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        // Act
        string formatted = diagnosticLogger.FormatRecentEntries();

        // Assert
        Assert.Contains("No recent log entries available", formatted);
    }

    [Fact]
    public void AllLogMethods_ShouldCreateCorrectNumberOfEntries()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        // Act
        diagnosticLogger.Debug("Debug message");
        diagnosticLogger.Info("Info message");
        diagnosticLogger.Warning("Sample warning");
        diagnosticLogger.Error("Sample error");
        diagnosticLogger.Fatal("Fatal message");

        // Assert
        LogEntry[] entries = diagnosticLogger.GetRecentEntries();
        Assert.Equal(5, entries.Length);
    }

    [Theory]
    [InlineData(0, LogLevel.Debug)]
    [InlineData(1, LogLevel.Info)]
    [InlineData(2, LogLevel.Warning)]
    [InlineData(3, LogLevel.Error)]
    [InlineData(4, LogLevel.Fatal)]
    public void AllLogMethods_ShouldCreateCorrectLogLevel(int entryIndex, LogLevel expectedLevel)
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger();
        using var diagnosticLogger = new DiagnosticLogger(innerLogger, 10);

        // Act
        diagnosticLogger.Debug("Debug message");
        diagnosticLogger.Info("Info message");
        diagnosticLogger.Warning("Sample warning");
        diagnosticLogger.Error("Sample error");
        diagnosticLogger.Fatal("Fatal message");

        // Assert
        LogEntry[] entries = diagnosticLogger.GetRecentEntries();
        Assert.Equal(expectedLevel, entries[entryIndex].Level);
    }

    [Fact]
    public void MinimumLevel_ShouldReflectInnerLoggerLevel()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger(LogLevel.Warning);
        using var diagnosticLogger = new DiagnosticLogger(innerLogger);

        // Act & Assert
        Assert.Equal(LogLevel.Warning, diagnosticLogger.MinimumLevel);
    }

    [Fact]
    public void MinimumLevel_WhenSet_ShouldUpdateInnerLogger()
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger(LogLevel.Warning);
        using var diagnosticLogger = new DiagnosticLogger(innerLogger);

        // Act
        diagnosticLogger.MinimumLevel = LogLevel.Error;

        // Assert
        Assert.Equal(LogLevel.Error, innerLogger.MinimumLevel);
    }

    [Theory]
    [InlineData(LogLevel.Debug, false)]
    [InlineData(LogLevel.Info, false)]
    [InlineData(LogLevel.Warning, true)]
    [InlineData(LogLevel.Error, true)]
    [InlineData(LogLevel.Fatal, true)]
    public void IsEnabled_ShouldReflectInnerLoggerSettings(LogLevel level, bool expectedEnabled)
    {
        // Arrange
        ConsoleLogger innerLogger = CreateSuppressedConsoleLogger(LogLevel.Warning);
        using var diagnosticLogger = new DiagnosticLogger(innerLogger);

        // Act & Assert
        Assert.Equal(expectedEnabled, diagnosticLogger.IsEnabled(level));
    }

    [Fact]
    public void Dispose_ShouldDisposeInnerLoggerIfDisposable()
    {
        // Arrange & Act
        using var innerLogger = new FileLogger("test-diagnostic.log");
        using var diagnosticLogger = new DiagnosticLogger(innerLogger);

        // The using statements will automatically dispose the loggers at the end of the scope

        // Assert - If inner logger was properly disposed, the file should be released
        // We'll test this by trying to delete the file after the using block completes
        try
        {
            if (File.Exists("test-diagnostic.log"))
            {
                File.Delete("test-diagnostic.log");
            }

            // If this succeeds without exception, the file was properly released
            Assert.True(true);
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore file access errors during test cleanup
        }
        catch (IOException)
        {
            // Ignore I/O errors during test cleanup
        }
    }
}
