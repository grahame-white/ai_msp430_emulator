using System;
using System.Collections.Generic;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Logic;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.Logic;

/// <summary>
/// Unit tests for the BisInstruction class.
/// </summary>
public class BisInstructionTests
{

    [Theory]
    [InlineData(0xD123)]
    [InlineData(0xD456)]
    [InlineData(0xD789)]
    public void Constructor_ValidParameters_SetsFormat(ushort instructionWord)
    {
        var instruction = new BisInstruction(instructionWord, RegisterName.R1, RegisterName.R2, AddressingMode.Register, AddressingMode.Register, false);
        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
    }

    [Theory]
    [InlineData(0xD123)]
    [InlineData(0xD456)]
    [InlineData(0xD789)]
    public void Constructor_ValidParameters_SetsOpcode(ushort instructionWord)
    {
        var instruction = new BisInstruction(instructionWord, RegisterName.R1, RegisterName.R2, AddressingMode.Register, AddressingMode.Register, false);
        Assert.Equal((byte)0xD, instruction.Opcode);
    }

    [Theory]
    [InlineData(0xD123)]
    [InlineData(0xD456)]
    [InlineData(0xD789)]
    public void Constructor_ValidParameters_SetsInstructionWord(ushort instructionWord)
    {
        var instruction = new BisInstruction(instructionWord, RegisterName.R1, RegisterName.R2, AddressingMode.Register, AddressingMode.Register, false);
        Assert.Equal(instructionWord, instruction.InstructionWord);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_ValidParameters_SetsByteOperation(bool isByteOp)
    {
        var instruction = new BisInstruction(0xD123, RegisterName.R1, RegisterName.R2, AddressingMode.Register, AddressingMode.Register, isByteOp);
        Assert.Equal(isByteOp, instruction.IsByteOperation);
    }

    [Theory]
    [InlineData(false, "BIS")]
    [InlineData(true, "BIS.B")]
    public void Constructor_ByteOperationFlag_SetsMnemonic(bool isByteOperation, string expectedMnemonic)
    {
        var instruction = new BisInstruction(0xD563, RegisterName.R5, RegisterName.R6, AddressingMode.Register, AddressingMode.Register, isByteOperation);
        Assert.Equal(expectedMnemonic, instruction.Mnemonic);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Register, AddressingMode.Indirect, 0)]
    [InlineData(AddressingMode.Register, AddressingMode.IndirectAutoIncrement, 0)]
    [InlineData(AddressingMode.Register, AddressingMode.Absolute, 1)]
    [InlineData(AddressingMode.Register, AddressingMode.Symbolic, 1)]
    [InlineData(AddressingMode.Register, AddressingMode.Indexed, 1)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Absolute, 2)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Indexed, 2)]
    public void ExtensionWordCount_VariousAddressingModes_ReturnsCorrectCount(
        AddressingMode sourceMode,
        AddressingMode destMode,
        int expectedCount)
    {
        // Arrange
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            RegisterName.R2,
            sourceMode,
            destMode,
            false);

        // Act & Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    [Theory]
    [InlineData(AddressingMode.Register, "R1")]
    [InlineData(AddressingMode.Indexed, "X(R1)")]
    [InlineData(AddressingMode.Indirect, "@R1")]
    [InlineData(AddressingMode.IndirectAutoIncrement, "@R1+")]
    [InlineData(AddressingMode.Immediate, "#N")]
    [InlineData(AddressingMode.Absolute, "&ADDR")]
    [InlineData(AddressingMode.Symbolic, "ADDR")]
    public void ToString_VariousAddressingModes_FormatsCorrectly(
        AddressingMode mode,
        string expectedOperand)
    {
        // Arrange
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            RegisterName.R1,
            mode,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal($"BIS {expectedOperand}, R1", result);
    }

    [Fact]
    public void ToString_ByteOperation_IncludesByteModifier()
    {
        // Arrange
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R3,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("BIS.B R3, R4", result);
    }

    [Theory]
    [InlineData(RegisterName.R0)]
    [InlineData(RegisterName.R1)]
    [InlineData(RegisterName.R2)]
    [InlineData(RegisterName.CG1)]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R15)]
    public void SourceRegister_AllRegisters_ReturnsCorrectRegister(RegisterName register)
    {
        // Arrange & Act
        var instruction = new BisInstruction(
            0xD000,
            register,
            RegisterName.R1,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(register, instruction.SourceRegister);
    }

    [Theory]
    [InlineData(RegisterName.R0)]
    [InlineData(RegisterName.R1)]
    [InlineData(RegisterName.R2)]
    [InlineData(RegisterName.CG1)]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R15)]
    public void DestinationRegister_AllRegisters_ReturnsCorrectRegister(RegisterName register)
    {
        // Arrange & Act
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            register,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(register, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_SetsSourceAddressingMode(AddressingMode mode)
    {
        // Arrange & Act
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            RegisterName.R2,
            mode,
            mode,
            false);

        // Assert
        Assert.Equal(mode, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_SetsDestinationAddressingMode(AddressingMode mode)
    {
        // Arrange & Act
        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            RegisterName.R2,
            mode,
            mode,
            false);

        // Assert
        Assert.Equal(mode, instruction.DestinationAddressingMode);
    }

    #region Execute Method Tests

    [Theory]
    [InlineData(false)]
    public void Execute_RegisterToRegister_SetsZeroFlag(bool expectedZero)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x00F0);
        registerFile.WriteRegister(RegisterName.R5, 0x0F00);

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
    }

    [Theory]
    [InlineData(false)]
    public void Execute_RegisterToRegister_SetsNegativeFlag(bool expectedNegative)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x00F0);
        registerFile.WriteRegister(RegisterName.R5, 0x0F00);

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
    }

    [Theory]
    [InlineData(false)]
    public void Execute_RegisterToRegister_SetsCarryFlag(bool expectedCarry)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x00F0);
        registerFile.WriteRegister(RegisterName.R5, 0x0F00);

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedCarry, registerFile.StatusRegister.Carry);
    }

    [Theory]
    [InlineData(false)]
    public void Execute_RegisterToRegister_SetsOverflowFlag(bool expectedOverflow)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x00F0);
        registerFile.WriteRegister(RegisterName.R5, 0x0F00);

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedOverflow, registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_RegisterToRegister_PerformsBisOperation()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x00F0);
        registerFile.WriteRegister(RegisterName.R5, 0x0F00);

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert - BIS is OR operation: 0x00F0 | 0x0F00 = 0x0FF0
        Assert.Equal(0x0FF0, registerFile.ReadRegister(RegisterName.R5));
    }

    /// <summary>
    /// Tests all valid source/destination addressing mode combinations for BIS instruction.
    /// Based on MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3: "CPU"
    /// Testing all 42 valid combinations (7 source × 6 destination modes).
    /// </summary>
    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register)]
    [InlineData(AddressingMode.Register, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Register, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Register, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Register, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Register, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Immediate, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Indirect, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Register)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Indexed)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Absolute)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Indexed, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Register)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Absolute, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Register)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Symbolic)]
    public void Execute_AllAddressingModeCombinations_ReturnsPositiveCycles(AddressingMode sourceMode, AddressingMode destMode)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.WriteRegister(RegisterName.R4, 0x2000);
        registerFile.SetProgramCounter(0x8000);

        // Set up memory for addressing modes that access memory
        memory[0x1000] = 0x34; // For indirect modes
        memory[0x1001] = 0x12;
        memory[0x2000] = 0x78;
        memory[0x2001] = 0x56;
        memory[0x1010] = 0xBC; // For indexed modes
        memory[0x1011] = 0x9A;
        memory[0x3000] = 0xEF; // For absolute modes
        memory[0x3001] = 0xCD;

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            RegisterName.R4,
            sourceMode,
            destMode,
            false);

        // Set up extension words based on addressing modes
        List<ushort> extensionWords = [];
        if (sourceMode == AddressingMode.Immediate)
        {
            extensionWords.Add(0x0100);
        }

        if (sourceMode == AddressingMode.Indexed)
        {
            extensionWords.Add(0x0010);
        }

        if (sourceMode == AddressingMode.Absolute)
        {
            extensionWords.Add(0x3000);
        }

        if (sourceMode == AddressingMode.Symbolic)
        {
            extensionWords.Add(0x1000);
        }

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
        Assert.True(cycles > 0, $"Expected positive cycle count for {sourceMode} to {destMode}");
    }

    /// <summary>
    /// Tests all valid source/destination addressing mode combinations for BIS instruction.
    /// Based on MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3: "CPU"
    /// Testing all 42 valid combinations (7 source × 6 destination modes).
    /// </summary>
    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register)]
    [InlineData(AddressingMode.Register, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Register, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Register, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Register, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Register, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Immediate, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Indirect, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Indirect, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Register)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Indexed)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Absolute)]
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Indexed, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Register)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Absolute, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Register)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Indexed)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Symbolic)]
    public void Execute_AllAddressingModeCombinations_ReturnsReasonableCycles(AddressingMode sourceMode, AddressingMode destMode)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.WriteRegister(RegisterName.R4, 0x2000);
        registerFile.SetProgramCounter(0x8000);

        // Set up memory for addressing modes that access memory
        memory[0x1000] = 0x34; // For indirect modes
        memory[0x1001] = 0x12;
        memory[0x2000] = 0x78;
        memory[0x2001] = 0x56;
        memory[0x1010] = 0xBC; // For indexed modes
        memory[0x1011] = 0x9A;
        memory[0x3000] = 0xEF; // For absolute modes
        memory[0x3001] = 0xCD;

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R1,
            RegisterName.R4,
            sourceMode,
            destMode,
            false);

        // Set up extension words based on addressing modes
        List<ushort> extensionWords = [];
        if (sourceMode == AddressingMode.Immediate)
        {
            extensionWords.Add(0x0100);
        }

        if (sourceMode == AddressingMode.Indexed)
        {
            extensionWords.Add(0x0010);
        }

        if (sourceMode == AddressingMode.Absolute)
        {
            extensionWords.Add(0x3000);
        }

        if (sourceMode == AddressingMode.Symbolic)
        {
            extensionWords.Add(0x1000);
        }

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
        Assert.True(cycles <= 10, $"Cycle count {cycles} seems too high for {sourceMode} to {destMode}");
    }

    [Fact]
    public void Execute_RegisterToRegister_ReturnsCycles()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x00F0);
        registerFile.WriteRegister(RegisterName.R5, 0x0F00);

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(1u, cycles);
    }

    [Theory]
    [InlineData(false)]
    public void Execute_ByteOperation_SetsZeroFlag(bool expectedZero)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x120F);
        registerFile.WriteRegister(RegisterName.R5, 0x34F0);

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            true); // Byte operation

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
    }

    [Theory]
    [InlineData(true)]
    public void Execute_ByteOperation_SetsNegativeFlag(bool expectedNegative)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x120F);
        registerFile.WriteRegister(RegisterName.R5, 0x34F0);

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            true); // Byte operation

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_ByteOperation_PerformsBisOnBytes()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[1024];
        registerFile.WriteRegister(RegisterName.R4, 0x120F);
        registerFile.WriteRegister(RegisterName.R5, 0x34F0);

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Register,
            true); // Byte operation

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert - only low byte should be affected: (0x0F | 0xF0) | 0x3400 = 0x34FF
        Assert.Equal((ushort)((0x0F | 0xF0) | 0x3400), registerFile.ReadRegister(RegisterName.R5));
    }

    [Fact]
    public void Execute_ImmediateToRegister_Takes1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R5, 0x0FF0);

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R0, // PC for immediate
            RegisterName.R5,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0xFF00 }; // Immediate value

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(1u, cycles); // 1 base + 0 source (immediate) + 0 dest (register)
    }

    [Fact]
    public void Execute_RegisterToIndexed_Takes4Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x0100); // Base address

        // Set up memory at indexed location R5 + 0x10 = 0x0110
        memory[0x0110] = 0xF0;
        memory[0x0111] = 0x0F;

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Indexed,
            false);

        ushort[] extensionWords = { 0x0010 }; // Offset for indexed addressing

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(4u, cycles); // 1 base + 0 source (register) + 3 dest (indexed)
    }

    [Fact]
    public void Execute_IndexedToRegister_Takes4Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0100); // Base address
        registerFile.WriteRegister(RegisterName.R5, 0x0000);

        // Set up memory at indexed location R4 + 0x10 = 0x0110
        memory[0x0110] = 0x00;
        memory[0x0111] = 0xFF;

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Indexed,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = { 0x0010 }; // Offset for indexed addressing

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(4u, cycles); // 1 base + 3 source (indexed) + 0 dest (register)
    }

    [Fact]
    public void Execute_IndirectToRegister_Takes3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0100); // Points to memory address
        registerFile.WriteRegister(RegisterName.R5, 0x0000);

        // Set up memory at indirect location
        memory[0x0100] = 0x00;
        memory[0x0101] = 0xFF;

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Indirect,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = Array.Empty<ushort>();

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(3u, cycles); // 1 base + 2 source (indirect) + 0 dest (register)
    }

    [Fact]
    public void Execute_RegisterToIndirect_Takes3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0xFF00);
        registerFile.WriteRegister(RegisterName.R5, 0x0100); // Points to memory address

        // Set up memory at indirect location
        memory[0x0100] = 0xF0;
        memory[0x0101] = 0x0F;

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.Register,
            AddressingMode.Indirect,
            false);

        ushort[] extensionWords = Array.Empty<ushort>();

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(3u, cycles); // 1 base + 0 source (register) + 2 dest (indirect)
    }

    [Fact]
    public void Execute_AbsoluteToAbsolute_Takes7Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();

        // Set up memory at source absolute address
        memory[0x0300] = 0x00;
        memory[0x0301] = 0xFF;

        // Set up memory at destination absolute address
        memory[0x0400] = 0xF0;
        memory[0x0401] = 0x0F;

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R0, // Absolute source
            RegisterName.R1, // Absolute destination
            AddressingMode.Absolute,
            AddressingMode.Absolute,
            false);

        ushort[] extensionWords = { 0x0300, 0x0400 }; // Source address, destination address

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(7u, cycles); // 1 base + 3 source (absolute) + 3 dest (absolute)
    }

    [Fact]
    public void Execute_SymbolicToSymbolic_Takes7Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.SetProgramCounter(0x1000);

        // Set up memory at symbolic source location PC + 0x10 = 0x1010
        memory[0x1010] = 0x00;
        memory[0x1011] = 0xFF;

        // Set up memory at symbolic destination location PC + 0x20 = 0x1020
        memory[0x1020] = 0xF0;
        memory[0x1021] = 0x0F;

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R0, // PC for symbolic
            RegisterName.R0, // PC for symbolic
            AddressingMode.Symbolic,
            AddressingMode.Symbolic,
            false);

        ushort[] extensionWords = { 0x0010, 0x0020 }; // Source offset, destination offset

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(7u, cycles); // 1 base + 3 source (symbolic) + 3 dest (symbolic)
    }

    [Fact]
    public void Execute_IndirectAutoIncrementToRegister_Takes3Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0100); // Points to memory address
        registerFile.WriteRegister(RegisterName.R5, 0x0000);

        // Set up memory at indirect location
        memory[0x0100] = 0x00;
        memory[0x0101] = 0xFF;

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = Array.Empty<ushort>();

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(3u, cycles); // 1 base + 2 source (indirect auto-increment) + 0 dest (register)
    }

    [Fact]
    public void Execute_IndirectAutoIncrementToRegister_IncrementsSourceRegister()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R4, 0x0100); // Points to memory address
        registerFile.WriteRegister(RegisterName.R5, 0x0000);

        // Set up memory at indirect location
        memory[0x0100] = 0x00;
        memory[0x0101] = 0xFF;

        var instruction = new BisInstruction(
            0xD000,
            RegisterName.R4,
            RegisterName.R5,
            AddressingMode.IndirectAutoIncrement,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = Array.Empty<ushort>();

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x0102, registerFile.ReadRegister(RegisterName.R4)); // Should be incremented by 2 for word operation
    }

    #endregion
}
