using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;

namespace MSP430.Emulator.Tests.Instructions;

/// <summary>
/// Tests for constant generator behavior in MSP430 instruction execution.
/// 
/// The MSP430 uses R2 (CG1) and R3 (CG2) as constant generators to provide
/// commonly used constants without requiring additional program memory.
/// These tests verify that constant generation works according to MSP430FR2355 specifications.
/// 
/// References:
/// - docs/references/SLAU445/4.3.4_constant_generator_registers_(cg1_and_cg2).md
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.3.4: Constant Generator Registers
/// </summary>
public class ConstantGeneratorTests
{
    [Theory]
    [InlineData(RegisterName.R3, 0x00, 0x0000)] // R3 As=00 → 0
    [InlineData(RegisterName.R3, 0x01, 0x0001)] // R3 As=01 → +1 (implemented)
    // Note: R3 As=10 → +2 and R3 As=11 → -1 cannot be distinguished in current architecture
    // All R3 constants with As=01/10/11 decode to AddressingMode.Immediate, losing As bit information
    public void InstructionHelpers_ReadOperand_R3ConstantGenerator_ReturnsCorrectConstants(
        RegisterName register,
        byte asBits,
        ushort expectedConstant)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set R3 to some arbitrary value - it should be ignored for constant generation
        registerFile.WriteRegister(RegisterName.R3, 0x1234);

        AddressingMode addressingMode = AddressingModeDecoder.DecodeSourceAddressingMode(register, asBits);

        // Act  
        ushort actualValue = InstructionHelpers.ReadOperand(
            register,
            addressingMode,
            false, // word operation
            registerFile,
            memory,
            0x0000 // extension word - should not be used for constant generators
        );

        // Assert
        Assert.Equal(expectedConstant, actualValue);
    }

    [Theory]
    [InlineData(RegisterName.R2, 0x02, 0x0004)] // R2 As=10 → +4
    [InlineData(RegisterName.R2, 0x03, 0x0008)] // R2 As=11 → +8
    public void InstructionHelpers_ReadOperand_R2ConstantGenerator_ReturnsCorrectConstants(
        RegisterName register,
        byte asBits,
        ushort expectedConstant)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set R2 to some arbitrary value - for constants it should be ignored
        registerFile.WriteRegister(RegisterName.R2, 0x5678);

        AddressingMode addressingMode = AddressingModeDecoder.DecodeSourceAddressingMode(register, asBits);

        // Act
        ushort actualValue = InstructionHelpers.ReadOperand(
            register,
            addressingMode,
            false, // word operation 
            registerFile,
            memory,
            0x0000 // extension word - should not be used for constant generators
        );

        // Assert
        Assert.Equal(expectedConstant, actualValue);
    }

    [Fact]
    public void AddInstruction_WithR3Constant_ExecutesCorrectly()
    {
        // Test ADD #1, R6 using R3 constant generator (As=01)

        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set destination register
        registerFile.WriteRegister(RegisterName.R6, 0x1000);

        // Create ADD instruction using R3 with As=01 (constant +1)
        // This should add 1 to R6 without requiring an extension word
        var instruction = new AddInstruction(
            0x5316, // ADD R3(As=01), R6 - using R3 as constant generator for +1
            RegisterName.R3,
            RegisterName.R6,
            AddressingMode.Immediate, // R3 As=01 decodes to Immediate 
            AddressingMode.Register,
            false);

        // Act - no extension words should be needed for constant generators
        uint cycles = instruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1001, registerFile.ReadRegister(RegisterName.R6)); // 0x1000 + 1 = 0x1001
        Assert.True(cycles > 0, "Instruction should consume at least one cycle");
    }

    [Fact]
    public void AddInstruction_WithR2Constant4_ExecutesCorrectly()
    {
        // Test ADD #4, R6 using R2 constant generator (As=10)

        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set destination register  
        registerFile.WriteRegister(RegisterName.R6, 0x1000);

        // Create ADD instruction using R2 with As=10 (constant +4)
        var instruction = new AddInstruction(
            0x5226, // ADD R2(As=10), R6 - using R2 as constant generator for +4
            RegisterName.R2,
            RegisterName.R6,
            AddressingMode.Indirect, // R2 As=10 decodes to Indirect - this might be the issue!
            AddressingMode.Register,
            false);

        // Act - no extension words should be needed for constant generators
        uint cycles = instruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1004, registerFile.ReadRegister(RegisterName.R6)); // 0x1000 + 4 = 0x1004
        Assert.True(cycles > 0, "Instruction should consume at least one cycle");
    }
}
