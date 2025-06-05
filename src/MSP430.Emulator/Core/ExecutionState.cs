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
