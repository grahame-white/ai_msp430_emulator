using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for the RetiInstruction class.
/// 
/// Tests cover the Return from Interrupt (RETI) instruction behavior including:
/// - Status register restoration from stack
/// - Program counter restoration from stack
/// - Stack pointer management
/// - Error conditions and edge cases
/// - Cycle count verification
/// </summary>
public class RetiInstructionTests
{
    /// <summary>
    /// Creates a test environment with initialized register file and memory.
    /// </summary>
    /// <param name="stackPointer">Initial stack pointer value.</param>
    /// <param name="programCounter">Initial program counter value.</param>
    /// <returns>A tuple containing the register file and memory array.</returns>
    private static (RegisterFile registerFile, byte[] memory) CreateTestEnvironment(
        ushort stackPointer = 0x1000,
        ushort programCounter = 0x8000)
    {
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000]; // 64KB memory

        // Initialize stack pointer and program counter
        registerFile.SetStackPointer(stackPointer);
        registerFile.SetProgramCounter(programCounter);

        return (registerFile, memory);
    }

    /// <summary>
    /// Creates a test RETI instruction.
    /// </summary>
    /// <returns>A RetiInstruction instance.</returns>
    private static RetiInstruction CreateTestInstruction()
    {
        // RETI instruction word: 0x1300 per SLAU445I Table 4-6
        return new RetiInstruction(0x1300);
    }

    /// <summary>
    /// Tests basic instruction properties.
    /// </summary>
    public class BasicPropertiesTests
    {
        [Fact]
        public void Mnemonic_ReturnsCorrectValue()
        {
            RetiInstruction instruction = CreateTestInstruction();
            Assert.Equal("RETI", instruction.Mnemonic);
        }

        [Fact]
        public void Format_ReturnsFormatII()
        {
            RetiInstruction instruction = CreateTestInstruction();
            Assert.Equal(InstructionFormat.FormatII, instruction.Format);
        }

        [Fact]
        public void Opcode_ReturnsCorrectValue()
        {
            RetiInstruction instruction = CreateTestInstruction();
            Assert.Equal(0x13, instruction.Opcode);
        }

        [Fact]
        public void IsByteOperation_ReturnsFalse()
        {
            RetiInstruction instruction = CreateTestInstruction();
            Assert.False(instruction.IsByteOperation);
        }

        [Fact]
        public void ExtensionWordCount_ReturnsZero()
        {
            RetiInstruction instruction = CreateTestInstruction();
            Assert.Equal(0, instruction.ExtensionWordCount);
        }

        [Fact]
        public void SourceRegister_ReturnsNull()
        {
            RetiInstruction instruction = CreateTestInstruction();
            Assert.Null(instruction.SourceRegister);
        }

        [Fact]
        public void DestinationRegister_ReturnsNull()
        {
            RetiInstruction instruction = CreateTestInstruction();
            Assert.Null(instruction.DestinationRegister);
        }

        [Fact]
        public void SourceAddressingMode_ReturnsNull()
        {
            RetiInstruction instruction = CreateTestInstruction();
            Assert.Null(instruction.SourceAddressingMode);
        }

        [Fact]
        public void DestinationAddressingMode_ReturnsNull()
        {
            RetiInstruction instruction = CreateTestInstruction();
            Assert.Null(instruction.DestinationAddressingMode);
        }

        [Fact]
        public void ToString_ReturnsMnemonic()
        {
            RetiInstruction instruction = CreateTestInstruction();
            Assert.Equal("RETI", instruction.ToString());
        }
    }

    /// <summary>
    /// Tests for normal RETI execution behavior.
    /// </summary>
    public class ExecutionTests
    {
        [Fact]
        public void Execute_NormalOperation_RestoresStatusRegisterFromStack()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x1000,
                programCounter: 0x8000);

            // Set up stack with SR and PC values (little-endian format)
            ushort expectedSR = 0x0155; // Some status register value with various flags set
            ushort expectedPC = 0x9000; // Return address

            // Place SR at stack pointer (little-endian)
            memory[0x1000] = (byte)(expectedSR & 0xFF);         // SR low byte
            memory[0x1001] = (byte)((expectedSR >> 8) & 0xFF);  // SR high byte

            // Place PC at stack pointer + 2 (little-endian)
            memory[0x1002] = (byte)(expectedPC & 0xFF);         // PC low byte
            memory[0x1003] = (byte)((expectedPC >> 8) & 0xFF);  // PC high byte

            // Set initial SR to different value to verify restoration
            registerFile.StatusRegister.Value = 0x0000;

            RetiInstruction instruction = CreateTestInstruction();

            // Act
            uint cycleCount = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(expectedSR, registerFile.StatusRegister.Value);
            Assert.Equal(expectedPC, registerFile.GetProgramCounter());
            Assert.Equal(0x1004, registerFile.GetStackPointer()); // SP should be incremented by 4
            Assert.Equal(5U, cycleCount); // RETI takes 5 cycles per SLAU445I Table 4-8
        }

        [Fact]
        public void Execute_NormalOperation_RestoresProgramCounterFromStack()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x2000,
                programCounter: 0x8000);

            // Set up stack with SR and PC values
            ushort testSR = 0x0008; // GIE bit set
            ushort expectedPC = 0xA500; // Specific return address

            // Place values on stack (little-endian)
            memory[0x2000] = (byte)(testSR & 0xFF);
            memory[0x2001] = (byte)((testSR >> 8) & 0xFF);
            memory[0x2002] = (byte)(expectedPC & 0xFF);
            memory[0x2003] = (byte)((expectedPC >> 8) & 0xFF);

            RetiInstruction instruction = CreateTestInstruction();

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(expectedPC, registerFile.GetProgramCounter());
        }

        [Fact]
        public void Execute_NormalOperation_UpdatesStackPointerCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(
                stackPointer: 0x0800);

            // Set up stack with test values
            memory[0x0800] = 0x08; // SR low byte (GIE set)
            memory[0x0801] = 0x00; // SR high byte
            memory[0x0802] = 0x00; // PC low byte
            memory[0x0803] = 0x90; // PC high byte (0x9000)

            RetiInstruction instruction = CreateTestInstruction();

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Stack pointer should be incremented by 4 (2 for SR + 2 for PC)
            Assert.Equal(0x0804, registerFile.GetStackPointer());
        }

        [Theory]
        [InlineData(0x0001)] // Carry flag
        [InlineData(0x0002)] // Zero flag
        [InlineData(0x0004)] // Negative flag
        [InlineData(0x0008)] // GIE flag
        [InlineData(0x0100)] // Overflow flag
        [InlineData(0x01FF)] // Multiple flags
        public void Execute_VariousStatusRegisterValues_RestoresCorrectly(ushort statusRegisterValue)
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Place SR and PC on stack
            memory[0x1000] = (byte)(statusRegisterValue & 0xFF);
            memory[0x1001] = (byte)((statusRegisterValue >> 8) & 0xFF);
            memory[0x1002] = 0x00; // PC low byte
            memory[0x1003] = 0x80; // PC high byte

            // Set different initial value
            registerFile.StatusRegister.Value = 0x0000;

            RetiInstruction instruction = CreateTestInstruction();

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(statusRegisterValue, registerFile.StatusRegister.Value);
        }

        [Fact]
        public void Execute_AlwaysReturns5Cycles()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Set up valid stack data
            memory[0x1000] = 0x08; // SR
            memory[0x1001] = 0x00;
            memory[0x1002] = 0x00; // PC
            memory[0x1003] = 0x80;

            RetiInstruction instruction = CreateTestInstruction();

            // Act
            uint cycleCount = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(5U, cycleCount);
        }
    }

    /// <summary>
    /// Tests for error conditions and edge cases.
    /// </summary>
    public class ErrorConditionTests
    {
        [Fact]
        public void Execute_StackPointerAtMemoryBoundary_ThrowsException()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[4]; // Very limited memory (addresses 0-3)
            registerFile.SetStackPointer(0x0002); // SP=2, needs to access 2,3,4,5 (4,5 are beyond bounds)

            RetiInstruction instruction = CreateTestInstruction();

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
            Assert.Contains("Stack overflow detected", exception.Message);
        }

        [Fact]
        public void Execute_StackPointerNearMemoryEnd_ThrowsException()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[0x1002]; // Memory from 0x0000 to 0x1001
            registerFile.SetStackPointer(0x1000); // SP=0x1000, needs to access 0x1000,0x1001,0x1002,0x1003 (0x1002,0x1003 are beyond bounds)

            RetiInstruction instruction = CreateTestInstruction();

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
            Assert.Contains("Stack overflow detected", exception.Message);
        }

        [Fact]
        public void Execute_StackPointerOverflowAfterFirstIncrement_ThrowsException()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[0x10000];
            registerFile.SetStackPointer(0xFFFF); // SP that would overflow when incremented

            RetiInstruction instruction = CreateTestInstruction();

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
            Assert.Contains("Stack overflow detected", exception.Message);
        }

        [Fact]
        public void Execute_StackPointerOverflowAfterSecondIncrement_ThrowsException()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[0x10000];
            registerFile.SetStackPointer(0xFFFE); // SP that would overflow on second increment (0xFFFE -> 0x0000 -> 0x0002 fails)

            RetiInstruction instruction = CreateTestInstruction();

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
            Assert.Contains("Stack overflow detected", exception.Message);
        }
    }

    /// <summary>
    /// Tests for stack management behavior.
    /// </summary>
    public class StackManagementTests
    {
        [Fact]
        public void Execute_MinimumValidStackPointer_ExecutesSuccessfully()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[0x10000];
            registerFile.SetStackPointer(0x0000); // Minimum valid SP that can access 4 bytes

            // Set up stack data
            memory[0x0000] = 0x08; // SR
            memory[0x0001] = 0x00;
            memory[0x0002] = 0x00; // PC
            memory[0x0003] = 0x80;

            RetiInstruction instruction = CreateTestInstruction();

            // Act & Assert - Should not throw
            uint cycleCount = instruction.Execute(registerFile, memory, Array.Empty<ushort>());
            Assert.Equal(5U, cycleCount);
            Assert.Equal(0x0004, registerFile.GetStackPointer());
        }

        [Fact]
        public void Execute_StackDataConsistency_MaintainsLittleEndianFormat()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Set up specific test values in little-endian format
            ushort testSR = 0x1234;
            ushort testPC = 0x5678;

            memory[0x1000] = 0x34; // SR low byte
            memory[0x1001] = 0x12; // SR high byte
            memory[0x1002] = 0x78; // PC low byte
            memory[0x1003] = 0x56; // PC high byte

            RetiInstruction instruction = CreateTestInstruction();

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(testSR, registerFile.StatusRegister.Value);
            Assert.Equal(testPC, registerFile.GetProgramCounter());
        }
    }
}
