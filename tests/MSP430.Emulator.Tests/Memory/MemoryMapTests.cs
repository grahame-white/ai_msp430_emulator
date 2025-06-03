using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Memory;

public class MemoryMapTests
{
    [Fact]
    public void Constructor_DefaultRegions_CreatesValidMemoryMap()
    {
        var memoryMap = new MemoryMap();

        Assert.NotNull(memoryMap.Regions);
        Assert.Equal(8, memoryMap.Regions.Count); // Should have 8 default regions
    }

    [Fact]
    public void Constructor_CustomRegions_CreatesMemoryMapWithCustomRegions()
    {
        MemoryRegionInfo[] customRegions = new[]
        {
            new MemoryRegionInfo(MemoryRegion.Ram, 0x0000, 0x00FF, MemoryAccessPermissions.All, "Test RAM"),
            new MemoryRegionInfo(MemoryRegion.Flash, 0x1000, 0x1FFF, MemoryAccessPermissions.ReadExecute, "Test Flash")
        };

        var memoryMap = new MemoryMap(customRegions);

        Assert.Equal(2, memoryMap.Regions.Count);
        Assert.Contains(memoryMap.Regions, r => r.Region == MemoryRegion.Ram);
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
    public void GetRegion_ValidAddress_ReturnsCorrectRegion()
    {
        var memoryMap = new MemoryMap();

        // Test SFR region (0x0000-0x00FF)
        MemoryRegionInfo sfrRegion = memoryMap.GetRegion(0x0080);
        Assert.Equal(MemoryRegion.SpecialFunctionRegisters, sfrRegion.Region);
        Assert.Equal((ushort)0x0000, sfrRegion.StartAddress);
        Assert.Equal((ushort)0x00FF, sfrRegion.EndAddress);

        // Test SRAM region (0x2000-0x2FFF)
        MemoryRegionInfo ramRegion = memoryMap.GetRegion(0x2500);
        Assert.Equal(MemoryRegion.Ram, ramRegion.Region);
        Assert.Equal((ushort)0x2000, ramRegion.StartAddress);
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
    public void GetRegionInfo_ValidRegion_ReturnsCorrectInfo()
    {
        var memoryMap = new MemoryMap();

        MemoryRegionInfo ramInfo = memoryMap.GetRegionInfo(MemoryRegion.Ram);
        Assert.Equal(MemoryRegion.Ram, ramInfo.Region);
        Assert.Equal((ushort)0x2000, ramInfo.StartAddress);
        Assert.Equal((ushort)0x2FFF, ramInfo.EndAddress);
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

    [Fact]
    public void IsValidAddress_ValidAddresses_ReturnsTrue()
    {
        var memoryMap = new MemoryMap();

        Assert.True(memoryMap.IsValidAddress(0x0000)); // SFR start
        Assert.True(memoryMap.IsValidAddress(0x00FF)); // SFR end
        Assert.True(memoryMap.IsValidAddress(0x2000)); // RAM start
        Assert.True(memoryMap.IsValidAddress(0x2FFF)); // SRAM end
        Assert.True(memoryMap.IsValidAddress(0xFFFF)); // Interrupt vector table end
    }

    [Fact]
    public void IsValidAddress_InvalidAddresses_ReturnsFalse()
    {
        var memoryMap = new MemoryMap();

        Assert.False(memoryMap.IsValidAddress(0x0300)); // Between peripherals and bootstrap loader
        Assert.False(memoryMap.IsValidAddress(0x1A00)); // Between info memory and SRAM
    }

    [Fact]
    public void IsAccessAllowed_ValidPermissions_ReturnsTrue()
    {
        var memoryMap = new MemoryMap();

        // RAM allows all access
        Assert.True(memoryMap.IsAccessAllowed(0x2000, MemoryAccessPermissions.Read));
        Assert.True(memoryMap.IsAccessAllowed(0x2000, MemoryAccessPermissions.Write));
        Assert.True(memoryMap.IsAccessAllowed(0x2000, MemoryAccessPermissions.Execute));

        // Flash allows read and execute
        Assert.True(memoryMap.IsAccessAllowed(0x4000, MemoryAccessPermissions.Read));
        Assert.True(memoryMap.IsAccessAllowed(0x4000, MemoryAccessPermissions.Execute));
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
    public void GetPermissions_ValidAddress_ReturnsCorrectPermissions()
    {
        var memoryMap = new MemoryMap();

        MemoryAccessPermissions ramPermissions = memoryMap.GetPermissions(0x2000);
        Assert.Equal(MemoryAccessPermissions.All, ramPermissions);

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
    [InlineData(MemoryRegion.SpecialFunctionRegisters, 0x0000, 0x00FF)]
    [InlineData(MemoryRegion.Peripherals8Bit, 0x0100, 0x01FF)]
    [InlineData(MemoryRegion.Peripherals16Bit, 0x0200, 0x027F)]
    [InlineData(MemoryRegion.BootstrapLoader, 0x1000, 0x17FF)]
    [InlineData(MemoryRegion.InformationMemory, 0x1800, 0x19FF)]
    [InlineData(MemoryRegion.Ram, 0x2000, 0x2FFF)]
    [InlineData(MemoryRegion.Flash, 0x4000, 0xBFFF)]
    [InlineData(MemoryRegion.InterruptVectorTable, 0xFFE0, 0xFFFF)]
    public void DefaultMemoryLayout_RegionBoundaries_AreCorrect(MemoryRegion region, ushort expectedStart, ushort expectedEnd)
    {
        var memoryMap = new MemoryMap();

        MemoryRegionInfo regionInfo = memoryMap.GetRegionInfo(region);
        Assert.Equal(expectedStart, regionInfo.StartAddress);
        Assert.Equal(expectedEnd, regionInfo.EndAddress);
    }

    [Fact]
    public void DefaultMemoryLayout_RegionPermissions_AreCorrect()
    {
        var memoryMap = new MemoryMap();

        // Registers should be read/write
        Assert.Equal(MemoryAccessPermissions.ReadWrite, memoryMap.GetRegionInfo(MemoryRegion.SpecialFunctionRegisters).Permissions);
        Assert.Equal(MemoryAccessPermissions.ReadWrite, memoryMap.GetRegionInfo(MemoryRegion.Peripherals8Bit).Permissions);
        Assert.Equal(MemoryAccessPermissions.ReadWrite, memoryMap.GetRegionInfo(MemoryRegion.Peripherals16Bit).Permissions);
        Assert.Equal(MemoryAccessPermissions.ReadWrite, memoryMap.GetRegionInfo(MemoryRegion.InformationMemory).Permissions);

        // Bootstrap loader should be read/execute only
        Assert.Equal(MemoryAccessPermissions.ReadExecute, memoryMap.GetRegionInfo(MemoryRegion.BootstrapLoader).Permissions);

        // RAM should allow all access
        Assert.Equal(MemoryAccessPermissions.All, memoryMap.GetRegionInfo(MemoryRegion.Ram).Permissions);

        // FRAM should allow all access (unlike traditional Flash)
        Assert.Equal(MemoryAccessPermissions.All, memoryMap.GetRegionInfo(MemoryRegion.Flash).Permissions);
        Assert.Equal(MemoryAccessPermissions.ReadExecute, memoryMap.GetRegionInfo(MemoryRegion.InterruptVectorTable).Permissions);
    }
}
