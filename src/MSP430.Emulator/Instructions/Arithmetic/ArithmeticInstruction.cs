using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Arithmetic;

/// <summary>
/// Base class for MSP430 two-operand arithmetic instructions.
/// 
/// Provides common implementation for arithmetic operations (ADD, SUB, CMP)
/// that share similar instruction format, addressing modes, and basic structure.
/// All two-operand arithmetic instructions use Format I encoding.
/// </summary>
public abstract class ArithmeticInstruction : Instruction
{
    /// <summary>
    /// Initializes a new instance of the ArithmeticInstruction class.
    /// </summary>
    /// <param name="opcode">The instruction opcode.</param>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    protected ArithmeticInstruction(
        byte opcode,
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, opcode, instructionWord)
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
    /// Gets the base mnemonic for this arithmetic instruction (to be overridden by derived classes).
    /// </summary>
    protected abstract string BaseMnemonic { get; }

    /// <summary>
    /// Gets the mnemonic for this instruction, with optional .B suffix for byte operations.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? $"{BaseMnemonic}.B" : BaseMnemonic;

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
    /// Returns a string representation of the arithmetic instruction.
    /// </summary>
    /// <returns>A string describing the instruction in assembly format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {InstructionHelpers.FormatOperand(_sourceRegister, _sourceAddressingMode)}, {InstructionHelpers.FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }
}
