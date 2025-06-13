using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for the RetInstruction class.
/// Tests return from subroutine behavior, stack management, and integration with CALL instruction.
/// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class RetInstructionTests
{
    /// <summary>
    /// Creates a standard test environment with register file and memory.
    /// </summary>
    /// <param name="stackPointer">Initial stack pointer value (default: 0x0FFE - after a CALL)</param>
    /// <param name="programCounter">Initial program counter value (default: 0x9000 - in subroutine)</param>
    /// <returns>Tuple containing register file and memory array</returns>
    private static (RegisterFile RegisterFile, byte[] Memory) CreateTestEnvironment(
        ushort stackPointer = 0x0FFE,
        ushort programCounter = 0x9000)
    {
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        registerFile.SetStackPointer(stackPointer);
        registerFile.SetProgramCounter(programCounter);

        return (registerFile, memory);
    }

    /// <summary>
    /// Creates a standard RetInstruction for testing.
    /// </summary>
    /// <returns>Configured RetInstruction</returns>
    private static RetInstruction CreateTestInstruction()
    {
        ushort instructionWord = 0x4130; // RET emulated as MOV @SP+, PC
        return new RetInstruction(instructionWord);
    }

    /// <summary>
    /// Simulates a CALL instruction by setting up the stack with a return address.
    /// </summary>
    /// <param name="memory">Memory array</param>
    /// <param name="stackPointer">Stack pointer where return address should be placed</param>
    /// <param name="returnAddress">Return address to place on stack</param>
    private static void SimulateCall(byte[] memory, ushort stackPointer, ushort returnAddress)
    {
        // Place return address on stack (little-endian)
        memory[stackPointer] = (byte)(returnAddress & 0xFF);
        memory[stackPointer + 1] = (byte)((returnAddress >> 8) & 0xFF);
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
            RetInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal(InstructionFormat.FormatI, instruction.Format); // Emulated as MOV
        }

        [Fact]
        public void Constructor_ValidParameters_SetsOpcode()
        {
            // Arrange & Act
            RetInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal(0x4, instruction.Opcode); // MOV instruction opcode
        }

        [Fact]
        public void Constructor_ValidParameters_SetsSourceRegister()
        {
            // Arrange & Act
            RetInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal(RegisterName.R1, instruction.SourceRegister); // SP
        }

        [Fact]
        public void Constructor_ValidParameters_SetsDestinationRegister()
        {
            // Arrange & Act
            RetInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal(RegisterName.R0, instruction.DestinationRegister); // PC
        }

        [Fact]
        public void Constructor_ValidParameters_SetsSourceAddressingMode()
        {
            // Arrange & Act
            RetInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal(AddressingMode.IndirectAutoIncrement, instruction.SourceAddressingMode); // @SP+
        }

        [Fact]
        public void Constructor_ValidParameters_SetsDestinationAddressingMode()
        {
            // Arrange & Act
            RetInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode); // PC register
        }

        [Fact]
        public void Constructor_ValidParameters_SetsIsByteOperation()
        {
            // Arrange & Act
            RetInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.False(instruction.IsByteOperation); // RET is always word operation
        }

        [Fact]
        public void ExtensionWordCount_ReturnsZero()
        {
            // Arrange & Act
            RetInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal(0, instruction.ExtensionWordCount); // RET doesn't need extension words
        }

        [Fact]
        public void Mnemonic_ReturnsCorrectValue()
        {
            // Arrange & Act
            RetInstruction instruction = CreateTestInstruction();

            // Assert
            Assert.Equal("RET", instruction.Mnemonic);
        }
    }

    /// <summary>
    /// Tests for basic return execution behavior.
    /// </summary>
    public class BasicExecutionTests
    {
        [Fact]
        public void Execute_SimpleReturn_ReturnsToCorrectAddress()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x0FFE,
                programCounter: 0x9000);

            // Simulate a previous CALL that pushed return address 0x8000
            SimulateCall(memory, 0x0FFE, 0x8000);

            RetInstruction instruction = CreateTestInstruction();

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Check that PC was set to return address
            Assert.Equal(0x8000, registerFile.GetProgramCounter());

            // Assert - Check that stack pointer was incremented
            Assert.Equal(0x1000, registerFile.GetStackPointer());

            // Assert - Check cycle count (fixed 4 cycles per SLAU445I Table 4-8)
            Assert.Equal(4u, cycles);
        }

        [Fact]
        public void Execute_ReturnFromNestedCall_ReturnsToCorrectAddress()
        {
            // Arrange - Simulate nested calls
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x0FFC, // After two CALLs
                programCounter: 0xA000); // In second subroutine

            // First CALL pushed 0x8000 (main program)
            SimulateCall(memory, 0x0FFE, 0x8000);

            // Second CALL pushed 0x9010 (first subroutine)
            SimulateCall(memory, 0x0FFC, 0x9010);

            RetInstruction instruction = CreateTestInstruction();

            // Act - Return from second subroutine
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Check that PC was set to first subroutine address
            Assert.Equal(0x9010, registerFile.GetProgramCounter());

            // Assert - Check that stack pointer was incremented
            Assert.Equal(0x0FFE, registerFile.GetStackPointer());

            // Assert - Check cycle count
            Assert.Equal(4u, cycles);

            // Now simulate second RET to return to main program
            RetInstruction instruction2 = CreateTestInstruction();
            uint cycles2 = instruction2.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Check that PC was set to main program address
            Assert.Equal(0x8000, registerFile.GetProgramCounter());

            // Assert - Check that stack pointer was incremented again
            Assert.Equal(0x1000, registerFile.GetStackPointer());

            // Assert - Check cycle count
            Assert.Equal(4u, cycles2);
        }

        [Fact]
        public void Execute_LittleEndianReturnAddress_HandlesCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x0FFE,
                programCounter: 0x9000);

            // Manually set return address 0x1234 in little-endian format
            memory[0x0FFE] = 0x34; // Low byte
            memory[0x0FFF] = 0x12; // High byte

            RetInstruction instruction = CreateTestInstruction();

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Check that PC was set to correct address
            Assert.Equal(0x1234, registerFile.GetProgramCounter());
        }
    }

    /// <summary>
    /// Tests for stack management and error handling.
    /// </summary>
    public class StackManagementTests
    {
        [Fact]
        public void Execute_StackUnderflow_ThrowsException()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[4]; // Very limited memory (addresses 0-3)
            registerFile.SetStackPointer(0x0004); // SP pointing to memory beyond bounds
            RetInstruction instruction = CreateTestInstruction();

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
            Assert.Contains("Stack overflow detected", exception.Message);
        }

        [Fact]
        public void Execute_StackPointerOverflow_ThrowsException()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0xFFFE); // SP + 2 would overflow
            RetInstruction instruction = CreateTestInstruction();

            // Simulate return address on stack
            memory[0xFFFE] = 0x00;
            memory[0xFFFF] = 0x80;

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
            Assert.Contains("Stack overflow detected", exception.Message);
        }

        [Fact]
        public void Execute_ValidStackBoundaries_WorksCorrectly()
        {
            // Arrange - Test near boundaries but within valid range
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x0002); // Minimum valid SP for RET

            // Set return address
            memory[0x0002] = 0x00;
            memory[0x0003] = 0x80; // Return to 0x8000

            RetInstruction instruction = CreateTestInstruction();

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Should work without exception
            Assert.Equal(0x8000, registerFile.GetProgramCounter());
            Assert.Equal(0x0004, registerFile.GetStackPointer());
        }
    }

    /// <summary>
    /// Tests for integration with CALL instruction.
    /// </summary>
    public class CallReturnIntegrationTests
    {
        [Fact]
        public void Execute_CallReturnSequence_RestoresOriginalState()
        {
            // Arrange - Setup initial state
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x1000,
                programCounter: 0x8000);

            // Store original values
            ushort originalSP = registerFile.GetStackPointer();
            ushort originalPC = registerFile.GetProgramCounter();

            // Execute CALL R4 (assume R4 contains 0x9000)
            registerFile.WriteRegister(RegisterName.R4, 0x9000);
            var callInstruction = new CallInstruction(0x1284, RegisterName.R4, AddressingMode.Register);
            callInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Verify CALL worked
            Assert.Equal(0x0FFE, registerFile.GetStackPointer()); // SP decremented
            Assert.Equal(0x9000, registerFile.GetProgramCounter()); // PC set to subroutine

            // Act - Execute RET
            RetInstruction retInstruction = CreateTestInstruction();
            retInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Check that original state is restored
            Assert.Equal(originalSP, registerFile.GetStackPointer());
            Assert.Equal(originalPC, registerFile.GetProgramCounter());
        }

        [Fact]
        public void Execute_MultipleCallReturnSequences_MaintainsStackIntegrity()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x1000,
                programCounter: 0x8000);

            // Execute multiple CALL/RET sequences
            for (int i = 0; i < 5; i++)
            {
                ushort currentSP = registerFile.GetStackPointer();
                ushort currentPC = registerFile.GetProgramCounter();

                // CALL
                registerFile.WriteRegister(RegisterName.R4, (ushort)(0x9000 + i * 0x100));
                var callInstruction = new CallInstruction(0x1284, RegisterName.R4, AddressingMode.Register);
                callInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

                // Verify CALL
                Assert.Equal((ushort)(currentSP - 2), registerFile.GetStackPointer());

                // RET
                RetInstruction retInstruction = CreateTestInstruction();
                retInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

                // Verify RET restored state
                Assert.Equal(currentSP, registerFile.GetStackPointer());
                Assert.Equal(currentPC, registerFile.GetProgramCounter());
            }
        }
    }

    /// <summary>
    /// Tests for string representation.
    /// </summary>
    public class StringRepresentationTests
    {
        [Fact]
        public void ToString_ReturnsCorrectMnemonic()
        {
            // Arrange
            RetInstruction instruction = CreateTestInstruction();

            // Act
            string result = instruction.ToString();

            // Assert
            Assert.Equal("RET", result);
        }
    }

    /// <summary>
    /// Tests for cycle count (always 4 per SLAU445I Table 4-8).
    /// </summary>
    public class CycleCountTests
    {
        [Fact]
        public void Execute_AlwaysReturns4Cycles()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            SimulateCall(memory, 0x0FFE, 0x8000);
            RetInstruction instruction = CreateTestInstruction();

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(4u, cycles); // Fixed cycle count per SLAU445I Table 4-8
        }
    }
}
