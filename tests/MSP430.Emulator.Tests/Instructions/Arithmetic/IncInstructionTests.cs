using System;
using System.Collections.Generic;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.Arithmetic;

/// <summary>
/// Unit tests for the IncInstruction class.
/// </summary>
public class IncInstructionTests
{
    [Theory]
    [InlineData(InstructionFormat.FormatI)]
    public void Constructor_ValidParameters_SetsFormat(InstructionFormat expectedFormat)
    {
        var instruction = new IncInstruction(0xA123, RegisterName.R4, AddressingMode.Register, false);

        Assert.Equal(expectedFormat, instruction.Format);
    }

    [Theory]
    [InlineData(0xA)]
    public void Constructor_ValidParameters_SetsOpcode(byte expectedOpcode)
    {
        var instruction = new IncInstruction(0xA123, RegisterName.R4, AddressingMode.Register, false);

        Assert.Equal(expectedOpcode, instruction.Opcode);
    }

    [Theory]
    [InlineData(0xA123, 0xA123)]
    [InlineData(0xA456, 0xA456)]
    public void Constructor_ValidParameters_SetsInstructionWord(ushort instructionWord, ushort expected)
    {
        var instruction = new IncInstruction(instructionWord, RegisterName.R4, AddressingMode.Register, false);

        Assert.Equal(expected, instruction.InstructionWord);
    }

    [Theory]
    [InlineData(RegisterName.R2, RegisterName.R2)]
    [InlineData(RegisterName.R3, RegisterName.R3)]
    public void Constructor_ValidParameters_SetsDestinationRegister(RegisterName destReg, RegisterName expected)
    {
        var instruction = new IncInstruction(0xA123, destReg, AddressingMode.Register, false);

        Assert.Equal(expected, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indexed)]
    public void Constructor_ValidParameters_SetsDestinationAddressingMode(AddressingMode destMode, AddressingMode expected)
    {
        var instruction = new IncInstruction(0xA123, RegisterName.R4, destMode, false);

        Assert.Equal(expected, instruction.DestinationAddressingMode);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void Constructor_ValidParameters_SetsIsByteOperation(bool isByteOp, bool expected)
    {
        var instruction = new IncInstruction(0xA123, RegisterName.R4, AddressingMode.Register, isByteOp);

        Assert.Equal(expected, instruction.IsByteOperation);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsSourceRegisterToNull()
    {
        var instruction = new IncInstruction(0xA123, RegisterName.R4, AddressingMode.Register, false);

        Assert.Null(instruction.SourceRegister);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsSourceAddressingModeToNull()
    {
        var instruction = new IncInstruction(0xA123, RegisterName.R4, AddressingMode.Register, false);

        Assert.Null(instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(false, "INC")]
    [InlineData(true, "INC.B")]
    public void Constructor_ByteOperationFlag_SetsMnemonic(bool isByteOperation, string expectedMnemonic)
    {
        var instruction = new IncInstruction(0xA563, RegisterName.R5, AddressingMode.Register, isByteOperation);
        Assert.Equal(expectedMnemonic, instruction.Mnemonic);
    }

    [Theory]
    [InlineData(AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Indirect, 0)]
    [InlineData(AddressingMode.IndirectAutoIncrement, 0)]
    [InlineData(AddressingMode.Absolute, 1)]
    [InlineData(AddressingMode.Symbolic, 1)]
    [InlineData(AddressingMode.Indexed, 1)]
    public void ExtensionWordCount_VariousAddressingModes_ReturnsCorrectCount(
        AddressingMode destMode,
        int expectedCount)
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA000,
            RegisterName.R4,
            destMode,
            false);

        // Act & Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    [Theory]
    [InlineData(RegisterName.R0, AddressingMode.Register, "R0")]
    [InlineData(RegisterName.R5, AddressingMode.Indexed, "X(R5)")]
    [InlineData(RegisterName.R3, AddressingMode.Indirect, "@R3")]
    [InlineData(RegisterName.R4, AddressingMode.IndirectAutoIncrement, "@R4+")]
    [InlineData(RegisterName.R4, AddressingMode.Absolute, "&ADDR")]
    [InlineData(RegisterName.R0, AddressingMode.Symbolic, "ADDR")]
    public void ToString_VariousAddressingModes_FormatsCorrectly(
        RegisterName register,
        AddressingMode mode,
        string expectedOperand)
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA000,
            register,
            mode,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal($"INC {expectedOperand}", result);
    }

    [Fact]
    public void ToString_ByteOperation_IncludesBSuffix()
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA563,
            RegisterName.R5,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("INC.B R5", result);
    }

    [Theory]
    [InlineData(RegisterName.R0)]
    [InlineData(RegisterName.R7)]
    [InlineData(RegisterName.R3)]
    [InlineData(RegisterName.R5)]
    public void Properties_VariousRegisters_ReturnCorrectDestinationRegister(RegisterName dest)
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA000,
            dest,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(dest, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(RegisterName.R0)]
    [InlineData(RegisterName.R7)]
    [InlineData(RegisterName.R3)]
    [InlineData(RegisterName.R5)]
    public void Properties_VariousRegisters_SourceRegisterIsNull(RegisterName dest)
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA000,
            dest,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Null(instruction.SourceRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_ReturnCorrectDestinationAddressingMode(AddressingMode mode)
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA000,
            RegisterName.R4,
            mode,
            false);

        // Act & Assert
        Assert.Equal(mode, instruction.DestinationAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_SourceAddressingModeIsNull(AddressingMode mode)
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA000,
            RegisterName.R4,
            mode,
            false);

        // Act & Assert
        Assert.Null(instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R8)]
    [InlineData(RegisterName.R6)]
    public void Properties_DifferentRegisters_ReturnCorrectDestination(RegisterName expected)
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA000,
            expected,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(expected, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(RegisterName.R4, RegisterName.R5)]
    [InlineData(RegisterName.R8, RegisterName.R3)]
    [InlineData(RegisterName.R6, RegisterName.R7)]
    public void Properties_DifferentRegisters_DoesNotReturnOtherRegister(RegisterName expected, RegisterName other)
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA000,
            expected,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.NotEqual(other, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(0xA)]
    public void Opcode_IsCorrectValue(byte expectedOpcode)
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA000,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(expectedOpcode, instruction.Opcode);
    }

    [Theory]
    [InlineData(InstructionFormat.FormatI)]
    public void Format_IsFormatI(InstructionFormat expectedFormat)
    {
        // Arrange
        var instruction = new IncInstruction(
            0xA000,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(expectedFormat, instruction.Format);
    }

    [Fact]
    public void Execute_RegisterMode_IncrementsValue()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1234);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x1235, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_RegisterMode_ClearsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1234);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_RegisterMode_ClearsNegativeFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1234);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_RegisterMode_ClearsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1234);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_RegisterMode_ClearsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1234);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    /// <summary>
    /// Tests all valid addressing modes for INC instruction (single operand).
    /// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4: "CPUX"
    /// Testing all 6 valid addressing modes for single-operand instructions (excluding Immediate for destination).
    /// </summary>
    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void Execute_AllAddressingModes_ExecutesSuccessfully(AddressingMode destMode)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.SetProgramCounter(0x8000);

        // Set up memory for addressing modes that access memory
        memory[0x1000] = 0x34; // For indirect modes
        memory[0x1001] = 0x12;
        memory[0x1010] = 0xBC; // For indexed modes
        memory[0x1011] = 0x9A;
        memory[0x3000] = 0xEF; // For absolute modes
        memory[0x3001] = 0xCD;

        var instruction = new IncInstruction(
            0xA000,
            RegisterName.R1,
            destMode,
            false);

        // Set up extension words based on addressing mode
        List<ushort> extensionWords = [];
        if (destMode == AddressingMode.Indexed)
        {
            extensionWords.Add(0x0010);
        }

        if (destMode == AddressingMode.Absolute)
        {
            extensionWords.Add(0x3000);
        }

        if (destMode == AddressingMode.Symbolic)
        {
            extensionWords.Add(0x1000);
        }

        // Act - instruction should execute without throwing exceptions
        uint cycles = instruction.Execute(registerFile, memory, extensionWords.ToArray());

        // Assert - verify instruction executed and returned positive cycle count
        Assert.True(cycles > 0, $"Expected positive cycle count for {destMode}");
    }

    /// <summary>
    /// Tests that cycle count for all INC instruction addressing modes is reasonable.
    /// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.5: "MSP430 and MSP430X Instructions"
    /// </summary>
    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void Execute_AllAddressingModes_ReasonableCycleCount(AddressingMode destMode)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.SetProgramCounter(0x8000);

        // Set up memory for addressing modes that access memory
        memory[0x1000] = 0x34; // For indirect modes
        memory[0x1001] = 0x12;
        memory[0x1010] = 0xBC; // For indexed modes
        memory[0x1011] = 0x9A;
        memory[0x3000] = 0xEF; // For absolute modes
        memory[0x3001] = 0xCD;

        var instruction = new IncInstruction(
            0xA000,
            RegisterName.R1,
            destMode,
            false);

        // Set up extension words based on addressing mode
        List<ushort> extensionWords = [];
        if (destMode == AddressingMode.Indexed)
        {
            extensionWords.Add(0x0010);
        }

        if (destMode == AddressingMode.Absolute)
        {
            extensionWords.Add(0x3000);
        }

        if (destMode == AddressingMode.Symbolic)
        {
            extensionWords.Add(0x1000);
        }

        // Act - instruction should execute without throwing exceptions
        uint cycles = instruction.Execute(registerFile, memory, extensionWords.ToArray());

        // Assert - verify cycle count is reasonable (not too high)
        Assert.True(cycles <= 7, $"Cycle count {cycles} seems too high for {destMode}");
    }

    /// <summary>
    /// Tests cycle counts for INC instruction addressing modes.
    /// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.5: "MSP430 and MSP430X Instructions"
    /// Cycle counts per TI specification: base (1) + destination cycles.
    /// Note: Immediate mode excluded as destination (cannot write to immediate values).
    /// </summary>
    [Theory]
    [InlineData(AddressingMode.Register, 1u)]
    [InlineData(AddressingMode.Indirect, 3u)]
    [InlineData(AddressingMode.IndirectAutoIncrement, 3u)]
    [InlineData(AddressingMode.Indexed, 4u)]
    [InlineData(AddressingMode.Absolute, 4u)]
    [InlineData(AddressingMode.Symbolic, 4u)]
    public void Execute_CycleCounts_AreCorrect(AddressingMode destMode, uint expectedCycles)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.SetProgramCounter(0x8000);

        // Set up memory for addressing modes that access memory
        memory[0x1000] = 0x34; // For indirect modes
        memory[0x1001] = 0x12;
        memory[0x1010] = 0xBC; // For indexed modes
        memory[0x1011] = 0x9A;
        memory[0x3000] = 0xEF; // For absolute modes
        memory[0x3001] = 0xCD;

        var instruction = new IncInstruction(
            0xA000,
            RegisterName.R1,
            destMode,
            false);

        // Set up extension words based on addressing mode
        List<ushort> extensionWords = [];
        if (destMode == AddressingMode.Indexed)
        {
            extensionWords.Add(0x0010);
        }

        if (destMode == AddressingMode.Absolute)
        {
            extensionWords.Add(0x3000);
        }

        if (destMode == AddressingMode.Symbolic)
        {
            extensionWords.Add(0x1000);
        }

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords.ToArray());

        // Assert
        Assert.Equal(expectedCycles, cycles);
    }

    [Fact]
    public void Execute_RegisterMode_Returns1Cycle()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0x1234);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_IncrementsLowByteOnly()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB34);

        var instruction = new IncInstruction(
            0xA543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0xAB35, registerFile.ReadRegister(RegisterName.R3));
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_ClearsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB34);

        var instruction = new IncInstruction(
            0xA543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_ClearsNegativeFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB34);

        var instruction = new IncInstruction(
            0xA543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_ClearsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB34);

        var instruction = new IncInstruction(
            0xA543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_ClearsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB34);

        var instruction = new IncInstruction(
            0xA543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_RegisterMode_ByteOperation_Returns1Cycle()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R3, 0xAB34);

        var instruction = new IncInstruction(
            0xA543,
            RegisterName.R3,
            AddressingMode.Register,
            true);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_ZeroValue_IncrementsToZero()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x0000, registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_ZeroValue_SetsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_ZeroValue_ClearsNegativeFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_ZeroValue_SetsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_ZeroValue_ClearsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R5, 0xFFFF);

        var instruction = new IncInstruction(
            0xA505,
            RegisterName.R5,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_ByteOverflow_IncrementsToZero()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x12FF);

        var instruction = new IncInstruction(
            0xA544,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x1200, registerFile.ReadRegister(RegisterName.R4));
    }

    [Fact]
    public void Execute_ByteOverflow_SetsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x12FF);

        var instruction = new IncInstruction(
            0xA544,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_ByteOverflow_ClearsNegativeFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x12FF);

        var instruction = new IncInstruction(
            0xA544,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_ByteOverflow_SetsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x12FF);

        var instruction = new IncInstruction(
            0xA544,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_ByteOverflow_ClearsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x12FF);

        var instruction = new IncInstruction(
            0xA544,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_PositiveOverflow_IncrementsTo8000()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Max positive value

        var instruction = new IncInstruction(
            0xA504,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x8000, registerFile.ReadRegister(RegisterName.R4));
    }

    [Fact]
    public void Execute_PositiveOverflow_ClearsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Max positive value

        var instruction = new IncInstruction(
            0xA504,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_PositiveOverflow_SetsNegativeFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Max positive value

        var instruction = new IncInstruction(
            0xA504,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_PositiveOverflow_ClearsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Max positive value

        var instruction = new IncInstruction(
            0xA504,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_PositiveOverflow_SetsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Max positive value

        var instruction = new IncInstruction(
            0xA504,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_BytePositiveOverflow_IncrementsTo1280()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R6, 0x127F); // Max positive byte value

        var instruction = new IncInstruction(
            0xA546,
            RegisterName.R6,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x1280, registerFile.ReadRegister(RegisterName.R6));
    }

    [Fact]
    public void Execute_BytePositiveOverflow_ClearsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R6, 0x127F); // Max positive byte value

        var instruction = new IncInstruction(
            0xA546,
            RegisterName.R6,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_BytePositiveOverflow_SetsNegativeFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R6, 0x127F); // Max positive byte value

        var instruction = new IncInstruction(
            0xA546,
            RegisterName.R6,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_BytePositiveOverflow_ClearsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R6, 0x127F); // Max positive byte value

        var instruction = new IncInstruction(
            0xA546,
            RegisterName.R6,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_BytePositiveOverflow_SetsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R6, 0x127F); // Max positive byte value

        var instruction = new IncInstruction(
            0xA546,
            RegisterName.R6,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_AbsoluteMode_IncrementsMemoryValueLowByte()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = new ushort[] { 0x0200 }; // Absolute address

        // Set up memory with initial value
        memory[0x0200] = 0x34;
        memory[0x0201] = 0x12; // 0x1234 in little-endian

        var instruction = new IncInstruction(
            0xA582,
            RegisterName.R2, // Register not used in absolute mode
            AddressingMode.Absolute,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x35, memory[0x0200]);
    }

    [Fact]
    public void Execute_AbsoluteMode_MemoryValueHighByteUnchanged()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = new ushort[] { 0x0200 }; // Absolute address

        // Set up memory with initial value
        memory[0x0200] = 0x34;
        memory[0x0201] = 0x12; // 0x1234 in little-endian

        var instruction = new IncInstruction(
            0xA582,
            RegisterName.R2, // Register not used in absolute mode
            AddressingMode.Absolute,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x12, memory[0x0201]); // Should be 0x1235 in little-endian
    }

    [Fact]
    public void Execute_AbsoluteMode_ClearsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = new ushort[] { 0x0200 }; // Absolute address

        // Set up memory with initial value
        memory[0x0200] = 0x34;
        memory[0x0201] = 0x12; // 0x1234 in little-endian

        var instruction = new IncInstruction(
            0xA582,
            RegisterName.R2, // Register not used in absolute mode
            AddressingMode.Absolute,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_AbsoluteMode_ClearsNegativeFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = new ushort[] { 0x0200 }; // Absolute address

        // Set up memory with initial value
        memory[0x0200] = 0x34;
        memory[0x0201] = 0x12; // 0x1234 in little-endian

        var instruction = new IncInstruction(
            0xA582,
            RegisterName.R2, // Register not used in absolute mode
            AddressingMode.Absolute,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_AbsoluteMode_ClearsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = new ushort[] { 0x0200 }; // Absolute address

        // Set up memory with initial value
        memory[0x0200] = 0x34;
        memory[0x0201] = 0x12; // 0x1234 in little-endian

        var instruction = new IncInstruction(
            0xA582,
            RegisterName.R2, // Register not used in absolute mode
            AddressingMode.Absolute,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_AbsoluteMode_ClearsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = new ushort[] { 0x0200 }; // Absolute address

        // Set up memory with initial value
        memory[0x0200] = 0x34;
        memory[0x0201] = 0x12; // 0x1234 in little-endian

        var instruction = new IncInstruction(
            0xA582,
            RegisterName.R2, // Register not used in absolute mode
            AddressingMode.Absolute,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_AbsoluteMode_Returns4Cycles()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = new ushort[] { 0x0200 }; // Absolute address

        // Set up memory with initial value
        memory[0x0200] = 0x34;
        memory[0x0201] = 0x12; // 0x1234 in little-endian

        var instruction = new IncInstruction(
            0xA582,
            RegisterName.R2, // Register not used in absolute mode
            AddressingMode.Absolute,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(4u, cycles);
    }

    [Fact]
    public void Execute_IndirectMode_IncrementsMemoryValueLowByte()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R7, 0x0300); // Address in R7
        memory[0x0300] = 0x99;
        memory[0x0301] = 0x88; // 0x8899 in little-endian

        var instruction = new IncInstruction(
            0xA527,
            RegisterName.R7,
            AddressingMode.Indirect,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x9A, memory[0x0300]);
    }

    [Fact]
    public void Execute_IndirectMode_MemoryValueHighByteUnchanged()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R7, 0x0300); // Address in R7
        memory[0x0300] = 0x99;
        memory[0x0301] = 0x88; // 0x8899 in little-endian

        var instruction = new IncInstruction(
            0xA527,
            RegisterName.R7,
            AddressingMode.Indirect,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x88, memory[0x0301]); // Should be 0x889A in little-endian
    }

    [Fact]
    public void Execute_IndirectMode_RegisterUnchanged()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R7, 0x0300); // Address in R7
        memory[0x0300] = 0x99;
        memory[0x0301] = 0x88; // 0x8899 in little-endian

        var instruction = new IncInstruction(
            0xA527,
            RegisterName.R7,
            AddressingMode.Indirect,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal((ushort)0x0300, registerFile.ReadRegister(RegisterName.R7)); // R7 unchanged
    }

    [Fact]
    public void Execute_IndirectMode_ClearsZeroFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R7, 0x0300); // Address in R7
        memory[0x0300] = 0x99;
        memory[0x0301] = 0x88; // 0x8899 in little-endian

        var instruction = new IncInstruction(
            0xA527,
            RegisterName.R7,
            AddressingMode.Indirect,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_IndirectMode_SetsNegativeFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R7, 0x0300); // Address in R7
        memory[0x0300] = 0x99;
        memory[0x0301] = 0x88; // 0x8899 in little-endian

        var instruction = new IncInstruction(
            0xA527,
            RegisterName.R7,
            AddressingMode.Indirect,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_IndirectMode_ClearsCarryFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R7, 0x0300); // Address in R7
        memory[0x0300] = 0x99;
        memory[0x0301] = 0x88; // 0x8899 in little-endian

        var instruction = new IncInstruction(
            0xA527,
            RegisterName.R7,
            AddressingMode.Indirect,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_IndirectMode_ClearsOverflowFlag()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R7, 0x0300); // Address in R7
        memory[0x0300] = 0x99;
        memory[0x0301] = 0x88; // 0x8899 in little-endian

        var instruction = new IncInstruction(
            0xA527,
            RegisterName.R7,
            AddressingMode.Indirect,
            false);

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_IndirectMode_Returns3Cycles()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[65536];
        ushort[] extensionWords = Array.Empty<ushort>();

        registerFile.WriteRegister(RegisterName.R7, 0x0300); // Address in R7
        memory[0x0300] = 0x99;
        memory[0x0301] = 0x88; // 0x8899 in little-endian

        var instruction = new IncInstruction(
            0xA527,
            RegisterName.R7,
            AddressingMode.Indirect,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(3u, cycles);
    }
}
