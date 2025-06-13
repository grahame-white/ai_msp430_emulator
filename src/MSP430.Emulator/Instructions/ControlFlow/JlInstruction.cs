using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Represents the JL (Jump if Less) instruction for the MSP430 CPU.
/// 
/// JL is a Format III instruction that performs a PC-relative jump when the signed comparison
/// result is less than zero. This instruction tests the N (Negative) and V (Overflow) flags
/// and jumps when N ⊕ V = 1 (N XOR V equals 1).
/// 
/// Condition: N ⊕ V = 1 (Negative XOR Overflow equals 1)
/// Opcode: 6 (condition code in bits 12:10)
/// 
/// The 10-bit offset is sign-extended and represents the number of words to jump,
/// with a range of -511 to +512 words (-1022 to +1024 bytes).
/// 
/// Implementation based on:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Instruction Set
/// </summary>
public class JlInstruction : ConditionalJumpInstruction
{
    /// <summary>
    /// Initializes a new instance of the JlInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="offset">The signed 10-bit word offset for the jump (-511 to +512 words).</param>
    public JlInstruction(ushort instructionWord, short offset)
        : base(6, instructionWord, offset) // Condition code 6 for JL
    {
    }

    /// <summary>
    /// Gets the mnemonic for the JL instruction.
    /// </summary>
    public override string Mnemonic => "JL";
}
