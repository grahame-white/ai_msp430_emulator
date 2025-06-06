namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the types of flash memory operations supported by the MSP430 emulator.
/// 
/// These operations correspond to the flash controller commands available in
/// real MSP430 devices for programming and erasing flash memory.
/// 
/// Based on MSP430x2xx Family User's Guide (SLAU144J) - Section 5.3: "Flash Memory Controller"
/// Flash Memory Control Register (FCTL1) operation bits and command sequences.
/// </summary>
public enum FlashOperation
{
    /// <summary>
    /// No operation in progress. Flash memory is in read mode.
    /// </summary>
    None = 0,

    /// <summary>
    /// Program (write) operation - writes data to flash memory.
    /// Requires unlock sequence and proper timing.
    /// </summary>
    Program = 1,

    /// <summary>
    /// Sector erase operation - erases a single flash sector (typically 512 bytes).
    /// Sets all bits in the sector to '1' (0xFF bytes).
    /// </summary>
    SectorErase = 2,

    /// <summary>
    /// Mass erase operation - erases all flash memory.
    /// Sets all bits in flash to '1' (0xFF bytes).
    /// </summary>
    MassErase = 3,

    /// <summary>
    /// Segment erase operation - erases a flash segment (sub-sector unit).
    /// Smaller than sector erase, typically 64 bytes.
    /// </summary>
    SegmentErase = 4
}
