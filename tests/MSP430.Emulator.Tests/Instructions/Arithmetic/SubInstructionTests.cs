using System;
using System.Collections.Generic;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.Arithmetic;

/// <summary>
/// Unit tests for the SubInstruction class.
/// </summary>
public class SubInstructionTests
{

    [Fact]
    public void Constructor_ValidParameters_SetsFormat()
    {
        var instruction = new SubInstruction(
            0x8123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        var instruction = new SubInstruction(
            0x8123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        Assert.Equal(0x8, instruction.Opcode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsInstructionWord()
    {
        var instruction = new SubInstruction(
            0x8123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        Assert.Equal(0x8123, instruction.InstructionWord);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsMnemonic()
    {
        var instruction = new SubInstruction(
            0x8123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        Assert.Equal("SUB", instruction.Mnemonic);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsByteOperationFlag()
    {
        var instruction = new SubInstruction(
            0x8123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        Assert.False(instruction.IsByteOperation);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsSourceRegister()
    {
        var instruction = new SubInstruction(
            0x8123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        Assert.Equal(RegisterName.R1, instruction.SourceRegister);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsDestinationRegister()
    {
        var instruction = new SubInstruction(
            0x8123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        Assert.Equal(RegisterName.R4, instruction.DestinationRegister);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsSourceAddressingMode()
    {
        var instruction = new SubInstruction(
            0x8123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        Assert.Equal(AddressingMode.Register, instruction.SourceAddressingMode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsDestinationAddressingMode()
    {
        var instruction = new SubInstruction(
            0x8123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsByteFlag()
    {
        var instruction = new SubInstruction(
            0x8563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        Assert.True(instruction.IsByteOperation);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsByteOperationMnemonic()
    {
        var instruction = new SubInstruction(
            0x8563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        Assert.Equal("SUB.B", instruction.Mnemonic);
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
        var instruction = new SubInstruction(
            0x8000,
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
        var instruction = new SubInstruction(
            0x8000,
            register,
            RegisterName.R1,
            mode,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal($"SUB {expectedOperand}, R1", result);
    }

    [Fact]
    public void ToString_ByteOperation_IncludesBSuffix()
    {
        // Arrange
        var instruction = new SubInstruction(
            0x8563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("SUB.B R5, R6", result);
    }

    [Theory]
    [InlineData(RegisterName.R0, RegisterName.R1)]
    [InlineData(RegisterName.R15, RegisterName.R4)]
    [InlineData(RegisterName.R3, RegisterName.R4)]
    [InlineData(RegisterName.R5, RegisterName.R6)]
    public void Properties_VariousRegisters_ReturnsCorrectSourceRegister(RegisterName source, RegisterName dest)
    {
        var instruction = new SubInstruction(
            0x8000,
            source,
            dest,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        Assert.Equal(source, instruction.SourceRegister);
    }

    [Theory]
    [InlineData(RegisterName.R0, RegisterName.R1)]
    [InlineData(RegisterName.R15, RegisterName.R4)]
    [InlineData(RegisterName.R3, RegisterName.R4)]
    [InlineData(RegisterName.R5, RegisterName.R6)]
    public void Properties_VariousRegisters_ReturnsCorrectDestinationRegister(RegisterName source, RegisterName dest)
    {
        var instruction = new SubInstruction(
            0x8000,
            source,
            dest,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

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
    public void AddressingModes_AllSupportedModes_ReturnsCorrectSourceAddressingMode(AddressingMode mode)
    {
        var instruction = new SubInstruction(
            0x8000,
            RegisterName.R1,
            RegisterName.R4,
            mode,
            AddressingMode.Register,
            false);

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
    public void AddressingModes_AllSupportedModes_ReturnsCorrectDestinationAddressingMode(AddressingMode mode)
    {
        var instruction = new SubInstruction(
            0x8000,
            RegisterName.R1,
            RegisterName.R4,
            mode,
            AddressingMode.Register,
            false);

        Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Execute_RegisterToRegister_SubtractsCorrectResult()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234); // Source
        registerFile.WriteRegister(RegisterName.R4, 0x5678); // Destination

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x4444, registerFile.ReadRegister(RegisterName.R4)); // 0x5678 - 0x1234 = 0x4444
    }

    [Fact]
    public void Execute_RegisterToRegister_Takes1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234); // Source
        registerFile.WriteRegister(RegisterName.R4, 0x5678); // Destination

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(1u, cycles);
    }

    [Theory]
    [InlineData(false, false, false, false)] // Zero, Negative, Carry, Overflow
    public void Execute_RegisterToRegister_SetsCorrectFlags(bool expectedZero, bool expectedNegative, bool expectedCarry, bool expectedOverflow)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234); // Source
        registerFile.WriteRegister(RegisterName.R4, 0x5678); // Destination

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
        Assert.Equal(expectedCarry, registerFile.StatusRegister.Carry);
        Assert.Equal(expectedOverflow, registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_SubtractLargerFromSmaller_GivesCorrectResult()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x5678); // Source (larger)
        registerFile.WriteRegister(RegisterName.R4, 0x1234); // Destination (smaller)

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0xBBBC, registerFile.ReadRegister(RegisterName.R4)); // 0x1234 - 0x5678 = 0xBBBC (with carry)
    }

    [Fact]
    public void Execute_SubtractLargerFromSmaller_SetsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x5678); // Source (larger)
        registerFile.WriteRegister(RegisterName.R4, 0x1234); // Destination (smaller)

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_SubtractLargerFromSmaller_SetsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x5678); // Source (larger)
        registerFile.WriteRegister(RegisterName.R4, 0x1234); // Destination (smaller)

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_SubtractToZero_GivesZeroResult()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0000, registerFile.ReadRegister(RegisterName.R4)); // 0x1234 - 0x1234 = 0x0000
    }

    [Fact]
    public void Execute_SubtractToZero_SetsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_SubtractToZero_DoesNotSetCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_OverflowCondition_GivesCorrectResult()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x8000); // Negative source
        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Positive destination

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0xFFFF, registerFile.ReadRegister(RegisterName.R4)); // 0x7FFF - 0x8000 = 0xFFFF
    }

    [Fact]
    public void Execute_OverflowCondition_SetsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x8000); // Negative source
        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Positive destination

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_OverflowCondition_SetsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x8000); // Negative source
        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Positive destination

        var instruction = new SubInstruction(
            0x8014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_ByteOperation_SubtractsLowBytesOnly()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x5678);

        var instruction = new SubInstruction(
            0x8552,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x5644, registerFile.ReadRegister(RegisterName.R4)); // High byte unchanged, low byte: 0x78 - 0x34 = 0x44
    }

    [Fact]
    public void Execute_ByteOperation_Takes1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x5678);

        var instruction = new SubInstruction(
            0x8552,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(1u, cycles);
    }

    /// <summary>
    /// Tests all valid source/destination addressing mode combinations for SUB instruction.
    /// Based on MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3: "CPU"
    /// Testing all 42 valid combinations (7 source × 6 destination modes).
    /// </summary>
    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register)]
    [InlineData(AddressingMode.Register, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Register, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Register, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Register, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Register, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Immediate, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Indirect, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Register)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Indexed)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Absolute)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Indexed, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Register)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Absolute, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Register)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Symbolic)]
    public void Execute_AllAddressingModeCombinations_ExecutesSuccessfully(AddressingMode sourceMode, AddressingMode destMode)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.WriteRegister(RegisterName.R4, 0x2000);
        registerFile.SetProgramCounter(0x8000);

        // Set up memory for addressing modes that access memory
        memory[0x1000] = 0x34; // For indirect modes
        memory[0x1001] = 0x12;
        memory[0x2000] = 0x78;
        memory[0x2001] = 0x56;
        memory[0x1010] = 0xBC; // For indexed modes
        memory[0x1011] = 0x9A;
        memory[0x3000] = 0xEF; // For absolute modes
        memory[0x3001] = 0xCD;

        var instruction = new SubInstruction(
            0x8000,
            RegisterName.R1,
            RegisterName.R4,
            sourceMode,
            destMode,
            false);

        // Set up extension words based on addressing modes
        List<ushort> extensionWords = [];
        if (sourceMode == AddressingMode.Immediate)
        {
            extensionWords.Add(0x0100);
        }

        if (sourceMode == AddressingMode.Indexed)
        {
            extensionWords.Add(0x0010);
        }

        if (sourceMode == AddressingMode.Absolute)
        {
            extensionWords.Add(0x3000);
        }

        if (sourceMode == AddressingMode.Symbolic)
        {
            extensionWords.Add(0x1000);
        }

        if (destMode == AddressingMode.Indexed)
        {
            extensionWords.Add(0x0010);
        }

        if (destMode == AddressingMode.Absolute)
        {
            extensionWords.Add(0x3000);
        }

        if (destMode == AddressingMode.Symbolic)
        {
            extensionWords.Add(0x1000);
        }

        // Act - instruction should execute without throwing exceptions
        uint cycles = instruction.Execute(registerFile, memory, extensionWords.ToArray());

        // Assert - verify instruction executed and returned valid cycle count
        Assert.True(cycles > 0, $"Expected positive cycle count for {sourceMode} to {destMode}");
        Assert.True(cycles <= 10, $"Cycle count {cycles} seems too high for {sourceMode} to {destMode}");
    }

    /// <summary>
    /// Tests cycle counts for SUB instruction addressing mode combinations.
    /// Based on MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3.4: "Instruction Set"
    /// Cycle counts per TI specification: base (1) + source cycles + destination cycles.
    /// </summary>
    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, 1u)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, 1u)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Register, 3u)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Register, 3u)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Register, 4u)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Register, 4u)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Register, 4u)]
    [InlineData(AddressingMode.Register, AddressingMode.Indirect, 3u)]
    [InlineData(AddressingMode.Register, AddressingMode.IndirectAutoIncrement, 3u)]
    [InlineData(AddressingMode.Register, AddressingMode.Indexed, 4u)]
    [InlineData(AddressingMode.Register, AddressingMode.Absolute, 4u)]
    [InlineData(AddressingMode.Register, AddressingMode.Symbolic, 4u)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Indexed, 4u)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Absolute, 4u)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indexed, 7u)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute, 7u)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Symbolic, 7u)]
    public void Execute_CycleCounts_AreCorrect(AddressingMode sourceMode, AddressingMode destMode, uint expectedCycles)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.WriteRegister(RegisterName.R4, 0x2000);
        registerFile.SetProgramCounter(0x8000);

        // Set up memory for addressing modes that access memory
        memory[0x1000] = 0x34; // For indirect modes
        memory[0x1001] = 0x12;
        memory[0x2000] = 0x78;
        memory[0x2001] = 0x56;
        memory[0x1010] = 0xBC; // For indexed modes  
        memory[0x1011] = 0x9A;
        memory[0x3000] = 0xEF; // For absolute modes
        memory[0x3001] = 0xCD;

        var instruction = new SubInstruction(
            0x8000,
            RegisterName.R1,
            RegisterName.R4,
            sourceMode,
            destMode,
            false);

        // Set up extension words based on addressing modes
        List<ushort> extensionWords = [];
        if (sourceMode == AddressingMode.Immediate)
        {
            extensionWords.Add(0x0100);
        }

        if (sourceMode == AddressingMode.Indexed)
        {
            extensionWords.Add(0x0010);
        }

        if (sourceMode == AddressingMode.Absolute)
        {
            extensionWords.Add(0x3000);
        }

        if (sourceMode == AddressingMode.Symbolic)
        {
            extensionWords.Add(0x1000);
        }

        if (destMode == AddressingMode.Indexed)
        {
            extensionWords.Add(0x0010);
        }

        if (destMode == AddressingMode.Absolute)
        {
            extensionWords.Add(0x3000);
        }

        if (destMode == AddressingMode.Symbolic)
        {
            extensionWords.Add(0x1000);
        }

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords.ToArray());

        // Assert
        Assert.Equal(expectedCycles, cycles);
    }
}
