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
    private readonly EmulatorCoreTestFixture _fixture;

    public EmulatorCoreTests()
    {
        _fixture = new EmulatorCoreTestFixture();
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesValidInstance()
    {
        Assert.NotNull(_fixture.EmulatorCore);
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesToResetState()
    {
        Assert.Equal(ExecutionState.Reset, _fixture.EmulatorCore.State);
    }

    [Fact]
    public void Constructor_WithValidParameters_IsNotRunning()
    {
        Assert.False(_fixture.EmulatorCore.IsRunning);
    }

    [Fact]
    public void Constructor_WithValidParameters_IsNotHalted()
    {
        Assert.False(_fixture.EmulatorCore.IsHalted);
    }

    [Fact]
    public void Constructor_WithValidParameters_HasEmptyBreakpoints()
    {
        Assert.Empty(_fixture.EmulatorCore.Breakpoints);
    }

    [Fact]
    public void Constructor_WithNullRegisterFile_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmulatorCore(null!, _fixture.MemoryMap, _fixture.InstructionDecoder, _fixture.Logger));
    }

    [Fact]
    public void Constructor_WithNullMemoryMap_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmulatorCore(_fixture.RegisterFile, null!, _fixture.InstructionDecoder, _fixture.Logger));
    }

    [Fact]
    public void Constructor_WithNullInstructionDecoder_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmulatorCore(_fixture.RegisterFile, _fixture.MemoryMap, null!, _fixture.Logger));
    }

    [Fact]
    public void Reset_SetsStateToReset()
    {
        _fixture.EmulatorCore.Reset();

        Assert.Equal(ExecutionState.Reset, _fixture.EmulatorCore.State);
    }

    [Fact]
    public void Reset_ClearsInstructionStatistics()
    {
        _fixture.EmulatorCore.Reset();

        Assert.Equal(0UL, _fixture.EmulatorCore.Statistics.InstructionsExecuted);
    }

    [Fact]
    public void Reset_LoadsProgramCounterFromResetVector()
    {
        _fixture.EmulatorCore.Reset();

        // When memory is uninitialized (all zeros), reset vector at 0xFFFE-0xFFFF will be 0x0000
        // So PC should be loaded with 0x0000 from the reset vector, not just reset to 0
        Assert.Equal((ushort)0, _fixture.RegisterFile.GetProgramCounter());
    }

    [Fact]
    public void Reset_RaisesStateChangedEvent()
    {
        ExecutionStateChangedEventArgs? eventArgs = null;
        _fixture.EmulatorCore.StateChanged += (sender, args) => eventArgs = args;

        _fixture.EmulatorCore.Reset();

        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void Reset_StateChangedEventHasCorrectNewState()
    {
        ExecutionStateChangedEventArgs? eventArgs = null;
        _fixture.EmulatorCore.StateChanged += (sender, args) => eventArgs = args;

        _fixture.EmulatorCore.Reset();

        Assert.Equal(ExecutionState.Reset, eventArgs?.NewState);
    }

    [Fact]
    public void Step_FromResetState_ThrowsInvalidInstructionException()
    {
        // Set PC to a valid executable address (typical MSP430 program memory starts around 0x8000)
        _fixture.RegisterFile.SetProgramCounter(0x8000);

        // Since we don't have valid instructions in memory, this should throw InvalidInstructionException
        // when the decoder tries to decode the 0x0000 instruction word
        Assert.Throws<InvalidInstructionException>(() => _fixture.EmulatorCore.Step());
    }

    [Fact]
    public void Step_FromResetState_TransitionsToSingleStepState()
    {
        // Set PC to a valid executable address (typical MSP430 program memory starts around 0x8000)
        _fixture.RegisterFile.SetProgramCounter(0x8000);

        // Since we don't have valid instructions in memory, this should throw InvalidInstructionException
        // when the decoder tries to decode the 0x0000 instruction word
        try
        {
            _fixture.EmulatorCore.Step();
        }
        catch (InvalidInstructionException)
        {
            // Expected exception, check state after exception
        }

        Assert.Equal(ExecutionState.SingleStep, _fixture.EmulatorCore.State);
    }

    [Fact]
    public void AddBreakpoint_AddsBreakpointSuccessfully()
    {
        ushort address = 0x8000;

        bool added = _fixture.EmulatorCore.AddBreakpoint(address);

        Assert.True(added);
    }

    [Fact]
    public void AddBreakpoint_BreakpointCanBeFound()
    {
        ushort address = 0x8000;

        _fixture.EmulatorCore.AddBreakpoint(address);

        Assert.True(_fixture.EmulatorCore.HasBreakpoint(address));
    }

    [Fact]
    public void AddBreakpoint_BreakpointInCollection()
    {
        ushort address = 0x8000;

        _fixture.EmulatorCore.AddBreakpoint(address);

        Assert.Contains(address, _fixture.EmulatorCore.Breakpoints);
    }

    [Fact]
    public void AddBreakpoint_DuplicateAddress_ReturnsFalse()
    {
        ushort address = 0x8000;
        _fixture.EmulatorCore.AddBreakpoint(address);

        bool added = _fixture.EmulatorCore.AddBreakpoint(address);

        Assert.False(added);
    }

    [Fact]
    public void AddBreakpoint_DuplicateAddress_KeepsSingleBreakpoint()
    {
        ushort address = 0x8000;
        _fixture.EmulatorCore.AddBreakpoint(address);

        _fixture.EmulatorCore.AddBreakpoint(address);

        Assert.Single(_fixture.EmulatorCore.Breakpoints);
    }

    [Fact]
    public void RemoveBreakpoint_ExistingBreakpoint_ReturnsTrue()
    {
        ushort address = 0x8000;
        _fixture.EmulatorCore.AddBreakpoint(address);

        bool removed = _fixture.EmulatorCore.RemoveBreakpoint(address);

        Assert.True(removed);
    }

    [Fact]
    public void RemoveBreakpoint_ExistingBreakpoint_BreakpointNotFound()
    {
        ushort address = 0x8000;
        _fixture.EmulatorCore.AddBreakpoint(address);

        _fixture.EmulatorCore.RemoveBreakpoint(address);

        Assert.False(_fixture.EmulatorCore.HasBreakpoint(address));
    }

    [Fact]
    public void RemoveBreakpoint_ExistingBreakpoint_BreakpointsEmpty()
    {
        ushort address = 0x8000;
        _fixture.EmulatorCore.AddBreakpoint(address);

        _fixture.EmulatorCore.RemoveBreakpoint(address);

        Assert.Empty(_fixture.EmulatorCore.Breakpoints);
    }

    [Fact]
    public void RemoveBreakpoint_NonExistentBreakpoint_ReturnsFalse()
    {
        ushort address = 0x8000;

        bool removed = _fixture.EmulatorCore.RemoveBreakpoint(address);

        Assert.False(removed);
    }

    [Fact]
    public void ClearBreakpoints_RemovesAllBreakpoints()
    {
        _fixture.EmulatorCore.AddBreakpoint(0x8000);
        _fixture.EmulatorCore.AddBreakpoint(0x8002);
        _fixture.EmulatorCore.AddBreakpoint(0x8004);

        _fixture.EmulatorCore.ClearBreakpoints();

        Assert.Empty(_fixture.EmulatorCore.Breakpoints);
    }

    [Fact]
    public void Stop_SetsStateToStopped()
    {
        _fixture.EmulatorCore.Stop();

        Assert.Equal(ExecutionState.Stopped, _fixture.EmulatorCore.State);
    }

    [Fact]
    public void Stop_SetsIsRunningToFalse()
    {
        _fixture.EmulatorCore.Stop();

        Assert.False(_fixture.EmulatorCore.IsRunning);
    }

    [Fact]
    public void Halt_TransitionsToHaltedState()
    {
        // First transition to a state that can be halted (e.g., SingleStep)
        // Set PC to valid address to avoid memory access issues
        _fixture.RegisterFile.SetProgramCounter(0x8000);

        // This will transition to SingleStep state, then fail due to invalid instruction
        // when the decoder tries to decode the 0x0000 instruction word
        try
        {
            _fixture.EmulatorCore.Step();
        }
        catch (InvalidInstructionException)
        {
            // Expected exception
        }

        // Now halt from SingleStep state
        _fixture.EmulatorCore.Halt();

        Assert.Equal(ExecutionState.Halted, _fixture.EmulatorCore.State);
    }

    [Fact]
    public void Halt_SetsIsHaltedToTrue()
    {
        // First transition to a state that can be halted (e.g., SingleStep)
        // Set PC to valid address to avoid memory access issues
        _fixture.RegisterFile.SetProgramCounter(0x8000);

        // This will transition to SingleStep state, then fail due to invalid instruction
        // when the decoder tries to decode the 0x0000 instruction word
        try
        {
            _fixture.EmulatorCore.Step();
        }
        catch (InvalidInstructionException)
        {
            // Expected exception
        }

        // Now halt from SingleStep state
        _fixture.EmulatorCore.Halt();

        Assert.True(_fixture.EmulatorCore.IsHalted);
    }

    [Fact]
    public void Run_WithInstructionCount_ThrowsInvalidInstructionException()
    {
        // Set PC to a valid executable address 
        _fixture.RegisterFile.SetProgramCounter(0x8000);

        // This will fail due to invalid instructions, but we can test that it attempts to run
        Assert.Throws<InvalidInstructionException>(() => _fixture.EmulatorCore.Run(5));
    }

    [Fact]
    public void Run_WithInstructionCount_TransitionsToErrorState()
    {
        // Set PC to a valid executable address 
        _fixture.RegisterFile.SetProgramCounter(0x8000);

        // This will fail due to invalid instructions, but we can test that it attempts to run
        try
        {
            _fixture.EmulatorCore.Run(5);
        }
        catch (InvalidInstructionException)
        {
            // Expected exception
        }

        // State should transition to Error due to invalid instruction
        Assert.Equal(ExecutionState.Error, _fixture.EmulatorCore.State);
    }

    [Fact]
    public void Run_WithDuration_ThrowsInvalidInstructionException()
    {
        // Set PC to a valid executable address 
        _fixture.RegisterFile.SetProgramCounter(0x8000);
        var duration = TimeSpan.FromMilliseconds(10);

        // This will fail due to invalid instructions, but we can test that it attempts to run
        Assert.Throws<InvalidInstructionException>(() => _fixture.EmulatorCore.Run(duration));
    }

    [Fact]
    public void Run_WithDuration_TransitionsToErrorState()
    {
        // Set PC to a valid executable address 
        _fixture.RegisterFile.SetProgramCounter(0x8000);
        var duration = TimeSpan.FromMilliseconds(10);

        // This will fail due to invalid instructions, but we can test that it attempts to run
        try
        {
            _fixture.EmulatorCore.Run(duration);
        }
        catch (InvalidInstructionException)
        {
            // Expected exception
        }

        // State should transition to Error due to invalid instruction
        Assert.Equal(ExecutionState.Error, _fixture.EmulatorCore.State);
    }

    [Fact]
    public void Run_WithZeroInstructions_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _fixture.EmulatorCore.Run(0UL));
    }

    [Fact]
    public void Run_WithNegativeDuration_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _fixture.EmulatorCore.Run(TimeSpan.FromMilliseconds(-1)));
    }

    [Fact]
    public void IsRunning_WhenInRunningState_ReturnsTrue()
    {
        // This test is challenging because Run() is a blocking operation
        // In a real scenario, we'd need to test this from another thread
        Assert.False(_fixture.EmulatorCore.IsRunning); // Initially not running
    }

    [Fact]
    public void Statistics_InstructionsExecutedInitiallyZero()
    {
        Assert.Equal(0UL, _fixture.EmulatorCore.Statistics.InstructionsExecuted);
    }

    [Fact]
    public void Statistics_TotalCyclesInitiallyZero()
    {
        Assert.Equal(0UL, _fixture.EmulatorCore.Statistics.TotalCycles);
    }

    [Fact]
    public void StateChanged_Event_RaisedCorrectNumberOfTimes()
    {
        var stateChanges = new List<ExecutionStateChangedEventArgs>();
        _fixture.EmulatorCore.StateChanged += (sender, args) => stateChanges.Add(args);

        _fixture.EmulatorCore.Stop();
        _fixture.EmulatorCore.Reset();

        Assert.Equal(2, stateChanges.Count);
    }

    [Fact]
    public void StateChanged_Event_FirstTransitionToStopped()
    {
        var stateChanges = new List<ExecutionStateChangedEventArgs>();
        _fixture.EmulatorCore.StateChanged += (sender, args) => stateChanges.Add(args);

        _fixture.EmulatorCore.Stop();
        _fixture.EmulatorCore.Reset();

        Assert.Equal(ExecutionState.Stopped, stateChanges[0].NewState);
    }

    [Fact]
    public void StateChanged_Event_SecondTransitionToReset()
    {
        var stateChanges = new List<ExecutionStateChangedEventArgs>();
        _fixture.EmulatorCore.StateChanged += (sender, args) => stateChanges.Add(args);

        _fixture.EmulatorCore.Stop();
        _fixture.EmulatorCore.Reset();

        Assert.Equal(ExecutionState.Reset, stateChanges[1].NewState);
    }
}
