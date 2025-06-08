using System;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.IntegrationTests.Memory;

/// <summary>
/// Basic integration tests for the memory system components working together.
/// These tests validate that memory regions, access validation, and memory controllers
/// interact correctly according to MSP430FR2355 specifications.
/// 
/// References:
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6.4: Memory Organization
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1.9.1: Memory Map
/// </summary>
public class MemorySystemBasicIntegrationTests
{
    [Fact]
    public void MemorySystem_DefaultLayout_CreatesCorrectNumberOfRegions()
    {
        // Arrange & Act
        var memoryMap = new MemoryMap();

        // Assert - MSP430FR2355 has 8 defined memory regions
        Assert.Equal(8, memoryMap.Regions.Count);
    }

    [Fact]
    public void MemorySystem_AccessValidation_WorksAcrossAllRegions()
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act & Assert - Test key addresses from each region according to MSP430FR2355 layout

        // Special Function Registers (0x0000-0x00FF) - Read/Write access
        Assert.True(validator.IsAccessValid(0x0080, MemoryAccessPermissions.Read));
        Assert.True(validator.IsAccessValid(0x0080, MemoryAccessPermissions.Write));
        Assert.False(validator.IsAccessValid(0x0080, MemoryAccessPermissions.Execute));

        // SRAM (0x2000-0x2FFF) - Full Read/Write/Execute access
        Assert.True(validator.IsAccessValid(0x2500, MemoryAccessPermissions.Read));
        Assert.True(validator.IsAccessValid(0x2500, MemoryAccessPermissions.Write));
        Assert.True(validator.IsAccessValid(0x2500, MemoryAccessPermissions.Execute));

        // FRAM (0x4000-0xBFFF) - Full Read/Write/Execute access
        Assert.True(validator.IsAccessValid(0x8000, MemoryAccessPermissions.Read));
        Assert.True(validator.IsAccessValid(0x8000, MemoryAccessPermissions.Write));
        Assert.True(validator.IsAccessValid(0x8000, MemoryAccessPermissions.Execute));
    }

    [Theory]
    [InlineData(0x0300)] // Between peripherals and bootstrap loader
    [InlineData(0x1A00)] // Between info memory and SRAM
    [InlineData(0x3500)] // Between SRAM and FRAM
    [InlineData(0xD000)] // Between FRAM and interrupt vectors
    public void MemorySystem_UnmappedAddresses_AreRejectedByValidator(ushort unmappedAddress)
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act & Assert - Unmapped addresses should be invalid for any access type
        Assert.False(validator.IsAccessValid(unmappedAddress, MemoryAccessPermissions.Read));
        Assert.False(validator.IsAccessValid(unmappedAddress, MemoryAccessPermissions.Write));
        Assert.False(validator.IsAccessValid(unmappedAddress, MemoryAccessPermissions.Execute));
    }

    [Fact]
    public void MemorySystem_RegionBoundaries_AreExactlyAsMSP430FR2355Specification()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act & Assert - Verify exact boundaries match MSP430FR2355 datasheet

        // Special Function Registers: 0x0000-0x00FF
        MemoryRegionInfo sfrRegion = memoryMap.GetRegionInfo(MemoryRegion.SpecialFunctionRegisters);
        Assert.Equal((ushort)0x0000, sfrRegion.StartAddress);
        Assert.Equal((ushort)0x00FF, sfrRegion.EndAddress);

        // SRAM: 0x2000-0x2FFF (4KB)
        MemoryRegionInfo ramRegion = memoryMap.GetRegionInfo(MemoryRegion.Ram);
        Assert.Equal((ushort)0x2000, ramRegion.StartAddress);
        Assert.Equal((ushort)0x2FFF, ramRegion.EndAddress);

        // FRAM: 0x4000-0xBFFF (32KB)
        MemoryRegionInfo framRegion = memoryMap.GetRegionInfo(MemoryRegion.Flash);
        Assert.Equal((ushort)0x4000, framRegion.StartAddress);
        Assert.Equal((ushort)0xBFFF, framRegion.EndAddress);

        // Interrupt Vector Table: 0xFFE0-0xFFFF
        MemoryRegionInfo ivtRegion = memoryMap.GetRegionInfo(MemoryRegion.InterruptVectorTable);
        Assert.Equal((ushort)0xFFE0, ivtRegion.StartAddress);
        Assert.Equal((ushort)0xFFFF, ivtRegion.EndAddress);
    }

    [Fact]
    public void MemorySystem_FramVsTraditionalFlash_AllowsByteWriteAccess()
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act & Assert - FRAM allows byte-level write access unlike traditional Flash
        // This is a key characteristic of MSP430FR2355 FRAM technology
        ushort framAddress = 0x8000; // Middle of FRAM region

        Assert.True(validator.IsAccessValid(framAddress, MemoryAccessPermissions.Write));

        MemoryRegionInfo framRegion = memoryMap.GetRegion(framAddress);
        Assert.Equal(MemoryAccessPermissions.All, framRegion.Permissions);
    }
}
