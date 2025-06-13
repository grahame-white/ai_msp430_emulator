using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for JC (Jump if Carry) instruction.
/// 
/// Tests instruction properties, condition evaluation, and program counter calculation
/// according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class JcInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new JcInstruction(0x2000, 10);

        // Assert
        Assert.Equal(2, instruction.Opcode); // JC is condition code 2
    }

    [Fact]
    public void Mnemonic_AlwaysReturnsJC()
    {
        // Arrange & Act
        var instruction = new JcInstruction(0x2000, 0);

        // Assert
        Assert.Equal("JC", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(true, true)] // C=1 should jump
    [InlineData(false, false)] // C=0 should not jump
    public void Execute_CarryFlag_UpdatesProgramCounterCorrectly(bool carryFlag, bool shouldJump)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        var instruction = new JcInstruction(0x2000, 20);

        ushort originalPC = 0x1000;
        registerFile.SetProgramCounter(originalPC);
        registerFile.StatusRegister.Carry = carryFlag;

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        if (shouldJump)
        {
            Assert.Equal(originalPC + (20 * 2), registerFile.GetProgramCounter());
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
        var instruction = new JcInstruction(0x2000, 20);

        registerFile.SetProgramCounter(0x1000);
        registerFile.StatusRegister.Carry = true; // Set up for jump

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(2u, cycles);
    }
}
