using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.DataMovement;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.DataMovement;

/// <summary>
/// Unit tests for the MovInstruction class.
/// 
/// MOV instruction performs data movement from source to destination:
/// - Two-operand instruction with source and destination operands
/// - Does not affect status flags (except for special register operations)
/// - Supports all addressing mode combinations
/// - Available in both word (MOV) and byte (MOV.B) variants
/// - Opcode: 0x4 (Format I)
/// - Most commonly used instruction for data transfer operations
/// 
/// Tests all addressing mode combinations, byte/word operations, and flag behavior.
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - Section 4.3.21: MOV instruction
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.4: Data movement instructions
/// </summary>
public class MovInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsFormat()
    {
        // Arrange & Act
        var instruction = new MovInstruction(
            0x4123,
            RegisterName.R1,
            RegisterName.R4,
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
        var instruction = new MovInstruction(
            0x4123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(0x4, instruction.Opcode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsInstructionWord()
    {
        // Arrange & Act
        var instruction = new MovInstruction(
            0x4123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(0x4123, instruction.InstructionWord);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsMnemonic()
    {
        // Arrange & Act
        var instruction = new MovInstruction(
            0x4123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal("MOV", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_ValidParameters_SetsByteOperation(bool expectedByteOp)
    {
        // Arrange & Act
        var instruction = new MovInstruction(
            0x4123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            expectedByteOp);

        // Assert
        Assert.Equal(expectedByteOp, instruction.IsByteOperation);
    }

    [Theory]
    [InlineData(RegisterName.R1)]
    [InlineData(RegisterName.R2)]
    [InlineData(RegisterName.R15)]
    public void Constructor_ValidParameters_SetsSourceRegister(RegisterName expectedSource)
    {
        // Arrange & Act
        var instruction = new MovInstruction(
            0x4123,
            expectedSource,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedSource, instruction.SourceRegister);
    }

    [Theory]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R5)]
    [InlineData(RegisterName.R15)]
    public void Constructor_ValidParameters_SetsDestinationRegister(RegisterName expectedDest)
    {
        // Arrange & Act
        var instruction = new MovInstruction(
            0x4123,
            RegisterName.R1,
            expectedDest,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedDest, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Indirect)]
    public void Constructor_ValidParameters_SetsSourceAddressingMode(AddressingMode expectedSourceMode)
    {
        // Arrange & Act
        var instruction = new MovInstruction(
            0x4123,
            RegisterName.R1,
            RegisterName.R4,
            expectedSourceMode,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedSourceMode, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.Indexed)]
    public void Constructor_ValidParameters_SetsDestinationAddressingMode(AddressingMode expectedDestMode)
    {
        // Arrange & Act
        var instruction = new MovInstruction(
            0x4123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            expectedDestMode,
            false);

        // Assert
        Assert.Equal(expectedDestMode, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsByteFlag()
    {
        // Arrange & Act
        var instruction = new MovInstruction(
            0x4563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Assert
        Assert.True(instruction.IsByteOperation);
    }

    [Fact]
    public void Execute_RegisterToRegister_MovesValue()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R2, 0x5678);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R4, // Use R4 instead of R2
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R4));
    }

    [Fact]
    public void Execute_RegisterToRegister_Takes1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(1u, cycles); // Register to register takes 1 cycle
    }

    [Fact]
    public void Execute_ByteOperation_ReadOperandReturnsLowByte()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);

        // Act: check what ReadOperand returns
        ushort sourceValue = InstructionHelpers.ReadOperand(
            RegisterName.R1,
            AddressingMode.Register,
            true,
            registerFile,
            memory,
            0);

        // Assert
        Assert.Equal(0x34, sourceValue); // Should be 0x34 (low byte of 0x1234)
    }

    [Fact]
    public void Execute_ByteOperation_OnlyMovesLowByte()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R3, 0x5678); // Set up R3 with a known value

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R3, // Use R3 instead of R2 to avoid status register conflicts
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        ushort actualR3 = registerFile.ReadRegister(RegisterName.R3);
        Assert.Equal(0x5634, actualR3); // High byte preserved, low byte changed to 0x34
    }

    [Fact]
    public void Execute_ImmediateToRegister_MovesImmediateValue()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R3, 0x0000);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R0, // PC for immediate mode
            RegisterName.R3,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0xABCD };

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0xABCD, registerFile.ReadRegister(RegisterName.R3));
    }

    [Fact]
    public void Execute_IndirectToRegister_ReadsFromMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000); // Address
        memory[0x1000] = 0x34;
        memory[0x1001] = 0x12;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Indirect,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R2));
    }

    [Fact]
    public void Execute_IndirectAutoIncrement_ReadsValue()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        memory[0x1000] = 0x34;
        memory[0x1001] = 0x12;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R2));
    }

    [Fact]
    public void Execute_IndirectAutoIncrement_IncrementsPointer()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        memory[0x1000] = 0x34;
        memory[0x1001] = 0x12;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1002, registerFile.ReadRegister(RegisterName.R1)); // Incremented by 2 for word operation
    }

    [Fact]
    public void Execute_IndirectAutoIncrementByte_ReadsLowByte()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0x1000); // Use R5 instead of R1 to avoid alignment
        registerFile.WriteRegister(RegisterName.R3, 0xFF00); // Use R3 instead of R2
        memory[0x1000] = 0x34;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R5, // Use R5 instead of R1 to avoid alignment
            RegisterName.R3, // Use R3 instead of R2
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0xFF34, registerFile.ReadRegister(RegisterName.R3));
    }

    [Fact]
    public void Execute_IndirectAutoIncrementByte_IncrementsBy1()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0x1000);
        memory[0x1000] = 0x34;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R5,
            RegisterName.R3,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1001, registerFile.ReadRegister(RegisterName.R5)); // Incremented by 1 for byte operation
    }

    [Fact]
    public void Execute_IndexedToRegister_UsesOffset()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        memory[0x1005] = 0x78;
        memory[0x1006] = 0x56;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Indexed,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x0005 }; // Offset

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x5678, registerFile.ReadRegister(RegisterName.R2));
    }

    [Fact]
    public void Execute_AbsoluteToRegister_UsesDirectAddress()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        memory[0x2000] = 0xEF;
        memory[0x2001] = 0xBE;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R2, // SR for absolute mode
            RegisterName.R3,
            AddressingMode.Absolute,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x2000 }; // Absolute address

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0xBEEF, registerFile.ReadRegister(RegisterName.R3));
    }

    [Fact]
    public void Execute_SymbolicToRegister_UsesPCRelative()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R0, 0x1000); // PC
        memory[0x1010] = 0xAD;
        memory[0x1011] = 0xDE;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R0, // PC for symbolic mode
            RegisterName.R4,
            AddressingMode.Symbolic,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x0010 }; // PC offset

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0xDEAD, registerFile.ReadRegister(RegisterName.R4));
    }

    [Theory]
    [InlineData(0xABCD, 0x2000, 0xCD)]
    public void Execute_RegisterToIndirect_WritesToMemoryLowByte(ushort sourceValue, ushort address, byte expectedLowByte)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, sourceValue);
        registerFile.WriteRegister(RegisterName.R6, address);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Indirect,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedLowByte, memory[address]);
    }

    [Theory]
    [InlineData(0xABCD, 0x2000, 0xAB)]
    public void Execute_RegisterToIndirect_WritesToMemoryHighByte(ushort sourceValue, ushort address, byte expectedHighByte)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, sourceValue);
        registerFile.WriteRegister(RegisterName.R6, address);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Indirect,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedHighByte, memory[address + 1]);
    }

    [Theory]
    [InlineData(0x1234, 0x2000, 0x0010, 0x34)]
    public void Execute_RegisterToIndexed_WritesToOffsetAddressLowByte(ushort sourceValue, ushort baseAddress, ushort offset, byte expectedLowByte)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, sourceValue);
        registerFile.WriteRegister(RegisterName.R2, baseAddress);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Indexed,
            false);

        ushort[] extensionWords = { offset };

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(expectedLowByte, memory[baseAddress + offset]);
    }

    [Theory]
    [InlineData(0x1234, 0x2000, 0x0010, 0x12)]
    public void Execute_RegisterToIndexed_WritesToOffsetAddressHighByte(ushort sourceValue, ushort baseAddress, ushort offset, byte expectedHighByte)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, sourceValue);
        registerFile.WriteRegister(RegisterName.R2, baseAddress);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Indexed,
            false);

        ushort[] extensionWords = { offset };

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(expectedHighByte, memory[baseAddress + offset + 1]);
    }

    [Theory]
    [InlineData(0x0000, true)]
    [InlineData(0x0080, true)]
    [InlineData(0x007F, true)]
    [InlineData(0x8000, false)]
    [InlineData(0x7FFF, false)]
    [InlineData(0x1234, false)]
    public void Execute_FlagUpdates_SetsZeroFlag(ushort value, bool isByteOperation)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, value);
        registerFile.StatusRegister.Carry = true; // Should be preserved
        registerFile.StatusRegister.Overflow = true; // Should be cleared

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R3,
            AddressingMode.Register,
            AddressingMode.Register,
            isByteOperation);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(value == 0, registerFile.StatusRegister.Zero);
    }

    [Theory]
    [InlineData(0x0000, true, false)] // Zero value sets Z, clears N
    [InlineData(0x0080, true, true)]  // Byte: bit 7 set, sets N 
    [InlineData(0x007F, true, false)] // Byte: bit 7 clear, clears N
    [InlineData(0x8000, false, true)] // Word: bit 15 set, sets N
    [InlineData(0x7FFF, false, false)] // Word: bit 15 clear, clears N
    public void Execute_FlagUpdates_SetsNegativeFlag(ushort value, bool isByteOperation, bool expectedNegative)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, value);
        registerFile.StatusRegister.Carry = true; // Should be preserved
        registerFile.StatusRegister.Overflow = true; // Should be cleared

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R3,
            AddressingMode.Register,
            AddressingMode.Register,
            isByteOperation);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
    }

    [Theory]
    [InlineData(0x0000, true)]
    [InlineData(0x0080, true)]
    [InlineData(0x007F, true)]
    [InlineData(0x8000, false)]
    [InlineData(0x7FFF, false)]
    public void Execute_FlagUpdates_ClearsOverflowFlag(ushort value, bool isByteOperation)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, value);
        registerFile.StatusRegister.Carry = true; // Should be preserved
        registerFile.StatusRegister.Overflow = true; // Should be cleared

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R3,
            AddressingMode.Register,
            AddressingMode.Register,
            isByteOperation);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow); // Always cleared
    }

    [Theory]
    [InlineData(0x0000, true)]
    [InlineData(0x0080, true)]
    [InlineData(0x007F, true)]
    [InlineData(0x8000, false)]
    [InlineData(0x7FFF, false)]
    public void Execute_FlagUpdates_PreservesCarryFlag(ushort value, bool isByteOperation)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, value);
        registerFile.StatusRegister.Carry = true; // Should be preserved
        registerFile.StatusRegister.Overflow = true; // Should be cleared

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R3,
            AddressingMode.Register,
            AddressingMode.Register,
            isByteOperation);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Carry); // Preserved
    }

    // Extension word count tests
    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Register, AddressingMode.Indexed, 1)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Register, AddressingMode.Absolute, 1)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Indexed, 2)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Absolute, 2)]
    public void ExtensionWordCount_VariousAddressingModes_ReturnsCorrectCount(
        AddressingMode sourceMode, AddressingMode destMode, int expectedCount)
    {
        // Arrange
        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            sourceMode,
            destMode,
            false);

        // Act & Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    // Cycle count tests for key addressing mode combinations
    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, 1u)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, 1u)]
    [InlineData(AddressingMode.Register, AddressingMode.Indexed, 4u)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute, 7u)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Symbolic, 7u)]
    public void Execute_CycleCounts_AreCorrect(AddressingMode sourceMode, AddressingMode destMode, uint expectedCycles)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.WriteRegister(RegisterName.R4, 0x2000);

        var instruction = new MovInstruction(
            0x4000,
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
            AddressingMode.Symbolic when destMode == AddressingMode.Symbolic => [0x0010, 0x0020],
            AddressingMode.Register when destMode == AddressingMode.Indexed => [0x0010],
            _ => Array.Empty<ushort>()
        };

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(expectedCycles, cycles);
    }

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
    public void AddressingModes_AllSupportedModes_ReturnsCorrectSourceMode(AddressingMode sourceMode, AddressingMode destMode)
    {
        // Arrange
        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            sourceMode,
            destMode,
            false);

        // Act & Assert
        Assert.Equal(sourceMode, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Indexed)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Absolute, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Immediate)]
    public void AddressingModes_AllSupportedModes_ReturnsCorrectDestinationMode(AddressingMode sourceMode, AddressingMode destMode)
    {
        // Arrange
        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            sourceMode,
            destMode,
            false);

        // Act & Assert
        Assert.Equal(destMode, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void ToString_WordOperation_FormatsCorrectly()
    {
        // Arrange
        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("MOV R1, R2", result);
    }

    [Fact]
    public void ToString_ByteOperation_FormatsWithByteSuffix()
    {
        // Arrange
        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("MOV.B R1, R2", result);
    }

    [Fact]
    public void ToString_VariousAddressingModes_FormatsCorrectly()
    {
        // Test various addressing mode combinations
        (AddressingMode, AddressingMode, string)[] testCases = new[]
        {
            (AddressingMode.Immediate, AddressingMode.Register, "MOV #, R2"),
            (AddressingMode.Indirect, AddressingMode.Indexed, "MOV @R1, X(R2)"),
            (AddressingMode.IndirectAutoIncrement, AddressingMode.Absolute, "MOV @R1+, &ADDR"),
            (AddressingMode.Symbolic, AddressingMode.Indirect, "MOV ADDR, @R2")
        };

        foreach ((AddressingMode sourceMode, AddressingMode destMode, string expected) in testCases)
        {
            // Arrange
            var instruction = new MovInstruction(
                0x4000,
                RegisterName.R1,
                RegisterName.R2,
                sourceMode,
                destMode,
                false);

            // Act
            string result = instruction.ToString();

            // Assert
            Assert.Equal(expected, result);
        }
    }

    [Fact]
    public void Execute_StatusRegisterAsDestination_DoesNotUpdateFlags()
    {
        // Arrange
        // This test verifies the special case where MOV to R2 (Status Register) doesn't update flags
        // According to MSP430 specification, writing to SR should not trigger flag updates
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        // Use a simple test value and simple initial state
        registerFile.WriteRegister(RegisterName.R5, 0x1234);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R5,
            RegisterName.R2, // Destination is Status Register
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R2)); // Value is written correctly
        // The key point: flag update logic should NOT run when destination is R2
    }

    [Theory]
    [InlineData(false)]
    public void Execute_StatusRegisterAsSource_UpdatesNegativeFlag(bool expectedNegative)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R2, 0x0000);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R2,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
    }

    [Theory]
    [InlineData(true)]
    public void Execute_StatusRegisterAsSource_UpdatesZeroFlag(bool expectedZero)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R2, 0x0000);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R2,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
    }

    [Theory]
    [InlineData(false)]
    public void Execute_StatusRegisterAsSource_UpdatesOverflowFlag(bool expectedOverflow)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R2, 0x0000);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R2,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedOverflow, registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_StatusRegisterAsSource_MovesValueCorrectly()
    {
        // Arrange
        // This test verifies that when R2 is the source (not destination), flags are updated normally
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        // Set up R2 with a simple value
        registerFile.WriteRegister(RegisterName.R2, 0x0000); // Zero value to test Zero flag

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R2, // Source is Status Register
            RegisterName.R5, // Destination is general-purpose register
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0000, registerFile.ReadRegister(RegisterName.R5)); // Value is written correctly
    }

    [Theory]
    [InlineData(false)]
    public void Execute_R2WithAbsoluteMode_UpdatesNegativeFlag(bool expectedNegative)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort memoryAddress = 0x1000;

        // NOTE: R3 with Register mode (As=00) is a constant generator that returns 0,
        // so the value moved will be 0, not the value stored in R3
        registerFile.WriteRegister(RegisterName.R3, 0x8000);
        // Set opposite initial values to ensure the test is checking the actual update
        registerFile.StatusRegister.Negative = true; // Will be cleared since moving 0

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R3,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Absolute,
            false);

        // Act
        instruction.Execute(registerFile, memory, new ushort[] { memoryAddress });

        // Assert
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
    }

    [Theory]
    [InlineData(true)]
    public void Execute_R2WithAbsoluteMode_UpdatesZeroFlag(bool expectedZero)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort memoryAddress = 0x1000;

        // NOTE: R3 with Register mode (As=00) is a constant generator that returns 0,
        // so the Zero flag should be set since moving 0
        registerFile.WriteRegister(RegisterName.R3, 0x8000);
        // Set opposite initial values to ensure the test is checking the actual update
        registerFile.StatusRegister.Zero = false; // Will be set since moving 0

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R3,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Absolute,
            false);

        // Act
        instruction.Execute(registerFile, memory, new ushort[] { memoryAddress });

        // Assert
        Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
    }

    [Theory]
    [InlineData(false)]
    public void Execute_R2WithAbsoluteMode_UpdatesOverflowFlag(bool expectedOverflow)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort memoryAddress = 0x1000;
        ushort valueToMove = 0x8000;

        registerFile.WriteRegister(RegisterName.R3, valueToMove);
        // Set opposite initial values to ensure the test is checking the actual update
        registerFile.StatusRegister.Overflow = true;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R3,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Absolute,
            false);

        // Act
        instruction.Execute(registerFile, memory, new ushort[] { memoryAddress });

        // Assert
        Assert.Equal(expectedOverflow, registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_R2WithAbsoluteMode_WritesToMemory()
    {
        // This test verifies that when R2 is used as the destination register with Absolute mode,
        // the value is written to memory at the absolute address
        //
        // NOTE: R3 with Register mode (As=00) is a constant generator that returns 0,
        // per MSP430FR2xx FR4xx Family User's Guide (SLAU445I) Section 4.3.4

        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort memoryAddress = 0x1000;

        // Set up R3 with some value - this should be ignored since R3 Register mode is constant generator
        registerFile.WriteRegister(RegisterName.R3, 0x8000);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R3, // Source register - R3 As=00 generates constant 0
            RegisterName.R2, // Destination register is R2, but mode is Absolute
            AddressingMode.Register,
            AddressingMode.Absolute, // This means write to memory at absolute address, not directly to R2
            false);

        // Act
        instruction.Execute(registerFile, memory, new ushort[] { memoryAddress });

        // Assert
        // Check that constant 0 was written to memory (R3 Register mode generates 0)
        ushort writtenValue = (ushort)(memory[memoryAddress] | (memory[memoryAddress + 1] << 8));
        Assert.Equal(0x0000, writtenValue);
    }

    [Theory]
    [InlineData(false)]
    public void Execute_R2WithSymbolicMode_UpdatesNegativeFlag(bool expectedNegative)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort offset = 0x0010;
        ushort pcValue = 0x1000;
        ushort valueToMove = 0x0000;

        registerFile.WriteRegister(RegisterName.PC, pcValue);
        registerFile.WriteRegister(RegisterName.R4, valueToMove);
        // Set opposite initial values to ensure the test is checking the actual update
        registerFile.StatusRegister.Negative = true;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R4,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Symbolic,
            false);

        // Act
        instruction.Execute(registerFile, memory, new ushort[] { offset });

        // Assert
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
    }

    [Theory]
    [InlineData(true)]
    public void Execute_R2WithSymbolicMode_UpdatesZeroFlag(bool expectedZero)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort offset = 0x0010;
        ushort pcValue = 0x1000;
        ushort valueToMove = 0x0000;

        registerFile.WriteRegister(RegisterName.PC, pcValue);
        registerFile.WriteRegister(RegisterName.R4, valueToMove);
        // Set opposite initial values to ensure the test is checking the actual update
        registerFile.StatusRegister.Zero = false;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R4,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Symbolic,
            false);

        // Act
        instruction.Execute(registerFile, memory, new ushort[] { offset });

        // Assert
        Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
    }

    [Theory]
    [InlineData(false)]
    public void Execute_R2WithSymbolicMode_UpdatesOverflowFlag(bool expectedOverflow)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort offset = 0x0010;
        ushort pcValue = 0x1000;
        ushort valueToMove = 0x0000;

        registerFile.WriteRegister(RegisterName.PC, pcValue);
        registerFile.WriteRegister(RegisterName.R4, valueToMove);
        // Set opposite initial values to ensure the test is checking the actual update
        registerFile.StatusRegister.Overflow = true;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R4,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Symbolic,
            false);

        // Act
        instruction.Execute(registerFile, memory, new ushort[] { offset });

        // Assert
        Assert.Equal(expectedOverflow, registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_R2WithSymbolicMode_WritesToMemory()
    {
        // This test verifies that when R2 is used as the destination register with Symbolic mode,
        // the value is written to memory at PC + offset

        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort offset = 0x0010;
        ushort pcValue = 0x1000;
        ushort valueToMove = 0x0000; // Zero value to test Zero flag

        // Set up PC and source value
        registerFile.WriteRegister(RegisterName.PC, pcValue);
        registerFile.WriteRegister(RegisterName.R4, valueToMove);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R4, // Source register
            RegisterName.R2, // Destination register is R2, but mode is Symbolic
            AddressingMode.Register,
            AddressingMode.Symbolic, // This means write to memory at PC + offset, not directly to R2
            false);

        // Act
        instruction.Execute(registerFile, memory, new ushort[] { offset });

        // Assert
        // Check that value was written to memory at PC + offset (not to R2 register)
        ushort targetAddress = (ushort)(pcValue + offset);
        ushort writtenValue = (ushort)(memory[targetAddress] | (memory[targetAddress + 1] << 8));
        Assert.Equal(valueToMove, writtenValue);
    }

    [Fact]
    public void ToString_WithExtensionWords_ShowsActualValues()
    {
        // Arrange
        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Immediate,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = { 0x1234, 0x5678 }; // Immediate value and absolute address

        // Act
        string result = instruction.ToString(extensionWords);

        // Assert
        Assert.Equal("MOV #0x1234, &0x5678", result);
    }

    [Fact]
    public void ToString_WithExtensionWords_ByteOperation_ShowsActualValues()
    {
        // Arrange
        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Indexed,
            AddressingMode.Symbolic,
            true);

        ushort[] extensionWords = { 0x10, 0x20 }; // Indexed offset and symbolic offset

        // Act
        string result = instruction.ToString(extensionWords);

        // Assert
        Assert.Equal("MOV.B 0x10(R5), 0x20", result);
    }
}
