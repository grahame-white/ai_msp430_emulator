using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.DataMovement;

namespace MSP430.Emulator.Instructions.EmulatedInstructions;

/// <summary>
/// Implements the MSP430 BR (Branch) emulated instruction.
/// 
/// The BR instruction performs an unconditional branch to the address specified by the source operand.
/// This instruction is emulated as MOV src, PC per SLAU445I Table 4-7.
/// 
/// Operation: PC = src (branch to address in src)
/// Emulation: MOV src, PC
/// Cycles: Same as underlying MOV instruction
/// Flags affected: None
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class BrInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _sourceRegister;
    private readonly AddressingMode _sourceAddressingMode;

    /// <summary>
    /// Initializes a new instance of the BrInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    public BrInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        AddressingMode sourceAddressingMode)
        : base(InstructionFormat.FormatI, 0x4, instructionWord) // Emulated as MOV instruction
    {
        _sourceRegister = sourceRegister;
        _sourceAddressingMode = sourceAddressingMode;
    }

    /// <summary>
    /// Gets the destination register for this instruction (always PC for branch).
    /// </summary>
    public override RegisterName? DestinationRegister => RegisterName.PC;

    /// <summary>
    /// Gets the source register for this instruction.
    /// </summary>
    public override RegisterName? SourceRegister => _sourceRegister;

    /// <summary>
    /// Gets the source addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? SourceAddressingMode => _sourceAddressingMode;

    /// <summary>
    /// Gets the destination addressing mode for this instruction (always Register mode for PC).
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => AddressingMode.Register;

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (always false for BR).
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// </summary>
    public override int ExtensionWordCount =>
        InstructionHelpers.CalculateSourceOnlyExtensionWordCount(_sourceAddressingMode);

    /// <summary>
    /// Gets the mnemonic for the BR instruction.
    /// </summary>
    public override string Mnemonic => "BR";

    /// <summary>
    /// Executes the BR instruction on the specified CPU state.
    /// This emulates MOV src, PC to branch to the address specified in src.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction.</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Create an underlying MOV instruction to perform the actual operation
        // BR src is emulated as MOV src, PC
        var movInstruction = new MovInstruction(
            InstructionWord,
            _sourceRegister, // Source register
            RegisterName.PC, // Destination: Program Counter
            _sourceAddressingMode, // Source addressing mode
            AddressingMode.Register, // Destination: register mode for PC
            false); // Never a byte operation

        // Execute the underlying MOV instruction
        // This will load the address from src into PC, causing a branch
        return movInstruction.Execute(registerFile, memory, extensionWords);
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {InstructionHelpers.FormatOperand(_sourceRegister, _sourceAddressingMode)}";
    }
}
