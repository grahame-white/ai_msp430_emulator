using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Core;

namespace MSP430.Emulator.Tests.Core;

/// <summary>
/// Unit tests for the ExecutionState and ExecutionStateManager classes.
/// 
/// Tests validate execution state management according to:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) Section 1.4: Operating Modes
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) Section 3: CPU
/// 
/// Execution states include:
/// - Reset: Initial state after power-on or reset
/// - Running: Normal instruction execution
/// - Halted: Execution stopped (breakpoint, error)
/// - Low Power Modes: LPM0-LPM4.5 for power management
/// </summary>
public class ExecutionStateTests
{
    [Fact]
    public void ExecutionStateManager_Constructor_InitializesToResetState()
    {
        var stateManager = new ExecutionStateManager();

        Assert.Equal(ExecutionState.Reset, stateManager.CurrentState);
    }

    [Fact]
    public void TransitionTo_ValidTransition_ChangesState()
    {
        var stateManager = new ExecutionStateManager();

        stateManager.TransitionTo(ExecutionState.Running);

        Assert.Equal(ExecutionState.Running, stateManager.CurrentState);
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ThrowsException()
    {
        var stateManager = new ExecutionStateManager();
        stateManager.TransitionTo(ExecutionState.Running);
        stateManager.TransitionTo(ExecutionState.Error);

        Assert.Throws<InvalidOperationException>(() =>
            stateManager.TransitionTo(ExecutionState.Running));
    }

    [Theory]
    [InlineData(ExecutionState.Reset, ExecutionState.Running, true)]
    [InlineData(ExecutionState.Reset, ExecutionState.Stopped, true)]
    [InlineData(ExecutionState.Reset, ExecutionState.SingleStep, true)]
    [InlineData(ExecutionState.Reset, ExecutionState.Error, false)]
    [InlineData(ExecutionState.Running, ExecutionState.Stopped, true)]
    [InlineData(ExecutionState.Running, ExecutionState.Halted, true)]
    [InlineData(ExecutionState.Running, ExecutionState.Error, true)]
    [InlineData(ExecutionState.Running, ExecutionState.Reset, true)]
    [InlineData(ExecutionState.Stopped, ExecutionState.Running, true)]
    [InlineData(ExecutionState.Stopped, ExecutionState.SingleStep, true)]
    [InlineData(ExecutionState.Stopped, ExecutionState.Reset, true)]
    [InlineData(ExecutionState.SingleStep, ExecutionState.Running, true)]
    [InlineData(ExecutionState.SingleStep, ExecutionState.Stopped, true)]
    [InlineData(ExecutionState.SingleStep, ExecutionState.Halted, true)]
    [InlineData(ExecutionState.SingleStep, ExecutionState.Error, true)]
    [InlineData(ExecutionState.SingleStep, ExecutionState.Reset, true)]
    [InlineData(ExecutionState.Halted, ExecutionState.Reset, true)]
    [InlineData(ExecutionState.Halted, ExecutionState.Stopped, true)]
    [InlineData(ExecutionState.Halted, ExecutionState.Running, false)]
    [InlineData(ExecutionState.Error, ExecutionState.Reset, true)]
    [InlineData(ExecutionState.Error, ExecutionState.Running, false)]
    public void IsValidTransition_ReturnsExpectedResult(ExecutionState from, ExecutionState to, bool expected)
    {
        bool result = ExecutionStateManager.IsValidTransition(from, to);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ExecutionState.Running, true)]
    [InlineData(ExecutionState.SingleStep, true)]
    [InlineData(ExecutionState.Reset, false)]
    [InlineData(ExecutionState.Stopped, false)]
    [InlineData(ExecutionState.Halted, false)]
    [InlineData(ExecutionState.Error, false)]
    public void CanExecute_ReturnsExpectedResult(ExecutionState state, bool expected)
    {
        var stateManager = new ExecutionStateManager();

        // Transition to the test state through valid transitions
        switch (state)
        {
            case ExecutionState.Running:
                stateManager.TransitionTo(ExecutionState.Running);
                break;
            case ExecutionState.SingleStep:
                stateManager.TransitionTo(ExecutionState.SingleStep);
                break;
            case ExecutionState.Stopped:
                stateManager.TransitionTo(ExecutionState.Stopped);
                break;
            case ExecutionState.Halted:
                stateManager.TransitionTo(ExecutionState.Running);
                stateManager.TransitionTo(ExecutionState.Halted);
                break;
            case ExecutionState.Error:
                stateManager.TransitionTo(ExecutionState.Running);
                stateManager.TransitionTo(ExecutionState.Error);
                break;
            case ExecutionState.Reset:
                // Already in reset state
                break;
        }

        bool result = stateManager.CanExecute();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ExecutionState.Stopped, true)]
    [InlineData(ExecutionState.SingleStep, true)]
    [InlineData(ExecutionState.Reset, false)]
    [InlineData(ExecutionState.Running, false)]
    [InlineData(ExecutionState.Halted, false)]
    [InlineData(ExecutionState.Error, false)]
    public void CanResume_ReturnsExpectedResult(ExecutionState state, bool expected)
    {
        var stateManager = new ExecutionStateManager();

        // Transition to the test state through valid transitions
        switch (state)
        {
            case ExecutionState.Running:
                stateManager.TransitionTo(ExecutionState.Running);
                break;
            case ExecutionState.SingleStep:
                stateManager.TransitionTo(ExecutionState.SingleStep);
                break;
            case ExecutionState.Stopped:
                stateManager.TransitionTo(ExecutionState.Stopped);
                break;
            case ExecutionState.Halted:
                stateManager.TransitionTo(ExecutionState.Running);
                stateManager.TransitionTo(ExecutionState.Halted);
                break;
            case ExecutionState.Error:
                stateManager.TransitionTo(ExecutionState.Running);
                stateManager.TransitionTo(ExecutionState.Error);
                break;
            case ExecutionState.Reset:
                // Already in reset state
                break;
        }

        bool result = stateManager.CanResume();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Reset_ResetsToResetState()
    {
        var stateManager = new ExecutionStateManager();
        stateManager.TransitionTo(ExecutionState.Running);
        stateManager.TransitionTo(ExecutionState.Error);

        stateManager.Reset();

        Assert.Equal(ExecutionState.Reset, stateManager.CurrentState);
    }

    [Theory]
    [InlineData(ExecutionState.Reset, ExecutionState.Running, ExecutionState.Running)]
    [InlineData(ExecutionState.Running, ExecutionState.Stopped, ExecutionState.Stopped)]
    [InlineData(ExecutionState.Stopped, ExecutionState.SingleStep, ExecutionState.SingleStep)]
    [InlineData(ExecutionState.SingleStep, ExecutionState.Halted, ExecutionState.Halted)]
    [InlineData(ExecutionState.Halted, ExecutionState.Reset, ExecutionState.Reset)]
    public void TransitionTo_FromStateToState_UpdatesCurrentState(ExecutionState fromState, ExecutionState toState, ExecutionState expectedState)
    {
        var stateManager = new ExecutionStateManager();

        // Transition to the initial state if it's not Reset
        if (fromState != ExecutionState.Reset)
        {
            // Navigate to the from state through valid transitions
            switch (fromState)
            {
                case ExecutionState.Running:
                    stateManager.TransitionTo(ExecutionState.Running);
                    break;
                case ExecutionState.Stopped:
                    stateManager.TransitionTo(ExecutionState.Running);
                    stateManager.TransitionTo(ExecutionState.Stopped);
                    break;
                case ExecutionState.SingleStep:
                    stateManager.TransitionTo(ExecutionState.Running);
                    stateManager.TransitionTo(ExecutionState.Stopped);
                    stateManager.TransitionTo(ExecutionState.SingleStep);
                    break;
                case ExecutionState.Halted:
                    stateManager.TransitionTo(ExecutionState.Running);
                    stateManager.TransitionTo(ExecutionState.Stopped);
                    stateManager.TransitionTo(ExecutionState.SingleStep);
                    stateManager.TransitionTo(ExecutionState.Halted);
                    break;
            }
        }

        stateManager.TransitionTo(toState);

        Assert.Equal(expectedState, stateManager.CurrentState);
    }

    [Fact]
    public void MultipleTransitions_InitialState_IsReset()
    {
        var stateManager = new ExecutionStateManager();

        Assert.Equal(ExecutionState.Reset, stateManager.CurrentState);
    }
}
