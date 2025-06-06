using System;

namespace MSP430.Emulator.Core;

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
