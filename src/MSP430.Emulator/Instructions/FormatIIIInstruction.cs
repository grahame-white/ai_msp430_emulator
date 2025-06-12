using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions;

/// <summary>
/// Represents Format III (Jump) instructions for the MSP430 CPU.
/// 
/// Format III instructions provide conditional and unconditional jumps with PC-relative addressing.
/// The 10-bit offset is sign-extended and represents the number of words to jump.
/// 
/// Implementation based on:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Instruction Set
/// </summary>
public class FormatIIIInstruction : Instruction, IExecutableInstruction
{
    public FormatIIIInstruction(ushort opcode, ushort instructionWord, short offset)
        : base(InstructionFormat.FormatIII, opcode, instructionWord)
    {
        Offset = offset;
    }

    public short Offset { get; }

    public override string Mnemonic => GetJumpMnemonic(Opcode);

    /// <summary>
    /// Executes the jump instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory (not used for jump instructions).</param>
    /// <param name="extensionWords">Extension words (not used for jump instructions).</param>
    /// <returns>The number of CPU cycles consumed (always 2 per SLAU445I specification).</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Validate offset range: -1022 to +1024 bytes (-511 to +512 words)
        // Per SLAU445I Section 4.5.1.3: "This allows jumps in a range of –511 to +512 words"
        if (Offset < -511 || Offset > 512)
        {
            throw new InvalidOperationException($"Jump offset {Offset} is outside valid range (-511 to +512 words)");
        }

        // Evaluate jump condition based on status register flags
        bool shouldJump = EvaluateJumpCondition(Opcode, registerFile.StatusRegister);

        if (shouldJump)
        {
            // Calculate new PC: current PC + 2 + (offset × 2)
            // Note: PC has already been incremented by 2 after instruction fetch
            ushort currentPC = registerFile.GetProgramCounter();
            ushort newPC = (ushort)(currentPC + (Offset * 2));
            registerFile.SetProgramCounter(newPC);
        }

        // All jump instructions take 2 cycles regardless of whether jump is taken
        // Per SLAU445I Section 4.5.1.5.3: "Jump Instructions Cycles and Lengths"
        return 2;
    }

    public override string ToString()
    {
        return $"{Mnemonic} {Offset:+#;-#;0}";
    }

    /// <summary>
    /// Gets the jump instruction mnemonic based on the condition code.
    /// </summary>
    /// <param name="conditionCode">The 3-bit condition code (bits 12:10).</param>
    /// <returns>The instruction mnemonic.</returns>
    private static string GetJumpMnemonic(ushort conditionCode)
    {
        return conditionCode switch
        {
            0 => "JEQ", // Jump if equal/zero (Z = 1)
            1 => "JNE", // Jump if not equal/not zero (Z = 0)
            2 => "JC",  // Jump if carry set (C = 1)
            3 => "JNC", // Jump if carry clear (C = 0)
            4 => "JN",  // Jump if negative (N = 1)
            5 => "JGE", // Jump if greater or equal (N ⊕ V = 0)
            6 => "JL",  // Jump if less than (N ⊕ V = 1)
            7 => "JMP", // Jump unconditional (always)
            _ => $"JUMP_{conditionCode:X}" // Fallback for invalid codes
        };
    }

    /// <summary>
    /// Evaluates the jump condition based on status register flags.
    /// </summary>
    /// <param name="conditionCode">The 3-bit condition code (bits 12:10).</param>
    /// <param name="statusRegister">The CPU status register.</param>
    /// <returns>True if the jump should be taken, false otherwise.</returns>
    private static bool EvaluateJumpCondition(ushort conditionCode, StatusRegister statusRegister)
    {
        return conditionCode switch
        {
            0 => statusRegister.Zero,                                    // JEQ/JZ: Z = 1
            1 => !statusRegister.Zero,                                   // JNE/JNZ: Z = 0
            2 => statusRegister.Carry,                                   // JC: C = 1
            3 => !statusRegister.Carry,                                  // JNC: C = 0
            4 => statusRegister.Negative,                                // JN: N = 1
            5 => statusRegister.Negative == statusRegister.Overflow,     // JGE: N ⊕ V = 0
            6 => statusRegister.Negative != statusRegister.Overflow,     // JL: N ⊕ V = 1
            7 => true,                                                   // JMP: always
            _ => false // Invalid condition codes never jump
        };
    }
}
