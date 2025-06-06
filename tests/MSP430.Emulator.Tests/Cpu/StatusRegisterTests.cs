using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Tests.Cpu;

public class StatusRegisterTests
{
    [Fact]
    public void Constructor_DefaultValue_InitializesToZero()
    {
        var sr = new StatusRegister();
        Assert.Equal((ushort)0, sr.Value);
    }

    [Fact]
    public void Constructor_WithInitialValue_SetsValue()
    {
        ushort initialValue = 0x1234;
        var sr = new StatusRegister(initialValue);
        Assert.Equal(initialValue, sr.Value);
    }

    [Theory]
    [InlineData(0x0001, true)]   // Bit 0 set
    [InlineData(0x0000, false)]  // Bit 0 clear
    [InlineData(0xFFFE, false)]  // All bits except 0 set
    public void Carry_GetSet_WorksCorrectly(ushort value, bool expectedCarry)
    {
        var sr = new StatusRegister(value);
        Assert.Equal(expectedCarry, sr.Carry);
    }

    [Fact]
    public void Carry_SetFlag_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.Carry = true;
        Assert.Equal((ushort)0x0001, sr.Value);

        sr.Carry = false;
        Assert.Equal((ushort)0x0000, sr.Value);
    }

    [Theory]
    [InlineData(0x0002, true)]   // Bit 1 set
    [InlineData(0x0000, false)]  // Bit 1 clear
    [InlineData(0xFFFD, false)]  // All bits except 1 set
    public void Zero_GetSet_WorksCorrectly(ushort value, bool expectedZero)
    {
        var sr = new StatusRegister(value);
        Assert.Equal(expectedZero, sr.Zero);
    }

    [Fact]
    public void Zero_SetFlag_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.Zero = true;
        Assert.Equal((ushort)0x0002, sr.Value);

        sr.Zero = false;
        Assert.Equal((ushort)0x0000, sr.Value);
    }

    [Theory]
    [InlineData(0x0004, true)]   // Bit 2 set
    [InlineData(0x0000, false)]  // Bit 2 clear
    [InlineData(0xFFFB, false)]  // All bits except 2 set
    public void Negative_GetSet_WorksCorrectly(ushort value, bool expectedNegative)
    {
        var sr = new StatusRegister(value);
        Assert.Equal(expectedNegative, sr.Negative);
    }

    [Fact]
    public void Negative_SetFlag_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.Negative = true;
        Assert.Equal((ushort)0x0004, sr.Value);

        sr.Negative = false;
        Assert.Equal((ushort)0x0000, sr.Value);
    }

    [Theory]
    [InlineData(0x0008, true)]   // Bit 3 set
    [InlineData(0x0000, false)]  // Bit 3 clear
    [InlineData(0xFFF7, false)]  // All bits except 3 set
    public void GeneralInterruptEnable_GetSet_WorksCorrectly(ushort value, bool expected)
    {
        var sr = new StatusRegister(value);
        Assert.Equal(expected, sr.GeneralInterruptEnable);
    }

    [Fact]
    public void GeneralInterruptEnable_SetFlag_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.GeneralInterruptEnable = true;
        Assert.Equal((ushort)0x0008, sr.Value);

        sr.GeneralInterruptEnable = false;
        Assert.Equal((ushort)0x0000, sr.Value);
    }

    [Theory]
    [InlineData(0x0010, true)]   // Bit 4 set
    [InlineData(0x0000, false)]  // Bit 4 clear
    [InlineData(0xFFEF, false)]  // All bits except 4 set
    public void CpuOff_GetSet_WorksCorrectly(ushort value, bool expected)
    {
        var sr = new StatusRegister(value);
        Assert.Equal(expected, sr.CpuOff);
    }

    [Fact]
    public void CpuOff_SetFlag_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.CpuOff = true;
        Assert.Equal((ushort)0x0010, sr.Value);

        sr.CpuOff = false;
        Assert.Equal((ushort)0x0000, sr.Value);
    }

    [Theory]
    [InlineData(0x0020, true)]   // Bit 5 set
    [InlineData(0x0000, false)]  // Bit 5 clear
    [InlineData(0xFFDF, false)]  // All bits except 5 set
    public void OscillatorOff_GetSet_WorksCorrectly(ushort value, bool expected)
    {
        var sr = new StatusRegister(value);
        Assert.Equal(expected, sr.OscillatorOff);
    }

    [Fact]
    public void OscillatorOff_SetFlag_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.OscillatorOff = true;
        Assert.Equal((ushort)0x0020, sr.Value);

        sr.OscillatorOff = false;
        Assert.Equal((ushort)0x0000, sr.Value);
    }

    [Theory]
    [InlineData(0x0040, true)]   // Bit 6 set
    [InlineData(0x0000, false)]  // Bit 6 clear
    [InlineData(0xFFBF, false)]  // All bits except 6 set
    public void SystemClockGenerator0_GetSet_WorksCorrectly(ushort value, bool expected)
    {
        var sr = new StatusRegister(value);
        Assert.Equal(expected, sr.SystemClockGenerator0);
    }

    [Fact]
    public void SystemClockGenerator0_SetFlag_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.SystemClockGenerator0 = true;
        Assert.Equal((ushort)0x0040, sr.Value);

        sr.SystemClockGenerator0 = false;
        Assert.Equal((ushort)0x0000, sr.Value);
    }

    [Theory]
    [InlineData(0x0080, true)]   // Bit 7 set
    [InlineData(0x0000, false)]  // Bit 7 clear
    [InlineData(0xFF7F, false)]  // All bits except 7 set
    public void SystemClockGenerator1_GetSet_WorksCorrectly(ushort value, bool expected)
    {
        var sr = new StatusRegister(value);
        Assert.Equal(expected, sr.SystemClockGenerator1);
    }

    [Fact]
    public void SystemClockGenerator1_SetFlag_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.SystemClockGenerator1 = true;
        Assert.Equal((ushort)0x0080, sr.Value);

        sr.SystemClockGenerator1 = false;
        Assert.Equal((ushort)0x0000, sr.Value);
    }

    [Theory]
    [InlineData(0x0100, true)]   // Bit 8 set
    [InlineData(0x0000, false)]  // Bit 8 clear
    [InlineData(0xFEFF, false)]  // All bits except 8 set
    public void Overflow_GetSet_WorksCorrectly(ushort value, bool expected)
    {
        var sr = new StatusRegister(value);
        Assert.Equal(expected, sr.Overflow);
    }

    [Fact]
    public void Overflow_SetFlag_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.Overflow = true;
        Assert.Equal((ushort)0x0100, sr.Value);

        sr.Overflow = false;
        Assert.Equal((ushort)0x0000, sr.Value);
    }

    [Fact]
    public void MultipleFlags_SetSimultaneously_WorkCorrectly()
    {
        var sr = new StatusRegister();

        sr.Carry = true;
        sr.Zero = true;
        sr.Negative = true;
        sr.GeneralInterruptEnable = true;

        Assert.Equal((ushort)0x000F, sr.Value); // Bits 0-3 set
        Assert.True(sr.Carry);
        Assert.True(sr.Zero);
        Assert.True(sr.Negative);
        Assert.True(sr.GeneralInterruptEnable);
    }

    [Fact]
    public void Value_SetDirectly_UpdatesAllFlags()
    {
        var sr = new StatusRegister();
        sr.Value = 0x01FF; // Set bits 0-8

        Assert.True(sr.Carry);
        Assert.True(sr.Zero);
        Assert.True(sr.Negative);
        Assert.True(sr.GeneralInterruptEnable);
        Assert.True(sr.CpuOff);
        Assert.True(sr.OscillatorOff);
        Assert.True(sr.SystemClockGenerator0);
        Assert.True(sr.SystemClockGenerator1);
        Assert.True(sr.Overflow);
    }

    [Fact]
    public void Reset_ClearsAllFlags()
    {
        var sr = new StatusRegister(0xFFFF);
        sr.Reset();

        Assert.Equal((ushort)0, sr.Value);
        Assert.False(sr.Carry);
        Assert.False(sr.Zero);
        Assert.False(sr.Negative);
        Assert.False(sr.GeneralInterruptEnable);
        Assert.False(sr.CpuOff);
        Assert.False(sr.OscillatorOff);
        Assert.False(sr.SystemClockGenerator0);
        Assert.False(sr.SystemClockGenerator1);
        Assert.False(sr.Overflow);
    }

    [Theory]
    [InlineData(0x0000, false, false)]  // Zero result, no special flags
    [InlineData(0x8000, false, false)]  // Negative result
    [InlineData(0x0000, true, false)]   // Zero result with carry update
    [InlineData(0x0000, false, true)]   // Zero result with overflow update
    public void UpdateFlags_VariousScenarios_UpdatesCorrectly(ushort result, bool updateCarry, bool updateOverflow)
    {
        var sr = new StatusRegister();
        sr.UpdateFlags(result, updateCarry, updateOverflow);

        Assert.Equal(result == 0, sr.Zero);
        Assert.Equal((result & 0x8000) != 0, sr.Negative);
    }

    [Fact]
    public void ToString_NoFlagsSet_ReturnsCorrectFormat()
    {
        var sr = new StatusRegister();
        string result = sr.ToString();

        Assert.Contains("SR: 0x0000", result);
        Assert.Contains("[]", result);
    }

    [Fact]
    public void ToString_AllFlagsSet_ShowsAllFlags()
    {
        var sr = new StatusRegister();
        sr.Carry = true;
        sr.Zero = true;
        sr.Negative = true;
        sr.GeneralInterruptEnable = true;
        sr.CpuOff = true;
        sr.OscillatorOff = true;
        sr.SystemClockGenerator0 = true;
        sr.SystemClockGenerator1 = true;
        sr.Overflow = true;

        string result = sr.ToString();

        Assert.Contains("C", result);
        Assert.Contains("Z", result);
        Assert.Contains("N", result);
        Assert.Contains("GIE", result);
        Assert.Contains("CPU_OFF", result);
        Assert.Contains("OSC_OFF", result);
        Assert.Contains("SCG0", result);
        Assert.Contains("SCG1", result);
        Assert.Contains("V", result);
    }

    [Fact]
    public void ToString_SomeFlagsSet_ShowsOnlySetFlags()
    {
        var sr = new StatusRegister();
        sr.Carry = true;
        sr.Zero = true;

        string result = sr.ToString();

        Assert.Contains("C", result);
        Assert.Contains("Z", result);
        Assert.DoesNotContain("N", result);
        Assert.DoesNotContain("GIE", result);
    }

    [Theory]
    [InlineData(0x0001)]  // Only Carry
    [InlineData(0x0007)]  // C, Z, N
    [InlineData(0x00FF)]  // All low byte flags
    [InlineData(0xFFFF)]  // All flags
    public void FlagOperations_PreserveOtherBits(ushort initialValue)
    {
        var sr = new StatusRegister(initialValue);

        // Toggle one flag and verify others are preserved
        bool originalCarry = sr.Carry;
        sr.Carry = !originalCarry;

        // All other flags should remain the same
        Assert.Equal((initialValue & 0x0002) != 0, sr.Zero);
        Assert.Equal((initialValue & 0x0004) != 0, sr.Negative);
        Assert.Equal((initialValue & 0x0008) != 0, sr.GeneralInterruptEnable);
    }
}
