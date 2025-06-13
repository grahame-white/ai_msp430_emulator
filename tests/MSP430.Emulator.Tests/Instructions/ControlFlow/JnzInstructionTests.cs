using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for JNZ (Jump if Not Zero) instruction.
/// 
/// Tests instruction properties, condition evaluation, and program counter calculation
/// according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class JnzInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new JnzInstruction(0x2000, 10);

        // Assert
        Assert.Equal(1, instruction.Opcode); // JNZ is condition code 1
    }

    [Fact]
    public void Mnemonic_AlwaysReturnsJNZ()
    {
        // Arrange & Act
        var instruction = new JnzInstruction(0x2000, 0);

        // Assert
        Assert.Equal("JNZ", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(true, false)] // Z=1 should not jump
    [InlineData(false, true)] // Z=0 should jump
    public void Execute_ZeroFlag_UpdatesProgramCounterCorrectly(bool zeroFlag, bool shouldJump)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        var instruction = new JnzInstruction(0x2000, -5);

        ushort originalPC = 0x1000;
        registerFile.SetProgramCounter(originalPC);
        registerFile.StatusRegister.Zero = zeroFlag;

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        if (shouldJump)
        {
            Assert.Equal(originalPC + (-5 * 2), registerFile.GetProgramCounter());
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
        var instruction = new JnzInstruction(0x2000, -5);

        registerFile.SetProgramCounter(0x1000);
        registerFile.StatusRegister.Zero = false; // Set up for jump

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(2u, cycles);
    }
}
