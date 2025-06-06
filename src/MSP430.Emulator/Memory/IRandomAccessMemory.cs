namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the contract for random access memory operations in the MSP430 emulator.
/// 
/// This interface provides methods for reading and writing to RAM with proper
/// timing characteristics and bounds checking typical of MSP430 devices.
/// </summary>
public interface IRandomAccessMemory
{
    /// <summary>
    /// Gets the size of the memory in bytes.
    /// </summary>
    int Size { get; }

    /// <summary>
    /// Gets the base address where this memory is mapped.
    /// </summary>
    ushort BaseAddress { get; }

    /// <summary>
    /// Gets the end address of this memory region (inclusive).
    /// </summary>
    ushort EndAddress { get; }

    /// <summary>
    /// Reads a byte from the specified address.
    /// </summary>
    /// <param name="address">The memory address to read from.</param>
    /// <returns>The byte value at the specified address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    byte ReadByte(ushort address);

    /// <summary>
    /// Writes a byte to the specified address.
    /// </summary>
    /// <param name="address">The memory address to write to.</param>
    /// <param name="value">The byte value to write.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    void WriteByte(ushort address, byte value);

    /// <summary>
    /// Reads a 16-bit word from the specified address.
    /// Uses little-endian byte ordering (low byte at address, high byte at address+1).
    /// </summary>
    /// <param name="address">The memory address to read from (must be even for aligned access).</param>
    /// <returns>The 16-bit word value at the specified address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is not word-aligned.</exception>
    ushort ReadWord(ushort address);

    /// <summary>
    /// Writes a 16-bit word to the specified address.
    /// Uses little-endian byte ordering (low byte at address, high byte at address+1).
    /// </summary>
    /// <param name="address">The memory address to write to (must be even for aligned access).</param>
    /// <param name="value">The 16-bit word value to write.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is not word-aligned.</exception>
    void WriteWord(ushort address, ushort value);

    /// <summary>
    /// Clears all memory contents by setting all bytes to zero.
    /// </summary>
    void Clear();

    /// <summary>
    /// Initializes the memory with the specified pattern.
    /// </summary>
    /// <param name="pattern">The byte pattern to fill the memory with.</param>
    void Initialize(byte pattern = 0x00);

    /// <summary>
    /// Gets the number of CPU cycles required for a read operation at the specified address.
    /// </summary>
    /// <param name="address">The memory address being accessed.</param>
    /// <param name="isWordAccess">True if accessing a 16-bit word, false for 8-bit byte access.</param>
    /// <returns>The number of CPU cycles required for the read operation.</returns>
    uint GetReadCycles(ushort address, bool isWordAccess = false);

    /// <summary>
    /// Gets the number of CPU cycles required for a write operation at the specified address.
    /// </summary>
    /// <param name="address">The memory address being accessed.</param>
    /// <param name="isWordAccess">True if accessing a 16-bit word, false for 8-bit byte access.</param>
    /// <returns>The number of CPU cycles required for the write operation.</returns>
    uint GetWriteCycles(ushort address, bool isWordAccess = false);

    /// <summary>
    /// Determines if the specified address is within the bounds of this memory.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>True if the address is within bounds, false otherwise.</returns>
    bool IsAddressInBounds(ushort address);

    /// <summary>
    /// Determines if the specified address range is entirely within the bounds of this memory.
    /// </summary>
    /// <param name="startAddress">The starting address of the range.</param>
    /// <param name="length">The length of the range in bytes.</param>
    /// <returns>True if the entire range is within bounds, false otherwise.</returns>
    bool IsRangeInBounds(ushort startAddress, int length);
}
