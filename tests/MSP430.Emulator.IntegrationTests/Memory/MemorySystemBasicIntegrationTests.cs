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

    [Theory]
    [InlineData(0x0080, MemoryAccessPermissions.Read, true)]
    [InlineData(0x0080, MemoryAccessPermissions.Write, true)]
    [InlineData(0x0080, MemoryAccessPermissions.Execute, false)]
    public void MemorySystem_SfrRegion_HasCorrectAccessPermissions(ushort address, MemoryAccessPermissions permission, bool expectedValid)
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act
        bool result = validator.IsAccessValid(address, permission);

        // Assert
        Assert.Equal(expectedValid, result);
    }

    [Theory]
    [InlineData(0x2500, MemoryAccessPermissions.Read, true)]
    [InlineData(0x2500, MemoryAccessPermissions.Write, true)]
    [InlineData(0x2500, MemoryAccessPermissions.Execute, true)]
    public void MemorySystem_SramRegion_HasFullAccessPermissions(ushort address, MemoryAccessPermissions permission, bool expectedValid)
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act
        bool result = validator.IsAccessValid(address, permission);

        // Assert
        Assert.Equal(expectedValid, result);
    }

    [Theory]
    [InlineData(0x8000, MemoryAccessPermissions.Read, true)]
    [InlineData(0x8000, MemoryAccessPermissions.Write, true)]
    [InlineData(0x8000, MemoryAccessPermissions.Execute, true)]
    public void MemorySystem_FramRegion_HasFullAccessPermissions(ushort address, MemoryAccessPermissions permission, bool expectedValid)
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act
        bool result = validator.IsAccessValid(address, permission);

        // Assert
        Assert.Equal(expectedValid, result);
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
    public void MemorySystem_SfrRegionBoundaries_MatchMSP430FR2355Specification()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        MemoryRegionInfo sfrRegion = memoryMap.GetRegionInfo(MemoryRegion.SpecialFunctionRegisters);

        // Assert - Special Function Registers: 0x0000-0x00FF
        Assert.Equal((ushort)0x0000, sfrRegion.StartAddress);
    }

    [Fact]
    public void MemorySystem_SfrRegionEndAddress_MatchesMSP430FR2355Specification()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        MemoryRegionInfo sfrRegion = memoryMap.GetRegionInfo(MemoryRegion.SpecialFunctionRegisters);

        // Assert - Special Function Registers: 0x0000-0x00FF
        Assert.Equal((ushort)0x00FF, sfrRegion.EndAddress);
    }

    [Fact]
    public void MemorySystem_RamRegionStartAddress_MatchesMSP430FR2355Specification()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        MemoryRegionInfo ramRegion = memoryMap.GetRegionInfo(MemoryRegion.Ram);

        // Assert - SRAM: 0x2000-0x2FFF (4KB)
        Assert.Equal((ushort)0x2000, ramRegion.StartAddress);
    }

    [Fact]
    public void MemorySystem_RamRegionEndAddress_MatchesMSP430FR2355Specification()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        MemoryRegionInfo ramRegion = memoryMap.GetRegionInfo(MemoryRegion.Ram);

        // Assert - SRAM: 0x2000-0x2FFF (4KB)
        Assert.Equal((ushort)0x2FFF, ramRegion.EndAddress);
    }

    [Fact]
    public void MemorySystem_FramRegionStartAddress_MatchesMSP430FR2355Specification()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        MemoryRegionInfo framRegion = memoryMap.GetRegionInfo(MemoryRegion.Flash);

        // Assert - FRAM: 0x4000-0xBFFF (32KB)
        Assert.Equal((ushort)0x4000, framRegion.StartAddress);
    }

    [Fact]
    public void MemorySystem_FramRegionEndAddress_MatchesMSP430FR2355Specification()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        MemoryRegionInfo framRegion = memoryMap.GetRegionInfo(MemoryRegion.Flash);

        // Assert - FRAM: 0x4000-0xBFFF (32KB)
        Assert.Equal((ushort)0xBFFF, framRegion.EndAddress);
    }

    [Fact]
    public void MemorySystem_InterruptVectorTableStartAddress_MatchesMSP430FR2355Specification()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        MemoryRegionInfo ivtRegion = memoryMap.GetRegionInfo(MemoryRegion.InterruptVectorTable);

        // Assert - Interrupt Vector Table: 0xFFE0-0xFFFF
        Assert.Equal((ushort)0xFFE0, ivtRegion.StartAddress);
    }

    [Fact]
    public void MemorySystem_InterruptVectorTableEndAddress_MatchesMSP430FR2355Specification()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        MemoryRegionInfo ivtRegion = memoryMap.GetRegionInfo(MemoryRegion.InterruptVectorTable);

        // Assert - Interrupt Vector Table: 0xFFE0-0xFFFF
        Assert.Equal((ushort)0xFFFF, ivtRegion.EndAddress);
    }

    [Fact]
    public void MemorySystem_FramVsTraditionalFlash_AllowsByteWriteAccess()
    {
        // Arrange
        var memoryMap = new MemoryMap();
        var validator = new MemoryAccessValidator(memoryMap);

        // Act
        ushort framAddress = 0x8000; // Middle of FRAM region
        bool result = validator.IsAccessValid(framAddress, MemoryAccessPermissions.Write);

        // Assert - FRAM allows byte-level write access unlike traditional Flash
        // This is a key characteristic of MSP430FR2355 FRAM technology
        Assert.True(result);
    }

    [Fact]
    public void MemorySystem_FramRegion_SupportsAllAccessTypes()
    {
        // Arrange
        var memoryMap = new MemoryMap();

        // Act
        ushort framAddress = 0x8000; // Middle of FRAM region
        MemoryRegionInfo framRegion = memoryMap.GetRegion(framAddress);

        // Assert - FRAM region allows all access types
        Assert.Equal(MemoryAccessPermissions.All, framRegion.Permissions);
    }
}
