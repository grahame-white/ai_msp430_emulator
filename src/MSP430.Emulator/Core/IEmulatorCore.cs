namespace MSP430.Emulator.Core;

/// <summary>
/// Defines the contract for the MSP430 emulator core engine.
/// 
/// The emulator core coordinates CPU execution, memory access, and system state,
/// providing the main interface for controlling emulator execution.
/// </summary>
public interface IEmulatorCore
{
    /// <summary>
    /// Gets the current execution state of the emulator.
    /// </summary>
    ExecutionState State { get; }

    /// <summary>
    /// Gets the execution statistics for performance monitoring.
    /// </summary>
    ExecutionStatistics Statistics { get; }

    /// <summary>
    /// Gets a value indicating whether the emulator is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets a value indicating whether the emulator is halted.
    /// </summary>
    bool IsHalted { get; }

    /// <summary>
    /// Gets the collection of active breakpoints.
    /// </summary>
    IReadOnlySet<ushort> Breakpoints { get; }

    /// <summary>
    /// Resets the emulator to its initial state.
    /// </summary>
    /// <remarks>
    /// This operation resets the CPU registers, clears execution statistics,
    /// and sets the execution state to Reset.
    /// </remarks>
    void Reset();

    /// <summary>
    /// Executes a single instruction at the current program counter.
    /// </summary>
    /// <returns>The number of CPU cycles consumed by the instruction.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the emulator is not in a state that allows execution.</exception>
    uint Step();

    /// <summary>
    /// Starts continuous execution until a halt condition, breakpoint, or error occurs.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the emulator is not in a state that allows execution.</exception>
    void Run();

    /// <summary>
    /// Runs the emulator for a specified number of instructions.
    /// </summary>
    /// <param name="instructionCount">The maximum number of instructions to execute.</param>
    /// <returns>The actual number of instructions executed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when instructionCount is less than or equal to zero.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the emulator is not in a state that allows execution.</exception>
    ulong Run(ulong instructionCount);

    /// <summary>
    /// Runs the emulator for a specified duration.
    /// </summary>
    /// <param name="duration">The maximum time to run the emulator.</param>
    /// <returns>The actual time spent executing.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when duration is negative or zero.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the emulator is not in a state that allows execution.</exception>
    TimeSpan Run(TimeSpan duration);

    /// <summary>
    /// Stops execution and transitions the emulator to the Stopped state.
    /// </summary>
    void Stop();

    /// <summary>
    /// Halts the emulator and transitions to the Halted state.
    /// </summary>
    void Halt();

    /// <summary>
    /// Adds a breakpoint at the specified address.
    /// </summary>
    /// <param name="address">The memory address where execution should break.</param>
    /// <returns>True if the breakpoint was added, false if it already exists.</returns>
    bool AddBreakpoint(ushort address);

    /// <summary>
    /// Removes a breakpoint from the specified address.
    /// </summary>
    /// <param name="address">The memory address of the breakpoint to remove.</param>
    /// <returns>True if the breakpoint was removed, false if it didn't exist.</returns>
    bool RemoveBreakpoint(ushort address);

    /// <summary>
    /// Removes all breakpoints.
    /// </summary>
    void ClearBreakpoints();

    /// <summary>
    /// Determines if there is a breakpoint at the specified address.
    /// </summary>
    /// <param name="address">The memory address to check.</param>
    /// <returns>True if there is a breakpoint at the address, false otherwise.</returns>
    bool HasBreakpoint(ushort address);

    /// <summary>
    /// Event raised when the execution state changes.
    /// </summary>
    event EventHandler<ExecutionStateChangedEventArgs> StateChanged;

    /// <summary>
    /// Event raised when a breakpoint is hit during execution.
    /// </summary>
    event EventHandler<BreakpointHitEventArgs> BreakpointHit;

    /// <summary>
    /// Event raised when an instruction is executed.
    /// </summary>
    event EventHandler<InstructionExecutedEventArgs> InstructionExecuted;
}

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

/// <summary>
/// Provides data for the BreakpointHit event.
/// </summary>
public class BreakpointHitEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the BreakpointHitEventArgs class.
    /// </summary>
    /// <param name="address">The address where the breakpoint was hit.</param>
    public BreakpointHitEventArgs(ushort address)
    {
        Address = address;
    }

    /// <summary>
    /// Gets the address where the breakpoint was hit.
    /// </summary>
    public ushort Address { get; }
}

/// <summary>
/// Provides data for the InstructionExecuted event.
/// </summary>
public class InstructionExecutedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the InstructionExecutedEventArgs class.
    /// </summary>
    /// <param name="address">The address of the executed instruction.</param>
    /// <param name="instructionWord">The 16-bit instruction word that was executed.</param>
    /// <param name="cycles">The number of CPU cycles consumed.</param>
    public InstructionExecutedEventArgs(ushort address, ushort instructionWord, uint cycles)
    {
        Address = address;
        InstructionWord = instructionWord;
        Cycles = cycles;
    }

    /// <summary>
    /// Gets the address of the executed instruction.
    /// </summary>
    public ushort Address { get; }

    /// <summary>
    /// Gets the 16-bit instruction word that was executed.
    /// </summary>
    public ushort InstructionWord { get; }

    /// <summary>
    /// Gets the number of CPU cycles consumed by the instruction.
    /// </summary>
    public uint Cycles { get; }
}
