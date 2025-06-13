namespace MSP430.Emulator.Peripherals.DigitalIO;

/// <summary>
/// Defines the direction of a digital I/O pin.
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 8.2.3:
/// "Direction Registers (PxDIR) - Each bit in these registers configures 
/// the corresponding I/O pin as an input or output, regardless of the 
/// selected function for the pin."
/// </summary>
public enum IODirection : byte
{
    /// <summary>
    /// Pin is configured as an input (PxDIR bit = 0).
    /// The pin can read external signals but cannot drive output values.
    /// </summary>
    Input = 0,

    /// <summary>
    /// Pin is configured as an output (PxDIR bit = 1).
    /// The pin drives the value from the corresponding PxOUT bit.
    /// </summary>
    Output = 1
}
