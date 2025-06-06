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

    [Fact]
    public void Constructor_InitializesInLockedState()
    {
        var controller = new FlashController(_logger);

        Assert.Equal(FlashControllerState.Locked, controller.State);
        Assert.Equal(FlashProtectionLevel.None, controller.ProtectionLevel);
        Assert.Equal(FlashOperation.None, controller.CurrentOperation);
        Assert.False(controller.IsOperationInProgress);
        Assert.Equal(0u, controller.OperationCyclesRemaining);
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
        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Fact]
    public void TryUnlock_WhenAlreadyUnlocked_ReturnsFalse()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.TryUnlock(0xA555);

        Assert.False(result);
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
        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Fact]
    public void Lock_FromUnlockedState_ChangesToLocked()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.Lock();

        Assert.Equal(FlashControllerState.Locked, controller.State);
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

    [Fact]
    public void StartProgramming_ByteOperation_ReturnsTrue()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.StartProgramming(false);

        Assert.True(result);
        Assert.Equal(FlashControllerState.Programming, controller.State);
        Assert.Equal(FlashOperation.Program, controller.CurrentOperation);
        Assert.Equal(FlashController.ByteProgramCycles, controller.OperationCyclesRemaining);
        Assert.True(controller.IsOperationInProgress);
    }

    [Fact]
    public void StartProgramming_WordOperation_ReturnsTrue()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.StartProgramming(true);

        Assert.True(result);
        Assert.Equal(FlashControllerState.Programming, controller.State);
        Assert.Equal(FlashOperation.Program, controller.CurrentOperation);
        Assert.Equal(FlashController.WordProgramCycles, controller.OperationCyclesRemaining);
    }

    [Fact]
    public void StartProgramming_WhenLocked_ReturnsFalse()
    {
        var controller = new FlashController(_logger);

        bool result = controller.StartProgramming(false);

        Assert.False(result);
        Assert.Equal(FlashControllerState.Locked, controller.State);
        Assert.Equal(FlashOperation.None, controller.CurrentOperation);
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

    [Fact]
    public void StartSectorErase_WhenUnlocked_ReturnsTrue()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.StartSectorErase();

        Assert.True(result);
        Assert.Equal(FlashControllerState.Erasing, controller.State);
        Assert.Equal(FlashOperation.SectorErase, controller.CurrentOperation);
        Assert.Equal(FlashController.SectorEraseCycles, controller.OperationCyclesRemaining);
    }

    [Fact]
    public void StartSectorErase_WhenLocked_ReturnsFalse()
    {
        var controller = new FlashController(_logger);

        bool result = controller.StartSectorErase();

        Assert.False(result);
        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Fact]
    public void StartMassErase_WhenUnlocked_ReturnsTrue()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        bool result = controller.StartMassErase();

        Assert.True(result);
        Assert.Equal(FlashControllerState.Erasing, controller.State);
        Assert.Equal(FlashOperation.MassErase, controller.CurrentOperation);
        Assert.Equal(FlashController.MassEraseCycles, controller.OperationCyclesRemaining);
    }

    [Fact]
    public void Update_CompletesOperationWhenCyclesElapse()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);
        uint initialCycles = controller.OperationCyclesRemaining;

        controller.Update(initialCycles);

        Assert.Equal(FlashControllerState.Unlocked, controller.State);
        Assert.Equal(FlashOperation.None, controller.CurrentOperation);
        Assert.Equal(0u, controller.OperationCyclesRemaining);
        Assert.False(controller.IsOperationInProgress);
    }

    [Fact]
    public void Update_PartialCycles_ReducesCyclesRemaining()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);
        uint initialCycles = controller.OperationCyclesRemaining;

        controller.Update(10);

        Assert.Equal(FlashControllerState.Programming, controller.State);
        Assert.Equal(initialCycles - 10, controller.OperationCyclesRemaining);
        Assert.True(controller.IsOperationInProgress);
    }

    [Fact]
    public void Update_ExcessCycles_CompletesOperation()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);
        uint initialCycles = controller.OperationCyclesRemaining;

        controller.Update(initialCycles + 100);

        Assert.Equal(FlashControllerState.Unlocked, controller.State);
        Assert.Equal(0u, controller.OperationCyclesRemaining);
    }

    [Fact]
    public void Update_WhenNoOperationInProgress_DoesNothing()
    {
        var controller = new FlashController(_logger);
        controller.TryUnlock(0xA555);

        controller.Update(100);

        Assert.Equal(FlashControllerState.Unlocked, controller.State);
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
        Assert.Equal(FlashProtectionLevel.None, controller.ProtectionLevel);
    }

    [Fact]
    public void SetProtectionLevel_WhenPermanentlyLocked_ReturnsFalse()
    {
        var controller = new FlashController(_logger);
        controller.SetProtectionLevel(FlashProtectionLevel.PermanentlyLocked);

        bool result = controller.SetProtectionLevel(FlashProtectionLevel.None);

        Assert.False(result);
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
    public void GetEraseCycles_ValidOperation_ReturnsExpectedCycles(FlashOperation operation)
    {
        uint cycles = FlashController.GetEraseCycles(operation);

        Assert.True(cycles > 0);

        switch (operation)
        {
            case FlashOperation.SectorErase:
                Assert.Equal(FlashController.SectorEraseCycles, cycles);
                break;
            case FlashOperation.MassErase:
                Assert.Equal(FlashController.MassEraseCycles, cycles);
                break;
            case FlashOperation.SegmentErase:
                Assert.Equal(FlashController.SectorEraseCycles / 8, cycles);
                break;
        }
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
    public void GetProgramCycles_ReturnsExpectedCycles(bool isWordAccess)
    {
        uint cycles = FlashController.GetProgramCycles(isWordAccess);

        if (isWordAccess)
        {
            Assert.Equal(FlashController.WordProgramCycles, cycles);
        }
        else
        {
            Assert.Equal(FlashController.ByteProgramCycles, cycles);
        }

        Assert.True(cycles > 0);
    }

    [Fact]
    public void OperationLifecycle_ProgrammingOperation_WorksCorrectly()
    {
        var controller = new FlashController(_logger);

        // Initial state
        Assert.Equal(FlashControllerState.Locked, controller.State);
        Assert.False(controller.IsOperationInProgress);

        // Unlock
        Assert.True(controller.TryUnlock(0xA555));
        Assert.Equal(FlashControllerState.Unlocked, controller.State);

        // Start programming
        Assert.True(controller.StartProgramming(false));
        Assert.Equal(FlashControllerState.Programming, controller.State);
        Assert.True(controller.IsOperationInProgress);

        // Complete operation
        controller.Update(FlashController.ByteProgramCycles);
        Assert.Equal(FlashControllerState.Unlocked, controller.State);
        Assert.False(controller.IsOperationInProgress);

        // Lock
        controller.Lock();
        Assert.Equal(FlashControllerState.Locked, controller.State);
    }

    [Fact]
    public void OperationLifecycle_EraseOperation_WorksCorrectly()
    {
        var controller = new FlashController(_logger);

        // Unlock and start erase
        controller.TryUnlock(0xA555);
        Assert.True(controller.StartSectorErase());
        Assert.Equal(FlashControllerState.Erasing, controller.State);

        // Complete operation
        controller.Update(FlashController.SectorEraseCycles);
        Assert.Equal(FlashControllerState.Unlocked, controller.State);
    }

    [Fact]
    public void StateTransitions_LogCorrectMessages()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var controller = new FlashController(_logger);

        controller.TryUnlock(0xA555);
        controller.StartProgramming(false);
        controller.Update(FlashController.ByteProgramCycles);
        controller.Lock();

        // Check that debug messages were logged
        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("FlashController initialized"));
        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("Flash controller unlocked"));
        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("Programming operation started"));
        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("operation completed"));
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
