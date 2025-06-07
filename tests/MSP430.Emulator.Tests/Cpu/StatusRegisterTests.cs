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

    [Theory]
    [InlineData(true, 0x0001)]   // Set Carry to true
    [InlineData(false, 0x0000)]  // Set Carry to false
    public void Carry_SetFlag_UpdatesValue(bool flagValue, ushort expectedValue)
    {
        var sr = new StatusRegister();
        sr.Carry = flagValue;
        Assert.Equal(expectedValue, sr.Value);
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

    [Theory]
    [InlineData(true, 0x0002)]   // Set Zero to true
    [InlineData(false, 0x0000)]  // Set Zero to false
    public void Zero_SetFlag_UpdatesValue(bool flagValue, ushort expectedValue)
    {
        var sr = new StatusRegister();
        sr.Zero = flagValue;
        Assert.Equal(expectedValue, sr.Value);
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

    [Theory]
    [InlineData(true, 0x0004)]   // Set Negative to true
    [InlineData(false, 0x0000)]  // Set Negative to false
    public void Negative_SetFlag_UpdatesValue(bool flagValue, ushort expectedValue)
    {
        var sr = new StatusRegister();
        sr.Negative = flagValue;
        Assert.Equal(expectedValue, sr.Value);
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

    [Theory]
    [InlineData(true, 0x0008)]   // Set GeneralInterruptEnable to true
    [InlineData(false, 0x0000)]  // Set GeneralInterruptEnable to false
    public void GeneralInterruptEnable_SetFlag_UpdatesValue(bool flagValue, ushort expectedValue)
    {
        var sr = new StatusRegister();
        sr.GeneralInterruptEnable = flagValue;
        Assert.Equal(expectedValue, sr.Value);
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

    [Theory]
    [InlineData(true, 0x0010)]   // Set CpuOff to true
    [InlineData(false, 0x0000)]  // Set CpuOff to false
    public void CpuOff_SetFlag_UpdatesValue(bool flagValue, ushort expectedValue)
    {
        var sr = new StatusRegister();
        sr.CpuOff = flagValue;
        Assert.Equal(expectedValue, sr.Value);
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

    [Theory]
    [InlineData(true, 0x0020)]   // Set OscillatorOff to true
    [InlineData(false, 0x0000)]  // Set OscillatorOff to false
    public void OscillatorOff_SetFlag_UpdatesValue(bool flagValue, ushort expectedValue)
    {
        var sr = new StatusRegister();
        sr.OscillatorOff = flagValue;
        Assert.Equal(expectedValue, sr.Value);
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

    [Theory]
    [InlineData(true, 0x0040)]   // Set SystemClockGenerator0 to true
    [InlineData(false, 0x0000)]  // Set SystemClockGenerator0 to false
    public void SystemClockGenerator0_SetFlag_UpdatesValue(bool flagValue, ushort expectedValue)
    {
        var sr = new StatusRegister();
        sr.SystemClockGenerator0 = flagValue;
        Assert.Equal(expectedValue, sr.Value);
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

    [Theory]
    [InlineData(true, 0x0080)]   // Set SystemClockGenerator1 to true
    [InlineData(false, 0x0000)]  // Set SystemClockGenerator1 to false
    public void SystemClockGenerator1_SetFlag_UpdatesValue(bool flagValue, ushort expectedValue)
    {
        var sr = new StatusRegister();
        sr.SystemClockGenerator1 = flagValue;
        Assert.Equal(expectedValue, sr.Value);
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

    [Theory]
    [InlineData(true, 0x0100)]   // Set Overflow to true
    [InlineData(false, 0x0000)]  // Set Overflow to false
    public void Overflow_SetFlag_UpdatesValue(bool flagValue, ushort expectedValue)
    {
        var sr = new StatusRegister();
        sr.Overflow = flagValue;
        Assert.Equal(expectedValue, sr.Value);
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
    [InlineData(0x0000, false, false, true)]   // Zero result, no special flags
    [InlineData(0x8000, false, false, false)]  // Negative result
    [InlineData(0x0000, true, false, true)]    // Zero result with carry update
    [InlineData(0x0000, false, true, true)]    // Zero result with overflow update
    [InlineData(0x1234, false, false, false)]  // Non-zero, non-negative result
    public void UpdateFlags_VariousScenarios_SetsZeroFlag(ushort result, bool updateCarry, bool updateOverflow, bool expectedZero)
    {
        var sr = new StatusRegister();
        sr.UpdateFlags(result, updateCarry, updateOverflow);

        Assert.Equal(expectedZero, sr.Zero);
    }

    [Theory]
    [InlineData(0x0000, false, false, false)]  // Zero result, no special flags
    [InlineData(0x8000, false, false, true)]   // Negative result
    [InlineData(0x0000, true, false, false)]   // Zero result with carry update
    [InlineData(0x0000, false, true, false)]   // Zero result with overflow update
    [InlineData(0x1234, false, false, false)]  // Non-zero, non-negative result
    public void UpdateFlags_VariousScenarios_SetsNegativeFlag(ushort result, bool updateCarry, bool updateOverflow, bool expectedNegative)
    {
        var sr = new StatusRegister();
        sr.UpdateFlags(result, updateCarry, updateOverflow);

        Assert.Equal(expectedNegative, sr.Negative);
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
    [InlineData(0x0001, false)]  // Only Carry - Zero flag should be false
    [InlineData(0x0007, true)]   // C, Z, N - Zero flag should be true
    [InlineData(0x00FF, true)]   // All low byte flags - Zero flag should be true
    [InlineData(0xFFFF, true)]   // All flags - Zero flag should be true
    [InlineData(0x0000, false)]  // No flags - Zero flag should be false
    [InlineData(0x0005, false)]  // C and N only - Zero flag should be false
    public void FlagOperations_PreserveOtherBits_ZeroFlag(ushort initialValue, bool expectedZero)
    {
        var sr = new StatusRegister(initialValue);

        // Toggle carry flag and verify Zero flag is preserved
        bool originalCarry = sr.Carry;
        sr.Carry = !originalCarry;

        Assert.Equal(expectedZero, sr.Zero);
    }

    [Theory]
    [InlineData(0x0001, false)]  // Only Carry - Negative flag should be false
    [InlineData(0x0007, true)]   // C, Z, N - Negative flag should be true
    [InlineData(0x00FF, true)]   // All low byte flags - Negative flag should be true
    [InlineData(0xFFFF, true)]   // All flags - Negative flag should be true
    [InlineData(0x0000, false)]  // No flags - Negative flag should be false
    [InlineData(0x0003, false)]  // C and Z only - Negative flag should be false
    public void FlagOperations_PreserveOtherBits_NegativeFlag(ushort initialValue, bool expectedNegative)
    {
        var sr = new StatusRegister(initialValue);

        // Toggle carry flag and verify Negative flag is preserved
        bool originalCarry = sr.Carry;
        sr.Carry = !originalCarry;

        Assert.Equal(expectedNegative, sr.Negative);
    }

    [Theory]
    [InlineData(0x0001, false)]  // Only Carry - GIE flag should be false
    [InlineData(0x0007, false)]  // C, Z, N - GIE flag should be false
    [InlineData(0x00FF, true)]   // All low byte flags - GIE flag should be true
    [InlineData(0xFFFF, true)]   // All flags - GIE flag should be true
    [InlineData(0x0008, true)]   // Only GIE set - GIE flag should be true
    [InlineData(0x0000, false)]  // No flags - GIE flag should be false
    public void FlagOperations_PreserveOtherBits_GeneralInterruptEnableFlag(ushort initialValue, bool expectedGIE)
    {
        var sr = new StatusRegister(initialValue);

        // Toggle carry flag and verify GIE flag is preserved
        bool originalCarry = sr.Carry;
        sr.Carry = !originalCarry;

        Assert.Equal(expectedGIE, sr.GeneralInterruptEnable);
    }

    [Theory]
    [InlineData(0x1000, 0x2000, 0x3000, false, false)] // Normal addition
    [InlineData(0x0000, 0x0000, 0x0000, false, true)]  // Zero result
    [InlineData(0xFFFF, 0x0001, 0x10000, false, true)] // Carry out, zero result
    [InlineData(0x8000, 0x8000, 0x10000, false, true)] // Overflow: negative + negative = positive (with carry)
    public void UpdateFlagsAfterAddition_WordOperation_SetsZeroFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedZero)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterAddition(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedZero, sr.Zero);
    }

    [Theory]
    [InlineData(0x1000, 0x2000, 0x3000, false, false)] // Normal addition
    [InlineData(0x8000, 0x1000, 0x9000, false, true)]  // Negative result
    [InlineData(0x7FFF, 0x0001, 0x8000, false, true)]  // Overflow: positive + positive = negative
    [InlineData(0x0000, 0x0000, 0x0000, false, false)] // Zero result
    public void UpdateFlagsAfterAddition_WordOperation_SetsNegativeFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedNegative)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterAddition(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedNegative, sr.Negative);
    }

    [Theory]
    [InlineData(0x1000, 0x2000, 0x3000, false, false)]  // Normal addition
    [InlineData(0xFFFF, 0x0001, 0x10000, false, true)]  // Carry out
    [InlineData(0x8000, 0x8000, 0x10000, false, true)]  // Overflow: negative + negative = positive (with carry)
    [InlineData(0x0000, 0x0000, 0x0000, false, false)]  // Zero result
    public void UpdateFlagsAfterAddition_WordOperation_SetsCarryFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedCarry)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterAddition(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedCarry, sr.Carry);
    }

    [Theory]
    [InlineData(0x1000, 0x2000, 0x3000, false, false)]  // Normal addition
    [InlineData(0x7FFF, 0x0001, 0x8000, false, true)]   // Overflow: positive + positive = negative
    [InlineData(0x8000, 0x8000, 0x10000, false, true)]  // Overflow: negative + negative = positive (with carry)
    [InlineData(0x0000, 0x0000, 0x0000, false, false)]  // Zero result
    public void UpdateFlagsAfterAddition_WordOperation_SetsOverflowFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedOverflow)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterAddition(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedOverflow, sr.Overflow);
    }

    [Theory]
    [InlineData(0x10, 0x20, 0x30, true, false)]     // Normal byte addition
    [InlineData(0x00, 0x00, 0x00, true, true)]      // Zero result
    [InlineData(0xFF, 0x01, 0x100, true, true)]     // Carry out, zero result
    [InlineData(0x80, 0x80, 0x100, true, true)]     // Overflow: negative + negative = positive (with carry)
    public void UpdateFlagsAfterAddition_ByteOperation_SetsZeroFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedZero)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterAddition(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedZero, sr.Zero);
    }

    [Theory]
    [InlineData(0x10, 0x20, 0x30, true, false)]     // Normal byte addition
    [InlineData(0x80, 0x10, 0x90, true, true)]      // Negative result
    [InlineData(0x7F, 0x01, 0x80, true, true)]      // Overflow: positive + positive = negative
    [InlineData(0x00, 0x00, 0x00, true, false)]     // Zero result
    public void UpdateFlagsAfterAddition_ByteOperation_SetsNegativeFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedNegative)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterAddition(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedNegative, sr.Negative);
    }

    [Theory]
    [InlineData(0x10, 0x20, 0x30, true, false)]     // Normal byte addition
    [InlineData(0xFF, 0x01, 0x100, true, true)]     // Carry out, zero result
    [InlineData(0x80, 0x80, 0x100, true, true)]     // Overflow: negative + negative = positive (with carry)
    [InlineData(0x00, 0x00, 0x00, true, false)]     // Zero result
    public void UpdateFlagsAfterAddition_ByteOperation_SetsCarryFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedCarry)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterAddition(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedCarry, sr.Carry);
    }

    [Theory]
    [InlineData(0x10, 0x20, 0x30, true, false)]     // Normal byte addition
    [InlineData(0x7F, 0x01, 0x80, true, true)]      // Overflow: positive + positive = negative
    [InlineData(0x80, 0x80, 0x100, true, true)]     // Overflow: negative + negative = positive (with carry)
    [InlineData(0x00, 0x00, 0x00, true, false)]     // Zero result
    public void UpdateFlagsAfterAddition_ByteOperation_SetsOverflowFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedOverflow)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterAddition(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedOverflow, sr.Overflow);
    }

    [Theory]
    [InlineData(0x3000, 0x1000, 0x2000, false, false)]  // Normal subtraction
    [InlineData(0x1000, 0x1000, 0x0000, false, true)]   // Zero result
    [InlineData(0x0000, 0x0001, 0xFFFF, false, false)]  // Underflow
    [InlineData(0x8000, 0x0001, 0x7FFF, false, false)]  // Overflow: negative - positive = positive
    public void UpdateFlagsAfterSubtraction_WordOperation_SetsZeroFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedZero)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterSubtraction(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedZero, sr.Zero);
    }

    [Theory]
    [InlineData(0x3000, 0x1000, 0x2000, false, false)]  // Normal subtraction
    [InlineData(0x1000, 0x2000, 0xFFFF, false, true)]   // Negative result, borrow needed
    [InlineData(0x0000, 0x0001, 0xFFFF, false, true)]   // Underflow
    [InlineData(0x7FFF, 0x8000, 0xFFFF, false, true)]   // Overflow: positive - negative = negative
    public void UpdateFlagsAfterSubtraction_WordOperation_SetsNegativeFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedNegative)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterSubtraction(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedNegative, sr.Negative);
    }

    [Theory]
    [InlineData(0x3000, 0x1000, 0x2000, false, true)]   // Normal subtraction
    [InlineData(0x1000, 0x1000, 0x0000, false, true)]   // Zero result
    [InlineData(0x8000, 0x0001, 0x7FFF, false, true)]   // Overflow: negative - positive = positive
    [InlineData(0x1000, 0x2000, 0xFFFF, false, false)]  // Negative result, borrow needed
    public void UpdateFlagsAfterSubtraction_WordOperation_SetsCarryFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedCarry)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterSubtraction(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedCarry, sr.Carry);
    }

    [Theory]
    [InlineData(0x3000, 0x1000, 0x2000, false, false)]  // Normal subtraction
    [InlineData(0x8000, 0x0001, 0x7FFF, false, true)]   // Overflow: negative - positive = positive
    [InlineData(0x7FFF, 0x8000, 0xFFFF, false, true)]   // Overflow: positive - negative = negative
    [InlineData(0x1000, 0x1000, 0x0000, false, false)]  // Zero result
    public void UpdateFlagsAfterSubtraction_WordOperation_SetsOverflowFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedOverflow)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterSubtraction(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedOverflow, sr.Overflow);
    }

    [Theory]
    [InlineData(0x30, 0x10, 0x20, true, false)]     // Normal byte subtraction
    [InlineData(0x10, 0x10, 0x00, true, true)]      // Zero result
    [InlineData(0x10, 0x20, 0xF0, true, false)]     // Negative result, borrow needed
    [InlineData(0x00, 0x01, 0xFF, true, false)]     // Underflow
    public void UpdateFlagsAfterSubtraction_ByteOperation_SetsZeroFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedZero)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterSubtraction(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedZero, sr.Zero);
    }

    [Theory]
    [InlineData(0x30, 0x10, 0x20, true, false)]     // Normal byte subtraction
    [InlineData(0x10, 0x20, 0xF0, true, true)]      // Negative result, borrow needed
    [InlineData(0x00, 0x01, 0xFF, true, true)]      // Underflow
    [InlineData(0x7F, 0x80, 0xFF, true, true)]      // Overflow: positive - negative = negative
    public void UpdateFlagsAfterSubtraction_ByteOperation_SetsNegativeFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedNegative)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterSubtraction(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedNegative, sr.Negative);
    }

    [Theory]
    [InlineData(0x30, 0x10, 0x20, true, true)]      // Normal byte subtraction
    [InlineData(0x10, 0x10, 0x00, true, true)]      // Zero result
    [InlineData(0x80, 0x01, 0x7F, true, true)]      // Overflow: negative - positive = positive
    [InlineData(0x10, 0x20, 0xF0, true, false)]     // Negative result, borrow needed
    public void UpdateFlagsAfterSubtraction_ByteOperation_SetsCarryFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedCarry)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterSubtraction(operand1, operand2, result, isByteOp);

        Assert.Equal(expectedCarry, sr.Carry);
    }

    [Theory]
    [InlineData(0x30, 0x10, 0x20, true, false)]     // Normal byte subtraction
    [InlineData(0x80, 0x01, 0x7F, true, true)]      // Overflow: negative - positive = positive
    [InlineData(0x7F, 0x80, 0xFF, true, true)]      // Overflow: positive - negative = negative
    [InlineData(0x10, 0x10, 0x00, true, false)]     // Zero result
    public void UpdateFlagsAfterSubtraction_ByteOperation_SetsOverflowFlag(
        ushort operand1, ushort operand2, uint result, bool isByteOp, bool expectedOverflow)
    {
        var sr = new StatusRegister();
        sr.UpdateFlagsAfterSubtraction(operand1, operand2, result, isByteOp);

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
