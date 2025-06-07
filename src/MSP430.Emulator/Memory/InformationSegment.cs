namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the information memory segments in the MSP430FR2355.
/// 
/// Information memory contains calibration data and user-specific information
/// stored in FRAM. Each segment has specific characteristics and protection levels.
/// 
/// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 1.9.1:
/// "Memory Map" - Segment organization and access control.
/// </summary>
public enum InformationSegment
{
    /// <summary>
    /// Information Segment A (0x1980-0x19FF, 128 bytes).
    /// Typically contains device calibration data and factory settings.
    /// Often write-protected to preserve calibration values.
    /// </summary>
    SegmentA,

    /// <summary>
    /// Information Segment B (0x1900-0x197F, 128 bytes).
    /// Available for user information storage.
    /// Can be write-protected based on application requirements.
    /// </summary>
    SegmentB,

    /// <summary>
    /// Information Segment C (0x1880-0x18FF, 128 bytes).
    /// Available for user information storage.
    /// Can be write-protected based on application requirements.
    /// </summary>
    SegmentC,

    /// <summary>
    /// Information Segment D (0x1800-0x187F, 128 bytes).
    /// Available for user information storage.
    /// Can be write-protected based on application requirements.
    /// </summary>
    SegmentD
}

/// <summary>
/// Provides information about an information memory segment.
/// </summary>
public readonly struct InformationSegmentInfo
{
    /// <summary>
    /// Initializes a new instance of the InformationSegmentInfo struct.
    /// </summary>
    /// <param name="segment">The information segment.</param>
    /// <param name="startAddress">The starting address of the segment.</param>
    /// <param name="endAddress">The ending address of the segment (inclusive).</param>
    /// <param name="isWriteProtected">Indicates if the segment is write-protected.</param>
    /// <param name="description">A description of the segment's purpose.</param>
    public InformationSegmentInfo(InformationSegment segment, ushort startAddress, ushort endAddress,
        bool isWriteProtected, string description)
    {
        Segment = segment;
        StartAddress = startAddress;
        EndAddress = endAddress;
        IsWriteProtected = isWriteProtected;
        Description = description;
    }

    /// <summary>
    /// Gets the information segment.
    /// </summary>
    public InformationSegment Segment { get; }

    /// <summary>
    /// Gets the starting address of the segment.
    /// </summary>
    public ushort StartAddress { get; }

    /// <summary>
    /// Gets the ending address of the segment (inclusive).
    /// </summary>
    public ushort EndAddress { get; }

    /// <summary>
    /// Gets a value indicating whether the segment is write-protected.
    /// </summary>
    public bool IsWriteProtected { get; }

    /// <summary>
    /// Gets the description of the segment's purpose.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the size of the segment in bytes.
    /// </summary>
    public int Size => EndAddress - StartAddress + 1;

    /// <summary>
    /// Determines if the specified address is within this segment.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>True if the address is within the segment, false otherwise.</returns>
    public bool Contains(ushort address) => address >= StartAddress && address <= EndAddress;
}
