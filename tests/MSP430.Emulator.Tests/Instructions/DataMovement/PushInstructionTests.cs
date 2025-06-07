using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.DataMovement;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.DataMovement;

/// <summary>
/// Unit tests for the PushInstruction class.
/// Tests all addressing modes, byte/word operations, and stack management behavior.
/// </summary>
public class PushInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsFormat()
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(InstructionFormat.FormatII, instruction.Format);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(0x12, instruction.Opcode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsInstructionWord()
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(0x1204, instruction.InstructionWord);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsMnemonic()
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal("PUSH", instruction.Mnemonic);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsMnemonic()
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1244,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Assert
        Assert.Equal("PUSH.B", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_ValidParameters_SetsByteOperation(bool expectedByteOp)
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            expectedByteOp);

        // Assert
        Assert.Equal(expectedByteOp, instruction.IsByteOperation);
    }

    [Theory]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R5)]
    [InlineData(RegisterName.R15)]
    public void Constructor_ValidParameters_SetsSourceRegister(RegisterName expectedSourceRegister)
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            expectedSourceRegister,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedSourceRegister, instruction.SourceRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    public void Constructor_ValidParameters_SetsSourceAddressingMode(AddressingMode expectedSourceAddressingMode)
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            expectedSourceAddressingMode,
            false);

        // Assert
        Assert.Equal(expectedSourceAddressingMode, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Indexed, 1)]
    [InlineData(AddressingMode.Indirect, 0)]
    [InlineData(AddressingMode.IndirectAutoIncrement, 0)]
    [InlineData(AddressingMode.Immediate, 1)]
    [InlineData(AddressingMode.Absolute, 1)]
    [InlineData(AddressingMode.Symbolic, 1)]
    public void ExtensionWordCount_ReturnsCorrectCount(AddressingMode addressingMode, int expectedCount)
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            addressingMode,
            false);

        // Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    [Fact]
    public void Execute_RegisterMode_PushesValueAndDecrementsStackPointer()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PushInstruction(0x1204, RegisterName.R4, AddressingMode.Register, false);

        registerFile.WriteRegister(RegisterName.R4, 0x1234);
        registerFile.SetStackPointer(0x1000);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer()); // SP decremented by 2
        Assert.Equal(0x34, memory[0x0FFE]); // Low byte of value stored (little-endian)
        Assert.Equal(0x12, memory[0x0FFF]); // High byte of value stored
        Assert.Equal(1u, cycles); // Register mode cycle count
    }

    [Fact]
    public void Execute_ByteOperation_SignExtendsValue()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PushInstruction(0x1244, RegisterName.R4, AddressingMode.Register, true);

        registerFile.WriteRegister(RegisterName.R4, 0x12FF); // Negative byte value
        registerFile.SetStackPointer(0x1000);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());
        Assert.Equal(0xFF, memory[0x0FFE]); // Low byte
        Assert.Equal(0xFF, memory[0x0FFF]); // High byte (sign-extended)
    }

    [Fact]
    public void Execute_ByteOperationPositive_ZeroExtendsValue()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PushInstruction(0x1244, RegisterName.R4, AddressingMode.Register, true);

        registerFile.WriteRegister(RegisterName.R4, 0x1234); // Positive byte value
        registerFile.SetStackPointer(0x1000);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());
        Assert.Equal(0x34, memory[0x0FFE]); // Low byte
        Assert.Equal(0x00, memory[0x0FFF]); // High byte (positive, so no sign extension)
    }

    [Fact]
    public void Execute_ImmediateMode_PushesImmediateValueAndUsesExtensionWord()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PushInstruction(0x1230, RegisterName.R0, AddressingMode.Immediate, false);

        registerFile.SetStackPointer(0x1000);
        ushort[] extensionWords = new ushort[] { 0x5678 };

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());
        Assert.Equal(0x78, memory[0x0FFE]); // Low byte of immediate value
        Assert.Equal(0x56, memory[0x0FFF]); // High byte of immediate value
        Assert.Equal(1u, cycles); // Immediate mode cycle count
    }

    [Fact]
    public void Execute_IndirectMode_PushesIndirectValue()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PushInstruction(0x1284, RegisterName.R4, AddressingMode.Indirect, false);

        registerFile.WriteRegister(RegisterName.R4, 0x2000); // Address to read from
        registerFile.SetStackPointer(0x1000);

        // Setup memory at address 0x2000
        memory[0x2000] = 0xAB; // Low byte
        memory[0x2001] = 0xCD; // High byte

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());
        Assert.Equal(0xAB, memory[0x0FFE]); // Low byte of indirect value
        Assert.Equal(0xCD, memory[0x0FFF]); // High byte of indirect value
        Assert.Equal(3u, cycles); // Indirect mode cycle count
    }

    [Fact]
    public void Execute_IndirectAutoIncrementMode_PushesValueAndIncrementsRegister()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PushInstruction(0x12C4, RegisterName.R4, AddressingMode.IndirectAutoIncrement, false);

        registerFile.WriteRegister(RegisterName.R4, 0x2000); // Address to read from
        registerFile.SetStackPointer(0x1000);

        // Setup memory at address 0x2000
        memory[0x2000] = 0xEF; // Low byte
        memory[0x2001] = 0x12; // High byte

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());
        Assert.Equal(0xEF, memory[0x0FFE]); // Low byte of value
        Assert.Equal(0x12, memory[0x0FFF]); // High byte of value
        Assert.Equal(0x2002, registerFile.ReadRegister(RegisterName.R4)); // Register incremented by 2
        Assert.Equal(3u, cycles); // Indirect auto-increment mode cycle count
    }

    [Fact]
    public void Execute_StackOverflow_ThrowsException()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PushInstruction(0x1204, RegisterName.R4, AddressingMode.Register, false);

        registerFile.WriteRegister(RegisterName.R4, 0x1234);
        registerFile.SetStackPointer(0x0001); // SP that would underflow

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
        Assert.Contains("Stack overflow", exception.Message);
    }

    [Fact]
    public void Execute_StackMemoryOutOfBounds_ThrowsException()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[4]; // Very limited memory (addresses 0-3)
        var instruction = new PushInstruction(0x1204, RegisterName.R4, AddressingMode.Register, false);

        registerFile.WriteRegister(RegisterName.R4, 0x1234);
        registerFile.SetStackPointer(0x0006); // SP that would decrement to 0x0004, accessing 0x0004 and 0x0005 (beyond bounds)

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
        Assert.Contains("memory beyond bounds", exception.Message);
    }

    [Fact]
    public void ToString_WordOperation_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("PUSH R4", result);
    }

    [Fact]
    public void ToString_ByteOperation_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new PushInstruction(
            0x1244,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("PUSH.B R4", result);
    }

    [Fact]
    public void ToString_IndirectMode_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new PushInstruction(
            0x1284,
            RegisterName.R5,
            AddressingMode.Indirect,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("PUSH @R5", result);
    }
}
