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
    public void Debug_WritesToFile()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Debug("test debug message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test debug message", content);
        Assert.Contains("[DEBUG]", content);
    }

    [Fact]
    public void Info_WritesToFile()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Info("test info message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test info message", content);
        Assert.Contains("[INFO]", content);
    }

    [Fact]
    public void Warning_WritesToFile()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Warning("test warning message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test warning message", content);
        Assert.Contains("[WARNING]", content);
    }

    [Fact]
    public void Error_WritesToFile()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Error("test error message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test error message", content);
        Assert.Contains("[ERROR]", content);
    }

    [Fact]
    public void Fatal_WritesToFile()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        logger.Fatal("test fatal message");
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test fatal message", content);
        Assert.Contains("[FATAL]", content);
    }

    [Fact]
    public void Log_WithContext_IncludesContextInOutput()
    {
        using var logger = new FileLogger(_testLogPath) { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value", Number = 42 };
        logger.Info("test message", context);
        logger.Dispose();

        string content = File.ReadAllText(_testLogPath);
        Assert.Contains("test message", content);
        Assert.Contains("Context:", content);
        Assert.Contains("TestProperty", content);
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
    public void Constructor_CreatesDirectoryIfNotExists()
    {
        string testDir = Path.Join(Path.GetTempPath(), $"testdir_{Guid.NewGuid()}");
        string testFile = Path.Join(testDir, "test.log");

        try
        {
            using var logger = new FileLogger(testFile);
            Assert.True(Directory.Exists(testDir));
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
        var logger = new FileLogger(_testLogPath);
        logger.Dispose();
        logger.Dispose(); // Should not throw
        Assert.True(true); // Test passes if no exception is thrown
    }
}
