using System;
using System.Collections.Generic;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using MSP430.Emulator.Tests.TestUtilities;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for the FormatIIIInstruction class.
/// 
/// Tests jump instruction execution, condition evaluation, offset calculation,
/// and program counter modification for all 8 jump conditions.
/// 
/// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019,
/// Section 4.5.1.3: "Jump Instructions" - Table 4-11 and Figure 4-16.
/// </summary>
public class FormatIIIInstructionTests
{
    /// <summary>
    /// Helper method to create a FormatIIIInstruction with the specified condition and offset.
    /// </summary>
    /// <param name="condition">The jump condition.</param>
    /// <param name="offset">The 10-bit signed offset.</param>
    /// <returns>A new FormatIIIInstruction instance.</returns>
    private static FormatIIIInstruction CreateJumpInstruction(JumpCondition condition, short offset)
    {
        // Format III instruction word: 001 + 3-bit condition + 10-bit offset
        ushort instructionWord = (ushort)(0x2000 | ((byte)condition << 10) | (offset & 0x03FF));
        return new FormatIIIInstruction((byte)condition, instructionWord, offset);
    }

    #region Constructor Tests

    [Theory]
    [InlineData(0, 0x2000)]  // JEQ with offset 0
    [InlineData(1, 0x2400)]  // JNE with offset 0  
    [InlineData(2, 0x2800)]  // JC with offset 0
    [InlineData(3, 0x2C00)]  // JNC with offset 0
    [InlineData(4, 0x3000)]  // JN with offset 0
    [InlineData(5, 0x3400)]  // JGE with offset 0
    [InlineData(6, 0x3800)]  // JL with offset 0
    [InlineData(7, 0x3C00)]  // JMP with offset 0
    public void Constructor_VariousConditions_SetsFormatIII(byte conditionCode, ushort instructionWord)
    {
        var instruction = new FormatIIIInstruction(conditionCode, instructionWord, 0);
        Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
    }

    [Theory]
    [InlineData(JumpCondition.JEQ, 5, 5)]
    [InlineData(JumpCondition.JNE, -10, -10)]
    [InlineData(JumpCondition.JMP, 511, 511)]
    [InlineData(JumpCondition.JL, -512, -512)]
    public void Constructor_VariousOffsets_SetsOffset(JumpCondition condition, short offset, short expected)
    {
        var instruction = CreateJumpInstruction(condition, offset);
        Assert.Equal(expected, instruction.Offset);
    }

    [Theory]
    [InlineData(JumpCondition.JEQ, JumpCondition.JEQ)]
    [InlineData(JumpCondition.JNE, JumpCondition.JNE)]
    [InlineData(JumpCondition.JC, JumpCondition.JC)]
    [InlineData(JumpCondition.JNC, JumpCondition.JNC)]
    [InlineData(JumpCondition.JN, JumpCondition.JN)]
    [InlineData(JumpCondition.JGE, JumpCondition.JGE)]
    [InlineData(JumpCondition.JL, JumpCondition.JL)]
    [InlineData(JumpCondition.JMP, JumpCondition.JMP)]
    public void Constructor_VariousConditions_SetsCondition(JumpCondition input, JumpCondition expected)
    {
        var instruction = CreateJumpInstruction(input, 0);
        Assert.Equal(expected, instruction.Condition);
    }

    #endregion

    #region Mnemonic Tests

    [Theory]
    [InlineData(JumpCondition.JEQ, "JEQ")]
    [InlineData(JumpCondition.JNE, "JNE")]
    [InlineData(JumpCondition.JC, "JC")]
    [InlineData(JumpCondition.JNC, "JNC")]
    [InlineData(JumpCondition.JN, "JN")]
    [InlineData(JumpCondition.JGE, "JGE")]
    [InlineData(JumpCondition.JL, "JL")]
    [InlineData(JumpCondition.JMP, "JMP")]
    public void Mnemonic_VariousConditions_ReturnsCorrectMnemonic(JumpCondition condition, string expected)
    {
        var instruction = CreateJumpInstruction(condition, 0);
        Assert.Equal(expected, instruction.Mnemonic);
    }

    #endregion

    #region ToString Tests

    [Theory]
    [InlineData(JumpCondition.JEQ, 5, "JEQ +5")]
    [InlineData(JumpCondition.JNE, -10, "JNE -10")]
    [InlineData(JumpCondition.JMP, 0, "JMP 0")]
    public void ToString_VariousConditionsAndOffsets_FormatsCorrectly(JumpCondition condition, short offset, string expected)
    {
        var instruction = CreateJumpInstruction(condition, offset);
        Assert.Equal(expected, instruction.ToString());
    }

    #endregion

    #region Condition Evaluation Tests

    [Fact]
    public void Execute_JEQ_ZeroFlagSet_JumpsCorrectly()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        registerFile.SetProgramCounter(0x1000);
        registerFile.StatusRegister.Zero = true;
        
        var instruction = CreateJumpInstruction(JumpCondition.JEQ, 5); // Jump forward 10 bytes

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        ushort expectedPC = (ushort)(0x1000 + 2 + (5 * 2)); // 0x100C
        Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        Assert.Equal(2u, cycles);
    }

    [Fact]
    public void Execute_JEQ_ZeroFlagClear_DoesNotJump()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        ushort originalPC = 0x1000;
        registerFile.SetProgramCounter(originalPC);
        registerFile.StatusRegister.Zero = false;
        
        var instruction = CreateJumpInstruction(JumpCondition.JEQ, 5);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(originalPC, registerFile.GetProgramCounter()); // PC unchanged
        Assert.Equal(2u, cycles);
    }

    [Fact]
    public void Execute_JNE_ZeroFlagClear_JumpsCorrectly()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        registerFile.SetProgramCounter(0x1000);
        registerFile.StatusRegister.Zero = false;
        
        var instruction = CreateJumpInstruction(JumpCondition.JNE, -3); // Jump back 6 bytes

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        ushort expectedPC = (ushort)(0x1000 + 2 + (-3 * 2)); // 0x0FFA
        Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        Assert.Equal(2u, cycles);
    }

    [Fact]
    public void Execute_JC_CarryFlagSet_JumpsCorrectly()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        registerFile.SetProgramCounter(0x2000);
        registerFile.StatusRegister.Carry = true;
        
        var instruction = CreateJumpInstruction(JumpCondition.JC, 10);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        ushort expectedPC = (ushort)(0x2000 + 2 + (10 * 2)); // 0x2016
        Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        Assert.Equal(2u, cycles);
    }

    [Fact]
    public void Execute_JNC_CarryFlagClear_JumpsCorrectly()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        registerFile.SetProgramCounter(0x3000);
        registerFile.StatusRegister.Carry = false;
        
        var instruction = CreateJumpInstruction(JumpCondition.JNC, 8);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        ushort expectedPC = (ushort)(0x3000 + 2 + (8 * 2)); // 0x3012
        Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        Assert.Equal(2u, cycles);
    }

    [Fact]
    public void Execute_JN_NegativeFlagSet_JumpsCorrectly()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        registerFile.SetProgramCounter(0x4000);
        registerFile.StatusRegister.Negative = true;
        
        var instruction = CreateJumpInstruction(JumpCondition.JN, -5);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        ushort expectedPC = (ushort)(0x4000 + 2 + (-5 * 2)); // 0x3FF8
        Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        Assert.Equal(2u, cycles);
    }

    [Theory]
    [InlineData(false, false, true)]  // N=0, V=0 -> N⊕V=0 -> JGE jumps
    [InlineData(true, true, true)]    // N=1, V=1 -> N⊕V=0 -> JGE jumps
    [InlineData(false, true, false)]  // N=0, V=1 -> N⊕V=1 -> JGE doesn't jump
    [InlineData(true, false, false)]  // N=1, V=0 -> N⊕V=1 -> JGE doesn't jump
    public void Execute_JGE_VariousFlagCombinations_JumpsCorrectly(bool negative, bool overflow, bool shouldJump)
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        ushort originalPC = 0x5000;
        registerFile.SetProgramCounter(originalPC);
        registerFile.StatusRegister.Negative = negative;
        registerFile.StatusRegister.Overflow = overflow;
        
        var instruction = CreateJumpInstruction(JumpCondition.JGE, 4);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        if (shouldJump)
        {
            ushort expectedPC = (ushort)(originalPC + 2 + (4 * 2)); // 0x500A
            Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        }
        else
        {
            Assert.Equal(originalPC, registerFile.GetProgramCounter());
        }
        Assert.Equal(2u, cycles);
    }

    [Theory]
    [InlineData(false, false, false)] // N=0, V=0 -> N⊕V=0 -> JL doesn't jump
    [InlineData(true, true, false)]   // N=1, V=1 -> N⊕V=0 -> JL doesn't jump
    [InlineData(false, true, true)]   // N=0, V=1 -> N⊕V=1 -> JL jumps
    [InlineData(true, false, true)]   // N=1, V=0 -> N⊕V=1 -> JL jumps
    public void Execute_JL_VariousFlagCombinations_JumpsCorrectly(bool negative, bool overflow, bool shouldJump)
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        ushort originalPC = 0x6000;
        registerFile.SetProgramCounter(originalPC);
        registerFile.StatusRegister.Negative = negative;
        registerFile.StatusRegister.Overflow = overflow;
        
        var instruction = CreateJumpInstruction(JumpCondition.JL, 3);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        if (shouldJump)
        {
            ushort expectedPC = (ushort)(originalPC + 2 + (3 * 2)); // 0x6008
            Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        }
        else
        {
            Assert.Equal(originalPC, registerFile.GetProgramCounter());
        }
        Assert.Equal(2u, cycles);
    }

    [Fact]
    public void Execute_JMP_AlwaysJumps()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        registerFile.SetProgramCounter(0x8000);
        // Set all flags to ensure JMP ignores them
        registerFile.StatusRegister.Zero = true;
        registerFile.StatusRegister.Carry = true;
        registerFile.StatusRegister.Negative = true;
        registerFile.StatusRegister.Overflow = true;
        
        var instruction = CreateJumpInstruction(JumpCondition.JMP, 20);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        ushort expectedPC = (ushort)(0x8000 + 2 + (20 * 2)); // 0x802A
        Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        Assert.Equal(2u, cycles);
    }

    #endregion

    #region Offset Range Tests

    [Theory]
    [InlineData(511)]   // Maximum positive offset (+1022 bytes)
    [InlineData(-512)]  // Maximum negative offset (-1024 bytes)
    [InlineData(0)]     // Zero offset
    [InlineData(256)]   // Arbitrary positive offset
    [InlineData(-256)]  // Arbitrary negative offset
    public void Execute_ValidOffsetRange_CalculatesTargetCorrectly(short offset)
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        ushort basePC = 0x8000;
        registerFile.SetProgramCounter(basePC);
        
        var instruction = CreateJumpInstruction(JumpCondition.JMP, offset);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        ushort expectedPC = (ushort)(basePC + 2 + (offset * 2));
        Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        Assert.Equal(2u, cycles);
    }

    [Theory]
    [InlineData(512)]   // One beyond maximum positive
    [InlineData(-513)]  // One beyond maximum negative
    [InlineData(1000)]  // Far beyond maximum positive
    [InlineData(-1000)] // Far beyond maximum negative
    public void Execute_InvalidOffsetRange_ThrowsInvalidOperationException(short offset)
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        registerFile.SetProgramCounter(0x8000);
        
        var instruction = CreateJumpInstruction(JumpCondition.JMP, offset);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => instruction.Execute(registerFile, memory, extensionWords));
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Execute_JumpTo16BitBoundary_ThrowsInvalidOperationException()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        // Set PC such that jump target would exceed 16-bit range
        registerFile.SetProgramCounter(0xFFFE);
        
        var instruction = CreateJumpInstruction(JumpCondition.JMP, 1); // Would jump to 0x10002

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => instruction.Execute(registerFile, memory, extensionWords));
    }

    [Fact]
    public void Execute_JumpToNegativeAddress_ThrowsInvalidOperationException()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        // Set PC such that jump target would be negative
        registerFile.SetProgramCounter(0x0002);
        
        var instruction = CreateJumpInstruction(JumpCondition.JMP, -2); // Would jump to -2

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => instruction.Execute(registerFile, memory, extensionWords));
    }

    [Fact]
    public void Execute_JumpToOddAddress_ThrowsInvalidOperationException()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        // Set PC to an odd address - this should create an odd target
        // PC=0x1001, offset=0 -> target = 0x1001 + 2 + 0 = 0x1003 (odd)
        registerFile.SetProgramCounter(0x1001);
        
        var instruction = CreateJumpInstruction(JumpCondition.JMP, 0);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => instruction.Execute(registerFile, memory, extensionWords));
    }

    [Fact]
    public void Execute_AllConditions_Return2Cycles()
    {
        // Arrange
        var (registerFile, memory) = TestEnvironmentHelper.CreateTestEnvironment();
        var extensionWords = Array.Empty<ushort>();
        
        registerFile.SetProgramCounter(0x8000);
        
        // Set flags to make all conditions true
        registerFile.StatusRegister.Zero = true;
        registerFile.StatusRegister.Carry = true;
        registerFile.StatusRegister.Negative = true;
        registerFile.StatusRegister.Overflow = true;

        var conditions = new[]
        {
            JumpCondition.JEQ, JumpCondition.JNE, JumpCondition.JC, JumpCondition.JNC,
            JumpCondition.JN, JumpCondition.JGE, JumpCondition.JL, JumpCondition.JMP
        };

        foreach (var condition in conditions)
        {
            // Act
            var instruction = CreateJumpInstruction(condition, 5);
            uint cycles = instruction.Execute(registerFile, memory, extensionWords);

            // Assert
            Assert.Equal(2u, cycles);
            
            // Reset PC for next test
            registerFile.SetProgramCounter(0x8000);
        }
    }

    #endregion
}