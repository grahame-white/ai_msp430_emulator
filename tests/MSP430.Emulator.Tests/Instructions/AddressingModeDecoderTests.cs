using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;

namespace MSP430.Emulator.Tests.Instructions;

public class AddressingModeDecoderTests
{
    [Theory]
    [InlineData(RegisterName.R4, 0, AddressingMode.Register)]
    [InlineData(RegisterName.R5, 1, AddressingMode.Indexed)]
    [InlineData(RegisterName.R6, 2, AddressingMode.Indirect)]
    [InlineData(RegisterName.R7, 3, AddressingMode.IndirectAutoIncrement)]
    public void DecodeSourceAddressingMode_GeneralPurposeRegisters_ReturnsCorrectMode(
        RegisterName register, byte asBits, AddressingMode expected)
    {
        AddressingMode result = AddressingModeDecoder.DecodeSourceAddressingMode(register, asBits);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, AddressingMode.Register)]         // R0
    [InlineData(1, AddressingMode.Symbolic)]         // ADDR (PC relative)
    [InlineData(2, AddressingMode.Indirect)]         // @R0
    [InlineData(3, AddressingMode.Immediate)]        // #N
    public void DecodeSourceAddressingMode_R0_ReturnsCorrectMode(byte asBits, AddressingMode expected)
    {
        AddressingMode result = AddressingModeDecoder.DecodeSourceAddressingMode(RegisterName.R0, asBits);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, AddressingMode.Register)]                 // R2 (SR)
    [InlineData(1, AddressingMode.Absolute)]                 // &ADDR
    [InlineData(2, AddressingMode.Indirect)]                 // @R2
    [InlineData(3, AddressingMode.IndirectAutoIncrement)]    // @R2+
    public void DecodeSourceAddressingMode_R2_ReturnsCorrectMode(byte asBits, AddressingMode expected)
    {
        AddressingMode result = AddressingModeDecoder.DecodeSourceAddressingMode(RegisterName.R2, asBits);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, AddressingMode.Register)]         // R3 (Constant 0)
    [InlineData(1, AddressingMode.Immediate)]        // Constant +1
    [InlineData(2, AddressingMode.Immediate)]        // Constant +2
    [InlineData(3, AddressingMode.Immediate)]        // Constant -1
    public void DecodeSourceAddressingMode_R3_ReturnsCorrectMode(byte asBits, AddressingMode expected)
    {
        AddressingMode result = AddressingModeDecoder.DecodeSourceAddressingMode(RegisterName.R3, asBits);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DecodeSourceAddressingMode_InvalidAsBits_ReturnsInvalid()
    {
        AddressingMode result = AddressingModeDecoder.DecodeSourceAddressingMode(RegisterName.R4, 4);
        Assert.Equal(AddressingMode.Invalid, result);
    }

    [Theory]
    [InlineData(RegisterName.R4, false, AddressingMode.Register)]
    [InlineData(RegisterName.R4, true, AddressingMode.Indexed)]
    [InlineData(RegisterName.R5, false, AddressingMode.Register)]
    [InlineData(RegisterName.R5, true, AddressingMode.Indexed)]
    public void DecodeDestinationAddressingMode_GeneralPurposeRegisters_ReturnsCorrectMode(
        RegisterName register, bool adBit, AddressingMode expected)
    {
        AddressingMode result = AddressingModeDecoder.DecodeDestinationAddressingMode(register, adBit);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(false, AddressingMode.Register)]     // R0 (PC)
    [InlineData(true, AddressingMode.Symbolic)]      // ADDR (PC relative)
    public void DecodeDestinationAddressingMode_R0_ReturnsCorrectMode(bool adBit, AddressingMode expected)
    {
        AddressingMode result = AddressingModeDecoder.DecodeDestinationAddressingMode(RegisterName.R0, adBit);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(false, AddressingMode.Register)]     // R2 (SR)
    [InlineData(true, AddressingMode.Absolute)]      // &ADDR
    public void DecodeDestinationAddressingMode_R2_ReturnsCorrectMode(bool adBit, AddressingMode expected)
    {
        AddressingMode result = AddressingModeDecoder.DecodeDestinationAddressingMode(RegisterName.R2, adBit);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(AddressingMode.Register, false)]
    [InlineData(AddressingMode.Indirect, false)]
    [InlineData(AddressingMode.IndirectAutoIncrement, false)]
    [InlineData(AddressingMode.Indexed, true)]
    [InlineData(AddressingMode.Immediate, true)]
    [InlineData(AddressingMode.Absolute, true)]
    [InlineData(AddressingMode.Symbolic, true)]
    public void RequiresExtensionWord_VariousModes_ReturnsCorrectResult(
        AddressingMode mode, bool expected)
    {
        bool result = AddressingModeDecoder.RequiresExtensionWord(mode);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 0xFFFF)]
    public void GetConstantGeneratorValue_ValidAsBits_ReturnsCorrectConstant(byte asBits, ushort expected)
    {
        ushort? result = AddressingModeDecoder.GetConstantGeneratorValue(asBits);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetConstantGeneratorValue_InvalidAsBits_ReturnsNull()
    {
        ushort? result = AddressingModeDecoder.GetConstantGeneratorValue(4);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(15, true)]
    [InlineData(16, false)]
    [InlineData(255, false)]
    public void IsValidRegister_VariousValues_ReturnsCorrectResult(byte registerBits, bool expected)
    {
        bool result = AddressingModeDecoder.IsValidRegister(registerBits);
        Assert.Equal(expected, result);
    }
}
