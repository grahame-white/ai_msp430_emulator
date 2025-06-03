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
        var logger = new ConsoleLogger { MinimumLevel = LogLevel.Warning };
        var validator = new MemoryAccessValidator(memoryMap, logger);

        // Act & Assert - Test each memory region
        
        // Special Function Registers (0x0000-0x00FF) - Read/Write allowed
        validator.ValidateRead(0x0000);
        validator.ValidateWrite(0x0000);
        Assert.Throws<MemoryAccessException>(() => validator.ValidateExecute(0x0000));

        // RAM (0x3900-0x3AFF) - All access allowed
        validator.ValidateRead(0x3900);
        validator.ValidateWrite(0x3900);
        validator.ValidateExecute(0x3900);

        // Flash (0x3B00-0xFFDF) - Read/Execute only
        validator.ValidateRead(0x3B00);
        validator.ValidateExecute(0x3B00);
        Assert.Throws<MemoryAccessException>(() => validator.ValidateWrite(0x3B00));

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
        Assert.False(validator.IsAccessValid(0x1200, MemoryAccessPermissions.Read)); // Between info memory and RAM
    }

    [Fact]
    public void MemorySystem_DefaultRegions_CoverExpectedAddressSpace()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        var regions = memoryMap.Regions;

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

        // Verify address space coverage
        var totalMappedBytes = regions.Sum(r => r.Size);
        Assert.True(totalMappedBytes < 65536); // Should not exceed 16-bit address space
        Assert.True(totalMappedBytes > 50000); // Should cover significant portion
    }

    [Fact]
    public void MemorySystem_ValidationInfo_ProvidesComprehensiveDetails()
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act & Assert - Test validation info for different regions
        
        // Valid address in RAM
        var ramInfo = validator.GetValidationInfo(0x3900);
        Assert.True(ramInfo.IsValid);
        Assert.Equal(MemoryAccessPermissions.All, ramInfo.Permissions);
        Assert.NotNull(ramInfo.Region);
        Assert.Equal(MemoryRegion.Ram, ramInfo.Region.Value.Region);

        // Valid address in Flash
        var flashInfo = validator.GetValidationInfo(0x3B00);
        Assert.True(flashInfo.IsValid);
        Assert.Equal(MemoryAccessPermissions.ReadExecute, flashInfo.Permissions);
        Assert.Equal(MemoryRegion.Flash, flashInfo.Region!.Value.Region);

        // Invalid address
        var invalidInfo = validator.GetValidationInfo(0x0300);
        Assert.False(invalidInfo.IsValid);
        Assert.Equal(MemoryAccessPermissions.None, invalidInfo.Permissions);
        Assert.Null(invalidInfo.Region);
    }

    [Theory]
    [InlineData(0x0000, "Special Function Registers")]
    [InlineData(0x0150, "8-bit Peripherals")]
    [InlineData(0x0250, "16-bit Peripherals")]
    [InlineData(0x0500, "Bootstrap Loader Flash")]
    [InlineData(0x1050, "Information Memory Flash")]
    [InlineData(0x3950, "RAM")]
    [InlineData(0x4000, "Flash Memory")]
    [InlineData(0xFFF0, "Interrupt Vector Table")]
    public void MemorySystem_RegionDescriptions_AreCorrect(ushort address, string expectedDescription)
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        var region = memoryMap.GetRegion(address);

        // Assert
        Assert.Contains(expectedDescription, region.Description);
    }
}