using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Abstract base class for conditional jump instructions in the MSP430 CPU.
/// 
/// Conditional jumps are Format III instructions that perform PC-relative jumps
/// based on status register flag conditions. Each conditional jump tests specific
/// flag combinations and jumps only when the condition is met.
/// 
/// The 10-bit offset is sign-extended and represents the number of words to jump,
/// with a range of -511 to +512 words (-1022 to +1024 bytes).
/// 
/// Implementation based on:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Instruction Set
/// </summary>
public abstract class ConditionalJumpInstruction : Instruction, IExecutableInstruction
{
    /// <summary>
    /// The minimum allowed offset value for conditional jump instructions (-511 words).
    /// </summary>
    public const short MinOffset = -511;

    /// <summary>
    /// The maximum allowed offset value for conditional jump instructions (+512 words).
    /// </summary>
    public const short MaxOffset = 512;

    private readonly FormatIIIInstruction _formatIiiInstruction;

    /// <summary>
    /// Initializes a new instance of the ConditionalJumpInstruction class.
    /// </summary>
    /// <param name="conditionCode">The 3-bit condition code (0-7) that determines the jump condition.</param>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="offset">The signed 10-bit word offset for the jump (-511 to +512 words).</param>
    protected ConditionalJumpInstruction(ushort conditionCode, ushort instructionWord, short offset)
        : base(InstructionFormat.FormatIII, conditionCode, instructionWord)
    {
        if (offset < MinOffset || offset > MaxOffset)
        {
            throw new ArgumentOutOfRangeException(nameof(offset),
                $"Jump offset {offset} is outside valid range ({MinOffset} to +{MaxOffset} words)");
        }

        Offset = offset;
        _formatIiiInstruction = new FormatIIIInstruction(conditionCode, instructionWord, offset);
    }

    /// <summary>
    /// Gets the jump offset in words.
    /// </summary>
    public short Offset { get; }

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// Jump instructions do not use extension words.
    /// </summary>
    public override int ExtensionWordCount => 0;

    /// <summary>
    /// Executes the conditional jump instruction on the specified CPU state.
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
