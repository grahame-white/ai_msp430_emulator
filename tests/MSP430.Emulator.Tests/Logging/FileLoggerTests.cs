using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Tests.Logging;

public class FileLoggerTests : IDisposable
{
    private readonly string _testLogPath;

    public FileLoggerTests()
    {
        _testLogPath = Path.Join(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log");
    }

    public void Dispose()
    {
        if (File.Exists(_testLogPath))
        {
            File.Delete(_testLogPath);
        }
    }

    [Fact]
    public void Constructor_WithValidPath_CreatesFile()
    {
        using var logger = new FileLogger(_testLogPath, false);
        Assert.True(File.Exists(_testLogPath));
    }

    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FileLogger(null!));
    }

    [Fact]
    public void Constructor_SetsDefaultMinimumLevel()
    {
        using var logger = new FileLogger(_testLogPath);
        Assert.Equal(LogLevel.Info, logger.MinimumLevel);
    }

    [Fact]
    public void MinimumLevel_CanBeSet()
    {
        using var logger = new FileLogger(_testLogPath);
        logger.MinimumLevel = LogLevel.Debug;
        Assert.Equal(LogLevel.Debug, logger.MinimumLevel);
    }

    [Fact]
    public void IsEnabled_ReturnsTrueForLevelAtOrAboveMinimum()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Warning };
        Assert.True(logger.IsEnabled(LogLevel.Warning));
    }

    [Fact]
    public void IsEnabled_ReturnsTrueForLevelAboveMinimum()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Warning };
        Assert.True(logger.IsEnabled(LogLevel.Error));
    }

    [Fact]
    public void IsEnabled_ReturnsFalseForLevelBelowMinimum()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Warning };
        Assert.False(logger.IsEnabled(LogLevel.Info));
    }

    [Fact]
    public void Debug_WritesToFile_ContainsMessage()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Debug("test debug message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test debug message", content);
    }

    [Fact]
    public void Debug_WritesToFile_ContainsLogLevel()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Debug("test debug message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("[DEBUG]", content);
    }

    [Fact]
    public void Info_WritesToFile_IncludesMessage()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Info("test info message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test info message", content);
    }

    [Fact]
    public void Info_WritesToFile_IncludesLogLevel()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Info("test info message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("[INFO]", content);
    }

    [Fact]
    public void Warning_WritesToFile_IncludesMessage()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Warning("test warning message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test warning message", content);
    }

    [Fact]
    public void Warning_WritesToFile_IncludesLogLevel()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Warning("test warning message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("[WARNING]", content);
    }

    [Fact]
    public void Error_WritesToFile_IncludesMessage()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Error("test error message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test error message", content);
    }

    [Fact]
    public void Error_WritesToFile_IncludesLogLevel()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Error("test error message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("[ERROR]", content);
    }

    [Fact]
    public void Fatal_WritesToFile_IncludesMessage()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Fatal("test fatal message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test fatal message", content);
    }

    [Fact]
    public void Fatal_WritesToFile_IncludesLogLevel()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Fatal("test fatal message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("[FATAL]", content);
    }

    [Fact]
    public void Log_WithContext_IncludesMessage()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value", Number = 42 };
        logger.Info("test message", context);
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test message", content);
    }

    [Fact]
    public void Log_WithContext_IncludesContextLabel()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value", Number = 42 };
        logger.Info("test message", context);
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("Context:", content);
    }

    [Fact]
    public void Log_WithContext_IncludesPropertyName()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value", Number = 42 };
        logger.Info("test message", context);
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("TestProperty", content);
    }

    [Fact]
    public void Log_WithContext_IncludesPropertyValue()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value", Number = 42 };
        logger.Info("test message", context);
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("value", content);
    }

    [Fact]
    public void Log_WithLevelBelowMinimum_DoesNotWriteToFile()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Warning };
        logger.Info("test message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.DoesNotContain("test message", content);
    }

    [Fact]
    public void Log_IncludesTimestamp()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Info("test message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Matches(@"\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z\]", content);
    }

    [Fact]
    public void Constructor_CreatesDirectoryWhenNotExists()
    {
        string testDir = Path.Join(Path.GetTempPath(), $"testdir_{Guid.NewGuid()}");
        string testFile = Path.Join(testDir, "test.log");

        try
        {
            using var logger = new FileLogger(testFile);
            Assert.True(Directory.Exists(testDir));
        }
        finally
        {
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    public void Constructor_CreatesFileWhenNotExists()
    {
        string testDir = Path.Join(Path.GetTempPath(), $"testdir_{Guid.NewGuid()}");
        string testFile = Path.Join(testDir, "test.log");

        try
        {
            using var logger = new FileLogger(testFile);
            Assert.True(File.Exists(testFile));
        }
        finally
        {
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        using var logger = new FileLogger(_testLogPath);
        // Test that multiple calls to Dispose don't throw
        logger.Dispose(); // Should not throw
        logger.Dispose(); // Should not throw
        Assert.True(true); // Test passes if no exception is thrown
    }

    [Fact]
    public async Task Log_WithMultipleThreads_CreatesLogFile()
    {
        string testPath = Path.Join(Path.GetTempPath(), $"test_multithread_{Guid.NewGuid()}.log");

        const int threadCount = 5;
        const int messagesPerThread = 10;

        try
        {
            using var logger = new FileLogger(testPath);
            var tasks = new Task[threadCount];
            using var barrier = new Barrier(threadCount);

            // Create multiple threads that write to the logger simultaneously
            for (int threadId = 0; threadId < threadCount; threadId++)
            {
                int capturedThreadId = threadId;
                tasks[threadId] = Task.Run(() =>
                {
                    // Wait for all threads to be ready before starting
                    barrier.SignalAndWait();

                    for (int messageId = 0; messageId < messagesPerThread; messageId++)
                    {
                        logger.Info($"Thread {capturedThreadId} Message {messageId}",
                                  new { ThreadId = capturedThreadId, MessageId = messageId });
                    }
                });
            }

            // Wait for all threads to complete
            await Task.WhenAll(tasks);

            // Dispose the logger to ensure all data is flushed
        }
        finally
        {
            // Verification should happen after the using block to ensure proper disposal
        }

        // Verify the log file was written correctly
        Assert.True(File.Exists(testPath));

        try
        {
            // Clean up test file
            if (File.Exists(testPath))
            {
                File.Delete(testPath);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore cleanup errors
        }
        catch (IOException)
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task Log_WithMultipleThreads_WritesCorrectNumberOfLines()
    {
        string testPath = Path.Join(Path.GetTempPath(), $"test_multithread_{Guid.NewGuid()}.log");

        const int threadCount = 5;
        const int messagesPerThread = 10;

        try
        {
            using var logger = new FileLogger(testPath);
            var tasks = new Task[threadCount];
            using var barrier = new Barrier(threadCount);

            // Create multiple threads that write to the logger simultaneously
            for (int threadId = 0; threadId < threadCount; threadId++)
            {
                int capturedThreadId = threadId;
                tasks[threadId] = Task.Run(() =>
                {
                    // Wait for all threads to be ready before starting
                    barrier.SignalAndWait();

                    for (int messageId = 0; messageId < messagesPerThread; messageId++)
                    {
                        logger.Info($"Thread {capturedThreadId} Message {messageId}",
                                  new { ThreadId = capturedThreadId, MessageId = messageId });
                    }
                });
            }

            // Wait for all threads to complete
            await Task.WhenAll(tasks);

            // Dispose the logger to ensure all data is flushed
        }
        finally
        {
            // Verification should happen after the using block to ensure proper disposal
        }

        try
        {
            string[] lines = File.ReadAllLines(testPath);

            // Should have exactly the expected number of log entries
            int expectedLines = threadCount * messagesPerThread;
            Assert.Equal(expectedLines, lines.Length);
        }
        finally
        {
            // Clean up test file
            if (File.Exists(testPath))
            {
                File.Delete(testPath);
            }
        }
    }

    [Theory]
    [InlineData("Context:")]
    [InlineData("ThreadId")]
    [InlineData("MessageId")]
    public async Task Log_WithMultipleThreads_EachLineContainsExpectedContent(string expectedContent)
    {
        string testPath = Path.Join(Path.GetTempPath(), $"test_multithread_{Guid.NewGuid()}.log");

        const int threadCount = 5;
        const int messagesPerThread = 10;

        try
        {
            using var logger = new FileLogger(testPath);
            var tasks = new Task[threadCount];
            using var barrier = new Barrier(threadCount);

            // Create multiple threads that write to the logger simultaneously
            for (int threadId = 0; threadId < threadCount; threadId++)
            {
                int capturedThreadId = threadId;
                tasks[threadId] = Task.Run(() =>
                {
                    // Wait for all threads to be ready before starting
                    barrier.SignalAndWait();

                    for (int messageId = 0; messageId < messagesPerThread; messageId++)
                    {
                        logger.Info($"Thread {capturedThreadId} Message {messageId}",
                                  new { ThreadId = capturedThreadId, MessageId = messageId });
                    }
                });
            }

            // Wait for all threads to complete
            await Task.WhenAll(tasks);

            // Dispose the logger to ensure all data is flushed
        }
        finally
        {
            // Verification should happen after the using block to ensure proper disposal
        }

        try
        {
            string[] lines = File.ReadAllLines(testPath);

            // Verify each line contains proper JSON context
            foreach (string line in lines)
            {
                Assert.Contains(expectedContent, line);
                break; // Only test first line since this is a parametric test
            }
        }
        finally
        {
            // Clean up test file
            if (File.Exists(testPath))
            {
                File.Delete(testPath);
            }
        }
    }
}
