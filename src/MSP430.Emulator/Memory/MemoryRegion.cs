namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the memory regions in the MSP430 address space.
/// </summary>
public enum MemoryRegion
{
    /// <summary>
    /// Special Function Registers (0x0000-0x00FF).
    /// Contains system control and status registers.
    /// </summary>
    SpecialFunctionRegisters,

    /// <summary>
    /// 8-bit Peripherals (0x0100-0x01FF).
    /// Memory-mapped 8-bit peripheral registers.
    /// </summary>
    Peripherals8Bit,

    /// <summary>
    /// 16-bit Peripherals (0x0200-0x027F).
    /// Memory-mapped 16-bit peripheral registers.
    /// </summary>
    Peripherals16Bit,

    /// <summary>
    /// Unused address space (0x0280-0x03FF).
    /// </summary>
    Unused1,

    /// <summary>
    /// Bootstrap Loader Flash (0x0400-0x09FF).
    /// Contains the bootstrap loader code.
    /// </summary>
    BootstrapLoader,

    /// <summary>
    /// Unused address space (0x0A00-0x0FFF).
    /// </summary>
    Unused2,

    /// <summary>
    /// Information Memory Flash (0x1000-0x10FF).
    /// Contains calibration data and device-specific information.
    /// </summary>
    InformationMemory,

    /// <summary>
    /// Unused address space (0x1100-0x38FF).
    /// </summary>
    Unused3,

    /// <summary>
    /// RAM (0x3900-0x3AFF).
    /// Random Access Memory for program variables and stack.
    /// </summary>
    Ram,

    /// <summary>
    /// Flash Memory (0x3B00-0xFFDF).
    /// Main program memory (Flash ROM).
    /// </summary>
    Flash,

    /// <summary>
    /// Interrupt Vector Table (0xFFE0-0xFFFF).
    /// Contains interrupt service routine addresses.
    /// </summary>
    InterruptVectorTable
}

/// <summary>
/// Defines memory access permissions.
/// </summary>
[Flags]
public enum MemoryAccessPermissions
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
    /// Execute access allowed (instruction fetch).
    /// </summary>
    Execute = 4,

    /// <summary>
    /// Read and write access allowed.
    /// </summary>
    ReadWrite = Read | Write,

    /// <summary>
    /// Read and execute access allowed.
    /// </summary>
    ReadExecute = Read | Execute,

    /// <summary>
    /// All access permissions allowed.
    /// </summary>
    All = Read | Write | Execute
}

/// <summary>
/// Provides information about a memory region.
/// </summary>
public readonly struct MemoryRegionInfo
{
    /// <summary>
    /// Initializes a new instance of the MemoryRegionInfo struct.
    /// </summary>
    /// <param name="region">The memory region.</param>
    /// <param name="startAddress">The starting address of the region.</param>
    /// <param name="endAddress">The ending address of the region (inclusive).</param>
    /// <param name="permissions">The access permissions for the region.</param>
    /// <param name="description">A description of the region.</param>
    public MemoryRegionInfo(MemoryRegion region, ushort startAddress, ushort endAddress, 
        MemoryAccessPermissions permissions, string description)
    {
        Region = region;
        StartAddress = startAddress;
        EndAddress = endAddress;
        Permissions = permissions;
        Description = description;
    }

    /// <summary>
    /// Gets the memory region.
    /// </summary>
    public MemoryRegion Region { get; }

    /// <summary>
    /// Gets the starting address of the region.
    /// </summary>
    public ushort StartAddress { get; }

    /// <summary>
    /// Gets the ending address of the region (inclusive).
    /// </summary>
    public ushort EndAddress { get; }

    /// <summary>
    /// Gets the access permissions for the region.
    /// </summary>
    public MemoryAccessPermissions Permissions { get; }

    /// <summary>
    /// Gets the description of the region.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the size of the region in bytes.
    /// </summary>
    public int Size => EndAddress - StartAddress + 1;

    /// <summary>
    /// Determines if the specified address is within this region.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>True if the address is within the region, false otherwise.</returns>
    public bool Contains(ushort address) => address >= StartAddress && address <= EndAddress;
}