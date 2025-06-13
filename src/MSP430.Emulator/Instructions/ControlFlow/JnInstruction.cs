using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Represents the JN (Jump if Negative) instruction for the MSP430 CPU.
/// 
/// JN is a Format III instruction that performs a PC-relative jump when the Negative flag is set.
/// This instruction tests the N (Negative) flag in the status register and jumps only when N = 1.
/// 
/// Condition: N = 1 (Negative flag set)
/// Opcode: 4 (condition code in bits 12:10)
/// 
/// The 10-bit offset is sign-extended and represents the number of words to jump,
/// with a range of -511 to +512 words (-1022 to +1024 bytes).
/// 
/// Implementation based on:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Instruction Set
/// </summary>
public class JnInstruction : ConditionalJumpInstruction
{
    /// <summary>
    /// Initializes a new instance of the JnInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="offset">The signed 10-bit word offset for the jump (-511 to +512 words).</param>
    public JnInstruction(ushort instructionWord, short offset)
        : base(4, instructionWord, offset) // Condition code 4 for JN
    {
    }

    /// <summary>
    /// Gets the mnemonic for the JN instruction.
    /// </summary>
    public override string Mnemonic => "JN";
}
