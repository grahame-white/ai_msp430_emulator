using System;
using System.Linq;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Peripherals.DigitalIO;

/// <summary>
/// Implements a digital I/O port peripheral for the MSP430FR2355 emulator.
/// 
/// Provides full MSP430-compliant digital I/O functionality including:
/// - Pin direction control (input/output)
/// - Output value control and input reading
/// - Pull-up/pull-down resistor simulation
/// - Function select for GPIO vs peripheral modes
/// - Interrupt-on-change functionality
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 8:
/// "Digital I/O"
/// </summary>
public class DigitalIOPort : PeripheralBase
{
    private readonly PinState[] _pins;
    private readonly string _portName;
    private readonly ushort _interruptVector;

    // MSP430 Digital I/O Register Offsets
    private const ushort OffsetPxIN = 0x00;    // Input Register
    private const ushort OffsetPxOUT = 0x02;   // Output Register  
    private const ushort OffsetPxDIR = 0x04;   // Direction Register
    private const ushort OffsetPxREN = 0x06;   // Resistor Enable Register
    private const ushort OffsetPxSEL0 = 0x0A;  // Function Select Register 0
    private const ushort OffsetPxSEL1 = 0x0C;  // Function Select Register 1
    private const ushort OffsetPxIE = 0x18;    // Interrupt Enable Register
    private const ushort OffsetPxIES = 0x1A;   // Interrupt Edge Select Register
    private const ushort OffsetPxIFG = 0x1C;   // Interrupt Flag Register
    private const ushort OffsetPxIV = 0x0E;    // Interrupt Vector Register

    /// <summary>
    /// Initializes a new instance of the DigitalIOPort class.
    /// </summary>
    /// <param name="portName">The port name (e.g., "P1", "P2").</param>
    /// <param name="baseAddress">The base address of the port registers.</param>
    /// <param name="interruptVector">The interrupt vector address for this port.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when portName is null.</exception>
    public DigitalIOPort(string portName, ushort baseAddress, ushort interruptVector, ILogger? logger = null)
        : base($"GPIO_{portName}", baseAddress, 0x20, logger)
    {
        _portName = portName ?? throw new ArgumentNullException(nameof(portName));
        _interruptVector = interruptVector;
        _pins = new PinState[8];

        // Initialize all pins
        for (int i = 0; i < 8; i++)
        {
            _pins[i] = new PinState(i);
        }

        InitializeRegisters();
    }

    /// <summary>
    /// Gets the port name (e.g., "P1", "P2").
    /// </summary>
    public string PortName => _portName;

    /// <summary>
    /// Gets the pin state for the specified pin number.
    /// </summary>
    /// <param name="pinNumber">The pin number (0-7).</param>
    /// <returns>The pin state object.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pinNumber is not between 0 and 7.</exception>
    public PinState GetPin(int pinNumber)
    {
        if (pinNumber < 0 || pinNumber > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(pinNumber), "Pin number must be between 0 and 7");
        }

        return _pins[pinNumber];
    }

    /// <summary>
    /// Sets the external input value for a specific pin.
    /// This simulates an external signal being applied to the pin.
    /// </summary>
    /// <param name="pinNumber">The pin number (0-7).</param>
    /// <param name="value">The external input value.</param>
    /// <param name="hasExternalSignal">Whether an external signal is being applied.</param>
    public void SetExternalInput(int pinNumber, bool value, bool hasExternalSignal = true)
    {
        PinState pin = GetPin(pinNumber);
        bool previousValue = pin.InputValue;

        pin.ExternalInput = value;
        pin.HasExternalSignal = hasExternalSignal;

        UpdateInputRegister();
        CheckForInterrupt(pinNumber, previousValue, pin.InputValue);
    }

    /// <summary>
    /// Gets the current input value for a specific pin.
    /// </summary>
    /// <param name="pinNumber">The pin number (0-7).</param>
    /// <returns>The current input value.</returns>
    public bool GetInputValue(int pinNumber)
    {
        return GetPin(pinNumber).InputValue;
    }

    /// <summary>
    /// Gets the current output value for a specific pin.
    /// </summary>
    /// <param name="pinNumber">The pin number (0-7).</param>
    /// <returns>The current output value.</returns>
    public bool GetOutputValue(int pinNumber)
    {
        return GetPin(pinNumber).OutputValue;
    }

    /// <summary>
    /// Gets the current direction for a specific pin.
    /// </summary>
    /// <param name="pinNumber">The pin number (0-7).</param>
    /// <returns>The current pin direction.</returns>
    public IODirection GetDirection(int pinNumber)
    {
        return GetPin(pinNumber).Direction;
    }

    /// <summary>
    /// Gets the current pull resistor configuration for a specific pin.
    /// </summary>
    /// <param name="pinNumber">The pin number (0-7).</param>
    /// <returns>The current pull resistor configuration.</returns>
    public PullResistor GetPullResistor(int pinNumber)
    {
        return GetPin(pinNumber).PullResistor;
    }

    private void InitializeRegisters()
    {
        // PxIN - Input Register (read-only)
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetPxIN),
            0x0000,
            PeripheralRegisterAccess.Read,
            name: $"{_portName}IN"));

        // PxOUT - Output Register  
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetPxOUT),
            0x0000,
            PeripheralRegisterAccess.ReadWrite,
            name: $"{_portName}OUT"));

        // PxDIR - Direction Register
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetPxDIR),
            0x0000,
            PeripheralRegisterAccess.ReadWrite,
            name: $"{_portName}DIR"));

        // PxREN - Resistor Enable Register
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetPxREN),
            0x0000,
            PeripheralRegisterAccess.ReadWrite,
            name: $"{_portName}REN"));

        // PxSEL0 - Function Select Register 0
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetPxSEL0),
            0x0000,
            PeripheralRegisterAccess.ReadWrite,
            name: $"{_portName}SEL0"));

        // PxSEL1 - Function Select Register 1
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetPxSEL1),
            0x0000,
            PeripheralRegisterAccess.ReadWrite,
            name: $"{_portName}SEL1"));

        // PxIE - Interrupt Enable Register
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetPxIE),
            0x0000,
            PeripheralRegisterAccess.ReadWrite,
            name: $"{_portName}IE"));

        // PxIES - Interrupt Edge Select Register
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetPxIES),
            0x0000,
            PeripheralRegisterAccess.ReadWrite,
            name: $"{_portName}IES"));

        // PxIFG - Interrupt Flag Register
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetPxIFG),
            0x0000,
            PeripheralRegisterAccess.ReadWrite,
            name: $"{_portName}IFG"));

        // PxIV - Interrupt Vector Register (read-only)
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetPxIV),
            0x0000,
            PeripheralRegisterAccess.Read,
            name: $"{_portName}IV"));
    }

    protected override void OnRegisterChanged(PeripheralRegister register, ushort address)
    {
        base.OnRegisterChanged(register, address);

        ushort offset = (ushort)(address - BaseAddress);

        switch (offset)
        {
            case OffsetPxOUT:
                UpdateOutputValues(register.Value);
                UpdateInputRegister(); // Output affects input when direction is output
                break;

            case OffsetPxDIR:
                UpdateDirections(register.Value);
                UpdateInputRegister(); // Direction change affects input reading
                break;

            case OffsetPxREN:
                UpdatePullResistors();
                UpdateInputRegister(); // Pull resistor change affects input
                break;

            case OffsetPxIFG:
                // Writing to interrupt flag register clears flags
                break;
        }
    }

    private void UpdateOutputValues(ushort outValue)
    {
        for (int i = 0; i < 8; i++)
        {
            bool previousInput = _pins[i].InputValue;
            _pins[i].OutputValue = (outValue & (1 << i)) != 0;

            // Check for interrupt if this is an output pin
            if (_pins[i].Direction == IODirection.Output)
            {
                CheckForInterrupt(i, previousInput, _pins[i].InputValue);
            }
        }

        UpdatePullResistors(); // Output value affects pull resistor direction
    }

    private void UpdateDirections(ushort dirValue)
    {
        for (int i = 0; i < 8; i++)
        {
            bool previousInput = _pins[i].InputValue;
            _pins[i].Direction = (dirValue & (1 << i)) != 0 ? IODirection.Output : IODirection.Input;
            CheckForInterrupt(i, previousInput, _pins[i].InputValue);
        }
    }

    private void UpdatePullResistors()
    {
        PeripheralRegister? renReg = GetRegister((ushort)(BaseAddress + OffsetPxREN));
        PeripheralRegister? outReg = GetRegister((ushort)(BaseAddress + OffsetPxOUT));

        if (renReg == null || outReg == null)
        {
            return;
        }

        ushort renValue = renReg.Value;
        ushort outValue = outReg.Value;

        for (int i = 0; i < 8; i++)
        {
            bool previousInput = _pins[i].InputValue;

            // Pull resistor configuration based on REN and OUT bits
            bool resistorEnabled = (renValue & (1 << i)) != 0;
            bool pullUp = (outValue & (1 << i)) != 0;
            PullResistor resistorType = pullUp ? PullResistor.PullUp : PullResistor.PullDown;
            _pins[i].PullResistor = resistorEnabled ? resistorType : PullResistor.None;

            CheckForInterrupt(i, previousInput, _pins[i].InputValue);
        }
    }

    private void UpdateInputRegister()
    {
        PeripheralRegister? inReg = GetRegister((ushort)(BaseAddress + OffsetPxIN));
        if (inReg == null)
        {
            return;
        }

        ushort inputValue = 0;
        for (int i = 0; i < 8; i++)
        {
            inputValue |= _pins[i].InputValue ? (ushort)(1 << i) : (ushort)0;
        }

        inReg.SetValue(inputValue);
    }

    private void CheckForInterrupt(int pinNumber, bool previousValue, bool currentValue)
    {
        if (previousValue == currentValue)
        {
            return;
        }

        PeripheralRegister? ieReg = GetRegister((ushort)(BaseAddress + OffsetPxIE));
        PeripheralRegister? iesReg = GetRegister((ushort)(BaseAddress + OffsetPxIES));
        PeripheralRegister? ifgReg = GetRegister((ushort)(BaseAddress + OffsetPxIFG));

        if (ieReg == null || iesReg == null || ifgReg == null)
        {
            return;
        }

        // Check if interrupt is enabled for this pin
        if ((ieReg.Value & (1 << pinNumber)) == 0)
        {
            return;
        }

        // Check edge selection
        bool risingEdge = !previousValue && currentValue;
        bool fallingEdge = previousValue && !currentValue;
        bool iesBit = (iesReg.Value & (1 << pinNumber)) != 0;

        bool shouldTrigger = iesBit ? fallingEdge : risingEdge;

        if (shouldTrigger)
        {
            // Set interrupt flag
            ushort newIfgValue = (ushort)(ifgReg.Value | (1 << pinNumber));
            ifgReg.SetValue(newIfgValue);

            // Update interrupt vector register
            UpdateInterruptVector();

            // Raise interrupt event
            OnInterruptRequested(_interruptVector, $"{_portName}_P{pinNumber}", 0);
        }
    }

    private void UpdateInterruptVector()
    {
        PeripheralRegister? ifgReg = GetRegister((ushort)(BaseAddress + OffsetPxIFG));
        PeripheralRegister? ivReg = GetRegister((ushort)(BaseAddress + OffsetPxIV));

        if (ifgReg == null || ivReg == null)
        {
            return;
        }

        ushort ifgValue = ifgReg.Value;
        ushort vectorValue = 0;

        // Find highest priority interrupt (lowest pin number)
        for (int i = 0; i < 8; i++)
        {
            if ((ifgValue & (1 << i)) != 0)
            {
                vectorValue = (ushort)((i + 1) * 2);
                break;
            }
        }

        ivReg.SetValue(vectorValue);
    }

    public override void Reset()
    {
        base.Reset();

        // Reset all pins to default state
        for (int i = 0; i < 8; i++)
        {
            _pins[i].Reset();
        }

        // Update input register after reset
        UpdateInputRegister();
    }

    public override void Initialize(object? initializationData = null)
    {
        Reset();
        Logger?.Info($"Digital I/O Port {_portName} initialized at base address 0x{BaseAddress:X4}");
    }
}
