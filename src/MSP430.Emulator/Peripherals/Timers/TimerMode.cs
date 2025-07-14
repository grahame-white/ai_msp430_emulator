namespace MSP430.Emulator.Peripherals.Timers;

/// <summary>
/// Defines the timer counting modes for MSP430 Timer_A.
/// 
/// Timer_A supports four operating modes that control how the timer counter operates:
/// - Stop mode: Timer is halted
/// - Up mode: Counts from zero up to TAxCCR0
/// - Continuous mode: Counts from zero to 0xFFFF and rolls over
/// - Up/down mode: Counts up to TAxCCR0 then down to zero
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.3:
/// "Timer Mode Control"
/// </summary>
public enum TimerMode : byte
{
    /// <summary>
    /// Stop mode. Timer is halted and the counter is stopped.
    /// TAxMC = 00b in TAxCTL register.
    /// </summary>
    Stop = 0x00,

    /// <summary>
    /// Up mode. Timer repeatedly counts from zero up to the value of TAxCCR0.
    /// TAxMC = 01b in TAxCTL register.
    /// </summary>
    Up = 0x01,

    /// <summary>
    /// Continuous mode. Timer repeatedly counts from zero to 0xFFFF and restarts from zero.
    /// TAxMC = 10b in TAxCTL register.
    /// </summary>
    Continuous = 0x02,

    /// <summary>
    /// Up/down mode. Timer counts from zero up to TAxCCR0, then back down to zero.
    /// TAxMC = 11b in TAxCTL register.
    /// </summary>
    UpDown = 0x03
}
