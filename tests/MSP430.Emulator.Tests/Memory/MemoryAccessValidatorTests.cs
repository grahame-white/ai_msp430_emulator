using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Memory;

public class MemoryAccessValidatorTests
{
    private readonly IMemoryMap _memoryMap;
    private readonly TestLogger _logger;
    private readonly MemoryAccessValidator _validator;

    public MemoryAccessValidatorTests()
    {
        _memoryMap = new MemoryMap();
        _logger = new TestLogger();
        _validator = new MemoryAccessValidator(_memoryMap, _logger);
    }

    [Fact]
    public void Constructor_NullMemoryMap_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MemoryAccessValidator(null!));
    }

    [Fact]
    public void Constructor_WithoutLogger_CreatesValidator()
    {
        var validator = new MemoryAccessValidator(_memoryMap);
        Assert.NotNull(validator);
    }

    [Fact]
    public void ValidateRead_ValidAddress_DoesNotThrow()
    {
        // RAM allows read access
        _validator.ValidateRead(0x3900);
        
        // Should not throw
        Assert.True(true);
    }

    [Fact]
    public void ValidateRead_InvalidAddress_ThrowsMemoryAccessException()
    {
        var exception = Assert.Throws<MemoryAccessException>(() => _validator.ValidateRead(0x0300));
        
        Assert.Equal((ushort)0x0300, exception.Address);
        Assert.Equal(MemoryAccessPermissions.Read, exception.AccessType);
        Assert.Contains("Invalid memory address", exception.Message);
    }

    [Fact]
    public void ValidateWrite_ValidAddress_DoesNotThrow()
    {
        // RAM allows write access
        _validator.ValidateWrite(0x3900);
        
        // Should not throw
        Assert.True(true);
    }

    [Fact]
    public void ValidateWrite_ReadOnlyRegion_ThrowsMemoryAccessException()
    {
        var exception = Assert.Throws<MemoryAccessException>(() => _validator.ValidateWrite(0x3B00)); // Flash
        
        Assert.Equal((ushort)0x3B00, exception.Address);
        Assert.Equal(MemoryAccessPermissions.Write, exception.AccessType);
        Assert.Contains("Access denied", exception.Message);
    }

    [Fact]
    public void ValidateExecute_ValidAddress_DoesNotThrow()
    {
        // Flash allows execute access
        _validator.ValidateExecute(0x3B00);
        
        // Should not throw
        Assert.True(true);
    }

    [Fact]
    public void ValidateExecute_NonExecutableRegion_ThrowsMemoryAccessException()
    {
        // Create a custom memory map with a data-only region
        var customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.ReadWrite, "Data Only")
        };
        var customMap = new MemoryMap(customRegions);
        var customValidator = new MemoryAccessValidator(customMap, _logger);

        var exception = Assert.Throws<MemoryAccessException>(() => customValidator.ValidateExecute(0x0000));
        
        Assert.Equal((ushort)0x0000, exception.Address);
        Assert.Equal(MemoryAccessPermissions.Execute, exception.AccessType);
    }

    [Fact]
    public void ValidateAccess_WithAccessType_ValidatesCorrectly()
    {
        // Test valid access
        _validator.ValidateAccess(0x3900, MemoryAccessPermissions.Read);
        
        // Test invalid access
        Assert.Throws<MemoryAccessException>(() => 
            _validator.ValidateAccess(0x3B00, MemoryAccessPermissions.Write));
    }

    [Fact]
    public void IsAccessValid_ValidAccess_ReturnsTrue()
    {
        Assert.True(_validator.IsAccessValid(0x3900, MemoryAccessPermissions.Read));  // RAM read
        Assert.True(_validator.IsAccessValid(0x3900, MemoryAccessPermissions.Write)); // RAM write
        Assert.True(_validator.IsAccessValid(0x3B00, MemoryAccessPermissions.Read));  // Flash read
    }

    [Fact]
    public void IsAccessValid_InvalidAccess_ReturnsFalse()
    {
        Assert.False(_validator.IsAccessValid(0x0300, MemoryAccessPermissions.Read));  // Invalid address
        Assert.False(_validator.IsAccessValid(0x3B00, MemoryAccessPermissions.Write)); // Flash write (not allowed)
    }

    [Fact]
    public void GetValidationInfo_ValidAddress_ReturnsCorrectInfo()
    {
        var info = _validator.GetValidationInfo(0x3900); // RAM
        
        Assert.Equal((ushort)0x3900, info.Address);
        Assert.True(info.IsValid);
        Assert.Equal(MemoryAccessPermissions.All, info.Permissions);
        Assert.NotNull(info.Region);
        Assert.Equal(MemoryRegion.Ram, info.Region.Value.Region);
    }

    [Fact]
    public void GetValidationInfo_InvalidAddress_ReturnsInvalidInfo()
    {
        var info = _validator.GetValidationInfo(0x0300); // Unmapped
        
        Assert.Equal((ushort)0x0300, info.Address);
        Assert.False(info.IsValid);
        Assert.Equal(MemoryAccessPermissions.None, info.Permissions);
        Assert.Null(info.Region);
    }

    [Fact]
    public void ValidateRead_LogsWarningOnViolation()
    {
        Assert.Throws<MemoryAccessException>(() => _validator.ValidateRead(0x0300));
        
        Assert.Contains(_logger.LogEntries, entry => 
            entry.Level == LogLevel.Warning && 
            entry.Message.Contains("Memory access violation"));
    }

    [Fact]
    public void ValidateRead_LogsDebugOnSuccess()
    {
        _logger.MinimumLevel = LogLevel.Debug;
        
        _validator.ValidateRead(0x3900);
        
        Assert.Contains(_logger.LogEntries, entry => 
            entry.Level == LogLevel.Debug && 
            entry.Message.Contains("Memory read access validated"));
    }

    [Theory]
    [InlineData(0x0000, MemoryAccessPermissions.Read, true)]   // SFR read
    [InlineData(0x0000, MemoryAccessPermissions.Write, true)]  // SFR write
    [InlineData(0x3900, MemoryAccessPermissions.All, true)]    // RAM all access
    [InlineData(0x3B00, MemoryAccessPermissions.Read, true)]   // Flash read
    [InlineData(0x3B00, MemoryAccessPermissions.Write, false)] // Flash write (denied)
    [InlineData(0x0300, MemoryAccessPermissions.Read, false)]  // Invalid address
    public void IsAccessValid_VariousScenarios_ReturnsExpectedResult(ushort address, MemoryAccessPermissions accessType, bool expected)
    {
        var result = _validator.IsAccessValid(address, accessType);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidationMethods_LogCorrectAccessTypes()
    {
        _logger.MinimumLevel = LogLevel.Debug;

        _validator.ValidateRead(0x3900);
        _validator.ValidateWrite(0x3900);
        _validator.ValidateExecute(0x3900);

        Assert.Contains(_logger.LogEntries, entry => entry.Message.Contains("read access validated"));
        Assert.Contains(_logger.LogEntries, entry => entry.Message.Contains("write access validated"));
        Assert.Contains(_logger.LogEntries, entry => entry.Message.Contains("execute access validated"));
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