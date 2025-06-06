using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents the MSP430 XOR instruction.
/// 
/// The XOR instruction performs a bitwise exclusive OR operation between the source and destination operands.
/// The result is stored in the destination operand.
/// Format: XOR src, dst
/// Operation: dst = src ^ dst
/// Opcode: 0xE (Format I)
/// Flags affected: N, Z, C (cleared), V (cleared)
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131), Section 5.3.2: "XOR - Exclusive OR" - Instruction format and operation
/// - MSP430FR2xx/FR4xx Family User's Guide (SLAU445I), Section 4.3.1: "Format I Instructions" - Instruction encoding
/// - MSP430FR2355 Datasheet (SLAS847G), Section 6.12: "Instruction Set" - Opcode definition and flag behavior
/// </summary>
public class XorInstruction : Instruction
{
    /// <summary>
    /// Initializes a new instance of the XorInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public XorInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0xE, instructionWord)
    {
        _sourceRegister = sourceRegister;
        _destinationRegister = destinationRegister;
        _sourceAddressingMode = sourceAddressingMode;
        _destinationAddressingMode = destinationAddressingMode;
        _isByteOperation = isByteOperation;
    }

    private readonly RegisterName _sourceRegister;
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _sourceAddressingMode;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Gets the mnemonic for the XOR instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "XOR.B" : "XOR";

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (true) or words (false).
    /// </summary>
    public override bool IsByteOperation => _isByteOperation;

    /// <summary>
    /// Gets the source register for this instruction.
    /// </summary>
    public override RegisterName? SourceRegister => _sourceRegister;

    /// <summary>
    /// Gets the destination register for this instruction.
    /// </summary>
    public override RegisterName? DestinationRegister => _destinationRegister;

    /// <summary>
    /// Gets the source addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? SourceAddressingMode => _sourceAddressingMode;

    /// <summary>
    /// Gets the destination addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => _destinationAddressingMode;

    /// <summary>
    /// Gets the number of additional words required by this instruction.
    /// </summary>
    public override int ExtensionWordCount => InstructionHelpers.CalculateFormatIExtensionWordCount(_sourceAddressingMode, _destinationAddressingMode);

    /// <summary>
    /// Returns a string representation of the XOR instruction.
    /// </summary>
    /// <returns>A string describing the instruction in assembly format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {InstructionHelpers.FormatOperand(_sourceRegister, _sourceAddressingMode)}, {InstructionHelpers.FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }
}
