using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;

namespace MSP430.Emulator.Tests.Instructions.Arithmetic;

/// <summary>
/// Unit tests for the DecInstruction class.
/// </summary>
public class DecInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesInstruction()
    {
        // Arrange & Act
        var instruction = new DecInstruction(
            0xB123,
            RegisterName.R2,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
        Assert.Equal(0xB, instruction.Opcode);
        Assert.Equal(0xB123, instruction.InstructionWord);
        Assert.Equal("DEC", instruction.Mnemonic);
        Assert.False(instruction.IsByteOperation);
        Assert.Equal(RegisterName.R2, instruction.DestinationRegister);
        Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
        Assert.Null(instruction.SourceRegister);
        Assert.Null(instruction.SourceAddressingMode);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsByteFlag()
    {
        // Arrange & Act
        var instruction = new DecInstruction(
            0xB563,
            RegisterName.R5,
            AddressingMode.Register,
            true);

        // Assert
        Assert.True(instruction.IsByteOperation);
        Assert.Equal("DEC.B", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Indirect, 0)]
    [InlineData(AddressingMode.IndirectAutoIncrement, 0)]
    [InlineData(AddressingMode.Absolute, 1)]
    [InlineData(AddressingMode.Symbolic, 1)]
    [InlineData(AddressingMode.Indexed, 1)]
    public void ExtensionWordCount_VariousAddressingModes_ReturnsCorrectCount(
        AddressingMode destMode,
        int expectedCount)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            RegisterName.R1,
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
    [InlineData(RegisterName.R2, AddressingMode.Absolute, "&ADDR")]
    [InlineData(RegisterName.R0, AddressingMode.Symbolic, "ADDR")]
    public void ToString_VariousAddressingModes_FormatsCorrectly(
        RegisterName register,
        AddressingMode mode,
        string expectedOperand)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            register,
            mode,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal($"DEC {expectedOperand}", result);
    }

    [Fact]
    public void ToString_ByteOperation_IncludesBSuffix()
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB563,
            RegisterName.R5,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("DEC.B R5", result);
    }

    [Theory]
    [InlineData(RegisterName.R0)]
    [InlineData(RegisterName.R15)]
    [InlineData(RegisterName.R3)]
    [InlineData(RegisterName.R5)]
    public void Properties_VariousRegisters_ReturnCorrectValues(RegisterName dest)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            dest,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(dest, instruction.DestinationRegister);
        Assert.Null(instruction.SourceRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_ReturnCorrectValues(AddressingMode mode)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            RegisterName.R1,
            mode,
            false);

        // Act & Assert
        Assert.Equal(mode, instruction.DestinationAddressingMode);
        Assert.Null(instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(RegisterName.R1, RegisterName.R2)]
    [InlineData(RegisterName.R15, RegisterName.R3)]
    [InlineData(RegisterName.R4, RegisterName.R5)]
    [InlineData(RegisterName.R6, RegisterName.R7)]
    public void Properties_DifferentRegisters_ReturnCorrectDestination(RegisterName expected, RegisterName other)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            expected,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(expected, instruction.DestinationRegister);
        Assert.NotEqual(other, instruction.DestinationRegister);
    }

    [Fact]
    public void Opcode_IsCorrectValue()
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            RegisterName.R1,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(0xB, instruction.Opcode);
    }

    [Fact]
    public void Format_IsFormatI()
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            RegisterName.R1,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
    }
}
