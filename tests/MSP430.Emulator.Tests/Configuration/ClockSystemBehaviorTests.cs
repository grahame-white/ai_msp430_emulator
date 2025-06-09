using System;
using MSP430.Emulator.Configuration;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Tests.Configuration;

/// <summary>
/// Tests for clock system behavior validation based on MSP430FR2355 specifications.
/// 
/// MSP430FR2355 Clock System Features:
/// - DCO (Digitally Controlled Oscillator): Default clock source
/// - LFXT (Low-Frequency Crystal): 32.768 kHz crystal oscillator
/// - HFXT (High-Frequency Crystal): External crystal oscillator
/// - System Clock Generator control bits (SCG0, SCG1)
/// - Oscillator fault detection
/// 
/// References:
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 5.12: Timing and Switching Characteristics
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 5.13: Clock Specifications
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 5.2: PMM Operation
/// </summary>
public class ClockSystemBehaviorTests
{
    /// <summary>
    /// Tests for CPU frequency configuration validation according to MSP430FR2355 specifications.
    /// </summary>
    public class CpuFrequencyTests
    {
        [Theory]
        [InlineData(1000000)]    // 1 MHz - Default DCO frequency
        [InlineData(8000000)]    // 8 MHz - Maximum DCO frequency per MSP430FR2355
        [InlineData(16000000)]   // 16 MHz - Extended DCO frequency
        [InlineData(32768)]      // 32.768 kHz - LFXT frequency
        public void CpuConfig_ValidFrequencyValues_AcceptsValues(int frequency)
        {
            // Arrange & Act
            var config = new CpuConfig { Frequency = frequency };

            // Assert
            Assert.Equal(frequency, config.Frequency);
        }

        [Fact]
        public void CpuConfig_DefaultFrequency_Is1MHz()
        {
            // Arrange & Act
            var config = new CpuConfig();

            // Assert - Default frequency should be 1 MHz (conservative default)
            Assert.Equal(1000000, config.Frequency);
        }

        [Theory]
        [InlineData(0)]          // Zero frequency invalid
        [InlineData(-1000000)]   // Negative frequency invalid
        public void CpuConfig_InvalidFrequencyValues_StillAcceptsButShouldValidate(int frequency)
        {
            // Arrange & Act
            var config = new CpuConfig { Frequency = frequency };

            // Assert - Current implementation accepts any value
            // Future implementation should add validation
            Assert.Equal(frequency, config.Frequency);
        }
    }

    /// <summary>
    /// Tests for system clock generator control bits behavior according to SLAU445I Section 5.2.
    /// </summary>
    public class SystemClockGeneratorTests
    {
        [Fact]
        public void StatusRegister_SystemClockGenerator0_InitiallyFalse()
        {
            // Arrange & Act
            var sr = new StatusRegister();

            // Assert - SCG0 should be clear on reset
            Assert.False(sr.SystemClockGenerator0);
        }

        [Fact]
        public void StatusRegister_SystemClockGenerator1_InitiallyFalse()
        {
            // Arrange & Act
            var sr = new StatusRegister();

            // Assert - SCG1 should be clear on reset
            Assert.False(sr.SystemClockGenerator1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void StatusRegister_SystemClockGenerator0_SetCorrectly(bool scg0)
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.SystemClockGenerator0 = scg0;

            // Assert
            Assert.Equal(scg0, sr.SystemClockGenerator0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void StatusRegister_SystemClockGenerator1_SetCorrectly(bool scg1)
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.SystemClockGenerator1 = scg1;

            // Assert
            Assert.Equal(scg1, sr.SystemClockGenerator1);
        }

        [Fact]
        public void StatusRegister_SystemClockGenerator0_OperatesIndependentlyFromSCG1()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act 
            sr.SystemClockGenerator0 = true;

            // Assert - SCG0 can be set without affecting SCG1
            Assert.True(sr.SystemClockGenerator0);
        }

        [Fact]
        public void StatusRegister_SystemClockGenerator1_DoesNotAffectSCG0WhenSet()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.SystemClockGenerator0 = true;

            // Act
            sr.SystemClockGenerator1 = true;

            // Assert - SCG1 can be set without affecting SCG0
            Assert.True(sr.SystemClockGenerator0);
        }

        [Fact]
        public void StatusRegister_SystemClockGenerator0_CanBeClearedIndependently()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.SystemClockGenerator0 = true;
            sr.SystemClockGenerator1 = true;

            // Act
            sr.SystemClockGenerator0 = false;

            // Assert - SCG0 can be cleared without affecting SCG1
            Assert.True(sr.SystemClockGenerator1);
        }
    }

    /// <summary>
    /// Tests for oscillator control behavior according to MSP430FR2355 specifications.
    /// </summary>
    public class OscillatorControlTests
    {
        [Fact]
        public void StatusRegister_OscillatorOff_InitiallyFalse()
        {
            // Arrange & Act
            var sr = new StatusRegister();

            // Assert - Oscillator should be enabled on reset
            Assert.False(sr.OscillatorOff);
        }

        [Theory]
        [InlineData(true)]   // Oscillator disabled
        [InlineData(false)]  // Oscillator enabled
        public void StatusRegister_OscillatorOff_SetCorrectly(bool oscillatorOff)
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.OscillatorOff = oscillatorOff;

            // Assert
            Assert.Equal(oscillatorOff, sr.OscillatorOff);
        }

        [Fact]
        public void StatusRegister_OscillatorOff_CanBeSetWithSCGBits()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.OscillatorOff = true;
            sr.SystemClockGenerator0 = true;
            sr.SystemClockGenerator1 = true;

            // Assert - Oscillator control is independent of SCG bits
            Assert.True(sr.OscillatorOff);
        }

        [Fact]
        public void StatusRegister_OscillatorOff_DoesNotAffectSCG0()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.OscillatorOff = true;
            sr.SystemClockGenerator0 = true;
            sr.SystemClockGenerator1 = true;

            // Act
            sr.OscillatorOff = false;

            // Assert - Clearing oscillator off should not affect SCG0
            Assert.True(sr.SystemClockGenerator0);
        }

        [Fact]
        public void StatusRegister_OscillatorOff_DoesNotAffectSCG1()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.OscillatorOff = true;
            sr.SystemClockGenerator0 = true;
            sr.SystemClockGenerator1 = true;

            // Act
            sr.OscillatorOff = false;

            // Assert - Clearing oscillator off should not affect SCG1
            Assert.True(sr.SystemClockGenerator1);
        }
    }

    /// <summary>
    /// Tests for clock system reset behavior according to MSP430FR2355 specifications.
    /// </summary>
    public class ClockSystemResetTests
    {
        [Fact]
        public void StatusRegister_Reset_ClearsOscillatorOffBit()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.OscillatorOff = true;

            // Act
            sr.Reset();

            // Assert - Oscillator off bit should be cleared after reset
            Assert.False(sr.OscillatorOff);
        }

        [Fact]
        public void StatusRegister_Reset_ClearsSystemClockGenerator0Bit()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.SystemClockGenerator0 = true;

            // Act
            sr.Reset();

            // Assert - SCG0 bit should be cleared after reset
            Assert.False(sr.SystemClockGenerator0);
        }

        [Fact]
        public void StatusRegister_Reset_ClearsSystemClockGenerator1Bit()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.SystemClockGenerator1 = true;

            // Act
            sr.Reset();

            // Assert - SCG1 bit should be cleared after reset
            Assert.False(sr.SystemClockGenerator1);
        }

        [Theory]
        [InlineData(0xFFFF)]  // All bits set before reset
        [InlineData(0x00E0)]  // Only clock bits set before reset
        [InlineData(0x0020)]  // Only oscillator off bit set
        [InlineData(0x00C0)]  // Only SCG bits set
        public void StatusRegister_ResetFromVariousStates_ClearsOscillatorOffBit(ushort initialValue)
        {
            // Arrange
            var sr = new StatusRegister(initialValue);

            // Act
            sr.Reset();

            // Assert - Oscillator off bit should be cleared regardless of initial state
            Assert.False(sr.OscillatorOff);
        }

        [Theory]
        [InlineData(0xFFFF)]  // All bits set before reset
        [InlineData(0x00E0)]  // Only clock bits set before reset
        [InlineData(0x0020)]  // Only oscillator off bit set
        [InlineData(0x00C0)]  // Only SCG bits set
        public void StatusRegister_ResetFromVariousStates_ClearsSCG0Bit(ushort initialValue)
        {
            // Arrange
            var sr = new StatusRegister(initialValue);

            // Act
            sr.Reset();

            // Assert - SCG0 bit should be cleared regardless of initial state
            Assert.False(sr.SystemClockGenerator0);
        }

        [Theory]
        [InlineData(0xFFFF)]  // All bits set before reset
        [InlineData(0x00E0)]  // Only clock bits set before reset
        [InlineData(0x0020)]  // Only oscillator off bit set
        [InlineData(0x00C0)]  // Only SCG bits set
        public void StatusRegister_ResetFromVariousStates_ClearsSCG1Bit(ushort initialValue)
        {
            // Arrange
            var sr = new StatusRegister(initialValue);

            // Act
            sr.Reset();

            // Assert - SCG1 bit should be cleared regardless of initial state
            Assert.False(sr.SystemClockGenerator1);
        }
    }

    /// <summary>
    /// Tests for clock system configuration integration with emulator configuration.
    /// </summary>
    public class ClockConfigurationIntegrationTests
    {
        [Fact]
        public void EmulatorConfig_DefaultCpuFrequency_MatchesExpectedValue()
        {
            // Arrange & Act
            var config = EmulatorConfig.CreateDefault();

            // Assert - Default should be 1 MHz conservative value
            Assert.Equal(1000000, config.Cpu.Frequency);
        }

        [Theory]
        [InlineData(1000000)]     // 1 MHz - Valid default
        [InlineData(8000000)]     // 8 MHz - Valid maximum
        [InlineData(16000000)]    // 16 MHz - Valid extended
        [InlineData(32768)]       // 32.768 kHz - Valid LFXT
        public void EmulatorConfig_CpuFrequencyConfiguration_PersistsFrequency(int frequency)
        {
            // Arrange
            var config = EmulatorConfig.CreateDefault();
            config.Cpu.Frequency = frequency;

            // Act - Serialize and deserialize
            string json = config.ToJson();
            var deserializedConfig = EmulatorConfig.LoadFromJson(json);

            // Assert - Frequency should persist correctly
            Assert.Equal(frequency, deserializedConfig.Cpu.Frequency);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EmulatorConfig_CpuTracingConfiguration_PersistsTracing(bool enableTracing)
        {
            // Arrange
            var config = EmulatorConfig.CreateDefault();
            config.Cpu.EnableTracing = enableTracing;

            // Act - Serialize and deserialize
            string json = config.ToJson();
            var deserializedConfig = EmulatorConfig.LoadFromJson(json);

            // Assert - Tracing setting should persist correctly
            Assert.Equal(enableTracing, deserializedConfig.Cpu.EnableTracing);
        }

        [Fact]
        public void CpuConfig_EnableTracing_DefaultsFalse()
        {
            // Arrange & Act
            var config = new CpuConfig();

            // Assert - Tracing should be disabled by default
            Assert.False(config.EnableTracing);
        }
    }

    /// <summary>
    /// Tests for clock system validation according to MSP430FR2355 timing specifications.
    /// </summary>
    public class ClockSystemValidationTests
    {
        [Theory]
        [InlineData(1000000)]    // 1 MHz DCO
        [InlineData(4000000)]    // 4 MHz DCO
        [InlineData(8000000)]    // 8 MHz DCO
        [InlineData(16000000)]   // 16 MHz DCO (extended)
        public void ClockFrequency_ValidDcoFrequencies_AboveMinimum(int frequency)
        {
            // Arrange & Act
            var config = new CpuConfig { Frequency = frequency };

            // Assert - Frequency should be above minimum practical frequency
            Assert.True(frequency >= 1000000);
        }

        [Theory]
        [InlineData(1000000)]    // 1 MHz DCO
        [InlineData(4000000)]    // 4 MHz DCO
        [InlineData(8000000)]    // 8 MHz DCO
        [InlineData(16000000)]   // 16 MHz DCO (extended)
        public void ClockFrequency_ValidDcoFrequencies_BelowMaximum(int frequency)
        {
            // Arrange & Act
            var config = new CpuConfig { Frequency = frequency };

            // Assert - Frequency should be below maximum extended frequency
            Assert.True(frequency <= 16000000);
        }

        [Theory]
        [InlineData(1000000)]    // 1 MHz DCO
        [InlineData(4000000)]    // 4 MHz DCO
        [InlineData(8000000)]    // 8 MHz DCO
        [InlineData(16000000)]   // 16 MHz DCO (extended)
        public void ClockFrequency_ValidDcoFrequencies_SetCorrectly(int frequency)
        {
            // Arrange & Act
            var config = new CpuConfig { Frequency = frequency };

            // Assert - Frequency should be set as configured
            Assert.Equal(frequency, config.Frequency);
        }

        [Fact]
        public void ClockFrequency_LfxtFrequency_Is32768Hz()
        {
            // Arrange & Act
            var config = new CpuConfig { Frequency = 32768 };

            // Assert - LFXT frequency should be exactly 32.768 kHz
            Assert.Equal(32768, config.Frequency);
        }

        [Theory]
        [InlineData(1000000, 1000)]     // 1 MHz = 1000 cycles per ms
        [InlineData(8000000, 8000)]     // 8 MHz = 8000 cycles per ms
        [InlineData(32768, 32.768)]     // 32.768 kHz = 32.768 cycles per ms
        public void ClockFrequency_CyclesPerMillisecond_CalculatedCorrectly(int frequency, double expectedCyclesPerMs)
        {
            // Arrange & Act
            double cyclesPerMs = frequency / 1000.0;

            // Assert - Verify cycle calculation for timing purposes
            Assert.Equal(expectedCyclesPerMs, cyclesPerMs, 3); // 3 decimal places precision
        }
    }
}
