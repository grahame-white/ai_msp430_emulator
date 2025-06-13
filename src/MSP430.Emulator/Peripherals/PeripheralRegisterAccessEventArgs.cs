using System;

namespace MSP430.Emulator.Peripherals;

/// <summary>
/// Provides data for peripheral register access events.
/// </summary>
public class PeripheralRegisterAccessEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the PeripheralRegisterAccessEventArgs class.
    /// </summary>
    /// <param name="address">The register address that was accessed.</param>
    /// <param name="value">The value that was read or written.</param>
    /// <param name="isWrite">True if this was a write operation, false for read.</param>
    /// <param name="isWordAccess">True if this was a 16-bit word access, false for 8-bit byte access.</param>
    public PeripheralRegisterAccessEventArgs(ushort address, ushort value, bool isWrite, bool isWordAccess)
    {
        Address = address;
        Value = value;
        IsWrite = isWrite;
        IsWordAccess = isWordAccess;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the register address that was accessed.
    /// </summary>
    public ushort Address { get; }

    /// <summary>
    /// Gets the value that was read or written.
    /// For byte access, only the lower 8 bits are significant.
    /// </summary>
    public ushort Value { get; }

    /// <summary>
    /// Gets a value indicating whether this was a write operation.
    /// </summary>
    public bool IsWrite { get; }

    /// <summary>
    /// Gets a value indicating whether this was a read operation.
    /// </summary>
    public bool IsRead => !IsWrite;

    /// <summary>
    /// Gets a value indicating whether this was a 16-bit word access.
    /// </summary>
    public bool IsWordAccess { get; }

    /// <summary>
    /// Gets a value indicating whether this was an 8-bit byte access.
    /// </summary>
    public bool IsByteAccess => !IsWordAccess;

    /// <summary>
    /// Gets the timestamp when the register access occurred.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Gets the byte value for byte access operations.
    /// </summary>
    public byte ByteValue => (byte)(Value & 0xFF);
}
