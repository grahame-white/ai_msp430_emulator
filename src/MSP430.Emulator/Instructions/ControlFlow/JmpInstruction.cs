using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Represents the JMP (Jump unconditional) instruction for the MSP430 CPU.
/// 
/// JMP is a Format III instruction that performs an unconditional PC-relative jump.
/// This is a convenience wrapper around FormatIIIInstruction specifically for 
/// unconditional jumps (opcode 7).
/// 
/// The 10-bit offset is sign-extended and represents the number of words to jump,
/// with a range of -511 to +512 words (-1022 to +1024 bytes).
/// 
/// Implementation based on:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Instruction Set
/// </summary>
public class JmpInstruction : Instruction, IExecutableInstruction
{
    private readonly FormatIIIInstruction _formatIiiInstruction;

    /// <summary>
    /// Initializes a new instance of the JmpInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="offset">The signed 10-bit word offset for the jump (-511 to +512 words).</param>
    public JmpInstruction(ushort instructionWord, short offset)
        : base(InstructionFormat.FormatIII, 7, instructionWord) // Opcode 7 for unconditional jump
    {
        if (offset < -511 || offset > 512)
        {
            throw new ArgumentOutOfRangeException(nameof(offset),
                $"Jump offset {offset} is outside valid range (-511 to +512 words)");
        }

        Offset = offset;
        _formatIiiInstruction = new FormatIIIInstruction(7, instructionWord, offset);
    }

    /// <summary>
    /// Gets the jump offset in words.
    /// </summary>
    public short Offset { get; }

    /// <summary>
    /// Gets the mnemonic for the JMP instruction.
    /// </summary>
    public override string Mnemonic => "JMP";

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// JMP instructions do not use extension words.
    /// </summary>
    public override int ExtensionWordCount => 0;

    /// <summary>
    /// Executes the JMP instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory (not used for jump instructions).</param>
    /// <param name="extensionWords">Extension words (not used for jump instructions).</param>
    /// <returns>The number of CPU cycles consumed (always 2 per SLAU445I specification).</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        return _formatIiiInstruction.Execute(registerFile, memory, extensionWords);
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {Offset:+#;-#;0}";
    }
}
