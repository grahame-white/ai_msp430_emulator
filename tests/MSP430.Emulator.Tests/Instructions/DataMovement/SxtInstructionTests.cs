using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.DataMovement;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.DataMovement;

/// <summary>
/// Unit tests for the SxtInstruction class.
/// Tests sign extension from byte to word, flag setting, and addressing modes.
/// </summary>
public class SxtInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsFormat()
    {
        // Arrange & Act
        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal(InstructionFormat.FormatII, instruction.Format);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal(0x11, instruction.Opcode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsInstructionWord()
    {
        // Arrange & Act
        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal(0x1184, instruction.InstructionWord);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsMnemonic()
    {
        // Arrange & Act
        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal("SXT", instruction.Mnemonic);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsIsByteOperationToFalse()
    {
        // Arrange & Act
        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.False(instruction.IsByteOperation);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsDestinationRegister()
    {
        // Arrange & Act
        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal(RegisterName.R4, instruction.DestinationRegister);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsDestinationAddressingMode()
    {
        // Arrange & Act
        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Indexed, 1)]
    [InlineData(AddressingMode.Absolute, 1)]
    [InlineData(AddressingMode.Symbolic, 1)]
    public void Constructor_ValidParameters_SetsExtensionWordCount(AddressingMode mode, int expectedCount)
    {
        // Arrange & Act
        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            mode);

        // Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    [Fact]
    public void ToString_RegisterMode_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("SXT R4", result);
    }

    [Fact]
    public void ToString_IndirectMode_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new SxtInstruction(
            0x1194,
            RegisterName.R4,
            AddressingMode.Indirect);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("SXT @R4", result);
    }

    [Theory]
    [InlineData(0x0000, 0x0000)] // Zero case: byte 0x00 -> word 0x0000
    [InlineData(0x007F, 0x007F)] // Positive byte: 0x7F -> 0x007F (no sign extension needed)
    [InlineData(0x0080, 0xFF80)] // Negative byte: 0x80 -> 0xFF80 (sign extended)
    [InlineData(0x00FF, 0xFFFF)] // Maximum negative byte: 0xFF -> 0xFFFF
    [InlineData(0x0001, 0x0001)] // Positive byte: 0x01 -> 0x0001
    [InlineData(0x0040, 0x0040)] // Positive byte: 0x40 -> 0x0040
    [InlineData(0x00C0, 0xFFC0)] // Negative byte: 0xC0 -> 0xFFC0
    [InlineData(0x1234, 0x0034)] // Upper byte ignored: only low byte 0x34 -> 0x0034
    [InlineData(0xAB80, 0xFF80)] // Upper byte ignored: only low byte 0x80 -> 0xFF80
    public void Execute_RegisterMode_SignExtendsCorrectly(ushort inputValue, ushort expectedResult)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, inputValue);

        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedResult, registerFile.ReadRegister(RegisterName.R4));
        Assert.Equal(1u, cycles); // Register mode should take 1 cycle
    }

    [Theory]
    [InlineData(0x0000, true, false)]  // Zero result: Z=1, N=0
    [InlineData(0xFF80, false, true)]  // Negative result (sign extended): Z=0, N=1
    [InlineData(0x007F, false, false)] // Positive result: Z=0, N=0
    [InlineData(0xFFFF, false, true)]  // Negative result (0xFF sign extended): Z=0, N=1
    public void Execute_FlagUpdates_SetsZeroAndNegativeFlags(ushort expectedResult, bool expectedZero, bool expectedNegative)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        // Determine input value that will produce the expected result
        ushort inputValue = (ushort)(expectedResult & 0x00FF);
        if ((expectedResult & 0xFF00) == 0xFF00)
        {
            inputValue |= 0x0080; // Ensure bit 7 is set for negative numbers
        }

        registerFile.WriteRegister(RegisterName.R4, inputValue);

        // Set initial flag states to test preservation/clearing
        registerFile.StatusRegister.Carry = true; // Should be preserved
        registerFile.StatusRegister.Overflow = true; // Should be cleared

        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
        Assert.False(registerFile.StatusRegister.Overflow); // Always cleared
        Assert.True(registerFile.StatusRegister.Carry); // Preserved
    }

    [Fact]
    public void Execute_IndirectMode_SignExtendsValueInMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort address = 0x1000;

        registerFile.WriteRegister(RegisterName.R4, address);

        // Set up memory with input value (little-endian)
        memory[address] = 0x80;     // Low byte (will be sign extended)
        memory[address + 1] = 0x12; // High byte (will be modified)

        var instruction = new SxtInstruction(
            0x1194,
            RegisterName.R4,
            AddressingMode.Indirect);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        // Check memory contains sign-extended value
        Assert.Equal(0x80, memory[address]);     // Low byte unchanged
        Assert.Equal(0xFF, memory[address + 1]); // High byte sign extended
        Assert.Equal(3u, cycles); // Indirect mode should take 3 cycles (1 base + 2 for indirect)
    }

    [Fact]
    public void Execute_IndirectMode_PositiveByteNoSignExtension()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort address = 0x1000;

        registerFile.WriteRegister(RegisterName.R4, address);

        // Set up memory with input value (little-endian)
        memory[address] = 0x7F;     // Low byte (positive)
        memory[address + 1] = 0xAB; // High byte (will be cleared)

        var instruction = new SxtInstruction(
            0x1194,
            RegisterName.R4,
            AddressingMode.Indirect);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        // Check memory contains correctly extended value
        Assert.Equal(0x7F, memory[address]);     // Low byte unchanged
        Assert.Equal(0x00, memory[address + 1]); // High byte cleared (no sign extension)
    }

    [Fact]
    public void Execute_IndexedMode_SignExtendsValueInMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort baseAddress = 0x1000;
        ushort offset = 0x0010;
        ushort targetAddress = (ushort)(baseAddress + offset);

        registerFile.WriteRegister(RegisterName.R4, baseAddress);

        // Set up memory with input value at target address (little-endian)
        memory[targetAddress] = 0xC0;     // Low byte (will be sign extended)
        memory[targetAddress + 1] = 0x12; // High byte (will be modified)

        var instruction = new SxtInstruction(
            0x11A4,
            RegisterName.R4,
            AddressingMode.Indexed);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, new ushort[] { offset });

        // Assert
        // Check memory contains sign-extended value
        Assert.Equal(0xC0, memory[targetAddress]);     // Low byte unchanged
        Assert.Equal(0xFF, memory[targetAddress + 1]); // High byte sign extended
        Assert.Equal(4u, cycles); // Indexed mode should take 4 cycles (1 base + 3 for indexed)
    }

    [Theory]
    [InlineData(AddressingMode.Register, 1u)]
    [InlineData(AddressingMode.Indirect, 3u)]
    [InlineData(AddressingMode.IndirectAutoIncrement, 3u)]
    [InlineData(AddressingMode.Indexed, 4u)]
    [InlineData(AddressingMode.Absolute, 4u)]
    [InlineData(AddressingMode.Symbolic, 4u)]
    public void Execute_CycleCounts_AreCorrect(AddressingMode mode, uint expectedCycles)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x1000);

        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            mode);

        ushort[] extensionWords = mode == AddressingMode.Indexed ||
                                  mode == AddressingMode.Absolute ||
                                  mode == AddressingMode.Symbolic ?
                                  new ushort[] { 0x0010 } : Array.Empty<ushort>();

        // Act
        uint actualCycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(expectedCycles, actualCycles);
    }

    [Theory]
    [InlineData(0x00, 0x0000)] // Zero byte
    [InlineData(0x01, 0x0001)] // Small positive
    [InlineData(0x7F, 0x007F)] // Largest positive byte
    [InlineData(0x80, 0xFF80)] // Smallest negative byte
    [InlineData(0xFF, 0xFFFF)] // Largest negative byte
    [InlineData(0x40, 0x0040)] // Mid-range positive
    [InlineData(0xC0, 0xFFC0)] // Mid-range negative
    public void Execute_VariousSignExtensionCases_ProducesCorrectResults(byte inputByte, ushort expectedResult)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, inputByte);

        var instruction = new SxtInstruction(
            0x1184,
            RegisterName.R4,
            AddressingMode.Register);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedResult, registerFile.ReadRegister(RegisterName.R4));
    }
}
