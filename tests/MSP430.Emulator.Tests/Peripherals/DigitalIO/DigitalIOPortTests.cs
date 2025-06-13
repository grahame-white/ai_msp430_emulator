using System;
using MSP430.Emulator.Peripherals.DigitalIO;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Peripherals.DigitalIO;

/// <summary>
/// Unit tests for the DigitalIOPort class.
/// 
/// Tests digital I/O functionality including:
/// - Pin direction control (input/output)
/// - Output value control and input reading
/// - Pull-up/pull-down resistor simulation
/// - Register read/write operations
/// - Interrupt-on-change functionality
/// </summary>
public class DigitalIOPortTests
{
    private readonly TestLogger _logger;

    public DigitalIOPortTests()
    {
        _logger = new TestLogger();
    }

    /// <summary>
    /// Tests for basic port construction and initialization.
    /// </summary>
    public class ConstructionTests : DigitalIOPortTests
    {
        [Theory]
        [InlineData("P1", 0x0200, 0xFFE4)]
        [InlineData("P2", 0x0220, 0xFFE6)]
        [InlineData("P3", 0x0240, 0xFFE8)]
        public void Constructor_WithValidParameters_SetsProperties(string portName, ushort baseAddress, ushort interruptVector)
        {
            // Act
            var port = new DigitalIOPort(portName, baseAddress, interruptVector, _logger);

            // Assert
            Assert.Equal($"GPIO_{portName}", port.PeripheralId);
            Assert.Equal(portName, port.PortName);
            Assert.Equal(baseAddress, port.BaseAddress);
            Assert.Equal(0x20, port.AddressSpaceSize);
            Assert.Equal((ushort)(baseAddress + 0x1F), port.EndAddress);
            Assert.True(port.IsEnabled);
        }

        [Fact]
        public void Constructor_WithNullPortName_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DigitalIOPort(null!, 0x0200, 0xFFE4, _logger));
        }

        [Fact]
        public void Constructor_InitializesAllPinsAsInputs()
        {
            // Arrange & Act
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Assert
            for (int i = 0; i < 8; i++)
            {
                Assert.Equal(IODirection.Input, port.GetDirection(i));
                Assert.False(port.GetOutputValue(i));
                Assert.Equal(PullResistor.None, port.GetPullResistor(i));
            }
        }
    }

    /// <summary>
    /// Tests for pin state management and access.
    /// </summary>
    public class PinStateTests : DigitalIOPortTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(7)]
        public void GetPin_WithValidPinNumber_ReturnsCorrectPin(int pinNumber)
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Act
            PinState pin = port.GetPin(pinNumber);

            // Assert
            Assert.NotNull(pin);
            Assert.Equal(pinNumber, pin.PinNumber);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(8)]
        [InlineData(15)]
        public void GetPin_WithInvalidPinNumber_ThrowsArgumentOutOfRangeException(int pinNumber)
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => port.GetPin(pinNumber));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(3, false)]
        [InlineData(7, true)]
        public void SetExternalInput_UpdatesPinInput(int pinNumber, bool inputValue)
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Act
            port.SetExternalInput(pinNumber, inputValue);

            // Assert
            Assert.Equal(inputValue, port.GetInputValue(pinNumber));
        }
    }

    /// <summary>
    /// Tests for register read/write operations.
    /// </summary>
    public class RegisterAccessTests : DigitalIOPortTests
    {
        [Fact]
        public void InputRegister_ReadsCorrectValues()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);
            ushort inputRegisterAddress = 0x0200; // PxIN offset is 0

            // Set some external inputs
            port.SetExternalInput(0, true);
            port.SetExternalInput(2, true);
            port.SetExternalInput(7, true);

            // Act
            ushort inputValue = port.ReadWord(inputRegisterAddress);

            // Assert
            Assert.Equal(0x0085, inputValue); // Bits 0, 2, 7 set = 10000101 = 0x85
        }

        [Fact]
        public void OutputRegister_WriteUpdatesOutputValues()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);
            ushort outputRegisterAddress = 0x0202; // PxOUT offset is 2

            // Act
            bool success = port.WriteWord(outputRegisterAddress, 0x00AA); // 10101010

            // Assert
            Assert.True(success);
            Assert.False(port.GetOutputValue(0)); // Bit 0 = 0
            Assert.True(port.GetOutputValue(1));  // Bit 1 = 1
            Assert.False(port.GetOutputValue(2)); // Bit 2 = 0
            Assert.True(port.GetOutputValue(3));  // Bit 3 = 1
            Assert.False(port.GetOutputValue(4)); // Bit 4 = 0
            Assert.True(port.GetOutputValue(5));  // Bit 5 = 1
            Assert.False(port.GetOutputValue(6)); // Bit 6 = 0
            Assert.True(port.GetOutputValue(7));  // Bit 7 = 1
        }

        [Fact]
        public void DirectionRegister_WriteUpdatesDirections()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);
            ushort directionRegisterAddress = 0x0204; // PxDIR offset is 4

            // Act
            bool success = port.WriteWord(directionRegisterAddress, 0x00F0); // 11110000

            // Assert
            Assert.True(success);
            Assert.Equal(IODirection.Input, port.GetDirection(0));   // Bit 0 = 0
            Assert.Equal(IODirection.Input, port.GetDirection(1));   // Bit 1 = 0
            Assert.Equal(IODirection.Input, port.GetDirection(2));   // Bit 2 = 0
            Assert.Equal(IODirection.Input, port.GetDirection(3));   // Bit 3 = 0
            Assert.Equal(IODirection.Output, port.GetDirection(4));  // Bit 4 = 1
            Assert.Equal(IODirection.Output, port.GetDirection(5));  // Bit 5 = 1
            Assert.Equal(IODirection.Output, port.GetDirection(6));  // Bit 6 = 1
            Assert.Equal(IODirection.Output, port.GetDirection(7));  // Bit 7 = 1
        }

        [Fact]
        public void ResistorEnableRegister_WriteUpdatesPullResistors()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);
            ushort resistorEnableAddress = 0x0206; // PxREN offset is 6
            ushort outputRegisterAddress = 0x0202; // PxOUT offset is 2

            // Set output register to control pull direction
            port.WriteWord(outputRegisterAddress, 0x0055); // 01010101 - alternating pattern

            // Act
            bool success = port.WriteWord(resistorEnableAddress, 0x00FF); // Enable all resistors

            // Assert
            Assert.True(success);
            Assert.Equal(PullResistor.PullUp, port.GetPullResistor(0));   // OUT=1, REN=1 -> Pull-up
            Assert.Equal(PullResistor.PullDown, port.GetPullResistor(1)); // OUT=0, REN=1 -> Pull-down
            Assert.Equal(PullResistor.PullUp, port.GetPullResistor(2));   // OUT=1, REN=1 -> Pull-up
            Assert.Equal(PullResistor.PullDown, port.GetPullResistor(3)); // OUT=0, REN=1 -> Pull-down
        }
    }

    /// <summary>
    /// Tests for input/output operations with different pin configurations.
    /// </summary>
    public class InputOutputOperationTests : DigitalIOPortTests
    {
        [Fact]
        public void InputPin_WithoutExternalSignal_ReadsAsLowWithNoPullResistor()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);
            // Pin 0 is input by default with no pull resistor

            // Act
            bool inputValue = port.GetInputValue(0);

            // Assert
            Assert.False(inputValue);
        }

        [Fact]
        public void InputPin_WithPullUpResistor_ReadsAsHigh()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Configure pin 0 with pull-up resistor
            port.WriteWord(0x0202, 0x0001); // PxOUT bit 0 = 1 (pull-up direction)
            port.WriteWord(0x0206, 0x0001); // PxREN bit 0 = 1 (enable resistor)

            // Act
            bool inputValue = port.GetInputValue(0);

            // Assert
            Assert.True(inputValue);
        }

        [Fact]
        public void InputPin_WithPullDownResistor_ReadsAsLow()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Configure pin 0 with pull-down resistor
            port.WriteWord(0x0202, 0x0000); // PxOUT bit 0 = 0 (pull-down direction)
            port.WriteWord(0x0206, 0x0001); // PxREN bit 0 = 1 (enable resistor)

            // Act
            bool inputValue = port.GetInputValue(0);

            // Assert
            Assert.False(inputValue);
        }

        [Fact]
        public void OutputPin_ReflectsOutputValue()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Configure pin 0 as output
            port.WriteWord(0x0204, 0x0001); // PxDIR bit 0 = 1 (output)
            port.WriteWord(0x0202, 0x0001); // PxOUT bit 0 = 1 (high output)

            // Act
            bool inputValue = port.GetInputValue(0);

            // Assert
            Assert.True(inputValue);
        }

        [Fact]
        public void ExternalInput_OverridesPullResistor()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Configure pin 0 with pull-up resistor
            port.WriteWord(0x0202, 0x0001); // PxOUT bit 0 = 1 (pull-up direction)
            port.WriteWord(0x0206, 0x0001); // PxREN bit 0 = 1 (enable resistor)

            // Act - Apply external low signal
            port.SetExternalInput(0, false);
            bool inputValue = port.GetInputValue(0);

            // Assert
            Assert.False(inputValue); // External signal overrides pull-up
        }
    }

    /// <summary>
    /// Tests for interrupt-on-change functionality.
    /// </summary>
    public class InterruptTests : DigitalIOPortTests
    {
        [Fact]
        public void InterruptEvent_RaisedOnPinChange()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);
            bool interruptRaised = false;
            string? interruptName = null;

            port.InterruptRequested += (sender, args) =>
            {
                interruptRaised = true;
                interruptName = args.InterruptName;
            };

            // Enable interrupt for pin 0
            port.WriteWord(0x0218, 0x0001); // PxIE bit 0 = 1 (enable interrupt)

            // Act
            port.SetExternalInput(0, true); // Change from low to high

            // Assert
            Assert.True(interruptRaised);
            Assert.Equal("P1_P0", interruptName);
        }

        [Fact]
        public void InterruptFlag_SetOnPinChange()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Enable interrupt for pin 0
            port.WriteWord(0x0218, 0x0001); // PxIE bit 0 = 1 (enable interrupt)

            // Act
            port.SetExternalInput(0, true); // Change from low to high

            // Assert
            ushort flagValue = port.ReadWord(0x021C); // PxIFG register
            Assert.Equal(0x0001, flagValue & 0x0001); // Bit 0 should be set
        }

        [Fact]
        public void InterruptVector_UpdatedWithHighestPriorityFlag()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Enable interrupts for multiple pins
            port.WriteWord(0x0218, 0x00FF); // PxIE - enable all interrupts

            // Trigger interrupts on pins 3 and 5
            port.SetExternalInput(3, true);
            port.SetExternalInput(5, true);

            // Act
            ushort vectorValue = port.ReadWord(0x020E); // PxIV register

            // Assert
            Assert.Equal(8, vectorValue); // Pin 3 has higher priority (lower number), vector = (3+1)*2 = 8
        }
    }

    /// <summary>
    /// Tests for reset functionality.
    /// </summary>
    public class ResetTests : DigitalIOPortTests
    {
        [Fact]
        public void Reset_RestoresDefaultState()
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Modify port state
            port.WriteWord(0x0202, 0x00FF); // PxOUT = all high
            port.WriteWord(0x0204, 0x00FF); // PxDIR = all outputs
            port.WriteWord(0x0206, 0x00FF); // PxREN = all resistors enabled
            port.SetExternalInput(0, true);

            // Act
            port.Reset();

            // Assert
            Assert.Equal(0x0000, port.ReadWord(0x0202)); // PxOUT reset to 0
            Assert.Equal(0x0000, port.ReadWord(0x0204)); // PxDIR reset to 0 (all inputs)
            Assert.Equal(0x0000, port.ReadWord(0x0206)); // PxREN reset to 0 (no resistors)

            // All pins should be inputs with no pull resistors
            for (int i = 0; i < 8; i++)
            {
                Assert.Equal(IODirection.Input, port.GetDirection(i));
                Assert.Equal(PullResistor.None, port.GetPullResistor(i));
            }
        }
    }

    /// <summary>
    /// Tests for address handling and validation.
    /// </summary>
    public class AddressHandlingTests : DigitalIOPortTests
    {
        [Theory]
        [InlineData(0x0200)] // PxIN
        [InlineData(0x0202)] // PxOUT
        [InlineData(0x0204)] // PxDIR
        [InlineData(0x0206)] // PxREN
        [InlineData(0x020A)] // PxSEL0
        [InlineData(0x020C)] // PxSEL1
        [InlineData(0x0218)] // PxIE
        [InlineData(0x021A)] // PxIES
        [InlineData(0x021C)] // PxIFG
        [InlineData(0x020E)] // PxIV
        public void HandlesAddress_WithValidRegisterAddress_ReturnsTrue(ushort address)
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Act
            bool handles = port.HandlesAddress(address);

            // Assert
            Assert.True(handles);
        }

        [Theory]
        [InlineData(0x01FF)] // Just before range
        [InlineData(0x0220)] // Just after range
        [InlineData(0x0000)] // Far from range
        public void HandlesAddress_WithInvalidAddress_ReturnsFalse(ushort address)
        {
            // Arrange
            var port = new DigitalIOPort("P1", 0x0200, 0xFFE4, _logger);

            // Act
            bool handles = port.HandlesAddress(address);

            // Assert
            Assert.False(handles);
        }
    }
}
