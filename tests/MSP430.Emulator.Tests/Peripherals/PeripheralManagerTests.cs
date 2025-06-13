using System;
using System.Linq;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Peripherals;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Peripherals;

/// <summary>
/// Unit tests for the PeripheralManager class.
/// 
/// Tests peripheral management functionality including:
/// - Peripheral registration and unregistration
/// - Address routing and conflict detection
/// - Memory access delegation
/// - Event aggregation and lifecycle management
/// </summary>
public class PeripheralManagerTests
{
    private readonly TestLogger _logger;

    public PeripheralManagerTests()
    {
        _logger = new TestLogger();
    }

    /// <summary>
    /// Mock peripheral implementation for testing.
    /// </summary>
    private class MockPeripheral : IPeripheral
    {
        private bool _isEnabled = true;
        private byte _lastReadValue = 0;
        private byte _lastWrittenValue = 0;

        public MockPeripheral(string peripheralId, ushort baseAddress, ushort addressSpaceSize)
        {
            PeripheralId = peripheralId;
            BaseAddress = baseAddress;
            AddressSpaceSize = addressSpaceSize;
            EndAddress = (ushort)(baseAddress + addressSpaceSize - 1);
        }

        public string PeripheralId { get; }
        public ushort BaseAddress { get; }
        public ushort AddressSpaceSize { get; }
        public ushort EndAddress { get; }
        public bool IsEnabled => _isEnabled;

        public event EventHandler<PeripheralRegisterAccessEventArgs>? RegisterRead;
        public event EventHandler<PeripheralRegisterAccessEventArgs>? RegisterWritten;
        public event EventHandler<PeripheralInterruptEventArgs>? InterruptRequested;

        public bool HandlesAddress(ushort address)
        {
            return address >= BaseAddress && address <= EndAddress;
        }

        public byte ReadByte(ushort address)
        {
            if (!HandlesAddress(address))
            {
                throw new ArgumentException($"Address not handled: 0x{address:X4}");
            }

            RegisterRead?.Invoke(this, new PeripheralRegisterAccessEventArgs(address, _lastReadValue, false, false));
            return _lastReadValue;
        }

        public bool WriteByte(ushort address, byte value)
        {
            if (!HandlesAddress(address))
            {
                throw new ArgumentException($"Address not handled: 0x{address:X4}");
            }

            _lastWrittenValue = value;
            RegisterWritten?.Invoke(this, new PeripheralRegisterAccessEventArgs(address, value, true, false));
            return true;
        }

        public ushort ReadWord(ushort address)
        {
            if ((address & 1) != 0)
            {
                throw new ArgumentException("Address not word-aligned");
            }

            if (!HandlesAddress(address))
            {
                throw new ArgumentException($"Address not handled: 0x{address:X4}");
            }

            ushort value = (ushort)(_lastReadValue | (_lastReadValue << 8));
            RegisterRead?.Invoke(this, new PeripheralRegisterAccessEventArgs(address, value, false, true));
            return value;
        }

        public bool WriteWord(ushort address, ushort value)
        {
            if ((address & 1) != 0)
            {
                throw new ArgumentException("Address not word-aligned");
            }

            if (!HandlesAddress(address))
            {
                throw new ArgumentException($"Address not handled: 0x{address:X4}");
            }

            _lastWrittenValue = (byte)(value & 0xFF);
            RegisterWritten?.Invoke(this, new PeripheralRegisterAccessEventArgs(address, value, true, true));
            return true;
        }

        public void Reset()
        {
            _lastReadValue = 0;
            _lastWrittenValue = 0;
        }

        public void Initialize(object? initializationData = null)
        {
            // Mock implementation does nothing
        }

        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }

        public void SetReadValue(byte value)
        {
            _lastReadValue = value;
        }

        public byte GetLastWrittenValue()
        {
            return _lastWrittenValue;
        }

        public void TriggerInterrupt(ushort vector, string name, byte priority)
        {
            InterruptRequested?.Invoke(this, new PeripheralInterruptEventArgs(vector, name, priority, PeripheralId));
        }
    }

    /// <summary>
    /// Tests for peripheral manager construction and basic properties.
    /// </summary>
    public class ConstructionTests : PeripheralManagerTests
    {
        [Fact]
        public void Constructor_WithLogger_InitializesCorrectly()
        {
            // Act
            var manager = new PeripheralManager(_logger);

            // Assert
            Assert.NotNull(manager.Peripherals);
            Assert.Equal(0, manager.Count);
            Assert.False(manager.IsInitialized);
        }

        [Fact]
        public void Constructor_WithoutLogger_InitializesCorrectly()
        {
            // Act
            var manager = new PeripheralManager();

            // Assert
            Assert.NotNull(manager.Peripherals);
            Assert.Equal(0, manager.Count);
            Assert.False(manager.IsInitialized);
        }
    }

    /// <summary>
    /// Tests for peripheral registration functionality.
    /// </summary>
    public class RegistrationTests : PeripheralManagerTests
    {
        [Theory]
        [InlineData("GPIO_P1", 0x0200, 16)]
        [InlineData("TIMER_A0", 0x0340, 32)]
        [InlineData("UART", 0x05C0, 8)]
        public void RegisterPeripheral_WithValidPeripheral_AddsSuccessfully(string peripheralId, ushort baseAddress, ushort addressSpaceSize)
        {
            // Arrange
            var manager = new PeripheralManager(_logger);
            var peripheral = new MockPeripheral(peripheralId, baseAddress, addressSpaceSize);

            // Act
            manager.RegisterPeripheral(peripheral);

            // Assert
            Assert.Equal(1, manager.Count);
            Assert.Contains(peripheral, manager.Peripherals);
            Assert.Equal(peripheral, manager.GetPeripheral(peripheralId));
        }

        [Fact]
        public void RegisterPeripheral_WithNullPeripheral_ThrowsArgumentNullException()
        {
            // Arrange
            var manager = new PeripheralManager();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => manager.RegisterPeripheral(null!));
        }

        [Fact]
        public void RegisterPeripheral_WithDuplicateId_ThrowsInvalidOperationException()
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral1 = new MockPeripheral("TEST", 0x0200, 16);
            var peripheral2 = new MockPeripheral("TEST", 0x0300, 16);

            manager.RegisterPeripheral(peripheral1);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => manager.RegisterPeripheral(peripheral2));
        }

        [Theory]
        [InlineData(0x0200, 16, 0x0205, 16)] // Overlapping ranges
        [InlineData(0x0200, 16, 0x020F, 1)]  // Touching end address
        [InlineData(0x0200, 16, 0x01F0, 17)] // Overlapping from below
        public void RegisterPeripheral_WithAddressConflict_ThrowsInvalidOperationException(
            ushort baseAddress1, ushort size1, ushort baseAddress2, ushort size2)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral1 = new MockPeripheral("PERIPHERAL1", baseAddress1, size1);
            var peripheral2 = new MockPeripheral("PERIPHERAL2", baseAddress2, size2);

            manager.RegisterPeripheral(peripheral1);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => manager.RegisterPeripheral(peripheral2));
        }

        [Theory]
        [InlineData(0x0200, 16, 0x0220, 16)] // No overlap
        [InlineData(0x0200, 16, 0x0100, 16)] // No overlap (below)
        public void RegisterPeripheral_WithoutAddressConflict_AddsSuccessfully(
            ushort baseAddress1, ushort size1, ushort baseAddress2, ushort size2)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral1 = new MockPeripheral("PERIPHERAL1", baseAddress1, size1);
            var peripheral2 = new MockPeripheral("PERIPHERAL2", baseAddress2, size2);

            // Act
            manager.RegisterPeripheral(peripheral1);
            manager.RegisterPeripheral(peripheral2);

            // Assert
            Assert.Equal(2, manager.Count);
            Assert.Contains(peripheral1, manager.Peripherals);
            Assert.Contains(peripheral2, manager.Peripherals);
        }
    }

    /// <summary>
    /// Tests for peripheral unregistration functionality.
    /// </summary>
    public class UnregistrationTests : PeripheralManagerTests
    {
        [Fact]
        public void UnregisterPeripheral_WithExistingPeripheral_RemovesSuccessfully()
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            // Act
            bool result = manager.UnregisterPeripheral("TEST");

            // Assert
            Assert.True(result);
            Assert.Equal(0, manager.Count);
            Assert.DoesNotContain(peripheral, manager.Peripherals);
            Assert.Null(manager.GetPeripheral("TEST"));
        }

        [Fact]
        public void UnregisterPeripheral_WithNonExistentPeripheral_ReturnsFalse()
        {
            // Arrange
            var manager = new PeripheralManager();

            // Act
            bool result = manager.UnregisterPeripheral("NONEXISTENT");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void UnregisterPeripheral_WithInvalidId_ReturnsFalse(string? peripheralId)
        {
            // Arrange
            var manager = new PeripheralManager();

            // Act
            bool result = manager.UnregisterPeripheral(peripheralId!);

            // Assert
            Assert.False(result);
        }
    }

    /// <summary>
    /// Tests for address lookup and routing functionality.
    /// </summary>
    public class AddressLookupTests : PeripheralManagerTests
    {
        [Theory]
        [InlineData(0x0200)] // Base address
        [InlineData(0x020F)] // End address
        [InlineData(0x0205)] // Middle address
        public void GetPeripheralForAddress_WithHandledAddress_ReturnsCorrectPeripheral(ushort address)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            // Act
            IPeripheral? result = manager.GetPeripheralForAddress(address);

            // Assert
            Assert.Equal(peripheral, result);
        }

        [Theory]
        [InlineData(0x01FF)] // Below range
        [InlineData(0x0210)] // Above range
        public void GetPeripheralForAddress_WithUnhandledAddress_ReturnsNull(ushort address)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            // Act
            IPeripheral? result = manager.GetPeripheralForAddress(address);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0x0200, true)]  // Handled by first peripheral
        [InlineData(0x0300, true)]  // Handled by second peripheral
        [InlineData(0x0100, false)] // Not handled by any
        public void HandlesAddress_WithMultiplePeripherals_ReturnsCorrectResult(ushort address, bool expectedResult)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral1 = new MockPeripheral("TEST1", 0x0200, 16);
            var peripheral2 = new MockPeripheral("TEST2", 0x0300, 16);
            manager.RegisterPeripheral(peripheral1);
            manager.RegisterPeripheral(peripheral2);

            // Act
            bool result = manager.HandlesAddress(address);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }

    /// <summary>
    /// Tests for memory access delegation functionality.
    /// </summary>
    public class MemoryAccessTests : PeripheralManagerTests
    {
        [Theory]
        [InlineData(0x0200, 0x55)]
        [InlineData(0x0205, 0xAA)]
        public void ReadByte_WithHandledAddress_DelegatesToPeripheral(ushort address, byte expectedValue)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            peripheral.SetReadValue(expectedValue);
            manager.RegisterPeripheral(peripheral);

            // Act
            byte result = manager.ReadByte(address);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void ReadByte_WithUnhandledAddress_ThrowsArgumentException()
        {
            // Arrange
            var manager = new PeripheralManager();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => manager.ReadByte(0x0200));
        }

        [Theory]
        [InlineData(0x0200, 0x55)]
        [InlineData(0x0205, 0xAA)]
        public void WriteByte_WithHandledAddress_DelegatesToPeripheral(ushort address, byte value)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            // Act
            bool result = manager.WriteByte(address, value);

            // Assert
            Assert.True(result);
            Assert.Equal(value, peripheral.GetLastWrittenValue());
        }

        [Fact]
        public void WriteByte_WithUnhandledAddress_ThrowsArgumentException()
        {
            // Arrange
            var manager = new PeripheralManager();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => manager.WriteByte(0x0200, 0x55));
        }

        [Theory]
        [InlineData(0x0200)]
        [InlineData(0x0204)]
        public void ReadWord_WithHandledAddress_DelegatesToPeripheral(ushort address)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            peripheral.SetReadValue(0x55);
            manager.RegisterPeripheral(peripheral);

            // Act
            ushort result = manager.ReadWord(address);

            // Assert
            Assert.Equal(0x5555, result); // Mock returns same byte for both low and high
        }

        [Theory]
        [InlineData(0x0201)] // Odd address
        public void ReadWord_WithOddAddress_ThrowsArgumentException(ushort address)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => manager.ReadWord(address));
        }

        [Theory]
        [InlineData(0x0200, 0x1234)]
        [InlineData(0x0204, 0x5678)]
        public void WriteWord_WithHandledAddress_DelegatesToPeripheral(ushort address, ushort value)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            // Act
            bool result = manager.WriteWord(address, value);

            // Assert
            Assert.True(result);
            Assert.Equal((byte)(value & 0xFF), peripheral.GetLastWrittenValue());
        }

        [Theory]
        [InlineData(0x0201)] // Odd address
        public void WriteWord_WithOddAddress_ThrowsArgumentException(ushort address)
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => manager.WriteWord(address, 0x1234));
        }
    }

    /// <summary>
    /// Tests for reset and initialization functionality.
    /// </summary>
    public class ResetInitializeTests : PeripheralManagerTests
    {
        [Fact]
        public void Reset_WithMultiplePeripherals_ResetsAll()
        {
            // Arrange
            var manager = new PeripheralManager(_logger);
            var peripheral1 = new MockPeripheral("TEST1", 0x0200, 16);
            var peripheral2 = new MockPeripheral("TEST2", 0x0300, 16);

            peripheral1.SetReadValue(0x55);
            peripheral2.SetReadValue(0xAA);

            manager.RegisterPeripheral(peripheral1);
            manager.RegisterPeripheral(peripheral2);

            // Act
            manager.Reset();

            // Assert
            // Mock peripherals reset their read values to 0
            Assert.Equal(0x00, manager.ReadByte(0x0200));
            Assert.Equal(0x00, manager.ReadByte(0x0300));
        }

        [Fact]
        public void Initialize_WithoutData_InitializesAllPeripherals()
        {
            // Arrange
            var manager = new PeripheralManager(_logger);
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            // Act
            manager.Initialize();

            // Assert
            Assert.True(manager.IsInitialized);
        }

        [Fact]
        public void Initialize_WithData_InitializesAllPeripherals()
        {
            // Arrange
            var manager = new PeripheralManager(_logger);
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);
            var initData = new { TestProperty = "TestValue" };

            // Act
            manager.Initialize(initData);

            // Assert
            Assert.True(manager.IsInitialized);
        }
    }

    /// <summary>
    /// Tests for event aggregation functionality.
    /// </summary>
    public class EventAggregationTests : PeripheralManagerTests
    {
        [Fact]
        public void RegisterRead_FromPeripheral_PropagatesEvent()
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            PeripheralRegisterAccessEventArgs? capturedEventArgs = null;
            manager.RegisterRead += (sender, e) => capturedEventArgs = e;

            // Act
            manager.ReadByte(0x0200);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Equal(0x0200, capturedEventArgs.Address);
            Assert.True(capturedEventArgs.IsRead);
        }

        [Fact]
        public void RegisterWritten_FromPeripheral_PropagatesEvent()
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            PeripheralRegisterAccessEventArgs? capturedEventArgs = null;
            manager.RegisterWritten += (sender, e) => capturedEventArgs = e;

            // Act
            manager.WriteByte(0x0200, 0x55);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Equal(0x0200, capturedEventArgs.Address);
            Assert.Equal(0x55, capturedEventArgs.ByteValue);
            Assert.True(capturedEventArgs.IsWrite);
        }

        [Fact]
        public void InterruptRequested_FromPeripheral_PropagatesEvent()
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            PeripheralInterruptEventArgs? capturedEventArgs = null;
            manager.InterruptRequested += (sender, e) => capturedEventArgs = e;

            // Act
            peripheral.TriggerInterrupt(0xFFE4, "TEST_INTERRUPT", 5);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Equal(0xFFE4, capturedEventArgs.InterruptVector);
            Assert.Equal("TEST_INTERRUPT", capturedEventArgs.InterruptName);
            Assert.Equal(5, capturedEventArgs.Priority);
            Assert.Equal("TEST", capturedEventArgs.PeripheralId);
        }

        [Fact]
        public void UnregisterPeripheral_StopsEventPropagation()
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral = new MockPeripheral("TEST", 0x0200, 16);
            manager.RegisterPeripheral(peripheral);

            bool eventReceived = false;
            manager.RegisterRead += (sender, e) => eventReceived = true;

            // Act
            manager.UnregisterPeripheral("TEST");
            peripheral.ReadByte(0x0200); // Direct call to peripheral

            // Assert
            Assert.False(eventReceived);
        }
    }

    /// <summary>
    /// Tests for diagnostic information functionality.
    /// </summary>
    public class DiagnosticTests : PeripheralManagerTests
    {
        [Fact]
        public void GetDiagnosticInfo_WithMultiplePeripherals_ReturnsCorrectInfo()
        {
            // Arrange
            var manager = new PeripheralManager();
            var peripheral1 = new MockPeripheral("TEST1", 0x0200, 16);
            var peripheral2 = new MockPeripheral("TEST2", 0x0300, 8);

            manager.RegisterPeripheral(peripheral1);
            manager.RegisterPeripheral(peripheral2);
            manager.Initialize();

            // Act
            System.Collections.Generic.Dictionary<string, object> diagnosticInfo = manager.GetDiagnosticInfo();

            // Assert
            Assert.Equal(2, diagnosticInfo["TotalPeripherals"]);
            Assert.True((bool)diagnosticInfo["IsInitialized"]);

            var peripheralsList = (System.Collections.IEnumerable)diagnosticInfo["Peripherals"];
            Assert.NotNull(peripheralsList);
            Assert.Equal(2, peripheralsList.Cast<object>().Count());
        }

        [Fact]
        public void GetDiagnosticInfo_WithNoPeripherals_ReturnsEmptyInfo()
        {
            // Arrange
            var manager = new PeripheralManager();

            // Act
            System.Collections.Generic.Dictionary<string, object> diagnosticInfo = manager.GetDiagnosticInfo();

            // Assert
            Assert.Equal(0, diagnosticInfo["TotalPeripherals"]);
            Assert.False((bool)diagnosticInfo["IsInitialized"]);
        }
    }
}
