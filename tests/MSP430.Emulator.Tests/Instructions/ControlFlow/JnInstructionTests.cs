using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for JN (Jump if Negative) instruction.
/// 
/// Tests instruction properties, condition evaluation, and program counter calculation
/// according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class JnInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new JnInstruction(0x2000, 10);

        // Assert
        Assert.Equal(4, instruction.Opcode); // JN is condition code 4
    }

    [Fact]
    public void Mnemonic_AlwaysReturnsJN()
    {
        // Arrange & Act
        var instruction = new JnInstruction(0x2000, 0);

        // Assert
        Assert.Equal("JN", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(true, true)] // N=1 should jump
    [InlineData(false, false)] // N=0 should not jump
    public void Execute_NegativeFlag_UpdatesProgramCounterCorrectly(bool negativeFlag, bool shouldJump)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        var instruction = new JnInstruction(0x2000, 8);

        ushort originalPC = 0x1000;
        registerFile.SetProgramCounter(originalPC);
        registerFile.StatusRegister.Negative = negativeFlag;

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        if (shouldJump)
        {
            Assert.Equal(originalPC + (8 * 2), registerFile.GetProgramCounter());
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
        var instruction = new JnInstruction(0x2000, 8);

        registerFile.SetProgramCounter(0x1000);
        registerFile.StatusRegister.Negative = true; // Set up for jump

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(2u, cycles);
    }
}
