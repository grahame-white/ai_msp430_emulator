namespace MSP430.Emulator.Core;

/// <summary>
/// Defines the execution states of the MSP430 emulator core.
/// 
/// The emulator core can be in various states that control how instructions
/// are processed and system behavior is managed.
/// </summary>
public enum ExecutionState
{
    /// <summary>
    /// The emulator has been reset and is ready to start execution.
    /// Program counter and registers are at their initial state.
    /// </summary>
    Reset,

    /// <summary>
    /// The emulator is running continuously, executing instructions
    /// until a halt condition, breakpoint, or error occurs.
    /// </summary>
    Running,

    /// <summary>
    /// The emulator is stopped and waiting for a command.
    /// Can be resumed or single-stepped.
    /// </summary>
    Stopped,

    /// <summary>
    /// The emulator is in single-step mode, executing one instruction
    /// at a time when commanded.
    /// </summary>
    SingleStep,

    /// <summary>
    /// The emulator has halted due to a halt instruction or
    /// low-power mode entry.
    /// </summary>
    Halted,

    /// <summary>
    /// The emulator has encountered an error condition and
    /// cannot continue execution.
    /// </summary>
    Error
}

/// <summary>
/// Provides execution state management and transition logic for the MSP430 emulator core.
/// 
/// This class manages state transitions, validates state changes, and provides
/// utilities for working with emulator execution states.
/// </summary>
public class ExecutionStateManager
{
    private ExecutionState _currentState;

    /// <summary>
    /// Initializes a new instance of the ExecutionStateManager class.
    /// </summary>
    public ExecutionStateManager()
    {
        _currentState = ExecutionState.Reset;
    }

    /// <summary>
    /// Gets the current execution state.
    /// </summary>
    public ExecutionState CurrentState => _currentState;

    /// <summary>
    /// Transitions to a new execution state.
    /// </summary>
    /// <param name="newState">The new state to transition to.</param>
    /// <exception cref="InvalidOperationException">Thrown when the state transition is invalid.</exception>
    public void TransitionTo(ExecutionState newState)
    {
        if (!IsValidTransition(_currentState, newState))
        {
            throw new InvalidOperationException(
                $"Invalid state transition from {_currentState} to {newState}");
        }

        _currentState = newState;
    }

    /// <summary>
    /// Determines if a transition from one state to another is valid.
    /// </summary>
    /// <param name="from">The current state.</param>
    /// <param name="to">The target state.</param>
    /// <returns>True if the transition is valid, false otherwise.</returns>
    public static bool IsValidTransition(ExecutionState from, ExecutionState to)
    {
        return from switch
        {
            ExecutionState.Reset => to is ExecutionState.Running or ExecutionState.Stopped or ExecutionState.SingleStep,
            ExecutionState.Running => to is ExecutionState.Stopped or ExecutionState.Halted or ExecutionState.Error or ExecutionState.Reset,
            ExecutionState.Stopped => to is ExecutionState.Running or ExecutionState.SingleStep or ExecutionState.Reset,
            ExecutionState.SingleStep => to is ExecutionState.Running or ExecutionState.Stopped or ExecutionState.Halted or ExecutionState.Error or ExecutionState.Reset,
            ExecutionState.Halted => to is ExecutionState.Reset or ExecutionState.Stopped,
            ExecutionState.Error => to is ExecutionState.Reset,
            _ => false
        };
    }

    /// <summary>
    /// Determines if the emulator is in a state where it can execute instructions.
    /// </summary>
    /// <returns>True if the emulator can execute instructions, false otherwise.</returns>
    public bool CanExecute() => _currentState is ExecutionState.Running or ExecutionState.SingleStep;

    /// <summary>
    /// Determines if the emulator is in a stopped state and can be resumed.
    /// </summary>
    /// <returns>True if the emulator can be resumed, false otherwise.</returns>
    public bool CanResume() => _currentState is ExecutionState.Stopped or ExecutionState.SingleStep;

    /// <summary>
    /// Resets the execution state to its initial value.
    /// </summary>
    public void Reset()
    {
        _currentState = ExecutionState.Reset;
    }
}
