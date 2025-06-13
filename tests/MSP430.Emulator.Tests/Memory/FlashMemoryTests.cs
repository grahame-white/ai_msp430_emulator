using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Memory;

/// <summary>
/// Unit tests for the FlashMemory class.
/// 
/// Note: MSP430FR2355 uses FRAM, not Flash. This class manages FRAM memory:
/// - FRAM address range 0x4000-0xBFFF (per SLASEC4D Table 6-1)
/// - Non-volatile data retention without power
/// - Byte-level write operations
/// - No erase cycles required (unlike traditional Flash)
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 6: FRAM
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6.1: Memory Organization
/// </summary>
public class FlashMemoryTests
{
    private readonly TestLogger _logger;

    public FlashMemoryTests()
    {
        _logger = new TestLogger();
    }

    [Theory]
    [InlineData(4096, 4096)]
    [InlineData(8192, 8192)]
    [InlineData(2048, 2048)]
    public void Constructor_ValidParameters_SetsSize(int size, int expectedSize)
    {
        var flash = new FlashMemory(0x8000, size, 512, _logger);

        Assert.Equal(expectedSize, flash.Size);
    }

    [Theory]
    [InlineData(0x8000, 0x8000)]
    [InlineData(0x4000, 0x4000)]
    [InlineData(0xC000, 0xC000)]
    public void Constructor_ValidParameters_SetsBaseAddress(ushort baseAddress, ushort expectedBaseAddress)
    {
        var flash = new FlashMemory(baseAddress, 4096, 512, _logger);

        Assert.Equal(expectedBaseAddress, flash.BaseAddress);
    }

    [Theory]
    [InlineData(0x8000, 4096, 0x8FFF)]
    [InlineData(0x4000, 2048, 0x47FF)]
    [InlineData(0xC000, 1024, 0xC3FF)]
    public void Constructor_ValidParameters_SetsEndAddress(ushort baseAddress, int size, ushort expectedEndAddress)
    {
        var flash = new FlashMemory(baseAddress, size, 512, _logger);

        Assert.Equal(expectedEndAddress, flash.EndAddress);
    }

    [Theory]
    [InlineData(512, 512)]
    [InlineData(1024, 1024)]
    [InlineData(256, 256)]
    public void Constructor_ValidParameters_SetsSectorSize(int sectorSize, int expectedSectorSize)
    {
        var flash = new FlashMemory(0x8000, 4096, sectorSize, _logger);

        Assert.Equal(expectedSectorSize, flash.SectorSize);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsDefaultProtectionLevel()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.Equal(FlashProtectionLevel.None, flash.ProtectionLevel);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsDefaultControllerState()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.Equal(FlashControllerState.Locked, flash.ControllerState);
    }

    [Theory]
    [InlineData(4096)]
    public void Constructor_NullLogger_SetsSize(int expectedSize)
    {
        var flash = new FlashMemory(0x8000, expectedSize);

        Assert.Equal(expectedSize, flash.Size);
    }

    [Theory]
    [InlineData(0x8000)]
    public void Constructor_NullLogger_SetsBaseAddress(ushort expectedBaseAddress)
    {
        var flash = new FlashMemory(expectedBaseAddress, 4096);

        Assert.Equal(expectedBaseAddress, flash.BaseAddress);
    }

    [Theory]
    [InlineData(512)]     // Below minimum
    [InlineData(300000)]  // Above maximum
    [InlineData(0)]       // Zero size
    [InlineData(-1)]      // Negative size
    public void Constructor_InvalidSize_ThrowsArgumentOutOfRangeException(int invalidSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FlashMemory(0x8000, invalidSize, 512, _logger));
    }

    [Theory]
    [InlineData(500)] // Size not divisible by sector size
    [InlineData(0)]   // Zero sector size
    [InlineData(-1)]  // Negative sector size
    public void Constructor_InvalidSectorSize_ThrowsArgumentException(int invalidSectorSize)
    {
        Assert.Throws<ArgumentException>(() => new FlashMemory(0x8000, 4096, invalidSectorSize, _logger));
    }

    [Fact]
    public void Constructor_MemoryOverflow_ThrowsArgumentException()
    {
        // Base address 0xFF00 + 4096 bytes would overflow 16-bit address space
        Assert.Throws<ArgumentException>(() => new FlashMemory(0xFF00, 4096, 512, _logger));
    }

    [Fact]
    public void ReadByte_ValidAddress_ReturnsErasedValue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        byte value = flash.ReadByte(0x8000);

        Assert.Equal(FlashMemory.ErasedPattern, value);
    }

    [Theory]
    [InlineData(0x7FFF)] // One below base
    [InlineData(0x9000)] // One above end
    public void ReadByte_InvalidAddress_ThrowsArgumentOutOfRangeException(ushort invalidAddress)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        MemoryTestHelpers.ValidateReadByteThrowsForInvalidAddress(flash, invalidAddress);
    }

    [Fact]
    public void ReadWord_ValidAddress_ReturnsErasedValue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        ushort value = flash.ReadWord(0x8000);

        Assert.Equal((ushort)0xFFFF, value);
    }

    [Theory]
    [InlineData(0x8001)] // Odd address (not word-aligned)
    [InlineData(0x8003)] // Another odd address
    public void ReadWord_UnalignedAddress_ThrowsArgumentException(ushort unalignedAddress)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        MemoryTestHelpers.ValidateReadWordThrowsForUnalignedAddress(flash, unalignedAddress);
    }

    [Fact]
    public void ProgramByte_WhenLocked_ReturnsFalse()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        bool result = flash.ProgramByte(0x8000, 0xAB);

        Assert.False(result);
    }

    [Fact]
    public void ProgramByte_WhenLocked_DoesNotChangeMemory()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        flash.ProgramByte(0x8000, 0xAB);

        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8000));
    }

    [Fact]
    public void ProgramByte_WhenUnlocked_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        bool result = flash.ProgramByte(0x8000, 0xAB);

        Assert.True(result);
    }

    [Fact]
    public void ProgramByte_WhenUnlocked_WritesValue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        flash.ProgramByte(0x8000, 0xAB);

        Assert.Equal(0xAB, flash.ReadByte(0x8000));
    }

    [Fact]
    public void ProgramByte_RequiringErase_ReturnsFalse()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        // First program sets some bits to 0
        flash.ProgramByte(0x8000, 0xAB);

        // Try to program a value that would require setting 0 bits to 1
        bool result = flash.ProgramByte(0x8000, 0xFF);

        Assert.False(result);
    }

    [Fact]
    public void ProgramByte_RequiringErase_DoesNotChangeMemory()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        // First program sets some bits to 0
        flash.ProgramByte(0x8000, 0xAB);

        // Try to program a value that would require setting 0 bits to 1
        flash.ProgramByte(0x8000, 0xFF);

        Assert.Equal(0xAB, flash.ReadByte(0x8000));
    }

    [Fact]
    public void ProgramWord_WhenUnlocked_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        bool result = flash.ProgramWord(0x8000, 0x1234);

        Assert.True(result);
    }

    [Fact]
    public void ProgramWord_WhenUnlocked_WritesValue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        flash.ProgramWord(0x8000, 0x1234);

        Assert.Equal((ushort)0x1234, flash.ReadWord(0x8000));
    }

    [Fact]
    public void EraseSector_WhenUnlocked_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        // Program some data first
        flash.ProgramByte(0x8000, 0xAB);

        bool result = flash.EraseSector(0x8000);

        Assert.True(result);
    }

    [Theory]
    [InlineData(0x8000)] // Start of sector
    [InlineData(0x8001)] // Middle of sector
    [InlineData(0x81FF)] // End of sector
    public void EraseSector_WhenUnlocked_ErasesMemoryLocations(ushort address)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        // Program some data first
        flash.ProgramByte(0x8000, 0xAB);
        flash.ProgramByte(0x8001, 0xCD);

        flash.EraseSector(0x8000);

        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(address));
    }

    [Fact]
    public void EraseSector_WhenLocked_ReturnsFalse()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        bool result = flash.EraseSector(0x8000);

        Assert.False(result);
    }

    [Fact]
    public void MassErase_WhenUnlocked_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        // Program some data first
        flash.ProgramByte(0x8000, 0xAB);

        bool result = flash.MassErase();

        Assert.True(result);
    }

    [Theory]
    [InlineData(0x8000)] // Start of flash
    [InlineData(0x8500)] // Different sector
    [InlineData(0x8FFF)] // End of flash
    public void MassErase_WhenUnlocked_ErasesAllMemory(ushort address)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        // Program some data first
        flash.ProgramByte(0x8000, 0xAB);
        flash.ProgramByte(0x8500, 0xCD);

        flash.MassErase();

        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(address));
    }

    [Fact]
    public void Unlock_ValidKey_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        bool result = flash.Unlock(0xA555);

        Assert.True(result);
    }

    [Fact]
    public void Unlock_ValidKey_SetsControllerState()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        flash.Unlock(0xA555);

        Assert.Equal(FlashControllerState.Unlocked, flash.ControllerState);
    }

    [Theory]
    [InlineData(0x1234)] // Wrong key format
    [InlineData(0x5555)] // Wrong key format
    [InlineData(0x0000)] // Wrong key format
    public void Unlock_InvalidKey_ReturnsFalse(ushort invalidKey)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        bool result = flash.Unlock(invalidKey);

        Assert.False(result);
    }

    [Theory]
    [InlineData(0x1234)] // Wrong key format
    [InlineData(0x5555)] // Wrong key format
    [InlineData(0x0000)] // Wrong key format
    public void Unlock_InvalidKey_KeepsLockedState(ushort invalidKey)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        flash.Unlock(invalidKey);

        Assert.Equal(FlashControllerState.Locked, flash.ControllerState);
    }

    [Fact]
    public void Lock_ChangesToLockedState()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        flash.Lock();

        Assert.Equal(FlashControllerState.Locked, flash.ControllerState);
    }

    [Fact]
    public void SetProtectionLevel_WriteProtected_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        bool protectionResult = flash.SetProtectionLevel(FlashProtectionLevel.WriteProtected);

        Assert.True(protectionResult);
    }

    [Fact]
    public void SetProtectionLevel_WriteProtected_BlocksProgramming()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.SetProtectionLevel(FlashProtectionLevel.WriteProtected);

        flash.Unlock(0xA555);
        bool programResult = flash.ProgramByte(0x8000, 0xAB);

        Assert.False(programResult);
    }

    [Fact]
    public void SetProtectionLevel_WriteProtected_DoesNotChangeMemory()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.SetProtectionLevel(FlashProtectionLevel.WriteProtected);

        flash.Unlock(0xA555);
        flash.ProgramByte(0x8000, 0xAB);

        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8000));
    }

    [Theory]
    [InlineData(0x8000, 0)] // First sector
    [InlineData(0x81FF, 0)] // Still first sector
    [InlineData(0x8200, 1)] // Second sector
    [InlineData(0x8FFF, 7)] // Last sector
    public void GetSectorNumber_ValidAddress_ReturnsCorrectSector(ushort address, int expectedSector)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.Equal(expectedSector, flash.GetSectorNumber(address));
    }

    [Theory]
    [InlineData(0x8000, 0x8000)] // First sector start
    [InlineData(0x81FF, 0x8000)] // First sector end
    [InlineData(0x8200, 0x8200)] // Second sector start
    [InlineData(0x83FF, 0x8200)] // Second sector end
    [InlineData(0x8400, 0x8400)] // Third sector start
    [InlineData(0x8600, 0x8600)] // Fourth sector start
    [InlineData(0x8800, 0x8800)] // Fifth sector start
    [InlineData(0x8A00, 0x8A00)] // Sixth sector start
    [InlineData(0x8C00, 0x8C00)] // Seventh sector start
    [InlineData(0x8E00, 0x8E00)] // Eighth sector start
    [InlineData(0x8FFF, 0x8E00)] // Last sector end
    public void GetSectorBaseAddress_ValidAddress_ReturnsCorrectBase(ushort address, ushort expectedBase)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.Equal(expectedBase, flash.GetSectorBaseAddress(address));
    }

    [Theory]
    [InlineData(0x8000)]
    [InlineData(0x8FFF)]
    [InlineData(0x8500)]
    public void IsAddressInBounds_ValidAddresses_ReturnsTrue(ushort address)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.True(flash.IsAddressInBounds(address));
    }

    [Theory]
    [InlineData(0x7FFF)]
    [InlineData(0x9000)]
    public void IsAddressInBounds_InvalidAddresses_ReturnsFalse(ushort address)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.False(flash.IsAddressInBounds(address));
    }

    [Theory]
    [InlineData(0x8000, 100)]
    [InlineData(0x8000, 4096)]
    [InlineData(0x8FFF, 1)]
    public void IsRangeInBounds_ValidRanges_ReturnsTrue(ushort address, int length)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.True(flash.IsRangeInBounds(address, length));
    }

    [Theory]
    [InlineData(0x7FFF, 100)]  // Starts outside
    [InlineData(0x8000, 4097)] // Extends beyond
    [InlineData(0x8000, 0)]    // Zero length
    [InlineData(0x8000, -1)]   // Negative length
    public void IsRangeInBounds_InvalidRanges_ReturnsFalse(ushort address, int length)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.False(flash.IsRangeInBounds(address, length));
    }

    [Fact]
    public void GetReadCycles_ReturnsDefaultCycles()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        uint cycles = flash.GetReadCycles(0x8000);

        Assert.Equal(FlashMemory.DefaultReadCycles, cycles);
    }

    [Theory]
    [InlineData(false, FlashController.ByteProgramCycles)]
    [InlineData(true, FlashController.WordProgramCycles)]
    public void GetProgramCycles_ReturnsCorrectCycles(bool isWord, uint expectedCycles)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        uint cycles = flash.GetProgramCycles(0x8000, isWord);

        Assert.Equal(expectedCycles, cycles);
    }

    [Theory]
    [InlineData(FlashOperation.SectorErase, FlashController.SectorEraseCycles)]
    [InlineData(FlashOperation.MassErase, FlashController.MassEraseCycles)]
    public void GetEraseCycles_ReturnsCorrectCycles(FlashOperation operation, uint expectedCycles)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        uint cycles = flash.GetEraseCycles(operation);

        Assert.Equal(expectedCycles, cycles);
    }

    [Theory]
    [InlineData(0x8000)]
    [InlineData(0x8FFF)]
    public void Clear_SetsAllBytesToErasedPattern(ushort address)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        flash.Clear();

        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(address));
    }

    [Theory]
    [InlineData(0x8000)]
    [InlineData(0x8FFF)]
    public void Initialize_SetsAllBytesToPattern(ushort address)
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        flash.Initialize(0xAA);

        Assert.Equal(0xAA, flash.ReadByte(address));
    }

    [Theory]
    [InlineData("FlashMemory initialized")]
    [InlineData("Flash ReadByte")]
    [InlineData("Flash controller unlocked")]
    [InlineData("Flash ProgramByte")]
    [InlineData("Flash EraseSector")]
    public void Operations_LogCorrectMessages(string expectedMessage)
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        flash.ReadByte(0x8000);
        flash.Unlock(0xA555);
        flash.ProgramByte(0x8000, 0xAB);
        flash.EraseSector(0x8000);

        // Check that debug message was logged
        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains(expectedMessage));
    }
}
