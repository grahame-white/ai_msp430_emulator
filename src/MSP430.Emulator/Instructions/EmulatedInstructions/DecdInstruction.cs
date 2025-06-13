using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.Arithmetic;

namespace MSP430.Emulator.Instructions.EmulatedInstructions;

/// <summary>
/// Implements the MSP430 DECD (Decrement by 2) emulated instruction.
/// 
/// The DECD instruction decrements the destination operand by 2.
/// This instruction is emulated as SUB(.B) #2, dst per SLAU445I Table 4-7.
/// 
/// Operation: dst = dst - 2
/// Emulation: SUB(.B) #2, dst
/// Cycles: Same as underlying SUB instruction
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class DecdInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Initializes a new instance of the DecdInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public DecdInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0x8, instructionWord) // Emulated as SUB instruction
    {
        _destinationRegister = destinationRegister;
        _destinationAddressingMode = destinationAddressingMode;
        _isByteOperation = isByteOperation;
    }

    /// <summary>
    /// Gets the destination register for this instruction.
    /// </summary>
    public override RegisterName? DestinationRegister => _destinationRegister;

    /// <summary>
    /// Gets the source register for this instruction (always CG1/R2 for constant #2).
    /// </summary>
    public override RegisterName? SourceRegister => RegisterName.R2;

    /// <summary>
    /// Gets the source addressing mode for this instruction (always Immediate mode for #2).
    /// </summary>
    public override AddressingMode? SourceAddressingMode => AddressingMode.Immediate;

    /// <summary>
    /// Gets the destination addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => _destinationAddressingMode;

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (true) or words (false).
    /// </summary>
    public override bool IsByteOperation => _isByteOperation;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
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
    /// Gets the mnemonic for the DECD instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "DECD.B" : "DECD";

    /// <summary>
    /// Executes the DECD instruction on the specified CPU state.
    /// This emulates SUB #2, dst to decrement the destination by 2.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction.</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Create an underlying SUB instruction to perform the actual operation
        // DECD dst is emulated as SUB #2, dst
        var subInstruction = new SubInstruction(
            InstructionWord,
            RegisterName.R2, // Source: CG1 for constant #2 (using AS=10)
            _destinationRegister,
            AddressingMode.Immediate, // Source: immediate mode for #2
            _destinationAddressingMode,
            _isByteOperation);

        // Execute the underlying SUB instruction
        // The constant #2 will be provided by the constant generator
        return subInstruction.Execute(registerFile, memory, extensionWords);
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {InstructionHelpers.FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }
}
