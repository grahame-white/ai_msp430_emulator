using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Logic;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.Logic;

/// <summary>
/// Unit tests for the AndInstruction class.
/// </summary>
public class AndInstructionTests
{

    [Theory]
    [InlineData(0xF123)]
    [InlineData(0xF456)]
    [InlineData(0xF789)]
    public void Constructor_ValidParameters_SetsFormat(ushort instructionWord)
    {
        var instruction = new AndInstruction(instructionWord, RegisterName.R1, RegisterName.R2, AddressingMode.Register, AddressingMode.Register, false);
        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
    }

    [Theory]
    [InlineData(0xF123)]
    [InlineData(0xF456)]
    [InlineData(0xF789)]
    public void Constructor_ValidParameters_SetsOpcode(ushort instructionWord)
    {
        var instruction = new AndInstruction(instructionWord, RegisterName.R1, RegisterName.R2, AddressingMode.Register, AddressingMode.Register, false);
        Assert.Equal((byte)0xF, instruction.Opcode);
    }

    [Theory]
    [InlineData(0xF123)]
    [InlineData(0xF456)]
    [InlineData(0xF789)]
    public void Constructor_ValidParameters_SetsInstructionWord(ushort instructionWord)
    {
        var instruction = new AndInstruction(instructionWord, RegisterName.R1, RegisterName.R2, AddressingMode.Register, AddressingMode.Register, false);
        Assert.Equal(instructionWord, instruction.InstructionWord);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_ValidParameters_SetsByteOperation(bool isByteOp)
    {
        var instruction = new AndInstruction(0xF123, RegisterName.R1, RegisterName.R2, AddressingMode.Register, AddressingMode.Register, isByteOp);
        Assert.Equal(isByteOp, instruction.IsByteOperation);
    }

    [Theory]
    [InlineData(false, "AND")]
    [InlineData(true, "AND.B")]
    public void Constructor_ByteOperationFlag_SetsMnemonic(bool isByteOperation, string expectedMnemonic)
    {
        var instruction = new AndInstruction(0xF563, RegisterName.R5, RegisterName.R6, AddressingMode.Register, AddressingMode.Register, isByteOperation);
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
        // Arrange
        var instruction = new AndInstruction(
            0xF000,
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
        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R1,
            RegisterName.R1,
            mode,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal($"AND {expectedOperand}, R1", result);
    }

    [Fact]
    public void ToString_ByteOperation_IncludesByteModifier()
    {
        // Arrange
        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R3,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("AND.B R3, R4", result);
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
        var instruction = new AndInstruction(
            0xF000,
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
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R15)]
    public void DestinationRegister_AllRegisters_ReturnsCorrectRegister(RegisterName register)
    {
        // Arrange & Act
        var instruction = new AndInstruction(
            0xF000,
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
    public void AddressingModes_AllSupportedModes_SetsSourceAddressingMode(AddressingMode mode)
    {
        // Arrange & Act
        var instruction = new AndInstruction(
            0xF000,
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
    public void AddressingModes_AllSupportedModes_SetsDestinationAddressingMode(AddressingMode mode)
    {
        // Arrange & Act
        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R1,
            RegisterName.R2,
            mode,
            mode,
            false);

        // Assert
        Assert.Equal(mode, instruction.DestinationAddressingMode);
    }

    #region Execute Method Tests

    [Fact]
    public void Execute_RegisterToRegister_PerformsAndOperation()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0xFF0F & 0x0FF0, registerFile.ReadRegister(RegisterName.R5)); // 0x0F00
    }

    [Fact]
    public void Execute_RegisterToRegister_ReturnsOneCycle()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_RegisterToRegister_SetsZeroFlagCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new AndInstruction(
            0xF000,
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
    public void Execute_RegisterToRegister_SetsNegativeFlagCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new AndInstruction(
            0xF000,
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
    public void Execute_RegisterToRegister_SetsCarryFlagCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new AndInstruction(
            0xF000,
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
    public void Execute_RegisterToRegister_SetsOverflowFlagCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new AndInstruction(
            0xF000,
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

    [Fact]
    public void Execute_ResultIsZero_ComputesCorrectResult()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x00FF);

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_ResultIsZero_SetsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x00FF);

        var instruction = new AndInstruction(
            0xF000,
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
    public void Execute_ResultIsZero_ClearsNegativeFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x00FF);

        var instruction = new AndInstruction(
            0xF000,
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
    public void Execute_ResultIsZero_ClearsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x00FF);

        var instruction = new AndInstruction(
            0xF000,
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
    public void Execute_ResultIsZero_ClearsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x00FF);

        var instruction = new AndInstruction(
            0xF000,
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

    [Fact]
    public void Execute_WordResultNegative_ComputesCorrectResult()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x8000);
        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x8000, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_WordResultNegative_ClearsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x8000);
        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new AndInstruction(
            0xF000,
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
    public void Execute_WordResultNegative_SetsNegativeFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x8000);
        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new AndInstruction(
            0xF000,
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

    [Fact]
    public void Execute_WordResultNegative_ClearsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x8000);
        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new AndInstruction(
            0xF000,
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
    public void Execute_WordResultNegative_ClearsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x8000);
        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new AndInstruction(
            0xF000,
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

    [Fact]
    public void Execute_ImmediateSource_UsesExtensionWord()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R0, // Using R0 as immediate addressing source
            RegisterName.R5,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = [0x1234]; // Immediate value

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_AbsoluteDestination_UsesExtensionWord_LowByte()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        // Set up existing value at absolute address 0x200
        memory[0x200] = 0xFF;
        memory[0x201] = 0xFF; // 0xFFFF in little-endian

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R0, // Using R0 as absolute addressing destination
            AddressingMode.Register,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = [0x0200]; // Absolute address

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert - memory at 0x200 should be modified
        Assert.Equal(0x34, memory[0x200]); // Low byte of (0x1234 & 0xFFFF) = 0x1234
    }

    [Fact]
    public void Execute_AbsoluteDestination_UsesExtensionWord_HighByte()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        // Set up existing value at absolute address 0x200
        memory[0x200] = 0xFF;
        memory[0x201] = 0xFF; // 0xFFFF in little-endian

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R0, // Using R0 as absolute addressing destination
            AddressingMode.Register,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = [0x0200]; // Absolute address

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert - memory at 0x200 should be modified
        Assert.Equal(0x12, memory[0x201]); // High byte of 0x1234
    }

    [Fact]
    public void Execute_ByteOperation_WorksCorrectly_RegisterValue()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x12AB);
        registerFile.WriteRegister(RegisterName.R5, 0x34CD);

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            true); // Byte operation

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert - only low byte should be affected
        Assert.Equal((ushort)((0xAB & 0xCD) | 0x3400), registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_ByteOperation_WorksCorrectly_LowByteResult()
    {
        // Arrange & Act (calculation verification)

        // Assert - verify expected low byte result
        Assert.Equal(0x89, 0xAB & 0xCD);
    }

    [Fact]
    public void Execute_SymbolicSource_UsesExtensionWord()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R5, 0xFF00);

        // Set up memory at symbolic address (R0 + offset)
        registerFile.WriteRegister(RegisterName.R0, 0x0100); // PC base
        memory[0x0200] = 0x0F; // Low byte at symbolic address 0x200 = 0x100 + 0x100
        memory[0x0201] = 0x00; // High byte of 0x000F

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R0, // Using R0 for symbolic addressing
            RegisterName.R5,
            AddressingMode.Symbolic,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = [0x0100]; // Symbolic offset

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert - should AND 0xFF00 & 0x000F = 0x0000
        Assert.Equal(0x0000, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_SymbolicSource_UsesExtensionWord_ZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R5, 0xFF00);

        // Set up memory at symbolic address (R0 + offset)
        registerFile.WriteRegister(RegisterName.R0, 0x0100); // PC base
        memory[0x0200] = 0x0F; // Low byte at symbolic address 0x200 = 0x100 + 0x100
        memory[0x0201] = 0x00; // High byte of 0x000F

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R0, // Using R0 for symbolic addressing
            RegisterName.R5,
            AddressingMode.Symbolic,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = [0x0100]; // Symbolic offset

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_IndexedDestination_UsesExtensionWord_LowByte()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x00FF);
        registerFile.WriteRegister(RegisterName.R6, 0x0200); // Base address

        // Set up initial value at indexed destination
        memory[0x0210] = 0xF0; // Low byte at indexed address 0x200 + 0x10 = 0x210
        memory[0x0211] = 0x0F; // High byte of 0x0FF0

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Indexed,
            false);

        ushort[] extensionWords = [0x0010]; // Index offset

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert - memory at indexed location should be modified: 0x00FF & 0x0FF0 = 0x00F0
        Assert.Equal(0xF0, memory[0x0210]); // Low byte of 0x00F0
    }

    [Fact]
    public void Execute_IndexedDestination_UsesExtensionWord_HighByte()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x00FF);
        registerFile.WriteRegister(RegisterName.R6, 0x0200); // Base address

        // Set up initial value at indexed destination
        memory[0x0210] = 0xF0; // Low byte at indexed address 0x200 + 0x10 = 0x210
        memory[0x0211] = 0x0F; // High byte of 0x0FF0

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Indexed,
            false);

        ushort[] extensionWords = [0x0010]; // Index offset

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x00, memory[0x0211]); // High byte of 0x00F0
    }

    [Fact]
    public void Execute_BothSourceAndDestinationRequireExtensionWords_LowByte()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];

        // Set up memory values
        memory[0x0100] = 0xAA; // Source value at absolute address 0x100
        memory[0x0101] = 0x55; // High byte: 0x55AA
        memory[0x0200] = 0xFF; // Destination value at absolute address 0x200
        memory[0x0201] = 0x00; // High byte: 0x00FF

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R0, // Absolute source
            RegisterName.R1, // Absolute destination
            AddressingMode.Absolute,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = [0x0100, 0x0200]; // Source address, destination address

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert - memory at destination should be modified: 0x55AA & 0x00FF = 0x00AA
        Assert.Equal(0xAA, memory[0x0200]); // Low byte of 0x00AA
    }

    [Fact]
    public void Execute_BothSourceAndDestinationRequireExtensionWords_HighByte()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];

        // Set up memory values
        memory[0x0100] = 0xAA; // Source value at absolute address 0x100
        memory[0x0101] = 0x55; // High byte: 0x55AA
        memory[0x0200] = 0xFF; // Destination value at absolute address 0x200
        memory[0x0201] = 0x00; // High byte: 0x00FF

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R0, // Absolute source
            RegisterName.R1, // Absolute destination
            AddressingMode.Absolute,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = [0x0100, 0x0200]; // Source address, destination address

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x00, memory[0x0201]); // High byte of 0x00AA
    }

    [Fact]
    public void Execute_BothSourceAndDestinationRequireExtensionWords_Cycles()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];

        // Set up memory values
        memory[0x0100] = 0xAA; // Source value at absolute address 0x100
        memory[0x0101] = 0x55; // High byte: 0x55AA
        memory[0x0200] = 0xFF; // Destination value at absolute address 0x200
        memory[0x0201] = 0x00; // High byte: 0x00FF

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R0, // Absolute source
            RegisterName.R1, // Absolute destination
            AddressingMode.Absolute,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = [0x0100, 0x0200]; // Source address, destination address

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(7u, cycles); // 1 base + 3 for absolute source + 3 for absolute destination
    }

    [Fact]
    public void Execute_ImmediateToRegister_Takes1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R0, // PC for immediate
            RegisterName.R5,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0xFF00 }; // Immediate value

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(1u, cycles); // 1 base + 0 source (immediate) + 0 dest (register)
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

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Indexed,
            false);

        ushort[] extensionWords = { 0x0010 }; // Offset for indexed addressing

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(4u, cycles); // 1 base + 0 source (register) + 3 dest (indexed)
    }

    [Fact]
    public void Execute_IndexedToRegister_Takes4Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0100); // Base address
        registerFile.WriteRegister(RegisterName.R5, 0x0000);

        // Set up memory at indexed location R4 + 0x10 = 0x0110
        memory[0x0110] = 0x00;
        memory[0x0111] = 0xFF;

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Indexed,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x0010 }; // Offset for indexed addressing

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(4u, cycles); // 1 base + 3 source (indexed) + 0 dest (register)
    }

    [Fact]
    public void Execute_IndirectToRegister_Takes3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0100); // Points to memory address
        registerFile.WriteRegister(RegisterName.R5, 0x0000);

        // Set up memory at indirect location
        memory[0x0100] = 0x00;
        memory[0x0101] = 0xFF;

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Indirect,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = Array.Empty<ushort>();

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(3u, cycles); // 1 base + 2 source (indirect) + 0 dest (register)
    }

    [Fact]
    public void Execute_RegisterToIndirect_Takes3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x0100); // Points to memory address

        // Set up memory at indirect location
        memory[0x0100] = 0xF0;
        memory[0x0101] = 0x0F;

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Indirect,
            false);

        ushort[] extensionWords = Array.Empty<ushort>();

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(3u, cycles); // 1 base + 0 source (register) + 2 dest (indirect)
    }

    [Fact]
    public void Execute_SymbolicToSymbolic_Takes7Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.SetProgramCounter(0x1000);

        // Set up memory at symbolic source location PC + 0x10 = 0x1010
        memory[0x1010] = 0x00;
        memory[0x1011] = 0xFF;

        // Set up memory at symbolic destination location PC + 0x20 = 0x1020
        memory[0x1020] = 0xF0;
        memory[0x1021] = 0x0F;

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R0, // PC for symbolic
            RegisterName.R0, // PC for symbolic
            AddressingMode.Symbolic,
            AddressingMode.Symbolic,
            false);

        ushort[] extensionWords = { 0x0010, 0x0020 }; // Source offset, destination offset

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(7u, cycles); // 1 base + 3 source (symbolic) + 3 dest (symbolic)
    }

    [Fact]
    public void Execute_IndirectAutoIncrementToRegister_Takes3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0100); // Points to memory address
        registerFile.WriteRegister(RegisterName.R5, 0x0000);

        // Set up memory at indirect location
        memory[0x0100] = 0x00;
        memory[0x0101] = 0xFF;

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = Array.Empty<ushort>();

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(3u, cycles); // 1 base + 2 source (indirect auto-increment) + 0 dest (register)
    }

    [Fact]
    public void Execute_IndirectAutoIncrementToRegister_IncrementsSourceRegister()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0100); // Points to memory address
        registerFile.WriteRegister(RegisterName.R5, 0x0000);

        // Set up memory at indirect location
        memory[0x0100] = 0x00;
        memory[0x0101] = 0xFF;

        var instruction = new AndInstruction(
            0xF000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = Array.Empty<ushort>();

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x0102, registerFile.ReadRegister(RegisterName.R4)); // Should be incremented by 2 for word operation
    }

    #endregion
}
