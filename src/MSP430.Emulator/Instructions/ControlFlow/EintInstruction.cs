using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Implements the MSP430 EINT (Enable Interrupts) instruction.
/// 
/// The EINT instruction enables all maskable interrupts by setting the GIE bit in the status register.
/// This instruction is emulated as BIS #8, SR.
/// 
/// Note: Due to pipelined CPU architecture, the instruction following EINT is always executed,
/// even if an interrupt service request is pending when interrupts are enabled.
/// 
/// Operation: 1 → GIE (sets bit 3 in status register)
/// Emulation: BIS #8, SR
/// Cycles: 1 (per Format I immediate mode BIS instruction)
/// Flags: Not affected (except GIE bit is set)
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.20: EINT
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class EintInstruction : Instruction, IExecutableInstruction
{
    /// <summary>
    /// Initializes a new instance of the EintInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word (EINT is emulated as BIS #8, SR).</param>
    public EintInstruction(ushort instructionWord)
        : base(InstructionFormat.FormatI, 0xD, instructionWord) // Emulated as BIS instruction
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
    /// EINT instructions are always word operations.
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// EINT does not require extension words.
    /// </summary>
    public override int ExtensionWordCount => 0;

    /// <summary>
    /// Gets the mnemonic for the EINT instruction.
    /// </summary>
    public override string Mnemonic => "EINT";

    /// <summary>
    /// Executes the EINT instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (not used for EINT).</param>
    /// <returns>The number of CPU cycles consumed by this instruction (always 1 for emulated BIS #8, SR).</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // EINT sets the GIE (General Interrupt Enable) bit in the status register
        // This is equivalent to BIS #8, SR (set bit 3)
        StatusRegister statusRegister = registerFile.StatusRegister;
        statusRegister.GeneralInterruptEnable = true;

        // EINT instructions do not affect other status flags per SLAU445I specification

        // Return cycle count for emulated BIS #8, SR instruction (1 cycle)
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
