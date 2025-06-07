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
    public void Carry_SetFlagToTrue_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.Carry = true;
        Assert.Equal((ushort)0x0001, sr.Value);
    }

    [Fact]
    public void Carry_SetFlagToFalse_UpdatesValue()
    {
        var sr = new StatusRegister();
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
    public void Zero_SetFlagToTrue_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.Zero = true;
        Assert.Equal((ushort)0x0002, sr.Value);
    }

    [Fact]
    public void Zero_SetFlagToFalse_UpdatesValue()
    {
        var sr = new StatusRegister();
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
    public void Negative_SetFlagToTrue_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.Negative = true;
        Assert.Equal((ushort)0x0004, sr.Value);
    }

    [Fact]
    public void Negative_SetFlagToFalse_UpdatesValue()
    {
        var sr = new StatusRegister();
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
    public void GeneralInterruptEnable_SetFlagToTrue_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.GeneralInterruptEnable = true;
        Assert.Equal((ushort)0x0008, sr.Value);
    }

    [Fact]
    public void GeneralInterruptEnable_SetFlagToFalse_UpdatesValue()
    {
        var sr = new StatusRegister();
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
    public void CpuOff_SetFlagToTrue_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.CpuOff = true;
        Assert.Equal((ushort)0x0010, sr.Value);
    }

    [Fact]
    public void CpuOff_SetFlagToFalse_UpdatesValue()
    {
        var sr = new StatusRegister();
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
    public void OscillatorOff_SetFlagToTrue_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.OscillatorOff = true;
        Assert.Equal((ushort)0x0020, sr.Value);
    }

    [Fact]
    public void OscillatorOff_SetFlagToFalse_UpdatesValue()
    {
        var sr = new StatusRegister();
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
    public void SystemClockGenerator0_SetFlagToTrue_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.SystemClockGenerator0 = true;
        Assert.Equal((ushort)0x0040, sr.Value);
    }

    [Fact]
    public void SystemClockGenerator0_SetFlagToFalse_UpdatesValue()
    {
        var sr = new StatusRegister();
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
    public void SystemClockGenerator1_SetFlagToTrue_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.SystemClockGenerator1 = true;
        Assert.Equal((ushort)0x0080, sr.Value);
    }

    [Fact]
    public void SystemClockGenerator1_SetFlagToFalse_UpdatesValue()
    {
        var sr = new StatusRegister();
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
    public void Overflow_SetFlagToTrue_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.Overflow = true;
        Assert.Equal((ushort)0x0100, sr.Value);
    }

    [Fact]
    public void Overflow_SetFlagToFalse_UpdatesValue()
    {
        var sr = new StatusRegister();
        sr.Overflow = false;
        Assert.Equal((ushort)0x0000, sr.Value);
    }

    [Fact]
    public void MultipleFlags_SetSimultaneously_SetsCorrectValue()
    {
        var sr = new StatusRegister();

        sr.Carry = true;
        sr.Zero = true;
        sr.Negative = true;
        sr.GeneralInterruptEnable = true;

        Assert.Equal((ushort)0x000F, sr.Value); // Bits 0-3 set
    }

    [Fact]
    public void MultipleFlags_SetSimultaneously_CarryFlagSet()
    {
        var sr = new StatusRegister();

        sr.Carry = true;
        sr.Zero = true;
        sr.Negative = true;
        sr.GeneralInterruptEnable = true;

        Assert.True(sr.Carry);
    }

    [Fact]
    public void MultipleFlags_SetSimultaneously_ZeroFlagSet()
    {
        var sr = new StatusRegister();

        sr.Carry = true;
        sr.Zero = true;
        sr.Negative = true;
        sr.GeneralInterruptEnable = true;

        Assert.True(sr.Zero);
    }

    [Fact]
    public void MultipleFlags_SetSimultaneously_NegativeFlagSet()
    {
        var sr = new StatusRegister();

        sr.Carry = true;
        sr.Zero = true;
        sr.Negative = true;
        sr.GeneralInterruptEnable = true;

        Assert.True(sr.Negative);
    }

    [Fact]
    public void MultipleFlags_SetSimultaneously_GeneralInterruptEnableFlagSet()
    {
        var sr = new StatusRegister();

        sr.Carry = true;
        sr.Zero = true;
        sr.Negative = true;
        sr.GeneralInterruptEnable = true;

        Assert.True(sr.GeneralInterruptEnable);
    }

    [Theory]
    [InlineData(0x01FF, true)]  // Carry bit set
    [InlineData(0x01FE, false)] // Carry bit clear
    public void Value_SetDirectly_UpdatesCarryFlag(ushort value, bool expectedCarry)
    {
        var sr = new StatusRegister();
        sr.Value = value;

        Assert.Equal(expectedCarry, sr.Carry);
    }

    [Theory]
    [InlineData(0x01FF, true)]  // Zero bit set
    [InlineData(0x01FD, false)] // Zero bit clear
    public void Value_SetDirectly_UpdatesZeroFlag(ushort value, bool expectedZero)
    {
        var sr = new StatusRegister();
        sr.Value = value;

        Assert.Equal(expectedZero, sr.Zero);
    }

    [Theory]
    [InlineData(0x01FF, true)]  // Negative bit set
    [InlineData(0x01FB, false)] // Negative bit clear
    public void Value_SetDirectly_UpdatesNegativeFlag(ushort value, bool expectedNegative)
    {
        var sr = new StatusRegister();
        sr.Value = value;

        Assert.Equal(expectedNegative, sr.Negative);
    }

    [Theory]
    [InlineData(0x01FF, true)]  // GIE bit set
    [InlineData(0x01F7, false)] // GIE bit clear
    public void Value_SetDirectly_UpdatesGeneralInterruptEnableFlag(ushort value, bool expectedGie)
    {
        var sr = new StatusRegister();
        sr.Value = value;

        Assert.Equal(expectedGie, sr.GeneralInterruptEnable);
    }

    [Theory]
    [InlineData(0x01FF, true)]  // CpuOff bit set
    [InlineData(0x01EF, false)] // CpuOff bit clear
    public void Value_SetDirectly_UpdatesCpuOffFlag(ushort value, bool expectedCpuOff)
    {
        var sr = new StatusRegister();
        sr.Value = value;

        Assert.Equal(expectedCpuOff, sr.CpuOff);
    }

    [Theory]
    [InlineData(0x01FF, true)]  // OscillatorOff bit set
    [InlineData(0x01DF, false)] // OscillatorOff bit clear
    public void Value_SetDirectly_UpdatesOscillatorOffFlag(ushort value, bool expectedOscOff)
    {
        var sr = new StatusRegister();
        sr.Value = value;

        Assert.Equal(expectedOscOff, sr.OscillatorOff);
    }

    [Theory]
    [InlineData(0x01FF, true)]  // SCG0 bit set
    [InlineData(0x01BF, false)] // SCG0 bit clear
    public void Value_SetDirectly_UpdatesSystemClockGenerator0Flag(ushort value, bool expectedScg0)
    {
        var sr = new StatusRegister();
        sr.Value = value;

        Assert.Equal(expectedScg0, sr.SystemClockGenerator0);
    }

    [Theory]
    [InlineData(0x01FF, true)]  // SCG1 bit set
    [InlineData(0x017F, false)] // SCG1 bit clear
    public void Value_SetDirectly_UpdatesSystemClockGenerator1Flag(ushort value, bool expectedScg1)
    {
        var sr = new StatusRegister();
        sr.Value = value;

        Assert.Equal(expectedScg1, sr.SystemClockGenerator1);
    }

    [Theory]
    [InlineData(0x01FF, true)]  // Overflow bit set
    [InlineData(0x00FF, false)] // Overflow bit clear
    public void Value_SetDirectly_UpdatesOverflowFlag(ushort value, bool expectedOverflow)
    {
        var sr = new StatusRegister();
        sr.Value = value;

        Assert.Equal(expectedOverflow, sr.Overflow);
    }

    [Fact]
    public void Reset_ClearsValueToZero()
    {
        var sr = new StatusRegister(0xFFFF);
        sr.Reset();

        Assert.Equal((ushort)0, sr.Value);
    }

    [Theory]
    [InlineData(0xFFFF, false)] // All bits set, expect carry clear after reset
    public void Reset_ClearsCarryFlag(ushort initialValue, bool expectedCarry)
    {
        var sr = new StatusRegister(initialValue);
        sr.Reset();

        Assert.Equal(expectedCarry, sr.Carry);
    }

    [Theory]
    [InlineData(0xFFFF, false)] // All bits set, expect zero clear after reset
    public void Reset_ClearsZeroFlag(ushort initialValue, bool expectedZero)
    {
        var sr = new StatusRegister(initialValue);
        sr.Reset();

        Assert.Equal(expectedZero, sr.Zero);
    }

    [Theory]
    [InlineData(0xFFFF, false)] // All bits set, expect negative clear after reset
    public void Reset_ClearsNegativeFlag(ushort initialValue, bool expectedNegative)
    {
        var sr = new StatusRegister(initialValue);
        sr.Reset();

        Assert.Equal(expectedNegative, sr.Negative);
    }

    [Theory]
    [InlineData(0xFFFF, false)] // All bits set, expect GIE clear after reset
    public void Reset_ClearsGeneralInterruptEnableFlag(ushort initialValue, bool expectedGie)
    {
        var sr = new StatusRegister(initialValue);
        sr.Reset();

        Assert.Equal(expectedGie, sr.GeneralInterruptEnable);
    }

    [Theory]
    [InlineData(0xFFFF, false)] // All bits set, expect CpuOff clear after reset
    public void Reset_ClearsCpuOffFlag(ushort initialValue, bool expectedCpuOff)
    {
        var sr = new StatusRegister(initialValue);
        sr.Reset();

        Assert.Equal(expectedCpuOff, sr.CpuOff);
    }

    [Theory]
    [InlineData(0xFFFF, false)] // All bits set, expect OscillatorOff clear after reset
    public void Reset_ClearsOscillatorOffFlag(ushort initialValue, bool expectedOscOff)
    {
        var sr = new StatusRegister(initialValue);
        sr.Reset();

        Assert.Equal(expectedOscOff, sr.OscillatorOff);
    }

    [Theory]
    [InlineData(0xFFFF, false)] // All bits set, expect SCG0 clear after reset
    public void Reset_ClearsSystemClockGenerator0Flag(ushort initialValue, bool expectedScg0)
    {
        var sr = new StatusRegister(initialValue);
        sr.Reset();

        Assert.Equal(expectedScg0, sr.SystemClockGenerator0);
    }

    [Theory]
    [InlineData(0xFFFF, false)] // All bits set, expect SCG1 clear after reset
    public void Reset_ClearsSystemClockGenerator1Flag(ushort initialValue, bool expectedScg1)
    {
        var sr = new StatusRegister(initialValue);
        sr.Reset();

        Assert.Equal(expectedScg1, sr.SystemClockGenerator1);
    }

    [Theory]
    [InlineData(0xFFFF, false)] // All bits set, expect overflow clear after reset
    public void Reset_ClearsOverflowFlag(ushort initialValue, bool expectedOverflow)
    {
        var sr = new StatusRegister(initialValue);
        sr.Reset();

        Assert.Equal(expectedOverflow, sr.Overflow);
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
    public void ToString_NoFlagsSet_ContainsCorrectHexValue()
    {
        var sr = new StatusRegister();
        string result = sr.ToString();

        Assert.Contains("SR: 0x0000", result);
    }

    [Fact]
    public void ToString_NoFlagsSet_ContainsEmptyFlagsList()
    {
        var sr = new StatusRegister();
        string result = sr.ToString();

        Assert.Contains("[]", result);
    }

    [Theory]
    [InlineData("C")]
    [InlineData("Z")]
    [InlineData("N")]
    [InlineData("GIE")]
    [InlineData("CPU_OFF")]
    [InlineData("OSC_OFF")]
    [InlineData("SCG0")]
    [InlineData("SCG1")]
    [InlineData("V")]
    public void ToString_AllFlagsSet_ShowsAllFlags(string expectedFlag)
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

        Assert.Contains(expectedFlag, result);
    }

    [Fact]
    public void ToString_SomeFlagsSet_ShowsCarryFlag()
    {
        var sr = new StatusRegister();
        sr.Carry = true;
        sr.Zero = true;

        string result = sr.ToString();

        Assert.Contains("C", result);
    }

    [Fact]
    public void ToString_SomeFlagsSet_ShowsZeroFlag()
    {
        var sr = new StatusRegister();
        sr.Carry = true;
        sr.Zero = true;

        string result = sr.ToString();

        Assert.Contains("Z", result);
    }

    [Fact]
    public void ToString_SomeFlagsSet_DoesNotShowNegativeFlag()
    {
        var sr = new StatusRegister();
        sr.Carry = true;
        sr.Zero = true;

        string result = sr.ToString();

        Assert.DoesNotContain("N", result);
    }

    [Fact]
    public void ToString_SomeFlagsSet_DoesNotShowGeneralInterruptEnableFlag()
    {
        var sr = new StatusRegister();
        sr.Carry = true;
        sr.Zero = true;

        string result = sr.ToString();

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

    [Theory]
    [InlineData(0x1000, 0x2000, 0x3000, false, false, false, false, false)] // Normal addition
    [InlineData(0x0000, 0x0000, 0x0000, false, true, false, false, false)]  // Zero result
    [InlineData(0x8000, 0x1000, 0x9000, false, false, true, false, false)]  // Negative result
    [InlineData(0xFFFF, 0x0001, 0x10000, false, true, false, true, false)]  // Carry out, zero result
    [InlineData(0x7FFF, 0x0001, 0x8000, false, false, true, false, true)]   // Overflow: positive + positive = negative
    [InlineData(0x8000, 0x8000, 0x10000, false, true, false, true, true)]   // Overflow: negative + negative = positive (with carry)
    public void UpdateFlagsAfterAddition_WordOperation_SetsCorrectFlags(
        ushort operand1, ushort operand2, uint result, bool isByteOp,
        bool expectedZero, bool expectedNegative, bool expectedCarry, bool expectedOverflow)
    {
        // Arrange
        var sr = new StatusRegister();

        // Act
        sr.UpdateFlagsAfterAddition(operand1, operand2, result, isByteOp);

        // Assert
        Assert.Equal(expectedZero, sr.Zero);
        Assert.Equal(expectedNegative, sr.Negative);
        Assert.Equal(expectedCarry, sr.Carry);
        Assert.Equal(expectedOverflow, sr.Overflow);
    }

    [Theory]
    [InlineData(0x10, 0x20, 0x30, true, false, false, false, false)]    // Normal byte addition
    [InlineData(0x00, 0x00, 0x00, true, true, false, false, false)]     // Zero result
    [InlineData(0x80, 0x10, 0x90, true, false, true, false, false)]     // Negative result
    [InlineData(0xFF, 0x01, 0x100, true, true, false, true, false)]     // Carry out, zero result
    [InlineData(0x7F, 0x01, 0x80, true, false, true, false, true)]      // Overflow: positive + positive = negative
    [InlineData(0x80, 0x80, 0x100, true, true, false, true, true)]      // Overflow: negative + negative = positive (with carry)
    public void UpdateFlagsAfterAddition_ByteOperation_SetsCorrectFlags(
        ushort operand1, ushort operand2, uint result, bool isByteOp,
        bool expectedZero, bool expectedNegative, bool expectedCarry, bool expectedOverflow)
    {
        // Arrange
        var sr = new StatusRegister();

        // Act
        sr.UpdateFlagsAfterAddition(operand1, operand2, result, isByteOp);

        // Assert
        Assert.Equal(expectedZero, sr.Zero);
        Assert.Equal(expectedNegative, sr.Negative);
        Assert.Equal(expectedCarry, sr.Carry);
        Assert.Equal(expectedOverflow, sr.Overflow);
    }

    [Theory]
    [InlineData(0x3000, 0x1000, 0x2000, false, false, false, true, false)]  // Normal subtraction
    [InlineData(0x1000, 0x1000, 0x0000, false, true, false, true, false)]   // Zero result
    [InlineData(0x1000, 0x2000, 0xFFFF, false, false, true, false, false)]  // Negative result, borrow needed
    [InlineData(0x0000, 0x0001, 0xFFFF, false, false, true, false, false)]  // Underflow
    [InlineData(0x8000, 0x0001, 0x7FFF, false, false, false, true, true)]   // Overflow: negative - positive = positive
    [InlineData(0x7FFF, 0x8000, 0xFFFF, false, false, true, false, true)]   // Overflow: positive - negative = negative
    public void UpdateFlagsAfterSubtraction_WordOperation_SetsCorrectFlags(
        ushort operand1, ushort operand2, uint result, bool isByteOp,
        bool expectedZero, bool expectedNegative, bool expectedCarry, bool expectedOverflow)
    {
        // Arrange
        var sr = new StatusRegister();

        // Act
        sr.UpdateFlagsAfterSubtraction(operand1, operand2, result, isByteOp);

        // Assert
        Assert.Equal(expectedZero, sr.Zero);
        Assert.Equal(expectedNegative, sr.Negative);
        Assert.Equal(expectedCarry, sr.Carry);
        Assert.Equal(expectedOverflow, sr.Overflow);
    }

    [Theory]
    [InlineData(0x30, 0x10, 0x20, true, false, false, true, false)]     // Normal byte subtraction
    [InlineData(0x10, 0x10, 0x00, true, true, false, true, false)]      // Zero result
    [InlineData(0x10, 0x20, 0xF0, true, false, true, false, false)]     // Negative result, borrow needed
    [InlineData(0x00, 0x01, 0xFF, true, false, true, false, false)]     // Underflow
    [InlineData(0x80, 0x01, 0x7F, true, false, false, true, true)]      // Overflow: negative - positive = positive
    [InlineData(0x7F, 0x80, 0xFF, true, false, true, false, true)]      // Overflow: positive - negative = negative
    public void UpdateFlagsAfterSubtraction_ByteOperation_SetsCorrectFlags(
        ushort operand1, ushort operand2, uint result, bool isByteOp,
        bool expectedZero, bool expectedNegative, bool expectedCarry, bool expectedOverflow)
    {
        // Arrange
        var sr = new StatusRegister();

        // Act
        sr.UpdateFlagsAfterSubtraction(operand1, operand2, result, isByteOp);

        // Assert
        Assert.Equal(expectedZero, sr.Zero);
        Assert.Equal(expectedNegative, sr.Negative);
        Assert.Equal(expectedCarry, sr.Carry);
        Assert.Equal(expectedOverflow, sr.Overflow);
    }

    [Fact]
    public void UpdateFlagsAfterAddition_EdgeCases_SetsZeroFlag()
    {
        var sr = new StatusRegister();

        // Test maximum values
        sr.UpdateFlagsAfterAddition(0xFFFF, 0xFFFF, 0x1FFFE, false);

        Assert.False(sr.Zero);
    }

    [Fact]
    public void UpdateFlagsAfterAddition_EdgeCases_SetsNegativeFlag()
    {
        var sr = new StatusRegister();

        // Test maximum values
        sr.UpdateFlagsAfterAddition(0xFFFF, 0xFFFF, 0x1FFFE, false);

        Assert.True(sr.Negative);
    }

    [Fact]
    public void UpdateFlagsAfterAddition_EdgeCases_SetsCarryFlag()
    {
        var sr = new StatusRegister();

        // Test maximum values
        sr.UpdateFlagsAfterAddition(0xFFFF, 0xFFFF, 0x1FFFE, false);

        Assert.True(sr.Carry);
    }

    [Fact]
    public void UpdateFlagsAfterAddition_EdgeCases_SetsOverflowFlag()
    {
        var sr = new StatusRegister();

        // Test maximum values
        sr.UpdateFlagsAfterAddition(0xFFFF, 0xFFFF, 0x1FFFE, false);

        Assert.False(sr.Overflow);
    }

    [Fact]
    public void UpdateFlagsAfterSubtraction_EdgeCases_SetsZeroFlag()
    {
        var sr = new StatusRegister();

        // Test minimum subtraction  
        sr.UpdateFlagsAfterSubtraction(0x0000, 0xFFFF, 0x00000001, false);

        Assert.False(sr.Zero);
    }

    [Fact]
    public void UpdateFlagsAfterSubtraction_EdgeCases_SetsNegativeFlag()
    {
        var sr = new StatusRegister();

        // Test minimum subtraction  
        sr.UpdateFlagsAfterSubtraction(0x0000, 0xFFFF, 0x00000001, false);

        Assert.False(sr.Negative);
    }

    [Fact]
    public void UpdateFlagsAfterSubtraction_EdgeCases_SetsCarryFlag()
    {
        var sr = new StatusRegister();

        // Test minimum subtraction  
        sr.UpdateFlagsAfterSubtraction(0x0000, 0xFFFF, 0x00000001, false);

        Assert.False(sr.Carry);
    }

    [Fact]
    public void UpdateFlagsAfterSubtraction_EdgeCases_SetsOverflowFlag()
    {
        var sr = new StatusRegister();

        // Test minimum subtraction  
        sr.UpdateFlagsAfterSubtraction(0x0000, 0xFFFF, 0x00000001, false);

        Assert.False(sr.Overflow);
    }
}
