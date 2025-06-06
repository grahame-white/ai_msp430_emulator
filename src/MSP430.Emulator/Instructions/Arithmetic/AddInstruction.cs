using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Arithmetic;

/// <summary>
/// Represents the MSP430 ADD instruction.
/// 
/// The ADD instruction adds the source operand to the destination operand.
/// Format: ADD src, dst
/// Operation: dst = dst + src
/// Opcode: 0x5 (Format I)
/// Flags affected: N, Z, C, V
/// </summary>
public class AddInstruction : Instruction
{
    /// <summary>
    /// Initializes a new instance of the AddInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public AddInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0x5, instructionWord)
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
    /// Gets the mnemonic for the ADD instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "ADD.B" : "ADD";

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
    public override int ExtensionWordCount
    {
        get
        {
            int count = 0;

            // Source operand extension words
            if (_sourceAddressingMode == AddressingMode.Immediate ||
                _sourceAddressingMode == AddressingMode.Absolute ||
                _sourceAddressingMode == AddressingMode.Symbolic ||
                _sourceAddressingMode == AddressingMode.Indexed)
            {
                count++;
            }

            // Destination operand extension words
            if (_destinationAddressingMode == AddressingMode.Absolute ||
                _destinationAddressingMode == AddressingMode.Symbolic ||
                _destinationAddressingMode == AddressingMode.Indexed)
            {
                count++;
            }

            return count;
        }
    }

    /// <summary>
    /// Returns a string representation of the ADD instruction.
    /// </summary>
    /// <returns>A string describing the instruction in assembly format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {FormatOperand(_sourceRegister, _sourceAddressingMode)}, {FormatOperand(_destinationRegister, _destinationAddressingMode)}";
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
            AddressingMode.Immediate => "#N",
            AddressingMode.Absolute => "&ADDR",
            AddressingMode.Symbolic => "ADDR",
            _ => "?"
        };
    }
}
