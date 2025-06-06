using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Arithmetic;

/// <summary>
/// Represents the MSP430 DEC instruction.
/// 
/// The DEC instruction decrements the destination operand by 1.
/// This is equivalent to SUB #1, dst but implemented as a single-operand instruction.
/// Format: DEC dst
/// Operation: dst = dst - 1
/// Opcode: 0xB (Format I)
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131), Section 5.3.2: "DEC - Decrement" - Instruction format and operation
/// - MSP430FR2xx/FR4xx Family User's Guide (SLAU445I), Section 4.3.1: "Format I Instructions" - Instruction encoding
/// - MSP430FR2355 Datasheet (SLAS847G), Section 6.12: "Instruction Set" - Opcode definition and flag behavior
/// </summary>
public class DecInstruction : Instruction
{
    /// <summary>
    /// Initializes a new instance of the DecInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public DecInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0xB, instructionWord)
    {
        _destinationRegister = destinationRegister;
        _destinationAddressingMode = destinationAddressingMode;
        _isByteOperation = isByteOperation;
    }

    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Gets the mnemonic for the DEC instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "DEC.B" : "DEC";

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (true) or words (false).
    /// </summary>
    public override bool IsByteOperation => _isByteOperation;

    /// <summary>
    /// Gets the destination register for this instruction.
    /// </summary>
    public override RegisterName? DestinationRegister => _destinationRegister;

    /// <summary>
    /// Gets the destination addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => _destinationAddressingMode;

    /// <summary>
    /// Gets the number of additional words required by this instruction.
    /// </summary>
    public override int ExtensionWordCount
    {
        get
        {
            // Only destination operand can require extension words
            if (_destinationAddressingMode == AddressingMode.Absolute ||
                _destinationAddressingMode == AddressingMode.Symbolic ||
                _destinationAddressingMode == AddressingMode.Indexed)
            {
                return 1;
            }

            return 0;
        }
    }

    /// <summary>
    /// Returns a string representation of the DEC instruction.
    /// </summary>
    /// <returns>A string describing the instruction in assembly format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }

    /// <summary>
    /// Formats an operand for display based on its register and addressing mode.
    /// </summary>
    /// <param name="register">The register.</param>
    /// <param name="addressingMode">The addressing mode.</param>
    /// <returns>A formatted string representation of the operand.</returns>
    private static string FormatOperand(RegisterName register, AddressingMode addressingMode)
    {
        return addressingMode switch
        {
            AddressingMode.Register => $"R{(int)register}",
            AddressingMode.Indexed => $"X(R{(int)register})",
            AddressingMode.Indirect => $"@R{(int)register}",
            AddressingMode.IndirectAutoIncrement => $"@R{(int)register}+",
            AddressingMode.Absolute => "&ADDR",
            AddressingMode.Symbolic => "ADDR",
            _ => "?"
        };
    }
}
