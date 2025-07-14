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
/// Defines the capture edge selection for Timer_A capture/compare units in capture mode.
/// 
/// The capture mode selects which edge of the input signal triggers a capture event.
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.4.1:
/// "Capture Mode"
/// </summary>
public enum CaptureEdgeMode : byte
{
    /// <summary>
    /// No capture.
    /// CM = 00b in TAxCCTLn register.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Capture on rising edge.
    /// CM = 01b in TAxCCTLn register.
    /// </summary>
    RisingEdge = 0x01,

    /// <summary>
    /// Capture on falling edge.
    /// CM = 10b in TAxCCTLn register.
    /// </summary>
    FallingEdge = 0x02,

    /// <summary>
    /// Capture on both rising and falling edges.
    /// CM = 11b in TAxCCTLn register.
    /// </summary>
    BothEdges = 0x03
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
    private CaptureEdgeMode _captureEdgeMode;
    private bool _synchronizeCaptureSource;
    private bool _outputValue;
    private bool _interruptEnable;
    private bool _interruptFlag;
    private bool _captureOverflow;
    private bool _lastCaptureRead;
    private bool _captureCompareInput;
    private bool _synchronizedCaptureCompareInput;

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
        get
        {
            // Mark that the capture value has been read (for COV logic)
            if (_mode == CaptureCompareMode.Capture)
            {
                _lastCaptureRead = true;
            }
            return _captureCompareValue;
        }
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
    /// Gets or sets the capture edge mode (used in capture mode).
    /// 
    /// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.4.1:
    /// "Capture Mode"
    /// </summary>
    public CaptureEdgeMode CaptureEdgeMode
    {
        get => _captureEdgeMode;
        set => _captureEdgeMode = value;
    }

    /// <summary>
    /// Gets or sets whether capture source is synchronized with timer clock.
    /// Setting this bit synchronizes the capture with the next timer clock.
    /// 
    /// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.4.1:
    /// "Capture Mode"
    /// </summary>
    public bool SynchronizeCaptureSource
    {
        get => _synchronizeCaptureSource;
        set => _synchronizeCaptureSource = value;
    }

    /// <summary>
    /// Gets or sets the current capture/compare input value (CCI bit).
    /// The input signal level can be read at any time through this property.
    /// 
    /// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.4.1:
    /// "Capture Mode"
    /// </summary>
    public bool CaptureCompareInput
    {
        get => _captureCompareInput;
        set => _captureCompareInput = value;
    }

    /// <summary>
    /// Gets the synchronized capture/compare input value (SCCI bit).
    /// In compare mode, the input signal CCI is latched into SCCI.
    /// 
    /// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.4.2:
    /// "Compare Mode"
    /// </summary>
    public bool SynchronizedCaptureCompareInput => _synchronizedCaptureCompareInput;

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
    /// Gets or sets the capture overflow flag (COV).
    /// Set when a second capture is performed before the first capture value was read.
    /// Must be cleared by software.
    /// 
    /// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.4.1:
    /// "Capture Mode" - Figure 13-11
    /// </summary>
    public bool CaptureOverflow
    {
        get => _captureOverflow;
        set => _captureOverflow = value;
    }

    /// <summary>
    /// Event raised when an interrupt condition occurs.
    /// </summary>
    public event EventHandler<CaptureCompareInterruptEventArgs>? InterruptRequested;

    /// <summary>
    /// Handles a timer count update in compare mode.
    /// </summary>
    /// <param name="timerValue">The current timer value.</param>
    /// <param name="isEqu0Event">True if this is an EQU0 event (timer equals TAxCCR0).</param>
    public void HandleCompareEvent(ushort timerValue, bool isEqu0Event = false)
    {
        if (_mode != CaptureCompareMode.Compare)
        {
            return;
        }

        bool isEqunEvent = timerValue == _captureCompareValue;

        if (isEqunEvent || isEqu0Event)
        {
            UpdateOutput(isEqunEvent, isEqu0Event);

            if (isEqunEvent)
            {
                // According to SLAU445I Section 13.2.4.2: "The input signal CCI is latched into SCCI"
                _synchronizedCaptureCompareInput = _captureCompareInput;

                // Set interrupt flag only on EQUn events (not EQU0)
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

        // Check for capture overflow according to SLAU445I Section 13.2.4.1 Figure 13-11
        // COV is set if a second capture occurs before the first capture was read
        if (_interruptFlag && !_lastCaptureRead)
        {
            _captureOverflow = true;
        }

        // Capture the timer value
        _captureCompareValue = timerValue;
        _lastCaptureRead = false;

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
    /// Clears the capture overflow flag.
    /// Must be called by software to clear COV bit.
    /// </summary>
    public void ClearCaptureOverflow()
    {
        _captureOverflow = false;
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
        _captureEdgeMode = CaptureEdgeMode.None;
        _synchronizeCaptureSource = false;
        _outputValue = false;
        _interruptEnable = false;
        _interruptFlag = false;
        _captureOverflow = false;
        _lastCaptureRead = true;
        _captureCompareInput = false;
        _synchronizedCaptureCompareInput = false;
    }

    /// <summary>
    /// Updates the output signal based on the output mode and event type.
    /// 
    /// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13.2.5.1:
    /// Table 13-2: Output Modes
    /// </summary>
    /// <param name="isEqunEvent">True if this is an EQUn event (timer equals TAxCCRn).</param>
    /// <param name="isEqu0Event">True if this is an EQU0 event (timer equals TAxCCR0).</param>
    private void UpdateOutput(bool isEqunEvent, bool isEqu0Event)
    {
        switch (_outputMode)
        {
            case OutputMode.Output:
                // Mode 0: Output is controlled by the OUT bit - no automatic changes
                break;

            case OutputMode.Set:
                // Mode 1: Set on EQUn, remains set until reset or mode change
                if (isEqunEvent)
                {
                    _outputValue = true;
                }
                break;

            case OutputMode.ToggleReset:
                // Mode 2: Toggle on EQUn, reset on EQU0
                if (isEqunEvent)
                {
                    _outputValue = !_outputValue;
                }
                if (isEqu0Event)
                {
                    _outputValue = false;
                }
                break;

            case OutputMode.SetReset:
                // Mode 3: Set on EQUn, reset on EQU0
                if (isEqunEvent)
                {
                    _outputValue = true;
                }
                if (isEqu0Event)
                {
                    _outputValue = false;
                }
                break;

            case OutputMode.Toggle:
                // Mode 4: Toggle on EQUn - output period is double timer period
                if (isEqunEvent)
                {
                    _outputValue = !_outputValue;
                }
                break;

            case OutputMode.Reset:
                // Mode 5: Reset on EQUn, remains reset until mode change
                if (isEqunEvent)
                {
                    _outputValue = false;
                }
                break;

            case OutputMode.ToggleSet:
                // Mode 6: Toggle on EQUn, set on EQU0
                if (isEqunEvent)
                {
                    _outputValue = !_outputValue;
                }
                if (isEqu0Event)
                {
                    _outputValue = true;
                }
                break;

            case OutputMode.ResetSet:
                // Mode 7: Reset on EQUn, set on EQU0
                if (isEqunEvent)
                {
                    _outputValue = false;
                }
                if (isEqu0Event)
                {
                    _outputValue = true;
                }
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
