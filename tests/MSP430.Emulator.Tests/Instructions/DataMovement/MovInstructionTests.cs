using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.DataMovement;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.DataMovement;

/// <summary>
/// Unit tests for the MovInstruction class.
/// Tests all addressing mode combinations, byte/word operations, and flag behavior.
/// </summary>
public class MovInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesInstruction()
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
        Assert.Equal(0x4, instruction.Opcode);
        Assert.Equal(0x4123, instruction.InstructionWord);
        Assert.Equal("MOV", instruction.Mnemonic);
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
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R2));
        Assert.Equal(1u, cycles); // Register to register takes 1 cycle
    }

    [Fact]
    public void Execute_ByteOperation_OnlyMovesLowByte()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R2, 0x5678);

        // Debug: check what ReadOperand returns
        ushort sourceValue = InstructionHelpers.ReadOperand(
            RegisterName.R1,
            AddressingMode.Register,
            true,
            registerFile,
            memory,
            0);

        // Should be 0x34 (low byte of 0x1234)
        Assert.Equal(0x34, sourceValue);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        ushort actualR2 = registerFile.ReadRegister(RegisterName.R2);
        Assert.Equal(0x5634, actualR2); // High byte preserved, low byte changed
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
    public void Execute_IndirectAutoIncrement_ReadsAndIncrementsPointer()
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
        Assert.Equal(0x1002, registerFile.ReadRegister(RegisterName.R1)); // Incremented by 2 for word operation
    }

    [Fact]
    public void Execute_IndirectAutoIncrementByte_IncrementsBy1()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.WriteRegister(RegisterName.R2, 0xFF00);
        memory[0x1000] = 0x34;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0xFF34, registerFile.ReadRegister(RegisterName.R2));
        Assert.Equal(0x1001, registerFile.ReadRegister(RegisterName.R1)); // Incremented by 1 for byte operation
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

    [Fact]
    public void Execute_RegisterToIndirect_WritesToMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0xABCD);
        registerFile.WriteRegister(RegisterName.R2, 0x2000);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Indirect,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0xCD, memory[0x2000]);
        Assert.Equal(0xAB, memory[0x2001]);
    }

    [Fact]
    public void Execute_RegisterToIndexed_WritesToOffsetAddress()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R2, 0x2000);

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Indexed,
            false);

        ushort[] extensionWords = { 0x0010 }; // Offset

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x34, memory[0x2010]);
        Assert.Equal(0x12, memory[0x2011]);
    }

    [Theory]
    [InlineData(0x0000, true, false)] // Zero value sets Z, clears N
    [InlineData(0x0080, true, true)]  // Byte: bit 7 set, sets N 
    [InlineData(0x007F, true, false)] // Byte: bit 7 clear, clears N
    [InlineData(0x8000, false, true)] // Word: bit 15 set, sets N
    [InlineData(0x7FFF, false, false)] // Word: bit 15 clear, clears N
    public void Execute_FlagUpdates_SetsCorrectFlags(ushort value, bool isByteOperation, bool expectedNegative)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, value);
        registerFile.StatusRegister.Carry = true; // Should be preserved
        registerFile.StatusRegister.Overflow = true; // Should be cleared

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            isByteOperation);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(value == 0, registerFile.StatusRegister.Zero);
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
        Assert.False(registerFile.StatusRegister.Overflow); // Always cleared
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
    [InlineData(AddressingMode.Immediate, AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Indexed)]
    public void AddressingModes_AllSupportedModes_ReturnCorrectValues(AddressingMode sourceMode, AddressingMode destMode)
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
            (AddressingMode.Immediate, AddressingMode.Register, "MOV #N, R2"),
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
}
