using System;
using System.Collections.Generic;
using System.Linq;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Implements information memory functionality for the MSP430 emulator.
/// 
/// Provides non-volatile storage for calibration data and user information
/// with segment-based organization and write protection controls.
/// 
/// Implementation based on MSP430FR2xx/FR4xx Family User's Guide (SLAU445I, June 2013 - Revised July 2019) - Section 7.2.4:
/// "Information Memory" - Memory organization and protection mechanisms.
/// </summary>
public class InformationMemory : IInformationMemory
{
    private const int TotalSize = 512; // Information memory size (0x1800-0x19FF)
    private const ushort BaseAddr = 0x1800; // Information memory base address
    private const int SegSize = 128; // Each segment is 128 bytes

    private readonly byte[] _memory;
    private readonly ILogger? _logger;
    private readonly Dictionary<InformationSegment, bool> _segmentProtection;
    private IReadOnlyList<InformationSegmentInfo>? _cachedSegments;

    /// <summary>
    /// Initializes a new instance of the InformationMemory class.
    /// </summary>
    /// <param name="logger">Optional logger for debugging and diagnostics.</param>
    public InformationMemory(ILogger? logger = null)
    {
        _memory = new byte[TotalSize];
        _logger = logger;
        _segmentProtection = new Dictionary<InformationSegment, bool>
        {
            { InformationSegment.SegmentA, true },  // Segment A is protected by default (calibration data)
            { InformationSegment.SegmentB, false },
            { InformationSegment.SegmentC, false },
            { InformationSegment.SegmentD, false }
        };

        // Initialize memory to erased state (all segments)
        Array.Fill(_memory, (byte)0xFF);

        _logger?.Debug($"Information memory initialized at 0x{BaseAddress:X4}-0x{EndAddress:X4}");
    }

    /// <inheritdoc />
    public int Size => TotalSize;

    /// <inheritdoc />
    public ushort BaseAddress => BaseAddr;

    /// <inheritdoc />
    public ushort EndAddress => (ushort)(BaseAddr + TotalSize - 1);

    /// <inheritdoc />
    public int SegmentSize => SegSize;

    /// <inheritdoc />
    public IReadOnlyList<InformationSegmentInfo> Segments => _cachedSegments ??= GetCurrentSegmentInfos();

    private List<InformationSegmentInfo> GetCurrentSegmentInfos()
    {
        return new List<InformationSegmentInfo>
        {
            new(InformationSegment.SegmentA, (ushort)(BaseAddr + SegSize * 3), (ushort)(BaseAddr + SegSize * 4 - 1), IsSegmentWriteProtected(InformationSegment.SegmentA), "Calibration data and factory settings"),
            new(InformationSegment.SegmentB, (ushort)(BaseAddr + SegSize * 2), (ushort)(BaseAddr + SegSize * 3 - 1), IsSegmentWriteProtected(InformationSegment.SegmentB), "User information storage"),
            new(InformationSegment.SegmentC, (ushort)(BaseAddr + SegSize * 1), (ushort)(BaseAddr + SegSize * 2 - 1), IsSegmentWriteProtected(InformationSegment.SegmentC), "User information storage"),
            new(InformationSegment.SegmentD, (ushort)(BaseAddr + SegSize * 0), (ushort)(BaseAddr + SegSize * 1 - 1), IsSegmentWriteProtected(InformationSegment.SegmentD), "User information storage")
        };
    }

    /// <inheritdoc />
    public byte ReadByte(ushort address)
    {
        ValidateAddress(address);

        int offset = address - BaseAddress;
        byte value = _memory[offset];

        _logger?.Debug($"Read byte 0x{value:X2} from information memory address 0x{address:X4}");
        return value;
    }

    /// <inheritdoc />
    public ushort ReadWord(ushort address)
    {
        ValidateAddress(address);
        ValidateWordAlignment(address);
        ValidateAddress((ushort)(address + 1)); // Ensure both bytes are in bounds

        int offset = address - BaseAddress;
        byte lowByte = _memory[offset];
        byte highByte = _memory[offset + 1];
        ushort value = (ushort)(lowByte | (highByte << 8));

        _logger?.Debug($"Read word 0x{value:X4} from information memory address 0x{address:X4}");
        return value;
    }

    /// <inheritdoc />
    public bool WriteByte(ushort address, byte value)
    {
        ValidateAddress(address);

        InformationSegment segment = GetSegment(address);
        if (IsSegmentWriteProtected(segment))
        {
            _logger?.Warning($"Write to protected segment {segment} at address 0x{address:X4} blocked");
            return false;
        }

        int offset = address - BaseAddress;
        _memory[offset] = value;

        _logger?.Debug($"Wrote byte 0x{value:X2} to information memory address 0x{address:X4}");
        return true;
    }

    /// <inheritdoc />
    public bool WriteWord(ushort address, ushort value)
    {
        ValidateAddress(address);
        ValidateWordAlignment(address);
        ValidateAddress((ushort)(address + 1)); // Ensure both bytes are in bounds

        InformationSegment segment = GetSegment(address);
        InformationSegment segment2 = GetSegment((ushort)(address + 1));

        // Check if either byte's segment is protected
        if (IsSegmentWriteProtected(segment) || IsSegmentWriteProtected(segment2))
        {
            _logger?.Warning($"Write to protected segment at address 0x{address:X4} blocked");
            return false;
        }

        int offset = address - BaseAddress;
        _memory[offset] = (byte)(value & 0xFF);
        _memory[offset + 1] = (byte)((value >> 8) & 0xFF);

        _logger?.Debug($"Wrote word 0x{value:X4} to information memory address 0x{address:X4}");
        return true;
    }

    /// <inheritdoc />
    public InformationSegment GetSegment(ushort address)
    {
        ValidateAddress(address);

        // Calculate segment boundaries based on BaseAddr and SegSize
        ushort segmentDEnd = (ushort)(BaseAddr + SegSize - 1);
        ushort segmentCStart = (ushort)(BaseAddr + SegSize);
        ushort segmentCEnd = (ushort)(BaseAddr + SegSize * 2 - 1);
        ushort segmentBStart = (ushort)(BaseAddr + SegSize * 2);
        ushort segmentBEnd = (ushort)(BaseAddr + SegSize * 3 - 1);
        ushort segmentAStart = (ushort)(BaseAddr + SegSize * 3);

        if (address >= segmentAStart)
        {
            return InformationSegment.SegmentA;
        }
        else if (address >= segmentBStart && address <= segmentBEnd)
        {
            return InformationSegment.SegmentB;
        }
        else if (address >= segmentCStart && address <= segmentCEnd)
        {
            return InformationSegment.SegmentC;
        }
        else if (address >= BaseAddr && address <= segmentDEnd)
        {
            return InformationSegment.SegmentD;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(address),
                $"Address 0x{address:X4} is not within information memory bounds");
        }
    }

    /// <inheritdoc />
    public InformationSegmentInfo GetSegmentInfo(InformationSegment segment)
    {
        return Segments.First(s => s.Segment == segment);
    }

    /// <inheritdoc />
    public bool IsSegmentWriteProtected(InformationSegment segment)
    {
        return _segmentProtection[segment];
    }

    /// <inheritdoc />
    public bool SetSegmentWriteProtection(InformationSegment segment, bool isProtected)
    {
        bool oldState = _segmentProtection[segment];
        _segmentProtection[segment] = isProtected;

        // Invalidate cache since protection state changed
        _cachedSegments = null;

        _logger?.Info($"Segment {segment} write protection changed from {oldState} to {isProtected}");

        return true;
    }

    /// <inheritdoc />
    public bool EraseSegment(InformationSegment segment)
    {
        if (IsSegmentWriteProtected(segment))
        {
            _logger?.Warning($"Erase of protected segment {segment} blocked");
            return false;
        }

        InformationSegmentInfo segmentInfo = GetSegmentInfo(segment);
        int startOffset = segmentInfo.StartAddress - BaseAddress;
        int size = segmentInfo.Size;

        Array.Fill(_memory, (byte)0xFF, startOffset, size);

        _logger?.Info($"Erased segment {segment} (0x{segmentInfo.StartAddress:X4}-0x{segmentInfo.EndAddress:X4})");

        return true;
    }

    /// <inheritdoc />
    public bool StoreCalibrationData(ReadOnlySpan<byte> calibrationData)
    {
        if (IsSegmentWriteProtected(InformationSegment.SegmentA))
        {
            _logger?.Warning("Cannot store calibration data - Segment A is write-protected");
            return false;
        }

        InformationSegmentInfo segmentInfo = GetSegmentInfo(InformationSegment.SegmentA);
        int maxSize = segmentInfo.Size;

        if (calibrationData.Length > maxSize)
        {
            _logger?.Warning($"Calibration data size {calibrationData.Length} exceeds segment capacity {maxSize}");
            return false;
        }

        int startOffset = segmentInfo.StartAddress - BaseAddress;
        calibrationData.CopyTo(_memory.AsSpan(startOffset, calibrationData.Length));

        _logger?.Info($"Stored {calibrationData.Length} bytes of calibration data in Segment A");
        return true;
    }

    /// <inheritdoc />
    public int ReadCalibrationData(Span<byte> buffer)
    {
        InformationSegmentInfo segmentInfo = GetSegmentInfo(InformationSegment.SegmentA);
        int startOffset = segmentInfo.StartAddress - BaseAddress;
        int bytesToRead = Math.Min(buffer.Length, segmentInfo.Size);

        _memory.AsSpan(startOffset, bytesToRead).CopyTo(buffer);

        _logger?.Debug($"Read {bytesToRead} bytes of calibration data from Segment A");
        return bytesToRead;
    }

    /// <inheritdoc />
    public bool Clear()
    {
        bool allCleared = true;

        foreach (InformationSegment segment in Enum.GetValues<InformationSegment>())
        {
            if (IsSegmentWriteProtected(segment))
            {
                _logger?.Warning($"Cannot clear protected segment {segment}");
                allCleared = false;
                continue;
            }

            EraseSegment(segment);
        }

        return allCleared;
    }

    /// <inheritdoc />
    public bool Initialize(byte pattern = 0xFF)
    {
        bool allInitialized = true;

        foreach (InformationSegment segment in Enum.GetValues<InformationSegment>())
        {
            if (IsSegmentWriteProtected(segment))
            {
                _logger?.Debug($"Skipping initialization of protected segment {segment}");
                allInitialized = false;
                continue;
            }

            InformationSegmentInfo segmentInfo = GetSegmentInfo(segment);
            int startOffset = segmentInfo.StartAddress - BaseAddress;
            Array.Fill(_memory, pattern, startOffset, segmentInfo.Size);
        }

        _logger?.Debug($"Initialized information memory with pattern 0x{pattern:X2}");
        return allInitialized;
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

        int endAddress = startAddress + length - 1;
        return IsAddressInBounds(startAddress) && endAddress <= EndAddress;
    }

    private void ValidateAddress(ushort address)
    {
        if (!IsAddressInBounds(address))
        {
            throw new ArgumentOutOfRangeException(nameof(address),
                $"Address 0x{address:X4} is outside information memory bounds (0x{BaseAddress:X4}-0x{EndAddress:X4})");
        }
    }

    private static void ValidateWordAlignment(ushort address)
    {
        if ((address & 1) != 0)
        {
            throw new ArgumentException($"Address 0x{address:X4} is not word-aligned", nameof(address));
        }
    }
}
