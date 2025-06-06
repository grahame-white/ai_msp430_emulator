using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Manages flash memory operations and state transitions for the MSP430 emulator.
/// 
/// This class implements the flash controller state machine that manages
/// programming, erase operations, and protection mechanisms typical of MSP430 devices.
/// 
/// Implementation follows MSP430 flash controller specifications:
/// - MSP430x2xx Family User's Guide (SLAU144J, February 2007 - Revised December 2013)
///   Section 5.3: "Flash Memory Controller" - State machine and operation control
/// - MSP430x2xx Family User's Guide (SLAU144J) - Section 5.4: "Flash Memory Timing"
///   Table 5-1: "Flash Memory Timing Parameters" - Programming and erase cycle counts
/// </summary>
public class FlashController
{
    private readonly ILogger? _logger;
    private FlashControllerState _state;
    private FlashProtectionLevel _protectionLevel;
    private FlashOperation _currentOperation;
    private uint _operationCyclesRemaining;

    /// <summary>
    /// Standard unlock key for MSP430 flash operations (0xA500 + key byte).
    /// Based on MSP430x2xx Family User's Guide (SLAU144J) - Section 5.3.1: "Flash Memory Control Register (FCTL1)"
    /// </summary>
    public const ushort UnlockKeyBase = 0xA500;

    /// <summary>
    /// Valid unlock key byte values (any value 0x00-0xFF when combined with base).
    /// </summary>
    public const byte ValidUnlockKeyMask = 0xFF;

    /// <summary>
    /// CPU cycles required for sector erase operations (typical MSP430 timing).
    /// Based on MSP430x2xx Family User's Guide (SLAU144J) - Table 5-1: "Flash Memory Timing Parameters"
    /// </summary>
    public const uint SectorEraseCycles = 4819;

    /// <summary>
    /// CPU cycles required for mass erase operations (typical MSP430 timing).
    /// Based on MSP430x2xx Family User's Guide (SLAU144J) - Table 5-1: "Flash Memory Timing Parameters"
    /// </summary>
    public const uint MassEraseCycles = 5297;

    /// <summary>
    /// CPU cycles required for byte programming operations (typical MSP430 timing).
    /// Based on MSP430x2xx Family User's Guide (SLAU144J) - Table 5-1: "Flash Memory Timing Parameters"
    /// </summary>
    public const uint ByteProgramCycles = 30;

    /// <summary>
    /// CPU cycles required for word programming operations (typical MSP430 timing).
    /// Based on MSP430x2xx Family User's Guide (SLAU144J) - Table 5-1: "Flash Memory Timing Parameters"
    /// </summary>
    public const uint WordProgramCycles = 35;

    /// <summary>
    /// Initializes a new instance of the FlashController class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    public FlashController(ILogger? logger = null)
    {
        _logger = logger;
        _state = FlashControllerState.Locked;
        _protectionLevel = FlashProtectionLevel.None;
        _currentOperation = FlashOperation.None;
        _operationCyclesRemaining = 0;

        _logger?.Debug("FlashController initialized in locked state");
    }

    /// <summary>
    /// Gets the current state of the flash controller.
    /// </summary>
    public FlashControllerState State => _state;

    /// <summary>
    /// Gets the current protection level.
    /// </summary>
    public FlashProtectionLevel ProtectionLevel => _protectionLevel;

    /// <summary>
    /// Gets the current operation in progress.
    /// </summary>
    public FlashOperation CurrentOperation => _currentOperation;

    /// <summary>
    /// Gets a value indicating whether an operation is currently in progress.
    /// </summary>
    public bool IsOperationInProgress => _state == FlashControllerState.Programming ||
                                       _state == FlashControllerState.Erasing;

    /// <summary>
    /// Gets the number of CPU cycles remaining for the current operation.
    /// </summary>
    public uint OperationCyclesRemaining => _operationCyclesRemaining;

    /// <summary>
    /// Attempts to unlock the flash controller with the specified key.
    /// </summary>
    /// <param name="unlockKey">The unlock key (must match MSP430 format: 0xA5xx).</param>
    /// <returns>True if unlock was successful, false otherwise.</returns>
    public bool TryUnlock(ushort unlockKey)
    {
        // MSP430 flash unlock key format: 0xA5xx where xx can be any value
        // Per SLAU144J Section 5.3.1: "Flash Memory Control Register (FCTL1)" - unlock key requirement
        if ((unlockKey & 0xFF00) != UnlockKeyBase)
        {
            _logger?.Warning($"Invalid unlock key format: 0x{unlockKey:X4} (expected 0xA5xx)");
            return false;
        }

        // Check if protected
        if (_protectionLevel == FlashProtectionLevel.SecurityLocked ||
            _protectionLevel == FlashProtectionLevel.PermanentlyLocked)
        {
            _logger?.Warning($"Cannot unlock: protection level is {_protectionLevel}");
            return false;
        }

        if (_state != FlashControllerState.Locked)
        {
            _logger?.Warning($"Flash controller is not in locked state (current: {_state})");
            return false;
        }

        TransitionTo(FlashControllerState.Unlocked);
        _logger?.Debug($"Flash controller unlocked with key 0x{unlockKey:X4}");
        return true;
    }

    /// <summary>
    /// Locks the flash controller, preventing further operations.
    /// </summary>
    public void Lock()
    {
        if (_state == FlashControllerState.Programming || _state == FlashControllerState.Erasing)
        {
            _logger?.Warning("Cannot lock while operation is in progress");
            return;
        }

        TransitionTo(FlashControllerState.Locked);
        _currentOperation = FlashOperation.None;
        _operationCyclesRemaining = 0;
        _logger?.Debug("Flash controller locked");
    }

    /// <summary>
    /// Initiates a programming operation.
    /// </summary>
    /// <param name="isWordAccess">True for word programming, false for byte programming.</param>
    /// <returns>True if programming was initiated successfully, false otherwise.</returns>
    public bool StartProgramming(bool isWordAccess)
    {
        if (!CanStartOperation())
        {
            return false;
        }

        _currentOperation = FlashOperation.Program;
        _operationCyclesRemaining = isWordAccess ? WordProgramCycles : ByteProgramCycles;
        TransitionTo(FlashControllerState.Programming);

        _logger?.Debug($"Programming operation started ({(isWordAccess ? "word" : "byte")}, {_operationCyclesRemaining} cycles)");
        return true;
    }

    /// <summary>
    /// Initiates a sector erase operation.
    /// </summary>
    /// <returns>True if erase was initiated successfully, false otherwise.</returns>
    public bool StartSectorErase()
    {
        if (!CanStartOperation())
        {
            return false;
        }

        _currentOperation = FlashOperation.SectorErase;
        _operationCyclesRemaining = SectorEraseCycles;
        TransitionTo(FlashControllerState.Erasing);

        _logger?.Debug($"Sector erase operation started ({_operationCyclesRemaining} cycles)");
        return true;
    }

    /// <summary>
    /// Initiates a mass erase operation.
    /// </summary>
    /// <returns>True if mass erase was initiated successfully, false otherwise.</returns>
    public bool StartMassErase()
    {
        if (!CanStartOperation())
        {
            return false;
        }

        _currentOperation = FlashOperation.MassErase;
        _operationCyclesRemaining = MassEraseCycles;
        TransitionTo(FlashControllerState.Erasing);

        _logger?.Debug($"Mass erase operation started ({_operationCyclesRemaining} cycles)");
        return true;
    }

    /// <summary>
    /// Updates the controller state by advancing the operation by the specified number of cycles.
    /// </summary>
    /// <param name="cycles">The number of CPU cycles that have elapsed.</param>
    public void Update(uint cycles)
    {
        if (!IsOperationInProgress)
        {
            return;
        }

        if (cycles >= _operationCyclesRemaining)
        {
            _operationCyclesRemaining = 0;
            CompleteOperation();
        }
        else
        {
            _operationCyclesRemaining -= cycles;
        }
    }

    /// <summary>
    /// Sets the protection level for the flash memory.
    /// </summary>
    /// <param name="protectionLevel">The desired protection level.</param>
    /// <returns>True if protection level was set successfully, false otherwise.</returns>
    public bool SetProtectionLevel(FlashProtectionLevel protectionLevel)
    {
        // Cannot change protection while operation is in progress
        if (IsOperationInProgress)
        {
            _logger?.Warning("Cannot change protection level while operation is in progress");
            return false;
        }

        // Cannot remove permanent protection
        if (_protectionLevel == FlashProtectionLevel.PermanentlyLocked)
        {
            _logger?.Warning("Cannot change protection level: permanently locked");
            return false;
        }

        FlashProtectionLevel oldLevel = _protectionLevel;
        _protectionLevel = protectionLevel;

        // Lock controller if protection is applied
        if (protectionLevel != FlashProtectionLevel.None && _state == FlashControllerState.Unlocked)
        {
            Lock();
        }

        _logger?.Debug($"Protection level changed from {oldLevel} to {protectionLevel}");
        return true;
    }

    /// <summary>
    /// Gets the CPU cycles required for the specified erase operation.
    /// </summary>
    /// <param name="operation">The erase operation type.</param>
    /// <returns>The number of CPU cycles required.</returns>
    public static uint GetEraseCycles(FlashOperation operation)
    {
        return operation switch
        {
            FlashOperation.SectorErase => SectorEraseCycles,
            FlashOperation.MassErase => MassEraseCycles,
            FlashOperation.SegmentErase => SectorEraseCycles / 8, // Smaller segments
            _ => 0
        };
    }

    /// <summary>
    /// Gets the CPU cycles required for programming operations.
    /// </summary>
    /// <param name="isWordAccess">True for word programming, false for byte programming.</param>
    /// <returns>The number of CPU cycles required.</returns>
    public static uint GetProgramCycles(bool isWordAccess)
    {
        return isWordAccess ? WordProgramCycles : ByteProgramCycles;
    }

    private bool CanStartOperation()
    {
        if (_state != FlashControllerState.Unlocked)
        {
            _logger?.Warning($"Cannot start operation: controller state is {_state}");
            return false;
        }

        if (_protectionLevel == FlashProtectionLevel.WriteProtected ||
            _protectionLevel == FlashProtectionLevel.SecurityLocked ||
            _protectionLevel == FlashProtectionLevel.PermanentlyLocked)
        {
            _logger?.Warning($"Cannot start operation: protection level is {_protectionLevel}");
            return false;
        }

        return true;
    }

    private void CompleteOperation()
    {
        FlashOperation operation = _currentOperation;
        _currentOperation = FlashOperation.None;
        TransitionTo(FlashControllerState.OperationComplete);

        _logger?.Debug($"{operation} operation completed");

        // Automatically return to unlocked state after completion
        TransitionTo(FlashControllerState.Unlocked);
    }

    private void TransitionTo(FlashControllerState newState)
    {
        FlashControllerState oldState = _state;
        _state = newState;
        _logger?.Debug($"Flash controller state transition: {oldState} -> {newState}");
    }
}
