using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.Arithmetic;

namespace MSP430.Emulator.Instructions.EmulatedInstructions;

/// <summary>
/// Implements the MSP430 DADC (Decimal Add Carry) emulated instruction.
/// 
/// The DADC instruction adds the carry bit to the destination operand using BCD arithmetic.
/// This instruction is emulated as DADD #0, dst per SLAU445I Table 4-7.
/// 
/// Operation: dst = dst + C (BCD)
/// Emulation: DADD(.B) #0, dst
/// Cycles: Same as underlying DADD instruction
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class DadcInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Initializes a new instance of the DadcInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public DadcInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0xA, instructionWord) // Emulated as DADD instruction
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
    /// Gets the source register for this instruction (always CG2/R3 for constant #0).
    /// </summary>
    public override RegisterName? SourceRegister => RegisterName.R3;

    /// <summary>
    /// Gets the source addressing mode for this instruction (always Immediate mode for #0).
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
    public override int ExtensionWordCount =>
        InstructionHelpers.CalculateDestinationOnlyExtensionWordCount(_destinationAddressingMode);

    /// <summary>
    /// Gets the mnemonic for the DADC instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "DADC.B" : "DADC";

    /// <summary>
    /// Executes the DADC instruction on the specified CPU state.
    /// This emulates DADD #0, dst to add only the carry bit to the destination using BCD arithmetic.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction.</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Create an underlying DADD instruction to perform the actual operation
        // DADC dst is emulated as DADD #0, dst
        var daddInstruction = new DaddInstruction(
            InstructionWord,
            RegisterName.R3, // Source: CG2 for constant #0
            _destinationRegister,
            AddressingMode.Register, // Source: register mode for #0 constant
            _destinationAddressingMode,
            _isByteOperation);

        // Execute the underlying DADD instruction
        // Note: The DADD instruction will handle reading the carry flag from the status register
        return daddInstruction.Execute(registerFile, memory, extensionWords);
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
