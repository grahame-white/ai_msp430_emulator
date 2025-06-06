using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Logic;

namespace MSP430.Emulator.Tests.Instructions.Logic;

/// <summary>
/// Unit tests for the BisInstruction class.
/// </summary>
public class BisInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesInstruction()
    {
        // Arrange & Act
        var instruction = new BisInstruction(
            0xD123,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
        Assert.Equal(0xD, instruction.Opcode);
        Assert.Equal(0xD123, instruction.InstructionWord);
        Assert.Equal("BIS", instruction.Mnemonic);
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
        var instruction = new BisInstruction(
            0xD563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Assert
        Assert.True(instruction.IsByteOperation);
        Assert.Equal("BIS.B", instruction.Mnemonic);
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
        // Arrange
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            RegisterName.R2,
            sourceMode,
            destMode,
            false);

        // Act & Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    [Theory]
    [InlineData(AddressingMode.Register, "R1")]
    [InlineData(AddressingMode.Indexed, "X(R1)")]
    [InlineData(AddressingMode.Indirect, "@R1")]
    [InlineData(AddressingMode.IndirectAutoIncrement, "@R1+")]
    [InlineData(AddressingMode.Immediate, "#N")]
    [InlineData(AddressingMode.Absolute, "&ADDR")]
    [InlineData(AddressingMode.Symbolic, "ADDR")]
    public void ToString_VariousAddressingModes_FormatsCorrectly(
        AddressingMode mode,
        string expectedOperand)
    {
        // Arrange
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            RegisterName.R1,
            mode,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal($"BIS {expectedOperand}, R1", result);
    }

    [Fact]
    public void ToString_ByteOperation_IncludesByteModifier()
    {
        // Arrange
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R3,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("BIS.B R3, R4", result);
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
        var instruction = new BisInstruction(
            0xD000,
            register,
            RegisterName.R1,
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
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            register,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(register, instruction.DestinationRegister);
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
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            RegisterName.R2,
            mode,
            mode,
            false);

        // Assert
        Assert.Equal(mode, instruction.SourceAddressingMode);
        Assert.Equal(mode, instruction.DestinationAddressingMode);
    }
}
