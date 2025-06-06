using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.Arithmetic;

/// <summary>
/// Unit tests for the AddInstruction class.
/// </summary>
public class AddInstructionTests
{

    [Fact]
    public void Constructor_ValidParameters_CreatesInstruction()
    {
        // Arrange & Act
        var instruction = new AddInstruction(
            0x5123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
        Assert.Equal(0x5, instruction.Opcode);
        Assert.Equal(0x5123, instruction.InstructionWord);
        Assert.Equal("ADD", instruction.Mnemonic);
        Assert.False(instruction.IsByteOperation);
        Assert.Equal(RegisterName.R1, instruction.SourceRegister);
        Assert.Equal(RegisterName.R4, instruction.DestinationRegister);
        Assert.Equal(AddressingMode.Register, instruction.SourceAddressingMode);
        Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsByteFlag()
    {
        // Arrange & Act
        var instruction = new AddInstruction(
            0x5563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Assert
        Assert.True(instruction.IsByteOperation);
        Assert.Equal("ADD.B", instruction.Mnemonic);
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
    public void Properties_VariousRegisters_ReturnCorrectValues(RegisterName source, RegisterName dest)
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
    public void AddressingModes_AllSupportedModes_ReturnCorrectValues(AddressingMode mode)
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
        Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Execute_RegisterToRegister_AddsValues()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0x1234); // Use R5 instead of R1 to avoid alignment
        registerFile.WriteRegister(RegisterName.R4, 0x5678); // Use R4 to avoid status register conflicts

        var instruction = new AddInstruction(
            0x5054,
            RegisterName.R5, // Use R5 instead of R1
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal((ushort)0x68AC, registerFile.ReadRegister(RegisterName.R4)); // 0x1234 + 0x5678 = 0x68AC
        Assert.Equal(1u, cycles);
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.False(registerFile.StatusRegister.Carry);
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_Addition_SetsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0xFFFF); // Use R5 instead of R1 to avoid alignment
        registerFile.WriteRegister(RegisterName.R4, 0x0001);

        var instruction = new AddInstruction(
            0x5054,
            RegisterName.R5, // Use R5 instead of R1
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal((ushort)0x0000, registerFile.ReadRegister(RegisterName.R4)); // 0xFFFF + 0x0001 = 0x10000 (truncated to 0x0000)
        Assert.True(registerFile.StatusRegister.Carry);
        Assert.True(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_PositiveOverflow_SetsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0x7FFF); // Maximum positive signed 16-bit value
        registerFile.WriteRegister(RegisterName.R4, 0x0001);

        var instruction = new AddInstruction(
            0x5054,
            RegisterName.R5, // Use R5 instead of R1
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal((ushort)0x8000, registerFile.ReadRegister(RegisterName.R4)); // 0x7FFF + 0x0001 = 0x8000 (negative)
        Assert.True(registerFile.StatusRegister.Overflow);
        Assert.True(registerFile.StatusRegister.Negative);
    }

    // Cycle count tests for key addressing mode combinations
    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, 1u)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, 1u)]
    [InlineData(AddressingMode.Register, AddressingMode.Indexed, 4u)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute, 7u)]
    public void Execute_CycleCounts_AreCorrect(AddressingMode sourceMode, AddressingMode destMode, uint expectedCycles)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.WriteRegister(RegisterName.R4, 0x2000);

        var instruction = new AddInstruction(
            0x5000,
            RegisterName.R1,
            RegisterName.R4,
            sourceMode,
            destMode,
            false);

        // Set up extension words for modes that need them
        ushort[] extensionWords = sourceMode switch
        {
            AddressingMode.Immediate when destMode == AddressingMode.Register => [0x0100],
            AddressingMode.Absolute when destMode == AddressingMode.Absolute => [0x1000, 0x2000],
            AddressingMode.Register when destMode == AddressingMode.Indexed => [0x0010],
            _ => Array.Empty<ushort>()
        };

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(expectedCycles, cycles);
    }
}
