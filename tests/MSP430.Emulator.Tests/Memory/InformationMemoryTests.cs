using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Memory;

public class InformationMemoryTests
{
    private readonly TestLogger _logger;

    public InformationMemoryTests()
    {
        _logger = new TestLogger();
    }

    [Fact]
    public void Constructor_WithLogger_SetsSize()
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal(512, infoMemory.Size);
    }

    [Fact]
    public void Constructor_WithLogger_SetsBaseAddress()
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal((ushort)0x1800, infoMemory.BaseAddress);
    }

    [Fact]
    public void Constructor_WithLogger_SetsEndAddress()
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal((ushort)0x19FF, infoMemory.EndAddress);
    }

    [Fact]
    public void Constructor_WithLogger_SetsSegmentSize()
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal(128, infoMemory.SegmentSize);
    }

    [Fact]
    public void Constructor_WithLogger_SetsSegmentCount()
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal(4, infoMemory.Segments.Count);
    }

    [Fact]
    public void Constructor_WithoutLogger_SetsSize()
    {
        var infoMemory = new InformationMemory();

        Assert.Equal(512, infoMemory.Size);
    }

    [Fact]
    public void Constructor_WithoutLogger_SetsBaseAddress()
    {
        var infoMemory = new InformationMemory();

        Assert.Equal((ushort)0x1800, infoMemory.BaseAddress);
    }

    [Fact]
    public void Constructor_WithoutLogger_SetsEndAddress()
    {
        var infoMemory = new InformationMemory();

        Assert.Equal((ushort)0x19FF, infoMemory.EndAddress);
    }

    [Fact]
    public void Segments_HasCorrectCount()
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal(4, infoMemory.Segments.Count);
    }

    [Theory]
    [InlineData(InformationSegment.SegmentA)]
    [InlineData(InformationSegment.SegmentB)]
    [InlineData(InformationSegment.SegmentC)]
    [InlineData(InformationSegment.SegmentD)]
    public void Segments_ContainsSegment(InformationSegment expectedSegment)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Contains(infoMemory.Segments, s => s.Segment == expectedSegment);
    }

    [Theory]
    [InlineData(InformationSegment.SegmentA, 0x1980, 0x19FF, true)]
    [InlineData(InformationSegment.SegmentB, 0x1900, 0x197F, false)]
    [InlineData(InformationSegment.SegmentC, 0x1880, 0x18FF, false)]
    [InlineData(InformationSegment.SegmentD, 0x1800, 0x187F, false)]
    public void GetSegmentInfo_ReturnsCorrectSegmentInfo(InformationSegment segment,
        ushort expectedStart, ushort expectedEnd, bool expectedProtection)
    {
        var infoMemory = new InformationMemory(_logger);

        InformationSegmentInfo segmentInfo = infoMemory.GetSegmentInfo(segment);

        Assert.Equal(segment, segmentInfo.Segment);
        Assert.Equal(expectedStart, segmentInfo.StartAddress);
        Assert.Equal(expectedEnd, segmentInfo.EndAddress);
        Assert.Equal(128, segmentInfo.Size);
        Assert.Equal(expectedProtection, segmentInfo.IsWriteProtected);
    }

    [Theory]
    [InlineData(0x1800, InformationSegment.SegmentD)]
    [InlineData(0x187F, InformationSegment.SegmentD)]
    [InlineData(0x1880, InformationSegment.SegmentC)]
    [InlineData(0x18FF, InformationSegment.SegmentC)]
    [InlineData(0x1900, InformationSegment.SegmentB)]
    [InlineData(0x197F, InformationSegment.SegmentB)]
    [InlineData(0x1980, InformationSegment.SegmentA)]
    [InlineData(0x19FF, InformationSegment.SegmentA)]
    public void GetSegment_ValidAddresses_ReturnsCorrectSegment(ushort address, InformationSegment expectedSegment)
    {
        var infoMemory = new InformationMemory(_logger);

        InformationSegment segment = infoMemory.GetSegment(address);

        Assert.Equal(expectedSegment, segment);
    }

    [Theory]
    [InlineData(0x17FF)] // One below base
    [InlineData(0x1A00)] // One above end
    [InlineData(0x0000)] // Far below
    [InlineData(0xFFFF)] // Far above
    public void GetSegment_InvalidAddresses_ThrowsArgumentOutOfRangeException(ushort invalidAddress)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Throws<ArgumentOutOfRangeException>(() => infoMemory.GetSegment(invalidAddress));
    }

    [Fact]
    public void ReadByte_ValidAddress_ReturnsErasedValue()
    {
        var infoMemory = new InformationMemory(_logger);

        byte value = infoMemory.ReadByte(0x1800);

        Assert.Equal(0xFF, value); // Erased state
    }

    [Theory]
    [InlineData(0x17FF)] // One below base
    [InlineData(0x1A00)] // One above end
    public void ReadByte_InvalidAddress_ThrowsArgumentOutOfRangeException(ushort invalidAddress)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Throws<ArgumentOutOfRangeException>(() => infoMemory.ReadByte(invalidAddress));
    }

    [Fact]
    public void ReadWord_ValidAddress_ReturnsErasedValue()
    {
        var infoMemory = new InformationMemory(_logger);

        ushort value = infoMemory.ReadWord(0x1800);

        Assert.Equal((ushort)0xFFFF, value); // Erased state
    }

    [Theory]
    [InlineData(0x1801)] // Odd address (not word-aligned)
    [InlineData(0x1803)] // Another odd address
    public void ReadWord_UnalignedAddress_ThrowsArgumentException(ushort unalignedAddress)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Throws<ArgumentException>(() => infoMemory.ReadWord(unalignedAddress));
    }

    [Theory]
    [InlineData(0x17FF)] // One below base
    [InlineData(0x1A00)] // One above end
    public void ReadWord_InvalidAddress_ThrowsArgumentOutOfRangeException(ushort invalidAddress)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Throws<ArgumentOutOfRangeException>(() => infoMemory.ReadWord(invalidAddress));
    }

    [Fact]
    public void WriteByte_ToUnprotectedSegment_ReturnsTrue()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1800; // Segment D (unprotected)
        const byte testValue = 0x42;

        bool result = infoMemory.WriteByte(address, testValue);

        Assert.True(result);
    }

    [Fact]
    public void WriteByte_ToUnprotectedSegment_WritesValue()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1800; // Segment D (unprotected)
        const byte testValue = 0x42;

        infoMemory.WriteByte(address, testValue);

        Assert.Equal(testValue, infoMemory.ReadByte(address));
    }

    [Fact]
    public void WriteByte_ToProtectedSegment_Fails()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1980; // Segment A (protected by default)
        const byte testValue = 0x42;

        bool result = infoMemory.WriteByte(address, testValue);

        Assert.False(result);
        Assert.Equal(0xFF, infoMemory.ReadByte(address)); // Should remain erased
    }

    [Theory]
    [InlineData(0x17FF)] // One below base
    [InlineData(0x1A00)] // One above end
    public void WriteByte_InvalidAddress_ThrowsArgumentOutOfRangeException(ushort invalidAddress)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Throws<ArgumentOutOfRangeException>(() => infoMemory.WriteByte(invalidAddress, 0x42));
    }

    [Fact]
    public void WriteWord_ToUnprotectedSegment_Succeeds()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1800; // Segment D (unprotected)
        const ushort testValue = 0x1234;

        bool result = infoMemory.WriteWord(address, testValue);

        Assert.True(result);
        Assert.Equal(testValue, infoMemory.ReadWord(address));
    }

    [Fact]
    public void WriteWord_ToProtectedSegment_Fails()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1980; // Segment A (protected by default)
        const ushort testValue = 0x1234;

        bool result = infoMemory.WriteWord(address, testValue);

        Assert.False(result);
        Assert.Equal((ushort)0xFFFF, infoMemory.ReadWord(address)); // Should remain erased
    }

    [Fact]
    public void WriteWord_SpanningSegments_ChecksBothSegments()
    {
        var infoMemory = new InformationMemory(_logger);

        // Make Segment D protected 
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentD, true);

        const ushort address = 0x187E; // Both bytes in Segment D which is now protected
        const ushort testValue = 0x1234;

        bool result = infoMemory.WriteWord(address, testValue);

        Assert.False(result); // Should fail because Segment D is protected
    }

    [Theory]
    [InlineData(0x1801)] // Odd address (not word-aligned)
    [InlineData(0x1803)] // Another odd address
    public void WriteWord_UnalignedAddress_ThrowsArgumentException(ushort unalignedAddress)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Throws<ArgumentException>(() => infoMemory.WriteWord(unalignedAddress, 0x1234));
    }

    [Theory]
    [InlineData(InformationSegment.SegmentA, true)]  // Protected by default
    [InlineData(InformationSegment.SegmentB, false)] // Unprotected by default
    [InlineData(InformationSegment.SegmentC, false)] // Unprotected by default
    [InlineData(InformationSegment.SegmentD, false)] // Unprotected by default
    public void IsSegmentWriteProtected_DefaultState_ReturnsExpectedValue(InformationSegment segment, bool expectedProtection)
    {
        var infoMemory = new InformationMemory(_logger);

        bool isProtected = infoMemory.IsSegmentWriteProtected(segment);

        Assert.Equal(expectedProtection, isProtected);
    }

    [Fact]
    public void SetSegmentWriteProtection_ChangesProtectionState()
    {
        var infoMemory = new InformationMemory(_logger);

        // Initially Segment B is unprotected
        Assert.False(infoMemory.IsSegmentWriteProtected(InformationSegment.SegmentB));

        // Enable protection
        bool result1 = infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentB, true);
        Assert.True(result1);
        Assert.True(infoMemory.IsSegmentWriteProtected(InformationSegment.SegmentB));

        // Disable protection
        bool result2 = infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentB, false);
        Assert.True(result2);
        Assert.False(infoMemory.IsSegmentWriteProtected(InformationSegment.SegmentB));
    }

    [Fact]
    public void EraseSegment_UnprotectedSegment_Succeeds()
    {
        var infoMemory = new InformationMemory(_logger);

        // Write some data to Segment D first
        infoMemory.WriteByte(0x1800, 0x42);
        infoMemory.WriteByte(0x1801, 0x43);

        bool result = infoMemory.EraseSegment(InformationSegment.SegmentD);

        Assert.True(result);
        Assert.Equal(0xFF, infoMemory.ReadByte(0x1800));
        Assert.Equal(0xFF, infoMemory.ReadByte(0x1801));
    }

    [Fact]
    public void EraseSegment_ProtectedSegment_Fails()
    {
        var infoMemory = new InformationMemory(_logger);

        bool result = infoMemory.EraseSegment(InformationSegment.SegmentA); // Protected by default

        Assert.False(result);
    }

    [Fact]
    public void StoreCalibrationData_SegmentAUnprotected_Succeeds()
    {
        var infoMemory = new InformationMemory(_logger);

        // Unprotect Segment A for this test
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentA, false);

        byte[] calibrationData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        bool result = infoMemory.StoreCalibrationData(calibrationData);

        Assert.True(result);

        // Verify data was stored
        byte[] buffer = new byte[calibrationData.Length];
        int bytesRead = infoMemory.ReadCalibrationData(buffer);
        Assert.Equal(calibrationData.Length, bytesRead);
        Assert.Equal(calibrationData, buffer);
    }

    [Fact]
    public void StoreCalibrationData_SegmentAProtected_Fails()
    {
        var infoMemory = new InformationMemory(_logger);

        // Segment A is protected by default
        byte[] calibrationData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        bool result = infoMemory.StoreCalibrationData(calibrationData);

        Assert.False(result);
    }

    [Fact]
    public void StoreCalibrationData_DataTooLarge_Fails()
    {
        var infoMemory = new InformationMemory(_logger);

        // Unprotect Segment A for this test
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentA, false);

        byte[] calibrationData = new byte[129]; // Larger than segment size (128 bytes)

        bool result = infoMemory.StoreCalibrationData(calibrationData);

        Assert.False(result);
    }

    [Fact]
    public void ReadCalibrationData_ReturnsStoredData()
    {
        var infoMemory = new InformationMemory(_logger);

        // Unprotect Segment A and store some data
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentA, false);
        byte[] originalData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        infoMemory.StoreCalibrationData(originalData);

        byte[] buffer = new byte[10]; // Larger buffer
        int bytesRead = infoMemory.ReadCalibrationData(buffer);

        Assert.Equal(10, bytesRead); // Should read min(buffer.Length, segmentSize)
        Assert.Equal(0x01, buffer[0]);
        Assert.Equal(0x02, buffer[1]);
        Assert.Equal(0x03, buffer[2]);
        Assert.Equal(0x04, buffer[3]);
        Assert.Equal(0xFF, buffer[4]); // Rest should be erased state
    }

    [Fact]
    public void Clear_WithMixedProtection_ClearsOnlyUnprotectedSegments()
    {
        var infoMemory = new InformationMemory(_logger);

        // Write data to all segments first (unprotected ones)
        infoMemory.WriteByte(0x1800, 0x42); // Segment D
        infoMemory.WriteByte(0x1880, 0x43); // Segment C
        infoMemory.WriteByte(0x1900, 0x44); // Segment B
                                            // Segment A is protected, so we can't write to it

        bool result = infoMemory.Clear();

        Assert.False(result); // Should return false because Segment A couldn't be cleared
        Assert.Equal(0xFF, infoMemory.ReadByte(0x1800)); // Segment D should be cleared
        Assert.Equal(0xFF, infoMemory.ReadByte(0x1880)); // Segment C should be cleared
        Assert.Equal(0xFF, infoMemory.ReadByte(0x1900)); // Segment B should be cleared
    }

    [Fact]
    public void Initialize_WithPattern_InitializesUnprotectedSegments()
    {
        var infoMemory = new InformationMemory(_logger);
        const byte pattern = 0x55;

        bool result = infoMemory.Initialize(pattern);

        Assert.False(result); // Should return false because Segment A couldn't be initialized
        Assert.Equal(pattern, infoMemory.ReadByte(0x1800)); // Segment D should have pattern
        Assert.Equal(pattern, infoMemory.ReadByte(0x1880)); // Segment C should have pattern
        Assert.Equal(pattern, infoMemory.ReadByte(0x1900)); // Segment B should have pattern
        Assert.Equal(0xFF, infoMemory.ReadByte(0x1980)); // Segment A should remain erased (protected)
    }

    [Theory]
    [InlineData(0x1800, true)]  // Base address
    [InlineData(0x19FF, true)]  // End address
    [InlineData(0x1900, true)]  // Middle address
    [InlineData(0x17FF, false)] // One below base
    [InlineData(0x1A00, false)] // One above end
    [InlineData(0x0000, false)] // Far below
    [InlineData(0xFFFF, false)] // Far above
    public void IsAddressInBounds_VariousAddresses_ReturnsExpectedResult(ushort address, bool expected)
    {
        var infoMemory = new InformationMemory(_logger);

        bool result = infoMemory.IsAddressInBounds(address);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0x1800, 1, true)]     // Single byte at start
    [InlineData(0x19FF, 1, true)]     // Single byte at end
    [InlineData(0x1800, 512, true)]   // Entire memory
    [InlineData(0x19FF, 2, false)]    // Would go beyond end
    [InlineData(0x17FF, 1, false)]    // Start before memory
    [InlineData(0x1800, 0, false)]    // Zero length
    [InlineData(0x1800, -1, false)]   // Negative length
    public void IsRangeInBounds_VariousRanges_ReturnsExpectedResult(ushort startAddress, int length, bool expected)
    {
        var infoMemory = new InformationMemory(_logger);

        bool result = infoMemory.IsRangeInBounds(startAddress, length);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void InformationSegmentInfo_Contains_WorksCorrectly()
    {
        var segmentInfo = new InformationSegmentInfo(
            InformationSegment.SegmentA, 0x1980, 0x19FF, true, "Test segment");

        Assert.True(segmentInfo.Contains(0x1980));  // Start address
        Assert.True(segmentInfo.Contains(0x19FF));  // End address
        Assert.True(segmentInfo.Contains(0x1990));  // Middle address
        Assert.False(segmentInfo.Contains(0x197F)); // Before start
        Assert.False(segmentInfo.Contains(0x1A00)); // After end
    }

    [Fact]
    public void InformationSegmentInfo_Size_CalculatedCorrectly()
    {
        var segmentInfo = new InformationSegmentInfo(
            InformationSegment.SegmentA, 0x1980, 0x19FF, true, "Test segment");

        Assert.Equal(128, segmentInfo.Size); // 0x19FF - 0x1980 + 1 = 128
    }

    private class TestLogger : ILogger
    {
        public LogLevel MinimumLevel { get; set; } = LogLevel.Warning;
        public List<LogEntry> LogEntries { get; } = new();

        public void Log(LogLevel level, string message)
        {
            if (IsEnabled(level))
            {
                LogEntries.Add(new LogEntry(level, message, null));
            }
        }

        public void Log(LogLevel level, string message, object? context)
        {
            if (IsEnabled(level))
            {
                LogEntries.Add(new LogEntry(level, message, context));
            }
        }

        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Debug(string message, object? context) => Log(LogLevel.Debug, message, context);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Info(string message, object? context) => Log(LogLevel.Info, message, context);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Warning(string message, object? context) => Log(LogLevel.Warning, message, context);
        public void Error(string message) => Log(LogLevel.Error, message);
        public void Error(string message, object? context) => Log(LogLevel.Error, message, context);
        public void Fatal(string message) => Log(LogLevel.Fatal, message);
        public void Fatal(string message, object? context) => Log(LogLevel.Fatal, message, context);

        public bool IsEnabled(LogLevel level) => level >= MinimumLevel;
    }

    private record LogEntry(LogLevel Level, string Message, object? Context);
}
