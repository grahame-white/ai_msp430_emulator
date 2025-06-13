using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.DataMovement;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.DataMovement;

/// <summary>
/// Unit tests for the PushInstruction class.
/// Tests all addressing modes, byte/word operations, and stack management behavior.
/// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4: "CPUX"
/// </summary>
public class PushInstructionTests
{
    /// <summary>
    /// Creates a standard test environment with register file and memory.
    /// </summary>
    /// <param name="stackPointer">Initial stack pointer value (default: 0x1000)</param>
    /// <param name="r4Value">Initial value for R4 register (default: 0x1234)</param>
    /// <returns>Tuple containing register file and memory array</returns>
    private static (RegisterFile RegisterFile, byte[] Memory) CreateTestEnvironment(ushort stackPointer = 0x1000, ushort r4Value = 0x1234)
    {
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();

        registerFile.SetStackPointer(stackPointer);
        registerFile.WriteRegister(RegisterName.R4, r4Value);

        return (registerFile, memory);
    }

    /// <summary>
    /// Creates a standard PushInstruction for testing.
    /// </summary>
    /// <param name="sourceRegister">Source register (default: R4)</param>
    /// <param name="addressingMode">Addressing mode (default: Register)</param>
    /// <param name="isByteOperation">Whether this is a byte operation (default: false)</param>
    /// <returns>Configured PushInstruction</returns>
    private static PushInstruction CreateTestInstruction(
        RegisterName sourceRegister = RegisterName.R4,
        AddressingMode addressingMode = AddressingMode.Register,
        bool isByteOperation = false)
    {
        ushort instructionWord = isByteOperation ? (ushort)0x1244 : (ushort)0x1204;
        return new PushInstruction(instructionWord, sourceRegister, addressingMode, isByteOperation);
    }
    [Fact]
    public void Constructor_ValidParameters_SetsFormat()
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(InstructionFormat.FormatII, instruction.Format);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsOpcode()
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(0x12, instruction.Opcode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsInstructionWord()
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(0x1204, instruction.InstructionWord);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsMnemonic()
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal("PUSH", instruction.Mnemonic);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsMnemonic()
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1244,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Assert
        Assert.Equal("PUSH.B", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_ValidParameters_SetsByteOperation(bool expectedByteOp)
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            expectedByteOp);

        // Assert
        Assert.Equal(expectedByteOp, instruction.IsByteOperation);
    }

    [Theory]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R5)]
    [InlineData(RegisterName.R15)]
    public void Constructor_ValidParameters_SetsSourceRegister(RegisterName expectedSourceRegister)
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            expectedSourceRegister,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedSourceRegister, instruction.SourceRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    public void Constructor_ValidParameters_SetsSourceAddressingMode(AddressingMode expectedSourceAddressingMode)
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            expectedSourceAddressingMode,
            false);

        // Assert
        Assert.Equal(expectedSourceAddressingMode, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Indexed, 1)]
    [InlineData(AddressingMode.Indirect, 0)]
    [InlineData(AddressingMode.IndirectAutoIncrement, 0)]
    [InlineData(AddressingMode.Immediate, 1)]
    [InlineData(AddressingMode.Absolute, 1)]
    [InlineData(AddressingMode.Symbolic, 1)]
    public void ExtensionWordCount_ReturnsCorrectCount(AddressingMode addressingMode, int expectedCount)
    {
        // Arrange & Act
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            addressingMode,
            false);

        // Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    [Fact]
    public void Execute_RegisterMode_DecrementsStackPointer()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
        PushInstruction instruction = CreateTestInstruction();

        // Act
        _ = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer()); // SP decremented by 2
    }

    [Fact]
    public void Execute_RegisterMode_WritesLowByteToMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
        PushInstruction instruction = CreateTestInstruction();

        // Act
        _ = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x34, memory[0x0FFE]); // Low byte of value stored (little-endian)
    }

    [Fact]
    public void Execute_RegisterMode_WritesHighByteToMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
        PushInstruction instruction = CreateTestInstruction();

        // Act
        _ = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x12, memory[0x0FFF]); // High byte of value stored
    }

    [Fact]
    public void Execute_RegisterMode_ReturnsCorrectCycleCount()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
        PushInstruction instruction = CreateTestInstruction();

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(1u, cycles); // Register mode cycle count
    }

    [Fact]
    public void Execute_ByteOperation_DecrementsStackPointer()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(r4Value: 0x12FF); // Negative byte value
        PushInstruction instruction = CreateTestInstruction(isByteOperation: true);

        // Act
        _ = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());
    }

    [Fact]
    public void Execute_ByteOperation_WritesSignExtendedLowByte()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(r4Value: 0x12FF); // Negative byte value
        PushInstruction instruction = CreateTestInstruction(isByteOperation: true);

        // Act
        _ = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0xFF, memory[0x0FFE]); // Low byte
    }

    [Fact]
    public void Execute_ByteOperation_WritesSignExtendedHighByte()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(r4Value: 0x12FF); // Negative byte value
        PushInstruction instruction = CreateTestInstruction(isByteOperation: true);

        // Act
        _ = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0xFF, memory[0x0FFF]); // High byte (sign-extended)
    }

    [Fact]
    public void Execute_ByteOperationPositive_DecrementsStackPointer()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(); // Default 0x1234 is positive byte value
        PushInstruction instruction = CreateTestInstruction(isByteOperation: true);

        // Act
        _ = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());
    }

    [Fact]
    public void Execute_ByteOperationPositive_WritesLowByteToMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment(); // Default 0x1234 is positive byte value
        PushInstruction instruction = CreateTestInstruction(isByteOperation: true);

        // Act
        _ = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x34, memory[0x0FFE]); // Low byte
    }

    [Fact]
    public void Execute_ByteOperationPositive_ZeroExtendsHighByte()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new PushInstruction(0x1244, RegisterName.R4, AddressingMode.Register, true);

        registerFile.WriteRegister(RegisterName.R4, 0x1234); // Positive byte value
        registerFile.SetStackPointer(0x1000);

        // Act
        _ = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x00, memory[0x0FFF]); // High byte (positive, so no sign extension)
    }

    [Fact]
    public void Execute_ImmediateMode_DecrementsStackPointer()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new PushInstruction(0x1230, RegisterName.R0, AddressingMode.Immediate, false);

        registerFile.SetStackPointer(0x1000);
        ushort[] extensionWords = new ushort[] { 0x5678 };

        // Act
        _ = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());
    }

    [Fact]
    public void Execute_ImmediateMode_WritesLowByteToMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new PushInstruction(0x1230, RegisterName.R0, AddressingMode.Immediate, false);

        registerFile.SetStackPointer(0x1000);
        ushort[] extensionWords = new ushort[] { 0x5678 };

        // Act
        _ = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x78, memory[0x0FFE]); // Low byte of immediate value
    }

    [Fact]
    public void Execute_ImmediateMode_WritesHighByteToMemory()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new PushInstruction(0x1230, RegisterName.R0, AddressingMode.Immediate, false);

        registerFile.SetStackPointer(0x1000);
        ushort[] extensionWords = new ushort[] { 0x5678 };

        // Act
        _ = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x56, memory[0x0FFF]); // High byte of immediate value
    }

    [Fact]
    public void Execute_ImmediateMode_ReturnsCorrectCycleCount()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new PushInstruction(0x1230, RegisterName.R0, AddressingMode.Immediate, false);

        registerFile.SetStackPointer(0x1000);
        ushort[] extensionWords = new ushort[] { 0x5678 };

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(1u, cycles); // Immediate mode cycle count
    }

    [Fact]
    public void Execute_IndirectMode_PushesIndirectValue()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new PushInstruction(0x1284, RegisterName.R4, AddressingMode.Indirect, false);

        registerFile.WriteRegister(RegisterName.R4, 0x2000); // Address to read from
        registerFile.SetStackPointer(0x1000);

        // Setup memory at address 0x2000
        memory[0x2000] = 0xAB; // Low byte
        memory[0x2001] = 0xCD; // High byte

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());
        Assert.Equal(0xAB, memory[0x0FFE]); // Low byte of indirect value
        Assert.Equal(0xCD, memory[0x0FFF]); // High byte of indirect value
        Assert.Equal(3u, cycles); // Indirect mode cycle count
    }

    [Fact]
    public void Execute_IndirectAutoIncrementMode_PushesValueAndIncrementsRegister()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new PushInstruction(0x12C4, RegisterName.R4, AddressingMode.IndirectAutoIncrement, false);

        registerFile.WriteRegister(RegisterName.R4, 0x2000); // Address to read from
        registerFile.SetStackPointer(0x1000);

        // Setup memory at address 0x2000
        memory[0x2000] = 0xEF; // Low byte
        memory[0x2001] = 0x12; // High byte

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());
        Assert.Equal(0xEF, memory[0x0FFE]); // Low byte of value
        Assert.Equal(0x12, memory[0x0FFF]); // High byte of value
        Assert.Equal(0x2002, registerFile.ReadRegister(RegisterName.R4)); // Register incremented by 2
        Assert.Equal(3u, cycles); // Indirect auto-increment mode cycle count
    }

    [Fact]
    public void Execute_StackOverflow_ThrowsException()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new PushInstruction(0x1204, RegisterName.R4, AddressingMode.Register, false);

        registerFile.WriteRegister(RegisterName.R4, 0x1234);
        registerFile.SetStackPointer(0x0001); // SP that would underflow

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
        Assert.Contains("Stack overflow", exception.Message);
    }

    [Fact]
    public void Execute_StackMemoryOutOfBounds_ThrowsException()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        var instruction = new PushInstruction(0x1204, RegisterName.R4, AddressingMode.Register, false);

        registerFile.WriteRegister(RegisterName.R4, 0x1234);
        registerFile.SetStackPointer(0x0002); // SP that would decrement to 0x0000, accessing memory beyond stack bounds

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
        Assert.Contains("memory beyond bounds", exception.Message);
    }

    [Fact]
    public void ToString_WordOperation_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("PUSH R4", result);
    }

    [Fact]
    public void ToString_ByteOperation_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new PushInstruction(
            0x1244,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("PUSH.B R4", result);
    }

    /// <summary>
    /// Tests all valid addressing modes for PUSH instruction (single operand).
    /// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4: "CPUX"
    /// Testing all 7 valid addressing modes for single-operand source instructions.
    /// </summary>
    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    [InlineData(AddressingMode.Immediate)]
    public void Execute_AllSourceAddressingModes_ExecutesSuccessfully(AddressingMode sourceMode)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();

        // Set up initial values
        registerFile.SetStackPointer(0x1000);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        // Set up memory for addressing modes that need it
        memory[0x0200] = 0x56;
        memory[0x0201] = 0x78;

        var instruction = new PushInstruction(
            0x1204,
            RegisterName.R4,
            sourceMode,
            false);

        ushort[] extensionWords = sourceMode switch
        {
            AddressingMode.Indexed => [0x0100],
            AddressingMode.Absolute => [0x0200],
            AddressingMode.Symbolic => [0x0200],
            AddressingMode.Immediate => [0xABCD],
            _ => []
        };

        // Act & Assert - Should not throw
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Verify stack pointer was decremented
        Assert.Equal(0x0FFE, registerFile.GetStackPointer());

        // Verify cycle count is reasonable
        Assert.True(cycles > 0 && cycles <= 6);
    }

    [Fact]
    public void ToString_IndirectMode_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new PushInstruction(
            0x1284,
            RegisterName.R5,
            AddressingMode.Indirect,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("PUSH @R5", result);
    }
}
