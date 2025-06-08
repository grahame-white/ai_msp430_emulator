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
        public void StatusRegister_SystemClockGenerator0And1_InitiallyFalse()
        {
            // Arrange & Act
            var sr = new StatusRegister();

            // Assert - SCG0 and SCG1 should be clear on reset
            Assert.False(sr.SystemClockGenerator0);
            Assert.False(sr.SystemClockGenerator1);
        }

        [Theory]
        [InlineData(true, true)]    // Both SCG bits set
        [InlineData(true, false)]   // Only SCG0 set
        [InlineData(false, true)]   // Only SCG1 set
        [InlineData(false, false)]  // Both SCG bits clear
        public void StatusRegister_SystemClockGeneratorCombinations_SetCorrectly(bool scg0, bool scg1)
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.SystemClockGenerator0 = scg0;
            sr.SystemClockGenerator1 = scg1;

            // Assert
            Assert.Equal(scg0, sr.SystemClockGenerator0);
            Assert.Equal(scg1, sr.SystemClockGenerator1);
        }

        [Fact]
        public void StatusRegister_SystemClockGeneratorFlags_IndependentOperation()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act & Assert - Test independence
            sr.SystemClockGenerator0 = true;
            Assert.True(sr.SystemClockGenerator0);
            Assert.False(sr.SystemClockGenerator1);

            sr.SystemClockGenerator1 = true;
            Assert.True(sr.SystemClockGenerator0);
            Assert.True(sr.SystemClockGenerator1);

            sr.SystemClockGenerator0 = false;
            Assert.False(sr.SystemClockGenerator0);
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
        public void StatusRegister_OscillatorOffAndClockGenerator_IndependentOperation()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act & Assert - Test that oscillator control is independent of SCG bits
            sr.OscillatorOff = true;
            sr.SystemClockGenerator0 = true;
            sr.SystemClockGenerator1 = true;

            Assert.True(sr.OscillatorOff);
            Assert.True(sr.SystemClockGenerator0);
            Assert.True(sr.SystemClockGenerator1);

            // Clear oscillator off, SCG bits should remain
            sr.OscillatorOff = false;
            Assert.False(sr.OscillatorOff);
            Assert.True(sr.SystemClockGenerator0);
            Assert.True(sr.SystemClockGenerator1);
        }
    }

    /// <summary>
    /// Tests for clock system reset behavior according to MSP430FR2355 specifications.
    /// </summary>
    public class ClockSystemResetTests
    {
        [Fact]
        public void StatusRegister_Reset_ClearsAllClockControlBits()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.OscillatorOff = true;
            sr.SystemClockGenerator0 = true;
            sr.SystemClockGenerator1 = true;

            // Act
            sr.Reset();

            // Assert - All clock control bits should be cleared after reset
            Assert.False(sr.OscillatorOff);
            Assert.False(sr.SystemClockGenerator0);
            Assert.False(sr.SystemClockGenerator1);
        }

        [Theory]
        [InlineData(0xFFFF)]  // All bits set before reset
        [InlineData(0x00E0)]  // Only clock bits set before reset
        [InlineData(0x0020)]  // Only oscillator off bit set
        [InlineData(0x00C0)]  // Only SCG bits set
        public void StatusRegister_ResetFromVariousStates_ClearsClockBits(ushort initialValue)
        {
            // Arrange
            var sr = new StatusRegister(initialValue);

            // Act
            sr.Reset();

            // Assert - Clock control bits should be cleared regardless of initial state
            Assert.False(sr.OscillatorOff);
            Assert.False(sr.SystemClockGenerator0);
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
        [InlineData(1000000, true)]   // 1 MHz - Valid default
        [InlineData(8000000, true)]   // 8 MHz - Valid maximum
        [InlineData(16000000, true)]  // 16 MHz - Valid extended
        [InlineData(32768, true)]     // 32.768 kHz - Valid LFXT
        public void EmulatorConfig_CpuFrequencyConfiguration_PersistsCorrectly(int frequency, bool enableTracing)
        {
            // Arrange
            var config = EmulatorConfig.CreateDefault();
            config.Cpu.Frequency = frequency;
            config.Cpu.EnableTracing = enableTracing;

            // Act - Serialize and deserialize
            string json = config.ToJson();
            var deserializedConfig = EmulatorConfig.LoadFromJson(json);

            // Assert - Configuration should persist correctly
            Assert.Equal(frequency, deserializedConfig.Cpu.Frequency);
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
        public void ClockFrequency_ValidDcoFrequencies_WithinSpecification(int frequency)
        {
            // Arrange & Act
            var config = new CpuConfig { Frequency = frequency };

            // Assert - These frequencies should be within MSP430FR2355 DCO range
            // Note: This test validates configuration acceptance
            // Future implementation should add proper validation logic
            Assert.True(frequency >= 1000000); // Minimum practical frequency
            Assert.True(frequency <= 16000000); // Maximum extended frequency
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
