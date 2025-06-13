using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.DataMovement;

namespace MSP430.Emulator.Instructions.EmulatedInstructions;

/// <summary>
/// Implements the MSP430 CLR (Clear destination) emulated instruction.
/// 
/// The CLR instruction clears the destination operand (sets it to zero).
/// This instruction is emulated as MOV(.B) #0, dst per SLAU445I Table 4-7.
/// 
/// Operation: dst = 0
/// Emulation: MOV(.B) #0, dst
/// Cycles: Same as underlying MOV instruction
/// Flags affected: N=0, Z=1, C=0, V=0
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class ClrInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Initializes a new instance of the ClrInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public ClrInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0x4, instructionWord) // Emulated as MOV instruction
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
    /// Gets the source addressing mode for this instruction (always Register mode for #0).
    /// </summary>
    public override AddressingMode? SourceAddressingMode => AddressingMode.Register;

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
    /// Gets the mnemonic for the CLR instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "CLR.B" : "CLR";

    /// <summary>
    /// Executes the CLR instruction on the specified CPU state.
    /// This emulates MOV #0, dst to clear the destination.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction.</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Create an underlying MOV instruction to perform the actual operation
        // CLR dst is emulated as MOV #0, dst (using R3 as constant generator)
        var movInstruction = new MovInstruction(
            InstructionWord,
            RegisterName.R3, // Source: CG2 for constant #0 (using AS=00)
            _destinationRegister,
            AddressingMode.Register, // Source: register mode for constant #0
            _destinationAddressingMode,
            _isByteOperation);

        // Execute the underlying MOV instruction
        // The constant #0 will be provided by the constant generator (R3=0)
        return movInstruction.Execute(registerFile, memory, extensionWords);
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
