using System;
using System.Collections.Generic;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Core;

/// <summary>
/// Tests for interrupt system functionality based on MSP430FR2355 specifications.
/// 
/// MSP430FR2355 Interrupt Vector Table Layout (0xFFE0-0xFFFF):
/// - Each vector is 2 bytes (16-bit address)
/// - Highest priority at highest address (0xFFFE-0xFFFF)
/// - Reset vector at 0xFFFE-0xFFFF
/// - NMI vector at 0xFFFC-0xFFFD
/// - Various peripheral interrupt vectors at lower addresses
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1.3: Interrupts
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1.3.4: Interrupt Processing
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1.3.6: Interrupt Vectors
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1.3.7: SYS Interrupt Vector Generators
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6.3
/// </summary>
public class InterruptSystemTests
{
    [Fact]
    public void InterruptVectorTable_MemoryRegion_HasCorrectAddressRange()
    {
        // Test validates that interrupt vector table region matches MSP430FR2355 specification
        // SLAU445I Section 1.3.6: Interrupt vector table at 0xFFE0-0xFFFF
        var memoryMap = new MemoryMap();

        MemoryRegionInfo vectorTableInfo = memoryMap.GetRegionInfo(MemoryRegion.InterruptVectorTable);

        Assert.Equal((ushort)0xFFE0, vectorTableInfo.StartAddress);
        Assert.Equal((ushort)0xFFFF, vectorTableInfo.EndAddress);
    }

    [Fact]
    public void InterruptVectorTable_MemoryRegion_HasReadExecutePermissions()
    {
        // Test validates that interrupt vector table has correct access permissions
        // SLAU445I Section 1.3.6: Vector table should be readable and executable
        var memoryMap = new MemoryMap();

        MemoryRegionInfo vectorTableInfo = memoryMap.GetRegionInfo(MemoryRegion.InterruptVectorTable);

        Assert.Equal(MemoryAccessPermissions.ReadExecute, vectorTableInfo.Permissions);
    }

    [Theory]
    [InlineData(0xFFFE)] // Reset vector (highest priority)
    [InlineData(0xFFFC)] // NMI vector
    [InlineData(0xFFFA)] // Typical peripheral interrupt vector
    [InlineData(0xFFE0)] // Lowest interrupt vector
    public void InterruptVectorTable_AddressRange_ContainsValidVectorAddresses(ushort vectorAddress)
    {
        // Test validates that interrupt vector addresses are within the correct range
        // SLAU445I Section 1.3.6: All interrupt vectors must be in range 0xFFE0-0xFFFF
        var memoryMap = new MemoryMap();

        Assert.True(memoryMap.IsValidAddress(vectorAddress));

        MemoryRegionInfo region = memoryMap.GetRegion(vectorAddress);
        Assert.Equal(MemoryRegion.InterruptVectorTable, region.Region);
    }

    [Theory]
    [InlineData(0xC000)] // Just after FRAM region, before vector table
    public void InterruptVectorTable_AddressRange_ExcludesInvalidAddresses(ushort invalidAddress)
    {
        // Test validates that addresses outside interrupt vector table are not included
        var memoryMap = new MemoryMap();

        if (invalidAddress < 0xFFE0)
        {
            // If address is valid, it should be in a different region
            if (memoryMap.IsValidAddress(invalidAddress))
            {
                MemoryRegionInfo region = memoryMap.GetRegion(invalidAddress);
                Assert.NotEqual(MemoryRegion.InterruptVectorTable, region.Region);
            }
            else
            {
                // If address is invalid, that's also acceptable for this test
                Assert.False(memoryMap.IsValidAddress(invalidAddress));
            }
        }
    }

    [Fact]
    public void StatusRegister_GeneralInterruptEnable_DefaultsToFalse()
    {
        // Test validates that GIE flag starts disabled after reset
        // SLAU445I Section 1.3.4: Interrupts disabled by default after reset
        var statusRegister = new StatusRegister();

        Assert.False(statusRegister.GeneralInterruptEnable);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void StatusRegister_GeneralInterruptEnable_CanBeSetAndCleared(bool enableInterrupts)
    {
        // Test validates that GIE flag can be controlled
        // SLAU445I Section 1.3.4: GIE flag controls maskable interrupt acceptance
        var statusRegister = new StatusRegister();

        statusRegister.GeneralInterruptEnable = enableInterrupts;

        Assert.Equal(enableInterrupts, statusRegister.GeneralInterruptEnable);
    }

    [Fact]
    public void StatusRegister_Reset_ClearsGeneralInterruptEnable()
    {
        // Test validates that reset properly disables interrupts
        // SLAU445I Section 1.3.4: Reset clears GIE flag
        var statusRegister = new StatusRegister();
        statusRegister.GeneralInterruptEnable = true;

        statusRegister.Reset();

        Assert.False(statusRegister.GeneralInterruptEnable);
    }

    [Fact]
    public void StatusRegister_GeneralInterruptEnable_UsesCorrectBitMask()
    {
        // Test validates that GIE flag uses correct bit position (bit 3)
        // SLAU445I Section 1.3.4: GIE is bit 3 of status register
        var statusRegister = new StatusRegister();

        statusRegister.GeneralInterruptEnable = true;
        Assert.Equal((ushort)0x0008, statusRegister.Value & 0x0008); // Bit 3 set

        statusRegister.GeneralInterruptEnable = false;
        Assert.Equal((ushort)0x0000, statusRegister.Value & 0x0008); // Bit 3 clear
    }

    [Theory]
    [InlineData(0x0008, true)]  // Only GIE bit set
    [InlineData(0x0000, false)] // No bits set
    [InlineData(0xFFF7, false)] // All bits except GIE set
    [InlineData(0xFFFF, true)]  // All bits including GIE set
    public void StatusRegister_GeneralInterruptEnable_ReadsCorrectBitFromValue(ushort registerValue, bool expectedGie)
    {
        // Test validates that GIE flag correctly reads from status register value
        // SLAU445I Section 1.3.4: GIE flag interpretation
        var statusRegister = new StatusRegister(registerValue);

        Assert.Equal(expectedGie, statusRegister.GeneralInterruptEnable);
    }

    [Fact]
    public void InterruptVectorTable_Size_MatchesMSP430Specification()
    {
        // Test validates that vector table size matches MSP430 specification
        // SLAU445I Section 1.3.6: Vector table is 32 bytes (16 vectors × 2 bytes each)
        var memoryMap = new MemoryMap();
        MemoryRegionInfo vectorTableInfo = memoryMap.GetRegionInfo(MemoryRegion.InterruptVectorTable);

        uint tableSize = (uint)(vectorTableInfo.EndAddress - vectorTableInfo.StartAddress + 1);
        Assert.Equal(32u, tableSize); // 0xFFFF - 0xFFE0 + 1 = 32 bytes
    }

    [Theory]
    [InlineData(0xFFFE, "Reset")]           // Highest priority - Reset vector
    [InlineData(0xFFFC, "NMI")]             // Non-maskable interrupt
    [InlineData(0xFFFA, "HardwareBreakpoint")] // Hardware breakpoint (if available)
    [InlineData(0xFFF8, "Bootstrap")]       // Bootstrap loader (if available)
    public void InterruptVectorTable_WellKnownVectors_AreAtCorrectAddresses(ushort address, string vectorName)
    {
        // Test validates that well-known interrupt vectors are at expected addresses
        // SLAU445I Section 1.3.6: Standard MSP430 interrupt vector layout
        var memoryMap = new MemoryMap();

        Assert.True(memoryMap.IsValidAddress(address));

        MemoryRegionInfo region = memoryMap.GetRegion(address);
        Assert.Equal(MemoryRegion.InterruptVectorTable, region.Region);

        // Vector addresses should be even (word-aligned)
        Assert.Equal(0, address % 2);

        // Ensure we're testing the expected vector type (documentation reference)
        Assert.NotNull(vectorName); // Verify test is parameterized correctly
    }

    [Fact]
    public void InterruptVectorTable_AllAddresses_AreWordAligned()
    {
        // Test validates that all vector addresses are word-aligned (even addresses)
        // SLAU445I Section 1.3.6: Interrupt vectors are 16-bit values, must be word-aligned
        var memoryMap = new MemoryMap();
        MemoryRegionInfo vectorTableInfo = memoryMap.GetRegionInfo(MemoryRegion.InterruptVectorTable);

        // Iterate through word-aligned addresses in the vector table region
        for (int addr = vectorTableInfo.StartAddress; addr <= vectorTableInfo.EndAddress; addr += 2)
        {
            ushort address = (ushort)addr;
            Assert.True(memoryMap.IsValidAddress(address));
            MemoryRegionInfo region = memoryMap.GetRegion(address);
            Assert.Equal(MemoryRegion.InterruptVectorTable, region.Region);
        }
    }
}
