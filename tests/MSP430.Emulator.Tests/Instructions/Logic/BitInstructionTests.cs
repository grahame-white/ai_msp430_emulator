using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Logic;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.Logic;

/// <summary>
/// Unit tests for the BitInstruction class.
/// </summary>
public class BitInstructionTests
{

    [Theory]
    [InlineData(InstructionFormat.FormatI)]
    public void Constructor_ValidParameters_SetsFormat(InstructionFormat expectedFormat)
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB123,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedFormat, instruction.Format);
    }

    [Theory]
    [InlineData(0xB)]
    public void Constructor_ValidParameters_SetsOpcode(byte expectedOpcode)
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB123,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedOpcode, instruction.Opcode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsInstructionWord()
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB123,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(0xB123, instruction.InstructionWord);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsMnemonic()
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB123,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal("BIT", instruction.Mnemonic);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsIsByteOperationFalse()
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB123,
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
        var instruction = new BitInstruction(
            0xB123,
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
        var instruction = new BitInstruction(
            0xB123,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(RegisterName.R2, instruction.DestinationRegister);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsSourceAddressingMode()
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB123,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(AddressingMode.Register, instruction.SourceAddressingMode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsDestinationAddressingMode()
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB123,
            RegisterName.R1,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsIsByteOperationTrue()
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Assert
        Assert.True(instruction.IsByteOperation);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsByteMnemonic()
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Assert
        Assert.Equal("BIT.B", instruction.Mnemonic);
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
        var instruction = new BitInstruction(
            0xB000,
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
        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R1,
            RegisterName.R1,
            mode,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal($"BIT {expectedOperand}, R1", result);
    }

    [Fact]
    public void ToString_ByteOperation_IncludesByteModifier()
    {
        // Arrange
        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R3,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("BIT.B R3, R4", result);
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
        var instruction = new BitInstruction(
            0xB000,
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
        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R1,
            register,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(register, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Immediate)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_SetsSourceAddressingMode(AddressingMode sourceMode, AddressingMode destMode)
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R1,
            RegisterName.R2,
            sourceMode,
            destMode,
            false);

        // Assert
        Assert.Equal(sourceMode, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Immediate)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_SetsDestinationAddressingMode(AddressingMode sourceMode, AddressingMode destMode)
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R1,
            RegisterName.R2,
            sourceMode,
            destMode,
            false);

        // Assert
        Assert.Equal(destMode, instruction.DestinationAddressingMode);
    }

    #region Execute Method Tests

    [Fact]
    public void Execute_RegisterToRegister_DestinationValueUnchanged()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);
        ushort originalR5 = registerFile.ReadRegister(RegisterName.R5);

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert - destination should be unchanged
        Assert.Equal(originalR5, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_RegisterToRegister_ZeroFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Zero); // 0xFF0F & 0x0FF0 = 0x0F00 (not zero)
    }

    [Fact]
    public void Execute_RegisterToRegister_NegativeFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new BitInstruction(
            0xB000,
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
    public void Execute_RegisterToRegister_CarryFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new BitInstruction(
            0xB000,
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
    public void Execute_RegisterToRegister_OverflowFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new BitInstruction(
            0xB000,
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
    public void Execute_RegisterToRegister_Returns1Cycle()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF0F);
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new BitInstruction(
            0xB000,
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
    public void Execute_ResultIsZero_DestinationValueUnchanged()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x00FF);
        ushort originalR5 = registerFile.ReadRegister(RegisterName.R5);

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert - destination unchanged
        Assert.Equal(originalR5, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_ResultIsZero_SetsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x00FF);

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Zero); // 0xFF00 & 0x00FF = 0
    }

    [Fact]
    public void Execute_ResultIsZero_NegativeFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x00FF);

        var instruction = new BitInstruction(
            0xB000,
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
    public void Execute_ResultIsZero_CarryFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x00FF);

        var instruction = new BitInstruction(
            0xB000,
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
    public void Execute_ResultIsZero_OverflowFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x00FF);

        var instruction = new BitInstruction(
            0xB000,
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
    public void Execute_ImmediateSource_DestinationValueUnchanged()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R5, 0x12FF);
        ushort originalR5 = registerFile.ReadRegister(RegisterName.R5);

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R0, // Using R0 as immediate addressing source
            RegisterName.R5,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = [0x00F0]; // Immediate value

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert - destination unchanged
        Assert.Equal(originalR5, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_ImmediateSource_ZeroFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R5, 0x12FF);

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R0, // Using R0 as immediate addressing source
            RegisterName.R5,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = [0x00F0]; // Immediate value

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero); // 0x00F0 & 0x12FF = 0x00F0 (not zero)
    }

    [Fact]
    public void Execute_ImmediateSource_NegativeFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R5, 0x12FF);

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R0, // Using R0 as immediate addressing source
            RegisterName.R5,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = [0x00F0]; // Immediate value

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_ByteOperation_DestinationValueUnchanged()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x12AB);
        registerFile.WriteRegister(RegisterName.R5, 0x34CD);
        ushort originalR5 = registerFile.ReadRegister(RegisterName.R5);

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            true); // Byte operation

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert - destination unchanged
        Assert.Equal(originalR5, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_ByteOperation_ZeroFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x12AB);
        registerFile.WriteRegister(RegisterName.R5, 0x34CD);

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            true); // Byte operation

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Zero); // 0xAB & 0xCD = 0x89 (not zero)
    }

    [Fact]
    public void Execute_ByteOperation_NegativeFlagSetCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x12AB);
        registerFile.WriteRegister(RegisterName.R5, 0x34CD);

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            true); // Byte operation

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Negative); // 0x89 has bit 7 set (negative for byte)
    }

    [Fact]
    public void Execute_ImmediateToRegister_Takes1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new BitInstruction(
            0xB000,
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

        var instruction = new BitInstruction(
            0xB000,
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

        var instruction = new BitInstruction(
            0xB000,
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

        var instruction = new BitInstruction(
            0xB000,
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

        var instruction = new BitInstruction(
            0xB000,
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

        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R0, // Absolute source
            RegisterName.R1, // Absolute destination
            AddressingMode.Absolute,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = { 0x0300, 0x0400 }; // Source address, destination address

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(7u, cycles); // 1 base + 3 source (absolute) + 3 dest (absolute)
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

        var instruction = new BitInstruction(
            0xB000,
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
    public void Execute_IndirectAutoIncrementToRegister_Returns3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0100); // Points to memory address
        registerFile.WriteRegister(RegisterName.R5, 0x0000);

        // Set up memory at indirect location
        memory[0x0100] = 0x00;
        memory[0x0101] = 0xFF;

        var instruction = new BitInstruction(
            0xB000,
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

        var instruction = new BitInstruction(
            0xB000,
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
