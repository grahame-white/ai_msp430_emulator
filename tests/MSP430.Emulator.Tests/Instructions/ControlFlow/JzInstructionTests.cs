using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for JZ (Jump if Zero) instruction.
/// 
/// Tests instruction properties, condition evaluation, and program counter calculation
/// according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class JzInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsFormat()
    {
        // Arrange & Act
        var instruction = new JzInstruction(0x2000, 10);

        // Assert
        Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new JzInstruction(0x2000, 10);

        // Assert
        Assert.Equal(0, instruction.Opcode); // JZ is condition code 0
    }

    [Fact]
    public void Mnemonic_AlwaysReturnsJZ()
    {
        // Arrange & Act
        var instruction = new JzInstruction(0x2000, 0);

        // Assert
        Assert.Equal("JZ", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(true, true)] // Z=1 should jump
    [InlineData(false, false)] // Z=0 should not jump
    public void Execute_ZeroFlag_BehavesCorrectly(bool zeroFlag, bool shouldJump)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        var instruction = new JzInstruction(0x2000, 10);

        ushort originalPC = 0x1000;
        registerFile.SetProgramCounter(originalPC);
        registerFile.StatusRegister.Zero = zeroFlag;

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        if (shouldJump)
        {
            Assert.Equal(originalPC + (10 * 2), registerFile.GetProgramCounter());
        }
        else
        {
            Assert.Equal(originalPC, registerFile.GetProgramCounter());
        }
        Assert.Equal(2u, cycles);
    }
}
