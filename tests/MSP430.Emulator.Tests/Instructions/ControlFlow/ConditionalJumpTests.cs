using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for conditional jump instructions (JZ, JNZ, JC, JNC, JN, JGE, JL).
/// 
/// Tests instruction properties, condition evaluation, program counter calculation,
/// offset range validation, and cycle count according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class ConditionalJumpTests
{
    /// <summary>
    /// Tests for JZ (Jump if Zero) instruction.
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

    /// <summary>
    /// Tests for JNZ (Jump if Not Zero) instruction.
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
        public void Execute_ZeroFlag_BehavesCorrectly(bool zeroFlag, bool shouldJump)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JnzInstruction(0x2000, -5);

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);
            registerFile.StatusRegister.Zero = zeroFlag;

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            if (shouldJump)
            {
                Assert.Equal(originalPC + (-5 * 2), registerFile.GetProgramCounter());
            }
            else
            {
                Assert.Equal(originalPC, registerFile.GetProgramCounter());
            }
            Assert.Equal(2u, cycles);
        }
    }

    /// <summary>
    /// Tests for JC (Jump if Carry) instruction.
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
        public void Execute_CarryFlag_BehavesCorrectly(bool carryFlag, bool shouldJump)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JcInstruction(0x2000, 20);

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);
            registerFile.StatusRegister.Carry = carryFlag;

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            if (shouldJump)
            {
                Assert.Equal(originalPC + (20 * 2), registerFile.GetProgramCounter());
            }
            else
            {
                Assert.Equal(originalPC, registerFile.GetProgramCounter());
            }
            Assert.Equal(2u, cycles);
        }
    }

    /// <summary>
    /// Tests for JNC (Jump if No Carry) instruction.
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
        public void Execute_CarryFlag_BehavesCorrectly(bool carryFlag, bool shouldJump)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JncInstruction(0x2000, -15);

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);
            registerFile.StatusRegister.Carry = carryFlag;

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            if (shouldJump)
            {
                Assert.Equal(originalPC + (-15 * 2), registerFile.GetProgramCounter());
            }
            else
            {
                Assert.Equal(originalPC, registerFile.GetProgramCounter());
            }
            Assert.Equal(2u, cycles);
        }
    }

    /// <summary>
    /// Tests for JN (Jump if Negative) instruction.
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
        public void Execute_NegativeFlag_BehavesCorrectly(bool negativeFlag, bool shouldJump)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JnInstruction(0x2000, 8);

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);
            registerFile.StatusRegister.Negative = negativeFlag;

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            if (shouldJump)
            {
                Assert.Equal(originalPC + (8 * 2), registerFile.GetProgramCounter());
            }
            else
            {
                Assert.Equal(originalPC, registerFile.GetProgramCounter());
            }
            Assert.Equal(2u, cycles);
        }
    }

    /// <summary>
    /// Tests for JGE (Jump if Greater or Equal) instruction.
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

    /// <summary>
    /// Tests for JL (Jump if Less) instruction.
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
        public void Execute_NegativeXorOverflow_BehavesCorrectly(bool negative, bool overflow, bool shouldJump)
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
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            if (shouldJump)
            {
                Assert.Equal(originalPC + (-8 * 2), registerFile.GetProgramCounter());
            }
            else
            {
                Assert.Equal(originalPC, registerFile.GetProgramCounter());
            }
            Assert.Equal(2u, cycles);
        }
    }

    /// <summary>
    /// Tests for offset range validation across all conditional jump instructions.
    /// </summary>
    public class OffsetValidationTests
    {
        [Theory]
        [InlineData(-511)]
        [InlineData(512)]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(-100)]
        public void ConditionalJumpInstructions_ValidOffsetRange_DoesNotThrow(short offset)
        {
            // Arrange & Act & Assert
            Assert.Null(Record.Exception(() => new JzInstruction(0x2000, offset)));
            Assert.Null(Record.Exception(() => new JnzInstruction(0x2000, offset)));
            Assert.Null(Record.Exception(() => new JcInstruction(0x2000, offset)));
            Assert.Null(Record.Exception(() => new JncInstruction(0x2000, offset)));
            Assert.Null(Record.Exception(() => new JnInstruction(0x2000, offset)));
            Assert.Null(Record.Exception(() => new JgeInstruction(0x2000, offset)));
            Assert.Null(Record.Exception(() => new JlInstruction(0x2000, offset)));
        }

        [Theory]
        [InlineData(-512)]
        [InlineData(513)]
        [InlineData(-1000)]
        [InlineData(1000)]
        public void ConditionalJumpInstructions_InvalidOffsetRange_ThrowsArgumentOutOfRangeException(short offset)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new JzInstruction(0x2000, offset));
            Assert.Throws<ArgumentOutOfRangeException>(() => new JnzInstruction(0x2000, offset));
            Assert.Throws<ArgumentOutOfRangeException>(() => new JcInstruction(0x2000, offset));
            Assert.Throws<ArgumentOutOfRangeException>(() => new JncInstruction(0x2000, offset));
            Assert.Throws<ArgumentOutOfRangeException>(() => new JnInstruction(0x2000, offset));
            Assert.Throws<ArgumentOutOfRangeException>(() => new JgeInstruction(0x2000, offset));
            Assert.Throws<ArgumentOutOfRangeException>(() => new JlInstruction(0x2000, offset));
        }
    }

    /// <summary>
    /// Tests for string formatting across all conditional jump instructions.
    /// </summary>
    public class StringFormatTests
    {
        [Theory]
        [InlineData(5, "JZ +5")]
        [InlineData(-10, "JZ -10")]
        [InlineData(0, "JZ 0")]
        public void JzInstruction_ToString_FormatsCorrectly(short offset, string expected)
        {
            // Arrange & Act
            var instruction = new JzInstruction(0x2000, offset);

            // Assert
            Assert.Equal(expected, instruction.ToString());
        }

        [Theory]
        [InlineData(5, "JNZ +5")]
        [InlineData(-10, "JNZ -10")]
        [InlineData(0, "JNZ 0")]
        public void JnzInstruction_ToString_FormatsCorrectly(short offset, string expected)
        {
            // Arrange & Act
            var instruction = new JnzInstruction(0x2000, offset);

            // Assert
            Assert.Equal(expected, instruction.ToString());
        }

        [Theory]
        [InlineData(5, "JC +5")]
        [InlineData(-10, "JC -10")]
        [InlineData(0, "JC 0")]
        public void JcInstruction_ToString_FormatsCorrectly(short offset, string expected)
        {
            // Arrange & Act
            var instruction = new JcInstruction(0x2000, offset);

            // Assert
            Assert.Equal(expected, instruction.ToString());
        }

        [Theory]
        [InlineData(5, "JNC +5")]
        [InlineData(-10, "JNC -10")]
        [InlineData(0, "JNC 0")]
        public void JncInstruction_ToString_FormatsCorrectly(short offset, string expected)
        {
            // Arrange & Act
            var instruction = new JncInstruction(0x2000, offset);

            // Assert
            Assert.Equal(expected, instruction.ToString());
        }

        [Theory]
        [InlineData(5, "JN +5")]
        [InlineData(-10, "JN -10")]
        [InlineData(0, "JN 0")]
        public void JnInstruction_ToString_FormatsCorrectly(short offset, string expected)
        {
            // Arrange & Act
            var instruction = new JnInstruction(0x2000, offset);

            // Assert
            Assert.Equal(expected, instruction.ToString());
        }

        [Theory]
        [InlineData(5, "JGE +5")]
        [InlineData(-10, "JGE -10")]
        [InlineData(0, "JGE 0")]
        public void JgeInstruction_ToString_FormatsCorrectly(short offset, string expected)
        {
            // Arrange & Act
            var instruction = new JgeInstruction(0x2000, offset);

            // Assert
            Assert.Equal(expected, instruction.ToString());
        }

        [Theory]
        [InlineData(5, "JL +5")]
        [InlineData(-10, "JL -10")]
        [InlineData(0, "JL 0")]
        public void JlInstruction_ToString_FormatsCorrectly(short offset, string expected)
        {
            // Arrange & Act
            var instruction = new JlInstruction(0x2000, offset);

            // Assert
            Assert.Equal(expected, instruction.ToString());
        }
    }

    /// <summary>
    /// Tests for instruction properties across all conditional jump instructions.
    /// </summary>
    public class InstructionPropertiesTests
    {
        [Fact]
        public void AllConditionalJumpInstructions_HaveCorrectFormat()
        {
            // Arrange & Act
            var jz = new JzInstruction(0x2000, 0);
            var jnz = new JnzInstruction(0x2000, 0);
            var jc = new JcInstruction(0x2000, 0);
            var jnc = new JncInstruction(0x2000, 0);
            var jn = new JnInstruction(0x2000, 0);
            var jge = new JgeInstruction(0x2000, 0);
            var jl = new JlInstruction(0x2000, 0);

            // Assert
            Assert.Equal(InstructionFormat.FormatIII, jz.Format);
            Assert.Equal(InstructionFormat.FormatIII, jnz.Format);
            Assert.Equal(InstructionFormat.FormatIII, jc.Format);
            Assert.Equal(InstructionFormat.FormatIII, jnc.Format);
            Assert.Equal(InstructionFormat.FormatIII, jn.Format);
            Assert.Equal(InstructionFormat.FormatIII, jge.Format);
            Assert.Equal(InstructionFormat.FormatIII, jl.Format);
        }

        [Fact]
        public void AllConditionalJumpInstructions_HaveZeroExtensionWords()
        {
            // Arrange & Act
            var jz = new JzInstruction(0x2000, 0);
            var jnz = new JnzInstruction(0x2000, 0);
            var jc = new JcInstruction(0x2000, 0);
            var jnc = new JncInstruction(0x2000, 0);
            var jn = new JnInstruction(0x2000, 0);
            var jge = new JgeInstruction(0x2000, 0);
            var jl = new JlInstruction(0x2000, 0);

            // Assert
            Assert.Equal(0, jz.ExtensionWordCount);
            Assert.Equal(0, jnz.ExtensionWordCount);
            Assert.Equal(0, jc.ExtensionWordCount);
            Assert.Equal(0, jnc.ExtensionWordCount);
            Assert.Equal(0, jn.ExtensionWordCount);
            Assert.Equal(0, jge.ExtensionWordCount);
            Assert.Equal(0, jl.ExtensionWordCount);
        }
    }
}
