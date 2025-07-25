using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Memory;

/// <summary>
/// Unit tests for the MemoryAccessValidator class.
/// 
/// Memory access validation includes:
/// - Address range boundary checking
/// - Read/Write/Execute permission validation
/// - Memory alignment requirements (word vs byte access)
/// - Memory protection and access rights
/// - Invalid address space detection
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 2: Memory Organization
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: Detailed Description
/// </summary>
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

    [Theory]
    [InlineData(0x0300)]
    public void ValidateRead_InvalidAddress_ExceptionHasCorrectAddress(ushort invalidAddress)
    {
        try
        {
            _validator.ValidateRead(invalidAddress);
            throw new InvalidOperationException("Expected exception was not thrown");
        }
        catch (MemoryAccessException exception)
        {
            Assert.Equal(invalidAddress, exception.Address);
        }
    }

    [Theory]
    [InlineData(0x0300, MemoryAccessPermissions.Read)]
    public void ValidateRead_InvalidAddress_ExceptionHasCorrectAccessType(ushort invalidAddress, MemoryAccessPermissions expectedAccessType)
    {
        try
        {
            _validator.ValidateRead(invalidAddress);
            throw new InvalidOperationException("Expected exception was not thrown");
        }
        catch (MemoryAccessException exception)
        {
            Assert.Equal(expectedAccessType, exception.AccessType);
        }
    }

    [Theory]
    [InlineData(0x0300)]
    public void ValidateRead_InvalidAddress_ExceptionContainsCorrectMessage(ushort invalidAddress)
    {
        try
        {
            _validator.ValidateRead(invalidAddress);
            throw new InvalidOperationException("Expected exception was not thrown");
        }
        catch (MemoryAccessException exception)
        {
            Assert.Contains("Invalid memory address", exception.Message);
        }
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

    [Theory]
    [InlineData(0x1000)]  // Bootstrap Loader FRAM - read/execute only
    public void ValidateWrite_ReadOnlyRegion_ExceptionHasCorrectAddress(ushort readOnlyAddress)
    {
        try
        {
            _validator.ValidateWrite(readOnlyAddress);
            throw new InvalidOperationException("Expected exception was not thrown");
        }
        catch (MemoryAccessException exception)
        {
            Assert.Equal(readOnlyAddress, exception.Address);
        }
    }

    [Theory]
    [InlineData(0x1000, MemoryAccessPermissions.Write)]  // Bootstrap Loader FRAM - read/execute only
    public void ValidateWrite_ReadOnlyRegion_ExceptionHasCorrectAccessType(ushort readOnlyAddress, MemoryAccessPermissions expectedAccessType)
    {
        try
        {
            _validator.ValidateWrite(readOnlyAddress);
            throw new InvalidOperationException("Expected exception was not thrown");
        }
        catch (MemoryAccessException exception)
        {
            Assert.Equal(expectedAccessType, exception.AccessType);
        }
    }

    [Theory]
    [InlineData(0x1000)]  // Bootstrap Loader FRAM - read/execute only
    public void ValidateWrite_ReadOnlyRegion_ExceptionContainsCorrectMessage(ushort readOnlyAddress)
    {
        try
        {
            _validator.ValidateWrite(readOnlyAddress);
            throw new InvalidOperationException("Expected exception was not thrown");
        }
        catch (MemoryAccessException exception)
        {
            Assert.Contains("Access denied", exception.Message);
        }
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
    public void ValidateExecute_NonExecutableRegion_ExceptionHasCorrectAddress()
    {
        // Create a custom memory map with a data-only region
        MemoryRegionInfo[] customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.ReadWrite, "Data Only")
        };
        var customMap = new MemoryMap(customRegions);
        var customValidator = new MemoryAccessValidator(customMap, _logger);

        try
        {
            customValidator.ValidateExecute(0x0000);
            throw new InvalidOperationException("Expected exception was not thrown");
        }
        catch (MemoryAccessException exception)
        {
            Assert.Equal((ushort)0x0000, exception.Address);
        }
    }

    [Fact]
    public void ValidateExecute_NonExecutableRegion_ExceptionHasCorrectAccessType()
    {
        // Create a custom memory map with a data-only region
        MemoryRegionInfo[] customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.ReadWrite, "Data Only")
        };
        var customMap = new MemoryMap(customRegions);
        var customValidator = new MemoryAccessValidator(customMap, _logger);

        try
        {
            customValidator.ValidateExecute(0x0000);
            throw new InvalidOperationException("Expected exception was not thrown");
        }
        catch (MemoryAccessException exception)
        {
            Assert.Equal(MemoryAccessPermissions.Execute, exception.AccessType);
        }
    }

    [Fact]
    public void ValidateAccess_ValidAccessType_DoesNotThrow()
    {
        // RAM allows read access
        _validator.ValidateAccess(0x2000, MemoryAccessPermissions.Read);

        // Should not throw - test passes if no exception
        Assert.True(true);
    }

    [Fact]
    public void ValidateAccess_InvalidAccessType_ThrowsMemoryAccessException()
    {
        // Bootstrap Loader doesn't allow write
        Assert.Throws<MemoryAccessException>(() =>
            _validator.ValidateAccess(0x1000, MemoryAccessPermissions.Write));
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
    public void ValidateRead_InvalidAddress_LogsWarningOnViolation()
    {
        // Act - this will throw, but we're testing the logging behavior
        try
        {
            _validator.ValidateRead(0x0300);
        }
        catch (MemoryAccessException)
        {
            // Expected - we're testing the logging
        }

        // Assert
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

    [Fact]
    public void ValidateRead_ValidAddress_LogsDebugMessage()
    {
        _logger.MinimumLevel = LogLevel.Debug;

        _validator.ValidateRead(0x2000);

        Assert.Contains(_logger.LogEntries, entry => entry.Message.Contains("read access validated"));
    }

    [Fact]
    public void ValidateWrite_ValidAddress_LogsDebugMessage()
    {
        _logger.MinimumLevel = LogLevel.Debug;

        _validator.ValidateWrite(0x2000);

        Assert.Contains(_logger.LogEntries, entry => entry.Message.Contains("write access validated"));
    }

    [Fact]
    public void ValidateExecute_ValidAddress_LogsDebugMessage()
    {
        _logger.MinimumLevel = LogLevel.Debug;

        _validator.ValidateExecute(0x2000);

        Assert.Contains(_logger.LogEntries, entry => entry.Message.Contains("execute access validated"));
    }
}
