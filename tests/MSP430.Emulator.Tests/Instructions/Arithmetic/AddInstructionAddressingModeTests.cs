using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;

namespace MSP430.Emulator.Tests.Instructions.Arithmetic;

/// <summary>
/// Comprehensive unit tests for ADD instruction addressing mode combinations.
/// 
/// Tests verify that ADD instruction works correctly with all valid source/destination
/// addressing mode combinations as specified in MSP430FR2355 documentation:
/// 
/// Source Modes: Register, Indexed, Indirect, IndirectAutoIncrement, Immediate, Absolute, Symbolic
/// Destination Modes: Register, Indexed, Absolute, Symbolic
/// 
/// Special cases verified:
/// - Constant generators (R2 and R3) work correctly for source operands only
/// - Destination operands never use constant generator rules
/// - Extension word handling for each addressing mode
/// - Cycle count calculations
/// - Byte vs word operation handling
/// 
/// References:
/// - docs/references/SLAU445/4.4_addressing_modes.md - Addressing modes overview
/// - docs/references/SLAU445/4.6.2.2_add.md - ADD instruction specification  
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.4: Addressing Modes
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.2: ADD instruction
/// </summary>
public class AddInstructionAddressingModeTests
{
    [Fact]
    public void Execute_RegisterToRegister_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        registerFile.WriteRegister(RegisterName.R5, 0x1234);
        registerFile.WriteRegister(RegisterName.R6, 0x5678);

        var instruction = new AddInstruction(
            0x5560, // ADD R5, R6
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x68AC, registerFile.ReadRegister(RegisterName.R6)); // 0x1234 + 0x5678 = 0x68AC
        Assert.Equal(1u, cycles); // Register to register should be 1 cycle
    }

    [Fact]
    public void Execute_ImmediateToRegister_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        registerFile.WriteRegister(RegisterName.R6, 0x1000);

        var instruction = new AddInstruction(
            0x5036, // ADD #0x0500, R6
            RegisterName.R0,
            RegisterName.R6,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x0500 };

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x1500, registerFile.ReadRegister(RegisterName.R6));
        Assert.Equal(2u, cycles); // #N → Rm = 2 cycles per SLAU445 Table 4-10
    }

    [Fact]
    public void Execute_AbsoluteToRegister_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up memory at absolute address 0x2000
        memory[0x2000] = 0x34; // Low byte
        memory[0x2001] = 0x12; // High byte (0x1234 in little-endian)

        registerFile.WriteRegister(RegisterName.R6, 0x1000);

        var instruction = new AddInstruction(
            0x5026, // ADD &0x2000, R6
            RegisterName.R2,
            RegisterName.R6,
            AddressingMode.Absolute,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x2000 };

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x2234, registerFile.ReadRegister(RegisterName.R6)); // 0x1000 + 0x1234 = 0x2234
        Assert.Equal(3u, cycles); // &EDE → Rm = 3 cycles per SLAU445 Table 4-10
    }

    [Fact]
    public void Execute_RegisterToAbsolute_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up initial value in memory at absolute address 0x2000
        memory[0x2000] = 0x00; // Low byte
        memory[0x2001] = 0x10; // High byte (0x1000 in little-endian)

        registerFile.WriteRegister(RegisterName.R5, 0x0234);

        var instruction = new AddInstruction(
            0x5520, // ADD R5, &0x2000
            RegisterName.R5,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = { 0x2000 };

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert - memory should contain 0x1000 + 0x0234 = 0x1234
        Assert.Equal(0x34, memory[0x2000]); // Low byte
        Assert.Equal(0x12, memory[0x2001]); // High byte
        Assert.Equal(4u, cycles); // Rn → &EDE = 4 cycles per SLAU445 Table 4-10
    }

    [Fact]
    public void Execute_IndirectToRegister_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set R5 to point to address 0x2000
        registerFile.WriteRegister(RegisterName.R5, 0x2000);

        // Set up value at address 0x2000
        memory[0x2000] = 0x78; // Low byte
        memory[0x2001] = 0x56; // High byte (0x5678 in little-endian)

        registerFile.WriteRegister(RegisterName.R6, 0x1234);

        var instruction = new AddInstruction(
            0x5560, // ADD @R5, R6
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Indirect,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x68AC, registerFile.ReadRegister(RegisterName.R6)); // 0x1234 + 0x5678 = 0x68AC
        Assert.Equal(2u, cycles); // @Rn → Rm = 2 cycles per SLAU445 Table 4-10
    }

    [Fact]
    public void Execute_IndirectAutoIncrementToRegister_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set R5 to point to address 0x2000
        registerFile.WriteRegister(RegisterName.R5, 0x2000);

        // Set up value at address 0x2000
        memory[0x2000] = 0x78; // Low byte
        memory[0x2001] = 0x56; // High byte (0x5678 in little-endian)

        registerFile.WriteRegister(RegisterName.R6, 0x1234);

        var instruction = new AddInstruction(
            0x5560, // ADD @R5+, R6
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x68AC, registerFile.ReadRegister(RegisterName.R6)); // 0x1234 + 0x5678 = 0x68AC
    }

    [Fact]
    public void Execute_IndirectAutoIncrementToRegister_IncrementsSourceRegister()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set R5 to point to address 0x2000
        registerFile.WriteRegister(RegisterName.R5, 0x2000);

        // Set up value at address 0x2000
        memory[0x2000] = 0x78; // Low byte
        memory[0x2001] = 0x56; // High byte (0x5678 in little-endian)

        registerFile.WriteRegister(RegisterName.R6, 0x1234);

        var instruction = new AddInstruction(
            0x5560, // ADD @R5+, R6
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x2002, registerFile.ReadRegister(RegisterName.R5)); // R5 should be incremented by 2 for word operation
    }

    [Fact]
    public void Execute_IndirectAutoIncrementToRegister_ReturnsCorrectCycles()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set R5 to point to address 0x2000
        registerFile.WriteRegister(RegisterName.R5, 0x2000);

        // Set up value at address 0x2000
        memory[0x2000] = 0x78; // Low byte
        memory[0x2001] = 0x56; // High byte (0x5678 in little-endian)

        registerFile.WriteRegister(RegisterName.R6, 0x1234);

        var instruction = new AddInstruction(
            0x5560, // ADD @R5+, R6
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(2u, cycles); // @Rn+ → Rm = 2 cycles per SLAU445 Table 4-10
    }

    [Fact]
    public void Execute_IndirectAutoIncrementByteOperation_IncrementsBy1()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set R5 to point to address 0x2000
        registerFile.WriteRegister(RegisterName.R5, 0x2000);

        // Set up byte value at address 0x2000
        memory[0x2000] = 0x78;

        registerFile.WriteRegister(RegisterName.R6, 0x34);

        var instruction = new AddInstruction(
            0x5560, // ADD.B @R5+, R6
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            true); // Byte operation

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0xAC, registerFile.ReadRegister(RegisterName.R6) & 0xFF); // 0x34 + 0x78 = 0xAC
        Assert.Equal(0x2001, registerFile.ReadRegister(RegisterName.R5)); // R5 should be incremented by 1 for byte operation
        Assert.Equal(3u, cycles);
    }

    [Fact]
    public void Execute_IndexedToRegister_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set R5 to base address 0x2000
        registerFile.WriteRegister(RegisterName.R5, 0x2000);

        // Set up value at address 0x2000 + 0x0010 = 0x2010
        memory[0x2010] = 0x78; // Low byte
        memory[0x2011] = 0x56; // High byte (0x5678 in little-endian)

        registerFile.WriteRegister(RegisterName.R6, 0x1234);

        var instruction = new AddInstruction(
            0x5560, // ADD 0x10(R5), R6
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Indexed,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x0010 }; // Index offset

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x68AC, registerFile.ReadRegister(RegisterName.R6)); // 0x1234 + 0x5678 = 0x68AC
        Assert.Equal(3u, cycles); // x(Rn) → Rm = 3 cycles per SLAU445 Table 4-10
    }

    [Fact]
    public void Execute_RegisterToIndexed_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set R6 to base address 0x2000
        registerFile.WriteRegister(RegisterName.R6, 0x2000);

        // Set up initial value at address 0x2000 + 0x0010 = 0x2010
        memory[0x2010] = 0x00; // Low byte
        memory[0x2011] = 0x10; // High byte (0x1000 in little-endian)

        registerFile.WriteRegister(RegisterName.R5, 0x0234);

        var instruction = new AddInstruction(
            0x5560, // ADD R5, 0x10(R6)
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Indexed,
            false);

        ushort[] extensionWords = { 0x0010 }; // Index offset

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert - memory at 0x2010 should contain 0x1000 + 0x0234 = 0x1234
        Assert.Equal(0x34, memory[0x2010]); // Low byte
        Assert.Equal(0x12, memory[0x2011]); // High byte
        Assert.Equal(4u, cycles); // Rn → x(Rm) = 4 cycles per SLAU445 Table 4-10
    }

    [Fact]
    public void Execute_SymbolicToRegister_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set PC to 0x1000 for symbolic addressing calculation
        registerFile.SetProgramCounter(0x1000);

        // Set up value at address PC + offset = 0x1000 + 0x1000 = 0x2000
        memory[0x2000] = 0x78; // Low byte
        memory[0x2001] = 0x56; // High byte (0x5678 in little-endian)

        registerFile.WriteRegister(RegisterName.R6, 0x1234);

        var instruction = new AddInstruction(
            0x5060, // ADD SYMB, R6 (where SYMB is PC-relative)
            RegisterName.R0,
            RegisterName.R6,
            AddressingMode.Symbolic,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x1000 }; // PC-relative offset

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x68AC, registerFile.ReadRegister(RegisterName.R6)); // 0x1234 + 0x5678 = 0x68AC
        Assert.Equal(3u, cycles); // EDE → Rm = 3 cycles per SLAU445 Table 4-10
    }

    [Fact]
    public void Execute_RegisterToSymbolic_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set PC to 0x1000 for symbolic addressing calculation
        registerFile.SetProgramCounter(0x1000);

        // Set up initial value at address PC + offset = 0x1000 + 0x1000 = 0x2000
        memory[0x2000] = 0x00; // Low byte
        memory[0x2001] = 0x10; // High byte (0x1000 in little-endian)

        registerFile.WriteRegister(RegisterName.R5, 0x0234);

        var instruction = new AddInstruction(
            0x5500, // ADD R5, SYMB (where SYMB is PC-relative)
            RegisterName.R5,
            RegisterName.R0,
            AddressingMode.Register,
            AddressingMode.Symbolic,
            false);

        ushort[] extensionWords = { 0x1000 }; // PC-relative offset

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert - memory at 0x2000 should contain 0x1000 + 0x0234 = 0x1234
        Assert.Equal(0x34, memory[0x2000]); // Low byte
        Assert.Equal(0x12, memory[0x2001]); // High byte
        Assert.Equal(4u, cycles); // Rn → EDE = 4 cycles per SLAU445 Table 4-10
    }

    [Fact]
    public void Execute_ConstantGeneratorR2Plus4_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        registerFile.WriteRegister(RegisterName.R6, 0x1000);

        var instruction = new AddInstruction(
            0x5260, // ADD @R2, R6 (R2 indirect generates constant +4)
            RegisterName.R2,
            RegisterName.R6,
            AddressingMode.Indirect,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1004, registerFile.ReadRegister(RegisterName.R6)); // 0x1000 + 4 = 0x1004
        Assert.Equal(1u, cycles); // Constant generator should be 1 cycle
    }

    [Fact]
    public void Execute_ConstantGeneratorR2Plus8_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        registerFile.WriteRegister(RegisterName.R6, 0x1000);

        var instruction = new AddInstruction(
            0x5360, // ADD @R2+, R6 (R2 indirect autoincrement generates constant +8)
            RegisterName.R2,
            RegisterName.R6,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1008, registerFile.ReadRegister(RegisterName.R6)); // 0x1000 + 8 = 0x1008
        Assert.Equal(1u, cycles); // Constant generator should be 1 cycle
    }

    [Fact]
    public void Execute_ConstantGeneratorR3Zero_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        registerFile.WriteRegister(RegisterName.R6, 0x1234);

        var instruction = new AddInstruction(
            0x5360, // ADD R3, R6 (R3 register mode generates constant 0)
            RegisterName.R3,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R6)); // 0x1234 + 0 = 0x1234
        Assert.Equal(1u, cycles); // Constant generator should be 1 cycle
    }

    [Fact]
    public void Execute_ConstantGeneratorR3Plus1_WorksCorrectly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        registerFile.WriteRegister(RegisterName.R6, 0x1234);

        var instruction = new AddInstruction(
            0x5160, // ADD #1, R6 (R3 immediate mode generates constant +1)
            RegisterName.R3,
            RegisterName.R6,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1235, registerFile.ReadRegister(RegisterName.R6)); // 0x1234 + 1 = 0x1235
        Assert.Equal(1u, cycles); // Constant generator should be 1 cycle
    }

    [Fact]
    public void Execute_DestinationNeverUsesConstantGenerator_VerifyR2AsDestination()
    {
        // Arrange - This test verifies that R2 as destination doesn't use constant generator rules
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set initial value in status register (R2)
        registerFile.StatusRegister.Value = 0x0100;

        registerFile.WriteRegister(RegisterName.R5, 0x0234);

        var instruction = new AddInstruction(
            0x5520, // ADD R5, R2 (destination R2 should NOT use constant generator)
            RegisterName.R5,
            RegisterName.R2,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert - R2 should be treated as a normal register destination
        Assert.Equal(0x0334, registerFile.StatusRegister.Value); // 0x0100 + 0x0234 = 0x0334
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_DestinationNeverUsesConstantGenerator_VerifyR3AsDestination()
    {
        // Arrange - This test verifies that R3 as destination doesn't use constant generator rules
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set initial value in R3
        registerFile.WriteRegister(RegisterName.R3, 0x1000);
        registerFile.WriteRegister(RegisterName.R5, 0x0234);

        var instruction = new AddInstruction(
            0x5530, // ADD R5, R3 (destination R3 should NOT use constant generator)
            RegisterName.R5,
            RegisterName.R3,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert - R3 should be treated as a normal register destination
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R3)); // 0x1000 + 0x0234 = 0x1234
        Assert.Equal(1u, cycles);
    }
}
