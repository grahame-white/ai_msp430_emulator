using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.Arithmetic;

/// <summary>
/// Unit tests for the CmpInstruction class.
/// 
/// CMP instruction performs comparison (subtraction without storing result):
/// - Two-operand instruction with source and destination operands
/// - Sets status flags (Zero, Negative, Carry, Overflow) based on comparison result
/// - Supports all addressing modes for both source and destination
/// - Available in both word (CMP) and byte (CMP.B) variants
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - Section 4.3.15: CMP instruction
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.4: CPUX instruction extensions
/// </summary>
public class CmpInstructionTests
{


    [Theory]
    [InlineData(false, "CMP")]
    [InlineData(true, "CMP.B")]
    public void Constructor_ByteOperationFlag_SetsMnemonic(bool isByteOperation, string expectedMnemonic)
    {
        var instruction = new CmpInstruction(0x9563, RegisterName.R5, RegisterName.R6, AddressingMode.Register, AddressingMode.Register, isByteOperation);
        Assert.Equal(expectedMnemonic, instruction.Mnemonic);
    }

    [Theory]
    [InlineData(RegisterName.R1)]
    [InlineData(RegisterName.R5)]
    [InlineData(RegisterName.R12)]
    public void Constructor_ValidParameters_SetsSourceRegister(RegisterName expectedRegister)
    {
        // Arrange & Act
        var instruction = new CmpInstruction(
            0x9123,
            expectedRegister,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedRegister, instruction.SourceRegister);
    }

    [Theory]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R7)]
    [InlineData(RegisterName.R15)]
    public void Constructor_ValidParameters_SetsDestinationRegister(RegisterName expectedRegister)
    {
        // Arrange & Act
        var instruction = new CmpInstruction(
            0x9123,
            RegisterName.R1,
            expectedRegister,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedRegister, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    public void Constructor_ValidParameters_SetsSourceAddressingMode(AddressingMode expectedMode)
    {
        // Arrange & Act
        var instruction = new CmpInstruction(
            0x9123,
            RegisterName.R1,
            RegisterName.R4,
            expectedMode,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedMode, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    public void Constructor_ValidParameters_SetsDestinationAddressingMode(AddressingMode expectedMode)
    {
        // Arrange & Act
        var instruction = new CmpInstruction(
            0x9123,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            expectedMode,
            false);

        // Assert
        Assert.Equal(expectedMode, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsByteFlag()
    {
        // Arrange & Act
        var instruction = new CmpInstruction(
            0x9563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Assert
        Assert.True(instruction.IsByteOperation);
    }

    [Theory]
    [InlineData("CMP.B")]
    public void Constructor_ByteOperation_SetsByteMnemonic(string expectedMnemonic)
    {
        // Arrange & Act
        var instruction = new CmpInstruction(
            0x9563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Assert
        Assert.Equal(expectedMnemonic, instruction.Mnemonic);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Register, 1)]
    [InlineData(AddressingMode.Indexed, AddressingMode.Register, 1)]
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
        var instruction = new CmpInstruction(
            0x9000,
            RegisterName.R1,
            RegisterName.R4,
            sourceMode,
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
    [InlineData(RegisterName.R0, AddressingMode.Immediate, "#N")]
    [InlineData(RegisterName.R4, AddressingMode.Absolute, "&ADDR")]
    [InlineData(RegisterName.R0, AddressingMode.Symbolic, "ADDR")]
    public void ToString_VariousAddressingModes_FormatsCorrectly(
        RegisterName register,
        AddressingMode mode,
        string expectedOperand)
    {
        // Arrange
        var instruction = new CmpInstruction(
            0x9000,
            register,
            RegisterName.R1,
            mode,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal($"CMP {expectedOperand}, R1", result);
    }

    [Fact]
    public void ToString_ByteOperation_IncludesBSuffix()
    {
        // Arrange
        var instruction = new CmpInstruction(
            0x9563,
            RegisterName.R5,
            RegisterName.R6,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("CMP.B R5, R6", result);
    }

    [Theory]
    [InlineData(RegisterName.R0, RegisterName.R1)]
    [InlineData(RegisterName.R15, RegisterName.R4)]
    [InlineData(RegisterName.R3, RegisterName.R4)]
    [InlineData(RegisterName.R5, RegisterName.R6)]
    public void Properties_VariousRegisters_ReturnsCorrectSourceRegister(RegisterName source, RegisterName dest)
    {
        // Arrange
        var instruction = new CmpInstruction(
            0x9000,
            source,
            dest,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(source, instruction.SourceRegister);
    }

    [Theory]
    [InlineData(RegisterName.R1, RegisterName.R4)]
    [InlineData(RegisterName.R5, RegisterName.R6)]
    [InlineData(RegisterName.R12, RegisterName.R15)]
    public void Properties_VariousRegisters_ReturnsCorrectDestinationRegister(RegisterName source, RegisterName dest)
    {
        // Arrange
        var instruction = new CmpInstruction(
            0x9000,
            source,
            dest,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(dest, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_ReturnsCorrectSourceMode(AddressingMode mode)
    {
        // Arrange
        var instruction = new CmpInstruction(
            0x9000,
            RegisterName.R1,
            RegisterName.R4,
            mode,
            AddressingMode.Register,
            false);

        // Act & Assert
        Assert.Equal(mode, instruction.SourceAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void AddressingModes_AllSupportedModes_ReturnsCorrectDestinationMode(AddressingMode mode)
    {
        // Arrange
        var instruction = new CmpInstruction(
            0x9000,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            mode,
            false);

        // Act & Assert
        Assert.Equal(mode, instruction.DestinationAddressingMode);
    }

    [Fact]
    public void Execute_EqualValues_DestinationUnchanged()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234); // Source
        registerFile.WriteRegister(RegisterName.R4, 0x1234); // Destination

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R4)); // Destination unchanged
    }

    [Fact]
    public void Execute_EqualValues_Takes1Cycle()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_EqualValues_SetsZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void Execute_EqualValues_ClearsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_EqualValues_ClearsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_EqualValues_ClearsOverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.False(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_SourceGreaterThanDestination_DestinationUnchanged()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x5678); // Source (larger)
        registerFile.WriteRegister(RegisterName.R4, 0x1234); // Destination (smaller)

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R4)); // Destination unchanged
    }

    [Fact]
    public void Execute_SourceGreaterThanDestination_SetsCarryFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x5678);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_SourceGreaterThanDestination_SetsNegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x5678);
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_DestinationNotModified_DestinationValue()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.WriteRegister(RegisterName.R4, 0x2000);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x2000, registerFile.ReadRegister(RegisterName.R4)); // Destination completely unchanged
    }

    [Theory]
    [InlineData(0x1000, 0x2000, false)]
    public void Execute_DestinationNotModified_ZeroFlagState(ushort sourceValue, ushort destValue, bool expectedZero)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, sourceValue);
        registerFile.WriteRegister(RegisterName.R4, destValue);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
    }

    [Theory]
    [InlineData(0x1000, 0x2000, false)]
    public void Execute_DestinationNotModified_NegativeFlagState(ushort sourceValue, ushort destValue, bool expectedNegative)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, sourceValue);
        registerFile.WriteRegister(RegisterName.R4, destValue);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
    }

    [Theory]
    [InlineData(0x1000, 0x2000, false)]
    public void Execute_DestinationNotModified_CarryFlagState(ushort sourceValue, ushort destValue, bool expectedCarry)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, sourceValue);
        registerFile.WriteRegister(RegisterName.R4, destValue);

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(expectedCarry, registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void Execute_OverflowCondition_SetsOverflowFlag_DestinationValue()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x8000); // Negative source
        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Positive destination

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x7FFF, registerFile.ReadRegister(RegisterName.R4)); // Destination unchanged
    }

    [Fact]
    public void Execute_OverflowCondition_SetsOverflowFlag_OverflowFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x8000); // Negative source
        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Positive destination

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Overflow);
    }

    [Fact]
    public void Execute_OverflowCondition_SetsOverflowFlag_NegativeFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x8000); // Negative source
        registerFile.WriteRegister(RegisterName.R4, 0x7FFF); // Positive destination

        var instruction = new CmpInstruction(
            0x9014,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            false);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Negative);
    }

    [Fact]
    public void Execute_ByteOperation_ComparesLowBytesOnly_DestinationValue()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x5634); // Same low byte as source

        var instruction = new CmpInstruction(
            0x9552,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x5634, registerFile.ReadRegister(RegisterName.R4)); // Destination unchanged
    }

    [Fact]
    public void Execute_ByteOperation_ComparesLowBytesOnly_Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x5634); // Same low byte as source

        var instruction = new CmpInstruction(
            0x9552,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_ByteOperation_ComparesLowBytesOnly_ZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);
        registerFile.WriteRegister(RegisterName.R4, 0x5634); // Same low byte as source

        var instruction = new CmpInstruction(
            0x9552,
            RegisterName.R1,
            RegisterName.R4,
            AddressingMode.Register,
            AddressingMode.Register,
            true);

        // Act
        instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.True(registerFile.StatusRegister.Zero); // Low bytes are equal (0x34)
    }

    [Fact]
    public void Execute_ImmediateComparison_ComparesWithImmediateValue_DestinationValue()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);

        var instruction = new CmpInstruction(
            0x9031,
            RegisterName.R0, // Using R0 for immediate addressing
            RegisterName.R1,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = [0x1234]; // Immediate value

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R1)); // Destination unchanged
    }

    [Fact]
    public void Execute_ImmediateComparison_ComparesWithImmediateValue_Cycles()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);

        var instruction = new CmpInstruction(
            0x9031,
            RegisterName.R0, // Using R0 for immediate addressing
            RegisterName.R1,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = [0x1234]; // Immediate value

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(1u, cycles);
    }

    [Fact]
    public void Execute_ImmediateComparison_ComparesWithImmediateValue_ZeroFlag()
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1234);

        var instruction = new CmpInstruction(
            0x9031,
            RegisterName.R0, // Using R0 for immediate addressing
            RegisterName.R1,
            AddressingMode.Immediate,
            AddressingMode.Register,
            false);

        ushort[] extensionWords = [0x1234]; // Immediate value

        // Act
        instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.True(registerFile.StatusRegister.Zero); // Values are equal
    }

    // Cycle count tests
    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, 1u)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, 1u)]
    [InlineData(AddressingMode.Register, AddressingMode.Indexed, 4u)]
    [InlineData(AddressingMode.Register, AddressingMode.Indirect, 3u)]
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute, 7u)]
    [InlineData(AddressingMode.Symbolic, AddressingMode.Symbolic, 7u)]
    public void Execute_CycleCounts_AreCorrect(AddressingMode sourceMode, AddressingMode destMode, uint expectedCycles)
    {
        // Arrange
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateTestEnvironment();
        registerFile.WriteRegister(RegisterName.R1, 0x1000);
        registerFile.WriteRegister(RegisterName.R4, 0x2000);

        var instruction = new CmpInstruction(
            0x9000,
            RegisterName.R1,
            RegisterName.R4,
            sourceMode,
            destMode,
            false);

        // Set up extension words for modes that need them
        ushort[] extensionWords = sourceMode switch
        {
            AddressingMode.Immediate when destMode == AddressingMode.Register => [0x0100],
            AddressingMode.Absolute when destMode == AddressingMode.Absolute => [0x1000, 0x2000],
            AddressingMode.Symbolic when destMode == AddressingMode.Symbolic => [0x1000, 0x2000],
            AddressingMode.Register when destMode == AddressingMode.Indexed => [0x0010],
            AddressingMode.Register when destMode == AddressingMode.Indirect => Array.Empty<ushort>(),
            _ => Array.Empty<ushort>()
        };

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(expectedCycles, cycles);
    }
}
