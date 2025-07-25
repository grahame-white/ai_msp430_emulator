using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.Arithmetic;

/// <summary>
/// Unit tests for the DecInstruction class.
/// 
/// DEC instruction performs decrement operation (subtract 1 from destination):
/// - Single-operand instruction operating on destination only
/// - Sets status flags (Zero, Negative, Carry, Overflow) based on result
/// - Supports all addressing modes except immediate for destination
/// - Available in both word (DEC) and byte (DEC.B) variants
/// - Opcode: 0xB (Format I)
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - Section 4.3.17: DEC instruction
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.4: Single-operand instructions
/// </summary>
public class DecInstructionTests
{
    [Theory]
    [InlineData(0xB123)]
    [InlineData(0xB456)]
    [InlineData(0xB789)]
    public void Constructor_ValidParameters_SetsInstructionWord(ushort instructionWord)
    {
        var instruction = new DecInstruction(instructionWord, RegisterName.R3, AddressingMode.Register, false);
        Assert.Equal(instructionWord, instruction.InstructionWord);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_ValidParameters_SetsByteOperation(bool isByteOp)
    {
        var instruction = new DecInstruction(0xB123, RegisterName.R3, AddressingMode.Register, isByteOp);
        Assert.Equal(isByteOp, instruction.IsByteOperation);
    }

    [Theory]
    [InlineData(false, "DEC")]
    [InlineData(true, "DEC.B")]
    public void Constructor_ByteOperationFlag_SetsMnemonic(bool isByteOperation, string expectedMnemonic)
    {
        var instruction = new DecInstruction(0xB563, RegisterName.R5, AddressingMode.Register, isByteOperation);
        Assert.Equal(expectedMnemonic, instruction.Mnemonic);
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
    public void Properties_VariousRegisters_ReturnCorrectDestinationRegister(RegisterName dest)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            dest,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(dest, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(RegisterName.R0)]
    [InlineData(RegisterName.R7)]
    [InlineData(RegisterName.R3)]
    [InlineData(RegisterName.R5)]
    public void Properties_VariousRegisters_SourceRegisterIsNull(RegisterName dest)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            dest,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Null(instruction.SourceRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_ReturnCorrectDestinationAddressingMode(AddressingMode mode)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            RegisterName.R4,
            mode,
            false);

        // Act & Assert
        Assert.Equal(mode, instruction.DestinationAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_SourceAddressingModeIsNull(AddressingMode mode)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            RegisterName.R4,
            mode,
            false);

        // Act & Assert
        Assert.Null(instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R7)]
    [InlineData(RegisterName.R6)]
    public void Properties_DifferentRegisters_ReturnCorrectDestination(RegisterName expected)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            expected,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(expected, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(RegisterName.R4, RegisterName.R5)]
    [InlineData(RegisterName.R7, RegisterName.R3)]
    [InlineData(RegisterName.R6, RegisterName.R7)]
    public void Properties_DifferentRegisters_DoesNotReturnOtherRegister(RegisterName expected, RegisterName other)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            expected,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.NotEqual(other, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(0xB)]
    public void Opcode_IsCorrectValue(byte expectedOpcode)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(expectedOpcode, instruction.Opcode);
    }

    [Theory]
    [InlineData(InstructionFormat.FormatI)]
    public void Format_IsFormatI(InstructionFormat expectedFormat)
    {
        // Arrange
        var instruction = new DecInstruction(
            0xB000,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(expectedFormat, instruction.Format);
    }

    [Fact]
    public void Execute_RegisterMode_DecrementsValue()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1235);

        var instruction = new DecInstruction(
            0xB505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x1234, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_RegisterMode_ClearsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1235);

        var instruction = new DecInstruction(
            0xB505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_RegisterMode_ClearsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1235);

        var instruction = new DecInstruction(
            0xB505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_RegisterMode_SetsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1235);

        var instruction = new DecInstruction(
            0xB505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Carry); // Carry set when not decrementing 0
    }

    [Fact]
    public void Execute_RegisterMode_ClearsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1235);

        var instruction = new DecInstruction(
            0xB505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_RegisterMode_Returns1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_DecrementsLowByteOnly()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB35);

        var instruction = new DecInstruction(
            0xB543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0xAB34, registerFile.ReadRegister(RegisterName.R3));
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_ClearsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB35);

        var instruction = new DecInstruction(
            0xB543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_ClearsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB35);

        var instruction = new DecInstruction(
            0xB543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_SetsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB35);

        var instruction = new DecInstruction(
            0xB543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_ClearsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB35);

        var instruction = new DecInstruction(
            0xB543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_Returns1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_DecrementOne_DecrementsToZero()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
    }

    [Fact]
    public void Execute_DecrementOne_SetsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.True(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_DecrementOne_ClearsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_DecrementOne_SetsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.True(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_DecrementOne_ClearsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_DecrementZero_DecrementsToFFFF()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
    }

    [Fact]
    public void Execute_DecrementZero_ClearsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_DecrementZero_SetsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.True(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_DecrementZero_ClearsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.False(registerFile.StatusRegister.Carry); // Carry clear when decrementing 0
    }

    [Fact]
    public void Execute_DecrementZero_ClearsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_NegativeOverflow_DecrementsTo7FFF()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
    }

    [Fact]
    public void Execute_NegativeOverflow_ClearsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_NegativeOverflow_ClearsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_NegativeOverflow_SetsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.True(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_NegativeOverflow_SetsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.True(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_ByteNegativeOverflow_DecrementsTo127F()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
    }

    [Fact]
    public void Execute_ByteNegativeOverflow_ClearsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_ByteNegativeOverflow_ClearsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_ByteNegativeOverflow_SetsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.True(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_ByteNegativeOverflow_SetsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.True(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_AbsoluteMode_DecrementsMemoryValue()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x34, memory[0x0200]);
    }

    [Fact]
    public void Execute_AbsoluteMode_HighByteUnchanged()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x12, memory[0x0201]); // Should be 0x1234 in little-endian
    }

    [Fact]
    public void Execute_AbsoluteMode_ClearsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_AbsoluteMode_ClearsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_AbsoluteMode_SetsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_AbsoluteMode_ClearsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_AbsoluteMode_Returns4Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.Equal(4u, cycles);
    }

    [Fact]
    public void Execute_IndirectMode_DecrementsMemoryValueLowByte()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x00, memory[0x0300]);
    }

    [Fact]
    public void Execute_IndirectMode_DecrementsMemoryValueHighByte()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x80, memory[0x0301]); // Should be 0x8000 in little-endian
    }

    [Fact]
    public void Execute_IndirectMode_RegisterUnchanged()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x0300, registerFile.ReadRegister(RegisterName.R7)); // R7 unchanged
    }

    [Fact]
    public void Execute_IndirectMode_ClearsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_IndirectMode_SetsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_IndirectMode_SetsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_IndirectMode_ClearsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_IndirectMode_Returns3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
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
        Assert.Equal(3u, cycles);
    }
}
