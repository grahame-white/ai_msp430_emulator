using System;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Provides data for memory access violation events.
/// </summary>
public class MemoryAccessViolationEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the MemoryAccessViolationEventArgs class.
    /// </summary>
    /// <param name="context">The memory access context that caused the violation.</param>
    /// <param name="reason">The reason for the access violation.</param>
    /// <param name="exception">The exception that was raised (optional).</param>
    public MemoryAccessViolationEventArgs(MemoryAccessContext context, string reason, Exception? exception = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        Exception = exception;
    }

    /// <summary>
    /// Gets the memory access context that caused the violation.
    /// </summary>
    public MemoryAccessContext Context { get; }

    /// <summary>
    /// Gets the reason for the access violation.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Gets the exception that was raised (if any).
    /// </summary>
    public Exception? Exception { get; }
}
