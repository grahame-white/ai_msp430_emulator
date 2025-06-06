using System;
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
            RegisterName.R4,
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
    [InlineData(RegisterName.R4, AddressingMode.Absolute, "&ADDR")]
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
    [InlineData(RegisterName.R7)]
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
            RegisterName.R4,
            mode,
            false);

        // Act & Assert
        Assert.Equal(mode, instruction.DestinationAddressingMode);
        Assert.Null(instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(RegisterName.R4, RegisterName.R5)]
    [InlineData(RegisterName.R7, RegisterName.R3)]
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
            RegisterName.R4,
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
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
    }

    [Fact]
    public void Execute_RegisterMode_DecrementsValueAndUpdatesFlags()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1235);

        var instruction = new DecInstruction(
            0xB505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x1234, registerFile.ReadRegister(RegisterName.R5));
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.True(registerFile.StatusRegister.Carry); // Carry set when not decrementing 0
        Assert.False(registerFile.StatusRegister.Overflow);
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_DecrementsLowByteOnly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB35);

        var instruction = new DecInstruction(
            0xB543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0xAB34, registerFile.ReadRegister(RegisterName.R3));
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.True(registerFile.StatusRegister.Carry);
        Assert.False(registerFile.StatusRegister.Overflow);
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_DecrementOne_SetsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x0001);

        var instruction = new DecInstruction(
            0xB505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x0000, registerFile.ReadRegister(RegisterName.R5));
        Assert.True(registerFile.StatusRegister.Zero);
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.True(registerFile.StatusRegister.Carry);
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_DecrementZero_ClearsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x0000);

        var instruction = new DecInstruction(
            0xB504,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0xFFFF, registerFile.ReadRegister(RegisterName.R4));
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.True(registerFile.StatusRegister.Negative);
        Assert.False(registerFile.StatusRegister.Carry); // Carry clear when decrementing 0
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_NegativeOverflow_SetsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x8000); // Most negative value

        var instruction = new DecInstruction(
            0xB504,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x7FFF, registerFile.ReadRegister(RegisterName.R4));
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.True(registerFile.StatusRegister.Carry);
        Assert.True(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_ByteNegativeOverflow_SetsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R6, 0x1280); // Most negative byte value

        var instruction = new DecInstruction(
            0xB546,
            RegisterName.R6,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x127F, registerFile.ReadRegister(RegisterName.R6));
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.True(registerFile.StatusRegister.Carry);
        Assert.True(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_AbsoluteMode_DecrementsMemoryValue()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = new ushort[] { 0x0200 }; // Absolute address

        // Set up memory with initial value
        memory[0x0200] = 0x35;
        memory[0x0201] = 0x12; // 0x1235 in little-endian

        var instruction = new DecInstruction(
            0xB582,
            RegisterName.R2, // Register not used in absolute mode
            AddressingMode.Absolute,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x34, memory[0x0200]);
        Assert.Equal(0x12, memory[0x0201]); // Should be 0x1234 in little-endian
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.True(registerFile.StatusRegister.Carry);
        Assert.False(registerFile.StatusRegister.Overflow);
        Assert.Equal(4u, cycles);
    }

    [Fact]
    public void Execute_IndirectMode_DecrementsMemoryValueAtRegisterAddress()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R7, 0x0300); // Address in R7
        memory[0x0300] = 0x01;
        memory[0x0301] = 0x80; // 0x8001 in little-endian

        var instruction = new DecInstruction(
            0xB527,
            RegisterName.R7,
            AddressingMode.Indirect,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x00, memory[0x0300]);
        Assert.Equal(0x80, memory[0x0301]); // Should be 0x8000 in little-endian
        Assert.Equal((ushort)0x0300, registerFile.ReadRegister(RegisterName.R7)); // R7 unchanged
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.True(registerFile.StatusRegister.Negative);
        Assert.True(registerFile.StatusRegister.Carry);
        Assert.False(registerFile.StatusRegister.Overflow);
        Assert.Equal(3u, cycles);
    }
}
