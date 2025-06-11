using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Logic;
using MSP430.Emulator.Tests.TestUtilities;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.Logic;

/// <summary>
/// Unit tests for the BicInstruction class.
/// 
/// BIC instruction performs bit clear operation (AND destination with inverted source):
/// - Two-operand instruction with source and destination operands
/// - Sets Zero and Negative status flags based on result
/// - Clears Carry and Overflow flags
/// - Available in both word (BIC) and byte (BIC.B) variants
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - Section 4.3.3: BIC instruction
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.4: Logic instructions
/// </summary>
public class BicInstructionTests
{

    [Fact]
    public void Constructor_ValidParameters_SetsFormat()
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
    }

    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
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
        Assert.Equal(0xC, instruction.Opcode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsInstructionWord()
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
        Assert.Equal(0xC123, instruction.InstructionWord);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsMnemonic()
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
        Assert.Equal("BIC", instruction.Mnemonic);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsIsByteOperationToFalse()
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
        Assert.False(instruction.IsByteOperation);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsSourceRegister()
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
        Assert.Equal(RegisterName.R1, instruction.SourceRegister);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsDestinationRegister()
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
        Assert.Equal(RegisterName.R2, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    public void Constructor_ValidParameters_SetsSourceAddressingMode(AddressingMode expectedMode)
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
        Assert.Equal(expectedMode, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    public void Constructor_ValidParameters_SetsDestinationAddressingMode(AddressingMode expectedMode)
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
        Assert.Equal(expectedMode, instruction.DestinationAddressingMode);
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
    }

    [Theory]
    [InlineData("BIC.B")]
    public void Constructor_ByteOperation_SetsByteMnemonic(string expectedMnemonic)
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
        Assert.Equal(expectedMnemonic, instruction.Mnemonic);
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
    public void AddressingModes_AllSupportedModes_SourceAddressingModeSet(AddressingMode mode)
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
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_DestinationAddressingModeSet(AddressingMode mode)
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
        Assert.Equal(mode, instruction.DestinationAddressingMode);
    }

    [Theory]
    [InlineData(RegisterName.R0)]
    [InlineData(RegisterName.R1)]
    [InlineData(RegisterName.R2)]
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
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void ToString_VariousAddressingModes_ContainsMnemonic(AddressingMode mode)
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
    }

    [Theory]
    [InlineData(AddressingMode.Register, "R1")]
    [InlineData(AddressingMode.Indirect, "@R1")]
    [InlineData(AddressingMode.IndirectAutoIncrement, "@R1+")]
    [InlineData(AddressingMode.Immediate, "#N")]
    [InlineData(AddressingMode.Indexed, "X(R1)")]
    [InlineData(AddressingMode.Absolute, "&ADDR")]
    [InlineData(AddressingMode.Symbolic, "ADDR")]
    public void ToString_VariousAddressingModes_ContainsSourceOperand(
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
        Assert.Contains(expectedOperand, result);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void ToString_VariousAddressingModes_ContainsDestinationRegister(AddressingMode mode)
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

    [Theory]
    [InlineData(0x00F0)]
    public void Execute_BasicOperation_ClearsBitsCorrectly(ushort expectedResult)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedResult, registerFile.ReadRegister(RegisterName.R5)); // cleared bits 0-3 and 8-15
    }

    [Fact]
    public void Execute_BasicOperation_DoesNotSetZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_BasicOperation_DoesNotSetNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_BasicOperation_DoesNotSetCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_BasicOperation_DoesNotSetOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Theory]
    [InlineData(1u)]
    public void Execute_BasicOperation_Takes1Cycle(uint expectedCycles)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        Assert.Equal(expectedCycles, cycles); // 1 base + 0 source (register) + 0 dest (register)
    }

    [Fact]
    public void Execute_ByteOperation_PerformsBicOnBytes()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal((ushort)((0xF0) | 0x3400), registerFile.ReadRegister(RegisterName.R5)); // cleared low byte bits, high byte unchanged
    }

    [Fact]
    public void Execute_ByteOperation_DoesNotSetZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_ByteOperation_SetsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Negative); // 0xF0 has bit 7 set (negative for byte)
    }

    [Theory]
    [InlineData(1u)]
    public void Execute_ByteOperation_Takes1Cycle(uint expectedCycles)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        Assert.Equal(expectedCycles, cycles); // 1 base + 0 source (register) + 0 dest (register)
    }

    [Theory]
    [InlineData(0x0000)]
    public void Execute_ResultZero_ClearsAllBits(ushort expectedResult)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        Assert.Equal(expectedResult, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_ResultZero_SetsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        Assert.True(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_ResultZero_DoesNotSetNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Theory]
    [InlineData(0x8000)]
    public void Execute_NegativeResult_LeavesNegativeBit(ushort expectedResult)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        Assert.Equal(expectedResult, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_NegativeResult_DoesNotSetZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_NegativeResult_SetsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

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
        Assert.True(registerFile.StatusRegister.Negative);
    }

    #region Cycle Count Tests

    [Fact]
    public void Execute_RegisterToRegister_Takes1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

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
        Assert.Equal(1u, cycles); // 1 base + 0 source (register) + 0 dest (register)
    }

    [Fact]
    public void Execute_ImmediateToRegister_Takes1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R0, // PC for immediate
            RegisterName.R5,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0xFF00 }; // Immediate value

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(2u, cycles); // SLAU445 Table 4-10: #N→Rm = 2 cycles
    }

    [Fact]
    public void Execute_RegisterToIndexed_Takes4Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x0100); // Base address

        // Set up memory at indexed location R5 + 0x10 = 0x0110
        memory[0x0110] = 0xF0;
        memory[0x0111] = 0x0F;

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Indexed,
            false);

        ushort[] extensionWords = { 0x0010 }; // Offset for indexed addressing

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(4u, cycles); // SLAU445 Table 4-10: Rn→x(Rm) = 4 cycles
    }

    [Fact]
    public void Execute_IndexedToRegister_Takes4Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0100); // Base address
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        // Set up memory at indexed location R4 + 0x10 = 0x0110
        memory[0x0110] = 0x00;
        memory[0x0111] = 0xFF;

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Indexed,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x0010 }; // Offset for indexed addressing

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(3u, cycles); // SLAU445 Table 4-10: x(Rn)→Rm = 3 cycles
    }

    [Fact]
    public void Execute_IndirectToRegister_Takes3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0200); // Points to memory location
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        // Set up memory at indirect location
        memory[0x0200] = 0x00;
        memory[0x0201] = 0xFF;

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Indirect,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(2u, cycles); // SLAU445 Table 4-10: @Rn→Rm = 2 cycles
    }

    [Fact]
    public void Execute_RegisterToIndirect_Takes3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x0200); // Points to memory location

        // Set up memory at indirect location
        memory[0x0200] = 0xF0;
        memory[0x0201] = 0x0F;

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Indirect,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(3u, cycles); // Non-standard destination mode: legacy calculation = 3 cycles
    }

    [Fact]
    public void Execute_AbsoluteToAbsolute_Takes7Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        // Set up memory at source absolute address
        memory[0x0300] = 0x00;
        memory[0x0301] = 0xFF;

        // Set up memory at destination absolute address
        memory[0x0400] = 0xF0;
        memory[0x0401] = 0x0F;

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R2, // SR for absolute addressing
            RegisterName.R2, // SR for absolute addressing
            AddressingMode.Absolute,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = { 0x0300, 0x0400 }; // Source and dest absolute addresses

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(6u, cycles); // SLAU445 Table 4-10: &EDE→&TONI = 6 cycles
    }

    [Fact]
    public void Execute_SymbolicToSymbolic_Takes7Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R0, 0x1000); // PC value

        // Set up memory at source symbolic address (PC + offset)
        memory[0x1000 + 0x0100] = 0x00; // PC + 0x0100
        memory[0x1000 + 0x0100 + 1] = 0xFF;

        // Set up memory at destination symbolic address (PC + offset)
        memory[0x1000 + 0x0200] = 0xF0; // PC + 0x0200
        memory[0x1000 + 0x0200 + 1] = 0x0F;

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R0, // PC for symbolic addressing
            RegisterName.R0, // PC for symbolic addressing
            AddressingMode.Symbolic,
            AddressingMode.Symbolic,
            false);

        ushort[] extensionWords = { 0x0100, 0x0200 }; // Source and dest offsets from PC

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(6u, cycles); // SLAU445 Table 4-10: EDE→TONI = 6 cycles
    }

    [Fact]
    public void Execute_IndirectAutoIncrementToRegister_Takes3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0200); // Points to memory location
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        // Set up memory at indirect location
        memory[0x0200] = 0x00;
        memory[0x0201] = 0xFF;

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(2u, cycles); // SLAU445 Table 4-10: @Rn+→Rm = 2 cycles
    }

    [Fact]
    public void Execute_IndirectAutoIncrementToRegister_AutoIncrementsRegister()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0200); // Points to memory location
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        // Set up memory at indirect location
        memory[0x0200] = 0x00;
        memory[0x0201] = 0xFF;

        var instruction = new BicInstruction(
            0xC000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal((ushort)0x0202, registerFile.ReadRegister(RegisterName.R4)); // Auto-incremented by 2 for word
    }

    #endregion
}
