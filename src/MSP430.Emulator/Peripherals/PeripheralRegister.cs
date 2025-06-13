using System;

namespace MSP430.Emulator.Peripherals;

/// <summary>
/// Defines the access permissions for a peripheral register.
/// </summary>
[Flags]
public enum PeripheralRegisterAccess
{
    /// <summary>
    /// No access allowed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Read access allowed.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Write access allowed.
    /// </summary>
    Write = 2,

    /// <summary>
    /// Both read and write access allowed.
    /// </summary>
    ReadWrite = Read | Write
}

/// <summary>
/// Represents a memory-mapped peripheral register with configurable access permissions and bit masking.
/// 
/// This class provides the foundation for implementing MSP430FR2355 peripheral registers,
/// handling read/write operations, access validation, and bit-level control.
/// </summary>
public class PeripheralRegister
{
    private ushort _value;
    private readonly ushort _resetValue;
    private readonly ushort _readMask;
    private readonly ushort _writeMask;
    private readonly PeripheralRegisterAccess _access;

    /// <summary>
    /// Initializes a new instance of the PeripheralRegister class.
    /// </summary>
    /// <param name="address">The memory address of this register.</param>
    /// <param name="resetValue">The value this register should have after reset.</param>
    /// <param name="access">The access permissions for this register.</param>
    /// <param name="readMask">Bitmask defining which bits can be read (1 = readable, 0 = always reads as 0).</param>
    /// <param name="writeMask">Bitmask defining which bits can be written (1 = writable, 0 = ignored on write).</param>
    /// <param name="name">Optional name for this register (for debugging and logging).</param>
    public PeripheralRegister(ushort address, ushort resetValue = 0,
        PeripheralRegisterAccess access = PeripheralRegisterAccess.ReadWrite,
        ushort readMask = 0xFFFF, ushort writeMask = 0xFFFF, string? name = null)
    {
        Address = address;
        _resetValue = resetValue;
        _access = access;
        _readMask = readMask;
        _writeMask = writeMask;
        Name = name ?? $"REG_{address:X4}";
        _value = resetValue;
    }

    /// <summary>
    /// Gets the memory address of this register.
    /// </summary>
    public ushort Address { get; }

    /// <summary>
    /// Gets the name of this register.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the current value of this register.
    /// Reading applies the read mask to return only readable bits.
    /// </summary>
    public ushort Value => (ushort)(_value & _readMask);

    /// <summary>
    /// Gets the raw internal value of this register without applying read mask.
    /// </summary>
    internal ushort RawValue => _value;

    /// <summary>
    /// Gets the reset value for this register.
    /// </summary>
    public ushort ResetValue => _resetValue;

    /// <summary>
    /// Gets the access permissions for this register.
    /// </summary>
    public PeripheralRegisterAccess Access => _access;

    /// <summary>
    /// Gets the read mask for this register.
    /// </summary>
    public ushort ReadMask => _readMask;

    /// <summary>
    /// Gets the write mask for this register.
    /// </summary>
    public ushort WriteMask => _writeMask;

    /// <summary>
    /// Gets a value indicating whether this register supports read operations.
    /// </summary>
    public bool IsReadable => (_access & PeripheralRegisterAccess.Read) != 0;

    /// <summary>
    /// Gets a value indicating whether this register supports write operations.
    /// </summary>
    public bool IsWritable => (_access & PeripheralRegisterAccess.Write) != 0;

    /// <summary>
    /// Reads a byte value from this register.
    /// </summary>
    /// <param name="offset">The byte offset within the register (0 for low byte, 1 for high byte).</param>
    /// <returns>The byte value at the specified offset.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the register is not readable.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset is not 0 or 1.</exception>
    public byte ReadByte(int offset = 0)
    {
        if (!IsReadable)
        {
            throw new InvalidOperationException($"Register {Name} at address 0x{Address:X4} is not readable");
        }

        if (offset < 0 || offset > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be 0 or 1");
        }

        ushort maskedValue = Value;
        return offset == 0 ? (byte)(maskedValue & 0xFF) : (byte)((maskedValue >> 8) & 0xFF);
    }

    /// <summary>
    /// Reads the full 16-bit word value from this register.
    /// </summary>
    /// <returns>The 16-bit word value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the register is not readable.</exception>
    public ushort ReadWord()
    {
        if (!IsReadable)
        {
            throw new InvalidOperationException($"Register {Name} at address 0x{Address:X4} is not readable");
        }

        return Value;
    }

    /// <summary>
    /// Writes a byte value to this register.
    /// </summary>
    /// <param name="value">The byte value to write.</param>
    /// <param name="offset">The byte offset within the register (0 for low byte, 1 for high byte).</param>
    /// <returns>True if the write was successful, false if blocked by access permissions.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset is not 0 or 1.</exception>
    public bool WriteByte(byte value, int offset = 0)
    {
        if (!IsWritable)
        {
            return false;
        }

        if (offset < 0 || offset > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be 0 or 1");
        }

        ushort mask = offset == 0 ? (ushort)0x00FF : (ushort)0xFF00;
        ushort writeMask = (ushort)(_writeMask & mask);
        ushort shiftedValue = offset == 0 ? value : (ushort)(value << 8);

        // Only update bits that are writable
        _value = (ushort)((_value & ~writeMask) | (shiftedValue & writeMask));
        return true;
    }

    /// <summary>
    /// Writes a 16-bit word value to this register.
    /// </summary>
    /// <param name="value">The 16-bit word value to write.</param>
    /// <returns>True if the write was successful, false if blocked by access permissions.</returns>
    public bool WriteWord(ushort value)
    {
        if (!IsWritable)
        {
            return false;
        }

        // Only update bits that are writable
        _value = (ushort)((_value & ~_writeMask) | (value & _writeMask));
        return true;
    }

    /// <summary>
    /// Resets this register to its reset value.
    /// </summary>
    public void Reset()
    {
        _value = _resetValue;
    }

    /// <summary>
    /// Sets the internal value directly, bypassing write protection.
    /// This should only be used for internal peripheral operations.
    /// </summary>
    /// <param name="value">The value to set.</param>
    internal void SetValue(ushort value)
    {
        _value = value;
    }

    /// <summary>
    /// Returns a string representation of this register.
    /// </summary>
    /// <returns>A string containing the register name, address, and current value.</returns>
    public override string ToString()
    {
        return $"{Name} (0x{Address:X4}) = 0x{Value:X4}";
    }
}
