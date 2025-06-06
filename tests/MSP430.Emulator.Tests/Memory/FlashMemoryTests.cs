using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Memory;

public class FlashMemoryTests
{
    private readonly TestLogger _logger;

    public FlashMemoryTests()
    {
        _logger = new TestLogger();
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesFlashMemory()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.Equal(4096, flash.Size);
        Assert.Equal((ushort)0x8000, flash.BaseAddress);
        Assert.Equal((ushort)0x8FFF, flash.EndAddress);
        Assert.Equal(512, flash.SectorSize);
        Assert.Equal(FlashProtectionLevel.None, flash.ProtectionLevel);
        Assert.Equal(FlashControllerState.Locked, flash.ControllerState);
    }

    [Fact]
    public void Constructor_NullLogger_CreatesFlashMemory()
    {
        var flash = new FlashMemory(0x8000, 4096);

        Assert.Equal(4096, flash.Size);
        Assert.Equal((ushort)0x8000, flash.BaseAddress);
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

    [Fact]
    public void Constructor_InvalidSectorSize_ThrowsArgumentException()
    {
        // Size not divisible by sector size
        Assert.Throws<ArgumentException>(() => new FlashMemory(0x8000, 4096, 500, _logger));

        // Zero sector size
        Assert.Throws<ArgumentException>(() => new FlashMemory(0x8000, 4096, 0, _logger));

        // Negative sector size
        Assert.Throws<ArgumentException>(() => new FlashMemory(0x8000, 4096, -1, _logger));
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

        Assert.Throws<ArgumentOutOfRangeException>(() => flash.ReadByte(invalidAddress));
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

        Assert.Throws<ArgumentException>(() => flash.ReadWord(unalignedAddress));
    }

    [Fact]
    public void ProgramByte_WhenLocked_ReturnsFalse()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        bool result = flash.ProgramByte(0x8000, 0xAB);

        Assert.False(result);
        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8000));
    }

    [Fact]
    public void ProgramByte_WhenUnlocked_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        bool result = flash.ProgramByte(0x8000, 0xAB);

        Assert.True(result);
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
        Assert.Equal(0xAB, flash.ReadByte(0x8000));
    }

    [Fact]
    public void ProgramWord_WhenUnlocked_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        bool result = flash.ProgramWord(0x8000, 0x1234);

        Assert.True(result);
        Assert.Equal((ushort)0x1234, flash.ReadWord(0x8000));
    }

    [Fact]
    public void EraseSector_WhenUnlocked_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);
        flash.Unlock(0xA555);

        // Program some data first
        flash.ProgramByte(0x8000, 0xAB);
        flash.ProgramByte(0x8001, 0xCD);

        bool result = flash.EraseSector(0x8000);

        Assert.True(result);
        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8000));
        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8001));
        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x81FF)); // End of sector
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
        flash.ProgramByte(0x8500, 0xCD); // Different sector

        bool result = flash.MassErase();

        Assert.True(result);
        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8000));
        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8500));
        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8FFF)); // End of flash
    }

    [Fact]
    public void Unlock_ValidKey_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        bool result = flash.Unlock(0xA555);

        Assert.True(result);
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
    public void SetProtectionLevel_WriteProtected_BlocksOperations()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        bool protectionResult = flash.SetProtectionLevel(FlashProtectionLevel.WriteProtected);
        Assert.True(protectionResult);

        flash.Unlock(0xA555);
        bool programResult = flash.ProgramByte(0x8000, 0xAB);

        Assert.False(programResult);
        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8000));
    }

    [Fact]
    public void GetSectorNumber_ValidAddress_ReturnsCorrectSector()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.Equal(0, flash.GetSectorNumber(0x8000)); // First sector
        Assert.Equal(0, flash.GetSectorNumber(0x81FF)); // Still first sector
        Assert.Equal(1, flash.GetSectorNumber(0x8200)); // Second sector
        Assert.Equal(7, flash.GetSectorNumber(0x8FFF)); // Last sector
    }

    [Fact]
    public void GetSectorBaseAddress_ValidAddress_ReturnsCorrectBase()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.Equal((ushort)0x8000, flash.GetSectorBaseAddress(0x8000));
        Assert.Equal((ushort)0x8000, flash.GetSectorBaseAddress(0x81FF));
        Assert.Equal((ushort)0x8200, flash.GetSectorBaseAddress(0x8200));
        Assert.Equal((ushort)0x8E00, flash.GetSectorBaseAddress(0x8FFF));
    }

    [Fact]
    public void IsAddressInBounds_ValidAddresses_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.True(flash.IsAddressInBounds(0x8000));
        Assert.True(flash.IsAddressInBounds(0x8FFF));
        Assert.True(flash.IsAddressInBounds(0x8500));
    }

    [Fact]
    public void IsAddressInBounds_InvalidAddresses_ReturnsFalse()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.False(flash.IsAddressInBounds(0x7FFF));
        Assert.False(flash.IsAddressInBounds(0x9000));
    }

    [Fact]
    public void IsRangeInBounds_ValidRanges_ReturnsTrue()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.True(flash.IsRangeInBounds(0x8000, 100));
        Assert.True(flash.IsRangeInBounds(0x8000, 4096));
        Assert.True(flash.IsRangeInBounds(0x8FFF, 1));
    }

    [Fact]
    public void IsRangeInBounds_InvalidRanges_ReturnsFalse()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        Assert.False(flash.IsRangeInBounds(0x7FFF, 100)); // Starts outside
        Assert.False(flash.IsRangeInBounds(0x8000, 4097)); // Extends beyond
        Assert.False(flash.IsRangeInBounds(0x8000, 0)); // Zero length
        Assert.False(flash.IsRangeInBounds(0x8000, -1)); // Negative length
    }

    [Fact]
    public void GetReadCycles_ReturnsDefaultCycles()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        uint cycles = flash.GetReadCycles(0x8000);

        Assert.Equal(FlashMemory.DefaultReadCycles, cycles);
    }

    [Fact]
    public void GetProgramCycles_ReturnsCorrectCycles()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        uint byteCycles = flash.GetProgramCycles(0x8000, false);
        uint wordCycles = flash.GetProgramCycles(0x8000, true);

        Assert.Equal(FlashController.ByteProgramCycles, byteCycles);
        Assert.Equal(FlashController.WordProgramCycles, wordCycles);
    }

    [Fact]
    public void GetEraseCycles_ReturnsCorrectCycles()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        uint sectorCycles = flash.GetEraseCycles(FlashOperation.SectorErase);
        uint massCycles = flash.GetEraseCycles(FlashOperation.MassErase);

        Assert.Equal(FlashController.SectorEraseCycles, sectorCycles);
        Assert.Equal(FlashController.MassEraseCycles, massCycles);
    }

    [Fact]
    public void Clear_SetsAllBytesToErasedPattern()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        flash.Clear();

        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8000));
        Assert.Equal(FlashMemory.ErasedPattern, flash.ReadByte(0x8FFF));
    }

    [Fact]
    public void Initialize_SetsAllBytesToPattern()
    {
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        flash.Initialize(0xAA);

        Assert.Equal(0xAA, flash.ReadByte(0x8000));
        Assert.Equal(0xAA, flash.ReadByte(0x8FFF));
    }

    [Fact]
    public void Operations_LogCorrectMessages()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var flash = new FlashMemory(0x8000, 4096, 512, _logger);

        flash.ReadByte(0x8000);
        flash.Unlock(0xA555);
        flash.ProgramByte(0x8000, 0xAB);
        flash.EraseSector(0x8000);

        // Check that debug messages were logged
        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("FlashMemory initialized"));
        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("Flash ReadByte"));
        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("Flash controller unlocked"));
        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("Flash ProgramByte"));
        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("Flash EraseSector"));
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
