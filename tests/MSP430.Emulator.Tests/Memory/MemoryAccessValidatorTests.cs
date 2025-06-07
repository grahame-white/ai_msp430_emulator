using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        // SRAM allows read access
        _validator.ValidateRead(0x2000);

        // Should not throw
        Assert.True(true);
    }

    [Fact]
    public void ValidateRead_InvalidAddress_ThrowsMemoryAccessException()
    {
        Assert.Throws<MemoryAccessException>(() => _validator.ValidateRead(0x0300));
    }

    [Fact]
    public void ValidateRead_InvalidAddress_ExceptionContainsCorrectAddress()
    {
        MemoryAccessException exception = Assert.Throws<MemoryAccessException>(() => _validator.ValidateRead(0x0300));

        Assert.Equal((ushort)0x0300, exception.Address);
    }

    [Fact]
    public void ValidateRead_InvalidAddress_ExceptionContainsCorrectAccessType()
    {
        MemoryAccessException exception = Assert.Throws<MemoryAccessException>(() => _validator.ValidateRead(0x0300));

        Assert.Equal(MemoryAccessPermissions.Read, exception.AccessType);
    }

    [Fact]
    public void ValidateRead_InvalidAddress_ExceptionContainsExpectedMessage()
    {
        MemoryAccessException exception = Assert.Throws<MemoryAccessException>(() => _validator.ValidateRead(0x0300));

        Assert.Contains("Invalid memory address", exception.Message);
    }

    [Fact]
    public void ValidateWrite_ValidAddress_DoesNotThrow()
    {
        // RAM allows write access
        _validator.ValidateWrite(0x2000);

        // Should not throw
        Assert.True(true);
    }

    [Fact]
    public void ValidateWrite_ReadOnlyRegion_ThrowsMemoryAccessException()
    {
        Assert.Throws<MemoryAccessException>(() => _validator.ValidateWrite(0x1000)); // Bootstrap Loader FRAM - read/execute only
    }

    [Fact]
    public void ValidateWrite_ReadOnlyRegion_ExceptionContainsCorrectAddress()
    {
        MemoryAccessException exception = Assert.Throws<MemoryAccessException>(() => _validator.ValidateWrite(0x1000)); // Bootstrap Loader FRAM - read/execute only

        Assert.Equal((ushort)0x1000, exception.Address);
    }

    [Fact]
    public void ValidateWrite_ReadOnlyRegion_ExceptionContainsCorrectAccessType()
    {
        MemoryAccessException exception = Assert.Throws<MemoryAccessException>(() => _validator.ValidateWrite(0x1000)); // Bootstrap Loader FRAM - read/execute only

        Assert.Equal(MemoryAccessPermissions.Write, exception.AccessType);
    }

    [Fact]
    public void ValidateWrite_ReadOnlyRegion_ExceptionContainsExpectedMessage()
    {
        MemoryAccessException exception = Assert.Throws<MemoryAccessException>(() => _validator.ValidateWrite(0x1000)); // Bootstrap Loader FRAM - read/execute only

        Assert.Contains("Access denied", exception.Message);
    }

    [Fact]
    public void ValidateExecute_ValidAddress_DoesNotThrow()
    {
        // FRAM allows execute access
        _validator.ValidateExecute(0x4000);

        // Should not throw
        Assert.True(true);
    }

    [Fact]
    public void ValidateExecute_NonExecutableRegion_ThrowsMemoryAccessException()
    {
        // Create a custom memory map with a data-only region
        MemoryRegionInfo[] customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.ReadWrite, "Data Only")
        };
        var customMap = new MemoryMap(customRegions);
        var customValidator = new MemoryAccessValidator(customMap, _logger);

        Assert.Throws<MemoryAccessException>(() => customValidator.ValidateExecute(0x0000));
    }

    [Fact]
    public void ValidateExecute_NonExecutableRegion_ExceptionContainsCorrectAddress()
    {
        // Create a custom memory map with a data-only region
        MemoryRegionInfo[] customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.ReadWrite, "Data Only")
        };
        var customMap = new MemoryMap(customRegions);
        var customValidator = new MemoryAccessValidator(customMap, _logger);

        MemoryAccessException exception = Assert.Throws<MemoryAccessException>(() => customValidator.ValidateExecute(0x0000));

        Assert.Equal((ushort)0x0000, exception.Address);
    }

    [Fact]
    public void ValidateExecute_NonExecutableRegion_ExceptionContainsCorrectAccessType()
    {
        // Create a custom memory map with a data-only region
        MemoryRegionInfo[] customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.ReadWrite, "Data Only")
        };
        var customMap = new MemoryMap(customRegions);
        var customValidator = new MemoryAccessValidator(customMap, _logger);

        MemoryAccessException exception = Assert.Throws<MemoryAccessException>(() => customValidator.ValidateExecute(0x0000));

        Assert.Equal(MemoryAccessPermissions.Execute, exception.AccessType);
    }

    [Fact]
    public void ValidateAccess_WithAccessType_ValidatesCorrectly()
    {
        // Test valid access
        _validator.ValidateAccess(0x2000, MemoryAccessPermissions.Read);

        // Test invalid access
        Assert.Throws<MemoryAccessException>(() =>
            _validator.ValidateAccess(0x1000, MemoryAccessPermissions.Write)); // Bootstrap Loader doesn't allow write
    }

    [Fact]
    public void IsAccessValid_RamRead_ReturnsTrue()
    {
        Assert.True(_validator.IsAccessValid(0x2000, MemoryAccessPermissions.Read));  // RAM read
    }

    [Fact]
    public void IsAccessValid_RamWrite_ReturnsTrue()
    {
        Assert.True(_validator.IsAccessValid(0x2000, MemoryAccessPermissions.Write)); // RAM write
    }

    [Fact]
    public void IsAccessValid_FramRead_ReturnsTrue()
    {
        Assert.True(_validator.IsAccessValid(0x4000, MemoryAccessPermissions.Read));  // FRAM read
    }

    [Fact]
    public void IsAccessValid_InvalidAddress_ReturnsFalse()
    {
        Assert.False(_validator.IsAccessValid(0x0300, MemoryAccessPermissions.Read));  // Invalid address
    }

    [Fact]
    public void IsAccessValid_FramWrite_ReturnsTrue()
    {
        Assert.True(_validator.IsAccessValid(0x4000, MemoryAccessPermissions.Write)); // FRAM write (allowed)
    }

    [Fact]
    public void GetValidationInfo_ValidAddress_ReturnsCorrectAddress()
    {
        MemoryAccessValidationInfo info = _validator.GetValidationInfo(0x2000); // RAM

        Assert.Equal((ushort)0x2000, info.Address);
    }

    [Fact]
    public void GetValidationInfo_ValidAddress_ReturnsIsValidTrue()
    {
        MemoryAccessValidationInfo info = _validator.GetValidationInfo(0x2000); // RAM

        Assert.True(info.IsValid);
    }

    [Fact]
    public void GetValidationInfo_ValidAddress_ReturnsCorrectPermissions()
    {
        MemoryAccessValidationInfo info = _validator.GetValidationInfo(0x2000); // RAM

        Assert.Equal(MemoryAccessPermissions.All, info.Permissions);
    }

    [Fact]
    public void GetValidationInfo_ValidAddress_ReturnsNonNullRegion()
    {
        MemoryAccessValidationInfo info = _validator.GetValidationInfo(0x2000); // RAM

        Assert.NotNull(info.Region);
    }

    [Fact]
    public void GetValidationInfo_ValidAddress_ReturnsNotNullRegion()
    {
        MemoryAccessValidationInfo info = _validator.GetValidationInfo(0x2000); // RAM

        Assert.NotNull(info.Region);
    }

    [Fact]
    public void GetValidationInfo_ValidAddress_ReturnsCorrectRegionType()
    {
        MemoryAccessValidationInfo info = _validator.GetValidationInfo(0x2000); // RAM

        Assert.Equal(MemoryRegion.Ram, info.Region!.Value.Region);
    }

    [Fact]
    public void GetValidationInfo_InvalidAddress_ReturnsCorrectAddress()
    {
        MemoryAccessValidationInfo info = _validator.GetValidationInfo(0x0300); // Unmapped

        Assert.Equal((ushort)0x0300, info.Address);
    }

    [Fact]
    public void GetValidationInfo_InvalidAddress_ReturnsIsValidFalse()
    {
        MemoryAccessValidationInfo info = _validator.GetValidationInfo(0x0300); // Unmapped

        Assert.False(info.IsValid);
    }

    [Fact]
    public void GetValidationInfo_InvalidAddress_ReturnsNoPermissions()
    {
        MemoryAccessValidationInfo info = _validator.GetValidationInfo(0x0300); // Unmapped

        Assert.Equal(MemoryAccessPermissions.None, info.Permissions);
    }

    [Fact]
    public void GetValidationInfo_InvalidAddress_ReturnsNullRegion()
    {
        MemoryAccessValidationInfo info = _validator.GetValidationInfo(0x0300); // Unmapped

        Assert.Null(info.Region);
    }

    [Fact]
    public void ValidateRead_LogsWarningOnViolation_ThrowsException()
    {
        Assert.Throws<MemoryAccessException>(() => _validator.ValidateRead(0x0300));
    }

    [Fact]
    public void ValidateRead_LogsWarningOnViolation_LogsWarning()
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

        _validator.ValidateRead(0x2000);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug &&
            entry.Message.Contains("Memory read access validated"));
    }

    [Theory]
    [InlineData(0x0000, MemoryAccessPermissions.Read, true)]   // SFR read
    [InlineData(0x0000, MemoryAccessPermissions.Write, true)]  // SFR write
    [InlineData(0x2000, MemoryAccessPermissions.All, true)]    // RAM all access
    [InlineData(0x4000, MemoryAccessPermissions.Read, true)]   // FRAM read
    [InlineData(0x4000, MemoryAccessPermissions.Write, true)] // FRAM write (allowed)
    [InlineData(0x0300, MemoryAccessPermissions.Read, false)]  // Invalid address
    public void IsAccessValid_VariousScenarios_ReturnsExpectedResult(ushort address, MemoryAccessPermissions accessType, bool expected)
    {
        bool result = _validator.IsAccessValid(address, accessType);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("ValidateRead", "read access validated")]
    [InlineData("ValidateWrite", "write access validated")]
    [InlineData("ValidateExecute", "execute access validated")]
    public void ValidationMethod_LogsCorrectAccessType(string methodName, string expectedLogMessage)
    {
        _logger.MinimumLevel = LogLevel.Debug;

        // Use reflection or switch to call the appropriate method
        switch (methodName)
        {
            case "ValidateRead":
                _validator.ValidateRead(0x2000);
                break;
            case "ValidateWrite":
                _validator.ValidateWrite(0x2000);
                break;
            case "ValidateExecute":
                _validator.ValidateExecute(0x2000);
                break;
        }

        Assert.Contains(_logger.LogEntries, entry => entry.Message.Contains(expectedLogMessage));
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
