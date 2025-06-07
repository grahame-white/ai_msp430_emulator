using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Memory;

public class RandomAccessMemoryTests
{
    private readonly TestLogger _logger;

    public RandomAccessMemoryTests()
    {
        _logger = new TestLogger();
    }

    [Fact]
    public void Constructor_ValidParameters_SetsSize()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        Assert.Equal(1024, memory.Size);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsBaseAddress()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        Assert.Equal((ushort)0x2000, memory.BaseAddress);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsEndAddress()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        Assert.Equal((ushort)0x23FF, memory.EndAddress);
    }

    [Fact]
    public void Constructor_NullLogger_SetsSize()
    {
        var memory = new RandomAccessMemory(0x2000, 1024);

        Assert.Equal(1024, memory.Size);
    }

    [Fact]
    public void Constructor_NullLogger_SetsBaseAddress()
    {
        var memory = new RandomAccessMemory(0x2000, 1024);

        Assert.Equal((ushort)0x2000, memory.BaseAddress);
    }

    [Fact]
    public void Constructor_NullLogger_SetsEndAddress()
    {
        var memory = new RandomAccessMemory(0x2000, 1024);

        Assert.Equal((ushort)0x23FF, memory.EndAddress);
    }

    [Theory]
    [InlineData(511)]    // Below minimum
    [InlineData(10241)]  // Above maximum
    [InlineData(0)]      // Zero size
    [InlineData(-1)]     // Negative size
    public void Constructor_InvalidSize_ThrowsArgumentOutOfRangeException(int invalidSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RandomAccessMemory(0x2000, invalidSize, _logger));
    }

    [Fact]
    public void Constructor_MemoryOverflow_ThrowsArgumentException()
    {
        // Base address 0xFFF0 + 1024 bytes would overflow 16-bit address space
        Assert.Throws<ArgumentException>(() => new RandomAccessMemory(0xFFF0, 1024, _logger));
    }

    [Theory]
    [InlineData(512)]    // Minimum size
    [InlineData(1024)]   // Common size
    [InlineData(4096)]   // 4KB
    [InlineData(10240)]  // Maximum size
    public void Constructor_ValidSizes_CreatesMemory(int validSize)
    {
        var memory = new RandomAccessMemory(0x2000, validSize, _logger);
        Assert.Equal(validSize, memory.Size);
    }

    [Fact]
    public void ReadByte_ValidAddress_ReturnsValue()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        byte value = memory.ReadByte(0x2000);

        Assert.Equal(0x00, value); // Initial value should be 0
    }

    [Fact]
    public void WriteByte_ValidAddress_StoresValue()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        memory.WriteByte(0x2000, 0xAB);
        byte value = memory.ReadByte(0x2000);

        Assert.Equal(0xAB, value);
    }

    [Theory]
    [InlineData(0x1FFF)] // One below base
    [InlineData(0x2400)] // One above end (assuming 1024 byte memory)
    public void ReadByte_InvalidAddress_ThrowsArgumentOutOfRangeException(ushort invalidAddress)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        Assert.Throws<ArgumentOutOfRangeException>(() => memory.ReadByte(invalidAddress));
    }

    [Theory]
    [InlineData(0x1FFF)] // One below base
    [InlineData(0x2400)] // One above end (assuming 1024 byte memory)
    public void WriteByte_InvalidAddress_ThrowsArgumentOutOfRangeException(ushort invalidAddress)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        Assert.Throws<ArgumentOutOfRangeException>(() => memory.WriteByte(invalidAddress, 0xFF));
    }

    [Fact]
    public void ReadWord_ValidEvenAddress_ReturnsLittleEndianValue()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        // Write bytes in little-endian order: low byte at 0x2000, high byte at 0x2001
        memory.WriteByte(0x2000, 0x34); // Low byte
        memory.WriteByte(0x2001, 0x12); // High byte

        ushort value = memory.ReadWord(0x2000);

        Assert.Equal((ushort)0x1234, value);
    }

    [Fact]
    public void WriteWord_ValidEvenAddress_StoresLowByte()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        memory.WriteWord(0x2000, 0x5678);

        Assert.Equal(0x78, memory.ReadByte(0x2000)); // Low byte
    }

    [Fact]
    public void WriteWord_ValidEvenAddress_StoresHighByte()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        memory.WriteWord(0x2000, 0x5678);

        Assert.Equal(0x56, memory.ReadByte(0x2001)); // High byte
    }

    [Theory]
    [InlineData(0x2001)] // Odd address
    [InlineData(0x2003)] // Another odd address
    public void ReadWord_OddAddress_ThrowsArgumentException(ushort oddAddress)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        Assert.Throws<ArgumentException>(() => memory.ReadWord(oddAddress));
    }

    [Theory]
    [InlineData(0x2001)] // Odd address
    [InlineData(0x2003)] // Another odd address
    public void WriteWord_OddAddress_ThrowsArgumentException(ushort oddAddress)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        Assert.Throws<ArgumentException>(() => memory.WriteWord(oddAddress, 0x1234));
    }

    [Fact]
    public void ReadWord_LastValidEvenAddressOutOfBounds_ThrowsArgumentOutOfRangeException()
    {
        var memory = new RandomAccessMemory(0x2000, 1023, _logger); // End address is 0x23FE (odd size)

        // Last even address is 0x23FE, but reading word would need 0x23FF which is out of bounds
        Assert.Throws<ArgumentOutOfRangeException>(() => memory.ReadWord(0x23FE));
    }

    [Fact]
    public void WriteWord_LastValidEvenAddressOutOfBounds_ThrowsArgumentOutOfRangeException()
    {
        var memory = new RandomAccessMemory(0x2000, 1023, _logger); // End address is 0x23FE (odd size)

        // Last even address is 0x23FE, but writing word would need 0x23FF which is out of bounds
        Assert.Throws<ArgumentOutOfRangeException>(() => memory.WriteWord(0x23FE, 0x1234));
    }

    [Theory]
    [InlineData(0x2000)]
    [InlineData(0x2100)]
    [InlineData(0x23FF)]
    public void Clear_AllMemory_SetsAllBytesToZero(ushort address)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        // Write some non-zero values
        memory.WriteByte(0x2000, 0xFF);
        memory.WriteByte(0x2100, 0xAB);
        memory.WriteByte(0x23FF, 0x55);

        memory.Clear();

        Assert.Equal(0x00, memory.ReadByte(address));
    }

    [Theory]
    [InlineData(0x00, 0x2000)]
    [InlineData(0x00, 0x2100)]
    [InlineData(0x00, 0x23FF)]
    [InlineData(0xFF, 0x2000)]
    [InlineData(0xFF, 0x2100)]
    [InlineData(0xFF, 0x23FF)]
    [InlineData(0xAA, 0x2000)]
    [InlineData(0xAA, 0x2100)]
    [InlineData(0xAA, 0x23FF)]
    [InlineData(0x55, 0x2000)]
    [InlineData(0x55, 0x2100)]
    [InlineData(0x55, 0x23FF)]
    public void Initialize_WithPattern_SetsAllBytesToPattern(byte pattern, ushort address)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        memory.Initialize(pattern);

        Assert.Equal(pattern, memory.ReadByte(address));
    }

    [Theory]
    [InlineData(0x2000)]
    [InlineData(0x2100)]
    [InlineData(0x23FF)]  // Last valid address (baseAddress + size - 1)
    public void Initialize_NoPattern_SetsAllBytesToZero(ushort address)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        // Write some non-zero values first
        memory.WriteByte(0x2000, 0xFF);
        memory.WriteByte(0x2100, 0xAB);

        memory.Initialize(); // Default pattern is 0x00

        Assert.Equal(0x00, memory.ReadByte(address));
    }

    [Theory]
    [InlineData(0x2000, false, 1u)] // Byte read
    [InlineData(0x2000, true, 1u)]  // Word read
    [InlineData(0x2100, false, 1u)] // Different address, byte read
    [InlineData(0x2100, true, 1u)]  // Different address, word read
    public void GetReadCycles_AllAddresses_ReturnsConstantCycles(ushort address, bool isWordAccess, uint expectedCycles)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        uint cycles = memory.GetReadCycles(address, isWordAccess);

        Assert.Equal(expectedCycles, cycles);
    }

    [Theory]
    [InlineData(0x2000, false, 1u)] // Byte write
    [InlineData(0x2000, true, 1u)]  // Word write
    [InlineData(0x2100, false, 1u)] // Different address, byte write
    [InlineData(0x2100, true, 1u)]  // Different address, word write
    public void GetWriteCycles_AllAddresses_ReturnsConstantCycles(ushort address, bool isWordAccess, uint expectedCycles)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        uint cycles = memory.GetWriteCycles(address, isWordAccess);

        Assert.Equal(expectedCycles, cycles);
    }

    [Theory]
    [InlineData(0x2000, true)]  // Base address
    [InlineData(0x23FF, true)]  // End address (for 1024 bytes)
    [InlineData(0x2100, true)]  // Middle address
    [InlineData(0x1FFF, false)] // One below base
    [InlineData(0x2400, false)] // One above end
    public void IsAddressInBounds_VariousAddresses_ReturnsExpectedResult(ushort address, bool expected)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        bool result = memory.IsAddressInBounds(address);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0x2000, 1, true)]    // Single byte at start
    [InlineData(0x23FF, 1, true)]    // Single byte at end
    [InlineData(0x2000, 1024, true)] // Entire memory
    [InlineData(0x2000, 1025, false)] // Too large
    [InlineData(0x1FFF, 1, false)]   // Start before memory
    [InlineData(0x2000, 0, false)]   // Zero length
    [InlineData(0x2000, -1, false)]  // Negative length
    public void IsRangeInBounds_VariousRanges_ReturnsExpectedResult(ushort startAddress, int length, bool expected)
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        bool result = memory.IsRangeInBounds(startAddress, length);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsRangeInBounds_RangeOverflowsAddressSpace_ReturnsFalse()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        // Range that would overflow ushort.MaxValue
        bool result = memory.IsRangeInBounds(0xFFFF, 2);

        Assert.False(result);
    }

    [Fact]
    public void EndAddress_CalculatedCorrectly()
    {
        var memory = new RandomAccessMemory(0x3000, 2048, _logger);

        Assert.Equal((ushort)0x37FF, memory.EndAddress); // 0x3000 + 2048 - 1
    }

    [Fact]
    public void Operations_LogInitialization()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("RandomAccessMemory initialized"));
    }

    [Fact]
    public void Operations_LogWriteByte()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        memory.WriteByte(0x2000, 0xAB);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("RAM WriteByte"));
    }

    [Fact]
    public void Operations_LogReadByte()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        memory.ReadByte(0x2000);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("RAM ReadByte"));
    }

    [Fact]
    public void Operations_LogWriteWord()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        memory.WriteWord(0x2002, 0x1234);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("RAM WriteWord"));
    }

    [Fact]
    public void Operations_LogReadWord()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        memory.ReadWord(0x2002);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("RAM ReadWord"));
    }

    [Fact]
    public void Operations_LogClear()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        memory.Clear();

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("RAM cleared"));
    }

    [Fact]
    public void Operations_LogInitialize()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        memory.Initialize(0xFF);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug && entry.Message.Contains("RAM initialized"));
    }

    [Fact]
    public void BoundaryTesting_LastValidByteAddress_WorksCorrectly()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger); // End = 0x23FF

        // Test last valid byte address
        memory.WriteByte(0x23FF, 0xAB);

        Assert.Equal(0xAB, memory.ReadByte(0x23FF));
    }

    [Fact]
    public void BoundaryTesting_LastValidWordAddress_WorksCorrectly()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger); // End = 0x23FF

        // Test last valid word address (must be even and have space for high byte)
        memory.WriteWord(0x23FE, 0x1234);

        Assert.Equal((ushort)0x1234, memory.ReadWord(0x23FE));
    }

    [Fact]
    public void MemoryContents_PersistAcrossOperations_ByteAt2000()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        // Write pattern to multiple locations
        memory.WriteByte(0x2000, 0x11);
        memory.WriteByte(0x2001, 0x22);
        memory.WriteWord(0x2002, 0x4433);

        Assert.Equal(0x11, memory.ReadByte(0x2000));
    }

    [Fact]
    public void MemoryContents_PersistAcrossOperations_ByteAt2001()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        // Write pattern to multiple locations
        memory.WriteByte(0x2000, 0x11);
        memory.WriteByte(0x2001, 0x22);
        memory.WriteWord(0x2002, 0x4433);

        Assert.Equal(0x22, memory.ReadByte(0x2001));
    }

    [Fact]
    public void MemoryContents_PersistAcrossOperations_WordAt2002()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        // Write pattern to multiple locations
        memory.WriteByte(0x2000, 0x11);
        memory.WriteByte(0x2001, 0x22);
        memory.WriteWord(0x2002, 0x4433);

        Assert.Equal((ushort)0x4433, memory.ReadWord(0x2002));
    }

    [Fact]
    public void MemoryContents_PersistAcrossOperations_WordLowByte()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        // Write pattern to multiple locations
        memory.WriteByte(0x2000, 0x11);
        memory.WriteByte(0x2001, 0x22);
        memory.WriteWord(0x2002, 0x4433);

        // Verify individual bytes of the word
        Assert.Equal(0x33, memory.ReadByte(0x2002)); // Low byte
    }

    [Fact]
    public void MemoryContents_PersistAcrossOperations_WordHighByte()
    {
        var memory = new RandomAccessMemory(0x2000, 1024, _logger);

        // Write pattern to multiple locations
        memory.WriteByte(0x2000, 0x11);
        memory.WriteByte(0x2001, 0x22);
        memory.WriteWord(0x2002, 0x4433);

        // Verify individual bytes of the word
        Assert.Equal(0x44, memory.ReadByte(0x2003)); // High byte
    }

    private class TestLogger : ILogger
    {
        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;
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
