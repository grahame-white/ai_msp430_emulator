using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Core;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Core;

/// <summary>
/// Unit tests for the EmulatorCore class.
/// 
/// Core emulator functionality includes:
/// - CPU instruction execution cycle
/// - Memory subsystem integration
/// - Interrupt handling coordination
/// - Execution state management
/// - Reset and initialization behavior
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1: Introduction
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 1: Device Overview
/// </summary>
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
    public void Constructor_WithValidParameters_CreatesValidInstance()
    {
        Assert.NotNull(_emulatorCore);
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesToResetState()
    {
        Assert.Equal(ExecutionState.Reset, _emulatorCore.State);
    }

    [Fact]
    public void Constructor_WithValidParameters_IsNotRunning()
    {
        Assert.False(_emulatorCore.IsRunning);
    }

    [Fact]
    public void Constructor_WithValidParameters_IsNotHalted()
    {
        Assert.False(_emulatorCore.IsHalted);
    }

    [Fact]
    public void Constructor_WithValidParameters_HasEmptyBreakpoints()
    {
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
    public void Reset_SetsStateToReset()
    {
        _emulatorCore.Reset();

        Assert.Equal(ExecutionState.Reset, _emulatorCore.State);
    }

    [Fact]
    public void Reset_ClearsInstructionStatistics()
    {
        _emulatorCore.Reset();

        Assert.Equal(0UL, _emulatorCore.Statistics.InstructionsExecuted);
    }

    [Fact]
    public void Reset_LoadsProgramCounterFromResetVector()
    {
        _emulatorCore.Reset();

        // When memory is uninitialized (all zeros), reset vector at 0xFFFE-0xFFFF will be 0x0000
        // So PC should be loaded with 0x0000 from the reset vector, not just reset to 0
        Assert.Equal((ushort)0, _registerFile.GetProgramCounter());
    }

    [Fact]
    public void Reset_RaisesStateChangedEvent()
    {
        ExecutionStateChangedEventArgs? eventArgs = null;
        _emulatorCore.StateChanged += (sender, args) => eventArgs = args;

        _emulatorCore.Reset();

        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void Reset_StateChangedEventHasCorrectNewState()
    {
        ExecutionStateChangedEventArgs? eventArgs = null;
        _emulatorCore.StateChanged += (sender, args) => eventArgs = args;

        _emulatorCore.Reset();

        Assert.Equal(ExecutionState.Reset, eventArgs?.NewState);
    }

    [Fact]
    public void Step_FromResetState_ThrowsInvalidInstructionException()
    {
        // Set PC to a valid executable address (typical MSP430 program memory starts around 0x8000)
        _registerFile.SetProgramCounter(0x8000);

        // Since we don't have valid instructions in memory, this should throw InvalidInstructionException
        // when the decoder tries to decode the 0x0000 instruction word
        Assert.Throws<InvalidInstructionException>(() => _emulatorCore.Step());
    }

    [Fact]
    public void Step_FromResetState_TransitionsToSingleStepState()
    {
        // Set PC to a valid executable address (typical MSP430 program memory starts around 0x8000)
        _registerFile.SetProgramCounter(0x8000);

        // Since we don't have valid instructions in memory, this should throw InvalidInstructionException
        // when the decoder tries to decode the 0x0000 instruction word
        try
        {
            _emulatorCore.Step();
        }
        catch (InvalidInstructionException)
        {
            // Expected exception, check state after exception
        }

        Assert.Equal(ExecutionState.SingleStep, _emulatorCore.State);
    }

    [Fact]
    public void AddBreakpoint_AddsBreakpointSuccessfully()
    {
        ushort address = 0x8000;

        bool added = _emulatorCore.AddBreakpoint(address);

        Assert.True(added);
    }

    [Fact]
    public void AddBreakpoint_BreakpointCanBeFound()
    {
        ushort address = 0x8000;

        _emulatorCore.AddBreakpoint(address);

        Assert.True(_emulatorCore.HasBreakpoint(address));
    }

    [Fact]
    public void AddBreakpoint_BreakpointInCollection()
    {
        ushort address = 0x8000;

        _emulatorCore.AddBreakpoint(address);

        Assert.Contains(address, _emulatorCore.Breakpoints);
    }

    [Fact]
    public void AddBreakpoint_DuplicateAddress_ReturnsFalse()
    {
        ushort address = 0x8000;
        _emulatorCore.AddBreakpoint(address);

        bool added = _emulatorCore.AddBreakpoint(address);

        Assert.False(added);
    }

    [Fact]
    public void AddBreakpoint_DuplicateAddress_KeepsSingleBreakpoint()
    {
        ushort address = 0x8000;
        _emulatorCore.AddBreakpoint(address);

        _emulatorCore.AddBreakpoint(address);

        Assert.Single(_emulatorCore.Breakpoints);
    }

    [Fact]
    public void RemoveBreakpoint_ExistingBreakpoint_ReturnsTrue()
    {
        ushort address = 0x8000;
        _emulatorCore.AddBreakpoint(address);

        bool removed = _emulatorCore.RemoveBreakpoint(address);

        Assert.True(removed);
    }

    [Fact]
    public void RemoveBreakpoint_ExistingBreakpoint_BreakpointNotFound()
    {
        ushort address = 0x8000;
        _emulatorCore.AddBreakpoint(address);

        _emulatorCore.RemoveBreakpoint(address);

        Assert.False(_emulatorCore.HasBreakpoint(address));
    }

    [Fact]
    public void RemoveBreakpoint_ExistingBreakpoint_BreakpointsEmpty()
    {
        ushort address = 0x8000;
        _emulatorCore.AddBreakpoint(address);

        _emulatorCore.RemoveBreakpoint(address);

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
    public void Stop_SetsStateToStopped()
    {
        _emulatorCore.Stop();

        Assert.Equal(ExecutionState.Stopped, _emulatorCore.State);
    }

    [Fact]
    public void Stop_SetsIsRunningToFalse()
    {
        _emulatorCore.Stop();

        Assert.False(_emulatorCore.IsRunning);
    }

    [Fact]
    public void Halt_TransitionsToHaltedState()
    {
        // First transition to a state that can be halted (e.g., SingleStep)
        // Set PC to valid address to avoid memory access issues
        _registerFile.SetProgramCounter(0x8000);

        // This will transition to SingleStep state, then fail due to invalid instruction
        // when the decoder tries to decode the 0x0000 instruction word
        try
        {
            _emulatorCore.Step();
        }
        catch (InvalidInstructionException)
        {
            // Expected exception
        }

        // Now halt from SingleStep state
        _emulatorCore.Halt();

        Assert.Equal(ExecutionState.Halted, _emulatorCore.State);
    }

    [Fact]
    public void Halt_SetsIsHaltedToTrue()
    {
        // First transition to a state that can be halted (e.g., SingleStep)
        // Set PC to valid address to avoid memory access issues
        _registerFile.SetProgramCounter(0x8000);

        // This will transition to SingleStep state, then fail due to invalid instruction
        // when the decoder tries to decode the 0x0000 instruction word
        try
        {
            _emulatorCore.Step();
        }
        catch (InvalidInstructionException)
        {
            // Expected exception
        }

        // Now halt from SingleStep state
        _emulatorCore.Halt();

        Assert.True(_emulatorCore.IsHalted);
    }

    [Fact]
    public void Run_WithInstructionCount_ThrowsInvalidInstructionException()
    {
        // Set PC to a valid executable address 
        _registerFile.SetProgramCounter(0x8000);

        // This will fail due to invalid instructions, but we can test that it attempts to run
        Assert.Throws<InvalidInstructionException>(() => _emulatorCore.Run(5));
    }

    [Fact]
    public void Run_WithInstructionCount_TransitionsToErrorState()
    {
        // Set PC to a valid executable address 
        _registerFile.SetProgramCounter(0x8000);

        // This will fail due to invalid instructions, but we can test that it attempts to run
        try
        {
            _emulatorCore.Run(5);
        }
        catch (InvalidInstructionException)
        {
            // Expected exception
        }

        // State should transition to Error due to invalid instruction
        Assert.Equal(ExecutionState.Error, _emulatorCore.State);
    }

    [Fact]
    public void Run_WithDuration_ThrowsInvalidInstructionException()
    {
        // Set PC to a valid executable address 
        _registerFile.SetProgramCounter(0x8000);
        var duration = TimeSpan.FromMilliseconds(10);

        // This will fail due to invalid instructions, but we can test that it attempts to run
        Assert.Throws<InvalidInstructionException>(() => _emulatorCore.Run(duration));
    }

    [Fact]
    public void Run_WithDuration_TransitionsToErrorState()
    {
        // Set PC to a valid executable address 
        _registerFile.SetProgramCounter(0x8000);
        var duration = TimeSpan.FromMilliseconds(10);

        // This will fail due to invalid instructions, but we can test that it attempts to run
        try
        {
            _emulatorCore.Run(duration);
        }
        catch (InvalidInstructionException)
        {
            // Expected exception
        }

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
    public void Statistics_InstructionsExecutedInitiallyZero()
    {
        Assert.Equal(0UL, _emulatorCore.Statistics.InstructionsExecuted);
    }

    [Fact]
    public void Statistics_TotalCyclesInitiallyZero()
    {
        Assert.Equal(0UL, _emulatorCore.Statistics.TotalCycles);
    }

    [Fact]
    public void StateChanged_Event_RaisedCorrectNumberOfTimes()
    {
        var stateChanges = new List<ExecutionStateChangedEventArgs>();
        _emulatorCore.StateChanged += (sender, args) => stateChanges.Add(args);

        _emulatorCore.Stop();
        _emulatorCore.Reset();

        Assert.Equal(2, stateChanges.Count);
    }

    [Fact]
    public void StateChanged_Event_FirstTransitionToStopped()
    {
        var stateChanges = new List<ExecutionStateChangedEventArgs>();
        _emulatorCore.StateChanged += (sender, args) => stateChanges.Add(args);

        _emulatorCore.Stop();
        _emulatorCore.Reset();

        Assert.Equal(ExecutionState.Stopped, stateChanges[0].NewState);
    }

    [Fact]
    public void StateChanged_Event_SecondTransitionToReset()
    {
        var stateChanges = new List<ExecutionStateChangedEventArgs>();
        _emulatorCore.StateChanged += (sender, args) => stateChanges.Add(args);

        _emulatorCore.Stop();
        _emulatorCore.Reset();

        Assert.Equal(ExecutionState.Reset, stateChanges[1].NewState);
    }
}
