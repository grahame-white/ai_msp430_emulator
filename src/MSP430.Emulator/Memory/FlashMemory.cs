using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Implementation of flash memory for the MSP430 emulator.
/// 
/// This class provides byte and word-level access to flash memory with proper timing
/// characteristics, programming/erase operations, and protection mechanisms
/// typical of MSP430 devices.
/// </summary>
public class FlashMemory : IFlashMemory
{
    private readonly byte[] _memory;
    private readonly FlashController _controller;
    private readonly ILogger? _logger;

    /// <summary>
    /// Minimum allowed flash memory size in bytes (1 KB).
    /// </summary>
    public const int MinimumSize = 1024;

    /// <summary>
    /// Maximum allowed flash memory size in bytes (256 KB).
    /// </summary>
    public const int MaximumSize = 256 * 1024;

    /// <summary>
    /// Standard sector size for MSP430 flash memory (512 bytes).
    /// </summary>
    public const int StandardSectorSize = 512;

    /// <summary>
    /// Default CPU cycles for flash read operations.
    /// Flash reads are typically slower than RAM reads.
    /// </summary>
    public const uint DefaultReadCycles = 3;

    /// <summary>
    /// Erased flash memory pattern (all bits set to 1).
    /// </summary>
    public const byte ErasedPattern = 0xFF;

    /// <summary>
    /// Initializes a new instance of the FlashMemory class.
    /// </summary>
    /// <param name="baseAddress">The base address where this flash memory is mapped.</param>
    /// <param name="size">The size of the flash memory in bytes.</param>
    /// <param name="sectorSize">The sector size in bytes (default is 512 bytes).</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when size is outside the allowed range.</exception>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    public FlashMemory(ushort baseAddress, int size, int sectorSize = StandardSectorSize, ILogger? logger = null)
    {
        if (size < MinimumSize || size > MaximumSize)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size,
                $"Flash memory size must be between {MinimumSize} and {MaximumSize} bytes");
        }

        if (sectorSize <= 0 || (size % sectorSize) != 0)
        {
            throw new ArgumentException($"Sector size must be positive and evenly divide total size", nameof(sectorSize));
        }

        if ((long)baseAddress + size > 0x10000)
        {
            throw new ArgumentException("Memory region would exceed 16-bit address space", nameof(baseAddress));
        }

        BaseAddress = baseAddress;
        Size = size;
        SectorSize = sectorSize;
        _memory = new byte[size];
        _controller = new FlashController(logger);
        _logger = logger;

        // Initialize flash to erased state (all 0xFF)
        Initialize();

        _logger?.Debug($"FlashMemory initialized: Base=0x{baseAddress:X4}, Size={size} bytes, SectorSize={sectorSize} bytes");
    }

    /// <inheritdoc />
    public int Size { get; }

    /// <inheritdoc />
    public ushort BaseAddress { get; }

    /// <inheritdoc />
    public ushort EndAddress => (ushort)(BaseAddress + Size - 1);

    /// <inheritdoc />
    public FlashProtectionLevel ProtectionLevel => _controller.ProtectionLevel;

    /// <inheritdoc />
    public FlashControllerState ControllerState => _controller.State;

    /// <inheritdoc />
    public bool IsOperationInProgress => _controller.IsOperationInProgress;

    /// <inheritdoc />
    public int SectorSize { get; }

    /// <inheritdoc />
    public byte ReadByte(ushort address)
    {
        ValidateAddress(address);
        int offset = address - BaseAddress;
        byte value = _memory[offset];

        _logger?.Debug($"Flash ReadByte: Address=0x{address:X4}, Value=0x{value:X2}");
        return value;
    }

    /// <inheritdoc />
    public ushort ReadWord(ushort address)
    {
        ValidateWordAddress(address);
        ValidateAddress((ushort)(address + 1)); // Ensure both bytes are in bounds

        int offset = address - BaseAddress;
        // Little-endian: low byte at address, high byte at address+1
        ushort value = (ushort)(_memory[offset] | (_memory[offset + 1] << 8));

        _logger?.Debug($"Flash ReadWord: Address=0x{address:X4}, Value=0x{value:X4}");
        return value;
    }

    /// <inheritdoc />
    public bool ProgramByte(ushort address, byte value)
    {
        ValidateAddress(address);

        if (!_controller.StartProgramming(isWordAccess: false))
        {
            _logger?.Warning($"Cannot program byte at 0x{address:X4}: controller rejected operation");
            return false;
        }

        int offset = address - BaseAddress;

        // Flash programming can only change 1 bits to 0 bits
        // Cannot change 0 bits to 1 bits without erase
        byte currentValue = _memory[offset];
        byte programmableValue = (byte)(currentValue & value);

        if (programmableValue != value)
        {
            _logger?.Warning($"Cannot program 0x{value:X2} at 0x{address:X4}: current value 0x{currentValue:X2} would require erasing bits");
            return false;
        }

        _memory[offset] = value;

        // Complete the programming operation immediately
        _controller.Update(FlashController.ByteProgramCycles);

        _logger?.Debug($"Flash ProgramByte: Address=0x{address:X4}, Value=0x{value:X2}");
        return true;
    }

    /// <inheritdoc />
    public bool ProgramWord(ushort address, ushort value)
    {
        ValidateWordAddress(address);
        ValidateAddress((ushort)(address + 1)); // Ensure both bytes are in bounds

        if (!_controller.StartProgramming(isWordAccess: true))
        {
            _logger?.Warning($"Cannot program word at 0x{address:X4}: controller rejected operation");
            return false;
        }

        int offset = address - BaseAddress;

        // Check if programming is possible for both bytes
        byte lowByte = (byte)(value & 0xFF);
        byte highByte = (byte)((value >> 8) & 0xFF);
        byte currentLow = _memory[offset];
        byte currentHigh = _memory[offset + 1];

        if ((currentLow & lowByte) != lowByte || (currentHigh & highByte) != highByte)
        {
            _logger?.Warning($"Cannot program 0x{value:X4} at 0x{address:X4}: would require erasing bits");
            return false;
        }

        // Little-endian: low byte at address, high byte at address+1
        _memory[offset] = lowByte;
        _memory[offset + 1] = highByte;

        // Complete the programming operation immediately
        _controller.Update(FlashController.WordProgramCycles);

        _logger?.Debug($"Flash ProgramWord: Address=0x{address:X4}, Value=0x{value:X4}");
        return true;
    }

    /// <inheritdoc />
    public bool EraseSector(ushort address)
    {
        ValidateAddress(address);

        if (!_controller.StartSectorErase())
        {
            _logger?.Warning($"Cannot erase sector containing 0x{address:X4}: controller rejected operation");
            return false;
        }

        ushort sectorBase = GetSectorBaseAddress(address);
        int offset = sectorBase - BaseAddress;

        // Set all bytes in sector to erased state (0xFF)
        Array.Fill(_memory, ErasedPattern, offset, SectorSize);

        // Complete the erase operation immediately
        _controller.Update(FlashController.SectorEraseCycles);

        _logger?.Debug($"Flash EraseSector: Address=0x{address:X4}, SectorBase=0x{sectorBase:X4}");
        return true;
    }

    /// <inheritdoc />
    public bool MassErase()
    {
        if (!_controller.StartMassErase())
        {
            _logger?.Warning("Cannot perform mass erase: controller rejected operation");
            return false;
        }

        // Set all bytes to erased state (0xFF)
        Array.Fill(_memory, ErasedPattern);

        // Complete the mass erase operation immediately
        _controller.Update(FlashController.MassEraseCycles);

        _logger?.Debug("Flash MassErase: All memory erased");
        return true;
    }

    /// <inheritdoc />
    public bool Unlock(ushort unlockKey)
    {
        return _controller.TryUnlock(unlockKey);
    }

    /// <inheritdoc />
    public void Lock()
    {
        _controller.Lock();
    }

    /// <inheritdoc />
    public bool SetProtectionLevel(FlashProtectionLevel protectionLevel)
    {
        return _controller.SetProtectionLevel(protectionLevel);
    }

    /// <inheritdoc />
    public void Clear()
    {
        Array.Clear(_memory, 0, _memory.Length);
        _logger?.Debug($"Flash cleared: {Size} bytes set to 0x00");
    }

    /// <inheritdoc />
    public void Initialize(byte pattern = ErasedPattern)
    {
        Array.Fill(_memory, pattern);
        _logger?.Debug($"Flash initialized: {Size} bytes set to 0x{pattern:X2}");
    }

    /// <inheritdoc />
    public uint GetReadCycles(ushort address, bool isWordAccess = false)
    {
        // Flash reads are typically slower than RAM reads
        return DefaultReadCycles;
    }

    /// <inheritdoc />
    public uint GetProgramCycles(ushort address, bool isWordAccess = false)
    {
        return FlashController.GetProgramCycles(isWordAccess);
    }

    /// <inheritdoc />
    public uint GetEraseCycles(FlashOperation operation)
    {
        return FlashController.GetEraseCycles(operation);
    }

    /// <inheritdoc />
    public bool IsAddressInBounds(ushort address)
    {
        return address >= BaseAddress && address <= EndAddress;
    }

    /// <inheritdoc />
    public bool IsRangeInBounds(ushort startAddress, int length)
    {
        if (length <= 0)
        {
            return false;
        }

        // Check for overflow in the range calculation
        if ((long)startAddress + length - 1 > ushort.MaxValue)
        {
            return false;
        }

        ushort endAddress = (ushort)(startAddress + length - 1);
        return IsAddressInBounds(startAddress) && IsAddressInBounds(endAddress);
    }

    /// <inheritdoc />
    public int GetSectorNumber(ushort address)
    {
        ValidateAddress(address);
        int offset = address - BaseAddress;
        return offset / SectorSize;
    }

    /// <inheritdoc />
    public ushort GetSectorBaseAddress(ushort address)
    {
        ValidateAddress(address);
        int sectorNumber = GetSectorNumber(address);
        return (ushort)(BaseAddress + (sectorNumber * SectorSize));
    }

    /// <summary>
    /// Updates the flash controller state by advancing operations by the specified number of cycles.
    /// This should be called periodically to simulate flash operation timing.
    /// </summary>
    /// <param name="cycles">The number of CPU cycles that have elapsed.</param>
    public void Update(uint cycles)
    {
        _controller.Update(cycles);
    }

    private void ValidateAddress(ushort address)
    {
        if (!IsAddressInBounds(address))
        {
            throw new ArgumentOutOfRangeException(nameof(address), address,
                $"Address 0x{address:X4} is outside memory bounds (0x{BaseAddress:X4}-0x{EndAddress:X4})");
        }
    }

    private void ValidateWordAddress(ushort address)
    {
        if ((address & 0x1) != 0)
        {
            throw new ArgumentException($"Address 0x{address:X4} is not word-aligned (must be even)", nameof(address));
        }
    }
}
