using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.Arithmetic;

namespace MSP430.Emulator.Instructions.EmulatedInstructions;

/// <summary>
/// Implements the MSP430 RLC (Rotate Left through Carry) emulated instruction.
/// 
/// The RLC instruction rotates the destination operand left through the carry flag.
/// This instruction is emulated as ADDC(.B) dst, dst per SLAU445I Table 4-7.
/// 
/// Operation: dst = (dst << 1) | C (rotate left with carry as LSB)
/// Emulation: ADDC(.B) dst, dst
/// Cycles: Same as underlying ADDC instruction
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class RlcInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Initializes a new instance of the RlcInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public RlcInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0x6, instructionWord) // Emulated as ADDC instruction
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
    /// Gets the source register for this instruction (same as destination for RLC).
    /// </summary>
    public override RegisterName? SourceRegister => _destinationRegister;

    /// <summary>
    /// Gets the source addressing mode for this instruction (same as destination for RLC).
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
    public override int ExtensionWordCount =>
        InstructionHelpers.CalculateDestinationOnlyExtensionWordCount(_destinationAddressingMode);

    /// <summary>
    /// Gets the mnemonic for the RLC instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "RLC.B" : "RLC";

    /// <summary>
    /// Executes the RLC instruction on the specified CPU state.
    /// This emulates ADDC dst, dst to rotate left through carry.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction.</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Create an underlying ADDC instruction to perform the actual operation
        // RLC dst is emulated as ADDC dst, dst (adding a value to itself with carry)
        var addcInstruction = new AddcInstruction(
            InstructionWord,
            _destinationRegister, // Source: same as destination
            _destinationRegister, // Destination: same as source
            _destinationAddressingMode, // Source addressing mode: same as destination
            _destinationAddressingMode, // Destination addressing mode
            _isByteOperation);

        // Execute the underlying ADDC instruction
        // Adding a value to itself with carry is equivalent to rotating left through carry
        return addcInstruction.Execute(registerFile, memory, extensionWords);
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
