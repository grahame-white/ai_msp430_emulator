using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Represents the JZ (Jump if Zero) instruction for the MSP430 CPU.
/// 
/// JZ is a Format III instruction that performs a PC-relative jump when the Zero flag is set.
/// This instruction tests the Z (Zero) flag in the status register and jumps only when Z = 1.
/// 
/// Condition: Z = 1 (Zero flag set)
/// Opcode: 0 (condition code in bits 12:10)
/// 
/// The 10-bit offset is sign-extended and represents the number of words to jump,
/// with a range of -511 to +512 words (-1022 to +1024 bytes).
/// 
/// Implementation based on:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Instruction Set
/// </summary>
public class JzInstruction : ConditionalJumpInstruction
{
    /// <summary>
    /// Initializes a new instance of the JzInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="offset">The signed 10-bit word offset for the jump (-511 to +512 words).</param>
    public JzInstruction(ushort instructionWord, short offset)
        : base(0, instructionWord, offset) // Condition code 0 for JZ
    {
    }

    /// <summary>
    /// Gets the mnemonic for the JZ instruction.
    /// </summary>
    public override string Mnemonic => "JZ";
}
