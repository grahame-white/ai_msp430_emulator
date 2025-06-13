using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for JGE (Jump if Greater or Equal) instruction.
/// 
/// Tests instruction properties, condition evaluation, and program counter calculation
/// according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class JgeInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new JgeInstruction(0x2000, 10);

        // Assert
        Assert.Equal(5, instruction.Opcode); // JGE is condition code 5
    }

    [Fact]
    public void Mnemonic_AlwaysReturnsJGE()
    {
        // Arrange & Act
        var instruction = new JgeInstruction(0x2000, 0);

        // Assert
        Assert.Equal("JGE", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(true, true, true)]   // N=1, V=1 → N⊕V=0 → JGE jumps
    [InlineData(false, false, true)] // N=0, V=0 → N⊕V=0 → JGE jumps
    [InlineData(true, false, false)] // N=1, V=0 → N⊕V=1 → JGE doesn't jump
    [InlineData(false, true, false)] // N=0, V=1 → N⊕V=1 → JGE doesn't jump
    public void Execute_NegativeXorOverflow_BehavesCorrectly(bool negative, bool overflow, bool shouldJump)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        var instruction = new JgeInstruction(0x2000, 12);

        ushort originalPC = 0x1000;
        registerFile.SetProgramCounter(originalPC);
        registerFile.StatusRegister.Negative = negative;
        registerFile.StatusRegister.Overflow = overflow;

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        if (shouldJump)
        {
            Assert.Equal(originalPC + (12 * 2), registerFile.GetProgramCounter());
        }
        else
        {
            Assert.Equal(originalPC, registerFile.GetProgramCounter());
        }
        Assert.Equal(2u, cycles);
    }
}
