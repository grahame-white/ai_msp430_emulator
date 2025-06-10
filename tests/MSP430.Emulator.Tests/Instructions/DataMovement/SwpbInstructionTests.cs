using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.DataMovement;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.DataMovement;

/// <summary>
/// Unit tests for the SwpbInstruction class.
/// Tests byte swapping operations, flag setting, and addressing modes.
/// </summary>
public class SwpbInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsFormat()
    {
        // Arrange & Act
        var instruction = new SwpbInstruction(
            0x1084,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal(InstructionFormat.FormatII, instruction.Format);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new SwpbInstruction(
            0x1084,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal(0x10, instruction.Opcode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsInstructionWord()
    {
        // Arrange & Act
        var instruction = new SwpbInstruction(
            0x1084,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal(0x1084, instruction.InstructionWord);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsMnemonic()
    {
        // Arrange & Act
        var instruction = new SwpbInstruction(
            0x1084,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal("SWPB", instruction.Mnemonic);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsIsByteOperationToFalse()
    {
        // Arrange & Act
        var instruction = new SwpbInstruction(
            0x1084,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.False(instruction.IsByteOperation);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsDestinationRegister()
    {
        // Arrange & Act
        var instruction = new SwpbInstruction(
            0x1084,
            RegisterName.R4,
            AddressingMode.Register);

        // Assert
        Assert.Equal(RegisterName.R4, instruction.DestinationRegister);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsDestinationAddressingMode()
    {
        // Arrange & Act
        var instruction = new SwpbInstruction(
            0x1084,
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
        var instruction = new SwpbInstruction(
            0x1084,
            RegisterName.R4,
            mode);

        // Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    [Fact]
    public void ToString_RegisterMode_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new SwpbInstruction(
            0x1084,
            RegisterName.R4,
            AddressingMode.Register);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("SWPB R4", result);
    }

    [Fact]
    public void ToString_IndirectMode_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new SwpbInstruction(
            0x1094,
            RegisterName.R4,
            AddressingMode.Indirect);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("SWPB @R4", result);
    }

    [Theory]
    [InlineData(0x1234, 0x3412)] // Normal case: swap 0x12 and 0x34
    [InlineData(0xABCD, 0xCDAB)] // Another case: swap 0xAB and 0xCD
    [InlineData(0x0000, 0x0000)] // Zero case
    [InlineData(0xFF00, 0x00FF)] // High byte only
    [InlineData(0x00FF, 0xFF00)] // Low byte only
    [InlineData(0x8040, 0x4080)] // Sign bit cases
    public void Execute_RegisterMode_SwapsBytesCorrectly(ushort inputValue, ushort expectedResult)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, inputValue);

        var instruction = new SwpbInstruction(
            0x1084,
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
    [InlineData(0x8000, false, true)] // Negative result: Z=0, N=1
    [InlineData(0x1234, false, false)] // Positive non-zero: Z=0, N=0
    [InlineData(0x7FFF, false, false)] // Positive max: Z=0, N=0
    public void Execute_FlagUpdates_SetsZeroAndNegativeFlags(ushort result, bool expectedZero, bool expectedNegative)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        // Calculate input value that will produce the expected result
        ushort inputValue = (ushort)(((result & 0xFF) << 8) | ((result & 0xFF00) >> 8));
        registerFile.WriteRegister(RegisterName.R4, inputValue);

        // Set initial flag states to test preservation/clearing
        registerFile.StatusRegister.Carry = true; // Should be preserved
        registerFile.StatusRegister.Overflow = true; // Should be cleared

        var instruction = new SwpbInstruction(
            0x1084,
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
    public void Execute_IndirectMode_SwapsBytesInMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort address = 0x1000;

        registerFile.WriteRegister(RegisterName.R4, address);

        // Set up memory with input value (little-endian)
        memory[address] = 0x34;     // Low byte
        memory[address + 1] = 0x12; // High byte

        var instruction = new SwpbInstruction(
            0x1094,
            RegisterName.R4,
            AddressingMode.Indirect);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        // Check memory contains swapped value
        Assert.Equal(0x12, memory[address]);     // Low byte after swap
        Assert.Equal(0x34, memory[address + 1]); // High byte after swap
        Assert.Equal(3u, cycles); // Indirect mode should take 3 cycles (1 base + 2 for indirect)
    }

    [Fact]
    public void Execute_IndexedMode_SwapsBytesInMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        ushort baseAddress = 0x1000;
        ushort offset = 0x0010;
        ushort targetAddress = (ushort)(baseAddress + offset);

        registerFile.WriteRegister(RegisterName.R4, baseAddress);

        // Set up memory with input value at target address (little-endian)
        memory[targetAddress] = 0xCD;     // Low byte
        memory[targetAddress + 1] = 0xAB; // High byte

        var instruction = new SwpbInstruction(
            0x10A4,
            RegisterName.R4,
            AddressingMode.Indexed);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, new ushort[] { offset });

        // Assert
        // Check memory contains swapped value
        Assert.Equal(0xAB, memory[targetAddress]);     // Low byte after swap
        Assert.Equal(0xCD, memory[targetAddress + 1]); // High byte after swap
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

        var instruction = new SwpbInstruction(
            0x1084,
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
}
