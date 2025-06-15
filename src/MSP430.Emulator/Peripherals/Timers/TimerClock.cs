using System;

namespace MSP430.Emulator.Peripherals.Timers;

/// <summary>
/// Defines the clock sources available for MSP430 Timer_A.
/// 
/// Timer_A can be clocked from multiple sources with optional input dividers:
/// - TAxCLK: External clock source
/// - ACLK: Auxiliary clock (typically 32.768 kHz)
/// - SMCLK: Sub-main clock (DCO-derived)
/// - INCLK: Inverted external clock
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.1:
/// "16-Bit Timer Counter"
/// </summary>
public enum TimerClockSource : byte
{
    /// <summary>
    /// External TAxCLK clock source.
    /// TAxSSEL = 00b in TAxCTL register.
    /// </summary>
    TAxCLK = 0x00,

    /// <summary>
    /// Auxiliary clock (ACLK).
    /// TAxSSEL = 01b in TAxCTL register.
    /// </summary>
    ACLK = 0x01,

    /// <summary>
    /// Sub-main clock (SMCLK).
    /// TAxSSEL = 10b in TAxCTL register.
    /// </summary>
    SMCLK = 0x02,

    /// <summary>
    /// Inverted TAxCLK clock source.
    /// TAxSSEL = 11b in TAxCTL register.
    /// </summary>
    INCLK = 0x03
}

/// <summary>
/// Defines the input divider values for Timer_A clock sources.
/// 
/// The input divider can divide the clock source by 1, 2, 4, or 8.
/// This is controlled by the TAxID field in the TAxCTL register.
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.1:
/// "16-Bit Timer Counter"
/// </summary>
public enum TimerInputDivider : byte
{
    /// <summary>
    /// Divide by 1 (no division).
    /// TAxID = 00b in TAxCTL register.
    /// </summary>
    DivideBy1 = 0x00,

    /// <summary>
    /// Divide by 2.
    /// TAxID = 01b in TAxCTL register.
    /// </summary>
    DivideBy2 = 0x01,

    /// <summary>
    /// Divide by 4.
    /// TAxID = 10b in TAxCTL register.
    /// </summary>
    DivideBy4 = 0x02,

    /// <summary>
    /// Divide by 8.
    /// TAxID = 11b in TAxCTL register.
    /// </summary>
    DivideBy8 = 0x03
}

/// <summary>
/// Manages clock generation and timing for MSP430 Timer_A peripherals.
/// 
/// Provides clock source selection, input division, and frequency calculation
/// for timer operations. Handles the timing characteristics required for
/// accurate timer counting, capture/compare operations, and PWM generation.
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.1:
/// "16-Bit Timer Counter"
/// </summary>
public class TimerClock
{
    private TimerClockSource _clockSource;
    private TimerInputDivider _inputDivider;
    private uint _sourceFrequency;

    /// <summary>
    /// Initializes a new instance of the TimerClock class.
    /// </summary>
    /// <param name="clockSource">The initial clock source.</param>
    /// <param name="inputDivider">The initial input divider.</param>
    /// <param name="sourceFrequency">The frequency of the clock source in Hz.</param>
    public TimerClock(TimerClockSource clockSource = TimerClockSource.SMCLK,
                     TimerInputDivider inputDivider = TimerInputDivider.DivideBy1,
                     uint sourceFrequency = 1000000)
    {
        _clockSource = clockSource;
        _inputDivider = inputDivider;
        _sourceFrequency = sourceFrequency;
    }

    /// <summary>
    /// Gets or sets the current clock source.
    /// </summary>
    public TimerClockSource ClockSource
    {
        get => _clockSource;
        set => _clockSource = value;
    }

    /// <summary>
    /// Gets or sets the current input divider.
    /// </summary>
    public TimerInputDivider InputDivider
    {
        get => _inputDivider;
        set => _inputDivider = value;
    }

    /// <summary>
    /// Gets or sets the source frequency in Hz.
    /// This represents the frequency before any input division.
    /// </summary>
    public uint SourceFrequency
    {
        get => _sourceFrequency;
        set => _sourceFrequency = value;
    }

    /// <summary>
    /// Gets the effective timer frequency after input division.
    /// </summary>
    public uint EffectiveFrequency => _sourceFrequency / GetDividerValue();

    /// <summary>
    /// Gets the period of one timer clock cycle in microseconds.
    /// </summary>
    public double PeriodMicroseconds => EffectiveFrequency > 0 ? 1_000_000.0 / EffectiveFrequency : 0.0;

    /// <summary>
    /// Calculates the time in microseconds for a given number of timer counts.
    /// </summary>
    /// <param name="counts">The number of timer counts.</param>
    /// <returns>The time in microseconds.</returns>
    public double CountsToMicroseconds(uint counts)
    {
        return counts * PeriodMicroseconds;
    }

    /// <summary>
    /// Calculates the number of timer counts for a given time in microseconds.
    /// </summary>
    /// <param name="microseconds">The time in microseconds.</param>
    /// <returns>The number of timer counts.</returns>
    public uint MicrosecondsToTimerCounts(double microseconds)
    {
        return microseconds > 0 ? (uint)(microseconds / PeriodMicroseconds) : 0;
    }

    /// <summary>
    /// Gets the numeric divider value for the current input divider setting.
    /// </summary>
    /// <returns>The divider value (1, 2, 4, or 8).</returns>
    private uint GetDividerValue()
    {
        return _inputDivider switch
        {
            TimerInputDivider.DivideBy1 => 1,
            TimerInputDivider.DivideBy2 => 2,
            TimerInputDivider.DivideBy4 => 4,
            TimerInputDivider.DivideBy8 => 8,
            _ => 1
        };
    }

    /// <summary>
    /// Resets the clock configuration to default values.
    /// </summary>
    public void Reset()
    {
        _clockSource = TimerClockSource.SMCLK;
        _inputDivider = TimerInputDivider.DivideBy1;
        _sourceFrequency = 1000000; // Default 1 MHz SMCLK
    }
}
