using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using MSP430.Emulator.Tests.TestUtilities;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for the CallInstruction class.
/// Tests all addressing modes, subroutine call behavior, stack management, and nested calls.
/// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.9: CALL
/// </summary>
public class CallInstructionTests
{
    /// <summary>
    /// Creates a standard test environment with register file and memory.
    /// </summary>
    /// <param name="stackPointer">Initial stack pointer value (default: 0x1000)</param>
    /// <param name="programCounter">Initial program counter value (default: 0x8000)</param>
    /// <param name="r4Value">Initial value for R4 register (default: 0x9000)</param>
    /// <returns>Tuple containing register file and memory array</returns>
    private static (RegisterFile RegisterFile, byte[] Memory) CreateTestEnvironment(
        ushort stackPointer = 0x1000,
        ushort programCounter = 0x8000,
        ushort r4Value = 0x9000)
    {
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();

        registerFile.SetStackPointer(stackPointer);
        registerFile.SetProgramCounter(programCounter);
        registerFile.WriteRegister(RegisterName.R4, r4Value);

        return (registerFile, memory);
    }

    /// <summary>
    /// Creates a standard CallInstruction for testing.
    /// </summary>
    /// <param name="sourceRegister">Source register (default: R4)</param>
    /// <param name="addressingMode">Addressing mode (default: Register)</param>
    /// <returns>Configured CallInstruction</returns>
    private static CallInstruction CreateTestInstruction(
        RegisterName sourceRegister = RegisterName.R4,
        AddressingMode addressingMode = AddressingMode.Register)
    {
        ushort instructionWord = 0x1284; // CALL R4 example instruction word
        return new CallInstruction(instructionWord, sourceRegister, addressingMode);
    }

    /// <summary>
    /// Tests for basic instruction properties and construction.
    /// </summary>
    public class BasicPropertiesTests
    {
        [Fact]
        public void Constructor_ValidParameters_SetsFormat()
        {
            // Arrange & Act
            CallInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal(InstructionFormat.FormatII, instruction.Format);
        }

        [Fact]
        public void Constructor_ValidParameters_SetsOpcode()
        {
            // Arrange & Act
            CallInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal(0x12, instruction.Opcode);
        }

        [Fact]
        public void Constructor_ValidParameters_SetsSourceRegister()
        {
            // Arrange & Act
            CallInstruction instruction = CreateTestInstruction(RegisterName.R5);

            // Assert
            Assert.Equal(RegisterName.R5, instruction.SourceRegister);
        }

        [Fact]
        public void Constructor_ValidParameters_SetsSourceAddressingMode()
        {
            // Arrange & Act
            CallInstruction instruction = CreateTestInstruction(addressingMode: AddressingMode.Indirect);

            // Assert
            Assert.Equal(AddressingMode.Indirect, instruction.SourceAddressingMode);
        }

        [Fact]
        public void Constructor_ValidParameters_SetsIsByteOperation()
        {
            // Arrange & Act
            CallInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.False(instruction.IsByteOperation); // CALL is always word operation
        }

        [Fact]
        public void Mnemonic_ReturnsCorrectValue()
        {
            // Arrange & Act
            CallInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal("CALL", instruction.Mnemonic);
        }

        [Theory]
        [InlineData(AddressingMode.Register, 0)]
        [InlineData(AddressingMode.Indexed, 1)]
        [InlineData(AddressingMode.Indirect, 0)]
        [InlineData(AddressingMode.IndirectAutoIncrement, 0)]
        [InlineData(AddressingMode.Immediate, 1)]
        [InlineData(AddressingMode.Absolute, 1)]
        [InlineData(AddressingMode.Symbolic, 1)]
        public void ExtensionWordCount_ReturnsCorrectValue(AddressingMode mode, int expectedWords)
        {
            // Arrange & Act
            CallInstruction instruction = CreateTestInstruction(addressingMode: mode);

            // Assert
            Assert.Equal(expectedWords, instruction.ExtensionWordCount);
        }
    }

    /// <summary>
    /// Tests for basic subroutine call execution behavior.
    /// </summary>
    public class BasicExecutionTests
    {
        [Fact]
        public void Execute_RegisterMode_CallsSubroutineCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x1000,
                programCounter: 0x8000,
                r4Value: 0x9000);
            CallInstruction instruction = CreateTestInstruction(RegisterName.R4, AddressingMode.Register);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Check that return address was pushed to stack
            ushort returnAddress = (ushort)(memory[0x0FFE] | (memory[0x0FFF] << 8));
            Assert.Equal(0x8000, returnAddress);

            // Assert - Check that stack pointer was decremented
            Assert.Equal(0x0FFE, registerFile.GetStackPointer());

            // Assert - Check that PC was set to destination
            Assert.Equal(0x9000, registerFile.GetProgramCounter());

            // Assert - Check cycle count for register mode per SLAU445I Table 4-9
            Assert.Equal(4u, cycles);
        }

        [Fact]
        public void Execute_IndirectMode_CallsSubroutineCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x1000,
                programCounter: 0x8000);

            // Setup memory for indirect addressing: R4 contains address 0x2000, which contains destination 0x9500
            registerFile.WriteRegister(RegisterName.R4, 0x2000);
            memory[0x2000] = 0x00; // Low byte of 0x9500
            memory[0x2001] = 0x95; // High byte of 0x9500

            CallInstruction instruction = CreateTestInstruction(RegisterName.R4, AddressingMode.Indirect);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Check that return address was pushed to stack
            ushort returnAddress = (ushort)(memory[0x0FFE] | (memory[0x0FFF] << 8));
            Assert.Equal(0x8000, returnAddress);

            // Assert - Check that stack pointer was decremented
            Assert.Equal(0x0FFE, registerFile.GetStackPointer());

            // Assert - Check that PC was set to destination from memory
            Assert.Equal(0x9500, registerFile.GetProgramCounter());

            // Assert - Check cycle count for indirect mode per SLAU445I Table 4-9
            Assert.Equal(4u, cycles);
        }

        [Fact]
        public void Execute_ImmediateMode_CallsSubroutineCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x1000,
                programCounter: 0x8000);

            CallInstruction instruction = CreateTestInstruction(RegisterName.R0, AddressingMode.Immediate);
            ushort[] extensionWords = { 0xA000 }; // Immediate destination address

            // Act
            uint cycles = instruction.Execute(registerFile, memory, extensionWords);

            // Assert - Check that return address was pushed to stack
            ushort returnAddress = (ushort)(memory[0x0FFE] | (memory[0x0FFF] << 8));
            Assert.Equal(0x8000, returnAddress);

            // Assert - Check that stack pointer was decremented
            Assert.Equal(0x0FFE, registerFile.GetStackPointer());

            // Assert - Check that PC was set to immediate value
            Assert.Equal(0xA000, registerFile.GetProgramCounter());

            // Assert - Check cycle count for immediate mode per SLAU445I Table 4-9
            Assert.Equal(4u, cycles);
        }
    }

    /// <summary>
    /// Tests for stack management behavior.
    /// </summary>
    public class StackManagementTests
    {
        [Fact]
        public void Execute_StackOverflow_ThrowsException()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(stackPointer: 1); // Stack pointer too low
            CallInstruction instruction = CreateTestInstruction();

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
            Assert.Contains("Stack overflow detected", exception.Message);
        }

        [Fact]
        public void Execute_StackMemoryBeyondBounds_ThrowsException()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
            registerFile.SetStackPointer(0x0006); // SP that would decrement to 0x0004, accessing 0x0004 and 0x0005 (beyond bounds)
            registerFile.WriteRegister(RegisterName.R4, 0x9000);
            CallInstruction instruction = CreateTestInstruction();

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
            Assert.Contains("Stack overflow detected", exception.Message);
        }

        [Fact]
        public void Execute_NestedCalls_MaintainsStackCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x1000,
                programCounter: 0x8000);

            CallInstruction instruction1 = CreateTestInstruction(RegisterName.R4, AddressingMode.Register);
            registerFile.WriteRegister(RegisterName.R4, 0x9000);

            // First call
            instruction1.Execute(registerFile, memory, Array.Empty<ushort>());

            // Setup for second call from within first subroutine
            registerFile.SetProgramCounter(0x9010); // Simulate execution in first subroutine
            registerFile.WriteRegister(RegisterName.R5, 0xA000);
            CallInstruction instruction2 = CreateTestInstruction(RegisterName.R5, AddressingMode.Register);

            // Act - Second call (nested)
            instruction2.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Check first return address (0x8000) at stack bottom
            ushort firstReturn = (ushort)(memory[0x0FFE] | (memory[0x0FFF] << 8));
            Assert.Equal(0x8000, firstReturn);

            // Assert - Check second return address (0x9010) at stack top
            ushort secondReturn = (ushort)(memory[0x0FFC] | (memory[0x0FFD] << 8));
            Assert.Equal(0x9010, secondReturn);

            // Assert - Check stack pointer points to top of stack
            Assert.Equal(0x0FFC, registerFile.GetStackPointer());

            // Assert - Check PC is at second subroutine
            Assert.Equal(0xA000, registerFile.GetProgramCounter());
        }
    }

    /// <summary>
    /// Tests for string representation and formatting.
    /// </summary>
    public class StringRepresentationTests
    {
        [Theory]
        [InlineData(RegisterName.R4, AddressingMode.Register, "CALL R4")]
        [InlineData(RegisterName.R5, AddressingMode.Indirect, "CALL @R5")]
        [InlineData(RegisterName.R6, AddressingMode.IndirectAutoIncrement, "CALL @R6+")]
        [InlineData(RegisterName.R0, AddressingMode.Immediate, "CALL #N")]
        public void ToString_VariousAddressingModes_ReturnsCorrectFormat(
            RegisterName register, AddressingMode mode, string expected)
        {
            // Arrange
            CallInstruction instruction = CreateTestInstruction(register, mode);

            // Act
            string result = instruction.ToString();

            // Assert
            Assert.Equal(expected, result);
        }
    }

    /// <summary>
    /// Tests for cycle count calculations.
    /// </summary>
    public class CycleCountTests
    {
        [Theory]
        [InlineData(AddressingMode.Register, 4u)]
        [InlineData(AddressingMode.Indexed, 5u)]
        [InlineData(AddressingMode.Indirect, 4u)]
        [InlineData(AddressingMode.IndirectAutoIncrement, 4u)]
        [InlineData(AddressingMode.Immediate, 4u)]
        [InlineData(AddressingMode.Absolute, 6u)]
        [InlineData(AddressingMode.Symbolic, 5u)]
        public void Execute_VariousAddressingModes_ReturnsCorrectCycleCount(
            AddressingMode mode, uint expectedCycles)
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            CallInstruction instruction = CreateTestInstruction(RegisterName.R4, mode);
            ushort[] extensionWords;
            if (mode == AddressingMode.Immediate)
            {
                extensionWords = new ushort[] { 0x9000 };
            }
            else if (mode == AddressingMode.Indexed || mode == AddressingMode.Absolute || mode == AddressingMode.Symbolic)
            {
                extensionWords = new ushort[] { 0x0010 };
            }
            else
            {
                extensionWords = Array.Empty<ushort>();
            }

            // Setup memory for indirect modes if needed
            if (mode == AddressingMode.Indirect || mode == AddressingMode.IndirectAutoIncrement)
            {
                memory[0x9000] = 0x00; // Low byte
                memory[0x9001] = 0xA0; // High byte - destination 0xA000
            }

            // Act
            uint cycles = instruction.Execute(registerFile, memory, extensionWords);

            // Assert
            Assert.Equal(expectedCycles, cycles);
        }
    }
}
