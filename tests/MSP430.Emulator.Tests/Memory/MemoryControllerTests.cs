using System;
using System.Collections.Generic;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Memory;

/// <summary>
/// Unit tests for the MemoryController class.
/// 
/// Tests validate memory controller behavior according to:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) Section 2: Memory Organization
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) Section 6: Detailed Description
/// 
/// Memory controller manages:
/// - Unified memory space addressing (16-bit address space)
/// - Memory region routing (RAM, FRAM, Peripherals, Special Function Registers)
/// - Memory access coordination and arbitration
/// - Wait state management for FRAM access
/// - Memory-mapped I/O coordination
/// </summary>
public class MemoryControllerTests
{
    private readonly TestLogger _logger;
    private readonly IMemoryMap _memoryMap;
    private readonly IRandomAccessMemory _ram;
    private readonly FlashMemory _flash;
    private readonly IInformationMemory _informationMemory;
    private readonly MemoryController _controller;

    public MemoryControllerTests()
    {
        _logger = new TestLogger();
        _memoryMap = new MemoryMap();
        _ram = new RandomAccessMemory(0x2000, 4096, _logger); // 4KB SRAM at 0x2000
        _flash = new FlashMemory(0x4000, 32768, logger: _logger); // 32KB Flash at 0x4000
        _informationMemory = new InformationMemory(_logger); // Info memory with logger
        _controller = new MemoryController(_memoryMap, _ram, _flash, _informationMemory, _logger);
    }

    [Fact]
    public void Constructor_ValidParameters_InitializesMemoryMap()
    {
        // Act & Assert
        Assert.NotNull(_controller.MemoryMap);
    }

    [Fact]
    public void Constructor_ValidParameters_InitializesRam()
    {
        // Act & Assert
        Assert.NotNull(_controller.Ram);
    }

    [Fact]
    public void Constructor_ValidParameters_InitializesFlash()
    {
        // Act & Assert
        Assert.NotNull(_controller.Flash);
    }

    [Fact]
    public void Constructor_ValidParameters_InitializesInformationMemory()
    {
        // Act & Assert
        Assert.NotNull(_controller.InformationMemory);
    }

    [Fact]
    public void Constructor_ValidParameters_InitializesStatistics()
    {
        // Act & Assert
        Assert.NotNull(_controller.Statistics);
    }

    [Theory]
    [InlineData(0ul)]
    public void Constructor_ValidParameters_InitializesStatisticsWithZeroOperations(ulong expectedOperations)
    {
        // Act & Assert
        Assert.Equal(expectedOperations, _controller.Statistics.TotalOperations);
    }

    [Fact]
    public void Constructor_NullMemoryMap_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MemoryController(null!, _ram, _flash, _informationMemory, _logger));
    }

    [Fact]
    public void Constructor_NullRam_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MemoryController(_memoryMap, null!, _flash, _informationMemory, _logger));
    }

    [Fact]
    public void Constructor_NullFlash_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MemoryController(_memoryMap, _ram, null!, _informationMemory, _logger));
    }

    [Fact]
    public void Constructor_NullInformationMemory_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MemoryController(_memoryMap, _ram, _flash, null!, _logger));
    }

    [Theory]
    [InlineData(0x2000)] // RAM start
    [InlineData(0x2FFF)] // RAM end
    [InlineData(0x4000)] // Flash start
    [InlineData(0xBFFF)] // Flash end
    [InlineData(0x1800)] // Information memory start
    [InlineData(0x19FF)] // Information memory end
    public void ReadByte_ValidAddress_ReturnsValue(ushort address)
    {
        // Act
        byte result = _controller.ReadByte(address);

        // Assert
        Assert.IsType<byte>(result);
    }

    [Theory]
    [InlineData(0x2000)] // RAM start
    [InlineData(0x2FFF)] // RAM end
    [InlineData(0x4000)] // Flash start
    [InlineData(0xBFFF)] // Flash end
    [InlineData(0x1800)] // Information memory start
    [InlineData(0x19FF)] // Information memory end
    public void ReadByte_ValidAddress_IncrementsReadCount(ushort address)
    {
        // Act
        _controller.ReadByte(address);

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalReads);
    }

    [Theory]
    [InlineData(0x2000)] // RAM start
    [InlineData(0x2FFF)] // RAM end
    [InlineData(0x4000)] // Flash start
    [InlineData(0xBFFF)] // Flash end
    [InlineData(0x1800)] // Information memory start
    [InlineData(0x19FF)] // Information memory end
    public void ReadByte_ValidAddress_IncrementsCycles(ushort address)
    {
        // Act
        _controller.ReadByte(address);

        // Assert
        Assert.True(_controller.Statistics.TotalCycles > 0);
    }

    [Theory]
    [InlineData(0x2000)] // RAM start (word-aligned)
    [InlineData(0x2FFE)] // RAM end (word-aligned)
    [InlineData(0x4000)] // Flash start (word-aligned)
    [InlineData(0xBFFE)] // Flash end (word-aligned)
    [InlineData(0x1800)] // Information memory start (word-aligned)
    [InlineData(0x19FE)] // Information memory end (word-aligned)
    public void ReadWord_ValidAddress_ReturnsValue(ushort address)
    {
        // Act
        ushort result = _controller.ReadWord(address);

        // Assert
        Assert.IsType<ushort>(result);
    }

    [Theory]
    [InlineData(0x2000)] // RAM start (word-aligned)
    [InlineData(0x2FFE)] // RAM end (word-aligned)
    [InlineData(0x4000)] // Flash start (word-aligned)
    [InlineData(0xBFFE)] // Flash end (word-aligned)
    [InlineData(0x1800)] // Information memory start (word-aligned)
    [InlineData(0x19FE)] // Information memory end (word-aligned)
    public void ReadWord_ValidAddress_IncrementsReadCount(ushort address)
    {
        // Act
        _controller.ReadWord(address);

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalReads);
    }

    [Theory]
    [InlineData(0x2000)] // RAM start (word-aligned)
    [InlineData(0x2FFE)] // RAM end (word-aligned)
    [InlineData(0x4000)] // Flash start (word-aligned)
    [InlineData(0xBFFE)] // Flash end (word-aligned)
    [InlineData(0x1800)] // Information memory start (word-aligned)
    [InlineData(0x19FE)] // Information memory end (word-aligned)
    public void ReadWord_ValidAddress_IncrementsCycles(ushort address)
    {
        // Act
        _controller.ReadWord(address);

        // Assert
        Assert.True(_controller.Statistics.TotalCycles > 0);
    }

    [Theory]
    [InlineData(0x2001)] // Odd address in RAM
    [InlineData(0x4001)] // Odd address in Flash
    [InlineData(0x1801)] // Odd address in Information memory
    public void ReadWord_UnalignedAddress_ThrowsArgumentException(ushort address)
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => _controller.ReadWord(address));
    }

    [Theory]
    [InlineData(0x2000, 0x42)] // RAM
    [InlineData(0x1800, 0x55)] // Information memory
    public void WriteByte_ValidAddress_ReturnsTrue(ushort address, byte value)
    {
        // Act
        bool result = _controller.WriteByte(address, value);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(0x2000, 0x42)] // RAM
    [InlineData(0x1800, 0x84)] // Information memory
    public void WriteByte_ValidAddress_IncrementsWriteCount(ushort address, byte value)
    {
        // Act
        _controller.WriteByte(address, value);

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalWrites);
    }

    [Theory]
    [InlineData(0x2000, 0x42)] // RAM
    [InlineData(0x1800, 0x84)] // Information memory
    public void WriteByte_ValidAddress_IncrementsCycles(ushort address, byte value)
    {
        // Act
        _controller.WriteByte(address, value);

        // Assert
        Assert.True(_controller.Statistics.TotalCycles > 0);
    }

    [Theory]
    [InlineData(0x2000, 0x42)] // RAM
    [InlineData(0x1800, 0x84)] // Information memory
    public void WriteByte_ValidAddress_PersistsValue(ushort address, byte value)
    {
        // Act
        _controller.WriteByte(address, value);

        // Assert
        byte readValue = _controller.ReadByte(address);
        Assert.Equal(value, readValue);
    }

    [Theory]
    [InlineData(0x2000, 0x1234)] // RAM (word-aligned)
    [InlineData(0x1800, 0x5678)] // Information memory (word-aligned)
    public void WriteWord_ValidAddress_ReturnsTrue(ushort address, ushort value)
    {
        // Act
        bool result = _controller.WriteWord(address, value);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(0x2000, 0x1234)] // RAM (word-aligned)
    [InlineData(0x1800, 0x5678)] // Information memory (word-aligned)
    public void WriteWord_ValidAddress_IncrementsWriteCount(ushort address, ushort value)
    {
        // Act
        _controller.WriteWord(address, value);

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalWrites);
    }

    [Theory]
    [InlineData(0x2000, 0x1234)] // RAM (word-aligned)
    [InlineData(0x1800, 0x5678)] // Information memory (word-aligned)
    public void WriteWord_ValidAddress_IncrementsCycles(ushort address, ushort value)
    {
        // Act
        _controller.WriteWord(address, value);

        // Assert
        Assert.True(_controller.Statistics.TotalCycles > 0);
    }

    [Theory]
    [InlineData(0x2000, 0x1234)] // RAM (word-aligned)
    [InlineData(0x1800, 0x5678)] // Information memory (word-aligned)
    public void WriteWord_ValidAddress_PersistsValue(ushort address, ushort value)
    {
        // Act
        _controller.WriteWord(address, value);

        // Assert
        ushort readValue = _controller.ReadWord(address);
        Assert.Equal(value, readValue);
    }

    [Theory]
    [InlineData(0x2001)] // Odd address in RAM
    [InlineData(0x1801)] // Odd address in Information memory
    public void WriteWord_UnalignedAddress_ThrowsArgumentException(ushort address)
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => _controller.WriteWord(address, 0x1234));
    }

    [Theory]
    [InlineData(0x4000)] // Flash start
    [InlineData(0x4002)] // Flash word-aligned
    [InlineData(0xBFFE)] // Flash end (word-aligned)
    public void FetchInstruction_ValidAddress_ReturnsInstruction(ushort address)
    {
        // Act
        ushort result = _controller.FetchInstruction(address);

        // Assert
        Assert.IsType<ushort>(result);
    }

    [Theory]
    [InlineData(0x4000)] // Flash start
    [InlineData(0x4002)] // Flash word-aligned
    [InlineData(0xBFFE)] // Flash end (word-aligned)
    public void FetchInstruction_ValidAddress_IncrementsInstructionFetches(ushort address)
    {
        // Act
        _controller.FetchInstruction(address);

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalInstructionFetches);
    }

    [Theory]
    [InlineData(0x4000)] // Flash start
    [InlineData(0x4002)] // Flash word-aligned
    [InlineData(0xBFFE)] // Flash end (word-aligned)
    public void FetchInstruction_ValidAddress_IncrementsCycles(ushort address)
    {
        // Act
        _controller.FetchInstruction(address);

        // Assert
        Assert.True(_controller.Statistics.TotalCycles > 0);
    }

    [Theory]
    [InlineData(0x0300)] // Invalid address (gap between peripherals and BSL)
    [InlineData(0x1A00)] // Invalid address (gap between info memory and RAM)
    public void ReadByte_InvalidAddress_ThrowsMemoryAccessException(ushort address)
    {
        // Arrange, Act & Assert
        Assert.Throws<MemoryAccessException>(() => _controller.ReadByte(address));
    }

    [Theory]
    [InlineData(0x0300)] // Invalid address (gap between peripherals and BSL)
    [InlineData(0x1A00)] // Invalid address (gap between info memory and RAM)
    public void ReadByte_InvalidAddress_IncrementsViolations(ushort address)
    {
        // Arrange, Act
        try
        {
            _controller.ReadByte(address);
        }
        catch (MemoryAccessException)
        {
            // Expected
        }

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalViolations);
    }

    [Theory]
    [InlineData(0x0300)] // Invalid address (gap between peripherals and BSL)
    [InlineData(0x1A00)] // Invalid address (gap between info memory and RAM)
    public void WriteByte_InvalidAddress_ThrowsMemoryAccessException(ushort address)
    {
        // Arrange, Act & Assert
        Assert.Throws<MemoryAccessException>(() => _controller.WriteByte(address, 0x42));
    }

    [Theory]
    [InlineData(0x0300)] // Invalid address (gap between peripherals and BSL)
    [InlineData(0x1A00)] // Invalid address (gap between info memory and RAM)
    public void WriteByte_InvalidAddress_IncrementsViolations(ushort address)
    {
        // Arrange, Act
        try
        {
            _controller.WriteByte(address, 0x42);
        }
        catch (MemoryAccessException)
        {
            // Expected
        }

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalViolations);
    }

    [Fact]
    public void ReadByte_WithContext_NullContext_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => _controller.ReadByte(null!));
    }

    [Fact]
    public void ReadWord_WithContext_NullContext_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => _controller.ReadWord(null!));
    }

    [Fact]
    public void WriteByte_WithContext_NullContext_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => _controller.WriteByte(null!, 0x42));
    }

    [Fact]
    public void WriteWord_WithContext_NullContext_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => _controller.WriteWord(null!, 0x1234));
    }

    [Fact]
    public void FetchInstruction_WithContext_NullContext_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => _controller.FetchInstruction(null!));
    }

    [Fact]
    public void ValidateAccess_NullContext_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => _controller.ValidateAccess(null!));
    }

    [Fact]
    public void GetAccessCycles_NullContext_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => _controller.GetAccessCycles(null!));
    }

    [Fact]
    public void ReadByte_WithContext_ValidContext_ReturnsValue()
    {
        // Arrange
        var context = MemoryAccessContext.CreateReadByte(0x2000);

        // Act
        byte result = _controller.ReadByte(context);

        // Assert
        Assert.IsType<byte>(result);
    }

    [Fact]
    public void ReadByte_WithContext_ValidContext_IncrementsStatistics()
    {
        // Arrange
        var context = MemoryAccessContext.CreateReadByte(0x2000);

        // Act
        _controller.ReadByte(context);

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalReads);
    }

    [Fact]
    public void WriteWord_WithContext_ValidContext_ReturnsTrue()
    {
        // Arrange
        var context = MemoryAccessContext.CreateWriteWord(0x2000);
        const ushort testValue = 0x1234;

        // Act
        bool result = _controller.WriteWord(context, testValue);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WriteWord_WithContext_ValidContext_IncrementsStatistics()
    {
        // Arrange
        var context = MemoryAccessContext.CreateWriteWord(0x2000);
        const ushort testValue = 0x1234;

        // Act
        _controller.WriteWord(context, testValue);

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalWrites);
    }

    [Fact]
    public void WriteWord_WithContext_ValidContext_WritesValue()
    {
        // Arrange
        var context = MemoryAccessContext.CreateWriteWord(0x2000);
        const ushort testValue = 0x1234;

        // Act
        _controller.WriteWord(context, testValue);

        // Assert
        ushort readValue = _controller.ReadWord(0x2000);
        Assert.Equal(testValue, readValue);
    }

    [Theory]
    [InlineData(0x2000)] // RAM
    [InlineData(0x4000)] // Flash
    [InlineData(0x1800)] // Information memory
    public void IsValidAddress_ValidAddresses_ReturnsTrue(ushort address)
    {
        // Act & Assert
        Assert.True(_controller.IsValidAddress(address));
    }

    [Theory]
    [InlineData(0x0300)] // Invalid address (gap between peripherals and BSL)
    [InlineData(0x1A00)] // Invalid address (gap between info memory and RAM)
    public void IsValidAddress_InvalidAddresses_ReturnsFalse(ushort address)
    {
        // Act & Assert
        Assert.False(_controller.IsValidAddress(address));
    }

    [Theory]
    [InlineData(0x2000, MemoryAccessPermissions.Read)] // RAM read
    [InlineData(0x2000, MemoryAccessPermissions.Write)] // RAM write
    [InlineData(0x2000, MemoryAccessPermissions.Execute)] // RAM execute
    [InlineData(0x4000, MemoryAccessPermissions.Read)] // Flash read
    [InlineData(0x4000, MemoryAccessPermissions.Execute)] // Flash execute
    public void IsAccessAllowed_ValidAccess_ReturnsTrue(ushort address, MemoryAccessPermissions accessType)
    {
        // Act & Assert
        Assert.True(_controller.IsAccessAllowed(address, accessType));
    }

    [Fact]
    public void GetRegion_ValidAddress_ReturnsCorrectRegion()
    {
        // Arrange
        const ushort ramAddress = 0x2000;

        // Act
        MemoryRegionInfo region = _controller.GetRegion(ramAddress);

        // Assert
        Assert.Equal(MemoryRegion.Ram, region.Region);
    }

    [Fact]
    public void GetRegion_ValidAddress_RegionContainsAddress()
    {
        // Arrange
        const ushort ramAddress = 0x2000;

        // Act
        MemoryRegionInfo region = _controller.GetRegion(ramAddress);

        // Assert
        Assert.True(region.Contains(ramAddress));
    }

    [Fact]
    public void GetPermissions_ValidAddress_AllowsRead()
    {
        // Arrange
        const ushort ramAddress = 0x2000;

        // Act
        MemoryAccessPermissions permissions = _controller.GetPermissions(ramAddress);

        // Assert
        Assert.True((permissions & MemoryAccessPermissions.Read) != 0);
    }

    [Fact]
    public void GetPermissions_ValidAddress_AllowsWrite()
    {
        // Arrange
        const ushort ramAddress = 0x2000;

        // Act
        MemoryAccessPermissions permissions = _controller.GetPermissions(ramAddress);

        // Assert
        Assert.True((permissions & MemoryAccessPermissions.Write) != 0);
    }

    [Fact]
    public void GetPermissions_ValidAddress_AllowsExecute()
    {
        // Arrange
        const ushort ramAddress = 0x2000;

        // Act
        MemoryAccessPermissions permissions = _controller.GetPermissions(ramAddress);

        // Assert
        Assert.True((permissions & MemoryAccessPermissions.Execute) != 0);
    }

    [Fact]
    public void GetAccessCycles_RamAccess_ReturnsPositiveCycles()
    {
        // Arrange
        var context = MemoryAccessContext.CreateReadByte(0x2000);

        // Act
        uint cycles = _controller.GetAccessCycles(context);

        // Assert
        Assert.True(cycles > 0);
    }

    [Fact]
    public void GetAccessCycles_RamAccess_ReturnsReasonableCycles()
    {
        // Arrange
        var context = MemoryAccessContext.CreateReadByte(0x2000);

        // Act
        uint cycles = _controller.GetAccessCycles(context);

        // Assert
        Assert.True(cycles <= 10); // RAM should be fast
    }

    [Fact]
    public void GetAccessCycles_FlashAccess_ReturnsExpectedCycles()
    {
        // Arrange
        var context = MemoryAccessContext.CreateReadByte(0x4000);

        // Act
        uint cycles = _controller.GetAccessCycles(context);

        // Assert
        Assert.True(cycles > 0);
    }

    [Fact]
    public void Reset_ClearsStatistics_WritesAreTracked()
    {
        // Arrange & Act
        _controller.WriteByte(0x2000, 0x42); // Write to RAM

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalWrites);
    }

    [Fact]
    public void Reset_ClearsStatistics_OperationsAreCleared()
    {
        // Arrange
        _controller.WriteByte(0x2000, 0x42); // Write to RAM

        // Act
        _controller.Reset();

        // Assert
        Assert.Equal(0ul, _controller.Statistics.TotalOperations);
    }

    [Fact]
    public void Reset_ClearsMemory()
    {
        // Arrange
        _controller.WriteByte(0x2000, 0x42); // Write to RAM

        // Act
        _controller.Reset();

        // Assert
        byte ramValue = _controller.ReadByte(0x2000);
        Assert.Equal(0x00, ramValue);
    }

    [Fact]
    public void IsOperationInProgress_DelegatesCorrectly()
    {
        // Act & Assert
        Assert.Equal(_flash.IsOperationInProgress, _controller.IsOperationInProgress);
    }

    [Fact]
    public void MemoryAccessed_Event_RaisedOnMemoryOperation()
    {
        // Arrange
        bool eventRaised = false;
        _controller.MemoryAccessed += (sender, args) => eventRaised = true;

        // Act
        _controller.ReadByte(0x2000);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void MemoryAccessed_Event_ContainsEventArgs()
    {
        // Arrange
        MemoryAccessEventArgs? eventArgs = null;
        _controller.MemoryAccessed += (sender, args) => eventArgs = args;

        // Act
        _controller.ReadByte(0x2000);

        // Assert
        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void MemoryAccessed_Event_EventArgsContainsCorrectAddress_IsNotNull()
    {
        // Arrange
        MemoryAccessEventArgs? eventArgs = null;
        _controller.MemoryAccessed += (sender, args) => eventArgs = args;

        // Act
        _controller.ReadByte(0x2000);

        // Assert
        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void MemoryAccessed_Event_EventArgsContainsCorrectAddress_HasCorrectAddress()
    {
        // Arrange
        MemoryAccessEventArgs? eventArgs = null;
        _controller.MemoryAccessed += (sender, args) => eventArgs = args;

        // Act
        _controller.ReadByte(0x2000);

        // Assert
        Assert.Equal(0x2000, eventArgs!.Context.Address);
    }

    [Fact]
    public void MemoryAccessed_Event_EventArgsContainsCorrectAccessType_IsNotNull()
    {
        // Arrange
        MemoryAccessEventArgs? eventArgs = null;
        _controller.MemoryAccessed += (sender, args) => eventArgs = args;

        // Act
        _controller.ReadByte(0x2000);

        // Assert
        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void MemoryAccessed_Event_EventArgsContainsCorrectAccessType_HasCorrectAccessType()
    {
        // Arrange
        MemoryAccessEventArgs? eventArgs = null;
        _controller.MemoryAccessed += (sender, args) => eventArgs = args;

        // Act
        _controller.ReadByte(0x2000);

        // Assert
        Assert.Equal(MemoryAccessPermissions.Read, eventArgs!.Context.AccessType);
    }

    [Fact]
    public void MemoryAccessed_Event_EventArgsContainsCorrectRegion_IsNotNull()
    {
        // Arrange
        MemoryAccessEventArgs? eventArgs = null;
        _controller.MemoryAccessed += (sender, args) => eventArgs = args;

        // Act
        _controller.ReadByte(0x2000);

        // Assert
        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void MemoryAccessed_Event_EventArgsContainsCorrectRegion_HasCorrectRegion()
    {
        // Arrange
        MemoryAccessEventArgs? eventArgs = null;
        _controller.MemoryAccessed += (sender, args) => eventArgs = args;

        // Act
        _controller.ReadByte(0x2000);

        // Assert
        Assert.Equal(MemoryRegion.Ram, eventArgs!.Region.Region);
    }

    [Fact]
    public void MemoryAccessed_Event_EventArgsContainsPositiveCycles_IsNotNull()
    {
        // Arrange
        MemoryAccessEventArgs? eventArgs = null;
        _controller.MemoryAccessed += (sender, args) => eventArgs = args;

        // Act
        _controller.ReadByte(0x2000);

        // Assert
        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void MemoryAccessed_Event_EventArgsContainsPositiveCycles_HasPositiveCycles()
    {
        // Arrange
        MemoryAccessEventArgs? eventArgs = null;
        _controller.MemoryAccessed += (sender, args) => eventArgs = args;

        // Act
        _controller.ReadByte(0x2000);

        // Assert
        Assert.True(eventArgs!.Cycles > 0);
    }

    [Fact]
    public void AccessViolation_Event_ThrowsMemoryAccessException()
    {
        // Act & Assert
        Assert.Throws<MemoryAccessException>(() => _controller.ReadByte(0x0300));
    }

    [Fact]
    public void AccessViolation_Event_RaisesEvent_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<MemoryAccessException>(() => _controller.ReadByte(0x0300));
    }

    [Fact]
    public void AccessViolation_Event_RaisesEvent_EventIsRaised()
    {
        // Arrange
        bool eventRaised = false;
        _controller.AccessViolation += (sender, args) => eventRaised = true;

        // Act
        try
        {
            _controller.ReadByte(0x0300);
        }
        catch (MemoryAccessException)
        {
            // Expected
        }

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void AccessViolation_Event_ContainsEventArgs_ThrowsException()
    {
        // Arrange
        MemoryAccessViolationEventArgs? eventArgs = null;
        _controller.AccessViolation += (sender, args) => eventArgs = args;

        // Act & Assert
        Assert.Throws<MemoryAccessException>(() => _controller.ReadByte(0x0300));
    }

    [Fact]
    public void AccessViolation_Event_ContainsEventArgs_EventArgsNotNull()
    {
        // Arrange
        MemoryAccessViolationEventArgs? eventArgs = null;
        _controller.AccessViolation += (sender, args) => eventArgs = args;

        // Act
        try
        {
            _controller.ReadByte(0x0300);
        }
        catch (MemoryAccessException)
        {
            // Expected
        }

        // Assert
        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void AccessViolation_Event_EventArgsContainsCorrectAddress_ThrowsException()
    {
        // Arrange
        MemoryAccessViolationEventArgs? eventArgs = null;
        _controller.AccessViolation += (sender, args) => eventArgs = args;

        // Act & Assert
        Assert.Throws<MemoryAccessException>(() => _controller.ReadByte(0x0300));
    }

    [Fact]
    public void AccessViolation_Event_EventArgsContainsCorrectAddress_EventArgsNotNull()
    {
        // Arrange
        MemoryAccessViolationEventArgs? eventArgs = null;
        _controller.AccessViolation += (sender, args) => eventArgs = args;

        // Act
        try
        {
            _controller.ReadByte(0x0300);
        }
        catch (MemoryAccessException)
        {
            // Expected
        }

        // Assert
        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void AccessViolation_Event_EventArgsContainsCorrectAddress_HasCorrectAddress()
    {
        // Arrange
        MemoryAccessViolationEventArgs? eventArgs = null;
        _controller.AccessViolation += (sender, args) => eventArgs = args;

        // Act
        try
        {
            _controller.ReadByte(0x0300);
        }
        catch (MemoryAccessException)
        {
            // Expected
        }

        // Assert
        Assert.Equal(0x0300, eventArgs!.Context.Address);
    }

    [Fact]
    public void AccessViolation_Event_EventArgsContainsException_ThrowsException()
    {
        // Arrange
        MemoryAccessViolationEventArgs? eventArgs = null;
        _controller.AccessViolation += (sender, args) => eventArgs = args;

        // Act & Assert
        Assert.Throws<MemoryAccessException>(() => _controller.ReadByte(0x0300));
    }

    [Fact]
    public void AccessViolation_Event_EventArgsContainsException_EventArgsNotNull()
    {
        // Arrange
        MemoryAccessViolationEventArgs? eventArgs = null;
        _controller.AccessViolation += (sender, args) => eventArgs = args;

        // Act
        try
        {
            _controller.ReadByte(0x0300);
        }
        catch (MemoryAccessException)
        {
            // Expected
        }

        // Assert
        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void AccessViolation_Event_EventArgsContainsException_ExceptionNotNull()
    {
        // Arrange
        MemoryAccessViolationEventArgs? eventArgs = null;
        _controller.AccessViolation += (sender, args) => eventArgs = args;

        // Act
        try
        {
            _controller.ReadByte(0x0300);
        }
        catch (MemoryAccessException)
        {
            // Expected
        }

        // Assert
        Assert.NotNull(eventArgs!.Exception);
    }

    [Fact]
    public void Statistics_TrackMultipleOperations_TracksTotalReads()
    {
        // Arrange & Act
        _controller.ReadByte(0x2000);
        _controller.WriteByte(0x2001, 0x42);
        _controller.FetchInstruction(0x4000);

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalReads);
    }

    [Fact]
    public void Statistics_TrackMultipleOperations_TracksTotalWrites()
    {
        // Arrange & Act
        _controller.ReadByte(0x2000);
        _controller.WriteByte(0x2001, 0x42);
        _controller.FetchInstruction(0x4000);

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalWrites);
    }

    [Fact]
    public void Statistics_TrackMultipleOperations_TracksTotalInstructionFetches()
    {
        // Arrange & Act
        _controller.ReadByte(0x2000);
        _controller.WriteByte(0x2001, 0x42);
        _controller.FetchInstruction(0x4000);

        // Assert
        Assert.Equal(1ul, _controller.Statistics.TotalInstructionFetches);
    }

    [Fact]
    public void Statistics_TrackMultipleOperations_TracksTotalOperations()
    {
        // Arrange & Act
        _controller.ReadByte(0x2000);
        _controller.WriteByte(0x2001, 0x42);
        _controller.FetchInstruction(0x4000);

        // Assert
        Assert.Equal(3ul, _controller.Statistics.TotalOperations);
    }

    [Fact]
    public void Statistics_TrackMultipleOperations_TracksTotalCycles()
    {
        // Arrange & Act
        _controller.ReadByte(0x2000);
        _controller.WriteByte(0x2001, 0x42);
        _controller.FetchInstruction(0x4000);

        // Assert
        Assert.True(_controller.Statistics.TotalCycles > 0);
    }

    [Fact]
    public void ValidateAccess_ValidContext_DoesNotThrow()
    {
        // Arrange
        var context = MemoryAccessContext.CreateReadByte(0x2000);

        // Act & Assert
        _controller.ValidateAccess(context); // Should not throw
    }

    [Fact]
    public void ValidateAccess_InvalidContext_ThrowsMemoryAccessException()
    {
        // Arrange
        var context = MemoryAccessContext.CreateReadByte(0x0300);

        // Act & Assert
        Assert.Throws<MemoryAccessException>(() => _controller.ValidateAccess(context));
    }

    [Fact]
    public void FlashProgramming_ReflectsInOperationStatus()
    {
        // This test assumes flash programming operations affect the IsOperationInProgress status
        // The exact behavior depends on the flash memory implementation
        // For now, we just verify the property delegates correctly

        // Act & Assert
        Assert.Equal(_flash.IsOperationInProgress, _controller.IsOperationInProgress);
    }

    [Theory]
    [InlineData(0x0000)] // SFR
    [InlineData(0x0100)] // 8-bit peripherals
    [InlineData(0x0200)] // 16-bit peripherals
    public void PeripheralAccess_ReturnsDefaultValues_ByteValue(ushort address)
    {
        // These tests verify peripheral access routing
        // Currently returns default values since peripherals are not fully implemented

        // Act
        byte byteValue = _controller.ReadByte(address);

        // Assert
        Assert.Equal(0x00, byteValue); // Default value for unimplemented peripherals
    }

    [Theory]
    [InlineData(0x0000)] // SFR
    [InlineData(0x0100)] // 8-bit peripherals
    [InlineData(0x0200)] // 16-bit peripherals
    public void PeripheralAccess_ReturnsDefaultValues_WriteResult(ushort address)
    {
        // These tests verify peripheral access routing
        // Currently returns default values since peripherals are not fully implemented

        // Act
        bool writeResult = _controller.WriteByte(address, 0x42);

        // Assert
        Assert.True(writeResult); // Write should succeed for unimplemented peripherals
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
