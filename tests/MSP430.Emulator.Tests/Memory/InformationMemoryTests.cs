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

    [Theory]
    [InlineData(512)]
    public void Constructor_WithLogger_SetsSize(int expectedSize)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal(expectedSize, infoMemory.Size);
    }

    [Theory]
    [InlineData(0x1800)]
    public void Constructor_WithLogger_SetsBaseAddress(ushort expectedBaseAddress)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal(expectedBaseAddress, infoMemory.BaseAddress);
    }

    [Theory]
    [InlineData(0x19FF)]
    public void Constructor_WithLogger_SetsEndAddress(ushort expectedEndAddress)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal(expectedEndAddress, infoMemory.EndAddress);
    }

    [Theory]
    [InlineData(128)]
    public void Constructor_WithLogger_SetsSegmentSize(int expectedSegmentSize)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal(expectedSegmentSize, infoMemory.SegmentSize);
    }

    [Theory]
    [InlineData(4)]
    public void Constructor_WithLogger_SetsSegmentCount(int expectedCount)
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.Equal(expectedCount, infoMemory.Segments.Count);
    }

    [Theory]
    [InlineData(512)]
    public void Constructor_WithoutLogger_SetsSize(int expectedSize)
    {
        var infoMemory = new InformationMemory();

        Assert.Equal(expectedSize, infoMemory.Size);
    }

    [Theory]
    [InlineData(0x1800)]
    public void Constructor_WithoutLogger_SetsBaseAddress(ushort expectedBaseAddress)
    {
        var infoMemory = new InformationMemory();

        Assert.Equal(expectedBaseAddress, infoMemory.BaseAddress);
    }

    [Theory]
    [InlineData(0x19FF)]
    public void Constructor_WithoutLogger_SetsEndAddress(ushort expectedEndAddress)
    {
        var infoMemory = new InformationMemory();

        Assert.Equal(expectedEndAddress, infoMemory.EndAddress);
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
    [InlineData(InformationSegment.SegmentA, InformationSegment.SegmentA)]
    [InlineData(InformationSegment.SegmentB, InformationSegment.SegmentB)]
    [InlineData(InformationSegment.SegmentC, InformationSegment.SegmentC)]
    [InlineData(InformationSegment.SegmentD, InformationSegment.SegmentD)]
    public void GetSegmentInfo_ReturnsCorrectSegment(InformationSegment segment, InformationSegment expectedSegment)
    {
        var infoMemory = new InformationMemory(_logger);

        InformationSegmentInfo segmentInfo = infoMemory.GetSegmentInfo(segment);

        Assert.Equal(expectedSegment, segmentInfo.Segment);
    }

    [Theory]
    [InlineData(InformationSegment.SegmentA, 0x1980)]
    [InlineData(InformationSegment.SegmentB, 0x1900)]
    [InlineData(InformationSegment.SegmentC, 0x1880)]
    [InlineData(InformationSegment.SegmentD, 0x1800)]
    public void GetSegmentInfo_ReturnsCorrectStartAddress(InformationSegment segment, ushort expectedStart)
    {
        var infoMemory = new InformationMemory(_logger);

        InformationSegmentInfo segmentInfo = infoMemory.GetSegmentInfo(segment);

        Assert.Equal(expectedStart, segmentInfo.StartAddress);
    }

    [Theory]
    [InlineData(InformationSegment.SegmentA, 0x19FF)]
    [InlineData(InformationSegment.SegmentB, 0x197F)]
    [InlineData(InformationSegment.SegmentC, 0x18FF)]
    [InlineData(InformationSegment.SegmentD, 0x187F)]
    public void GetSegmentInfo_ReturnsCorrectEndAddress(InformationSegment segment, ushort expectedEnd)
    {
        var infoMemory = new InformationMemory(_logger);

        InformationSegmentInfo segmentInfo = infoMemory.GetSegmentInfo(segment);

        Assert.Equal(expectedEnd, segmentInfo.EndAddress);
    }

    [Theory]
    [InlineData(InformationSegment.SegmentA)]
    [InlineData(InformationSegment.SegmentB)]
    [InlineData(InformationSegment.SegmentC)]
    [InlineData(InformationSegment.SegmentD)]
    public void GetSegmentInfo_ReturnsCorrectSize(InformationSegment segment)
    {
        var infoMemory = new InformationMemory(_logger);

        InformationSegmentInfo segmentInfo = infoMemory.GetSegmentInfo(segment);

        Assert.Equal(128, segmentInfo.Size);
    }

    [Theory]
    [InlineData(InformationSegment.SegmentA, true)]
    [InlineData(InformationSegment.SegmentB, false)]
    [InlineData(InformationSegment.SegmentC, false)]
    [InlineData(InformationSegment.SegmentD, false)]
    public void GetSegmentInfo_ReturnsCorrectProtectionState(InformationSegment segment, bool expectedProtection)
    {
        var infoMemory = new InformationMemory(_logger);

        InformationSegmentInfo segmentInfo = infoMemory.GetSegmentInfo(segment);

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
    public void WriteByte_ToProtectedSegment_ReturnsFalse()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1980; // Segment A (protected by default)
        const byte testValue = 0x42;

        bool result = infoMemory.WriteByte(address, testValue);

        Assert.False(result);
    }

    [Fact]
    public void WriteByte_ToProtectedSegment_DoesNotModifyMemory()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1980; // Segment A (protected by default)
        const byte testValue = 0x42;

        infoMemory.WriteByte(address, testValue);

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
    public void WriteWord_ToUnprotectedSegment_ReturnsTrue()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1800; // Segment D (unprotected)
        const ushort testValue = 0x1234;

        bool result = infoMemory.WriteWord(address, testValue);

        Assert.True(result);
    }

    [Fact]
    public void WriteWord_ToUnprotectedSegment_StoresValue()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1800; // Segment D (unprotected)
        const ushort testValue = 0x1234;

        infoMemory.WriteWord(address, testValue);

        Assert.Equal(testValue, infoMemory.ReadWord(address));
    }

    [Fact]
    public void WriteWord_ToProtectedSegment_ReturnsFalse()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1980; // Segment A (protected by default)
        const ushort testValue = 0x1234;

        bool result = infoMemory.WriteWord(address, testValue);

        Assert.False(result);
    }

    [Fact]
    public void WriteWord_ToProtectedSegment_MemoryUnchanged()
    {
        var infoMemory = new InformationMemory(_logger);
        const ushort address = 0x1980; // Segment A (protected by default)
        const ushort testValue = 0x1234;

        infoMemory.WriteWord(address, testValue);

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
    public void SetSegmentWriteProtection_InitiallySegmentBIsUnprotected()
    {
        var infoMemory = new InformationMemory(_logger);

        Assert.False(infoMemory.IsSegmentWriteProtected(InformationSegment.SegmentB));
    }

    [Fact]
    public void SetSegmentWriteProtection_EnableProtection_ReturnsTrue()
    {
        var infoMemory = new InformationMemory(_logger);

        bool result = infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentB, true);

        Assert.True(result);
    }

    [Fact]
    public void SetSegmentWriteProtection_EnableProtection_SetsProtectedState()
    {
        var infoMemory = new InformationMemory(_logger);

        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentB, true);

        Assert.True(infoMemory.IsSegmentWriteProtected(InformationSegment.SegmentB));
    }

    [Fact]
    public void SetSegmentWriteProtection_DisableProtection_ReturnsTrue()
    {
        var infoMemory = new InformationMemory(_logger);
        // First enable protection
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentB, true);

        bool result = infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentB, false);

        Assert.True(result);
    }

    [Fact]
    public void SetSegmentWriteProtection_DisableProtection_SetsUnprotectedState()
    {
        var infoMemory = new InformationMemory(_logger);
        // First enable protection
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentB, true);

        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentB, false);

        Assert.False(infoMemory.IsSegmentWriteProtected(InformationSegment.SegmentB));
    }

    [Fact]
    public void EraseSegment_UnprotectedSegment_ReturnsTrue()
    {
        var infoMemory = new InformationMemory(_logger);

        // Write some data to Segment D first
        infoMemory.WriteByte(0x1800, 0x42);
        infoMemory.WriteByte(0x1801, 0x43);

        bool result = infoMemory.EraseSegment(InformationSegment.SegmentD);

        Assert.True(result);
    }

    [Fact]
    public void EraseSegment_UnprotectedSegment_ErasesFirstByte()
    {
        var infoMemory = new InformationMemory(_logger);

        // Write some data to Segment D first
        infoMemory.WriteByte(0x1800, 0x42);

        infoMemory.EraseSegment(InformationSegment.SegmentD);

        Assert.Equal(0xFF, infoMemory.ReadByte(0x1800));
    }

    [Fact]
    public void EraseSegment_UnprotectedSegment_ErasesSecondByte()
    {
        var infoMemory = new InformationMemory(_logger);

        // Write some data to Segment D first
        infoMemory.WriteByte(0x1801, 0x43);

        infoMemory.EraseSegment(InformationSegment.SegmentD);

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
    public void StoreCalibrationData_SegmentAUnprotected_ReturnsTrue()
    {
        var infoMemory = new InformationMemory(_logger);

        // Unprotect Segment A for this test
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentA, false);

        byte[] calibrationData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        bool result = infoMemory.StoreCalibrationData(calibrationData);

        Assert.True(result);
    }

    [Fact]
    public void StoreCalibrationData_SegmentAUnprotected_StoresCorrectBytesRead()
    {
        var infoMemory = new InformationMemory(_logger);

        // Unprotect Segment A for this test
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentA, false);

        byte[] calibrationData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        infoMemory.StoreCalibrationData(calibrationData);

        // Verify data was stored
        byte[] buffer = new byte[calibrationData.Length];
        int bytesRead = infoMemory.ReadCalibrationData(buffer);
        Assert.Equal(calibrationData.Length, bytesRead);
    }

    [Fact]
    public void StoreCalibrationData_SegmentAUnprotected_StoresCorrectData()
    {
        var infoMemory = new InformationMemory(_logger);

        // Unprotect Segment A for this test
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentA, false);

        byte[] calibrationData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        infoMemory.StoreCalibrationData(calibrationData);

        // Verify data was stored
        byte[] buffer = new byte[calibrationData.Length];
        infoMemory.ReadCalibrationData(buffer);
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
    public void ReadCalibrationData_ReturnsCorrectByteCount()
    {
        var infoMemory = new InformationMemory(_logger);

        // Unprotect Segment A and store some data
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentA, false);
        byte[] originalData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        infoMemory.StoreCalibrationData(originalData);

        byte[] buffer = new byte[10]; // Larger buffer
        int bytesRead = infoMemory.ReadCalibrationData(buffer);

        Assert.Equal(10, bytesRead); // Should read min(buffer.Length, segmentSize)
    }

    [Theory]
    [InlineData(0, 0x01)]
    [InlineData(1, 0x02)]
    [InlineData(2, 0x03)]
    [InlineData(3, 0x04)]
    [InlineData(4, 0xFF)]
    public void ReadCalibrationData_ReturnsCorrectDataAtIndex(int index, byte expectedValue)
    {
        var infoMemory = new InformationMemory(_logger);

        // Unprotect Segment A and store some data
        infoMemory.SetSegmentWriteProtection(InformationSegment.SegmentA, false);
        byte[] originalData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        infoMemory.StoreCalibrationData(originalData);

        byte[] buffer = new byte[10]; // Larger buffer
        infoMemory.ReadCalibrationData(buffer);

        Assert.Equal(expectedValue, buffer[index]);
    }

    [Fact]
    public void Clear_WithMixedProtection_ReturnsFalse()
    {
        var infoMemory = new InformationMemory(_logger);

        // Write data to all segments first (unprotected ones)
        infoMemory.WriteByte(0x1800, 0x42); // Segment D
        infoMemory.WriteByte(0x1880, 0x43); // Segment C
        infoMemory.WriteByte(0x1900, 0x44); // Segment B
                                            // Segment A is protected, so we can't write to it

        bool result = infoMemory.Clear();

        Assert.False(result); // Should return false because Segment A couldn't be cleared
    }

    [Fact]
    public void Clear_WithMixedProtection_ClearsSegmentD()
    {
        var infoMemory = new InformationMemory(_logger);

        // Write data to Segment D
        infoMemory.WriteByte(0x1800, 0x42);

        infoMemory.Clear();

        Assert.Equal(0xFF, infoMemory.ReadByte(0x1800)); // Segment D should be cleared
    }

    [Fact]
    public void Clear_WithMixedProtection_ClearsSegmentC()
    {
        var infoMemory = new InformationMemory(_logger);

        // Write data to Segment C
        infoMemory.WriteByte(0x1880, 0x43);

        infoMemory.Clear();

        Assert.Equal(0xFF, infoMemory.ReadByte(0x1880)); // Segment C should be cleared
    }

    [Fact]
    public void Clear_WithMixedProtection_ClearsSegmentB()
    {
        var infoMemory = new InformationMemory(_logger);

        // Write data to Segment B
        infoMemory.WriteByte(0x1900, 0x44);

        infoMemory.Clear();

        Assert.Equal(0xFF, infoMemory.ReadByte(0x1900)); // Segment B should be cleared
    }

    [Fact]
    public void Initialize_WithPattern_ReturnsFalse()
    {
        var infoMemory = new InformationMemory(_logger);
        const byte pattern = 0x55;

        bool result = infoMemory.Initialize(pattern);

        Assert.False(result); // Should return false because Segment A couldn't be initialized
    }

    [Fact]
    public void Initialize_WithPattern_InitializesSegmentD()
    {
        var infoMemory = new InformationMemory(_logger);
        const byte pattern = 0x55;

        infoMemory.Initialize(pattern);

        Assert.Equal(pattern, infoMemory.ReadByte(0x1800)); // Segment D should have pattern
    }

    [Fact]
    public void Initialize_WithPattern_InitializesSegmentC()
    {
        var infoMemory = new InformationMemory(_logger);
        const byte pattern = 0x55;

        infoMemory.Initialize(pattern);

        Assert.Equal(pattern, infoMemory.ReadByte(0x1880)); // Segment C should have pattern
    }

    [Fact]
    public void Initialize_WithPattern_InitializesSegmentB()
    {
        var infoMemory = new InformationMemory(_logger);
        const byte pattern = 0x55;

        infoMemory.Initialize(pattern);

        Assert.Equal(pattern, infoMemory.ReadByte(0x1900)); // Segment B should have pattern
    }

    [Fact]
    public void Initialize_WithPattern_SegmentARemainsErased()
    {
        var infoMemory = new InformationMemory(_logger);
        const byte pattern = 0x55;

        infoMemory.Initialize(pattern);

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

    [Theory]
    [InlineData(0x1980, true)]  // Start address
    [InlineData(0x19FF, true)]  // End address
    [InlineData(0x1990, true)]  // Middle address
    [InlineData(0x197F, false)] // Before start
    [InlineData(0x1A00, false)] // After end
    public void InformationSegmentInfo_Contains_ReturnsExpectedResult(ushort address, bool expected)
    {
        var segmentInfo = new InformationSegmentInfo(
            InformationSegment.SegmentA, 0x1980, 0x19FF, true, "Test segment");

        bool result = segmentInfo.Contains(address);

        Assert.Equal(expected, result);
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
