using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using MSP430.Emulator.Tests.TestUtilities;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for BR (Branch) instruction execution.
/// 
/// Tests instruction properties, program counter manipulation, addressing mode support,
/// and cycle count according to SLAU445I specifications.
/// 
/// BR instruction performs unconditional branch to any address in lower 64K space:
/// - Implemented as MOV src,PC
/// - Supports all source addressing modes
/// - Does not affect status flags
/// - Always word operation
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.8: BR, BRANCH
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Instruction Set
/// </summary>
public class BranchInstructionTests
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
            var instruction = new BranchInstruction(0x4080, RegisterName.R8, AddressingMode.Register);

            // Assert
            Assert.Equal(InstructionFormat.FormatI, instruction.Format);
        }

        [Fact]
        public void Constructor_ValidParameters_SetsOpcode()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R8, AddressingMode.Register);

            // Assert
            Assert.Equal(0x4, instruction.Opcode); // Same as MOV instruction
        }

        [Fact]
        public void Constructor_ValidParameters_SetsSourceRegister()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R8, AddressingMode.Register);

            // Assert
            Assert.Equal(RegisterName.R8, instruction.SourceRegister);
        }

        [Fact]
        public void Constructor_ValidParameters_SetsDestinationRegister()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R8, AddressingMode.Register);

            // Assert
            Assert.Equal(RegisterName.R0, instruction.DestinationRegister); // Always PC
        }

        [Fact]
        public void Constructor_ValidParameters_SetsSourceAddressingMode()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R8, AddressingMode.Indirect);

            // Assert
            Assert.Equal(AddressingMode.Indirect, instruction.SourceAddressingMode);
        }

        [Fact]
        public void Constructor_ValidParameters_SetsDestinationAddressingMode()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R8, AddressingMode.Indirect);

            // Assert
            Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode); // Always register mode for PC
        }

        [Fact]
        public void IsByteOperation_AlwaysReturnsFalse()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R8, AddressingMode.Register);

            // Assert
            Assert.False(instruction.IsByteOperation); // Branch is always word operation
        }

        [Fact]
        public void Mnemonic_AlwaysReturnsBR()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R8, AddressingMode.Register);

            // Assert
            Assert.Equal("BR", instruction.Mnemonic);
        }

        [Theory]
        [InlineData(AddressingMode.Register, 0)]
        [InlineData(AddressingMode.Indirect, 0)]
        [InlineData(AddressingMode.IndirectAutoIncrement, 0)]
        [InlineData(AddressingMode.Indexed, 1)]
        [InlineData(AddressingMode.Immediate, 1)]
        [InlineData(AddressingMode.Absolute, 1)]
        [InlineData(AddressingMode.Symbolic, 1)]
        public void ExtensionWordCount_VariousAddressingModes_ReturnsCorrectCount(AddressingMode mode, int expectedCount)
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R8, mode);

            // Assert
            Assert.Equal(expectedCount, instruction.ExtensionWordCount);
        }
    }

    /// <summary>
    /// Tests for string representation methods.
    /// </summary>
    public class StringRepresentationTests
    {
        [Fact]
        public void ToString_RegisterMode_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R5, AddressingMode.Register);

            // Assert
            Assert.Equal("BR R5", instruction.ToString());
        }

        [Fact]
        public void ToString_IndirectMode_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R5, AddressingMode.Indirect);

            // Assert
            Assert.Equal("BR @R5", instruction.ToString());
        }

        [Fact]
        public void ToString_IndirectAutoIncrementMode_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R5, AddressingMode.IndirectAutoIncrement);

            // Assert
            Assert.Equal("BR @R5+", instruction.ToString());
        }

        [Fact]
        public void ToString_IndexedMode_WithExtensionWord_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R5, AddressingMode.Indexed);
            ushort[] extensionWords = { 0x1234 };

            // Assert
            Assert.Equal("BR 0x1234(R5)", instruction.ToString(extensionWords));
        }

        [Fact]
        public void ToString_ImmediateMode_WithExtensionWord_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R0, AddressingMode.Immediate);
            ushort[] extensionWords = { 0x8000 };

            // Assert
            Assert.Equal("BR #0x8000", instruction.ToString(extensionWords));
        }

        [Fact]
        public void ToString_AbsoluteMode_WithExtensionWord_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R2, AddressingMode.Absolute);
            ushort[] extensionWords = { 0x0200 };

            // Assert
            Assert.Equal("BR &0x200", instruction.ToString(extensionWords));
        }

        [Fact]
        public void ToString_SymbolicMode_WithExtensionWord_FormatsCorrectly()
        {
            // Arrange & Act
            var instruction = new BranchInstruction(0x4080, RegisterName.R0, AddressingMode.Symbolic);
            ushort[] extensionWords = { 0x0100 };

            // Assert
            Assert.Equal("BR 0x100", instruction.ToString(extensionWords));
        }
    }

    /// <summary>
    /// Tests for instruction execution and program counter manipulation.
    /// </summary>
    public class ExecutionTests
    {
        [Fact]
        public void Execute_RegisterMode_UpdatesProgramCounterCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R5, AddressingMode.Register);

            ushort targetAddress = 0x8000;
            registerFile.WriteRegister(RegisterName.R5, targetAddress);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(targetAddress, registerFile.GetProgramCounter());
            Assert.True(cycles > 0); // Should consume some cycles
        }

        [Fact]
        public void Execute_ImmediateMode_UpdatesProgramCounterCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R0, AddressingMode.Immediate);

            ushort targetAddress = 0x8000;
            ushort[] extensionWords = { targetAddress };

            // Act
            uint cycles = instruction.Execute(registerFile, memory, extensionWords);

            // Assert
            Assert.Equal(targetAddress, registerFile.GetProgramCounter());
            Assert.True(cycles > 0);
        }

        [Fact]
        public void Execute_IndirectMode_UpdatesProgramCounterCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R5, AddressingMode.Indirect);

            ushort pointerAddress = 0x0200;
            ushort targetAddress = 0x8000;

            registerFile.WriteRegister(RegisterName.R5, pointerAddress);
            // Write target address to memory location pointed to by R5
            memory[pointerAddress] = (byte)(targetAddress & 0xFF);
            memory[pointerAddress + 1] = (byte)((targetAddress >> 8) & 0xFF);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(targetAddress, registerFile.GetProgramCounter());
            Assert.True(cycles > 0);
        }

        [Fact]
        public void Execute_IndirectAutoIncrementMode_UpdatesProgramCounterAndIncrementsRegister()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R5, AddressingMode.IndirectAutoIncrement);

            ushort pointerAddress = 0x0200;
            ushort targetAddress = 0x8000;

            registerFile.WriteRegister(RegisterName.R5, pointerAddress);
            // Write target address to memory location pointed to by R5
            memory[pointerAddress] = (byte)(targetAddress & 0xFF);
            memory[pointerAddress + 1] = (byte)((targetAddress >> 8) & 0xFF);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(targetAddress, registerFile.GetProgramCounter());
            Assert.Equal((ushort)(pointerAddress + 2), registerFile.ReadRegister(RegisterName.R5)); // Should increment by 2 for word operation
            Assert.True(cycles > 0);
        }

        [Fact]
        public void Execute_IndexedMode_UpdatesProgramCounterCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R5, AddressingMode.Indexed);

            ushort baseAddress = 0x0200;
            ushort offset = 0x0010;
            ushort targetAddress = 0x8000;
            ushort[] extensionWords = { offset };

            registerFile.WriteRegister(RegisterName.R5, baseAddress);
            // Write target address to memory location [R5 + offset]
            ushort effectiveAddress = (ushort)(baseAddress + offset);
            memory[effectiveAddress] = (byte)(targetAddress & 0xFF);
            memory[effectiveAddress + 1] = (byte)((targetAddress >> 8) & 0xFF);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, extensionWords);

            // Assert
            Assert.Equal(targetAddress, registerFile.GetProgramCounter());
            Assert.True(cycles > 0);
        }

        [Fact]
        public void Execute_AbsoluteMode_UpdatesProgramCounterCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R2, AddressingMode.Absolute);

            ushort absoluteAddress = 0x0200;
            ushort targetAddress = 0x8000;
            ushort[] extensionWords = { absoluteAddress };

            // Write target address to absolute memory location
            memory[absoluteAddress] = (byte)(targetAddress & 0xFF);
            memory[absoluteAddress + 1] = (byte)((targetAddress >> 8) & 0xFF);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, extensionWords);

            // Assert
            Assert.Equal(targetAddress, registerFile.GetProgramCounter());
            Assert.True(cycles > 0);
        }

        [Fact]
        public void Execute_SymbolicMode_UpdatesProgramCounterCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R0, AddressingMode.Symbolic);

            ushort initialPC = 0x1000;
            ushort offset = 0x0100;
            ushort targetAddress = 0x8000;
            ushort[] extensionWords = { offset };

            registerFile.SetProgramCounter(initialPC);
            // Write target address to memory location [PC + offset]
            ushort effectiveAddress = (ushort)(initialPC + offset);
            memory[effectiveAddress] = (byte)(targetAddress & 0xFF);
            memory[effectiveAddress + 1] = (byte)((targetAddress >> 8) & 0xFF);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, extensionWords);

            // Assert
            Assert.Equal(targetAddress, registerFile.GetProgramCounter());
            Assert.True(cycles > 0);
        }

        [Fact]
        public void Execute_DoesNotAffectOtherRegisters()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R8, AddressingMode.Register);

            // Set some register values
            registerFile.WriteRegister(RegisterName.R4, 0x1234);
            registerFile.WriteRegister(RegisterName.R5, 0x5678);
            registerFile.WriteRegister(RegisterName.R6, 0x9ABC);
            registerFile.WriteRegister(RegisterName.R8, 0x8000); // Source register

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - other registers should be unchanged
            Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R4));
            Assert.Equal(0x5678, registerFile.ReadRegister(RegisterName.R5));
            Assert.Equal(0x9ABC, registerFile.ReadRegister(RegisterName.R6));
            Assert.Equal(0x8000, registerFile.ReadRegister(RegisterName.R8)); // Source register unchanged
        }

        [Fact]
        public void Execute_DoesNotAffectStatusFlags()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R5, AddressingMode.Register);

            // Set some status flags
            registerFile.StatusRegister.Zero = true;
            registerFile.StatusRegister.Negative = true;
            registerFile.StatusRegister.Carry = true;
            registerFile.StatusRegister.Overflow = true;

            registerFile.WriteRegister(RegisterName.R5, 0x8000);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - status flags should be unchanged (BR doesn't affect flags)
            Assert.True(registerFile.StatusRegister.Zero);
            Assert.True(registerFile.StatusRegister.Negative);
            Assert.True(registerFile.StatusRegister.Carry);
            Assert.True(registerFile.StatusRegister.Overflow);
        }

        [Fact]
        public void Execute_MissingExtensionWord_ThrowsArgumentException()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R0, AddressingMode.Immediate);

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
            Assert.Contains("requires 1 extension word but none provided", exception.Message);
        }

        [Fact]
        public void Execute_TooManyExtensionWords_ThrowsArgumentException()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R0, AddressingMode.Immediate);

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                instruction.Execute(registerFile, memory, new ushort[] { 0x8000, 0x9000 }));
            Assert.Contains("requires exactly 1 extension word but multiple provided", exception.Message);
        }

        [Fact]
        public void Execute_WordAlignment_EnsuresPCIsWordAligned()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            var instruction = new BranchInstruction(0x4080, RegisterName.R5, AddressingMode.Register);

            ushort oddAddress = 0x8001; // Odd address (not word-aligned)
            registerFile.WriteRegister(RegisterName.R5, oddAddress);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - PC should be word-aligned (RegisterFile should handle this)
            ushort pc = registerFile.GetProgramCounter();
            Assert.Equal(0, pc % 2); // Should be even (word-aligned)
        }
    }
}
