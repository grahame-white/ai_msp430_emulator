namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the contract for memory mapping in the MSP430 emulator.
/// </summary>
public interface IMemoryMap
{
    /// <summary>
    /// Gets all memory regions defined in the memory map.
    /// </summary>
    IReadOnlyList<MemoryRegionInfo> Regions { get; }

    /// <summary>
    /// Gets the memory region that contains the specified address.
    /// </summary>
    /// <param name="address">The memory address to look up.</param>
    /// <returns>The memory region information for the address.</returns>
    /// <exception cref="ArgumentException">Thrown when the address is not mapped to any region.</exception>
    MemoryRegionInfo GetRegion(ushort address);

    /// <summary>
    /// Gets the memory region information for the specified region type.
    /// </summary>
    /// <param name="region">The memory region type.</param>
    /// <returns>The memory region information.</returns>
    /// <exception cref="ArgumentException">Thrown when the region is not defined in the memory map.</exception>
    MemoryRegionInfo GetRegionInfo(MemoryRegion region);

    /// <summary>
    /// Determines if the specified address is valid (mapped to a region).
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>True if the address is valid, false otherwise.</returns>
    bool IsValidAddress(ushort address);

    /// <summary>
    /// Determines if the specified access is allowed for the given address.
    /// </summary>
    /// <param name="address">The memory address.</param>
    /// <param name="accessType">The type of access requested.</param>
    /// <returns>True if the access is allowed, false otherwise.</returns>
    bool IsAccessAllowed(ushort address, MemoryAccessPermissions accessType);

    /// <summary>
    /// Gets the access permissions for the specified address.
    /// </summary>
    /// <param name="address">The memory address.</param>
    /// <returns>The access permissions for the address.</returns>
    /// <exception cref="ArgumentException">Thrown when the address is not mapped to any region.</exception>
    MemoryAccessPermissions GetPermissions(ushort address);
}