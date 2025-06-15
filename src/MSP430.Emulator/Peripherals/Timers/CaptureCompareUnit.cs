using System;

namespace MSP430.Emulator.Peripherals.Timers;

/// <summary>
/// Defines the capture/compare modes for Timer_A capture/compare units.
/// 
/// Each capture/compare unit can operate in capture mode (to capture timer values
/// on input events) or compare mode (to generate output signals and interrupts
/// when the timer reaches the compare value).
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.4:
/// "Capture/Compare Blocks"
/// </summary>
public enum CaptureCompareMode : byte
{
    /// <summary>
    /// Compare mode. The unit generates interrupts and output signals when
    /// the timer count equals the TAxCCRn value.
    /// CAP = 0 in TAxCCTLn register.
    /// </summary>
    Compare = 0x00,

    /// <summary>
    /// Capture mode. The unit captures the current timer value into TAxCCRn
    /// when an input event occurs.
    /// CAP = 1 in TAxCCTLn register.
    /// </summary>
    Capture = 0x01
}

/// <summary>
/// Defines the output modes for Timer_A capture/compare units in compare mode.
/// 
/// The output mode controls how the output signal (TAxOUTn) behaves when
/// compare events occur.
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.5:
/// "Output Unit"
/// </summary>
public enum OutputMode : byte
{
    /// <summary>
    /// Output mode 0. Output signal (TAxOUTn) is defined by the TAxOUTn bit.
    /// OUTMOD = 000b in TAxCCTLn register.
    /// </summary>
    Output = 0x00,

    /// <summary>
    /// Output mode 1. Set output on compare, reset on period.
    /// OUTMOD = 001b in TAxCCTLn register.
    /// </summary>
    Set = 0x01,

    /// <summary>
    /// Output mode 2. Toggle/reset output on compare, set on period.
    /// OUTMOD = 010b in TAxCCTLn register.
    /// </summary>
    ToggleReset = 0x02,

    /// <summary>
    /// Output mode 3. Set/reset output on compare.
    /// OUTMOD = 011b in TAxCCTLn register.
    /// </summary>
    SetReset = 0x03,

    /// <summary>
    /// Output mode 4. Toggle output on compare.
    /// OUTMOD = 100b in TAxCCTLn register.
    /// </summary>
    Toggle = 0x04,

    /// <summary>
    /// Output mode 5. Reset output on compare, set on period.
    /// OUTMOD = 101b in TAxCCTLn register.
    /// </summary>
    Reset = 0x05,

    /// <summary>
    /// Output mode 6. Toggle/set output on compare, reset on period.
    /// OUTMOD = 110b in TAxCCTLn register.
    /// </summary>
    ToggleSet = 0x06,

    /// <summary>
    /// Output mode 7. Reset/set output on compare.
    /// OUTMOD = 111b in TAxCCTLn register.
    /// </summary>
    ResetSet = 0x07
}

/// <summary>
/// Defines the capture input select for Timer_A capture/compare units in capture mode.
/// 
/// The capture input select determines which input signal triggers a capture event.
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.4:
/// "Capture/Compare Blocks"
/// </summary>
public enum CaptureInputSelect : byte
{
    /// <summary>
    /// CCIxA input select.
    /// CCIS = 00b in TAxCCTLn register.
    /// </summary>
    CCIxA = 0x00,

    /// <summary>
    /// CCIxB input select.
    /// CCIS = 01b in TAxCCTLn register.
    /// </summary>
    CCIxB = 0x01,

    /// <summary>
    /// GND (always low).
    /// CCIS = 10b in TAxCCTLn register.
    /// </summary>
    GND = 0x02,

    /// <summary>
    /// VCC (always high).
    /// CCIS = 11b in TAxCCTLn register.
    /// </summary>
    VCC = 0x03
}

/// <summary>
/// Implements a Timer_A capture/compare unit.
/// 
/// Each Timer_A has multiple capture/compare units (TAxCCRn) that can operate
/// in either capture mode or compare mode. In capture mode, they capture the
/// timer value when input events occur. In compare mode, they generate interrupts
/// and control output signals when the timer reaches the compare value.
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.4:
/// "Capture/Compare Blocks" and Section 13.2.5: "Output Unit"
/// </summary>
public class CaptureCompareUnit
{
    private ushort _captureCompareValue;
    private CaptureCompareMode _mode;
    private OutputMode _outputMode;
    private CaptureInputSelect _captureInput;
    private bool _outputValue;
    private bool _interruptEnable;
    private bool _interruptFlag;

    /// <summary>
    /// Initializes a new instance of the CaptureCompareUnit class.
    /// </summary>
    /// <param name="unitNumber">The unit number (0-6, where 0 is typically the period register).</param>
    public CaptureCompareUnit(int unitNumber)
    {
        UnitNumber = unitNumber;
        Reset();
    }

    /// <summary>
    /// Gets the unit number.
    /// </summary>
    public int UnitNumber { get; }

    /// <summary>
    /// Gets or sets the capture/compare value (TAxCCRn).
    /// </summary>
    public ushort CaptureCompareValue
    {
        get => _captureCompareValue;
        set => _captureCompareValue = value;
    }

    /// <summary>
    /// Gets or sets the capture/compare mode.
    /// </summary>
    public CaptureCompareMode Mode
    {
        get => _mode;
        set => _mode = value;
    }

    /// <summary>
    /// Gets or sets the output mode (used in compare mode).
    /// </summary>
    public OutputMode OutputMode
    {
        get => _outputMode;
        set => _outputMode = value;
    }

    /// <summary>
    /// Gets or sets the capture input select (used in capture mode).
    /// </summary>
    public CaptureInputSelect CaptureInput
    {
        get => _captureInput;
        set => _captureInput = value;
    }

    /// <summary>
    /// Gets or sets the current output value.
    /// </summary>
    public bool OutputValue
    {
        get => _outputValue;
        set => _outputValue = value;
    }

    /// <summary>
    /// Gets or sets whether interrupts are enabled for this unit.
    /// </summary>
    public bool InterruptEnable
    {
        get => _interruptEnable;
        set => _interruptEnable = value;
    }

    /// <summary>
    /// Gets or sets the interrupt flag state.
    /// </summary>
    public bool InterruptFlag
    {
        get => _interruptFlag;
        set => _interruptFlag = value;
    }

    /// <summary>
    /// Event raised when an interrupt condition occurs.
    /// </summary>
    public event EventHandler<CaptureCompareInterruptEventArgs>? InterruptRequested;

    /// <summary>
    /// Handles a timer count update in compare mode.
    /// </summary>
    /// <param name="timerValue">The current timer value.</param>
    /// <param name="isRollover">True if this is a timer rollover event.</param>
    public void HandleCompareEvent(ushort timerValue, bool isRollover = false)
    {
        if (_mode != CaptureCompareMode.Compare)
        {
            return;
        }

        bool compareMatch = timerValue == _captureCompareValue;

        if (compareMatch || isRollover)
        {
            UpdateOutput(compareMatch, isRollover);

            if (compareMatch)
            {
                // Set interrupt flag
                _interruptFlag = true;

                // Raise interrupt if enabled
                if (_interruptEnable)
                {
                    InterruptRequested?.Invoke(this, new CaptureCompareInterruptEventArgs(UnitNumber));
                }
            }
        }
    }

    /// <summary>
    /// Handles a capture event when an input signal edge is detected.
    /// </summary>
    /// <param name="timerValue">The current timer value to capture.</param>
    public void HandleCaptureEvent(ushort timerValue)
    {
        if (_mode != CaptureCompareMode.Capture)
        {
            return;
        }

        // Capture the timer value
        _captureCompareValue = timerValue;

        // Set interrupt flag
        _interruptFlag = true;

        // Raise interrupt if enabled
        if (_interruptEnable)
        {
            InterruptRequested?.Invoke(this, new CaptureCompareInterruptEventArgs(UnitNumber));
        }
    }

    /// <summary>
    /// Clears the interrupt flag.
    /// </summary>
    public void ClearInterruptFlag()
    {
        _interruptFlag = false;
    }

    /// <summary>
    /// Resets the capture/compare unit to its default state.
    /// </summary>
    public void Reset()
    {
        _captureCompareValue = 0x0000;
        _mode = CaptureCompareMode.Compare;
        _outputMode = OutputMode.Output;
        _captureInput = CaptureInputSelect.CCIxA;
        _outputValue = false;
        _interruptEnable = false;
        _interruptFlag = false;
    }

    /// <summary>
    /// Updates the output signal based on the output mode and event type.
    /// </summary>
    /// <param name="compareMatch">True if this is a compare match event.</param>
    /// <param name="isRollover">True if this is a timer rollover event.</param>
    private void UpdateOutput(bool compareMatch, bool isRollover)
    {
        switch (_outputMode)
        {
            case OutputMode.Output:
                // Output is controlled by the OUT bit
                break;

            case OutputMode.Set:
                if (compareMatch)
                {
                    _outputValue = true;
                }

                if (isRollover)
                {
                    _outputValue = false;
                }

                break;

            case OutputMode.ToggleReset:
                if (compareMatch)
                {
                    _outputValue = !_outputValue;
                }

                if (isRollover)
                {
                    _outputValue = false;
                }

                break;

            case OutputMode.SetReset:
                if (compareMatch)
                {
                    _outputValue = true;
                }
                // Reset happens at period (handled by unit 0)
                break;

            case OutputMode.Toggle:
                if (compareMatch)
                {
                    _outputValue = !_outputValue;
                }

                break;

            case OutputMode.Reset:
                if (compareMatch)
                {
                    _outputValue = false;
                }

                if (isRollover)
                {
                    _outputValue = true;
                }

                break;

            case OutputMode.ToggleSet:
                if (compareMatch)
                {
                    _outputValue = !_outputValue;
                }

                if (isRollover)
                {
                    _outputValue = true;
                }

                break;

            case OutputMode.ResetSet:
                if (compareMatch)
                {
                    _outputValue = false;
                }
                // Set happens at period (handled by unit 0)
                break;
        }
    }
}

/// <summary>
/// Event arguments for capture/compare interrupt events.
/// </summary>
public class CaptureCompareInterruptEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the CaptureCompareInterruptEventArgs class.
    /// </summary>
    /// <param name="unitNumber">The capture/compare unit number that generated the interrupt.</param>
    public CaptureCompareInterruptEventArgs(int unitNumber)
    {
        UnitNumber = unitNumber;
    }

    /// <summary>
    /// Gets the capture/compare unit number that generated the interrupt.
    /// </summary>
    public int UnitNumber { get; }
}
