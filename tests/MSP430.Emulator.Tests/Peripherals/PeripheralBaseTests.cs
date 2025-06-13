using System;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Peripherals;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Peripherals;

/// <summary>
/// Unit tests for the PeripheralBase abstract class.
/// 
/// Tests peripheral foundation functionality including:
/// - Register management and memory-mapped access
/// - Address space validation and routing
/// - Reset and initialization behavior
/// - Event handling and lifecycle operations
/// </summary>
public class PeripheralBaseTests
{
    private readonly TestLogger _logger;

    public PeripheralBaseTests()
    {
        _logger = new TestLogger();
    }

    /// <summary>
    /// Test peripheral implementation for testing PeripheralBase functionality.
    /// </summary>
    private class TestPeripheral : PeripheralBase
    {
        public TestPeripheral(string peripheralId, ushort baseAddress, ushort addressSpaceSize, ILogger? logger = null)
            : base(peripheralId, baseAddress, addressSpaceSize, logger)
        {
        }

        public void AddTestRegister(PeripheralRegister register)
        {
            AddRegister(register);
        }

        public PeripheralRegister? GetTestRegister(ushort address)
        {
            return GetRegister(address);
        }

        public void SetTestEnabled(bool enabled)
        {
            SetEnabled(enabled);
        }

        public void TriggerInterrupt(ushort vector, string name, byte priority)
        {
            OnInterruptRequested(vector, name, priority);
        }
    }

    /// <summary>
    /// Tests for peripheral construction and basic properties.
    /// </summary>
    public class ConstructionTests : PeripheralBaseTests
    {
        [Theory]
        [InlineData("TEST_PERIPHERAL", 0x0200, 16)]
        [InlineData("GPIO_PORT1", 0x0100, 8)]
        [InlineData("TIMER_A0", 0x0340, 32)]
        public void Constructor_WithValidParameters_SetsProperties(string peripheralId, ushort baseAddress, ushort addressSpaceSize)
        {
            // Act
            var peripheral = new TestPeripheral(peripheralId, baseAddress, addressSpaceSize, _logger);

            // Assert
            Assert.Equal(peripheralId, peripheral.PeripheralId);
            Assert.Equal(baseAddress, peripheral.BaseAddress);
            Assert.Equal(addressSpaceSize, peripheral.AddressSpaceSize);
            Assert.Equal((ushort)(baseAddress + addressSpaceSize - 1), peripheral.EndAddress);
            Assert.True(peripheral.IsEnabled);
        }

        [Fact]
        public void Constructor_WithNullPeripheralId_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestPeripheral(null!, 0x0200, 16));
        }

        [Theory]
        [InlineData(0)]
        public void Constructor_WithZeroAddressSpaceSize_ThrowsArgumentException(ushort addressSpaceSize)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new TestPeripheral("TEST", 0x0200, addressSpaceSize));
        }

        [Theory]
        [InlineData(0x0200, 16, 0x020F)]
        [InlineData(0x0100, 8, 0x0107)]
        [InlineData(0x0340, 32, 0x035F)]
        public void Constructor_CalculatesEndAddressCorrectly(ushort baseAddress, ushort addressSpaceSize, ushort expectedEndAddress)
        {
            // Act
            var peripheral = new TestPeripheral("TEST", baseAddress, addressSpaceSize);

            // Assert
            Assert.Equal(expectedEndAddress, peripheral.EndAddress);
        }
    }

    /// <summary>
    /// Tests for address handling and validation.
    /// </summary>
    public class AddressHandlingTests : PeripheralBaseTests
    {
        [Theory]
        [InlineData(0x0200, 16, 0x0200, true)]  // Base address
        [InlineData(0x0200, 16, 0x020F, true)]  // End address
        [InlineData(0x0200, 16, 0x0205, true)]  // Middle address
        [InlineData(0x0200, 16, 0x01FF, false)] // Below base
        [InlineData(0x0200, 16, 0x0210, false)] // Above end
        public void HandlesAddress_WithVariousAddresses_ReturnsExpectedResult(ushort baseAddress, ushort addressSpaceSize, ushort testAddress, bool expectedResult)
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", baseAddress, addressSpaceSize);

            // Act
            bool result = peripheral.HandlesAddress(testAddress);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }

    /// <summary>
    /// Tests for register management functionality.
    /// </summary>
    public class RegisterManagementTests : PeripheralBaseTests
    {
        [Fact]
        public void AddRegister_WithValidRegister_AddsSuccessfully()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16, _logger);
            var register = new PeripheralRegister(0x0200, 0x1234, PeripheralRegisterAccess.ReadWrite, name: "TEST_REG");

            // Act
            peripheral.AddTestRegister(register);

            // Assert
            PeripheralRegister? retrievedRegister = peripheral.GetTestRegister(0x0200);
            Assert.NotNull(retrievedRegister);
            Assert.Equal("TEST_REG", retrievedRegister.Name);
            Assert.Equal(0x1234, retrievedRegister.Value);
        }

        [Fact]
        public void AddRegister_WithNullRegister_ThrowsArgumentNullException()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => peripheral.AddTestRegister(null!));
        }

        [Fact]
        public void AddRegister_WithAddressOutsideRange_ThrowsArgumentException()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);
            var register = new PeripheralRegister(0x0300); // Outside range

            // Act & Assert
            Assert.Throws<ArgumentException>(() => peripheral.AddTestRegister(register));
        }

        [Fact]
        public void AddRegister_WithDuplicateAddress_ThrowsArgumentException()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);
            var register1 = new PeripheralRegister(0x0200);
            var register2 = new PeripheralRegister(0x0200);

            peripheral.AddTestRegister(register1);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => peripheral.AddTestRegister(register2));
        }
    }

    /// <summary>
    /// Tests for memory access operations (read/write).
    /// </summary>
    public class MemoryAccessTests : PeripheralBaseTests
    {
        [Theory]
        [InlineData(0x0200, 0x1234, 0x34)] // Low byte
        [InlineData(0x0201, 0x1234, 0x12)] // High byte
        public void ReadByte_FromRegisteredAddress_ReturnsCorrectValue(ushort address, ushort registerValue, byte expectedByte)
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);
            var register = new PeripheralRegister(0x0200, registerValue);
            peripheral.AddTestRegister(register);

            // Act
            byte result = peripheral.ReadByte(address);

            // Assert
            Assert.Equal(expectedByte, result);
        }

        [Theory]
        [InlineData(0x0200, 0x1234)]
        [InlineData(0x0202, 0x5678)]
        public void ReadWord_FromRegisteredAddress_ReturnsCorrectValue(ushort address, ushort expectedValue)
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);
            var register = new PeripheralRegister(address, expectedValue);
            peripheral.AddTestRegister(register);

            // Act
            ushort result = peripheral.ReadWord(address);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void ReadWord_WithOddAddress_ThrowsArgumentException()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => peripheral.ReadWord(0x0201));
        }

        [Theory]
        [InlineData(0x0200, 0x55, 0x0055)] // Low byte write
        [InlineData(0x0201, 0x55, 0x5500)] // High byte write
        public void WriteByte_ToRegisteredAddress_UpdatesCorrectByte(ushort address, byte value, ushort expectedRegisterValue)
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);
            var register = new PeripheralRegister(0x0200, 0x0000);
            peripheral.AddTestRegister(register);

            // Act
            bool result = peripheral.WriteByte(address, value);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedRegisterValue, register.Value);
        }

        [Theory]
        [InlineData(0x0200, 0x1234)]
        [InlineData(0x0202, 0x5678)]
        public void WriteWord_ToRegisteredAddress_UpdatesRegister(ushort address, ushort value)
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);
            var register = new PeripheralRegister(address, 0x0000);
            peripheral.AddTestRegister(register);

            // Act
            bool result = peripheral.WriteWord(address, value);

            // Assert
            Assert.True(result);
            Assert.Equal(value, register.Value);
        }

        [Fact]
        public void WriteWord_WithOddAddress_ThrowsArgumentException()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => peripheral.WriteWord(0x0201, 0x1234));
        }

        [Fact]
        public void ReadByte_FromUnhandledAddress_ThrowsArgumentException()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => peripheral.ReadByte(0x0300));
        }

        [Fact]
        public void WriteByte_ToUnhandledAddress_ThrowsArgumentException()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => peripheral.WriteByte(0x0300, 0x55));
        }
    }

    /// <summary>
    /// Tests for peripheral enable/disable functionality.
    /// </summary>
    public class EnableDisableTests : PeripheralBaseTests
    {
        [Fact]
        public void SetEnabled_ToFalse_DisablesPeripheral()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);

            // Act
            peripheral.SetTestEnabled(false);

            // Assert
            Assert.False(peripheral.IsEnabled);
        }

        [Fact]
        public void ReadByte_WhenDisabled_ThrowsInvalidOperationException()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);
            peripheral.SetTestEnabled(false);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => peripheral.ReadByte(0x0200));
        }

        [Fact]
        public void WriteByte_WhenDisabled_ThrowsInvalidOperationException()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);
            peripheral.SetTestEnabled(false);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => peripheral.WriteByte(0x0200, 0x55));
        }
    }

    /// <summary>
    /// Tests for reset and initialization functionality.
    /// </summary>
    public class ResetInitializeTests : PeripheralBaseTests
    {
        [Fact]
        public void Reset_WithRegisters_ResetsAllRegistersToDefaultValues()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16, _logger);
            var register1 = new PeripheralRegister(0x0200, 0x1234);
            var register2 = new PeripheralRegister(0x0202, 0x5678);

            peripheral.AddTestRegister(register1);
            peripheral.AddTestRegister(register2);

            // Modify register values
            register1.WriteWord(0xAAAA);
            register2.WriteWord(0xBBBB);

            // Act
            peripheral.Reset();

            // Assert
            Assert.Equal(0x1234, register1.Value);
            Assert.Equal(0x5678, register2.Value);
        }

        [Fact]
        public void Initialize_CallsSuccessfully()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16, _logger);

            // Act
            peripheral.Initialize();

            // Assert - Should not throw and peripheral should remain enabled
            Assert.True(peripheral.IsEnabled);
        }

        [Fact]
        public void Initialize_WithData_CallsSuccessfully()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16, _logger);
            var initData = new { TestProperty = "TestValue" };

            // Act
            peripheral.Initialize(initData);

            // Assert - Should not throw and peripheral should remain enabled
            Assert.True(peripheral.IsEnabled);
        }
    }

    /// <summary>
    /// Tests for event handling functionality.
    /// </summary>
    public class EventHandlingTests : PeripheralBaseTests
    {
        [Fact]
        public void RegisterRead_RaisesEvent()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);
            var register = new PeripheralRegister(0x0200, 0x1234);
            peripheral.AddTestRegister(register);

            PeripheralRegisterAccessEventArgs? capturedEventArgs = null;
            peripheral.RegisterRead += (sender, e) => capturedEventArgs = e;

            // Act
            peripheral.ReadByte(0x0200);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Equal(0x0200, capturedEventArgs.Address);
            Assert.Equal(0x34, capturedEventArgs.ByteValue);
            Assert.True(capturedEventArgs.IsRead);
            Assert.False(capturedEventArgs.IsWrite);
            Assert.True(capturedEventArgs.IsByteAccess);
        }

        [Fact]
        public void RegisterWritten_RaisesEvent()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);
            var register = new PeripheralRegister(0x0200, 0x0000);
            peripheral.AddTestRegister(register);

            PeripheralRegisterAccessEventArgs? capturedEventArgs = null;
            peripheral.RegisterWritten += (sender, e) => capturedEventArgs = e;

            // Act
            peripheral.WriteByte(0x0200, 0x55);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Equal(0x0200, capturedEventArgs.Address);
            Assert.Equal(0x55, capturedEventArgs.ByteValue);
            Assert.True(capturedEventArgs.IsWrite);
            Assert.False(capturedEventArgs.IsRead);
            Assert.True(capturedEventArgs.IsByteAccess);
        }

        [Fact]
        public void InterruptRequested_RaisesEvent()
        {
            // Arrange
            var peripheral = new TestPeripheral("TEST", 0x0200, 16);

            PeripheralInterruptEventArgs? capturedEventArgs = null;
            peripheral.InterruptRequested += (sender, e) => capturedEventArgs = e;

            // Act
            peripheral.TriggerInterrupt(0xFFE4, "TEST_INTERRUPT", 5);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Equal(0xFFE4, capturedEventArgs.InterruptVector);
            Assert.Equal("TEST_INTERRUPT", capturedEventArgs.InterruptName);
            Assert.Equal(5, capturedEventArgs.Priority);
            Assert.Equal("TEST", capturedEventArgs.PeripheralId);
        }
    }
}
