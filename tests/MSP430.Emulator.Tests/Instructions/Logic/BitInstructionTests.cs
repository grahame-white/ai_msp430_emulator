using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Logic;

namespace MSP430.Emulator.Tests.Instructions.Logic;

/// <summary>
/// Unit tests for the BitInstruction class.
/// </summary>
public class BitInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesInstruction()
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
        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
        Assert.Equal(0xB, instruction.Opcode);
        Assert.Equal(0xB123, instruction.InstructionWord);
        Assert.Equal("BIT", instruction.Mnemonic);
        Assert.False(instruction.IsByteOperation);
        Assert.Equal(RegisterName.R1, instruction.SourceRegister);
        Assert.Equal(RegisterName.R2, instruction.DestinationRegister);
        Assert.Equal(AddressingMode.Register, instruction.SourceAddressingMode);
        Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsByteFlag()
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
    [InlineData(RegisterName.CG1)]
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
    [InlineData(RegisterName.CG1)]
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
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_ReturnCorrectValues(AddressingMode mode)
    {
        // Arrange & Act
        var instruction = new BitInstruction(
            0xB000,
            RegisterName.R1,
            RegisterName.R2,
            mode,
            mode,
            false);

        // Assert
        Assert.Equal(mode, instruction.SourceAddressingMode);
        Assert.Equal(mode, instruction.DestinationAddressingMode);
    }

    #region Execute Method Tests

    [Fact]
    public void Execute_RegisterToRegister_TestsBitsWithoutWriting()
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
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert - destination should be unchanged
        Assert.Equal(originalR5, registerFile.ReadRegister(RegisterName.R5));
        Assert.False(registerFile.StatusRegister.Zero); // 0xFF0F & 0x0FF0 = 0x0F00 (not zero)
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.False(registerFile.StatusRegister.Carry);
        Assert.False(registerFile.StatusRegister.Overflow);
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_ResultIsZero_SetsZeroFlag()
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

        // Assert - destination unchanged but flags set based on AND result
        Assert.Equal(originalR5, registerFile.ReadRegister(RegisterName.R5));
        Assert.True(registerFile.StatusRegister.Zero); // 0xFF00 & 0x00FF = 0
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.False(registerFile.StatusRegister.Carry);
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_ImmediateSource_UsesExtensionWord()
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

        // Assert - destination unchanged, flags based on 0x00F0 & 0x12FF = 0x00F0
        Assert.Equal(originalR5, registerFile.ReadRegister(RegisterName.R5));
        Assert.False(registerFile.StatusRegister.Zero); // 0x00F0 & 0x12FF = 0x00F0 (not zero)
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_ByteOperation_TestsBytesOnly()
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

        // Assert - destination unchanged, flags based on byte AND (0xAB & 0xCD = 0x89)
        Assert.Equal(originalR5, registerFile.ReadRegister(RegisterName.R5));
        Assert.False(registerFile.StatusRegister.Zero); // 0xAB & 0xCD = 0x89 (not zero)
        Assert.True(registerFile.StatusRegister.Negative); // 0x89 has bit 7 set (negative for byte)
    }

    #endregion
}
