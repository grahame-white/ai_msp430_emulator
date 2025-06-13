using System;

namespace MSP430.Emulator.Peripherals;

/// <summary>
/// Provides data for peripheral interrupt events.
/// </summary>
public class PeripheralInterruptEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the PeripheralInterruptEventArgs class.
    /// </summary>
    /// <param name="interruptVector">The interrupt vector address for this interrupt.</param>
    /// <param name="interruptName">The name or identifier of the interrupt.</param>
    /// <param name="priority">The interrupt priority level.</param>
    /// <param name="peripheralId">The identifier of the peripheral generating the interrupt.</param>
    public PeripheralInterruptEventArgs(ushort interruptVector, string interruptName, byte priority, string peripheralId)
    {
        InterruptVector = interruptVector;
        InterruptName = interruptName ?? throw new ArgumentNullException(nameof(interruptName));
        Priority = priority;
        PeripheralId = peripheralId ?? throw new ArgumentNullException(nameof(peripheralId));
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the interrupt vector address for this interrupt.
    /// </summary>
    public ushort InterruptVector { get; }

    /// <summary>
    /// Gets the name or identifier of the interrupt.
    /// </summary>
    public string InterruptName { get; }

    /// <summary>
    /// Gets the interrupt priority level.
    /// Higher values indicate higher priority.
    /// </summary>
    public byte Priority { get; }

    /// <summary>
    /// Gets the identifier of the peripheral generating the interrupt.
    /// </summary>
    public string PeripheralId { get; }

    /// <summary>
    /// Gets the timestamp when the interrupt was requested.
    /// </summary>
    public DateTime Timestamp { get; }
}
