namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the types of flash memory operations supported by the MSP430 emulator.
/// 
/// These operations correspond to the flash controller commands available in
/// real MSP430 devices for programming and erasing flash memory.
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

/// <summary>
/// Defines the state of the flash memory controller.
/// 
/// The flash controller follows a state machine pattern for secure
/// flash operations, preventing accidental writes or erases.
/// </summary>
public enum FlashControllerState
{
    /// <summary>
    /// Flash is locked and in read-only mode.
    /// No programming or erase operations can be performed.
    /// </summary>
    Locked = 0,

    /// <summary>
    /// Flash has been unlocked but no operation is active.
    /// Ready to accept programming or erase commands.
    /// </summary>
    Unlocked = 1,

    /// <summary>
    /// Programming operation is in progress.
    /// Flash controller is busy writing data.
    /// </summary>
    Programming = 2,

    /// <summary>
    /// Erase operation is in progress.
    /// Flash controller is busy erasing sectors/segments.
    /// </summary>
    Erasing = 3,

    /// <summary>
    /// Operation completed successfully.
    /// Ready to return to locked or unlocked state.
    /// </summary>
    OperationComplete = 4,

    /// <summary>
    /// Error occurred during operation.
    /// Requires reset or error handling.
    /// </summary>
    Error = 5
}

/// <summary>
/// Defines flash memory protection levels.
/// 
/// Protection mechanisms prevent unauthorized access to flash memory
/// and maintain data integrity in MSP430 devices.
/// </summary>
public enum FlashProtectionLevel
{
    /// <summary>
    /// No protection - flash can be read, programmed, and erased.
    /// </summary>
    None = 0,

    /// <summary>
    /// Write protection - flash can be read but not modified.
    /// Programming and erase operations are blocked.
    /// </summary>
    WriteProtected = 1,

    /// <summary>
    /// Security protection - flash access is restricted.
    /// Requires security key or special unlock sequence.
    /// </summary>
    SecurityLocked = 2,

    /// <summary>
    /// Permanent protection - flash cannot be modified.
    /// Protection cannot be removed (one-time programmable).
    /// </summary>
    PermanentlyLocked = 3
}
