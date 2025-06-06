using System;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Provides data for memory access events.
/// </summary>
public class MemoryAccessEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the MemoryAccessEventArgs class.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <param name="region">The memory region accessed.</param>
    /// <param name="cycles">The number of CPU cycles consumed.</param>
    /// <param name="value">The value read or written (optional).</param>
    public MemoryAccessEventArgs(MemoryAccessContext context, MemoryRegionInfo region, uint cycles, object? value = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Region = region;
        Cycles = cycles;
        Value = value;
    }

    /// <summary>
    /// Gets the memory access context.
    /// </summary>
    public MemoryAccessContext Context { get; }

    /// <summary>
    /// Gets the memory region that was accessed.
    /// </summary>
    public MemoryRegionInfo Region { get; }

    /// <summary>
    /// Gets the number of CPU cycles consumed by the access.
    /// </summary>
    public uint Cycles { get; }

    /// <summary>
    /// Gets the value read or written (if applicable).
    /// </summary>
    public object? Value { get; }
}
