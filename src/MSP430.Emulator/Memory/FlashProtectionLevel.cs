namespace MSP430.Emulator.Memory;

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
