using System;
using MSP430.Emulator.Peripherals;
using MSP430.Emulator.Peripherals.Timers;
using Xunit;

namespace MSP430.Emulator.Tests.Peripherals.Timers;

/// <summary>
/// Unit tests for the TimerA peripheral implementation.
/// Tests timer counting modes, capture/compare functionality, and PWM generation.
/// </summary>
public class TimerATests
{
    private readonly TimerA _timer;

    public TimerATests()
    {
        _timer = new TimerA("TA0", 0x0340, 0xFFEC);
    }

    /// <summary>
    /// Tests for timer initialization and basic properties.
    /// </summary>
    public class InitializationTests : TimerATests
    {
        [Fact]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            Assert.Equal("Timer_TA0", _timer.PeripheralId);
            Assert.Equal(0x0340, _timer.BaseAddress);
            Assert.Equal(0x0000, _timer.TimerValue);
            Assert.Equal(TimerMode.Stop, _timer.Mode);
        }

        [Fact]
        public void Constructor_WithNullTimerName_ThrowsArgumentNullException()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
                new TimerA(null!, 0x0340, 0xFFEC));
            Assert.Equal("timerName", exception.ParamName);
        }

        [Fact]
        public void CaptureCompareUnits_AfterConstruction_HasThreeUnits()
        {
            Assert.Equal(3, _timer.CaptureCompareUnits.Count);
        }

        [Fact]
        public void Clock_AfterConstruction_HasDefaultConfiguration()
        {
            Assert.Equal(TimerClockSource.SMCLK, _timer.Clock.ClockSource);
            Assert.Equal(TimerInputDivider.DivideBy1, _timer.Clock.InputDivider);
            Assert.Equal(1000000U, _timer.Clock.SourceFrequency);
        }
    }

    /// <summary>
    /// Tests for timer counting modes.
    /// </summary>
    public class CountingModeTests : TimerATests
    {
        [Fact]
        public void Tick_InStopMode_DoesNotIncrementTimer()
        {
            SetTimerMode(TimerMode.Stop);
            ushort initialValue = _timer.TimerValue;

            _timer.Tick();

            Assert.Equal(initialValue, _timer.TimerValue);
        }

        [Fact]
        public void Tick_InUpModeWithPeriod100_CountsFromZeroTo100()
        {
            SetTimerMode(TimerMode.Up);
            _timer.CaptureCompareUnits[0].CaptureCompareValue = 100;

            for (int i = 0; i <= 100; i++)
            {
                Assert.Equal((ushort)i, _timer.TimerValue);
                _timer.Tick();
            }

            Assert.Equal(0, _timer.TimerValue); // Should roll over to 0
        }

        [Fact]
        public void Tick_InContinuousMode_CountsToMaxValue()
        {
            SetTimerMode(TimerMode.Continuous);

            // Fast forward near the rollover point
            SetTimerValue(0xFFFE);

            _timer.Tick();
            Assert.Equal(0xFFFF, _timer.TimerValue);

            _timer.Tick();
            Assert.Equal(0x0000, _timer.TimerValue); // Should roll over
        }

        [Fact]
        public void Tick_InUpDownModeWithPeriod10_CountsUpThenDown()
        {
            SetTimerMode(TimerMode.UpDown);
            _timer.CaptureCompareUnits[0].CaptureCompareValue = 10;

            // Count up to 10
            for (int i = 0; i <= 10; i++)
            {
                Assert.Equal((ushort)i, _timer.TimerValue);
                _timer.Tick();
            }

            // Count down to 0
            for (int i = 9; i >= 0; i--)
            {
                Assert.Equal((ushort)i, _timer.TimerValue);
                _timer.Tick();
            }

            Assert.Equal(1, _timer.TimerValue); // Should start counting up again
        }
    }

    /// <summary>
    /// Tests for capture/compare functionality.
    /// </summary>
    public class CaptureCompareTests : TimerATests
    {
        [Fact]
        public void CaptureCompareUnit_InCompareMode_GeneratesInterruptOnMatch()
        {
            bool interruptRaised = false;
            _timer.CaptureCompareUnits[1].InterruptRequested += (s, e) => interruptRaised = true;

            _timer.CaptureCompareUnits[1].Mode = CaptureCompareMode.Compare;
            _timer.CaptureCompareUnits[1].CaptureCompareValue = 50;
            _timer.CaptureCompareUnits[1].InterruptEnable = true;

            SetTimerMode(TimerMode.Continuous);
            SetTimerValue(49);

            _timer.Tick(); // Should reach 50 and trigger interrupt

            Assert.True(interruptRaised);
            Assert.True(_timer.CaptureCompareUnits[1].InterruptFlag);
        }

        [Fact]
        public void CaptureCompareUnit_InCaptureMode_CapturesTimerValue()
        {
            _timer.CaptureCompareUnits[1].Mode = CaptureCompareMode.Capture;
            SetTimerValue(0x1234);

            _timer.TriggerCaptureEvent(1);

            Assert.Equal(0x1234, _timer.CaptureCompareUnits[1].CaptureCompareValue);
            Assert.True(_timer.CaptureCompareUnits[1].InterruptFlag);
        }

        [Fact]
        public void ClearInterruptFlag_WhenSet_ClearsFlag()
        {
            _timer.CaptureCompareUnits[0].InterruptFlag = true;

            _timer.CaptureCompareUnits[0].ClearInterruptFlag();

            Assert.False(_timer.CaptureCompareUnits[0].InterruptFlag);
        }
    }

    /// <summary>
    /// Tests for PWM generation functionality.
    /// </summary>
    public class PwmTests : TimerATests
    {
        [Theory]
        [InlineData(1, 50.0, 100, 50)]
        [InlineData(2, 25.0, 200, 50)]
        [InlineData(1, 75.0, 80, 60)]
        public void SetPwmDutyCycle_WithValidParameters_SetsCorrectCompareValue(
            int unitNumber, double dutyCycle, ushort period, ushort expectedCompareValue)
        {
            _timer.CaptureCompareUnits[0].CaptureCompareValue = period;

            _timer.SetPwmDutyCycle(unitNumber, dutyCycle);

            Assert.Equal(expectedCompareValue, _timer.CaptureCompareUnits[unitNumber].CaptureCompareValue);
        }

        [Theory]
        [InlineData(1, 100, 50, 50.0)]
        [InlineData(2, 200, 50, 25.0)]
        [InlineData(1, 80, 60, 75.0)]
        public void GetPwmDutyCycle_WithValidConfiguration_ReturnsCorrectPercentage(
            int unitNumber, ushort period, ushort compareValue, double expectedDutyCycle)
        {
            SetTimerMode(TimerMode.Up);
            _timer.CaptureCompareUnits[0].CaptureCompareValue = period;
            _timer.CaptureCompareUnits[unitNumber].CaptureCompareValue = compareValue;

            double actualDutyCycle = _timer.GetPwmDutyCycle(unitNumber);

            Assert.Equal(expectedDutyCycle, actualDutyCycle, 1);
        }

        [Fact]
        public void SetPwmDutyCycle_WithDutyCycleOver100_ClampsToDutyCycle100()
        {
            _timer.CaptureCompareUnits[0].CaptureCompareValue = 100;

            _timer.SetPwmDutyCycle(1, 150.0);

            Assert.Equal(100, _timer.CaptureCompareUnits[1].CaptureCompareValue);
        }

        [Fact]
        public void SetPwmDutyCycle_WithNegativeDutyCycle_ClampsToZero()
        {
            _timer.CaptureCompareUnits[0].CaptureCompareValue = 100;

            _timer.SetPwmDutyCycle(1, -10.0);

            Assert.Equal(0, _timer.CaptureCompareUnits[1].CaptureCompareValue);
        }

        [Fact]
        public void GetPwmDutyCycle_WithUnit0_ReturnsZero()
        {
            SetTimerMode(TimerMode.Up);
            double dutyCycle = _timer.GetPwmDutyCycle(0);

            Assert.Equal(0.0, dutyCycle);
        }

        [Fact]
        public void GetPwmDutyCycle_InNonUpMode_ReturnsZero()
        {
            SetTimerMode(TimerMode.Continuous);
            _timer.CaptureCompareUnits[0].CaptureCompareValue = 100;
            _timer.CaptureCompareUnits[1].CaptureCompareValue = 50;

            double dutyCycle = _timer.GetPwmDutyCycle(1);

            Assert.Equal(0.0, dutyCycle);
        }
    }

    /// <summary>
    /// Tests for timer clock functionality.
    /// </summary>
    public class ClockTests : TimerATests
    {
        [Theory]
        [InlineData(TimerClockSource.TAxCLK)]
        [InlineData(TimerClockSource.ACLK)]
        [InlineData(TimerClockSource.SMCLK)]
        [InlineData(TimerClockSource.INCLK)]
        public void Clock_SetClockSource_UpdatesCorrectly(TimerClockSource clockSource)
        {
            _timer.Clock.ClockSource = clockSource;

            Assert.Equal(clockSource, _timer.Clock.ClockSource);
        }

        [Theory]
        [InlineData(TimerInputDivider.DivideBy1, 1000000U, 1000000U)]
        [InlineData(TimerInputDivider.DivideBy2, 1000000U, 500000U)]
        [InlineData(TimerInputDivider.DivideBy4, 1000000U, 250000U)]
        [InlineData(TimerInputDivider.DivideBy8, 1000000U, 125000U)]
        public void Clock_EffectiveFrequency_CalculatesCorrectly(
            TimerInputDivider divider, uint sourceFreq, uint expectedEffectiveFreq)
        {
            _timer.Clock.InputDivider = divider;
            _timer.Clock.SourceFrequency = sourceFreq;

            Assert.Equal(expectedEffectiveFreq, _timer.Clock.EffectiveFrequency);
        }

        [Fact]
        public void Clock_Reset_RestoresToDefaults()
        {
            _timer.Clock.ClockSource = TimerClockSource.ACLK;
            _timer.Clock.InputDivider = TimerInputDivider.DivideBy8;
            _timer.Clock.SourceFrequency = 32768;

            _timer.Clock.Reset();

            Assert.Equal(TimerClockSource.SMCLK, _timer.Clock.ClockSource);
            Assert.Equal(TimerInputDivider.DivideBy1, _timer.Clock.InputDivider);
            Assert.Equal(1000000U, _timer.Clock.SourceFrequency);
        }
    }

    /// <summary>
    /// Tests for timer reset functionality.
    /// </summary>
    public class ResetTests : TimerATests
    {
        [Fact]
        public void Reset_AfterModifyingState_RestoresToDefaults()
        {
            // Modify timer state
            SetTimerMode(TimerMode.Up);
            SetTimerValue(0x1234);
            _timer.InterruptFlag = true;
            _timer.CaptureCompareUnits[0].CaptureCompareValue = 500;
            _timer.CaptureCompareUnits[0].InterruptFlag = true;

            _timer.Reset();

            Assert.Equal(0x0000, _timer.TimerValue);
            Assert.Equal(TimerMode.Stop, _timer.Mode);
            Assert.False(_timer.InterruptFlag);
            Assert.Equal(0x0000, _timer.CaptureCompareUnits[0].CaptureCompareValue);
            Assert.False(_timer.CaptureCompareUnits[0].InterruptFlag);
        }
    }

    /// <summary>
    /// Helper methods for test setup.
    /// </summary>
    private void SetTimerMode(TimerMode mode)
    {
        // Simulate writing to the control register to set mode
        ushort controlValue = (ushort)((ushort)mode << 4);
        var controlRegister = _timer.GetType()
            .GetMethod("GetRegister", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_timer, new object[] { (ushort)(_timer.BaseAddress + 0x00) }) as PeripheralRegister;

        controlRegister?.WriteWord(controlValue);
    }

    private void SetTimerValue(ushort value)
    {
        // Use reflection to set the timer value directly for testing
        System.Reflection.FieldInfo? field = _timer.GetType().GetField("_timerValue",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(_timer, value);
    }
}
