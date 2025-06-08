using System;
using System.Collections.Generic;
using System.Linq;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Memory;

/// <summary>
/// Tests for peripheral memory region access and behavior validation based on MSP430FR2355 specifications.
/// 
/// MSP430FR2355 Peripheral Address Space:
/// - 8-bit Peripherals: 0x0100-0x01FF (256 bytes)
/// - 16-bit Peripherals: 0x0200-0x027F (128 bytes)
/// - Special Function Registers: 0x0000-0x00FF (256 bytes)
/// 
/// References:
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: Peripheral Modules
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6.1: Timer_A
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6.2: Timer_B
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6.5: ADC
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6.6: eUSCI_A/B
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6.7: DMA Controller
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I)
/// </summary>
public class PeripheralMemoryRegionTests
{
    /// <summary>
    /// Tests for 8-bit peripheral memory region according to MSP430FR2355 specifications.
    /// </summary>
    public class EightBitPeripheralTests
    {
        [Fact]
        public void MemoryMap_EightBitPeripherals_CorrectAddressRange()
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act
            MemoryRegionInfo regionInfo = memoryMap.GetRegionInfo(MemoryRegion.Peripherals8Bit);

            // Assert - 8-bit peripherals should be at 0x0100-0x01FF per MSP430FR2355
            Assert.Equal(0x0100, regionInfo.StartAddress);
            Assert.Equal(0x01FF, regionInfo.EndAddress);
            Assert.Equal(MemoryRegion.Peripherals8Bit, regionInfo.Region);
            Assert.Equal("8-bit Peripherals", regionInfo.Description);
        }

        [Theory]
        [InlineData(0x0100)]  // Start of 8-bit peripheral range
        [InlineData(0x0150)]  // Middle of 8-bit peripheral range
        [InlineData(0x01FF)]  // End of 8-bit peripheral range
        public void MemoryMap_EightBitPeripheralAddresses_ValidAccess(ushort address)
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act
            MemoryRegionInfo region = memoryMap.GetRegion(address);
            bool isValid = memoryMap.IsValidAddress(address);

            // Assert
            Assert.True(isValid);
            Assert.Equal(MemoryRegion.Peripherals8Bit, region.Region);
        }

        [Theory]
        [InlineData(0x00FF)]  // Just before 8-bit peripheral range
        [InlineData(0x0200)]  // Just after 8-bit peripheral range (16-bit range)
        public void MemoryMap_NonEightBitPeripheralAddresses_DifferentRegion(ushort address)
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act
            MemoryRegionInfo region = memoryMap.GetRegion(address);

            // Assert - Should not be in 8-bit peripheral region
            Assert.NotEqual(MemoryRegion.Peripherals8Bit, region.Region);
        }

        [Fact]
        public void MemoryMap_EightBitPeripherals_ReadWritePermissions()
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act
            MemoryAccessPermissions permissions = memoryMap.GetPermissions(0x0150); // Middle of 8-bit peripheral range

            // Assert - 8-bit peripherals should allow read/write access
            Assert.True(memoryMap.IsAccessAllowed(0x0150, MemoryAccessPermissions.Read));
            Assert.True(memoryMap.IsAccessAllowed(0x0150, MemoryAccessPermissions.Write));
            Assert.Equal(MemoryAccessPermissions.ReadWrite, permissions);
        }

        [Theory]
        [InlineData(0x0100, 256)]  // Total 8-bit peripheral region size
        public void MemoryMap_EightBitPeripherals_SizeCalculation(ushort startAddress, int expectedSize)
        {
            // Arrange
            var memoryMap = new MemoryMap();
            MemoryRegionInfo regionInfo = memoryMap.GetRegionInfo(MemoryRegion.Peripherals8Bit);

            // Act
            int actualSize = regionInfo.EndAddress - regionInfo.StartAddress + 1;

            // Assert
            Assert.Equal(expectedSize, actualSize);
            Assert.Equal(startAddress, regionInfo.StartAddress);
        }
    }

    /// <summary>
    /// Tests for 16-bit peripheral memory region according to MSP430FR2355 specifications.
    /// </summary>
    public class SixteenBitPeripheralTests
    {
        [Fact]
        public void MemoryMap_SixteenBitPeripherals_CorrectAddressRange()
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act
            MemoryRegionInfo regionInfo = memoryMap.GetRegionInfo(MemoryRegion.Peripherals16Bit);

            // Assert - 16-bit peripherals should be at 0x0200-0x027F per MSP430FR2355
            Assert.Equal(0x0200, regionInfo.StartAddress);
            Assert.Equal(0x027F, regionInfo.EndAddress);
            Assert.Equal(MemoryRegion.Peripherals16Bit, regionInfo.Region);
            Assert.Equal("16-bit Peripherals", regionInfo.Description);
        }

        [Theory]
        [InlineData(0x0200)]  // Start of 16-bit peripheral range
        [InlineData(0x0240)]  // Middle of 16-bit peripheral range
        [InlineData(0x027F)]  // End of 16-bit peripheral range
        public void MemoryMap_SixteenBitPeripheralAddresses_ValidAccess(ushort address)
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act
            MemoryRegionInfo region = memoryMap.GetRegion(address);
            bool isValid = memoryMap.IsValidAddress(address);

            // Assert
            Assert.True(isValid);
            Assert.Equal(MemoryRegion.Peripherals16Bit, region.Region);
        }

        [Theory]
        [InlineData(0x01FF)]  // Just before 16-bit peripheral range
        [InlineData(0x0280)]  // Just after 16-bit peripheral range
        public void MemoryMap_NonSixteenBitPeripheralAddresses_DifferentRegion(ushort address)
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act & Assert
            if (memoryMap.IsValidAddress(address))
            {
                MemoryRegionInfo region = memoryMap.GetRegion(address);
                Assert.NotEqual(MemoryRegion.Peripherals16Bit, region.Region);
            }
            else
            {
                // Address 0x0280 and beyond are unmapped in default MSP430FR2355 layout
                Assert.Throws<ArgumentException>(() => memoryMap.GetRegion(address));
            }
        }

        [Fact]
        public void MemoryMap_SixteenBitPeripherals_ReadWritePermissions()
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act
            MemoryAccessPermissions permissions = memoryMap.GetPermissions(0x0240); // Middle of 16-bit peripheral range

            // Assert - 16-bit peripherals should allow read/write access
            Assert.True(memoryMap.IsAccessAllowed(0x0240, MemoryAccessPermissions.Read));
            Assert.True(memoryMap.IsAccessAllowed(0x0240, MemoryAccessPermissions.Write));
            Assert.Equal(MemoryAccessPermissions.ReadWrite, permissions);
        }

        [Theory]
        [InlineData(0x0200, 128)]  // Total 16-bit peripheral region size
        public void MemoryMap_SixteenBitPeripherals_SizeCalculation(ushort startAddress, int expectedSize)
        {
            // Arrange
            var memoryMap = new MemoryMap();
            MemoryRegionInfo regionInfo = memoryMap.GetRegionInfo(MemoryRegion.Peripherals16Bit);

            // Act
            int actualSize = regionInfo.EndAddress - regionInfo.StartAddress + 1;

            // Assert
            Assert.Equal(expectedSize, actualSize);
            Assert.Equal(startAddress, regionInfo.StartAddress);
        }
    }

    /// <summary>
    /// Tests for Special Function Registers (SFR) according to MSP430FR2355 specifications.
    /// </summary>
    public class SpecialFunctionRegisterTests
    {
        [Fact]
        public void MemoryMap_SpecialFunctionRegisters_CorrectAddressRange()
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act
            MemoryRegionInfo regionInfo = memoryMap.GetRegionInfo(MemoryRegion.SpecialFunctionRegisters);

            // Assert - SFRs should be at 0x0000-0x00FF per MSP430FR2355
            Assert.Equal(0x0000, regionInfo.StartAddress);
            Assert.Equal(0x00FF, regionInfo.EndAddress);
            Assert.Equal(MemoryRegion.SpecialFunctionRegisters, regionInfo.Region);
            Assert.Equal("Special Function Registers", regionInfo.Description);
        }

        [Theory]
        [InlineData(0x0000)]  // Start of SFR range
        [InlineData(0x0050)]  // Middle of SFR range
        [InlineData(0x00FF)]  // End of SFR range
        public void MemoryMap_SpecialFunctionRegisterAddresses_ValidAccess(ushort address)
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act
            MemoryRegionInfo region = memoryMap.GetRegion(address);
            bool isValid = memoryMap.IsValidAddress(address);

            // Assert
            Assert.True(isValid);
            Assert.Equal(MemoryRegion.SpecialFunctionRegisters, region.Region);
        }

        [Fact]
        public void MemoryMap_SpecialFunctionRegisters_ReadWritePermissions()
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act
            MemoryAccessPermissions permissions = memoryMap.GetPermissions(0x0050); // Middle of SFR range

            // Assert - SFRs should allow read/write access
            Assert.True(memoryMap.IsAccessAllowed(0x0050, MemoryAccessPermissions.Read));
            Assert.True(memoryMap.IsAccessAllowed(0x0050, MemoryAccessPermissions.Write));
            Assert.Equal(MemoryAccessPermissions.ReadWrite, permissions);
        }

        [Theory]
        [InlineData(0x0000, 256)]  // Total SFR region size
        public void MemoryMap_SpecialFunctionRegisters_SizeCalculation(ushort startAddress, int expectedSize)
        {
            // Arrange
            var memoryMap = new MemoryMap();
            MemoryRegionInfo regionInfo = memoryMap.GetRegionInfo(MemoryRegion.SpecialFunctionRegisters);

            // Act
            int actualSize = regionInfo.EndAddress - regionInfo.StartAddress + 1;

            // Assert
            Assert.Equal(expectedSize, actualSize);
            Assert.Equal(startAddress, regionInfo.StartAddress);
        }
    }

    /// <summary>
    /// Tests for peripheral address space validation and boundary conditions.
    /// </summary>
    public class PeripheralAddressSpaceValidationTests
    {
        [Theory]
        [InlineData(0x0000, 0x00FF, MemoryRegion.SpecialFunctionRegisters)]    // SFRs
        [InlineData(0x0100, 0x01FF, MemoryRegion.Peripherals8Bit)]             // 8-bit peripherals
        [InlineData(0x0200, 0x027F, MemoryRegion.Peripherals16Bit)]            // 16-bit peripherals
        public void MemoryMap_PeripheralRegions_NoAddressGaps(ushort startAddr, ushort endAddr, MemoryRegion expectedRegion)
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act & Assert - Every address in range should map to expected region
            for (ushort addr = startAddr; addr <= endAddr; addr++)
            {
                MemoryRegionInfo region = memoryMap.GetRegion(addr);
                Assert.Equal(expectedRegion, region.Region);
            }
        }

        [Theory]
        [InlineData(0x0280)]  // First address after 16-bit peripherals
        [InlineData(0x0300)]  // Well after peripheral regions
        [InlineData(0x1FFF)]  // Just before SRAM
        public void MemoryMap_NonPeripheralAddresses_NotInPeripheralRegions(ushort address)
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act & Assert
            if (memoryMap.IsValidAddress(address))
            {
                MemoryRegionInfo region = memoryMap.GetRegion(address);
                Assert.NotEqual(MemoryRegion.SpecialFunctionRegisters, region.Region);
                Assert.NotEqual(MemoryRegion.Peripherals8Bit, region.Region);
                Assert.NotEqual(MemoryRegion.Peripherals16Bit, region.Region);
            }
            else
            {
                // Address is unmapped - this is expected for many addresses
                Assert.Throws<ArgumentException>(() => memoryMap.GetRegion(address));
            }
        }

        [Fact]
        public void MemoryMap_AllPeripheralRegions_HaveReadWriteAccess()
        {
            // Arrange
            var memoryMap = new MemoryMap();
            MemoryRegion[] peripheralRegions = new[]
            {
                MemoryRegion.SpecialFunctionRegisters,
                MemoryRegion.Peripherals8Bit,
                MemoryRegion.Peripherals16Bit
            };

            // Act & Assert
            IEnumerable<ushort> testAddresses = peripheralRegions
                .Select(regionType => memoryMap.GetRegionInfo(regionType))
                .Select(regionInfo => (ushort)((regionInfo.StartAddress + regionInfo.EndAddress) / 2));

            foreach (ushort testAddress in testAddresses)
            {
                Assert.True(memoryMap.IsAccessAllowed(testAddress, MemoryAccessPermissions.Read));
                Assert.True(memoryMap.IsAccessAllowed(testAddress, MemoryAccessPermissions.Write));
                Assert.False(memoryMap.IsAccessAllowed(testAddress, MemoryAccessPermissions.Execute));
            }
        }

        [Fact]
        public void MemoryMap_PeripheralRegionSizes_MatchMSP430FR2355Specification()
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act & Assert - Verify sizes match MSP430FR2355 specification
            MemoryRegionInfo sfrInfo = memoryMap.GetRegionInfo(MemoryRegion.SpecialFunctionRegisters);
            Assert.Equal(256, sfrInfo.EndAddress - sfrInfo.StartAddress + 1); // 256 bytes

            MemoryRegionInfo per8Info = memoryMap.GetRegionInfo(MemoryRegion.Peripherals8Bit);
            Assert.Equal(256, per8Info.EndAddress - per8Info.StartAddress + 1); // 256 bytes

            MemoryRegionInfo per16Info = memoryMap.GetRegionInfo(MemoryRegion.Peripherals16Bit);
            Assert.Equal(128, per16Info.EndAddress - per16Info.StartAddress + 1); // 128 bytes
        }
    }

    /// <summary>
    /// Tests for peripheral access patterns and behavior validation.
    /// </summary>
    public class PeripheralAccessPatternTests
    {
        [Theory]
        [InlineData(0x0050)]  // SFR address
        [InlineData(0x0150)]  // 8-bit peripheral address
        [InlineData(0x0250)]  // 16-bit peripheral address
        public void MemoryMap_PeripheralAddresses_AllowByteAccess(ushort address)
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act & Assert - All peripheral addresses should allow byte-level access
            Assert.True(memoryMap.IsValidAddress(address));
            Assert.True(memoryMap.IsAccessAllowed(address, MemoryAccessPermissions.Read));
            Assert.True(memoryMap.IsAccessAllowed(address, MemoryAccessPermissions.Write));
        }

        [Theory]
        [InlineData(0x0200)]  // Start of 16-bit peripheral range
        [InlineData(0x0240)]  // Middle of 16-bit peripheral range
        [InlineData(0x027E)]  // Even address near end of 16-bit peripheral range
        public void MemoryMap_SixteenBitPeripherals_AllowWordAccess(ushort address)
        {
            // Arrange
            var memoryMap = new MemoryMap();

            // Act & Assert - 16-bit peripherals should allow word-aligned access
            Assert.True(memoryMap.IsValidAddress(address));
            Assert.True(memoryMap.IsValidAddress((ushort)(address + 1))); // Next byte should also be valid
            Assert.True(memoryMap.IsAccessAllowed(address, MemoryAccessPermissions.Read));
            Assert.True(memoryMap.IsAccessAllowed(address, MemoryAccessPermissions.Write));
        }

        [Fact]
        public void MemoryMap_PeripheralRegions_NoExecutePermission()
        {
            // Arrange
            var memoryMap = new MemoryMap();
            ushort[] testAddresses = new ushort[] { 0x0050, 0x0150, 0x0250 }; // SFR, 8-bit, 16-bit

            // Act & Assert - Peripheral regions should not allow code execution
            foreach (ushort address in testAddresses)
            {
                Assert.False(memoryMap.IsAccessAllowed(address, MemoryAccessPermissions.Execute));

                MemoryAccessPermissions permissions = memoryMap.GetPermissions(address);
                Assert.Equal(MemoryAccessPermissions.ReadWrite, permissions);
            }
        }
    }
}
