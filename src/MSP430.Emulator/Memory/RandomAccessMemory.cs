using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Implementation of random access memory for the MSP430 emulator.
/// 
/// This class provides byte and word-level access to RAM with proper timing
/// characteristics and bounds checking typical of MSP430 devices.
/// Supports configurable memory sizes from 512B to 10KB.
/// </summary>
public class RandomAccessMemory : IRandomAccessMemory
{
    private readonly byte[] _memory;
    private readonly ILogger? _logger;

    /// <summary>
    /// Minimum allowed memory size in bytes (512 bytes).
    /// </summary>
    public const int MinimumSize = 512;

    /// <summary>
    /// Maximum allowed memory size in bytes (10 KB).
    /// </summary>
    public const int MaximumSize = 10 * 1024;

    /// <summary>
    /// Default CPU cycles for RAM read operations.
    /// </summary>
    public const uint DefaultReadCycles = 1;

    /// <summary>
    /// Default CPU cycles for RAM write operations.
    /// </summary>
    public const uint DefaultWriteCycles = 1;

    /// <summary>
    /// Initializes a new instance of the RandomAccessMemory class.
    /// </summary>
    /// <param name="baseAddress">The base address where this memory is mapped.</param>
    /// <param name="size">The size of the memory in bytes (must be between 512B and 10KB).</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when size is outside the allowed range.</exception>
    /// <exception cref="ArgumentException">Thrown when the memory region would overflow the 16-bit address space.</exception>
    public RandomAccessMemory(ushort baseAddress, int size, ILogger? logger = null)
    {
        if (size < MinimumSize || size > MaximumSize)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size,
                $"Memory size must be between {MinimumSize} and {MaximumSize} bytes");
        }

        if ((long)baseAddress + size > 0x10000)
        {
            throw new ArgumentException("Memory region would exceed 16-bit address space", nameof(baseAddress));
        }

        BaseAddress = baseAddress;
        Size = size;
        _memory = new byte[size];
        _logger = logger;

        _logger?.Debug($"RandomAccessMemory initialized: Base=0x{baseAddress:X4}, Size={size} bytes");
    }

    /// <inheritdoc />
    public int Size { get; }

    /// <inheritdoc />
    public ushort BaseAddress { get; }

    /// <inheritdoc />
    public ushort EndAddress => (ushort)(BaseAddress + Size - 1);

    /// <inheritdoc />
    public byte ReadByte(ushort address)
    {
        ValidateAddress(address);
        int offset = address - BaseAddress;
        byte value = _memory[offset];

        _logger?.Debug($"RAM ReadByte: Address=0x{address:X4}, Value=0x{value:X2}");
        return value;
    }

    /// <inheritdoc />
    public void WriteByte(ushort address, byte value)
    {
        ValidateAddress(address);
        int offset = address - BaseAddress;
        _memory[offset] = value;

        _logger?.Debug($"RAM WriteByte: Address=0x{address:X4}, Value=0x{value:X2}");
    }

    /// <inheritdoc />
    public ushort ReadWord(ushort address)
    {
        ValidateWordAddress(address);
        ValidateAddress((ushort)(address + 1)); // Ensure both bytes are in bounds

        int offset = address - BaseAddress;
        // Little-endian: low byte at address, high byte at address+1
        ushort value = (ushort)(_memory[offset] | (_memory[offset + 1] << 8));

        _logger?.Debug($"RAM ReadWord: Address=0x{address:X4}, Value=0x{value:X4}");
        return value;
    }

    /// <inheritdoc />
    public void WriteWord(ushort address, ushort value)
    {
        ValidateWordAddress(address);
        ValidateAddress((ushort)(address + 1)); // Ensure both bytes are in bounds

        int offset = address - BaseAddress;
        // Little-endian: low byte at address, high byte at address+1
        _memory[offset] = (byte)(value & 0xFF);
        _memory[offset + 1] = (byte)((value >> 8) & 0xFF);

        _logger?.Debug($"RAM WriteWord: Address=0x{address:X4}, Value=0x{value:X4}");
    }

    /// <inheritdoc />
    public void Clear()
    {
        Array.Clear(_memory, 0, _memory.Length);
        _logger?.Debug($"RAM cleared: {Size} bytes set to 0x00");
    }

    /// <inheritdoc />
    public void Initialize(byte pattern = 0x00)
    {
        Array.Fill(_memory, pattern);
        _logger?.Debug($"RAM initialized: {Size} bytes set to 0x{pattern:X2}");
    }

    /// <inheritdoc />
    public uint GetReadCycles(ushort address, bool isWordAccess = false)
    {
        // For RAM, access time is constant regardless of address or access type
        // In real MSP430, RAM access is typically 1 cycle
        return DefaultReadCycles;
    }

    /// <inheritdoc />
    public uint GetWriteCycles(ushort address, bool isWordAccess = false)
    {
        // For RAM, access time is constant regardless of address or access type
        // In real MSP430, RAM access is typically 1 cycle
        return DefaultWriteCycles;
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
