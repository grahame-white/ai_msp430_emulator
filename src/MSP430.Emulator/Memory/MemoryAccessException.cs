namespace MSP430.Emulator.Memory;

/// <summary>
/// Exception thrown when an invalid memory access is attempted.
/// </summary>
public class MemoryAccessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the MemoryAccessException class.
    /// </summary>
    /// <param name="address">The memory address where the access violation occurred.</param>
    /// <param name="accessType">The type of access that was attempted.</param>
    /// <param name="message">The error message.</param>
    public MemoryAccessException(ushort address, MemoryAccessPermissions accessType, string message)
        : base(message)
    {
        Address = address;
        AccessType = accessType;
    }

    /// <summary>
    /// Initializes a new instance of the MemoryAccessException class.
    /// </summary>
    /// <param name="address">The memory address where the access violation occurred.</param>
    /// <param name="accessType">The type of access that was attempted.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MemoryAccessException(ushort address, MemoryAccessPermissions accessType, string message, Exception innerException)
        : base(message, innerException)
    {
        Address = address;
        AccessType = accessType;
    }

    /// <summary>
    /// Gets the memory address where the access violation occurred.
    /// </summary>
    public ushort Address { get; }

    /// <summary>
    /// Gets the type of access that was attempted.
    /// </summary>
    public MemoryAccessPermissions AccessType { get; }
}