using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.Arithmetic;

namespace MSP430.Emulator.Instructions.EmulatedInstructions;

/// <summary>
/// Implements the MSP430 RLA (Rotate Left Arithmetically) emulated instruction.
/// 
/// The RLA instruction rotates the destination operand left arithmetically (equivalent to shifting left by 1).
/// This instruction is emulated as ADD(.B) dst, dst per SLAU445I Table 4-7.
/// 
/// Operation: dst = dst << 1 (arithmetic left shift)
/// Emulation: ADD(.B) dst, dst
/// Cycles: Same as underlying ADD instruction
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class RlaInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Initializes a new instance of the RlaInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public RlaInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0x5, instructionWord) // Emulated as ADD instruction
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
    /// Gets the source register for this instruction (same as destination for RLA).
    /// </summary>
    public override RegisterName? SourceRegister => _destinationRegister;

    /// <summary>
    /// Gets the source addressing mode for this instruction (same as destination for RLA).
    /// </summary>
    public override AddressingMode? SourceAddressingMode => _destinationAddressingMode;

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
            // Both source and destination use the same operand, so count extension words once
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
    /// Gets the mnemonic for the RLA instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "RLA.B" : "RLA";

    /// <summary>
    /// Executes the RLA instruction on the specified CPU state.
    /// This emulates ADD dst, dst to rotate left arithmetically (shift left by 1).
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction.</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Create an underlying ADD instruction to perform the actual operation
        // RLA dst is emulated as ADD dst, dst (adding a value to itself effectively shifts left by 1)
        var addInstruction = new AddInstruction(
            InstructionWord,
            _destinationRegister, // Source: same as destination
            _destinationRegister, // Destination: same as source
            _destinationAddressingMode, // Source addressing mode: same as destination
            _destinationAddressingMode, // Destination addressing mode
            _isByteOperation);

        // Execute the underlying ADD instruction
        // Adding a value to itself is equivalent to shifting left by 1 bit
        return addInstruction.Execute(registerFile, memory, extensionWords);
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
