using System;

namespace MSP430.Emulator.Core;

/// <summary>
/// Provides data for the StateChanged event.
/// </summary>
public class ExecutionStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the ExecutionStateChangedEventArgs class.
    /// </summary>
    /// <param name="previousState">The previous execution state.</param>
    /// <param name="newState">The new execution state.</param>
    public ExecutionStateChangedEventArgs(ExecutionState previousState, ExecutionState newState)
    {
        PreviousState = previousState;
        NewState = newState;
    }

    /// <summary>
    /// Gets the previous execution state.
    /// </summary>
    public ExecutionState PreviousState { get; }

    /// <summary>
    /// Gets the new execution state.
    /// </summary>
    public ExecutionState NewState { get; }
}
