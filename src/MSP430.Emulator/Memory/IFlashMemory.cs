using System;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the contract for flash memory operations in the MSP430 emulator.
/// 
/// This interface provides methods for reading, programming, and erasing flash memory
/// with proper timing characteristics, protection mechanisms, and state management
/// typical of MSP430 devices.
/// 
/// Interface design based on MSP430x2xx Family User's Guide (SLAU144J) - Section 5:
/// "Flash Memory Controller" - Programming model and operational requirements.
/// </summary>
public interface IFlashMemory
{
    /// <summary>
    /// Gets the size of the flash memory in bytes.
    /// </summary>
    int Size { get; }

    /// <summary>
    /// Gets the base address where this flash memory is mapped.
    /// </summary>
    ushort BaseAddress { get; }

    /// <summary>
    /// Gets the end address of this flash memory region (inclusive).
    /// </summary>
    ushort EndAddress { get; }

    /// <summary>
    /// Gets the current protection level of the flash memory.
    /// </summary>
    FlashProtectionLevel ProtectionLevel { get; }

    /// <summary>
    /// Gets the current state of the flash controller.
    /// </summary>
    FlashControllerState ControllerState { get; }

    /// <summary>
    /// Gets a value indicating whether a flash operation is currently in progress.
    /// </summary>
    bool IsOperationInProgress { get; }

    /// <summary>
    /// Gets the sector size in bytes (typically 512 bytes for MSP430).
    /// Based on MSP430x2xx Family User's Guide (SLAU144J) - Section 5.3: "Flash Memory Segmentation"
    /// </summary>
    int SectorSize { get; }

    /// <summary>
    /// Reads a byte from the specified address.
    /// Flash memory can always be read regardless of controller state.
    /// </summary>
    /// <param name="address">The memory address to read from.</param>
    /// <returns>The byte value at the specified address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    byte ReadByte(ushort address);

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
    /// Programs (writes) a byte to the specified address.
    /// Requires flash to be unlocked and not write-protected.
    /// </summary>
    /// <param name="address">The memory address to program.</param>
    /// <param name="value">The byte value to program.</param>
    /// <returns>True if programming was initiated successfully, false if blocked by protection or state.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    /// <exception cref="InvalidOperationException">Thrown when flash is in an invalid state for programming.</exception>
    bool ProgramByte(ushort address, byte value);

    /// <summary>
    /// Programs (writes) a 16-bit word to the specified address.
    /// Uses little-endian byte ordering (low byte at address, high byte at address+1).
    /// </summary>
    /// <param name="address">The memory address to program (must be even for aligned access).</param>
    /// <param name="value">The 16-bit word value to program.</param>
    /// <returns>True if programming was initiated successfully, false if blocked by protection or state.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is not word-aligned.</exception>
    /// <exception cref="InvalidOperationException">Thrown when flash is in an invalid state for programming.</exception>
    bool ProgramWord(ushort address, ushort value);

    /// <summary>
    /// Initiates a sector erase operation for the sector containing the specified address.
    /// </summary>
    /// <param name="address">Any address within the sector to be erased.</param>
    /// <returns>True if erase was initiated successfully, false if blocked by protection or state.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    /// <exception cref="InvalidOperationException">Thrown when flash is in an invalid state for erasing.</exception>
    bool EraseSector(ushort address);

    /// <summary>
    /// Initiates a mass erase operation to erase all flash memory.
    /// </summary>
    /// <returns>True if mass erase was initiated successfully, false if blocked by protection or state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when flash is in an invalid state for erasing.</exception>
    bool MassErase();

    /// <summary>
    /// Unlocks the flash memory for programming and erase operations.
    /// </summary>
    /// <param name="unlockKey">The unlock key sequence (typically 0xA5xx for MSP430).</param>
    /// <returns>True if unlock was successful, false if key is invalid or flash is protected.</returns>
    bool Unlock(ushort unlockKey);

    /// <summary>
    /// Locks the flash memory, preventing programming and erase operations.
    /// </summary>
    void Lock();

    /// <summary>
    /// Sets the protection level for the flash memory.
    /// </summary>
    /// <param name="protectionLevel">The desired protection level.</param>
    /// <returns>True if protection level was set successfully, false if operation is not allowed.</returns>
    bool SetProtectionLevel(FlashProtectionLevel protectionLevel);

    /// <summary>
    /// Clears all flash memory contents by setting all bytes to 0xFF (erased state).
    /// This is typically used for testing or initialization.
    /// </summary>
    void Clear();

    /// <summary>
    /// Initializes the flash memory with the specified pattern.
    /// This is typically used for testing or initialization.
    /// </summary>
    /// <param name="pattern">The byte pattern to fill the memory with (default is 0xFF for erased flash).</param>
    void Initialize(byte pattern = 0xFF);

    /// <summary>
    /// Gets the number of CPU cycles required for a read operation at the specified address.
    /// </summary>
    /// <param name="address">The memory address being accessed.</param>
    /// <param name="isWordAccess">True if accessing a 16-bit word, false for 8-bit byte access.</param>
    /// <returns>The number of CPU cycles required for the read operation.</returns>
    uint GetReadCycles(ushort address, bool isWordAccess = false);

    /// <summary>
    /// Gets the number of CPU cycles required for a programming operation at the specified address.
    /// Programming flash memory typically takes much longer than RAM writes.
    /// </summary>
    /// <param name="address">The memory address being programmed.</param>
    /// <param name="isWordAccess">True if programming a 16-bit word, false for 8-bit byte access.</param>
    /// <returns>The number of CPU cycles required for the programming operation.</returns>
    uint GetProgramCycles(ushort address, bool isWordAccess = false);

    /// <summary>
    /// Gets the number of CPU cycles required for an erase operation.
    /// </summary>
    /// <param name="operation">The type of erase operation (sector, mass, etc.).</param>
    /// <returns>The number of CPU cycles required for the erase operation.</returns>
    uint GetEraseCycles(FlashOperation operation);

    /// <summary>
    /// Determines if the specified address is within the bounds of this flash memory.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>True if the address is within bounds, false otherwise.</returns>
    bool IsAddressInBounds(ushort address);

    /// <summary>
    /// Determines if the specified address range is entirely within the bounds of this flash memory.
    /// </summary>
    /// <param name="startAddress">The starting address of the range.</param>
    /// <param name="length">The length of the range in bytes.</param>
    /// <returns>True if the entire range is within bounds, false otherwise.</returns>
    bool IsRangeInBounds(ushort startAddress, int length);

    /// <summary>
    /// Gets the sector number for the specified address.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>The sector number containing the address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    int GetSectorNumber(ushort address);

    /// <summary>
    /// Gets the base address of the sector containing the specified address.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>The base address of the sector containing the specified address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    ushort GetSectorBaseAddress(ushort address);
}
