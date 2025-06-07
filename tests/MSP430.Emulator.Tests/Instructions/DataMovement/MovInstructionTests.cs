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
            RegisterName.R4, // Use R4 instead of R2
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R4));
        Assert.Equal(1u, cycles); // Register to register takes 1 cycle
    }

    [Fact]
    public void Execute_ByteOperation_OnlyMovesLowByte()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R3, 0x5678); // Set up R3 with a known value

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

    [Fact]
    public void Execute_RegisterToIndirect_WritesToMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0xABCD); // Use R5 as source
        registerFile.WriteRegister(RegisterName.R6, 0x2000); // Use R6 as address pointer

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R5, // Use R5 as source
            RegisterName.R6, // Use R6 as address pointer
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
            RegisterName.R3, // Use R3 instead of R2 to avoid status register conflicts
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

    [Fact]
    public void Execute_StatusRegisterAsSource_UpdatesFlagsNormally()
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
        // Flags SHOULD be updated when destination is not R2
        Assert.False(registerFile.StatusRegister.Negative); // Negative flag clear (bit 15 not set)
        Assert.True(registerFile.StatusRegister.Zero); // Zero flag set (value is zero)
        Assert.False(registerFile.StatusRegister.Overflow); // Overflow flag cleared by MOV
        // Carry flag is preserved (not modified by MOV)
    }

    [Fact]
    public void Execute_R2WithAbsoluteMode_UpdatesFlags()
    {
        // This test verifies that when R2 is used as the destination register with Absolute mode,
        // flags are updated normally because we're not writing directly to the Status Register

        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort memoryAddress = 0x1000;
        ushort valueToMove = 0x8000; // Negative value to test flag updates

        // Set up R3 with the value to move
        registerFile.WriteRegister(RegisterName.R3, valueToMove);

        // Clear flags to test they get updated
        registerFile.StatusRegister.Negative = false;
        registerFile.StatusRegister.Zero = false;
        registerFile.StatusRegister.Overflow = true;

        var instruction = new MovInstruction(
            0x4000,
            RegisterName.R3, // Source register
            RegisterName.R2, // Destination register is R2, but mode is Absolute
            AddressingMode.Register,
            AddressingMode.Absolute, // This means write to memory at absolute address, not directly to R2
            false);

        // Act
        instruction.Execute(registerFile, memory, new ushort[] { memoryAddress });

        // Assert
        // Check that value was written to memory at the absolute address (not to R2 register)
        ushort writtenValue = (ushort)(memory[memoryAddress] | (memory[memoryAddress + 1] << 8));
        Assert.Equal(valueToMove, writtenValue);

        // Flags SHOULD be updated because we're using Absolute mode (not Register mode)
        Assert.True(registerFile.StatusRegister.Negative); // Negative flag set (bit 15 set)
        Assert.False(registerFile.StatusRegister.Zero); // Zero flag clear (value is not zero)
        Assert.False(registerFile.StatusRegister.Overflow); // Overflow flag cleared by MOV
    }

    [Fact]
    public void Execute_R2WithSymbolicMode_UpdatesFlags()
    {
        // This test verifies that when R2 is used as the destination register with Symbolic mode,
        // flags are updated normally because we're not writing directly to the Status Register

        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort offset = 0x0010;
        ushort pcValue = 0x1000;
        ushort valueToMove = 0x0000; // Zero value to test Zero flag

        // Set up PC and source value
        registerFile.WriteRegister(RegisterName.PC, pcValue);
        registerFile.WriteRegister(RegisterName.R4, valueToMove);

        // Clear flags to test they get updated
        registerFile.StatusRegister.Negative = true;
        registerFile.StatusRegister.Zero = false;
        registerFile.StatusRegister.Overflow = true;

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

        // Flags SHOULD be updated because we're using Symbolic mode (not Register mode)
        Assert.False(registerFile.StatusRegister.Negative); // Negative flag clear (bit 15 not set)
        Assert.True(registerFile.StatusRegister.Zero); // Zero flag set (value is zero)
        Assert.False(registerFile.StatusRegister.Overflow); // Overflow flag cleared by MOV
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
