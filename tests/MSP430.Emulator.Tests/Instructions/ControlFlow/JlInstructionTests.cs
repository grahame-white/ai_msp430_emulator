using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for JL (Jump if Less) instruction.
/// 
/// Tests instruction properties, condition evaluation, and program counter calculation
/// according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class JlInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new JlInstruction(0x2000, 10);

        // Assert
        Assert.Equal(6, instruction.Opcode); // JL is condition code 6
    }

    [Fact]
    public void Mnemonic_AlwaysReturnsJL()
    {
        // Arrange & Act
        var instruction = new JlInstruction(0x2000, 0);

        // Assert
        Assert.Equal("JL", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(true, true, false)]  // N=1, V=1 → N⊕V=0 → JL doesn't jump
    [InlineData(false, false, false)] // N=0, V=0 → N⊕V=0 → JL doesn't jump
    [InlineData(true, false, true)]  // N=1, V=0 → N⊕V=1 → JL jumps
    [InlineData(false, true, true)]  // N=0, V=1 → N⊕V=1 → JL jumps
    public void Execute_NegativeXorOverflow_UpdatesProgramCounterCorrectly(bool negative, bool overflow, bool shouldJump)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        var instruction = new JlInstruction(0x2000, -8);

        ushort originalPC = 0x1000;
        registerFile.SetProgramCounter(originalPC);
        registerFile.StatusRegister.Negative = negative;
        registerFile.StatusRegister.Overflow = overflow;

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        if (shouldJump)
        {
            Assert.Equal(originalPC + (-8 * 2), registerFile.GetProgramCounter());
        }
        else
        {
            Assert.Equal(originalPC, registerFile.GetProgramCounter());
        }
    }

    [Fact]
    public void Execute_AlwaysTakesTwoCycles()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        var instruction = new JlInstruction(0x2000, -8);

        registerFile.SetProgramCounter(0x1000);
        registerFile.StatusRegister.Negative = true;
        registerFile.StatusRegister.Overflow = false; // Set up for jump (N⊕V=1)

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(2u, cycles);
    }
}
