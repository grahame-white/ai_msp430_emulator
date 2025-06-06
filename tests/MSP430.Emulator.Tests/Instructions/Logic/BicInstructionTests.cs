using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Logic;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.Logic;

/// <summary>
/// Unit tests for the BicInstruction class.
/// </summary>
public class BicInstructionTests
{
    /// <summary>
    /// Creates a fresh register file and memory array for testing.
    /// </summary>
    /// <returns>A tuple containing a new RegisterFile and memory array.</returns>
    private static (RegisterFile registerFile, byte[] memory) CreateTestEnvironment()
    {
        return (new RegisterFile(), new byte[65536]);
    }
    [Fact]
    public void Constructor_ValidParameters_CreatesInstruction()
    {
        // Arrange & Act
        var instruction = new BicInstruction(
            0xC123,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
        Assert.Equal(0xC, instruction.Opcode);
        Assert.Equal(0xC123, instruction.InstructionWord);
        Assert.Equal("BIC", instruction.Mnemonic);
        Assert.False(instruction.IsByteOperation);
        Assert.Equal(RegisterName.R1, instruction.SourceRegister);
        Assert.Equal(RegisterName.R2, instruction.DestinationRegister);
        Assert.Equal(AddressingMode.Register, instruction.SourceAddressingMode);
        Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsByteFlag()
    {
        // Arrange & Act
        var instruction = new BicInstruction(
            0xC563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Assert
        Assert.True(instruction.IsByteOperation);
        Assert.Equal("BIC.B", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Register, AddressingMode.Indirect, 0)]
    [InlineData(AddressingMode.Register, AddressingMode.IndirectAutoIncrement, 0)]
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
        // Arrange & Act
        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R1,
            RegisterName.R2,
            sourceMode,
            destMode,
            false);

        // Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_ReturnCorrectValues(AddressingMode mode)
    {
        // Arrange & Act
        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R1,
            RegisterName.R2,
            mode,
            mode,
            false);

        // Assert
        Assert.Equal(mode, instruction.SourceAddressingMode);
        Assert.Equal(mode, instruction.DestinationAddressingMode);
    }

    [Theory]
    [InlineData(RegisterName.R0)]
    [InlineData(RegisterName.R1)]
    [InlineData(RegisterName.R2)]
    [InlineData(RegisterName.CG1)]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R15)]
    public void SourceRegister_AllRegisters_ReturnsCorrectRegister(RegisterName register)
    {
        // Arrange & Act
        var instruction = new BicInstruction(
            0xC000,
            register,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(register, instruction.SourceRegister);
    }

    [Theory]
    [InlineData(RegisterName.R0)]
    [InlineData(RegisterName.R1)]
    [InlineData(RegisterName.R2)]
    [InlineData(RegisterName.CG1)]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R15)]
    public void DestinationRegister_AllRegisters_ReturnsCorrectRegister(RegisterName register)
    {
        // Arrange & Act
        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R1,
            register,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(register, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register, "R1")]
    [InlineData(AddressingMode.Indirect, "@R1")]
    [InlineData(AddressingMode.IndirectAutoIncrement, "@R1+")]
    [InlineData(AddressingMode.Immediate, "#N")]
    [InlineData(AddressingMode.Indexed, "X(R1)")]
    [InlineData(AddressingMode.Absolute, "&ADDR")]
    [InlineData(AddressingMode.Symbolic, "ADDR")]
    public void ToString_VariousAddressingModes_FormatsCorrectly(
        AddressingMode mode,
        string expectedOperand)
    {
        // Arrange
        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R1,
            RegisterName.R2,
            mode,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Contains("BIC", result);
        Assert.Contains(expectedOperand, result);
        Assert.Contains("R2", result);
    }

    [Fact]
    public void ToString_ByteOperation_IncludesByteModifier()
    {
        // Arrange
        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Contains("BIC.B", result);
    }

    [Fact]
    public void Execute_BasicOperation_ClearsBitsCorrectly()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

        registerFile.WriteRegister(RegisterName.R4, 0xFF0F); // source: bits to clear
        registerFile.WriteRegister(RegisterName.R5, 0xFFFF); // destination: all bits set

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal((ushort)0x00F0, registerFile.ReadRegister(RegisterName.R5)); // cleared bits 0-3 and 8-15
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.False(registerFile.StatusRegister.Carry);
        Assert.False(registerFile.StatusRegister.Overflow);
        Assert.True(cycles > 0);
    }

    [Fact]
    public void Execute_ByteOperation_PerformsBicOnBytes()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

        registerFile.WriteRegister(RegisterName.R4, 0x340F); // source: bits to clear (byte operation uses low byte)
        registerFile.WriteRegister(RegisterName.R5, 0x34FF); // destination

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal((ushort)((0xF0) | 0x3400), registerFile.ReadRegister(RegisterName.R5)); // cleared low byte bits, high byte unchanged
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.True(registerFile.StatusRegister.Negative); // 0xF0 has bit 7 set (negative for byte)
        Assert.True(cycles > 0);
    }

    [Fact]
    public void Execute_ResultZero_SetsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

        registerFile.WriteRegister(RegisterName.R4, 0xFFFF); // source: clear all bits
        registerFile.WriteRegister(RegisterName.R5, 0x5555); // destination

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal((ushort)0x0000, registerFile.ReadRegister(RegisterName.R5));
        Assert.True(registerFile.StatusRegister.Zero);
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_NegativeResult_SetsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // source: clear bits 0-14, leave bit 15
        registerFile.WriteRegister(RegisterName.R5, 0xFFFF); // destination

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal((ushort)0x8000, registerFile.ReadRegister(RegisterName.R5));
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.True(registerFile.StatusRegister.Negative);
    }
}
