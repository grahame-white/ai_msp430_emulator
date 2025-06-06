namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the state of the flash memory controller.
/// 
/// The flash controller follows a state machine pattern for secure
/// flash operations, preventing accidental writes or erases.
/// 
/// State transitions based on MSP430x2xx Family User's Guide (SLAU144J) - Section 5.3.2:
/// "Flash Memory Control Register (FCTL3)" - Lock bit and protection mechanisms.
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
