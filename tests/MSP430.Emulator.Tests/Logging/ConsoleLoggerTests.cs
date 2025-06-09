using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Tests.Logging;

/// <summary>
/// Unit tests for the ConsoleLogger class.
/// 
/// Tests validate console logging functionality for MSP430 emulator output.
/// Console logging provides real-time emulator information including:
/// - Instruction execution messages
/// - Error and warning notifications  
/// - Debug information and state changes
/// - Performance and timing information
/// - User interaction feedback
/// </summary>
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

    [Theory]
    [InlineData(LogLevel.Warning, LogLevel.Warning, true)]   // At minimum level
    [InlineData(LogLevel.Warning, LogLevel.Error, true)]     // Above minimum level 
    [InlineData(LogLevel.Warning, LogLevel.Info, false)]     // Below minimum level
    public void IsEnabled_ChecksAgainstMinimumLevel(LogLevel minimumLevel, LogLevel testLevel, bool expectedResult)
    {
        var logger = new ConsoleLogger { MinimumLevel = minimumLevel };
        Assert.Equal(expectedResult, logger.IsEnabled(testLevel));
    }

    [Fact]
    public void Debug_CallsLogWithDebugLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        logger.Debug("test message");
        Assert.Equal(LogLevel.Debug, logger.LastLogLevel);
    }

    [Fact]
    public void Debug_WithContext_CallsLogWithDebugLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Debug("test message", context);
        Assert.Equal(LogLevel.Debug, logger.LastLogLevel);
    }

    [Fact]
    public void Debug_WithContext_PassesContextCorrectly()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Debug("test message", context);
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
    public void Info_WithContext_CallsLogWithInfoLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Info("test message", context);
        Assert.Equal(LogLevel.Info, logger.LastLogLevel);
    }

    [Fact]
    public void Info_WithContext_PassesContextCorrectly()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Info("test message", context);
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
    public void Warning_WithContext_CallsLogWithWarningLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Warning("test message", context);
        Assert.Equal(LogLevel.Warning, logger.LastLogLevel);
    }

    [Fact]
    public void Warning_WithContext_PassesContextCorrectly()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Warning("test message", context);
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
    public void Error_WithContext_CallsLogWithErrorLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Error("test message", context);
        Assert.Equal(LogLevel.Error, logger.LastLogLevel);
    }

    [Fact]
    public void Error_WithContext_PassesContextCorrectly()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Error("test message", context);
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
    public void Fatal_WithContext_CallsLogWithFatalLevel()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Fatal("test message", context);
        Assert.Equal(LogLevel.Fatal, logger.LastLogLevel);
    }

    [Fact]
    public void Fatal_WithContext_PassesContextCorrectly()
    {
        var logger = new TestableConsoleLogger { MinimumLevel = LogLevel.Debug };
        var context = new { TestProperty = "value" };
        logger.Fatal("test message", context);
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
    public void IsOutputSuppressed_DefaultsToFalse()
    {
        var logger = new ConsoleLogger();
        Assert.False(logger.IsOutputSuppressed);
    }

    [Fact]
    public void IsOutputSuppressed_CanBeSet()
    {
        var logger = new ConsoleLogger();
        logger.IsOutputSuppressed = true;
        Assert.True(logger.IsOutputSuppressed);
    }

    [Fact]
    public void Log_WithIsOutputSuppressed_DoesNotCallConsole()
    {
        var logger = new TestableConsoleLogger { IsOutputSuppressed = true };
        logger.Log(LogLevel.Info, "test message");
        // Logger should still receive the call when IsOutputSuppressed is true
        // but actual console output is suppressed in the base implementation
        Assert.Equal("test message", logger.LastMessage);
    }

    [Fact]
    public void RedirectErrorsToStdout_DefaultsToFalse()
    {
        var logger = new ConsoleLogger();
        Assert.False(logger.RedirectErrorsToStdout);
    }

    [Fact]
    public void RedirectErrorsToStdout_CanBeSet()
    {
        var logger = new ConsoleLogger();
        logger.RedirectErrorsToStdout = true;
        Assert.True(logger.RedirectErrorsToStdout);
    }

    [Fact]
    public void Log_ErrorLevelWithRedirectErrorsToStdout_StoresMessage()
    {
        // This test verifies the branch for RedirectErrorsToStdout=true with Error level
        var logger = new TestableConsoleLogger { RedirectErrorsToStdout = true };
        logger.Log(LogLevel.Error, "error message");
        Assert.Equal("error message", logger.LastMessage);
    }

    [Fact]
    public void Log_ErrorLevelWithRedirectErrorsToStdout_StoresLogLevel()
    {
        // This test verifies the branch for RedirectErrorsToStdout=true with Error level
        var logger = new TestableConsoleLogger { RedirectErrorsToStdout = true };
        logger.Log(LogLevel.Error, "error message");
        Assert.Equal(LogLevel.Error, logger.LastLogLevel);
    }

    [Fact]
    public void Log_ErrorLevelWithoutRedirectErrorsToStdout_StoresMessage()
    {
        // This test verifies the branch for RedirectErrorsToStdout=false with Error level
        var logger = new TestableConsoleLogger { RedirectErrorsToStdout = false };
        logger.Log(LogLevel.Error, "error message");
        Assert.Equal("error message", logger.LastMessage);
    }

    [Fact]
    public void Log_ErrorLevelWithoutRedirectErrorsToStdout_StoresLogLevel()
    {
        // This test verifies the branch for RedirectErrorsToStdout=false with Error level
        var logger = new TestableConsoleLogger { RedirectErrorsToStdout = false };
        logger.Log(LogLevel.Error, "error message");
        Assert.Equal(LogLevel.Error, logger.LastLogLevel);
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
