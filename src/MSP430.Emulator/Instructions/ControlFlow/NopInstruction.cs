using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Implements the MSP430 NOP (No Operation) instruction.
/// 
/// The NOP instruction performs no operation and is used for:
/// 1. Elimination of instructions during software development/debugging
/// 2. Creating defined waiting times in code
/// 3. Pipeline synchronization (e.g., after DINT/EINT per TI recommendations)
/// 
/// This instruction is emulated as MOV #0, R3 and takes 1 cycle.
/// 
/// Operation: None (no registers or memory affected)
/// Emulation: MOV #0, R3
/// Cycles: 1 (per Format I immediate mode MOV instruction)
/// Flags: Not affected
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.33: NOP
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class NopInstruction : Instruction, IExecutableInstruction
{
    /// <summary>
    /// Initializes a new instance of the NopInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word (NOP is emulated as MOV #0, R3).</param>
    public NopInstruction(ushort instructionWord)
        : base(InstructionFormat.FormatI, 0x4, instructionWord) // Emulated as MOV instruction
    {
    }

    /// <summary>
    /// Gets the source register for this instruction (always CG1/R2 for constant #0).
    /// </summary>
    public override RegisterName? SourceRegister => RegisterName.R2;

    /// <summary>
    /// Gets the destination register for this instruction (always R3).
    /// </summary>
    public override RegisterName? DestinationRegister => RegisterName.R3;

    /// <summary>
    /// Gets the source addressing mode for this instruction (always Register mode for constant generator).
    /// </summary>
    public override AddressingMode? SourceAddressingMode => AddressingMode.Register;

    /// <summary>
    /// Gets the destination addressing mode for this instruction (always Register mode for R3).
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => AddressingMode.Register;

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (false) or words (true).
    /// NOP instructions are always word operations.
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// NOP does not require extension words.
    /// </summary>
    public override int ExtensionWordCount => 0;

    /// <summary>
    /// Gets the mnemonic for the NOP instruction.
    /// </summary>
    public override string Mnemonic => "NOP";

    /// <summary>
    /// Executes the NOP instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (not used for NOP).</param>
    /// <returns>The number of CPU cycles consumed by this instruction (always 1 for emulated MOV #0, R3).</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // NOP performs no operation - it's essentially MOV #0, R3 but we don't actually
        // need to modify R3 since the purpose is to do nothing

        // NOP instructions do not affect status flags per SLAU445I specification

        // Return cycle count for emulated MOV #0, R3 instruction (1 cycle)
        return 1;
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        return Mnemonic;
    }
}
