using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Validates memory access operations against the memory map.
/// </summary>
public class MemoryAccessValidator
{
    private readonly IMemoryMap _memoryMap;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the MemoryAccessValidator class.
    /// </summary>
    /// <param name="memoryMap">The memory map to validate against.</param>
    /// <param name="logger">Optional logger for access violations and debugging.</param>
    /// <exception cref="ArgumentNullException">Thrown when memoryMap is null.</exception>
    public MemoryAccessValidator(IMemoryMap memoryMap, ILogger? logger = null)
    {
        _memoryMap = memoryMap ?? throw new ArgumentNullException(nameof(memoryMap));
        _logger = logger;
    }

    /// <summary>
    /// Validates a read access to the specified address.
    /// </summary>
    /// <param name="address">The memory address to read from.</param>
    /// <exception cref="MemoryAccessException">Thrown when the read access is not allowed.</exception>
    public void ValidateRead(ushort address)
    {
        ValidateAccess(address, MemoryAccessPermissions.Read, "read");
    }

    /// <summary>
    /// Validates a write access to the specified address.
    /// </summary>
    /// <param name="address">The memory address to write to.</param>
    /// <exception cref="MemoryAccessException">Thrown when the write access is not allowed.</exception>
    public void ValidateWrite(ushort address)
    {
        ValidateAccess(address, MemoryAccessPermissions.Write, "write");
    }

    /// <summary>
    /// Validates an execute access (instruction fetch) to the specified address.
    /// </summary>
    /// <param name="address">The memory address to execute from.</param>
    /// <exception cref="MemoryAccessException">Thrown when the execute access is not allowed.</exception>
    public void ValidateExecute(ushort address)
    {
        ValidateAccess(address, MemoryAccessPermissions.Execute, "execute");
    }

    /// <summary>
    /// Validates a memory access of the specified type to the given address.
    /// </summary>
    /// <param name="address">The memory address.</param>
    /// <param name="accessType">The type of access to validate.</param>
    /// <exception cref="MemoryAccessException">Thrown when the access is not allowed.</exception>
    public void ValidateAccess(ushort address, MemoryAccessPermissions accessType)
    {
        string accessTypeName = accessType switch
        {
            MemoryAccessPermissions.Read => "read",
            MemoryAccessPermissions.Write => "write",
            MemoryAccessPermissions.Execute => "execute",
            _ => accessType.ToString().ToLowerInvariant()
        };

        ValidateAccess(address, accessType, accessTypeName);
    }

    /// <summary>
    /// Checks if a memory access is valid without throwing an exception.
    /// </summary>
    /// <param name="address">The memory address.</param>
    /// <param name="accessType">The type of access to check.</param>
    /// <returns>True if the access is valid, false otherwise.</returns>
    public bool IsAccessValid(ushort address, MemoryAccessPermissions accessType)
    {
        try
        {
            if (!_memoryMap.IsValidAddress(address))
            {
                return false;
            }

            return _memoryMap.IsAccessAllowed(address, accessType);
        }
        catch (ArgumentException)
        {
            // Address validation can throw ArgumentException
            return false;
        }
    }

    /// <summary>
    /// Gets access validation information for the specified address.
    /// </summary>
    /// <param name="address">The memory address.</param>
    /// <returns>Information about the access validation for the address.</returns>
    public MemoryAccessValidationInfo GetValidationInfo(ushort address)
    {
        if (!_memoryMap.IsValidAddress(address))
        {
            return new MemoryAccessValidationInfo(address, false, MemoryAccessPermissions.None, null);
        }

        var region = _memoryMap.GetRegion(address);
        var permissions = _memoryMap.GetPermissions(address);
        
        return new MemoryAccessValidationInfo(address, true, permissions, region);
    }

    private void ValidateAccess(ushort address, MemoryAccessPermissions accessType, string accessTypeName)
    {
        // Check if address is valid
        if (!_memoryMap.IsValidAddress(address))
        {
            var message = $"Invalid memory address 0x{address:X4} for {accessTypeName} access - address not mapped";
            _logger?.Warning($"Memory access violation: {message}");
            throw new MemoryAccessException(address, accessType, message);
        }

        // Check if access is allowed
        if (!_memoryMap.IsAccessAllowed(address, accessType))
        {
            var region = _memoryMap.GetRegion(address);
            var message = $"Access denied for {accessTypeName} at address 0x{address:X4} in {region.Region} region - insufficient permissions";
            _logger?.Warning($"Memory access violation: {message}", new { Address = address, Region = region.Region, RequestedAccess = accessType, AllowedPermissions = region.Permissions });
            throw new MemoryAccessException(address, accessType, message);
        }

        // Log successful access if debug logging is enabled
        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            var region = _memoryMap.GetRegion(address);
            _logger.Debug($"Memory {accessTypeName} access validated for address 0x{address:X4} in {region.Region} region");
        }
    }
}

/// <summary>
/// Contains information about memory access validation.
/// </summary>
public readonly struct MemoryAccessValidationInfo
{
    /// <summary>
    /// Initializes a new instance of the MemoryAccessValidationInfo struct.
    /// </summary>
    /// <param name="address">The memory address.</param>
    /// <param name="isValid">Whether the address is valid.</param>
    /// <param name="permissions">The access permissions for the address.</param>
    /// <param name="region">The memory region information, if the address is valid.</param>
    public MemoryAccessValidationInfo(ushort address, bool isValid, MemoryAccessPermissions permissions, MemoryRegionInfo? region)
    {
        Address = address;
        IsValid = isValid;
        Permissions = permissions;
        Region = region;
    }

    /// <summary>
    /// Gets the memory address.
    /// </summary>
    public ushort Address { get; }

    /// <summary>
    /// Gets a value indicating whether the address is valid.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the access permissions for the address.
    /// </summary>
    public MemoryAccessPermissions Permissions { get; }

    /// <summary>
    /// Gets the memory region information, if the address is valid.
    /// </summary>
    public MemoryRegionInfo? Region { get; }
}