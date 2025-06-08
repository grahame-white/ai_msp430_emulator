using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Memory;

/// <summary>
/// Unit tests for the MemoryMap class.
/// 
/// Tests validate memory mapping functionality according to:
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) Section 6: Detailed Description
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) Section 2: Memory Organization
/// 
/// MSP430FR2355 Memory Map Layout:
/// - Special Function Registers (SFR): 0x0000-0x00FF (256 bytes)
/// - 8-bit Peripherals: 0x0100-0x01FF (256 bytes)
/// - 16-bit Peripherals: 0x0200-0x027F (128 bytes)
/// - RAM: 0x2000-0x3FFF (8KB)
/// - FRAM (Code/Data): 0x4000-0xBFFF (32KB)
/// - Information Memory: 0x1000-0x19FF (2.5KB)
/// - Boot Memory: 0x1A00-0x1BFF (512 bytes)
/// - Interrupt Vector Table: 0xFFE0-0xFFFF (32 bytes)
/// 
/// Memory regions have different access permissions:
/// - ReadWrite: RAM, FRAM (when unlocked)
/// - ReadExecute: FRAM (code), Boot Memory, Interrupt Vectors
/// - ReadWriteExecute: FRAM (full access when unlocked)
/// 
/// References:
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 2: Memory Organization
/// </summary>
public class MemoryMapTests
{
    [Fact]
    public void Constructor_DefaultRegions_CreatesValidMemoryMap()
    {
        var memoryMap = new MemoryMap();

        Assert.NotNull(memoryMap.Regions);
    }

    [Fact]
    public void Constructor_DefaultRegions_SetsCorrectRegionCount()
    {
        var memoryMap = new MemoryMap();

        Assert.Equal(8, memoryMap.Regions.Count); // Should have 8 default regions
    }

    [Fact]
    public void Constructor_CustomRegions_SetsCorrectRegionCount()
    {
        MemoryRegionInfo[] customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.All, "Test RAM"),
            new MemoryRegionInfo(MemoryRegion.Flash, 0x1000, 0x1FFF, MemoryAccessPermissions.ReadExecute, "Test Flash")
        };

        var memoryMap = new MemoryMap(customRegions);

        Assert.Equal(2, memoryMap.Regions.Count);
    }

    [Fact]
    public void Constructor_CustomRegions_ContainsRamRegion()
    {
        MemoryRegionInfo[] customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.All, "Test RAM"),
            new MemoryRegionInfo(MemoryRegion.Flash, 0x1000, 0x1FFF, MemoryAccessPermissions.ReadExecute, "Test Flash")
        };

        var memoryMap = new MemoryMap(customRegions);

        Assert.Contains(memoryMap.Regions, r => r.Region == MemoryRegion.Ram);
    }

    [Fact]
    public void Constructor_CustomRegions_ContainsFlashRegion()
    {
        MemoryRegionInfo[] customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.All, "Test RAM"),
            new MemoryRegionInfo(MemoryRegion.Flash, 0x1000, 0x1FFF, MemoryAccessPermissions.ReadExecute, "Test Flash")
        };

        var memoryMap = new MemoryMap(customRegions);

        Assert.Contains(memoryMap.Regions, r => r.Region == MemoryRegion.Flash);
    }

    [Fact]
    public void Constructor_NullCustomRegions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MemoryMap(null!));
    }

    [Fact]
    public void Constructor_OverlappingRegions_ThrowsArgumentException()
    {
        MemoryRegionInfo[] overlappingRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.All, "Region 1"),
            new MemoryRegionInfo(MemoryRegion.Flash, 0x00FF, 0x01FF, MemoryAccessPermissions.ReadExecute, "Region 2") // Overlaps at 0x00FF
        };

        Assert.Throws<ArgumentException>(() => new MemoryMap(overlappingRegions));
    }

    [Fact]
    public void GetRegion_SfrAddress_ReturnsCorrectRegion()
    {
        var memoryMap = new MemoryMap();

        // Test SFR region (0x0000-0x00FF)
        MemoryRegionInfo sfrRegion = memoryMap.GetRegion(0x0080);

        Assert.Equal(MemoryRegion.SpecialFunctionRegisters, sfrRegion.Region);
    }

    [Fact]
    public void GetRegion_SfrAddress_ReturnsCorrectStartAddress()
    {
        var memoryMap = new MemoryMap();

        // Test SFR region (0x0000-0x00FF)
        MemoryRegionInfo sfrRegion = memoryMap.GetRegion(0x0080);

        Assert.Equal((ushort)0x0000, sfrRegion.StartAddress);
    }

    [Fact]
    public void GetRegion_SfrAddress_ReturnsCorrectEndAddress()
    {
        var memoryMap = new MemoryMap();

        // Test SFR region (0x0000-0x00FF)
        MemoryRegionInfo sfrRegion = memoryMap.GetRegion(0x0080);

        Assert.Equal((ushort)0x00FF, sfrRegion.EndAddress);
    }

    [Fact]
    public void GetRegion_RamAddress_ReturnsCorrectRegion()
    {
        var memoryMap = new MemoryMap();

        // Test SRAM region (0x2000-0x2FFF)
        MemoryRegionInfo ramRegion = memoryMap.GetRegion(0x2500);

        Assert.Equal(MemoryRegion.Ram, ramRegion.Region);
    }

    [Fact]
    public void GetRegion_RamAddress_ReturnsCorrectStartAddress()
    {
        var memoryMap = new MemoryMap();

        // Test SRAM region (0x2000-0x2FFF)
        MemoryRegionInfo ramRegion = memoryMap.GetRegion(0x2500);

        Assert.Equal((ushort)0x2000, ramRegion.StartAddress);
    }

    [Fact]
    public void GetRegion_RamAddress_ReturnsCorrectEndAddress()
    {
        var memoryMap = new MemoryMap();

        // Test SRAM region (0x2000-0x2FFF)
        MemoryRegionInfo ramRegion = memoryMap.GetRegion(0x2500);

        Assert.Equal((ushort)0x2FFF, ramRegion.EndAddress);
    }

    [Fact]
    public void GetRegion_InvalidAddress_ThrowsArgumentException()
    {
        var memoryMap = new MemoryMap();

        // Test unmapped address space
        Assert.Throws<ArgumentException>(() => memoryMap.GetRegion(0x0300)); // Between peripherals and bootstrap loader
    }

    [Fact]
    public void GetRegionInfo_RamRegion_ReturnsCorrectRegion()
    {
        var memoryMap = new MemoryMap();

        MemoryRegionInfo ramInfo = memoryMap.GetRegionInfo(MemoryRegion.Ram);

        Assert.Equal(MemoryRegion.Ram, ramInfo.Region);
    }

    [Fact]
    public void GetRegionInfo_RamRegion_ReturnsCorrectStartAddress()
    {
        var memoryMap = new MemoryMap();

        MemoryRegionInfo ramInfo = memoryMap.GetRegionInfo(MemoryRegion.Ram);

        Assert.Equal((ushort)0x2000, ramInfo.StartAddress);
    }

    [Fact]
    public void GetRegionInfo_RamRegion_ReturnsCorrectEndAddress()
    {
        var memoryMap = new MemoryMap();

        MemoryRegionInfo ramInfo = memoryMap.GetRegionInfo(MemoryRegion.Ram);

        Assert.Equal((ushort)0x2FFF, ramInfo.EndAddress);
    }

    [Fact]
    public void GetRegionInfo_RamRegion_ReturnsCorrectPermissions()
    {
        var memoryMap = new MemoryMap();

        MemoryRegionInfo ramInfo = memoryMap.GetRegionInfo(MemoryRegion.Ram);

        Assert.Equal(MemoryAccessPermissions.All, ramInfo.Permissions);
    }

    [Fact]
    public void GetRegionInfo_UndefinedRegion_ThrowsArgumentException()
    {
        MemoryRegionInfo[] customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.All, "Test RAM")
        };
        var memoryMap = new MemoryMap(customRegions);

        Assert.Throws<ArgumentException>(() => memoryMap.GetRegionInfo(MemoryRegion.Flash));
    }

    [Theory]
    [InlineData(0x0000)] // SFR start
    [InlineData(0x00FF)] // SFR end
    [InlineData(0x2000)] // RAM start
    [InlineData(0x2FFF)] // SRAM end
    [InlineData(0xFFFF)] // Interrupt vector table end
    public void IsValidAddress_ValidAddresses_ReturnsTrue(ushort address)
    {
        var memoryMap = new MemoryMap();

        Assert.True(memoryMap.IsValidAddress(address));
    }

    [Theory]
    [InlineData(0x0300)] // Between peripherals and bootstrap loader
    [InlineData(0x1A00)] // Between info memory and SRAM
    public void IsValidAddress_InvalidAddresses_ReturnsFalse(ushort address)
    {
        var memoryMap = new MemoryMap();

        Assert.False(memoryMap.IsValidAddress(address));
    }

    [Theory]
    [InlineData(0x2000, MemoryAccessPermissions.Read)]   // RAM allows read
    [InlineData(0x2000, MemoryAccessPermissions.Write)]  // RAM allows write
    [InlineData(0x2000, MemoryAccessPermissions.Execute)] // RAM allows execute
    [InlineData(0x4000, MemoryAccessPermissions.Read)]   // Flash allows read
    [InlineData(0x4000, MemoryAccessPermissions.Execute)] // Flash allows execute
    public void IsAccessAllowed_ValidPermissions_ReturnsTrue(ushort address, MemoryAccessPermissions permission)
    {
        var memoryMap = new MemoryMap();

        Assert.True(memoryMap.IsAccessAllowed(address, permission));
    }

    [Fact]
    public void IsAccessAllowed_FramWriteAccess_ReturnsTrue()
    {
        var memoryMap = new MemoryMap();

        // FRAM allows write access
        Assert.True(memoryMap.IsAccessAllowed(0x4000, MemoryAccessPermissions.Write));
    }

    [Fact]
    public void IsAccessAllowed_InvalidAddress_ReturnsFalse()
    {
        var memoryMap = new MemoryMap();

        // Invalid/unmapped address
        Assert.False(memoryMap.IsAccessAllowed(0x0300, MemoryAccessPermissions.Read));
    }

    [Fact]
    public void GetPermissions_RamAddress_ReturnsAllPermissions()
    {
        var memoryMap = new MemoryMap();

        MemoryAccessPermissions ramPermissions = memoryMap.GetPermissions(0x2000);

        Assert.Equal(MemoryAccessPermissions.All, ramPermissions);
    }

    [Fact]
    public void GetPermissions_FramAddress_ReturnsAllPermissions()
    {
        var memoryMap = new MemoryMap();

        MemoryAccessPermissions framPermissions = memoryMap.GetPermissions(0x4000);

        Assert.Equal(MemoryAccessPermissions.All, framPermissions);
    }

    [Fact]
    public void GetPermissions_InvalidAddress_ThrowsArgumentException()
    {
        var memoryMap = new MemoryMap();

        Assert.Throws<ArgumentException>(() => memoryMap.GetPermissions(0x0300));
    }

    [Theory]
    [InlineData(MemoryRegion.SpecialFunctionRegisters, 0x0000)]
    [InlineData(MemoryRegion.Peripherals8Bit, 0x0100)]
    [InlineData(MemoryRegion.Peripherals16Bit, 0x0200)]
    [InlineData(MemoryRegion.BootstrapLoader, 0x1000)]
    [InlineData(MemoryRegion.InformationMemory, 0x1800)]
    [InlineData(MemoryRegion.Ram, 0x2000)]
    [InlineData(MemoryRegion.Flash, 0x4000)]
    [InlineData(MemoryRegion.InterruptVectorTable, 0xFFE0)]
    public void DefaultMemoryLayout_RegionBoundaries_StartAddressCorrect(MemoryRegion region, ushort expectedStart)
    {
        var memoryMap = new MemoryMap();

        MemoryRegionInfo regionInfo = memoryMap.GetRegionInfo(region);
        Assert.Equal(expectedStart, regionInfo.StartAddress);
    }

    [Theory]
    [InlineData(MemoryRegion.SpecialFunctionRegisters, 0x00FF)]
    [InlineData(MemoryRegion.Peripherals8Bit, 0x01FF)]
    [InlineData(MemoryRegion.Peripherals16Bit, 0x027F)]
    [InlineData(MemoryRegion.BootstrapLoader, 0x17FF)]
    [InlineData(MemoryRegion.InformationMemory, 0x19FF)]
    [InlineData(MemoryRegion.Ram, 0x2FFF)]
    [InlineData(MemoryRegion.Flash, 0xBFFF)]
    [InlineData(MemoryRegion.InterruptVectorTable, 0xFFFF)]
    public void DefaultMemoryLayout_RegionBoundaries_EndAddressCorrect(MemoryRegion region, ushort expectedEnd)
    {
        var memoryMap = new MemoryMap();

        MemoryRegionInfo regionInfo = memoryMap.GetRegionInfo(region);
        Assert.Equal(expectedEnd, regionInfo.EndAddress);
    }

    [Theory]
    [InlineData(MemoryRegion.SpecialFunctionRegisters, MemoryAccessPermissions.ReadWrite)]
    [InlineData(MemoryRegion.Peripherals8Bit, MemoryAccessPermissions.ReadWrite)]
    [InlineData(MemoryRegion.Peripherals16Bit, MemoryAccessPermissions.ReadWrite)]
    [InlineData(MemoryRegion.InformationMemory, MemoryAccessPermissions.ReadWrite)]
    [InlineData(MemoryRegion.BootstrapLoader, MemoryAccessPermissions.ReadExecute)]
    [InlineData(MemoryRegion.Ram, MemoryAccessPermissions.All)]
    [InlineData(MemoryRegion.Flash, MemoryAccessPermissions.All)]
    [InlineData(MemoryRegion.InterruptVectorTable, MemoryAccessPermissions.ReadExecute)]
    public void DefaultMemoryLayout_RegionPermissions_AreCorrect(MemoryRegion region, MemoryAccessPermissions expectedPermissions)
    {
        var memoryMap = new MemoryMap();

        Assert.Equal(expectedPermissions, memoryMap.GetRegionInfo(region).Permissions);
    }
}
