using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions;

/// <summary>
/// Unit tests for Format III (Jump) instruction execution.
/// 
/// Tests all 8 jump conditions, offset range validation, program counter calculation,
/// and cycle count according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class FormatIIIInstructionTests
{
    /// <summary>
    /// Tests for basic instruction properties and construction.
    /// </summary>
    public class BasicPropertiesTests
    {
        [Fact]
        public void Constructor_ValidParameters_SetsFormat()
        {
            // Arrange & Act
            var instruction = new FormatIIIInstruction(0, 0x2000, 10);

            // Assert
            Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
        }

        [Fact]
        public void Constructor_ValidParameters_SetsOpcode()
        {
            // Arrange & Act
            var instruction = new FormatIIIInstruction(5, 0x2000, 10);

            // Assert
            Assert.Equal(5, instruction.Opcode);
        }

        [Fact]
        public void Constructor_ValidParameters_SetsOffset()
        {
            // Arrange & Act
            var instruction = new FormatIIIInstruction(0, 0x2000, -25);

            // Assert
            Assert.Equal(-25, instruction.Offset);
        }

        [Theory]
        [InlineData(0, "JEQ")]
        [InlineData(1, "JNE")]
        [InlineData(2, "JC")]
        [InlineData(3, "JNC")]
        [InlineData(4, "JN")]
        [InlineData(5, "JGE")]
        [InlineData(6, "JL")]
        [InlineData(7, "JMP")]
        public void Mnemonic_ValidConditionCodes_ReturnsCorrectMnemonic(ushort conditionCode, string expectedMnemonic)
        {
            // Arrange & Act
            var instruction = new FormatIIIInstruction(conditionCode, 0x2000, 0);

            // Assert
            Assert.Equal(expectedMnemonic, instruction.Mnemonic);
        }

        [Fact]
        public void Mnemonic_InvalidConditionCode_ReturnsFallbackMnemonic()
        {
            // Arrange & Act
            var instruction = new FormatIIIInstruction(8, 0x2000, 0);

            // Assert
            Assert.Equal("JUMP_8", instruction.Mnemonic);
        }

        [Fact]
        public void ToString_PositiveOffset_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new FormatIIIInstruction(0, 0x2000, 5);

            // Assert
            Assert.Equal("JEQ +5", instruction.ToString());
        }

        [Fact]
        public void ToString_NegativeOffset_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new FormatIIIInstruction(7, 0x2000, -10);

            // Assert
            Assert.Equal("JMP -10", instruction.ToString());
        }

        [Fact]
        public void ToString_ZeroOffset_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new FormatIIIInstruction(2, 0x2000, 0);

            // Assert
            Assert.Equal("JC 0", instruction.ToString());
        }
    }

    /// <summary>
    /// Tests for jump condition evaluation logic.
    /// </summary>
    public class JumpConditionTests
    {
        [Fact]
        public void Execute_JEQ_ZeroFlagSet_PerformsJump()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(0, 0x2000, 10); // JEQ

            registerFile.SetProgramCounter(0x1000);
            registerFile.StatusRegister.Zero = true;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1000 + (10 * 2), registerFile.GetProgramCounter());
        }

        [Fact]
        public void Execute_JEQ_ZeroFlagClear_DoesNotJump()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(0, 0x2000, 10); // JEQ

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);
            registerFile.StatusRegister.Zero = false;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(originalPC, registerFile.GetProgramCounter());
        }

        [Fact]
        public void Execute_JNE_ZeroFlagClear_PerformsJump()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(1, 0x2000, -5); // JNE

            registerFile.SetProgramCounter(0x1000);
            registerFile.StatusRegister.Zero = false;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1000 + (-5 * 2), registerFile.GetProgramCounter());
        }

        [Fact]
        public void Execute_JNE_ZeroFlagSet_DoesNotJump()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(1, 0x2000, -5); // JNE

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);
            registerFile.StatusRegister.Zero = true;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(originalPC, registerFile.GetProgramCounter());
        }

        [Fact]
        public void Execute_JC_CarryFlagSet_PerformsJump()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(2, 0x2000, 20); // JC

            registerFile.SetProgramCounter(0x1000);
            registerFile.StatusRegister.Carry = true;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1000 + (20 * 2), registerFile.GetProgramCounter());
        }

        [Fact]
        public void Execute_JNC_CarryFlagClear_PerformsJump()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(3, 0x2000, 15); // JNC

            registerFile.SetProgramCounter(0x1000);
            registerFile.StatusRegister.Carry = false;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1000 + (15 * 2), registerFile.GetProgramCounter());
        }

        [Fact]
        public void Execute_JN_NegativeFlagSet_PerformsJump()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(4, 0x2000, 8); // JN

            registerFile.SetProgramCounter(0x1000);
            registerFile.StatusRegister.Negative = true;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1000 + (8 * 2), registerFile.GetProgramCounter());
        }

        [Theory]
        [InlineData(true, true, true)]   // N=1, V=1 → N⊕V=0 → JGE jumps
        [InlineData(false, false, true)] // N=0, V=0 → N⊕V=0 → JGE jumps
        [InlineData(true, false, false)] // N=1, V=0 → N⊕V=1 → JGE doesn't jump
        [InlineData(false, true, false)] // N=0, V=1 → N⊕V=1 → JGE doesn't jump
        public void Execute_JGE_NegativeXorOverflow_BehavesCorrectly(bool negative, bool overflow, bool shouldJump)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(5, 0x2000, 12); // JGE

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);
            registerFile.StatusRegister.Negative = negative;
            registerFile.StatusRegister.Overflow = overflow;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            if (shouldJump)
            {
                Assert.Equal(originalPC + (12 * 2), registerFile.GetProgramCounter());
            }
            else
            {
                Assert.Equal(originalPC, registerFile.GetProgramCounter());
            }
        }

        [Theory]
        [InlineData(true, true, false)]  // N=1, V=1 → N⊕V=0 → JL doesn't jump
        [InlineData(false, false, false)] // N=0, V=0 → N⊕V=0 → JL doesn't jump
        [InlineData(true, false, true)]  // N=1, V=0 → N⊕V=1 → JL jumps
        [InlineData(false, true, true)]  // N=0, V=1 → N⊕V=1 → JL jumps
        public void Execute_JL_NegativeXorOverflow_BehavesCorrectly(bool negative, bool overflow, bool shouldJump)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(6, 0x2000, -8); // JL

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
        public void Execute_JMP_AlwaysJumps()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(7, 0x2000, 100); // JMP

            registerFile.SetProgramCounter(0x1000);
            // Set all flags to false to ensure unconditional jump
            registerFile.StatusRegister.Zero = false;
            registerFile.StatusRegister.Carry = false;
            registerFile.StatusRegister.Negative = false;
            registerFile.StatusRegister.Overflow = false;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1000 + (100 * 2), registerFile.GetProgramCounter());
        }

        [Fact]
        public void Execute_InvalidConditionCode_DoesNotJump()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(8, 0x2000, 10); // Invalid condition

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(originalPC, registerFile.GetProgramCounter());
        }
    }

    /// <summary>
    /// Tests for offset range validation and program counter calculation.
    /// </summary>
    public class OffsetAndPCTests
    {
        [Theory]
        [InlineData(-512)] // Minimum valid offset
        [InlineData(511)]  // Maximum valid offset
        [InlineData(0)]    // Zero offset
        [InlineData(-256)] // Negative offset
        [InlineData(256)]  // Positive offset
        public void Execute_ValidOffsetRange_DoesNotThrow(short offset)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(7, 0x2000, offset); // JMP

            registerFile.SetProgramCounter(0x8000); // Middle of address space

            // Act & Assert
            Exception exception = Record.Exception(() => instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(-513)] // Below minimum valid offset
        [InlineData(512)]  // Above maximum valid offset
        [InlineData(-1000)] // Far below minimum
        [InlineData(1000)]  // Far above maximum
        public void Execute_InvalidOffsetRange_ThrowsInvalidOperationException(short offset)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(7, 0x2000, offset); // JMP

            registerFile.SetProgramCounter(0x8000);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
        }

        [Theory]
        [InlineData(0x1000, 10, 0x1000 + (10 * 2))]
        [InlineData(0x2000, -5, 0x2000 + (-5 * 2))]
        [InlineData(0x8000, 0, 0x8000)]
        [InlineData(0x4000, 250, 0x4000 + (250 * 2))]
        [InlineData(0x6000, -250, 0x6000 + (-250 * 2))]
        public void Execute_PCCalculation_FollowsFormula(ushort currentPC, short offset, ushort expectedPC)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(7, 0x2000, offset); // JMP (always jumps)

            registerFile.SetProgramCounter(currentPC);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        }
    }

    /// <summary>
    /// Tests for CPU cycle count according to SLAU445I specification.
    /// </summary>
    public class CycleCountTests
    {
        [Theory]
        [InlineData(0)] // JEQ
        [InlineData(1)] // JNE
        [InlineData(2)] // JC
        [InlineData(3)] // JNC
        [InlineData(4)] // JN
        [InlineData(5)] // JGE
        [InlineData(6)] // JL
        [InlineData(7)] // JMP
        public void Execute_AllJumpConditions_Takes2Cycles(ushort conditionCode)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(conditionCode, 0x2000, 10);

            registerFile.SetProgramCounter(0x1000);
            // Set all flags to ensure jump is taken for all conditions except JMP
            registerFile.StatusRegister.Zero = true;
            registerFile.StatusRegister.Carry = true;
            registerFile.StatusRegister.Negative = true;
            registerFile.StatusRegister.Overflow = true;

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(2u, cycles);
        }

        [Fact]
        public void Execute_JumpTaken_Takes2Cycles()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(7, 0x2000, 10); // JMP (always taken)

            registerFile.SetProgramCounter(0x1000);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(2u, cycles);
        }

        [Fact]
        public void Execute_JumpNotTaken_Takes2Cycles()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new FormatIIIInstruction(0, 0x2000, 10); // JEQ

            registerFile.SetProgramCounter(0x1000);
            registerFile.StatusRegister.Zero = false; // Condition not met

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(2u, cycles);
        }
    }
}
