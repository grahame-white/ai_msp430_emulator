using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for JMP (Jump unconditional) instruction execution.
/// 
/// Tests instruction properties, program counter calculation, offset range validation,
/// and cycle count according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class JmpInstructionTests
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
            var instruction = new JmpInstruction(0x3C00, 10);

            // Assert
            Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
        }

        [Fact]
        public void Constructor_ValidParameters_SetsOpcode()
        {
            // Arrange & Act
            var instruction = new JmpInstruction(0x3C00, 10);

            // Assert
            Assert.Equal(7, instruction.Opcode); // JMP is always opcode 7
        }

        [Fact]
        public void Constructor_ValidParameters_SetsOffset()
        {
            // Arrange & Act
            var instruction = new JmpInstruction(0x3C00, -25);

            // Assert
            Assert.Equal(-25, instruction.Offset);
        }

        [Fact]
        public void Mnemonic_AlwaysReturnsJMP()
        {
            // Arrange & Act
            var instruction = new JmpInstruction(0x3C00, 0);

            // Assert
            Assert.Equal("JMP", instruction.Mnemonic);
        }

        [Fact]
        public void ExtensionWordCount_AlwaysReturnsZero()
        {
            // Arrange & Act
            var instruction = new JmpInstruction(0x3C00, 0);

            // Assert
            Assert.Equal(0, instruction.ExtensionWordCount);
        }

        [Fact]
        public void ToString_PositiveOffset_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new JmpInstruction(0x3C00, 5);

            // Assert
            Assert.Equal("JMP +5", instruction.ToString());
        }

        [Fact]
        public void ToString_NegativeOffset_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new JmpInstruction(0x3C00, -10);

            // Assert
            Assert.Equal("JMP -10", instruction.ToString());
        }

        [Fact]
        public void ToString_ZeroOffset_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new JmpInstruction(0x3C00, 0);

            // Assert
            Assert.Equal("JMP 0", instruction.ToString());
        }
    }

    /// <summary>
    /// Tests for offset range validation.
    /// </summary>
    public class OffsetValidationTests
    {
        [Theory]
        [InlineData(-511)]
        [InlineData(512)]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(-100)]
        public void Constructor_ValidOffsetRange_DoesNotThrow(short offset)
        {
            // Arrange & Act & Assert
            Exception exception = Record.Exception(() => new JmpInstruction(0x3C00, offset));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(-512)]
        [InlineData(513)]
        [InlineData(-1000)]
        [InlineData(1000)]
        public void Constructor_InvalidOffsetRange_ThrowsArgumentOutOfRangeException(short offset)
        {
            // Arrange & Act & Assert
            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => new JmpInstruction(0x3C00, offset));
            Assert.Contains("outside valid range (-511 to +512 words)", exception.Message);
        }
    }

    /// <summary>
    /// Tests for instruction execution and program counter manipulation.
    /// </summary>
    public class ExecutionTests
    {
        [Fact]
        public void Execute_PositiveOffset_UpdatesProgramCounterCorrectly()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JmpInstruction(0x3C00, 10);

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            ushort expectedPC = (ushort)(originalPC + (10 * 2)); // 10 words = 20 bytes
            Assert.Equal(expectedPC, registerFile.GetProgramCounter());
            Assert.Equal(2u, cycles); // JMP always takes 2 cycles
        }

        [Fact]
        public void Execute_NegativeOffset_UpdatesProgramCounterCorrectly()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JmpInstruction(0x3C00, -8);

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            ushort expectedPC = (ushort)(originalPC + (-8 * 2)); // -8 words = -16 bytes
            Assert.Equal(expectedPC, registerFile.GetProgramCounter());
            Assert.Equal(2u, cycles); // JMP always takes 2 cycles
        }

        [Fact]
        public void Execute_ZeroOffset_KeepsProgramCounterUnchanged()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JmpInstruction(0x3C00, 0);

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(originalPC, registerFile.GetProgramCounter());
            Assert.Equal(2u, cycles); // JMP always takes 2 cycles
        }

        [Fact]
        public void Execute_MaximumPositiveOffset_UpdatesProgramCounterCorrectly()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JmpInstruction(0x3C00, 512);

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            ushort expectedPC = (ushort)(originalPC + (512 * 2)); // 512 words = 1024 bytes
            Assert.Equal(expectedPC, registerFile.GetProgramCounter());
            Assert.Equal(2u, cycles);
        }

        [Fact]
        public void Execute_MaximumNegativeOffset_UpdatesProgramCounterCorrectly()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JmpInstruction(0x3C00, -511);

            ushort originalPC = 0x1000;
            registerFile.SetProgramCounter(originalPC);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            ushort expectedPC = (ushort)(originalPC + (-511 * 2)); // -511 words = -1022 bytes
            Assert.Equal(expectedPC, registerFile.GetProgramCounter());
            Assert.Equal(2u, cycles);
        }

        [Fact]
        public void Execute_DoesNotAffectOtherRegisters()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JmpInstruction(0x3C00, 5);

            // Set some register values
            registerFile.WriteRegister(RegisterName.R4, 0x1234);
            registerFile.WriteRegister(RegisterName.R5, 0x5678);
            registerFile.WriteRegister(RegisterName.R6, 0x9ABC);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - other registers should be unchanged
            Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R4));
            Assert.Equal(0x5678, registerFile.ReadRegister(RegisterName.R5));
            Assert.Equal(0x9ABC, registerFile.ReadRegister(RegisterName.R6));
        }

        [Fact]
        public void Execute_DoesNotAffectStatusFlags()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new JmpInstruction(0x3C00, 5);

            // Set some status flags
            registerFile.StatusRegister.Zero = true;
            registerFile.StatusRegister.Negative = true;
            registerFile.StatusRegister.Carry = true;
            registerFile.StatusRegister.Overflow = true;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - status flags should be unchanged
            Assert.True(registerFile.StatusRegister.Zero);
            Assert.True(registerFile.StatusRegister.Negative);
            Assert.True(registerFile.StatusRegister.Carry);
            Assert.True(registerFile.StatusRegister.Overflow);
        }
    }
}
