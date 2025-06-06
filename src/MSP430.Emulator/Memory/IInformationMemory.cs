using System;
using System.Collections.Generic;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the contract for information memory operations in the MSP430 emulator.
/// 
/// Information memory provides non-volatile storage for calibration data and user
/// information in MSP430 devices. It supports segment-based organization with
/// individual write protection controls for each segment.
/// 
/// Interface design based on MSP430FR2xx/FR4xx Family User's Guide (SLAU445I, June 2013 - Revised July 2019) - Section 7.2.4:
/// "Information Memory" - Programming model and protection mechanisms.
/// </summary>
public interface IInformationMemory
{
    /// <summary>
    /// Gets the total size of the information memory in bytes.
    /// </summary>
    int Size { get; }

    /// <summary>
    /// Gets the base address where the information memory is mapped.
    /// </summary>
    ushort BaseAddress { get; }

    /// <summary>
    /// Gets the end address of the information memory region (inclusive).
    /// </summary>
    ushort EndAddress { get; }

    /// <summary>
    /// Gets the size of each information segment in bytes (typically 128 bytes).
    /// </summary>
    int SegmentSize { get; }

    /// <summary>
    /// Gets information about all information memory segments.
    /// </summary>
    IReadOnlyList<InformationSegmentInfo> Segments { get; }

    /// <summary>
    /// Reads a byte from the specified address in information memory.
    /// Information memory can always be read regardless of protection state.
    /// </summary>
    /// <param name="address">The memory address to read from.</param>
    /// <returns>The byte value at the specified address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    byte ReadByte(ushort address);

    /// <summary>
    /// Reads a 16-bit word from the specified address in information memory.
    /// Uses little-endian byte ordering (low byte at address, high byte at address+1).
    /// </summary>
    /// <param name="address">The memory address to read from (must be even for aligned access).</param>
    /// <returns>The 16-bit word value at the specified address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is not word-aligned.</exception>
    ushort ReadWord(ushort address);

    /// <summary>
    /// Writes a byte to the specified address in information memory.
    /// Write operation may be blocked if the containing segment is write-protected.
    /// </summary>
    /// <param name="address">The memory address to write to.</param>
    /// <param name="value">The byte value to write.</param>
    /// <returns>True if the write was successful, false if blocked by protection.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    bool WriteByte(ushort address, byte value);

    /// <summary>
    /// Writes a 16-bit word to the specified address in information memory.
    /// Uses little-endian byte ordering (low byte at address, high byte at address+1).
    /// </summary>
    /// <param name="address">The memory address to write to (must be even for aligned access).</param>
    /// <param name="value">The 16-bit word value to write.</param>
    /// <returns>True if the write was successful, false if blocked by protection.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is not word-aligned.</exception>
    bool WriteWord(ushort address, ushort value);

    /// <summary>
    /// Gets the segment that contains the specified address.
    /// </summary>
    /// <param name="address">The memory address to look up.</param>
    /// <returns>The information segment containing the address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the address is outside the memory bounds.</exception>
    InformationSegment GetSegment(ushort address);

    /// <summary>
    /// Gets detailed information about the specified segment.
    /// </summary>
    /// <param name="segment">The information segment to query.</param>
    /// <returns>The segment information.</returns>
    InformationSegmentInfo GetSegmentInfo(InformationSegment segment);

    /// <summary>
    /// Determines if the specified segment is write-protected.
    /// </summary>
    /// <param name="segment">The segment to check.</param>
    /// <returns>True if the segment is write-protected, false otherwise.</returns>
    bool IsSegmentWriteProtected(InformationSegment segment);

    /// <summary>
    /// Sets the write protection state for the specified segment.
    /// </summary>
    /// <param name="segment">The segment to modify.</param>
    /// <param name="isProtected">True to enable write protection, false to disable.</param>
    /// <returns>True if the protection state was changed successfully, false if operation is not allowed.</returns>
    bool SetSegmentWriteProtection(InformationSegment segment, bool isProtected);

    /// <summary>
    /// Erases the contents of the specified segment (sets all bytes to 0xFF).
    /// Erase operation may be blocked if the segment is write-protected.
    /// </summary>
    /// <param name="segment">The segment to erase.</param>
    /// <returns>True if the erase was successful, false if blocked by protection.</returns>
    bool EraseSegment(InformationSegment segment);

    /// <summary>
    /// Stores calibration data in the appropriate segment (typically Segment A).
    /// </summary>
    /// <param name="calibrationData">The calibration data to store.</param>
    /// <returns>True if the calibration data was stored successfully, false if blocked by protection.</returns>
    bool StoreCalibrationData(ReadOnlySpan<byte> calibrationData);

    /// <summary>
    /// Retrieves calibration data from the appropriate segment (typically Segment A).
    /// </summary>
    /// <param name="buffer">The buffer to store the calibration data.</param>
    /// <returns>The number of bytes read into the buffer.</returns>
    int ReadCalibrationData(Span<byte> buffer);

    /// <summary>
    /// Clears all information memory contents by setting all bytes to 0xFF (erased state).
    /// Only non-protected segments will be cleared.
    /// </summary>
    /// <returns>True if all accessible segments were cleared, false if some segments were protected.</returns>
    bool Clear();

    /// <summary>
    /// Initializes the information memory with default values.
    /// Protected segments will not be modified.
    /// </summary>
    /// <param name="pattern">The byte pattern to fill accessible segments with (default is 0xFF for erased state).</param>
    /// <returns>True if all accessible segments were initialized, false if some segments were protected.</returns>
    bool Initialize(byte pattern = 0xFF);

    /// <summary>
    /// Determines if the specified address is within the bounds of information memory.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>True if the address is within bounds, false otherwise.</returns>
    bool IsAddressInBounds(ushort address);

    /// <summary>
    /// Determines if the specified address range is entirely within the bounds of information memory.
    /// </summary>
    /// <param name="startAddress">The starting address of the range.</param>
    /// <param name="length">The length of the range in bytes.</param>
    /// <returns>True if the entire range is within bounds, false otherwise.</returns>
    bool IsRangeInBounds(ushort startAddress, int length);
}
