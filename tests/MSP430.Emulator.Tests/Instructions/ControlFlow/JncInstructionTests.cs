using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.ControlFlow;
using MSP430.Emulator.Tests.TestUtilities;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for JNC (Jump if No Carry) instruction.
/// 
/// Tests instruction properties, condition evaluation, and program counter calculation
/// according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class JncInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new JncInstruction(0x2000, 10);

        // Assert
        Assert.Equal(3, instruction.Opcode); // JNC is condition code 3
    }

    [Fact]
    public void Mnemonic_AlwaysReturnsJNC()
    {
        // Arrange & Act
        var instruction = new JncInstruction(0x2000, 0);

        // Assert
        Assert.Equal("JNC", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(true, false)] // C=1 should not jump
    [InlineData(false, true)] // C=0 should jump
    public void Execute_CarryFlag_UpdatesProgramCounterCorrectly(bool carryFlag, bool shouldJump)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new JncInstruction(0x2000, -15);

        ushort originalPC = 0x1000;
        registerFile.SetProgramCounter(originalPC);
        registerFile.StatusRegister.Carry = carryFlag;

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        if (shouldJump)
        {
            Assert.Equal(originalPC + (-15 * 2), registerFile.GetProgramCounter());
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
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new JncInstruction(0x2000, -15);

        registerFile.SetProgramCounter(0x1000);
        registerFile.StatusRegister.Carry = false; // Set up for jump

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(2u, cycles);
    }
}
