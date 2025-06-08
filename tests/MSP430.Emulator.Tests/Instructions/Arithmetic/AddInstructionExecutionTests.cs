using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;

namespace MSP430.Emulator.Tests.Instructions.Arithmetic;

/// <summary>
/// Unit tests for ADD instruction execution behavior.
/// 
/// Tests verify that ADD instruction execution matches MSP430FR2355 specifications:
/// - Operation: src + dst → dst
/// - Flags: N, Z, C, V affected per SLAU445I Section 4.6.2.2
/// - Addressing modes work correctly
/// - Byte vs word operations behave correctly
/// 
/// References:
/// - docs/references/SLAU445/4.6.2.2_add.md - ADD instruction specification
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.2: ADD instruction
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.3.3: Status Register (SR/R2)
/// </summary>
public class AddInstructionExecutionTests
{
    [Theory]
    [InlineData(0x1000, 0x2000, 0x3000, false, false, false, false)] // 0x1000 + 0x2000 = 0x3000, no flags
    [InlineData(0x8000, 0x8000, 0x0000, true, true, false, true)]    // 0x8000 + 0x8000 = 0x0000, carry, zero, overflow
    [InlineData(0x7FFF, 0x0001, 0x8000, false, false, true, true)]   // 0x7FFF + 0x0001 = 0x8000, negative, overflow
    [InlineData(0xFFFF, 0x0001, 0x0000, true, true, false, false)]   // 0xFFFF + 0x0001 = 0x0000, carry, zero
    [InlineData(0x0000, 0x0000, 0x0000, false, true, false, false)]  // 0x0000 + 0x0000 = 0x0000, zero
    public void Execute_WordOperation_SetsCorrectFlagsAndResult(
        ushort sourceValue,
        ushort destinationValue,
        ushort expectedResult,
        bool expectedCarry,
        bool expectedZero,
        bool expectedNegative,
        bool expectedOverflow)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000]; // 64KB memory

        // Set up source and destination values in registers
        registerFile.WriteRegister(RegisterName.R5, sourceValue);
        registerFile.WriteRegister(RegisterName.R6, destinationValue);

        var instruction = new AddInstruction(
            0x5560, // ADD R5, R6
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            false); // Word operation

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedResult, registerFile.ReadRegister(RegisterName.R6));
        Assert.Equal(expectedCarry, registerFile.StatusRegister.Carry);
        Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
        Assert.Equal(expectedOverflow, registerFile.StatusRegister.Overflow);
        Assert.True(cycles > 0, "Instruction should consume at least one cycle");
    }

    [Theory]
    [InlineData(0x10, 0x20, 0x30, false, false, false, false)] // 0x10 + 0x20 = 0x30, no flags
    [InlineData(0x80, 0x80, 0x00, true, true, false, true)]    // 0x80 + 0x80 = 0x00 (8-bit), carry, zero, overflow
    [InlineData(0x7F, 0x01, 0x80, false, false, true, true)]   // 0x7F + 0x01 = 0x80 (8-bit), negative, overflow
    [InlineData(0xFF, 0x01, 0x00, true, true, false, false)]   // 0xFF + 0x01 = 0x00 (8-bit), carry, zero
    [InlineData(0x00, 0x00, 0x00, false, true, false, false)]  // 0x00 + 0x00 = 0x00, zero
    public void Execute_ByteOperation_SetsCorrectFlagsAndResult(
        byte sourceValue,
        byte destinationValue,
        byte expectedResult,
        bool expectedCarry,
        bool expectedZero,
        bool expectedNegative,
        bool expectedOverflow)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000]; // 64KB memory

        // Set up source and destination values in registers (byte operations use low byte)
        registerFile.WriteRegister(RegisterName.R5, sourceValue);
        registerFile.WriteRegister(RegisterName.R6, destinationValue);

        var instruction = new AddInstruction(
            0x5560, // ADD.B R5, R6
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true); // Byte operation

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        ushort destinationRegister = registerFile.ReadRegister(RegisterName.R6);
        Assert.Equal(expectedResult, (byte)(destinationRegister & 0xFF));
        Assert.Equal(expectedCarry, registerFile.StatusRegister.Carry);
        Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
        Assert.Equal(expectedOverflow, registerFile.StatusRegister.Overflow);
        Assert.True(cycles > 0, "Instruction should consume at least one cycle");
    }

    [Fact]
    public void Execute_ImmediateMode_AddsCorrectValues()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set destination register
        registerFile.WriteRegister(RegisterName.R6, 0x1000);

        var instruction = new AddInstruction(
            0x5036, // ADD #0x0500, R6
            RegisterName.R0, // Immediate mode uses R0
            RegisterName.R6,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x0500 }; // Immediate value

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x1500, registerFile.ReadRegister(RegisterName.R6)); // 0x1000 + 0x0500 = 0x1500
        Assert.False(registerFile.StatusRegister.Carry);
        Assert.False(registerFile.StatusRegister.Zero);
        Assert.False(registerFile.StatusRegister.Negative);
        Assert.False(registerFile.StatusRegister.Overflow);
        Assert.True(cycles > 0, "Instruction should consume at least one cycle");
    }

    [Fact]
    public void Execute_MemoryToRegister_ReadsFromMemoryCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up memory with a value at address 0x2000
        memory[0x2000] = 0x34; // Low byte
        memory[0x2001] = 0x12; // High byte (0x1234 in little-endian)

        // Set destination register
        registerFile.WriteRegister(RegisterName.R6, 0x1000);

        var instruction = new AddInstruction(
            0x5026, // ADD &0x2000, R6
            RegisterName.R0, // Absolute mode uses R0  
            RegisterName.R6,
            AddressingMode.Absolute,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x2000 }; // Absolute address

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x2234, registerFile.ReadRegister(RegisterName.R6)); // 0x1000 + 0x1234 = 0x2234
        Assert.True(cycles > 0, "Instruction should consume at least one cycle");
    }

    [Fact]
    public void Execute_RegisterToMemory_WritesToMemoryCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up memory with initial value at address 0x2000
        memory[0x2000] = 0x00; // Low byte
        memory[0x2001] = 0x10; // High byte (0x1000 in little-endian)

        // Set source register
        registerFile.WriteRegister(RegisterName.R5, 0x0234);

        var instruction = new AddInstruction(
            0x5520, // ADD R5, &0x2000
            RegisterName.R5,
            RegisterName.R0, // Absolute mode uses R0
            AddressingMode.Register,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = { 0x2000 }; // Absolute address

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert - memory should contain 0x1000 + 0x0234 = 0x1234
        Assert.Equal(0x34, memory[0x2000]); // Low byte
        Assert.Equal(0x12, memory[0x2001]); // High byte
        Assert.True(cycles > 0, "Instruction should consume at least one cycle");
    }

    [Fact]
    public void Execute_OverflowConditions_DetectsPositiveOverflow()
    {
        // Arrange - two positive numbers that sum to negative
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        registerFile.WriteRegister(RegisterName.R5, 0x7FFF); // Largest positive 16-bit signed number
        registerFile.WriteRegister(RegisterName.R6, 0x0001);

        var instruction = new AddInstruction(
            0x5560,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x8000, registerFile.ReadRegister(RegisterName.R6)); // Result is negative
        Assert.True(registerFile.StatusRegister.Overflow, "Should detect overflow when two positive numbers sum to negative");
        Assert.True(registerFile.StatusRegister.Negative, "Result should be negative");
        Assert.False(registerFile.StatusRegister.Carry, "No carry from 16-bit addition");
    }

    [Fact]
    public void Execute_OverflowConditions_DetectsNegativeOverflow()
    {
        // Arrange - two negative numbers that sum to positive
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        registerFile.WriteRegister(RegisterName.R5, 0x8000); // Most negative 16-bit signed number
        registerFile.WriteRegister(RegisterName.R6, 0x8000);

        var instruction = new AddInstruction(
            0x5560,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0000, registerFile.ReadRegister(RegisterName.R6)); // Result wraps to zero
        Assert.True(registerFile.StatusRegister.Overflow, "Should detect overflow when two negative numbers sum to positive");
        Assert.True(registerFile.StatusRegister.Zero, "Result should be zero");
        Assert.True(registerFile.StatusRegister.Carry, "Should have carry from 16-bit addition");
    }
}
