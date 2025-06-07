using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.DataMovement;
using MSP430.Emulator.Tests.TestUtilities;

namespace MSP430.Emulator.Tests.Instructions.DataMovement;

/// <summary>
/// Unit tests for the PopInstruction class.
/// Tests all addressing modes, byte/word operations, and stack management behavior.
/// Based on MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3: "CPU"
/// </summary>
public class PopInstructionTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsFormat()
    {
        // Arrange & Act
        var instruction = new PopInstruction(
            0x1304,
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
        var instruction = new PopInstruction(
            0x1304,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(0x13, instruction.Opcode);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsInstructionWord()
    {
        // Arrange & Act
        var instruction = new PopInstruction(
            0x1304,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(0x1304, instruction.InstructionWord);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsMnemonic()
    {
        // Arrange & Act
        var instruction = new PopInstruction(
            0x1304,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal("POP", instruction.Mnemonic);
    }

    [Fact]
    public void Constructor_ByteOperation_SetsMnemonic()
    {
        // Arrange & Act
        var instruction = new PopInstruction(
            0x1344,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Assert
        Assert.Equal("POP.B", instruction.Mnemonic);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_ValidParameters_SetsByteOperation(bool expectedByteOp)
    {
        // Arrange & Act
        var instruction = new PopInstruction(
            0x1304,
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
    public void Constructor_ValidParameters_SetsDestinationRegister(RegisterName expectedDestinationRegister)
    {
        // Arrange & Act
        var instruction = new PopInstruction(
            0x1304,
            expectedDestinationRegister,
            AddressingMode.Register,
            false);

        // Assert
        Assert.Equal(expectedDestinationRegister, instruction.DestinationRegister);
    }

    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indexed)]
    public void Constructor_ValidParameters_SetsDestinationAddressingMode(AddressingMode expectedDestinationAddressingMode)
    {
        // Arrange & Act
        var instruction = new PopInstruction(
            0x1304,
            RegisterName.R4,
            expectedDestinationAddressingMode,
            false);

        // Assert
        Assert.Equal(expectedDestinationAddressingMode, instruction.DestinationAddressingMode);
    }

    [Theory]
    [InlineData(AddressingMode.Register, 0)]
    [InlineData(AddressingMode.Indexed, 1)]
    [InlineData(AddressingMode.Absolute, 1)]
    [InlineData(AddressingMode.Symbolic, 1)]
    public void ExtensionWordCount_ReturnsCorrectCount(AddressingMode addressingMode, int expectedCount)
    {
        // Arrange & Act
        var instruction = new PopInstruction(
            0x1304,
            RegisterName.R4,
            addressingMode,
            false);

        // Assert
        Assert.Equal(expectedCount, instruction.ExtensionWordCount);
    }

    [Fact]
    public void Execute_RegisterMode_PopsValueAndIncrementsStackPointer()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PopInstruction(0x1304, RegisterName.R4, AddressingMode.Register, false);

        registerFile.SetStackPointer(0x0FFE);

        // Setup stack memory
        memory[0x0FFE] = 0x34; // Low byte
        memory[0x0FFF] = 0x12; // High byte

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1000, registerFile.GetStackPointer()); // SP incremented by 2
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R4)); // Value popped to register
        Assert.Equal(1u, cycles); // Register mode cycle count
    }

    [Fact]
    public void Execute_ByteOperationWord_StoresOnlyLowByte()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PopInstruction(0x1344, RegisterName.R4, AddressingMode.Register, true);

        registerFile.WriteRegister(RegisterName.R4, 0xFFFF); // Pre-fill register
        registerFile.SetStackPointer(0x0FFE);

        // Setup stack memory
        memory[0x0FFE] = 0x34; // Low byte
        memory[0x0FFF] = 0x12; // High byte

        // Act
        uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1000, registerFile.GetStackPointer());
        // For byte operations, only the low byte is used, high byte of register is preserved
        Assert.Equal(0xFF34, registerFile.ReadRegister(RegisterName.R4));
    }

    [Fact]
    public void Execute_IndexedMode_PopsToIndexedLocation()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PopInstruction(0x1314, RegisterName.R4, AddressingMode.Indexed, false);

        registerFile.WriteRegister(RegisterName.R4, 0x2000); // Base address
        registerFile.SetStackPointer(0x0FFE);

        // Setup stack memory
        memory[0x0FFE] = 0xAB; // Low byte
        memory[0x0FFF] = 0xCD; // High byte

        ushort[] extensionWords = new ushort[] { 0x0010 }; // Offset

        // Act
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Assert
        Assert.Equal(0x1000, registerFile.GetStackPointer());
        // Value should be written to base + offset = 0x2000 + 0x0010 = 0x2010
        Assert.Equal(0xAB, memory[0x2010]); // Low byte at indexed location
        Assert.Equal(0xCD, memory[0x2011]); // High byte at indexed location
        Assert.Equal(4u, cycles); // Indexed mode cycle count
    }

    [Fact]
    public void Execute_StackUnderflow_MemoryOutOfBounds_ThrowsException()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x1000]; // Limited memory
        var instruction = new PopInstruction(0x1304, RegisterName.R4, AddressingMode.Register, false);

        registerFile.SetStackPointer(0x1000); // SP that would access beyond memory bounds

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
        Assert.Contains("Stack underflow", exception.Message);
    }

    [Fact]
    public void Execute_StackPointerOverflow_ThrowsException()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];
        var instruction = new PopInstruction(0x1304, RegisterName.R4, AddressingMode.Register, false);

        registerFile.SetStackPointer(0xFFFF); // SP that would overflow when incremented

        // Setup valid memory access at current SP
        memory[0xFFFF] = 0x34;

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            instruction.Execute(registerFile, memory, Array.Empty<ushort>()));
        Assert.Contains("Stack underflow", exception.Message);
    }

    [Fact]
    public void Execute_ValidStackOperation_MultipleOperations()
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Simulate a push followed by a pop
        var pushInstruction = new PushInstruction(0x1204, RegisterName.R4, AddressingMode.Register, false);
        var popInstruction = new PopInstruction(0x1305, RegisterName.R5, AddressingMode.Register, false);

        registerFile.WriteRegister(RegisterName.R4, 0x1234);
        registerFile.SetStackPointer(0x1000);

        // Act - Push first
        pushInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Act - Pop to different register
        popInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1000, registerFile.GetStackPointer()); // SP back to original position
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R4)); // Original value preserved
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R5)); // Value popped to new register
    }

    [Fact]
    public void ToString_WordOperation_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new PopInstruction(
            0x1304,
            RegisterName.R4,
            AddressingMode.Register,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("POP R4", result);
    }

    [Fact]
    public void ToString_ByteOperation_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new PopInstruction(
            0x1344,
            RegisterName.R4,
            AddressingMode.Register,
            true);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("POP.B R4", result);
    }

    /// <summary>
    /// Tests all valid addressing modes for POP instruction (single operand).
    /// Based on MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3: "CPU"
    /// Testing all 6 valid addressing modes for single-operand destination instructions (excluding Immediate).
    /// </summary>
    [Theory]
    [InlineData(AddressingMode.Register)]
    [InlineData(AddressingMode.Indirect)]
    [InlineData(AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Indexed)]
    [InlineData(AddressingMode.Absolute)]
    [InlineData(AddressingMode.Symbolic)]
    public void Execute_AllDestinationAddressingModes_ExecutesSuccessfully(AddressingMode destMode)
    {
        // Arrange
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up initial values
        registerFile.SetStackPointer(0x1000);
        registerFile.WriteRegister(RegisterName.R4, 0x0200);

        // Set up stack value to pop
        memory[0x1000] = 0x34;
        memory[0x1001] = 0x12;

        // Ensure memory is available for destination writes
        for (int i = 0x0200; i < 0x0300; i++)
        {
            memory[i] = 0x00;
        }

        var instruction = new PopInstruction(
            0x1304,
            RegisterName.R4,
            destMode,
            false);

        ushort[] extensionWords = destMode switch
        {
            AddressingMode.Indexed => [0x0100],
            AddressingMode.Absolute => [0x0250],
            AddressingMode.Symbolic => [0x0250],
            _ => []
        };

        // Act & Assert - Should not throw
        uint cycles = instruction.Execute(registerFile, memory, extensionWords);

        // Verify stack pointer was incremented
        Assert.Equal(0x1002, registerFile.GetStackPointer());

        // Verify cycle count is reasonable
        Assert.True(cycles > 0 && cycles <= 6);
    }

    [Fact]
    public void ToString_IndexedMode_ReturnsCorrectFormat()
    {
        // Arrange
        var instruction = new PopInstruction(
            0x1314,
            RegisterName.R5,
            AddressingMode.Indexed,
            false);

        // Act
        string result = instruction.ToString();

        // Assert
        Assert.Equal("POP X(R5)", result);
    }
}
