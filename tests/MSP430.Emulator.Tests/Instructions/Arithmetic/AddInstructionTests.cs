using System;
using System.Collections.Generic;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.Arithmetic;

/// <summary>
/// Unit tests for the AddInstruction class.
/// 
/// Tests ADD instruction functionality per MSP430FR2355 specifications.
/// ADD[.W] - Add source word to destination word
/// ADD.B - Add source byte to destination byte
/// Operation: src + dst → dst
/// 
/// References:
/// - docs/references/SLAU445/4.6.2.2_add.md - ADD instruction specification
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.2: ADD instruction
/// </summary>
public class AddInstructionTests
{

    [Fact]
    public void Constructor_ValidParameters_SetsFormat()
    {
        var instruction = new AddInstruction(0x5123, RegisterName.R1, RegisterName.R4, AddressingMode.Register, AddressingMode.Register, false);

        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        var instruction = new AddInstruction(0x5123, RegisterName.R1, RegisterName.R4, AddressingMode.Register, AddressingMode.Register, false);

        Assert.Equal(0x5, instruction.Opcode);
    }

    [Theory]
    [InlineData(0x5123, 0x5123)]
    [InlineData(0x5456, 0x5456)]
    public void Constructor_ValidParameters_SetsInstructionWord(ushort instructionWord, ushort expected)
    {
        var instruction = new AddInstruction(instructionWord, RegisterName.R1, RegisterName.R4, AddressingMode.Register, AddressingMode.Register, false);

        Assert.Equal(expected, instruction.InstructionWord);
    }

    [Theory]
    [InlineData(RegisterName.R1, RegisterName.R1)]
    [InlineData(RegisterName.R2, RegisterName.R2)]
    public void Constructor_ValidParameters_SetsSourceRegister(RegisterName sourceReg, RegisterName expected)
    {
        var instruction = new AddInstruction(0x5123, sourceReg, RegisterName.R4, AddressingMode.Register, AddressingMode.Register, false);

        Assert.Equal(expected, instruction.SourceRegister);
    }

    [Theory]
    [InlineData(RegisterName.R3, RegisterName.R3)]
    [InlineData(RegisterName.R4, RegisterName.R4)]
    public void Constructor_ValidParameters_SetsDestinationRegister(RegisterName destReg, RegisterName expected)
    {
        var instruction = new AddInstruction(0x5123, RegisterName.R1, destReg, AddressingMode.Register, AddressingMode.Register, false);

        Assert.Equal(expected, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Immediate)]
    public void Constructor_ValidParameters_SetsSourceAddressingMode(AddressingMode sourceMode, AddressingMode expected)
    {
        var instruction = new AddInstruction(0x5123, RegisterName.R1, RegisterName.R4, sourceMode, AddressingMode.Register, false);

        Assert.Equal(expected, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indexed)]
    public void Constructor_ValidParameters_SetsDestinationAddressingMode(AddressingMode destMode, AddressingMode expected)
    {
        var instruction = new AddInstruction(0x5123, RegisterName.R1, RegisterName.R4, AddressingMode.Register, destMode, false);

        Assert.Equal(expected, instruction.DestinationAddressingMode);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void Constructor_ValidParameters_SetsIsByteOperation(bool isByteOp, bool expected)
    {
        var instruction = new AddInstruction(0x5123, RegisterName.R1, RegisterName.R4, AddressingMode.Register, AddressingMode.Register, isByteOp);

        Assert.Equal(expected, instruction.IsByteOperation);
    }

    [Theory]
    [InlineData(false, "ADD")]
    [InlineData(true, "ADD.B")]
    public void Constructor_ByteOperationFlag_SetsMnemonic(bool isByteOperation, string expectedMnemonic)
    {
        var instruction = new AddInstruction(0x5563, RegisterName.R5, RegisterName.R6, AddressingMode.Register, AddressingMode.Register, isByteOperation);
        Assert.Equal(expectedMnemonic, instruction.Mnemonic);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Register, AddressingMode.Absolute, 1)]
    [InlineData(AddressingMode.Register, AddressingMode.Symbolic, 1)]
    [InlineData(AddressingMode.Register, AddressingMode.Indexed, 1)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Absolute, 2)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Indexed, 2)]
    public void ExtensionWordCount_VariousAddressingModes_ReturnsCorrectCount(
        AddressingMode sourceMode,
        AddressingMode destMode,
        int expectedCount)
    {
        // Arrange
        var instruction = new AddInstruction(
            0x5000,
            RegisterName.R1,
            RegisterName.R4,
            sourceMode,
            destMode,
            false);

        // Act & Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    [Theory]
    [InlineData(RegisterName.R0, AddressingMode.Register, "R0")]
    [InlineData(RegisterName.R5, AddressingMode.Indexed, "X(R5)")]
    [InlineData(RegisterName.R3, AddressingMode.Indirect, "@R3")]
    [InlineData(RegisterName.R4, AddressingMode.IndirectAutoIncrement, "@R4+")]
    [InlineData(RegisterName.R0, AddressingMode.Immediate, "#N")]
    [InlineData(RegisterName.R4, AddressingMode.Absolute, "&ADDR")]
    [InlineData(RegisterName.R0, AddressingMode.Symbolic, "ADDR")]
    public void ToString_VariousAddressingModes_FormatsCorrectly(
        RegisterName register,
        AddressingMode mode,
        string expectedOperand)
    {
        // Arrange
        var instruction = new AddInstruction(
            0x5000,
            register,
            RegisterName.R1,
            mode,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal($"ADD {expectedOperand}, R1", result);
    }

    [Fact]
    public void ToString_ByteOperation_IncludesBSuffix()
    {
        // Arrange
        var instruction = new AddInstruction(
            0x5563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("ADD.B R5, R6", result);
    }

    [Theory]
    [InlineData(RegisterName.R0, RegisterName.R1)]
    [InlineData(RegisterName.R15, RegisterName.R4)]
    [InlineData(RegisterName.R3, RegisterName.R4)]
    [InlineData(RegisterName.R5, RegisterName.R6)]
    public void Properties_VariousRegisters_SourceRegisterCorrect(RegisterName source, RegisterName dest)
    {
        // Arrange
        var instruction = new AddInstruction(
            0x5000,
            source,
            dest,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(source, instruction.SourceRegister);
    }

    [Theory]
    [InlineData(RegisterName.R0, RegisterName.R1)]
    [InlineData(RegisterName.R15, RegisterName.R4)]
    [InlineData(RegisterName.R3, RegisterName.R4)]
    [InlineData(RegisterName.R5, RegisterName.R6)]
    public void Properties_VariousRegisters_DestinationRegisterCorrect(RegisterName source, RegisterName dest)
    {
        // Arrange
        var instruction = new AddInstruction(
            0x5000,
            source,
            dest,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(dest, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_SourceAddressingModeCorrect(AddressingMode mode)
    {
        // Arrange
        var instruction = new AddInstruction(
            0x5000,
            RegisterName.R1,
            RegisterName.R4,
            mode,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(mode, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_DestinationAddressingModeCorrect(AddressingMode mode)
    {
        // Arrange
        var instruction = new AddInstruction(
            0x5000,
            RegisterName.R1,
            RegisterName.R4,
            mode,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Execute_RegisterToRegister_ComputesCorrectResult()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x5678);

        var instruction = new AddInstruction(0x5054, RegisterName.R5, RegisterName.R4, AddressingMode.Register, AddressingMode.Register, false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal((ushort)0x68AC, registerFile.ReadRegister(RegisterName.R4)); // 0x1234 + 0x5678 = 0x68AC
    }


}
