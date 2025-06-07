using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Memory;

/// <summary>
/// Integration tests for the memory system components working together.
/// </summary>
public class MemorySystemIntegrationTests
{
    [Fact]
    public void MemorySystem_Integration_SfrRegionBlocksExecution()
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var logger = new ConsoleLogger { MinimumLevel = LogLevel.Warning, IsOutputSuppressed = true };
        var validator = new MemoryAccessValidator(memoryMap, logger);

        // Act & Assert - Special Function Registers (0x0000-0x00FF) - Execute blocked
        validator.ValidateRead(0x0000);
        validator.ValidateWrite(0x0000);
        Assert.Throws<MemoryAccessException>(() => validator.ValidateExecute(0x0000));
    }

    [Fact]
    public void MemorySystem_Integration_InvalidAddressThrowsException()
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var logger = new ConsoleLogger { MinimumLevel = LogLevel.Warning, IsOutputSuppressed = true };
        var validator = new MemoryAccessValidator(memoryMap, logger);

        // Act & Assert - Invalid address - No access
        Assert.Throws<MemoryAccessException>(() => validator.ValidateRead(0x0300));
    }

    [Fact]
    public void MemorySystem_Integration_ValidRegionsAllowAccess()
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var logger = new ConsoleLogger { MinimumLevel = LogLevel.Warning, IsOutputSuppressed = true };
        var validator = new MemoryAccessValidator(memoryMap, logger);

        // Act & Assert - Valid regions allow access without throwing
        // SRAM (0x2000-0x2FFF) - All access allowed  
        validator.ValidateRead(0x2000);
        validator.ValidateWrite(0x2000);
        validator.ValidateExecute(0x2000);

        // FRAM (0x4000-0xBFFF) - All access allowed (FRAM allows write unlike Flash)
        validator.ValidateRead(0x4000);
        validator.ValidateWrite(0x4000);
        validator.ValidateExecute(0x4000);
    }

    [Theory]
    [InlineData(0x0000, MemoryAccessPermissions.Read, true)]   // SFR Start
    [InlineData(0x00FF, MemoryAccessPermissions.Read, true)]   // SFR End
    [InlineData(0x0000, MemoryAccessPermissions.Execute, false)] // SFR doesn't allow execute
    [InlineData(0x0100, MemoryAccessPermissions.Write, true)]  // Peripheral 8-bit Start
    [InlineData(0x01FF, MemoryAccessPermissions.Write, true)]  // Peripheral 8-bit End
    [InlineData(0x0300, MemoryAccessPermissions.Read, false)]  // Unmapped between peripherals and BSL
    [InlineData(0x1A00, MemoryAccessPermissions.Read, false)]  // Unmapped between info memory and SRAM
    public void MemorySystem_AddressBoundaries_AreEnforcedCorrectly(ushort address, MemoryAccessPermissions permission, bool expectedValid)
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act & Assert
        Assert.Equal(expectedValid, validator.IsAccessValid(address, permission));
    }

    [Fact]
    public void MemorySystem_DefaultRegions_HasCorrectRegionCount()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        IReadOnlyList<MemoryRegionInfo> regions = memoryMap.Regions;

        // Assert
        Assert.Equal(8, regions.Count);
    }

    [Theory]
    [InlineData(MemoryRegion.SpecialFunctionRegisters)]
    [InlineData(MemoryRegion.Peripherals8Bit)]
    [InlineData(MemoryRegion.Peripherals16Bit)]
    [InlineData(MemoryRegion.BootstrapLoader)]
    [InlineData(MemoryRegion.InformationMemory)]
    [InlineData(MemoryRegion.Ram)]
    [InlineData(MemoryRegion.Flash)]
    [InlineData(MemoryRegion.InterruptVectorTable)]
    public void MemorySystem_DefaultRegions_ContainsExpectedRegion(MemoryRegion expectedRegion)
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        IReadOnlyList<MemoryRegionInfo> regions = memoryMap.Regions;
        var regionTypes = regions.Select(r => r.Region).ToHashSet();

        // Assert
        Assert.Contains(expectedRegion, regionTypes);
    }

    [Fact]
    public void MemorySystem_DefaultRegions_AddressSpaceCoverageWithinLimits()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        IReadOnlyList<MemoryRegionInfo> regions = memoryMap.Regions;
        int totalMappedBytes = regions.Sum(r => r.Size);

        // Assert
        Assert.True(totalMappedBytes < 65536); // Should not exceed 16-bit address space
    }

    [Fact]
    public void MemorySystem_DefaultRegions_SignificantAddressSpaceCoverage()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        IReadOnlyList<MemoryRegionInfo> regions = memoryMap.Regions;
        int totalMappedBytes = regions.Sum(r => r.Size);

        // Assert
        Assert.True(totalMappedBytes > 30000); // Should cover significant portion (adjusted for FR2355)
    }

    [Theory]
    [InlineData(0x2000, true, MemoryAccessPermissions.All, MemoryRegion.Ram)]      // Valid SRAM
    [InlineData(0x4000, true, MemoryAccessPermissions.All, MemoryRegion.Flash)]    // Valid FRAM  
    [InlineData(0x0300, false, MemoryAccessPermissions.None, null)]                // Invalid address
    public void MemorySystem_ValidationInfo_ProvidesCorrectDetails(ushort address, bool expectedValid, MemoryAccessPermissions expectedPermissions, MemoryRegion? expectedRegion)
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act
        MemoryAccessValidationInfo info = validator.GetValidationInfo(address);

        // Assert
        Assert.Equal(expectedValid, info.IsValid);
        Assert.Equal(expectedPermissions, info.Permissions);
        if (expectedRegion.HasValue)
        {
            Assert.NotNull(info.Region);
            Assert.Equal(expectedRegion.Value, info.Region!.Value.Region);
        }
        else
        {
            Assert.Null(info.Region);
        }
    }

    [Theory]
    [InlineData(0x0000, "Special Function Registers")]
    [InlineData(0x0150, "8-bit Peripherals")]
    [InlineData(0x0250, "16-bit Peripherals")]
    [InlineData(0x1200, "Bootstrap Loader FRAM")]
    [InlineData(0x1850, "Information Memory FRAM")]
    [InlineData(0x2500, "SRAM")]
    [InlineData(0x6000, "FRAM")]
    [InlineData(0xFFF0, "Interrupt Vector Table")]
    public void MemorySystem_RegionDescriptions_AreCorrect(ushort address, string expectedDescription)
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        MemoryRegionInfo region = memoryMap.GetRegion(address);

        // Assert
        Assert.Contains(expectedDescription, region.Description);
    }
}
