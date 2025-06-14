namespace MSP430.Emulator.Peripherals.DigitalIO;

/// <summary>
/// Defines the pull resistor configuration for a digital I/O pin.
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 8.2.4:
/// "Pullup or Pulldown Resistors (PxREN) - Each bit in these registers 
/// enables or disables the pullup or pulldown resistor of the corresponding 
/// I/O pin. The corresponding bit in the PxOUT register selects if the pin 
/// is pulled up or pulled down."
/// </summary>
public enum PullResistor : byte
{
    /// <summary>
    /// No pull resistor is enabled (PxREN bit = 0).
    /// The pin floats when configured as input.
    /// </summary>
    None = 0,

    /// <summary>
    /// Pull-down resistor is enabled (PxREN bit = 1, PxOUT bit = 0).
    /// The pin is pulled to ground (logic 0) when no external signal is applied.
    /// </summary>
    PullDown = 1,

    /// <summary>
    /// Pull-up resistor is enabled (PxREN bit = 1, PxOUT bit = 1).
    /// The pin is pulled to VCC (logic 1) when no external signal is applied.
    /// </summary>
    PullUp = 2
}
