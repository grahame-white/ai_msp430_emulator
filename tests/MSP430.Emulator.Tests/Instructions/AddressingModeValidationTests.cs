using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions;

/// <summary>
/// Comprehensive addressing mode validation tests for MSP430 instructions.
/// 
/// Tests verify that all supported source/destination addressing mode combinations
/// work correctly for core instructions, ensuring compliance with MSP430FR2xx FR4xx 
/// Family User's Guide (SLAU445I) Section 4.5.1: Instruction Formats.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1: Instruction Formats
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.2: Addressing Modes
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Table 4-8: Instruction Cycles and Length
/// </summary>
public class AddressingModeValidationTests
{
    /// <summary>
    /// Creates a test environment with initialized register file and memory.
    /// </summary>
    /// <returns>A tuple containing the register file and memory array.</returns>
    private static (RegisterFile registerFile, byte[] memory) CreateTestEnvironment()
    {
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000]; // 64KB memory space

        // Initialize stack pointer to a safe location
        registerFile.SetStackPointer(0x8000);

        // Set up some test data in memory
        memory[0x0200] = 0x34; // Low byte at absolute address 0x0200
        memory[0x0201] = 0x12; // High byte at absolute address 0x0200
        memory[0x0202] = 0x78; // Low byte at absolute address 0x0202
        memory[0x0203] = 0x56; // High byte at absolute address 0x0202

        return (registerFile, memory);
    }

    /// <summary>
    /// Tests for ADD instruction addressing mode combinations.
    /// </summary>
    public class AddInstructionAddressingModeTests
    {
        [Fact]
        public void ADD_RegisterToRegister_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x1000);
            registerFile.WriteRegister(RegisterName.R5, 0x2000);

            var addInstruction = new AddInstruction(
                0x5445, // ADD R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                false);

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x3000, registerFile.ReadRegister(RegisterName.R5)); // R5 = R4 + R5
            Assert.Equal(1u, cycles); // Register-to-register should be 1 cycle
        }

        [Fact]
        public void ADD_IndirectToRegister_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x0200); // Points to memory location
            registerFile.WriteRegister(RegisterName.R5, 0x1000);

            var addInstruction = new AddInstruction(
                0x5485, // ADD @R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Indirect,
                AddressingMode.Register,
                false);

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Memory at 0x0200 contains 0x1234, so R5 = 0x1000 + 0x1234 = 0x2234
            Assert.Equal(0x2234, registerFile.ReadRegister(RegisterName.R5));
            Assert.Equal(2u, cycles); // Indirect source should be 2 cycles
        }

        [Fact]
        public void ADD_IndirectAutoIncrementToRegister_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x0200); // Points to memory location
            registerFile.WriteRegister(RegisterName.R5, 0x1000);

            var addInstruction = new AddInstruction(
                0x54C5, // ADD @R4+, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.IndirectAutoIncrement,
                AddressingMode.Register,
                false);

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Memory at 0x0200 contains 0x1234, so R5 = 0x1000 + 0x1234 = 0x2234
            Assert.Equal(0x2234, registerFile.ReadRegister(RegisterName.R5));
            // R4 should be incremented by 2 (word operation)
            Assert.Equal(0x0202, registerFile.ReadRegister(RegisterName.R4));
            Assert.Equal(2u, cycles); // Auto-increment source should be 2 cycles
        }

        [Fact]
        public void ADD_ImmediateToRegister_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R5, 0x1000);

            var addInstruction = new AddInstruction(
                0x5525, // ADD #0x1234, R5
                RegisterName.R2, // Source register for immediate
                RegisterName.R5,
                AddressingMode.Immediate,
                AddressingMode.Register,
                false);

            ushort[] extensionWords = { 0x1234 }; // Immediate value

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, extensionWords);

            // Assert
            Assert.Equal(0x2234, registerFile.ReadRegister(RegisterName.R5)); // R5 = 0x1000 + 0x1234
            Assert.Equal(2u, cycles); // Immediate source should be 2 cycles
        }

        [Fact]
        public void ADD_RegisterToIndirect_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x0500);
            registerFile.WriteRegister(RegisterName.R5, 0x0200); // Points to memory location

            // Initialize destination memory
            memory[0x0200] = 0x34; // Low byte
            memory[0x0201] = 0x12; // High byte -> 0x1234

            var addInstruction = new AddInstruction(
                0x5495, // ADD R4, @R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Indirect,
                false);

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Memory at 0x0200 should now contain 0x1234 + 0x0500 = 0x1734
            ushort result = (ushort)(memory[0x0200] | (memory[0x0201] << 8));
            Assert.Equal(0x1734, result);
            Assert.Equal(3u, cycles); // Register source + indirect destination = 3 cycles (legacy calculation)
        }

        [Fact]
        public void ADD_AbsoluteToRegister_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R5, 0x1000);

            var addInstruction = new AddInstruction(
                0x5525, // ADD &0x0200, R5
                RegisterName.R2, // Source register for absolute
                RegisterName.R5,
                AddressingMode.Absolute,
                AddressingMode.Register,
                false);

            ushort[] extensionWords = { 0x0200 }; // Absolute address

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, extensionWords);

            // Assert
            // Memory at 0x0200 contains 0x1234, so R5 = 0x1000 + 0x1234 = 0x2234
            Assert.Equal(0x2234, registerFile.ReadRegister(RegisterName.R5));
            Assert.Equal(3u, cycles); // Absolute source should be 3 cycles
        }

        [Fact]
        public void ADD_SymbolicToRegister_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R5, 0x1000);
            registerFile.SetProgramCounter(0x0100); // Set PC for symbolic addressing

            var addInstruction = new AddInstruction(
                0x5505, // ADD LABEL, R5 (symbolic)
                RegisterName.R0, // Source register for symbolic (PC)
                RegisterName.R5,
                AddressingMode.Symbolic,
                AddressingMode.Register,
                false);

            ushort[] extensionWords = { 0x0100 }; // Offset from PC -> 0x0100 + 0x0100 = 0x0200

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, extensionWords);

            // Assert
            // Memory at PC + offset (0x0100 + 0x0100 = 0x0200) contains 0x1234
            Assert.Equal(0x2234, registerFile.ReadRegister(RegisterName.R5));
            Assert.Equal(3u, cycles); // Symbolic source should be 3 cycles
        }

        [Fact]
        public void ADD_IndexedToRegister_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x0100); // Base address
            registerFile.WriteRegister(RegisterName.R5, 0x1000);

            var addInstruction = new AddInstruction(
                0x5545, // ADD 0x0100(R4), R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Indexed,
                AddressingMode.Register,
                false);

            ushort[] extensionWords = { 0x0100 }; // Index offset -> 0x0100 + 0x0100 = 0x0200

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, extensionWords);

            // Assert
            // Memory at R4 + offset (0x0100 + 0x0100 = 0x0200) contains 0x1234
            Assert.Equal(0x2234, registerFile.ReadRegister(RegisterName.R5));
            Assert.Equal(3u, cycles); // Indexed source should be 3 cycles
        }
    }

    /// <summary>
    /// Tests for byte operations with different addressing modes.
    /// </summary>
    public class ByteOperationAddressingModeTests
    {
        [Fact]
        public void ADD_Byte_RegisterToRegister_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x12FF); // High byte should be ignored
            registerFile.WriteRegister(RegisterName.R5, 0x3401); // High byte should be preserved

            var addInstruction = new AddInstruction(
                0x5445, // ADD.B R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                true); // Byte operation

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Low byte: 0xFF + 0x01 = 0x100 -> 0x00 (with carry)
            // High byte of R5 should be preserved
            Assert.Equal(0x3400, registerFile.ReadRegister(RegisterName.R5));
            Assert.Equal(1u, cycles);
            Assert.True(registerFile.StatusRegister.Carry); // Should set carry
            Assert.True(registerFile.StatusRegister.Zero); // Result is zero
        }

        [Fact]
        public void ADD_Byte_IndirectAutoIncrementToRegister_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x0200); // Points to memory
            registerFile.WriteRegister(RegisterName.R5, 0x1200); // High byte should be preserved

            var addInstruction = new AddInstruction(
                0x54C5, // ADD.B @R4+, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.IndirectAutoIncrement,
                AddressingMode.Register,
                true); // Byte operation

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Memory at 0x0200 contains 0x34 (low byte), so R5 low = 0x00 + 0x34 = 0x34
            Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R5));
            // R4 should be incremented by 1 (byte operation)
            Assert.Equal(0x0201, registerFile.ReadRegister(RegisterName.R4));
            Assert.Equal(2u, cycles);
        }
    }

    /// <summary>
    /// Tests for constant generator addressing modes.
    /// </summary>
    public class ConstantGeneratorAddressingModeTests
    {
        [Fact]
        public void ADD_ConstantZero_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R5, 0x1234);

            var addInstruction = new AddInstruction(
                0x5305, // ADD R3, R5 (R3 in register mode = constant 0)
                RegisterName.R3,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                false);

            // Act
            uint cycles = addInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R5)); // R5 = R5 + 0 = R5
            Assert.Equal(1u, cycles);
            Assert.False(registerFile.StatusRegister.Carry);
            Assert.False(registerFile.StatusRegister.Zero);
        }

        [Fact]
        public void ADD_ConstantOne_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R5, 0x1234);

            var addInstruction = new AddInstruction(
                0x5325, // ADD #1, R5 (R3 in immediate mode = constant 1)
                RegisterName.R3,
                RegisterName.R5,
                AddressingMode.Immediate,
                AddressingMode.Register,
                false);

            // Act - No extension words needed for constant generator
            uint cycles = addInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1235, registerFile.ReadRegister(RegisterName.R5)); // R5 = R5 + 1
            Assert.Equal(1u, cycles); // Constant generator should be 1 cycle
            Assert.False(registerFile.StatusRegister.Carry);
            Assert.False(registerFile.StatusRegister.Zero);
        }

        [Fact]
        public void ADD_ConstantFour_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R5, 0x1230);

            var addInstruction = new AddInstruction(
                0x5285, // ADD @R2, R5 (R2 in indirect mode = constant 4)
                RegisterName.R2,
                RegisterName.R5,
                AddressingMode.Indirect,
                AddressingMode.Register,
                false);

            // Act - No extension words needed for constant generator
            uint cycles = addInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R5)); // R5 = R5 + 4
            Assert.Equal(1u, cycles); // Constant generator should be 1 cycle
        }

        [Fact]
        public void ADD_ConstantEight_WorksCorrectly()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R5, 0x1230);

            var addInstruction = new AddInstruction(
                0x52C5, // ADD @R2+, R5 (R2 in auto-increment mode = constant 8)
                RegisterName.R2,
                RegisterName.R5,
                AddressingMode.IndirectAutoIncrement,
                AddressingMode.Register,
                false);

            // Act - No extension words needed for constant generator
            uint cycles = addInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1238, registerFile.ReadRegister(RegisterName.R5)); // R5 = R5 + 8
            Assert.Equal(1u, cycles); // Constant generator should be 1 cycle
        }
    }
}
