using System;
using System.Collections.Generic;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Peripherals.Timers;

/// <summary>
/// Implements the MSP430 Timer_A peripheral.
/// 
/// Timer_A is a 16-bit timer/counter with multiple capture/compare units.
/// It provides timer counting in multiple modes, capture functionality for
/// measuring external events, compare functionality for generating timed
/// outputs and interrupts, and PWM generation capabilities.
/// 
/// Features implemented:
/// - 16-bit timer counter with multiple operating modes
/// - Up to 7 capture/compare units (TAxCCR0-TAxCCR6)
/// - Multiple clock sources with input division
/// - Interrupt generation for timer and capture/compare events
/// - PWM output generation through compare modes
/// 
/// MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 13:
/// "Timer A"
/// </summary>
public class TimerA : PeripheralBase
{
    private readonly TimerClock _clock;
    private readonly CaptureCompareUnit[] _captureCompareUnits;
    private ushort _timerValue;
    private ushort _lastTimerValue;
    private TimerMode _mode;
    private bool _isCountingUp;
    private bool _interruptEnable;
    private bool _interruptFlag;
    private readonly string _timerName;
    private readonly ushort _interruptVector;

    // Timer_A Register Offsets (relative to base address)
    private const ushort OffsetTAxCTL = 0x00;    // Timer_A Control Register
    private const ushort OffsetTAxR = 0x10;      // Timer_A Counter Register
    private const ushort OffsetTAxCCTL0 = 0x02;  // Timer_A Capture/Compare Control Register 0
    private const ushort OffsetTAxCCR0 = 0x12;   // Timer_A Capture/Compare Register 0
    private const ushort OffsetTAxCCTL1 = 0x04;  // Timer_A Capture/Compare Control Register 1
    private const ushort OffsetTAxCCR1 = 0x14;   // Timer_A Capture/Compare Register 1
    private const ushort OffsetTAxCCTL2 = 0x06;  // Timer_A Capture/Compare Control Register 2
    private const ushort OffsetTAxCCR2 = 0x16;   // Timer_A Capture/Compare Register 2
    private const ushort OffsetTAxIV = 0x2E;     // Timer_A Interrupt Vector Register

    // TAxCTL Register Bit Definitions
    private const ushort TAxCTL_TAIFG = 0x0001;    // Timer_A Interrupt Flag
    private const ushort TAxCTL_TAIE = 0x0002;     // Timer_A Interrupt Enable
    private const ushort TAxCTL_TACLR = 0x0004;    // Timer_A Clear
    private const ushort TAxCTL_MC_MASK = 0x0030;  // Mode Control Mask
    private const ushort TAxCTL_MC_STOP = 0x0000;  // Stop Mode
    private const ushort TAxCTL_MC_UP = 0x0010;    // Up Mode
    private const ushort TAxCTL_MC_CONTINUOUS = 0x0020; // Continuous Mode
    private const ushort TAxCTL_MC_UPDOWN = 0x0030;     // Up/Down Mode
    private const ushort TAxCTL_ID_MASK = 0x00C0;  // Input Divider Mask
    private const ushort TAxCTL_TASSEL_MASK = 0x0300; // Timer_A Source Select Mask

    // TAxCCTLn Register Bit Definitions
    private const ushort TAxCCTLn_CCIFG = 0x0001;   // Capture/Compare Interrupt Flag
    private const ushort TAxCCTLn_COV = 0x0002;     // Capture Overflow
    private const ushort TAxCCTLn_OUT = 0x0004;     // Output
    private const ushort TAxCCTLn_CCI = 0x0008;     // Capture/Compare Input
    private const ushort TAxCCTLn_CCIE = 0x0010;    // Capture/Compare Interrupt Enable
    private const ushort TAxCCTLn_OUTMOD_MASK = 0x00E0; // Output Mode Mask
    private const ushort TAxCCTLn_CAP = 0x0100;     // Capture Mode
    private const ushort TAxCCTLn_SCCI = 0x0400;    // Synchronized Capture/Compare Input
    private const ushort TAxCCTLn_SCS = 0x0800;     // Synchronize Capture Source
    private const ushort TAxCCTLn_CCIS_MASK = 0x3000; // Capture/Compare Input Select Mask
    private const ushort TAxCCTLn_CM_MASK = 0xC000; // Capture Mode Mask

    /// <summary>
    /// Initializes a new instance of the TimerA class.
    /// </summary>
    /// <param name="timerName">The timer name (e.g., "TA0", "TA1").</param>
    /// <param name="baseAddress">The base address of the timer registers.</param>
    /// <param name="interruptVector">The interrupt vector address for this timer.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when timerName is null.</exception>
    public TimerA(string timerName, ushort baseAddress, ushort interruptVector, ILogger? logger = null)
        : base($"Timer_{timerName}", baseAddress, 0x30, logger)
    {
        _timerName = timerName ?? throw new ArgumentNullException(nameof(timerName));
        _interruptVector = interruptVector;
        _clock = new TimerClock();

        // Create capture/compare units (typically 3 units: CCR0, CCR1, CCR2)
        _captureCompareUnits = new CaptureCompareUnit[3];
        for (int i = 0; i < _captureCompareUnits.Length; i++)
        {
            _captureCompareUnits[i] = new CaptureCompareUnit(i);
            _captureCompareUnits[i].InterruptRequested += OnCaptureCompareInterrupt;
        }

        InitializeRegisters();
    }

    /// <summary>
    /// Gets the current timer value.
    /// </summary>
    public ushort TimerValue => _timerValue;

    /// <summary>
    /// Gets the current timer mode.
    /// </summary>
    public TimerMode Mode => _mode;

    /// <summary>
    /// Gets the timer clock configuration.
    /// </summary>
    public TimerClock Clock => _clock;

    /// <summary>
    /// Gets the capture/compare units.
    /// </summary>
    public IReadOnlyList<CaptureCompareUnit> CaptureCompareUnits => _captureCompareUnits;

    /// <summary>
    /// Gets or sets the timer interrupt flag.
    /// </summary>
    public bool InterruptFlag
    {
        get => _interruptFlag;
        set => _interruptFlag = value;
    }

    /// <summary>
    /// Advances the timer by one clock cycle.
    /// This method should be called by the emulator core to update the timer state.
    /// </summary>
    public void Tick()
    {
        if (_mode == TimerMode.Stop)
        {
            return;
        }

        bool rollover = false;

        switch (_mode)
        {
            case TimerMode.Up:
                TickUpMode(out rollover);
                break;

            case TimerMode.Continuous:
                TickContinuousMode(out rollover);
                break;

            case TimerMode.UpDown:
                TickUpDownMode(out rollover);
                break;
        }

        // Check for capture/compare events
        for (int i = 0; i < _captureCompareUnits.Length; i++)
        {
            CaptureCompareUnit unit = _captureCompareUnits[i];

            // Check for EQUn event (timer equals this unit's CCR value)
            bool isEqunEvent = _timerValue == unit.CaptureCompareValue;

            // Check for EQU0 event (timer equals CCR0 - only relevant for units 1+ in certain modes)
            bool isEqu0Event = false;
            if (i > 0) // Unit 0 doesn't respond to its own EQU0 event for output modes
            {
                isEqu0Event = _timerValue == _captureCompareUnits[0].CaptureCompareValue;
            }

            // Handle the compare event
            if (isEqunEvent || isEqu0Event)
            {
                unit.HandleCompareEvent(_timerValue, isEqu0Event);
            }
        }

        // Handle timer overflow interrupt
        if (rollover && _interruptEnable)
        {
            _interruptFlag = true;
            OnInterruptRequested(_interruptVector, $"{_timerName}_TAIFG", 1);
        }
    }

    /// <summary>
    /// Simulates a capture event on the specified capture/compare unit.
    /// </summary>
    /// <param name="unitNumber">The capture/compare unit number.</param>
    public void TriggerCaptureEvent(int unitNumber)
    {
        if (unitNumber >= 0 && unitNumber < _captureCompareUnits.Length)
        {
            _captureCompareUnits[unitNumber].HandleCaptureEvent(_timerValue);
        }
    }

    /// <summary>
    /// Gets the PWM duty cycle for the specified capture/compare unit.
    /// </summary>
    /// <param name="unitNumber">The capture/compare unit number.</param>
    /// <returns>The duty cycle as a percentage (0-100), or 0 if not in PWM mode.</returns>
    public double GetPwmDutyCycle(int unitNumber)
    {
        if (unitNumber < 0 || unitNumber >= _captureCompareUnits.Length || unitNumber == 0)
        {
            return 0.0;
        }

        if (_mode != TimerMode.Up)
        {
            return 0.0;
        }

        CaptureCompareUnit unit = _captureCompareUnits[unitNumber];
        ushort period = _captureCompareUnits[0].CaptureCompareValue;

        if (period == 0)
        {
            return 0.0;
        }

        return (double)unit.CaptureCompareValue / period * 100.0;
    }

    /// <summary>
    /// Sets the PWM duty cycle for the specified capture/compare unit.
    /// </summary>
    /// <param name="unitNumber">The capture/compare unit number.</param>
    /// <param name="dutyCyclePercent">The duty cycle as a percentage (0-100).</param>
    public void SetPwmDutyCycle(int unitNumber, double dutyCyclePercent)
    {
        if (unitNumber <= 0 || unitNumber >= _captureCompareUnits.Length)
        {
            return;
        }

        dutyCyclePercent = Math.Clamp(dutyCyclePercent, 0.0, 100.0);
        ushort period = _captureCompareUnits[0].CaptureCompareValue;
        ushort compareValue = (ushort)(period * dutyCyclePercent / 100.0);

        _captureCompareUnits[unitNumber].CaptureCompareValue = compareValue;
    }

    /// <inheritdoc />
    protected override void OnReset()
    {
        _timerValue = 0x0000;
        _lastTimerValue = 0x0000;
        _mode = TimerMode.Stop;
        _isCountingUp = true;
        _interruptEnable = false;
        _interruptFlag = false;
        _clock.Reset();

        foreach (CaptureCompareUnit unit in _captureCompareUnits)
        {
            unit.Reset();
        }

        Logger?.Debug($"Timer {_timerName} reset completed");
    }

    /// <inheritdoc />
    protected override void OnRegisterChanged(PeripheralRegister register, ushort address)
    {
        ushort offset = (ushort)(address - BaseAddress);

        switch (offset)
        {
            case OffsetTAxCTL:
                HandleControlRegisterChange(register.ReadWord());
                break;

            case OffsetTAxR:
                _timerValue = register.ReadWord();
                break;

            case OffsetTAxCCR0:
                // Warn if updating compare value while timer is running
                if (_mode != TimerMode.Stop && _captureCompareUnits[0].Mode == CaptureCompareMode.Compare)
                {
                    Logger?.Warning($"Timer {_timerName} CCR0 updated while timer is running. This may cause unexpected behavior. " +
                                   "Stop timer (MC=0) before updating compare values.");
                }
                _captureCompareUnits[0].CaptureCompareValue = register.ReadWord();
                break;

            case OffsetTAxCCR1:
                if (_captureCompareUnits.Length > 1)
                {
                    // Warn if updating compare value while timer is running
                    if (_mode != TimerMode.Stop && _captureCompareUnits[1].Mode == CaptureCompareMode.Compare)
                    {
                        Logger?.Warning($"Timer {_timerName} CCR1 updated while timer is running. This may cause unexpected behavior. " +
                                       "Stop timer (MC=0) before updating compare values.");
                    }
                    _captureCompareUnits[1].CaptureCompareValue = register.ReadWord();
                }

                break;

            case OffsetTAxCCR2:
                if (_captureCompareUnits.Length > 2)
                {
                    // Warn if updating compare value while timer is running
                    if (_mode != TimerMode.Stop && _captureCompareUnits[2].Mode == CaptureCompareMode.Compare)
                    {
                        Logger?.Warning($"Timer {_timerName} CCR2 updated while timer is running. This may cause unexpected behavior. " +
                                       "Stop timer (MC=0) before updating compare values.");
                    }
                    _captureCompareUnits[2].CaptureCompareValue = register.ReadWord();
                }

                break;

            case OffsetTAxCCTL0:
                HandleCaptureCompareControlChange(0, register.ReadWord());
                break;

            case OffsetTAxCCTL1:
                if (_captureCompareUnits.Length > 1)
                {
                    HandleCaptureCompareControlChange(1, register.ReadWord());
                }

                break;

            case OffsetTAxCCTL2:
                if (_captureCompareUnits.Length > 2)
                {
                    HandleCaptureCompareControlChange(2, register.ReadWord());
                }

                break;
        }
    }

    /// <summary>
    /// Initializes the timer registers.
    /// </summary>
    private void InitializeRegisters()
    {
        // Timer_A Control Register
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetTAxCTL),
            0x0000,
            PeripheralRegisterAccess.ReadWrite,
            readMask: 0x03FF,
            writeMask: 0x03FF,
            name: $"{_timerName}CTL"));

        // Timer_A Counter Register
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetTAxR),
            0x0000,
            PeripheralRegisterAccess.ReadWrite,
            name: $"{_timerName}R"));

        // Capture/Compare Control and Value Registers
        for (int i = 0; i < _captureCompareUnits.Length; i++)
        {
            ushort cctlOffset = (ushort)(OffsetTAxCCTL0 + (i * 2));
            ushort ccrOffset = (ushort)(OffsetTAxCCR0 + (i * 2));

            AddRegister(new PeripheralRegister(
                (ushort)(BaseAddress + cctlOffset),
                0x0000,
                PeripheralRegisterAccess.ReadWrite,
                readMask: 0xF1FF,
                writeMask: 0xF1FF,
                name: $"{_timerName}CCTL{i}"));

            AddRegister(new PeripheralRegister(
                (ushort)(BaseAddress + ccrOffset),
                0x0000,
                PeripheralRegisterAccess.ReadWrite,
                name: $"{_timerName}CCR{i}"));
        }

        // Timer_A Interrupt Vector Register (read-only)
        AddRegister(new PeripheralRegister(
            (ushort)(BaseAddress + OffsetTAxIV),
            0x0000,
            PeripheralRegisterAccess.Read,
            name: $"{_timerName}IV"));
    }

    /// <summary>
    /// Handles changes to the timer control register.
    /// </summary>
    /// <param name="value">The new control register value.</param>
    private void HandleControlRegisterChange(ushort value)
    {
        // Check for timer clear
        if ((value & TAxCTL_TACLR) != 0)
        {
            _timerValue = 0x0000;
            // According to SLAU445I Section 13.2.3.4: "The TACLR bit also clears the direction"
            _isCountingUp = true;
            // Clear bit is automatically cleared after operation
            PeripheralRegister? ctlRegister = GetRegister((ushort)(BaseAddress + OffsetTAxCTL));
            ctlRegister?.WriteWord((ushort)(value & ~TAxCTL_TACLR));
        }

        // Update mode
        int modeField = (value & TAxCTL_MC_MASK) >> 4;
        _mode = (TimerMode)modeField;

        // Update interrupt enable
        _interruptEnable = (value & TAxCTL_TAIE) != 0;

        // Update clock source
        int clockSourceField = (value & TAxCTL_TASSEL_MASK) >> 8;
        _clock.ClockSource = (TimerClockSource)clockSourceField;

        // Update input divider
        int dividerField = (value & TAxCTL_ID_MASK) >> 6;
        _clock.InputDivider = (TimerInputDivider)dividerField;

        Logger?.Debug($"Timer {_timerName} control updated: Mode={_mode}, ClockSource={_clock.ClockSource}, Divider={_clock.InputDivider}");
    }

    /// <summary>
    /// Handles changes to capture/compare control registers.
    /// </summary>
    /// <param name="unitNumber">The capture/compare unit number.</param>
    /// <param name="value">The new control register value.</param>
    private void HandleCaptureCompareControlChange(int unitNumber, ushort value)
    {
        CaptureCompareUnit unit = _captureCompareUnits[unitNumber];

        // Update capture/compare mode
        unit.Mode = (value & TAxCCTLn_CAP) != 0 ? CaptureCompareMode.Capture : CaptureCompareMode.Compare;

        // Update output mode (for compare mode)
        int outputModeField = (value & TAxCCTLn_OUTMOD_MASK) >> 5;
        unit.OutputMode = (OutputMode)outputModeField;

        // Update capture input select (for capture mode)
        int captureInputField = (value & TAxCCTLn_CCIS_MASK) >> 12;
        unit.CaptureInput = (CaptureInputSelect)captureInputField;

        // Update capture edge mode (for capture mode)
        int captureEdgeField = (value & TAxCCTLn_CM_MASK) >> 14;
        unit.CaptureEdgeMode = (CaptureEdgeMode)captureEdgeField;

        // Update synchronize capture source
        unit.SynchronizeCaptureSource = (value & TAxCCTLn_SCS) != 0;

        // Update interrupt enable
        unit.InterruptEnable = (value & TAxCCTLn_CCIE) != 0;

        // Update output value
        unit.OutputValue = (value & TAxCCTLn_OUT) != 0;

        // Handle software-initiated capture per SLAU445I Section 13.2.4.1.1
        // When CCIS is set to VCC/GND and toggled, it can trigger a capture
        if (unit.Mode == CaptureCompareMode.Capture &&
            (unit.CaptureInput == CaptureInputSelect.VCC || unit.CaptureInput == CaptureInputSelect.GND))
        {
            // Check if this is a transition that should trigger a capture
            CheckSoftwareCaptureTransition(unit);
        }

        Logger?.Debug($"Timer {_timerName} CCR{unitNumber} control updated: Mode={unit.Mode}, OutputMode={unit.OutputMode}, CaptureEdge={unit.CaptureEdgeMode}");

        // Handle COV bit clearing on write
        if ((value & TAxCCTLn_COV) == 0 && unit.CaptureOverflow)
        {
            unit.ClearCaptureOverflow();
            Logger?.Debug($"Timer {_timerName} CCR{unitNumber} COV flag cleared");
        }
    }

    /// <inheritdoc />
    public override ushort ReadWord(ushort address)
    {
        ushort offset = (ushort)(address - BaseAddress);

        // Handle dynamic register values that need to be built from current state
        switch (offset)
        {
            case OffsetTAxR:
                // Update timer register with current value
                GetRegister(address)?.WriteWord(_timerValue);
                break;

            case OffsetTAxCCTL0:
            case OffsetTAxCCTL1:
            case OffsetTAxCCTL2:
                // Update control register with current unit state including COV
                int unitIndex = (offset - OffsetTAxCCTL0) / 2;
                if (unitIndex < _captureCompareUnits.Length)
                {
                    ushort controlValue = BuildCaptureCompareControlValue(unitIndex);
                    GetRegister(address)?.WriteWord(controlValue);
                }
                break;
        }

        return base.ReadWord(address);
    }

    /// <summary>
    /// Builds the current control register value for a capture/compare unit.
    /// </summary>
    /// <param name="unitNumber">The unit number.</param>
    /// <returns>The control register value with current flags.</returns>
    private ushort BuildCaptureCompareControlValue(int unitNumber)
    {
        CaptureCompareUnit unit = _captureCompareUnits[unitNumber];
        ushort value = 0;

        // Build control value from current unit state
        if (unit.InterruptFlag)
        {
            value |= TAxCCTLn_CCIFG;
        }

        if (unit.CaptureOverflow)
        {
            value |= TAxCCTLn_COV;
        }

        if (unit.OutputValue)
        {
            value |= TAxCCTLn_OUT;
        }

        if (unit.CaptureCompareInput)
        {
            value |= TAxCCTLn_CCI;
        }

        if (unit.InterruptEnable)
        {
            value |= TAxCCTLn_CCIE;
        }

        value |= (ushort)((ushort)unit.OutputMode << 5);

        if (unit.Mode == CaptureCompareMode.Capture)
        {
            value |= TAxCCTLn_CAP;
        }

        if (unit.SynchronizedCaptureCompareInput)
        {
            value |= TAxCCTLn_SCCI;
        }

        if (unit.SynchronizeCaptureSource)
        {
            value |= TAxCCTLn_SCS;
        }

        value |= (ushort)((ushort)unit.CaptureInput << 12);
        value |= (ushort)((ushort)unit.CaptureEdgeMode << 14);

        return value;
    }

    /// <summary>
    /// Handles timer tick in up mode.
    /// </summary>
    /// <param name="rollover">Returns true if a rollover occurred.</param>
    private void TickUpMode(out bool rollover)
    {
        rollover = false;
        ushort period = _captureCompareUnits[0].CaptureCompareValue;

        if (period == 0)
        {
            // If period is 0, timer counts to 0xFFFF
            if (_timerValue == 0xFFFF)
            {
                _timerValue = 0x0000;
                rollover = true;
            }
            else
            {
                _timerValue++;
            }
        }
        else
        {
            // According to SLAU445I Section 13.2.3.1: 
            // "When the timer value equals TAxCCR0, the timer restarts counting from zero"
            if (_timerValue == period)
            {
                _timerValue = 0x0000;
                rollover = true;
            }
            else
            {
                _timerValue++;
            }
        }
    }

    /// <summary>
    /// Handles timer tick in continuous mode.
    /// </summary>
    /// <param name="rollover">Returns true if a rollover occurred.</param>
    private void TickContinuousMode(out bool rollover)
    {
        rollover = false;

        if (_timerValue == 0xFFFF)
        {
            _timerValue = 0x0000;
            rollover = true;
        }
        else
        {
            _timerValue++;
        }
    }

    /// <summary>
    /// Handles timer tick in up/down mode.
    /// According to SLAU445I Section 13.2.3.4: 
    /// - TAxCCR0 CCIFG is set when timer counts from TAxCCR0-1 to TAxCCR0
    /// - TAIFG is set when timer completes counting down from 0001h to 0000h
    /// </summary>
    /// <param name="rollover">Returns true if a rollover occurred.</param>
    private void TickUpDownMode(out bool rollover)
    {
        rollover = false;
        ushort period = _captureCompareUnits[0].CaptureCompareValue;
        _lastTimerValue = _timerValue;

        if (period == 0)
        {
            _timerValue = 0x0000;
            return;
        }

        if (_isCountingUp)
        {
            // Timer counts up to TAxCCR0 then down
            if (_timerValue == period)
            {
                _isCountingUp = false;
                // Don't increment on the turn-around point, start counting down
                _timerValue--;
            }
            else
            {
                _timerValue++;

                // Check for TAxCCR0 CCIFG: set when counting from CCR0-1 to CCR0
                if (_lastTimerValue == (period - 1) && _timerValue == period)
                {
                    _captureCompareUnits[0].InterruptFlag = true;
                    if (_captureCompareUnits[0].InterruptEnable)
                    {
                        OnCaptureCompareInterrupt(_captureCompareUnits[0], new CaptureCompareInterruptEventArgs(0));
                    }
                }
            }
        }
        else
        {
            if (_timerValue == 0)
            {
                _isCountingUp = true;
                rollover = true;

                // TAIFG is set when counting down from 0001h to 0000h
                if (_lastTimerValue == 1 && _timerValue == 0)
                {
                    _interruptFlag = true;
                    if (_interruptEnable)
                    {
                        OnInterruptRequested(_interruptVector, $"{_timerName}_TAIFG", 1);
                    }
                }

                // Don't decrement below 0, start counting up
                _timerValue++;
            }
            else
            {
                _timerValue--;
            }
        }
    }

    /// <summary>
    /// Checks for software-initiated capture transitions according to SLAU445I Section 13.2.4.1.1.
    /// When CCIS is set to VCC or GND, toggling can trigger captures.
    /// </summary>
    /// <param name="unit">The capture/compare unit to check.</param>
    private void CheckSoftwareCaptureTransition(CaptureCompareUnit unit)
    {
        bool newInputValue = unit.CaptureInput == CaptureInputSelect.VCC;
        bool oldInputValue = unit.CaptureCompareInput;

        // Update the current input value
        unit.CaptureCompareInput = newInputValue;

        // Check if this transition should trigger a capture based on edge mode
        bool triggerCapture = false;
        switch (unit.CaptureEdgeMode)
        {
            case CaptureEdgeMode.RisingEdge:
                triggerCapture = !oldInputValue && newInputValue;
                break;

            case CaptureEdgeMode.FallingEdge:
                triggerCapture = oldInputValue && !newInputValue;
                break;

            case CaptureEdgeMode.BothEdges:
                triggerCapture = oldInputValue != newInputValue;
                break;

            default: // CaptureEdgeMode.None or other values
                triggerCapture = false;
                break;
        }

        if (triggerCapture)
        {
            unit.HandleCaptureEvent(_timerValue);
            Logger?.Debug($"Timer {_timerName} software capture triggered on unit {unit.UnitNumber}");
        }
    }

    /// <summary>
    /// Handles capture/compare unit interrupt events.
    /// </summary>
    /// <param name="sender">The capture/compare unit that generated the interrupt.</param>
    /// <param name="e">The interrupt event arguments.</param>
    private void OnCaptureCompareInterrupt(object? sender, CaptureCompareInterruptEventArgs e)
    {
        // Calculate interrupt vector based on unit number
        ushort vector = (ushort)(_interruptVector + (e.UnitNumber * 2));
        OnInterruptRequested(vector, $"{_timerName}_CCR{e.UnitNumber}", (byte)(2 + e.UnitNumber));

        // Update interrupt vector register
        PeripheralRegister? ivRegister = GetRegister((ushort)(BaseAddress + OffsetTAxIV));
        ivRegister?.WriteWord((ushort)(e.UnitNumber * 2));
    }
}
