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
    public void MemorySystem_Integration_ValidatesAddressSpace()
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var logger = new ConsoleLogger { MinimumLevel = LogLevel.Warning, RedirectErrorsToStdout = true };
        var validator = new MemoryAccessValidator(memoryMap, logger);

        // Act & Assert - Test each memory region

        // Special Function Registers (0x0000-0x00FF) - Read/Write allowed
        validator.ValidateRead(0x0000);
        validator.ValidateWrite(0x0000);
        Assert.Throws<MemoryAccessException>(() => validator.ValidateExecute(0x0000));

        // SRAM (0x2000-0x2FFF) - All access allowed  
        validator.ValidateRead(0x2000);
        validator.ValidateWrite(0x2000);
        validator.ValidateExecute(0x2000);

        // FRAM (0x4000-0xBFFF) - All access allowed (FRAM allows write unlike Flash)
        validator.ValidateRead(0x4000);
        validator.ValidateWrite(0x4000);
        validator.ValidateExecute(0x4000);

        // Invalid address - No access
        Assert.Throws<MemoryAccessException>(() => validator.ValidateRead(0x0300));
    }

    [Fact]
    public void MemorySystem_AddressBoundaries_AreEnforcedCorrectly()
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act & Assert - Test boundary conditions

        // SFR region boundaries
        Assert.True(validator.IsAccessValid(0x0000, MemoryAccessPermissions.Read)); // Start
        Assert.True(validator.IsAccessValid(0x00FF, MemoryAccessPermissions.Read)); // End
        Assert.False(validator.IsAccessValid(0x0000, MemoryAccessPermissions.Execute)); // SFR doesn't allow execute

        // Peripheral 8-bit boundaries
        Assert.True(validator.IsAccessValid(0x0100, MemoryAccessPermissions.Write)); // Start
        Assert.True(validator.IsAccessValid(0x01FF, MemoryAccessPermissions.Write)); // End

        // Unmapped regions
        Assert.False(validator.IsAccessValid(0x0300, MemoryAccessPermissions.Read)); // Between peripherals and BSL
        Assert.False(validator.IsAccessValid(0x1A00, MemoryAccessPermissions.Read)); // Between info memory and SRAM
    }

    [Fact]
    public void MemorySystem_DefaultRegions_CoverExpectedAddressSpace()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        IReadOnlyList<MemoryRegionInfo> regions = memoryMap.Regions;

        // Assert
        Assert.Equal(8, regions.Count); // Expected number of regions

        // Verify we have all expected regions
        var regionTypes = regions.Select(r => r.Region).ToHashSet();
        Assert.Contains(MemoryRegion.SpecialFunctionRegisters, regionTypes);
        Assert.Contains(MemoryRegion.Peripherals8Bit, regionTypes);
        Assert.Contains(MemoryRegion.Peripherals16Bit, regionTypes);
        Assert.Contains(MemoryRegion.BootstrapLoader, regionTypes);
        Assert.Contains(MemoryRegion.InformationMemory, regionTypes);
        Assert.Contains(MemoryRegion.Ram, regionTypes);
        Assert.Contains(MemoryRegion.Flash, regionTypes);
        Assert.Contains(MemoryRegion.InterruptVectorTable, regionTypes);

        // Verify address space coverage - MSP430FR2355 specific
        int totalMappedBytes = regions.Sum(r => r.Size);
        Assert.True(totalMappedBytes < 65536); // Should not exceed 16-bit address space
        Assert.True(totalMappedBytes > 30000); // Should cover significant portion (adjusted for FR2355)
    }

    [Fact]
    public void MemorySystem_ValidationInfo_ProvidesComprehensiveDetails()
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act & Assert - Test validation info for different regions

        // Valid address in SRAM
        MemoryAccessValidationInfo ramInfo = validator.GetValidationInfo(0x2000);
        Assert.True(ramInfo.IsValid);
        Assert.Equal(MemoryAccessPermissions.All, ramInfo.Permissions);
        Assert.NotNull(ramInfo.Region);
        Assert.Equal(MemoryRegion.Ram, ramInfo.Region.Value.Region);

        // Valid address in FRAM
        MemoryAccessValidationInfo framInfo = validator.GetValidationInfo(0x4000);
        Assert.True(framInfo.IsValid);
        Assert.Equal(MemoryAccessPermissions.All, framInfo.Permissions);
        Assert.Equal(MemoryRegion.Flash, framInfo.Region!.Value.Region);

        // Invalid address
        MemoryAccessValidationInfo invalidInfo = validator.GetValidationInfo(0x0300);
        Assert.False(invalidInfo.IsValid);
        Assert.Equal(MemoryAccessPermissions.None, invalidInfo.Permissions);
        Assert.Null(invalidInfo.Region);
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
