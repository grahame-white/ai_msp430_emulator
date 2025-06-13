using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;
using MSP430.Emulator.Instructions.DataMovement;
using MSP430.Emulator.Instructions.Logic;

namespace MSP430.Emulator.Tests.Instructions;

/// <summary>
/// Tests for InstructionDecoder functionality specifically focusing on 
/// executable instruction creation and compliance with MSP430 specifications.
/// 
/// These tests verify that the decoder creates proper executable instruction objects
/// that implement IExecutableInstruction and can perform actual operations,
/// rather than placeholder objects.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1: Instruction Format
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6: Instruction Set Description
/// </summary>
public class InstructionDecoderExecutionTests
{
    [Theory]
    [InlineData(0x5506, typeof(AddInstruction))]  // ADD R5, R6
    [InlineData(0x8506, typeof(SubInstruction))]  // SUB R5, R6  
    [InlineData(0x9506, typeof(CmpInstruction))]  // CMP R5, R6
    [InlineData(0x4506, typeof(MovInstruction))]  // MOV R5, R6
    [InlineData(0xF506, typeof(AndInstruction))]  // AND R5, R6
    [InlineData(0xE506, typeof(XorInstruction))]  // XOR R5, R6
    [InlineData(0xD506, typeof(BisInstruction))]  // BIS R5, R6
    [InlineData(0xC506, typeof(BicInstruction))]  // BIC R5, R6
    [InlineData(0xB506, typeof(BitInstruction))]  // BIT R5, R6
    public void Decode_FormatIInstruction_CreatesSpecificExecutableInstruction(ushort instructionWord, Type expectedType)
    {
        // Arrange
        var decoder = new InstructionDecoder();

        // Act
        Instruction instruction = decoder.Decode(instructionWord);

        // Assert
        Assert.IsType(expectedType, instruction);
        Assert.IsAssignableFrom<IExecutableInstruction>(instruction);
        Assert.Equal(InstructionFormat.FormatI, instruction.Format);
    }

    [Fact]
    public void Decode_AddInstruction_CreatesExecutableAddInstruction()
    {
        // Arrange
        var decoder = new InstructionDecoder();
        ushort addInstructionWord = 0x5506; // ADD R5, R6

        // Act
        Instruction instruction = decoder.Decode(addInstructionWord);

        // Assert
        AddInstruction addInstruction = Assert.IsType<AddInstruction>(instruction);
        Assert.Equal(0x5, addInstruction.Opcode);
        Assert.Equal(RegisterName.R5, addInstruction.SourceRegister);
        Assert.Equal(RegisterName.R6, addInstruction.DestinationRegister);
        Assert.Equal(AddressingMode.Register, addInstruction.SourceAddressingMode);
        Assert.Equal(AddressingMode.Register, addInstruction.DestinationAddressingMode);
        Assert.False(addInstruction.IsByteOperation);
    }

    [Fact]
    public void Decode_AddInstruction_CanExecuteSuccessfully()
    {
        // Arrange
        var decoder = new InstructionDecoder();
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up test values
        registerFile.WriteRegister(RegisterName.R5, 0x1000);
        registerFile.WriteRegister(RegisterName.R6, 0x2000);

        ushort addInstructionWord = 0x5506; // ADD R5, R6

        // Act
        Instruction instruction = decoder.Decode(addInstructionWord);
        var executableInstruction = (IExecutableInstruction)instruction;
        uint cycles = executableInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x3000, registerFile.ReadRegister(RegisterName.R6)); // 0x1000 + 0x2000 = 0x3000
        Assert.True(cycles > 0, "Execution should consume at least one cycle");
    }

    [Fact]
    public void Decode_SubInstruction_CanExecuteSuccessfully()
    {
        // Arrange
        var decoder = new InstructionDecoder();
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up test values
        registerFile.WriteRegister(RegisterName.R5, 0x1000);
        registerFile.WriteRegister(RegisterName.R6, 0x3000);

        ushort subInstructionWord = 0x8506; // SUB R5, R6

        // Act
        Instruction instruction = decoder.Decode(subInstructionWord);
        var executableInstruction = (IExecutableInstruction)instruction;
        uint cycles = executableInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x2000, registerFile.ReadRegister(RegisterName.R6)); // 0x3000 - 0x1000 = 0x2000
        Assert.True(cycles > 0, "Execution should consume at least one cycle");
    }

    [Fact]
    public void Decode_MovInstruction_CanExecuteSuccessfully()
    {
        // Arrange
        var decoder = new InstructionDecoder();
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up test values
        registerFile.WriteRegister(RegisterName.R5, 0x1234);
        registerFile.WriteRegister(RegisterName.R6, 0x0000);

        ushort movInstructionWord = 0x4506; // MOV R5, R6

        // Act
        Instruction instruction = decoder.Decode(movInstructionWord);
        var executableInstruction = (IExecutableInstruction)instruction;
        uint cycles = executableInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x1234, registerFile.ReadRegister(RegisterName.R6)); // R6 should now contain R5's value
        Assert.True(cycles > 0, "Execution should consume at least one cycle");
    }

    [Theory]
    [InlineData(0x6000, "ADDC")]  // ADDC instruction - now implemented
    [InlineData(0x7000, "SUBC")]  // SUBC instruction - now implemented
    [InlineData(0xA000, "DADD")]  // DADD instruction - now implemented
    public void Decode_NewlyImplementedInstructions_DecodesSuccessfully(ushort instructionWord, string expectedMnemonic)
    {
        // Arrange
        var decoder = new InstructionDecoder();

        // Act
        Instruction instruction = decoder.Decode(instructionWord);

        // Assert
        Assert.NotNull(instruction);
        Assert.StartsWith(expectedMnemonic, instruction.Mnemonic);
    }

    [Fact]
    public void Decode_AddcInstruction_CanExecuteSuccessfullyWithCarry()
    {
        // Arrange
        var decoder = new InstructionDecoder();
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up test values: R5 = 0x1000, R6 = 0x2000, and set carry flag
        registerFile.WriteRegister(RegisterName.R5, 0x1000);
        registerFile.WriteRegister(RegisterName.R6, 0x2000);
        registerFile.WriteRegister(RegisterName.SR, 0x0001); // Set carry flag

        ushort addcInstructionWord = 0x6506; // ADDC R5, R6 (opcode=6, src=R5, as=00, bw=0, ad=0, dst=R6)

        // Act
        Instruction instruction = decoder.Decode(addcInstructionWord);
        var executableInstruction = (IExecutableInstruction)instruction;
        uint cycles = executableInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x3001, registerFile.ReadRegister(RegisterName.R6)); // 0x2000 + 0x1000 + 0x0001 (carry)
        Assert.True(cycles > 0, "Execution should consume at least one cycle");

        // Verify status flags are set correctly
        ushort statusRegister = registerFile.ReadRegister(RegisterName.SR);
        Assert.False((statusRegister & 0x0002) != 0, "Zero flag should be clear");
        Assert.False((statusRegister & 0x0004) != 0, "Negative flag should be clear");
    }

    [Fact]
    public void Decode_SubcInstruction_CanExecuteSuccessfullyWithCarry()
    {
        // Arrange
        var decoder = new InstructionDecoder();
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up test values: R5 = 0x1000, R6 = 0x3000, and set carry flag
        registerFile.WriteRegister(RegisterName.R5, 0x1000);
        registerFile.WriteRegister(RegisterName.R6, 0x3000);
        registerFile.WriteRegister(RegisterName.SR, 0x0001); // Set carry flag

        ushort subcInstructionWord = 0x7506; // SUBC R5, R6 (opcode=7, src=R5, as=00, bw=0, ad=0, dst=R6)

        // Act
        Instruction instruction = decoder.Decode(subcInstructionWord);
        var executableInstruction = (IExecutableInstruction)instruction;
        uint cycles = executableInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

        // Assert  
        Assert.Equal(0x2000, registerFile.ReadRegister(RegisterName.R6)); // 0x3000 - 0x1000 + 0x0001 (carry) - 1
        Assert.True(cycles > 0, "Execution should consume at least one cycle");

        // Verify status flags are set correctly
        ushort statusRegister = registerFile.ReadRegister(RegisterName.SR);
        Assert.False((statusRegister & 0x0002) != 0, "Zero flag should be clear");
        Assert.False((statusRegister & 0x0004) != 0, "Negative flag should be clear");
    }

    [Fact]
    public void Decode_DaddInstruction_CanExecuteSuccessfullyWithDecimalArithmetic()
    {
        // Arrange
        var decoder = new InstructionDecoder();
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up test values for decimal addition: R5 = 0x0099, R6 = 0x0001 (99 + 1 = 100 in BCD)
        registerFile.WriteRegister(RegisterName.R5, 0x0099);
        registerFile.WriteRegister(RegisterName.R6, 0x0001);

        ushort daddInstructionWord = 0xA506; // DADD R5, R6 (opcode=A, src=R5, as=00, bw=0, ad=0, dst=R6)

        // Act
        Instruction instruction = decoder.Decode(daddInstructionWord);
        var executableInstruction = (IExecutableInstruction)instruction;
        uint cycles = executableInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x0100, registerFile.ReadRegister(RegisterName.R6)); // BCD: 99 + 01 = 100
        Assert.True(cycles > 0, "Execution should consume at least one cycle");

        // Verify status flags are set correctly
        ushort statusRegister = registerFile.ReadRegister(RegisterName.SR);
        Assert.False((statusRegister & 0x0002) != 0, "Zero flag should be clear");
        Assert.False((statusRegister & 0x0004) != 0, "Negative flag should be clear");
    }

    [Fact]
    public void Decode_ByteOperationAddInstruction_CreatesCorrectInstruction()
    {
        // Arrange
        var decoder = new InstructionDecoder();
        ushort addByteInstructionWord = 0x5546; // ADD.B R5, R6 (B/W bit set)

        // Act
        Instruction instruction = decoder.Decode(addByteInstructionWord);

        // Assert
        AddInstruction addInstruction = Assert.IsType<AddInstruction>(instruction);
        Assert.True(addInstruction.IsByteOperation, "Should be a byte operation");
        Assert.Equal("ADD.B", addInstruction.Mnemonic);
    }

    [Theory]
    [InlineData(0x1084, typeof(SwpbInstruction))]  // SWPB R4
    [InlineData(0x1185, typeof(SxtInstruction))]   // SXT R5
    public void Decode_FormatIIInstruction_CreatesSpecificExecutableInstruction(ushort instructionWord, Type expectedType)
    {
        // Arrange
        var decoder = new InstructionDecoder();

        // Act
        Instruction instruction = decoder.Decode(instructionWord);

        // Assert
        Assert.IsType(expectedType, instruction);
        Assert.IsAssignableFrom<IExecutableInstruction>(instruction);
        Assert.Equal(InstructionFormat.FormatII, instruction.Format);
    }

    [Fact]
    public void Decode_SwpbInstruction_CanExecuteSuccessfully()
    {
        // Arrange
        var decoder = new InstructionDecoder();
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up test value: 0x1234 should become 0x3412 after byte swap
        registerFile.WriteRegister(RegisterName.R4, 0x1234);

        ushort swpbInstructionWord = 0x1084; // SWPB R4

        // Act
        Instruction instruction = decoder.Decode(swpbInstructionWord);
        var executableInstruction = (IExecutableInstruction)instruction;
        uint cycles = executableInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

        // Assert
        Assert.Equal(0x3412, registerFile.ReadRegister(RegisterName.R4)); // Bytes swapped
        Assert.True(cycles > 0, "Execution should consume at least one cycle");
    }

    [Fact]
    public void Decode_SxtInstruction_CanExecuteSuccessfully()
    {
        // Arrange
        var decoder = new InstructionDecoder();
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000];

        // Set up test value: 0x0080 should become 0xFF80 after sign extension
        registerFile.WriteRegister(RegisterName.R5, 0x0080);

        ushort sxtInstructionWord = 0x1185; // SXT R5

        // Act
        Instruction instruction = decoder.Decode(sxtInstructionWord);
        var executableInstruction = (IExecutableInstruction)instruction;
        uint cycles = executableInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

        // Assert
        Assert.Equal(0xFF80, registerFile.ReadRegister(RegisterName.R5)); // Sign extended
        Assert.True(cycles > 0, "Execution should consume at least one cycle");
    }
}
