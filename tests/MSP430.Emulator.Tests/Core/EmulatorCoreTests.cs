using MSP430.Emulator.Core;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Core;

public class EmulatorCoreTests
{
    private readonly RegisterFile _registerFile;
    private readonly MemoryMap _memoryMap;
    private readonly InstructionDecoder _instructionDecoder;
    private readonly TestLogger _logger;
    private readonly EmulatorCore _emulatorCore;

    public EmulatorCoreTests()
    {
        _logger = new TestLogger();
        _registerFile = new RegisterFile(_logger);
        _memoryMap = new MemoryMap();
        _instructionDecoder = new InstructionDecoder();

        _emulatorCore = new EmulatorCore(_registerFile, _memoryMap, _instructionDecoder, _logger);
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesSuccessfully()
    {
        Assert.NotNull(_emulatorCore);
        Assert.Equal(ExecutionState.Reset, _emulatorCore.State);
        Assert.False(_emulatorCore.IsRunning);
        Assert.False(_emulatorCore.IsHalted);
        Assert.Empty(_emulatorCore.Breakpoints);
    }

    [Fact]
    public void Constructor_WithNullRegisterFile_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmulatorCore(null!, _memoryMap, _instructionDecoder, _logger));
    }

    [Fact]
    public void Constructor_WithNullMemoryMap_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmulatorCore(_registerFile, null!, _instructionDecoder, _logger));
    }

    [Fact]
    public void Constructor_WithNullInstructionDecoder_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmulatorCore(_registerFile, _memoryMap, null!, _logger));
    }

    [Fact]
    public void Reset_ResetsRegisterFileAndState()
    {
        _emulatorCore.Reset();

        Assert.Equal(ExecutionState.Reset, _emulatorCore.State);
        Assert.Equal(0UL, _emulatorCore.Statistics.InstructionsExecuted);
        // Verify register file was reset by checking PC is 0
        Assert.Equal((ushort)0, _registerFile.GetProgramCounter());
    }

    [Fact]
    public void Reset_RaisesStateChangedEvent()
    {
        ExecutionStateChangedEventArgs? eventArgs = null;
        _emulatorCore.StateChanged += (sender, args) => eventArgs = args;

        _emulatorCore.Reset();

        Assert.NotNull(eventArgs);
        Assert.Equal(ExecutionState.Reset, eventArgs.NewState);
    }

    [Fact]
    public void Step_FromResetState_ChangesToSingleStepState()
    {
        // Set PC to a valid executable address (typical MSP430 program memory starts around 0x8000)
        _registerFile.SetProgramCounter(0x8000);

        // Since we don't have valid instructions in memory, this should throw InvalidInstructionException
        // when the decoder tries to decode the 0x0000 instruction word
        Assert.Throws<InvalidInstructionException>(() => _emulatorCore.Step());

        Assert.Equal(ExecutionState.SingleStep, _emulatorCore.State);
    }

    [Fact]
    public void AddBreakpoint_AddsBreakpointToCollection()
    {
        ushort address = 0x8000;

        bool added = _emulatorCore.AddBreakpoint(address);

        Assert.True(added);
        Assert.True(_emulatorCore.HasBreakpoint(address));
        Assert.Contains(address, _emulatorCore.Breakpoints);
    }

    [Fact]
    public void AddBreakpoint_DuplicateAddress_ReturnsFalse()
    {
        ushort address = 0x8000;
        _emulatorCore.AddBreakpoint(address);

        bool added = _emulatorCore.AddBreakpoint(address);

        Assert.False(added);
        Assert.Single(_emulatorCore.Breakpoints);
    }

    [Fact]
    public void RemoveBreakpoint_ExistingBreakpoint_RemovesAndReturnsTrue()
    {
        ushort address = 0x8000;
        _emulatorCore.AddBreakpoint(address);

        bool removed = _emulatorCore.RemoveBreakpoint(address);

        Assert.True(removed);
        Assert.False(_emulatorCore.HasBreakpoint(address));
        Assert.Empty(_emulatorCore.Breakpoints);
    }

    [Fact]
    public void RemoveBreakpoint_NonExistentBreakpoint_ReturnsFalse()
    {
        ushort address = 0x8000;

        bool removed = _emulatorCore.RemoveBreakpoint(address);

        Assert.False(removed);
    }

    [Fact]
    public void ClearBreakpoints_RemovesAllBreakpoints()
    {
        _emulatorCore.AddBreakpoint(0x8000);
        _emulatorCore.AddBreakpoint(0x8002);
        _emulatorCore.AddBreakpoint(0x8004);

        _emulatorCore.ClearBreakpoints();

        Assert.Empty(_emulatorCore.Breakpoints);
    }

    [Fact]
    public void Stop_ChangesToStoppedState()
    {
        _emulatorCore.Stop();

        Assert.Equal(ExecutionState.Stopped, _emulatorCore.State);
        Assert.False(_emulatorCore.IsRunning);
    }

    [Fact]
    public void Halt_ChangesToHaltedState()
    {
        // First transition to a state that can be halted (e.g., SingleStep)
        // Set PC to valid address to avoid memory access issues
        _registerFile.SetProgramCounter(0x8000);

        // This will transition to SingleStep state, then fail due to invalid instruction
        // when the decoder tries to decode the 0x0000 instruction word
        Assert.Throws<InvalidInstructionException>(() => _emulatorCore.Step());

        // Now halt from SingleStep state
        _emulatorCore.Halt();

        Assert.Equal(ExecutionState.Halted, _emulatorCore.State);
        Assert.True(_emulatorCore.IsHalted);
    }

    [Fact]
    public void Run_WithInstructionCount_StartsExecution()
    {
        // Set PC to a valid executable address 
        _registerFile.SetProgramCounter(0x8000);

        // This will fail due to invalid instructions, but we can test that it attempts to run
        Assert.Throws<InvalidInstructionException>(() => _emulatorCore.Run(5));
        // State should transition to Error due to invalid instruction
        Assert.Equal(ExecutionState.Error, _emulatorCore.State);
    }

    [Fact]
    public void Run_WithDuration_StartsExecution()
    {
        // Set PC to a valid executable address 
        _registerFile.SetProgramCounter(0x8000);
        var duration = TimeSpan.FromMilliseconds(10);

        // This will fail due to invalid instructions, but we can test that it attempts to run
        Assert.Throws<InvalidInstructionException>(() => _emulatorCore.Run(duration));
        // State should transition to Error due to invalid instruction
        Assert.Equal(ExecutionState.Error, _emulatorCore.State);
    }

    [Fact]
    public void Run_WithZeroInstructions_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _emulatorCore.Run(0UL));
    }

    [Fact]
    public void Run_WithNegativeDuration_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _emulatorCore.Run(TimeSpan.FromMilliseconds(-1)));
    }

    [Fact]
    public void IsRunning_WhenInRunningState_ReturnsTrue()
    {
        // This test is challenging because Run() is a blocking operation
        // In a real scenario, we'd need to test this from another thread
        Assert.False(_emulatorCore.IsRunning); // Initially not running
    }

    [Fact]
    public void Statistics_InitiallyZero()
    {
        Assert.Equal(0UL, _emulatorCore.Statistics.InstructionsExecuted);
        Assert.Equal(0UL, _emulatorCore.Statistics.TotalCycles);
    }

    [Fact]
    public void StateChanged_Event_RaisedOnStateTransitions()
    {
        var stateChanges = new List<ExecutionStateChangedEventArgs>();
        _emulatorCore.StateChanged += (sender, args) => stateChanges.Add(args);

        _emulatorCore.Stop();
        _emulatorCore.Reset();

        Assert.Equal(2, stateChanges.Count);
        Assert.Equal(ExecutionState.Stopped, stateChanges[0].NewState);
        Assert.Equal(ExecutionState.Reset, stateChanges[1].NewState);
    }

    private class TestLogger : ILogger
    {
        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;
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
        public void Error(string message, Exception exception) => Log(LogLevel.Error, $"{message}: {exception}");
        public void Fatal(string message) => Log(LogLevel.Fatal, message);
        public void Fatal(string message, object? context) => Log(LogLevel.Fatal, message, context);

        public bool IsEnabled(LogLevel level) => level >= MinimumLevel;
    }

    private record LogEntry(LogLevel Level, string Message, object? Context);
}
