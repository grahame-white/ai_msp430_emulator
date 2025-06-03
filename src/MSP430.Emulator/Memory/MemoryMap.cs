namespace MSP430.Emulator.Memory;

/// <summary>
/// Implementation of the MSP430FR2355 memory map with unified 16-bit address space.
/// 
/// This implementation is specifically designed for the MSP430FR2355 mixed-signal microcontroller,
/// featuring FRAM-based non-volatile memory architecture. The FR2355 was chosen as the reference
/// implementation to represent modern MSP430 FRAM devices with their unified memory model.
/// 
/// Key characteristics of MSP430FR2355:
/// - 32KB FRAM (Ferroelectric RAM) for non-volatile code/data storage
/// - 4KB SRAM for high-speed volatile operations  
/// - FRAM allows byte-level write access unlike traditional Flash
/// - Von Neumann architecture with unified code/data address space
/// 
/// Future developers: This memory layout is specifically for MSP430FR2355. Other MSP430 variants
/// can be supported using the constructor with custom memory regions.
/// </summary>
public class MemoryMap : IMemoryMap
{
    private readonly Dictionary<MemoryRegion, MemoryRegionInfo> _regionLookup;
    private readonly MemoryRegionInfo[] _addressLookup;

    /// <summary>
    /// Initializes a new instance of the MemoryMap class with default MSP430FR2355 memory layout.
    /// </summary>
    public MemoryMap()
    {
        var regions = CreateDefaultRegions();
        _regionLookup = regions.ToDictionary(r => r.Region, r => r);
        _addressLookup = new MemoryRegionInfo[65536]; // 16-bit address space

        // Build address lookup table for fast address-to-region mapping
        BuildAddressLookupTable(regions);
    }

    /// <summary>
    /// Initializes a new instance of the MemoryMap class with custom memory regions.
    /// </summary>
    /// <param name="customRegions">Custom memory regions to use instead of defaults.</param>
    /// <exception cref="ArgumentNullException">Thrown when customRegions is null.</exception>
    /// <exception cref="ArgumentException">Thrown when customRegions contains overlapping regions.</exception>
    public MemoryMap(IEnumerable<MemoryRegionInfo> customRegions)
    {
        ArgumentNullException.ThrowIfNull(customRegions);

        var regions = customRegions.ToList();
        ValidateRegions(regions);

        _regionLookup = regions.ToDictionary(r => r.Region, r => r);
        _addressLookup = new MemoryRegionInfo[65536]; // 16-bit address space

        BuildAddressLookupTable(regions);
    }

    /// <inheritdoc />
    public IReadOnlyList<MemoryRegionInfo> Regions => _regionLookup.Values.ToList().AsReadOnly();

    /// <inheritdoc />
    public MemoryRegionInfo GetRegion(ushort address)
    {
        var region = _addressLookup[address];
        if (region.Equals(default(MemoryRegionInfo)))
        {
            throw new ArgumentException($"Address 0x{address:X4} is not mapped to any memory region", nameof(address));
        }
        return region;
    }

    /// <inheritdoc />
    public MemoryRegionInfo GetRegionInfo(MemoryRegion region)
    {
        if (!_regionLookup.TryGetValue(region, out var regionInfo))
        {
            throw new ArgumentException($"Memory region {region} is not defined in this memory map", nameof(region));
        }
        return regionInfo;
    }

    /// <inheritdoc />
    public bool IsValidAddress(ushort address)
    {
        return !_addressLookup[address].Equals(default(MemoryRegionInfo));
    }

    /// <inheritdoc />
    public bool IsAccessAllowed(ushort address, MemoryAccessPermissions accessType)
    {
        if (!IsValidAddress(address))
            return false;

        var region = _addressLookup[address];
        return (region.Permissions & accessType) == accessType;
    }

    /// <inheritdoc />
    public MemoryAccessPermissions GetPermissions(ushort address)
    {
        var region = GetRegion(address);
        return region.Permissions;
    }

    /// <summary>
    /// Creates the default MSP430FR2355 memory regions.
    /// 
    /// This implementation is based on the MSP430FR2355 microcontroller specifications:
    /// - FRAM-based architecture with unified code/data memory
    /// - 4KB SRAM for high-speed data operations
    /// - 32KB FRAM for non-volatile program and data storage
    /// - Enhanced bootstrap loader and information memory
    /// 
    /// Reference: MSP430FR2355 Mixed-Signal Microcontroller
    /// </summary>
    /// <returns>A list of default memory regions for the MSP430FR2355.</returns>
    private static List<MemoryRegionInfo> CreateDefaultRegions()
    {
        return new List<MemoryRegionInfo>
        {
            new(MemoryRegion.SpecialFunctionRegisters, 0x0000, 0x00FF, 
                MemoryAccessPermissions.ReadWrite, 
                "Special Function Registers"),
            
            new(MemoryRegion.Peripherals8Bit, 0x0100, 0x01FF, 
                MemoryAccessPermissions.ReadWrite, 
                "8-bit Peripherals"),
            
            new(MemoryRegion.Peripherals16Bit, 0x0200, 0x027F, 
                MemoryAccessPermissions.ReadWrite, 
                "16-bit Peripherals"),
            
            new(MemoryRegion.BootstrapLoader, 0x1000, 0x17FF, 
                MemoryAccessPermissions.ReadExecute, 
                "Bootstrap Loader FRAM"),
            
            new(MemoryRegion.InformationMemory, 0x1800, 0x19FF, 
                MemoryAccessPermissions.ReadWrite, 
                "Information Memory FRAM"),
            
            new(MemoryRegion.Ram, 0x2000, 0x2FFF, 
                MemoryAccessPermissions.All, 
                "SRAM - High-speed volatile memory"),
            
            new(MemoryRegion.Flash, 0x4000, 0xBFFF, 
                MemoryAccessPermissions.All, 
                "FRAM - Non-volatile unified code/data memory"),
            
            new(MemoryRegion.InterruptVectorTable, 0xFFE0, 0xFFFF, 
                MemoryAccessPermissions.ReadExecute, 
                "Interrupt Vector Table")
        };
    }

    /// <summary>
    /// Builds the address lookup table for fast address-to-region mapping.
    /// </summary>
    /// <param name="regions">The memory regions to map.</param>
    private void BuildAddressLookupTable(List<MemoryRegionInfo> regions)
    {
        // Initialize all addresses as unmapped
        for (int i = 0; i < _addressLookup.Length; i++)
        {
            _addressLookup[i] = default(MemoryRegionInfo);
        }

        // Map each region's address range
        foreach (var region in regions)
        {
            for (int address = region.StartAddress; address <= region.EndAddress; address++)
            {
                _addressLookup[address] = region;
            }
        }
    }

    /// <summary>
    /// Validates that the memory regions do not overlap and cover valid address ranges.
    /// </summary>
    /// <param name="regions">The memory regions to validate.</param>
    /// <exception cref="ArgumentException">Thrown when regions are invalid or overlapping.</exception>
    private static void ValidateRegions(List<MemoryRegionInfo> regions)
    {
        // Check for invalid regions
        foreach (var region in regions)
        {
            if (region.StartAddress > region.EndAddress)
            {
                throw new ArgumentException($"Invalid region {region.Region}: start address 0x{region.StartAddress:X4} is greater than end address 0x{region.EndAddress:X4}");
            }
        }

        // Check for overlapping regions
        var sortedRegions = regions.OrderBy(r => r.StartAddress).ToList();
        for (int i = 0; i < sortedRegions.Count - 1; i++)
        {
            var current = sortedRegions[i];
            var next = sortedRegions[i + 1];

            if (current.EndAddress >= next.StartAddress)
            {
                throw new ArgumentException($"Overlapping memory regions: {current.Region} (0x{current.StartAddress:X4}-0x{current.EndAddress:X4}) overlaps with {next.Region} (0x{next.StartAddress:X4}-0x{next.EndAddress:X4})");
            }
        }
    }
}