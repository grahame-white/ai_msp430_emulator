using System;

namespace MSP430.Emulator.Peripherals.DigitalIO;

/// <summary>
/// Represents the state of an individual digital I/O pin.
/// 
/// Manages pin configuration including direction, output value, pull resistors,
/// and external input state. Provides MSP430FR2355-compliant pin behavior
/// including proper input/output isolation and pull resistor simulation.
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 8.2:
/// "Digital I/O Operation"
/// </summary>
public class PinState
{
    private IODirection _direction;
    private bool _outputValue;
    private PullResistor _pullResistor;
    private bool _externalInput;
    private bool _previousInputValue;

    /// <summary>
    /// Initializes a new instance of the PinState class with default MSP430 reset values.
    /// 
    /// After reset, pins are configured as inputs with no pull resistors enabled.
    /// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 8.3.1:
    /// "Configuration After Reset"
    /// </summary>
    /// <param name="pinNumber">The pin number (0-7 for each port).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pinNumber is not between 0 and 7.</exception>
    public PinState(int pinNumber)
    {
        if (pinNumber < 0 || pinNumber > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(pinNumber), "Pin number must be between 0 and 7");
        }

        PinNumber = pinNumber;
        _direction = IODirection.Input;
        _outputValue = false;
        _pullResistor = PullResistor.None;
        _externalInput = false;
        _previousInputValue = false;
    }

    /// <summary>
    /// Gets the pin number (0-7).
    /// </summary>
    public int PinNumber { get; }

    /// <summary>
    /// Gets or sets the direction of this pin.
    /// </summary>
    public IODirection Direction
    {
        get => _direction;
        set => _direction = value;
    }

    /// <summary>
    /// Gets or sets the output value for this pin.
    /// Only affects the pin output when Direction is Output.
    /// Also controls pull resistor direction when PullResistor is enabled.
    /// </summary>
    public bool OutputValue
    {
        get => _outputValue;
        set => _outputValue = value;
    }

    /// <summary>
    /// Gets or sets the pull resistor configuration for this pin.
    /// </summary>
    public PullResistor PullResistor
    {
        get => _pullResistor;
        set => _pullResistor = value;
    }

    /// <summary>
    /// Gets or sets the external input value applied to this pin.
    /// This simulates an external signal driving the pin.
    /// </summary>
    public bool ExternalInput
    {
        get => _externalInput;
        set => _externalInput = value;
    }

    /// <summary>
    /// Gets the effective input value that would be read by the PxIN register.
    /// Takes into account pin direction, external input, and pull resistor configuration.
    /// </summary>
    public bool InputValue
    {
        get
        {
            // When configured as output, reading returns the output value
            if (_direction == IODirection.Output)
            {
                return _outputValue;
            }

            // When configured as input, check if external signal is applied
            return HasExternalSignal
                ? _externalInput
                : _pullResistor switch
                {
                    PullResistor.PullUp => true,
                    PullResistor.PullDown => false,
                    PullResistor.None => false, // Floating input reads as 0
                    _ => false
                };
        }
    }

    /// <summary>
    /// Gets the previous input value for interrupt-on-change detection.
    /// </summary>
    public bool PreviousInputValue => _previousInputValue;

    /// <summary>
    /// Gets a value indicating whether this pin has changed state since the last update.
    /// Used for interrupt-on-change functionality.
    /// </summary>
    public bool HasChanged => InputValue != _previousInputValue;

    /// <summary>
    /// Gets a value indicating whether an external signal is being applied to this pin.
    /// This is a simulation feature to model external circuit connections.
    /// </summary>
    public bool HasExternalSignal { get; set; }

    /// <summary>
    /// Updates the pin state and captures the previous value for change detection.
    /// This should be called after any register write operations that might affect pin behavior.
    /// </summary>
    public void UpdateState()
    {
        _previousInputValue = InputValue;
    }

    /// <summary>
    /// Resets the pin to its power-on reset state.
    /// </summary>
    public void Reset()
    {
        _direction = IODirection.Input;
        _outputValue = false;
        _pullResistor = PullResistor.None;
        _externalInput = false;
        _previousInputValue = false;
        HasExternalSignal = false;
    }

    /// <summary>
    /// Returns a string representation of the pin state.
    /// </summary>
    /// <returns>A string describing the current pin configuration and state.</returns>
    public override string ToString()
    {
        return $"Pin{PinNumber}: {_direction}, Out={_outputValue}, In={InputValue}, Pull={_pullResistor}";
    }
}
