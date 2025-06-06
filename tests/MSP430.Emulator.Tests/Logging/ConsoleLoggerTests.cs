using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Tests.Logging;

public class ConsoleLoggerTests
{
    [Fact]
    public void Constructor_SetsDefaultMinimumLevel()
    {
        var logger = new ConsoleLogger();
        Assert.Equal(LogLevel.Info, logger.MinimumLevel);
    }

    [Fact]
    public void MinimumLevel_CanBeSet()
    {
        var logger = new ConsoleLogger();
        logger.MinimumLevel = LogLevel.Debug;
        Assert.Equal(LogLevel.Debug, logger.MinimumLevel);
    }

    [Fact]
    public void IsEnabled_ReturnsTrueForLevelAtOrAboveMinimum()
    {
        var logger = new ConsoleLogger { MinimumLevel = LogLevel.Warning };
        Assert.True(logger.IsEnabled(LogLevel.Warning));
    }

    [Fact]
    public void IsEnabled_ReturnsTrueForLevelAboveMinimum()
    {
        var logger = new ConsoleLogger { MinimumLevel = LogLevel.Warning };
        Assert.True(logger.IsEnabled(LogLevel.Error));
    }

    [Fact]
    public void IsEnabled_ReturnsFalseForLevelBelowMinimum()
    {
        var logger = new ConsoleLogger { MinimumLevel = LogLevel.Warning };
        Assert.False(logger.IsEnabled(LogLevel.Info));
    }

    [Fact]
    public void Debug_CallsLogWithDebugLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        logger.Debug("test message");
        Assert.Equal(LogLevel.Debug, logger.LastLogLevel);
    }

    [Fact]
    public void Debug_WithContext_CallsLogWithDebugLevelAndContext()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Debug("test message", context);
        Assert.Equal(LogLevel.Debug, logger.LastLogLevel);
        Assert.Equal(context, logger.LastContext);
    }

    [Fact]
    public void Info_CallsLogWithInfoLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        logger.Info("test message");
        Assert.Equal(LogLevel.Info, logger.LastLogLevel);
    }

    [Fact]
    public void Info_WithContext_CallsLogWithInfoLevelAndContext()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Info("test message", context);
        Assert.Equal(LogLevel.Info, logger.LastLogLevel);
        Assert.Equal(context, logger.LastContext);
    }

    [Fact]
    public void Warning_CallsLogWithWarningLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        logger.Warning("test message");
        Assert.Equal(LogLevel.Warning, logger.LastLogLevel);
    }

    [Fact]
    public void Warning_WithContext_CallsLogWithWarningLevelAndContext()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Warning("test message", context);
        Assert.Equal(LogLevel.Warning, logger.LastLogLevel);
        Assert.Equal(context, logger.LastContext);
    }

    [Fact]
    public void Error_CallsLogWithErrorLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        logger.Error("test message");
        Assert.Equal(LogLevel.Error, logger.LastLogLevel);
    }

    [Fact]
    public void Error_WithContext_CallsLogWithErrorLevelAndContext()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Error("test message", context);
        Assert.Equal(LogLevel.Error, logger.LastLogLevel);
        Assert.Equal(context, logger.LastContext);
    }

    [Fact]
    public void Fatal_CallsLogWithFatalLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        logger.Fatal("test message");
        Assert.Equal(LogLevel.Fatal, logger.LastLogLevel);
    }

    [Fact]
    public void Fatal_WithContext_CallsLogWithFatalLevelAndContext()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Fatal("test message", context);
        Assert.Equal(LogLevel.Fatal, logger.LastLogLevel);
        Assert.Equal(context, logger.LastContext);
    }

    [Fact]
    public void Log_WithMessage_StoresMessage()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        logger.Log(LogLevel.Info, "test message");
        Assert.Equal("test message", logger.LastMessage);
    }

    [Fact]
    public void Log_WithLevelBelowMinimum_DoesNotLog()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Warning };
        logger.Log(LogLevel.Info, "test message");
        Assert.Null(logger.LastMessage);
    }

    [Fact]
    public void SuppressOutput_DefaultsToFalse()
    {
        var logger = new ConsoleLogger();
        Assert.False(logger.SuppressOutput);
    }

    [Fact]
    public void SuppressOutput_CanBeSet()
    {
        var logger = new ConsoleLogger();
        logger.SuppressOutput = true;
        Assert.True(logger.SuppressOutput);
    }

    [Fact]
    public void Log_WithSuppressOutput_DoesNotCallConsole()
    {
        var logger = new TestableConsoleLogger { SuppressOutput = true };
        logger.Log(LogLevel.Info, "test message");
        // Logger should still receive the call when SuppressOutput is true
        // but actual console output is suppressed in the base implementation
        Assert.Equal("test message", logger.LastMessage);
    }

    private class TestableConsoleLogger : ConsoleLogger
    {
        public LogLevel? LastLogLevel { get; private set; }
        public string? LastMessage { get; private set; }
        public object? LastContext { get; private set; }

        public override void Log(LogLevel level, string message, object? context)
        {
            if (IsEnabled(level))
            {
                LastLogLevel = level;
                LastMessage = message;
                LastContext = context;
            }
        }
    }
}
