using System;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the memory regions in the MSP430FR2355 address space.
/// 
/// This enum defines memory regions specific to the MSP430FR2355 microcontroller,
/// which features FRAM-based architecture instead of traditional Flash memory.
/// The FR2355 serves as the reference implementation for modern MSP430 FRAM devices.
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
    /// Bootstrap Loader FRAM (MSP430FR2355: 0x1000-0x17FF).
    /// Contains the bootstrap loader code in FRAM memory.
    /// </summary>
    BootstrapLoader,

    /// <summary>
    /// Unused address space (0x0A00-0x0FFF).
    /// </summary>
    Unused2,

    /// <summary>
    /// Information Memory FRAM (MSP430FR2355: 0x1800-0x19FF).
    /// Contains calibration data and device-specific information in FRAM.
    /// </summary>
    InformationMemory,

    /// <summary>
    /// Unused address space (0x1100-0x38FF).
    /// </summary>
    Unused3,

    /// <summary>
    /// SRAM (MSP430FR2355: 0x2000-0x2FFF).
    /// High-speed volatile memory for program variables and stack.
    /// </summary>
    Ram,

    /// <summary>
    /// FRAM Memory (MSP430FR2355: 0x4000-0xBFFF).
    /// Non-volatile ferroelectric memory for unified code and data storage.
    /// Supports byte-level write operations unlike traditional Flash.
    /// 
    /// NOTE: This enum value is named "Flash" for historical reasons but represents
    /// FRAM memory in MSP430FR2355. FRAM has different characteristics than Flash:
    /// - Byte-level write capability (vs page/block writes in Flash)
    /// - No erase cycles required
    /// - Faster write operations
    /// - Different wait state behavior
    /// 
    /// This naming inconsistency should be addressed in a future major version.
    /// See TEST_REVIEW_ISSUES.md for details.
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
