using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Implements the MSP430 DINT (Disable Interrupts) instruction.
/// 
/// The DINT instruction disables all maskable interrupts by clearing the GIE bit in the status register.
/// This instruction is emulated as BIC #8, SR.
/// 
/// Operation: 0 → GIE (clears bit 3 in status register)
/// Emulation: BIC #8, SR
/// Cycles: 1 (per Format I immediate mode BIC instruction)
/// Flags: Not affected (except GIE bit is cleared)
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.19: DINT
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class DintInstruction : Instruction, IExecutableInstruction
{
    /// <summary>
    /// Initializes a new instance of the DintInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word (DINT is emulated as BIC #8, SR).</param>
    public DintInstruction(ushort instructionWord)
        : base(InstructionFormat.FormatI, 0xC, instructionWord) // Emulated as BIC instruction
    {
    }

    /// <summary>
    /// Gets the source register for this instruction (always CG1/R2 for constant #8).
    /// </summary>
    public override RegisterName? SourceRegister => RegisterName.R2;

    /// <summary>
    /// Gets the destination register for this instruction (always SR/R2).
    /// </summary>
    public override RegisterName? DestinationRegister => RegisterName.R2;

    /// <summary>
    /// Gets the source addressing mode for this instruction (always Immediate mode for #8).
    /// </summary>
    public override AddressingMode? SourceAddressingMode => AddressingMode.Immediate;

    /// <summary>
    /// Gets the destination addressing mode for this instruction (always Register mode for SR).
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => AddressingMode.Register;

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (false) or words (true).
    /// DINT instructions are always word operations.
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// DINT does not require extension words.
    /// </summary>
    public override int ExtensionWordCount => 0;

    /// <summary>
    /// Gets the mnemonic for the DINT instruction.
    /// </summary>
    public override string Mnemonic => "DINT";

    /// <summary>
    /// Executes the DINT instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (not used for DINT).</param>
    /// <returns>The number of CPU cycles consumed by this instruction (always 1 for emulated BIC #8, SR).</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // DINT clears the GIE (General Interrupt Enable) bit in the status register
        // This is equivalent to BIC #8, SR (clear bit 3)
        StatusRegister statusRegister = registerFile.StatusRegister;
        statusRegister.GeneralInterruptEnable = false;

        // DINT instructions do not affect other status flags per SLAU445I specification

        // Return cycle count for emulated BIC #8, SR instruction (1 cycle)
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
