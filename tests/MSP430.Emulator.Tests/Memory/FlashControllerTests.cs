using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Memory;

public class FlashControllerTests
{
    private readonly TestLogger _logger;

    public FlashControllerTests()
    {
        _logger = new TestLogger();
    }

    [Theory]
    [InlineData("State", FlashControllerState.Locked)]
    [InlineData("ProtectionLevel", FlashProtectionLevel.None)]
    [InlineData("CurrentOperation", FlashOperation.None)]
    [InlineData("IsOperationInProgress", false)]
    public void Constructor_SetsProperty(string propertyName, object expectedValue)
    {
        var controller = new FlashController(_logger);

        object? propertyValue = typeof(FlashController).GetProperty(propertyName)?.GetValue(controller);
        Assert.Equal(expectedValue, propertyValue);
    }

    [Theory]
    [InlineData(0u)]
    public void Constructor_SetsOperationCyclesRemaining(uint expectedCycles)
    {
        var controller = new FlashController(_logger);

        Assert.Equal(expectedCycles, controller.OperationCyclesRemaining);
    }

    [Fact]
    public void Constructor_NullLogger_InitializesSuccessfully()
    {
        var controller = new FlashController();

        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Theory]
    [InlineData(0xA500)]
    [InlineData(0xA555)]
    [InlineData(0xA5FF)]
    public void TryUnlock_ValidKey_ReturnsTrue(ushort validKey)
    {
        var controller = new FlashController(_logger);

        bool result = controller.TryUnlock(validKey);

        Assert.True(result);
    }

    [Theory]
    [InlineData(0xA500)]
    [InlineData(0xA555)]
    [InlineData(0xA5FF)]
    public void TryUnlock_ValidKey_UnlocksController(ushort validKey)
    {
        var controller = new FlashController(_logger);

        controller.TryUnlock(validKey);

        Assert.Equal(FlashControllerState.Unlocked, controller.State);
    }

    [Theory]
    [InlineData(0x1234)] // Wrong prefix
    [InlineData(0x5555)] // Wrong prefix
    [InlineData(0x0000)] // Wrong prefix
    [InlineData(0xFFFF)] // Wrong prefix
    public void TryUnlock_InvalidKey_ReturnsFalse(ushort invalidKey)
    {
        var controller = new FlashController(_logger);

        bool result = controller.TryUnlock(invalidKey);

        Assert.False(result);
    }

    [Theory]
    [InlineData(0x1234)] // Wrong prefix
    [InlineData(0x5555)] // Wrong prefix
    [InlineData(0x0000)] // Wrong prefix
    [InlineData(0xFFFF)] // Wrong prefix
    public void TryUnlock_InvalidKey_KeepsControllerLocked(ushort invalidKey)
    {
        var controller = new FlashController(_logger);

        controller.TryUnlock(invalidKey);

        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Fact]
    public void TryUnlock_WhenAlreadyUnlocked_ReturnsFalse()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.TryUnlock(0xA555);

        Assert.False(result);
    }

    [Fact]
    public void TryUnlock_WhenAlreadyUnlocked_KeepsControllerUnlocked()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.TryUnlock(0xA555);

        Assert.Equal(FlashControllerState.Unlocked, controller.State);
    }

    [Theory]
    [InlineData(FlashProtectionLevel.SecurityLocked)]
    [InlineData(FlashProtectionLevel.PermanentlyLocked)]
    public void TryUnlock_WhenProtected_ReturnsFalse(FlashProtectionLevel protectionLevel)
    {
        var controller = new FlashController(_logger);
        controller.SetProtectionLevel(protectionLevel);

        bool result = controller.TryUnlock(0xA555);

        Assert.False(result);
    }

    [Theory]
    [InlineData(FlashProtectionLevel.SecurityLocked)]
    [InlineData(FlashProtectionLevel.PermanentlyLocked)]
    public void TryUnlock_WhenProtected_KeepsControllerLocked(FlashProtectionLevel protectionLevel)
    {
        var controller = new FlashController(_logger);
        controller.SetProtectionLevel(protectionLevel);

        controller.TryUnlock(0xA555);

        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Fact]
    public void Lock_FromUnlockedState_ChangesToLocked()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.Lock();

        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Fact]
    public void Lock_FromUnlockedState_SetsCurrentOperationToNone()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.Lock();

        Assert.Equal(FlashOperation.None, controller.CurrentOperation);
    }

    [Fact]
    public void Lock_WhenOperationInProgress_DoesNotLock()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);

        controller.Lock();

        Assert.Equal(FlashControllerState.Programming, controller.State);
    }

    [Theory]
    [InlineData("ReturnsTrue", true)]
    [InlineData("State", FlashControllerState.Programming)]
    [InlineData("CurrentOperation", FlashOperation.Program)]
    [InlineData("IsOperationInProgress", true)]
    public void StartProgramming_ByteOperation_SetsProperty(string testCase, object expectedValue)
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.StartProgramming(false);

        switch (testCase)
        {
            case "ReturnsTrue":
                Assert.Equal(expectedValue, result);
                break;
            case "State":
                Assert.Equal(expectedValue, controller.State);
                break;
            case "CurrentOperation":
                Assert.Equal(expectedValue, controller.CurrentOperation);
                break;
            case "IsOperationInProgress":
                Assert.Equal(expectedValue, controller.IsOperationInProgress);
                break;
        }
    }

    [Fact]
    public void StartProgramming_ByteOperation_SetsOperationCyclesRemaining()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.StartProgramming(false);

        Assert.Equal(FlashController.ByteProgramCycles, controller.OperationCyclesRemaining);
    }

    [Theory]
    [InlineData("ReturnsTrue", true)]
    [InlineData("State", FlashControllerState.Programming)]
    [InlineData("CurrentOperation", FlashOperation.Program)]
    public void StartProgramming_WordOperation_SetsProperty(string testCase, object expectedValue)
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.StartProgramming(true);

        switch (testCase)
        {
            case "ReturnsTrue":
                Assert.Equal(expectedValue, result);
                break;
            case "State":
                Assert.Equal(expectedValue, controller.State);
                break;
            case "CurrentOperation":
                Assert.Equal(expectedValue, controller.CurrentOperation);
                break;
        }
    }

    [Fact]
    public void StartProgramming_WordOperation_SetsOperationCyclesRemaining()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.StartProgramming(true);

        Assert.Equal(FlashController.WordProgramCycles, controller.OperationCyclesRemaining);
    }

    [Theory]
    [InlineData("ReturnsFalse", false)]
    [InlineData("State", FlashControllerState.Locked)]
    [InlineData("CurrentOperation", FlashOperation.None)]
    public void StartProgramming_WhenLocked_BehavesCorrectly(string testCase, object expectedValue)
    {
        var controller = new FlashController(_logger);

        bool result = controller.StartProgramming(false);

        switch (testCase)
        {
            case "ReturnsFalse":
                Assert.Equal(expectedValue, result);
                break;
            case "State":
                Assert.Equal(expectedValue, controller.State);
                break;
            case "CurrentOperation":
                Assert.Equal(expectedValue, controller.CurrentOperation);
                break;
        }
    }

    [Fact]
    public void StartProgramming_WhenWriteProtected_ReturnsFalse()
    {
        var controller = new FlashController(_logger);
        controller.SetProtectionLevel(FlashProtectionLevel.WriteProtected);
        controller.TryUnlock(0xA555);

        bool result = controller.StartProgramming(false);

        Assert.False(result);
    }

    [Theory]
    [InlineData("ReturnsTrue", true)]
    [InlineData("State", FlashControllerState.Erasing)]
    [InlineData("CurrentOperation", FlashOperation.SectorErase)]
    public void StartSectorErase_WhenUnlocked_SetsProperty(string testCase, object expectedValue)
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.StartSectorErase();

        switch (testCase)
        {
            case "ReturnsTrue":
                Assert.Equal(expectedValue, result);
                break;
            case "State":
                Assert.Equal(expectedValue, controller.State);
                break;
            case "CurrentOperation":
                Assert.Equal(expectedValue, controller.CurrentOperation);
                break;
        }
    }

    [Fact]
    public void StartSectorErase_WhenUnlocked_SetsOperationCyclesRemaining()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.StartSectorErase();

        Assert.Equal(FlashController.SectorEraseCycles, controller.OperationCyclesRemaining);
    }

    [Fact]
    public void StartSectorErase_WhenLocked_ReturnsFalse()
    {
        var controller = new FlashController(_logger);

        bool result = controller.StartSectorErase();

        Assert.False(result);
    }

    [Fact]
    public void StartSectorErase_WhenLocked_KeepsStateAsLocked()
    {
        var controller = new FlashController(_logger);

        controller.StartSectorErase();

        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Theory]
    [InlineData("ReturnsTrue", true)]
    [InlineData("State", FlashControllerState.Erasing)]
    [InlineData("CurrentOperation", FlashOperation.MassErase)]
    public void StartMassErase_WhenUnlocked_SetsProperty(string testCase, object expectedValue)
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.StartMassErase();

        switch (testCase)
        {
            case "ReturnsTrue":
                Assert.Equal(expectedValue, result);
                break;
            case "State":
                Assert.Equal(expectedValue, controller.State);
                break;
            case "CurrentOperation":
                Assert.Equal(expectedValue, controller.CurrentOperation);
                break;
        }
    }

    [Fact]
    public void StartMassErase_WhenUnlocked_SetsOperationCyclesRemaining()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.StartMassErase();

        Assert.Equal(FlashController.MassEraseCycles, controller.OperationCyclesRemaining);
    }

    [Theory]
    [InlineData("State", FlashControllerState.Unlocked)]
    [InlineData("CurrentOperation", FlashOperation.None)]
    [InlineData("OperationCyclesRemaining", 0u)]
    [InlineData("IsOperationInProgress", false)]
    public void Update_CompletesOperationWhenCyclesElapse_SetsProperty(string propertyName, object expectedValue)
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);
        uint initialCycles = controller.OperationCyclesRemaining;

        controller.Update(initialCycles);

        object? propertyValue = typeof(FlashController).GetProperty(propertyName)?.GetValue(controller);
        Assert.Equal(expectedValue, propertyValue);
    }

    [Theory]
    [InlineData("State", FlashControllerState.Programming)]
    [InlineData("IsOperationInProgress", true)]
    public void Update_PartialCycles_KeepsProperty(string propertyName, object expectedValue)
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);

        controller.Update(10);

        object? propertyValue = typeof(FlashController).GetProperty(propertyName)?.GetValue(controller);
        Assert.Equal(expectedValue, propertyValue);
    }

    [Fact]
    public void Update_PartialCycles_ReducesCyclesRemaining()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);
        uint initialCycles = controller.OperationCyclesRemaining;

        controller.Update(10);

        Assert.Equal(initialCycles - 10, controller.OperationCyclesRemaining);
    }

    [Fact]
    public void Update_ExcessCycles_SetsStateToUnlocked()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);
        uint initialCycles = controller.OperationCyclesRemaining;

        controller.Update(initialCycles + 100);

        Assert.Equal(FlashControllerState.Unlocked, controller.State);
    }

    [Fact]
    public void Update_ExcessCycles_SetsOperationCyclesRemainingToZero()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);
        uint initialCycles = controller.OperationCyclesRemaining;

        controller.Update(initialCycles + 100);

        Assert.Equal(0u, controller.OperationCyclesRemaining);
    }

    [Fact]
    public void Update_WhenNoOperationInProgress_KeepsStateUnlocked()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.Update(100);

        Assert.Equal(FlashControllerState.Unlocked, controller.State);
    }

    [Fact]
    public void Update_WhenNoOperationInProgress_KeepsOperationCyclesRemainingAtZero()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.Update(100);

        Assert.Equal(0u, controller.OperationCyclesRemaining);
    }

    [Theory]
    [InlineData(FlashProtectionLevel.None)]
    [InlineData(FlashProtectionLevel.WriteProtected)]
    [InlineData(FlashProtectionLevel.SecurityLocked)]
    public void SetProtectionLevel_ValidLevel_ReturnsTrue(FlashProtectionLevel level)
    {
        var controller = new FlashController(_logger);

        bool result = controller.SetProtectionLevel(level);

        Assert.True(result);
    }

    [Theory]
    [InlineData(FlashProtectionLevel.None)]
    [InlineData(FlashProtectionLevel.WriteProtected)]
    [InlineData(FlashProtectionLevel.SecurityLocked)]
    public void SetProtectionLevel_ValidLevel_SetsProtectionLevel(FlashProtectionLevel level)
    {
        var controller = new FlashController(_logger);

        controller.SetProtectionLevel(level);

        Assert.Equal(level, controller.ProtectionLevel);
    }

    [Fact]
    public void SetProtectionLevel_WhenOperationInProgress_ReturnsFalse()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);

        bool result = controller.SetProtectionLevel(FlashProtectionLevel.WriteProtected);

        Assert.False(result);
    }

    [Fact]
    public void SetProtectionLevel_WhenOperationInProgress_KeepsOriginalProtectionLevel()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);

        controller.SetProtectionLevel(FlashProtectionLevel.WriteProtected);

        Assert.Equal(FlashProtectionLevel.None, controller.ProtectionLevel);
    }

    [Fact]
    public void SetProtectionLevel_WhenPermanentlyLocked_ReturnsFalse()
    {
        var controller = new FlashController(_logger);
        controller.SetProtectionLevel(FlashProtectionLevel.PermanentlyLocked);

        bool result = controller.SetProtectionLevel(FlashProtectionLevel.None);

        Assert.False(result);
    }

    [Fact]
    public void SetProtectionLevel_WhenPermanentlyLocked_KeepsPermanentlyLockedLevel()
    {
        var controller = new FlashController(_logger);
        controller.SetProtectionLevel(FlashProtectionLevel.PermanentlyLocked);

        controller.SetProtectionLevel(FlashProtectionLevel.None);

        Assert.Equal(FlashProtectionLevel.PermanentlyLocked, controller.ProtectionLevel);
    }

    [Fact]
    public void SetProtectionLevel_LocksControllerWhenProtected()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.SetProtectionLevel(FlashProtectionLevel.WriteProtected);

        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Theory]
    [InlineData(FlashOperation.SectorErase)]
    [InlineData(FlashOperation.MassErase)]
    [InlineData(FlashOperation.SegmentErase)]
    public void GetEraseCycles_ValidOperation_ReturnsPositiveCycles(FlashOperation operation)
    {
        uint cycles = FlashController.GetEraseCycles(operation);

        Assert.True(cycles > 0);
    }

    [Fact]
    public void GetEraseCycles_SectorErase_ReturnsCorrectCycles()
    {
        uint cycles = FlashController.GetEraseCycles(FlashOperation.SectorErase);

        Assert.Equal(FlashController.SectorEraseCycles, cycles);
    }

    [Fact]
    public void GetEraseCycles_MassErase_ReturnsCorrectCycles()
    {
        uint cycles = FlashController.GetEraseCycles(FlashOperation.MassErase);

        Assert.Equal(FlashController.MassEraseCycles, cycles);
    }

    [Fact]
    public void GetEraseCycles_SegmentErase_ReturnsCorrectCycles()
    {
        uint cycles = FlashController.GetEraseCycles(FlashOperation.SegmentErase);

        Assert.Equal(FlashController.SectorEraseCycles / 8, cycles);
    }

    [Theory]
    [InlineData(FlashOperation.None)]
    [InlineData(FlashOperation.Program)]
    public void GetEraseCycles_NonEraseOperation_ReturnsZero(FlashOperation operation)
    {
        uint cycles = FlashController.GetEraseCycles(operation);

        Assert.Equal(0u, cycles);
    }

    [Theory]
    [InlineData(false)] // Byte programming
    [InlineData(true)]  // Word programming
    public void GetProgramCycles_ReturnsPositiveCycles(bool isWordAccess)
    {
        uint cycles = FlashController.GetProgramCycles(isWordAccess);

        Assert.True(cycles > 0);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_InitialStateIsLocked()
    {
        var controller = new FlashController(_logger);

        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_InitialOperationInProgressIsFalse()
    {
        var controller = new FlashController(_logger);

        Assert.False(controller.IsOperationInProgress);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_UnlockReturnsTrue()
    {
        var controller = new FlashController(_logger);

        bool result = controller.TryUnlock(0xA555);

        Assert.True(result);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_UnlockSetsStateToUnlocked()
    {
        var controller = new FlashController(_logger);

        controller.TryUnlock(0xA555);

        Assert.Equal(FlashControllerState.Unlocked, controller.State);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_StartProgrammingReturnsTrue()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.StartProgramming(false);

        Assert.True(result);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_StartProgrammingSetsStateToProgramming()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.StartProgramming(false);

        Assert.Equal(FlashControllerState.Programming, controller.State);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_StartProgrammingSetsOperationInProgressToTrue()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.StartProgramming(false);

        Assert.True(controller.IsOperationInProgress);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_CompleteOperationSetsStateToUnlocked()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);

        controller.Update(FlashController.ByteProgramCycles);

        Assert.Equal(FlashControllerState.Unlocked, controller.State);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_CompleteOperationSetsOperationInProgressToFalse()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);

        controller.Update(FlashController.ByteProgramCycles);

        Assert.False(controller.IsOperationInProgress);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_LockSetsStateToLocked()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);
        controller.Update(FlashController.ByteProgramCycles);

        controller.Lock();

        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Fact]
    public void OperationLifecycle_EraseOperation_StartSectorEraseReturnsTrue()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.StartSectorErase();

        Assert.True(result);
    }

    [Fact]
    public void OperationLifecycle_EraseOperation_StartSectorEraseSetsStateToErasing()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.StartSectorErase();

        Assert.Equal(FlashControllerState.Erasing, controller.State);
    }

    [Fact]
    public void OperationLifecycle_EraseOperation_CompleteOperationSetsStateToUnlocked()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartSectorErase();

        controller.Update(FlashController.SectorEraseCycles);

        Assert.Equal(FlashControllerState.Unlocked, controller.State);
    }

    [Fact]
    public void StateTransitions_LogsInitializationMessage()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var controller = new FlashController(_logger);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("FlashController initialized"));
    }

    [Fact]
    public void StateTransitions_LogsUnlockMessage()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var controller = new FlashController(_logger);

        controller.TryUnlock(0xA555);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("Flash controller unlocked"));
    }

    [Fact]
    public void StateTransitions_LogsProgrammingStartMessage()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.StartProgramming(false);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("Programming operation started"));
    }

    [Fact]
    public void StateTransitions_LogsOperationCompletedMessage()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);

        controller.Update(FlashController.ByteProgramCycles);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("operation completed"));
    }

    [Fact]
    public void StateTransitions_LogsLockMessage()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);
        controller.Update(FlashController.ByteProgramCycles);

        controller.Lock();

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("Flash controller locked"));
    }

    private class TestLogger : ILogger
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

    private record LogEntry(LogLevel Level, string Message, object? Context);
}
