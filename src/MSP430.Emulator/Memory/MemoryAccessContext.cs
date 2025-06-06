using System;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Provides context information for memory access operations.
/// 
/// This class encapsulates details about a memory access request including
/// the address, access type, data size, and operation context. It is used
/// by the memory controller to make routing and arbitration decisions.
/// </summary>
public class MemoryAccessContext
{
    /// <summary>
    /// Initializes a new instance of the MemoryAccessContext class.
    /// </summary>
    /// <param name="address">The memory address being accessed.</param>
    /// <param name="accessType">The type of access (read, write, execute).</param>
    /// <param name="isWordAccess">True if accessing a 16-bit word, false for 8-bit byte access.</param>
    /// <param name="operationId">Optional unique identifier for the operation.</param>
    public MemoryAccessContext(ushort address, MemoryAccessPermissions accessType, bool isWordAccess = false, Guid? operationId = null)
    {
        Address = address;
        AccessType = accessType;
        IsWordAccess = isWordAccess;
        OperationId = operationId ?? Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the memory address being accessed.
    /// </summary>
    public ushort Address { get; }

    /// <summary>
    /// Gets the type of memory access being performed.
    /// </summary>
    public MemoryAccessPermissions AccessType { get; }

    /// <summary>
    /// Gets a value indicating whether this is a word (16-bit) access.
    /// If false, this is a byte (8-bit) access.
    /// </summary>
    public bool IsWordAccess { get; }

    /// <summary>
    /// Gets the size of the access in bytes (1 for byte access, 2 for word access).
    /// </summary>
    public int AccessSize => IsWordAccess ? 2 : 1;

    /// <summary>
    /// Gets the end address of the access (inclusive).
    /// For byte access, this equals the start address.
    /// For word access, this is start address + 1.
    /// </summary>
    public ushort EndAddress => (ushort)(Address + AccessSize - 1);

    /// <summary>
    /// Gets a unique identifier for this memory operation.
    /// </summary>
    public Guid OperationId { get; }

    /// <summary>
    /// Gets the timestamp when this context was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets additional properties for extensibility.
    /// </summary>
    public System.Collections.Generic.Dictionary<string, object> Properties { get; } = new();

    /// <summary>
    /// Validates that the access is properly aligned and within bounds.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when word access is not properly aligned.</exception>
    public void ValidateAlignment()
    {
        if (IsWordAccess && (Address & 0x1) != 0)
        {
            throw new ArgumentException($"Word access at address 0x{Address:X4} is not properly aligned (must be even)", nameof(Address));
        }
    }

    /// <summary>
    /// Determines if this access overlaps with another address range.
    /// </summary>
    /// <param name="otherStart">The start address of the other range.</param>
    /// <param name="otherEnd">The end address of the other range (inclusive).</param>
    /// <returns>True if the ranges overlap, false otherwise.</returns>
    public bool OverlapsWith(ushort otherStart, ushort otherEnd)
    {
        return !(EndAddress < otherStart || Address > otherEnd);
    }

    /// <summary>
    /// Determines if this access overlaps with another memory access context.
    /// </summary>
    /// <param name="other">The other memory access context.</param>
    /// <returns>True if the accesses overlap, false otherwise.</returns>
    public bool OverlapsWith(MemoryAccessContext other)
    {
        if (other == null)
        {
            return false;
        }

        return OverlapsWith(other.Address, other.EndAddress);
    }

    /// <summary>
    /// Returns a string representation of the memory access context.
    /// </summary>
    /// <returns>A string describing the memory access.</returns>
    public override string ToString()
    {
        string accessTypeStr = AccessType.ToString();
        string sizeStr = IsWordAccess ? "Word" : "Byte";

        if (IsWordAccess)
        {
            return $"{accessTypeStr} {sizeStr} at 0x{Address:X4}-0x{EndAddress:X4} (Op: {OperationId:D})";
        }
        else
        {
            return $"{accessTypeStr} {sizeStr} at 0x{Address:X4} (Op: {OperationId:D})";
        }
    }

    /// <summary>
    /// Creates a context for a byte read operation.
    /// </summary>
    /// <param name="address">The address to read from.</param>
    /// <returns>A new MemoryAccessContext for the read operation.</returns>
    public static MemoryAccessContext CreateReadByte(ushort address)
    {
        return new MemoryAccessContext(address, MemoryAccessPermissions.Read, isWordAccess: false);
    }

    /// <summary>
    /// Creates a context for a word read operation.
    /// </summary>
    /// <param name="address">The address to read from (must be word-aligned).</param>
    /// <returns>A new MemoryAccessContext for the read operation.</returns>
    public static MemoryAccessContext CreateReadWord(ushort address)
    {
        return new MemoryAccessContext(address, MemoryAccessPermissions.Read, isWordAccess: true);
    }

    /// <summary>
    /// Creates a context for a byte write operation.
    /// </summary>
    /// <param name="address">The address to write to.</param>
    /// <returns>A new MemoryAccessContext for the write operation.</returns>
    public static MemoryAccessContext CreateWriteByte(ushort address)
    {
        return new MemoryAccessContext(address, MemoryAccessPermissions.Write, isWordAccess: false);
    }

    /// <summary>
    /// Creates a context for a word write operation.
    /// </summary>
    /// <param name="address">The address to write to (must be word-aligned).</param>
    /// <returns>A new MemoryAccessContext for the write operation.</returns>
    public static MemoryAccessContext CreateWriteWord(ushort address)
    {
        return new MemoryAccessContext(address, MemoryAccessPermissions.Write, isWordAccess: true);
    }

    /// <summary>
    /// Creates a context for an instruction fetch operation.
    /// </summary>
    /// <param name="address">The address to fetch from (typically word-aligned).</param>
    /// <returns>A new MemoryAccessContext for the execute operation.</returns>
    public static MemoryAccessContext CreateExecute(ushort address)
    {
        return new MemoryAccessContext(address, MemoryAccessPermissions.Execute, isWordAccess: true);
    }
}
