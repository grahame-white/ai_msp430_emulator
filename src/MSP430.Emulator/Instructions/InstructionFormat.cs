namespace MSP430.Emulator.Instructions;

/// <summary>
/// Defines the three main instruction formats supported by the MSP430 processor.
/// 
/// The MSP430 instruction set architecture uses three distinct formats:
/// - Format I: Two-operand instructions (15-bit opcode space)
/// - Format II: Single-operand instructions (9-bit opcode space) 
/// - Format III: Jump instructions (10-bit opcode space)
/// </summary>
public enum InstructionFormat : byte
{
    /// <summary>
    /// Format I - Two-operand instructions.
    /// 
    /// Examples: MOV, ADD, SUB, CMP, BIT, BIC, BIS, XOR
    /// Bit pattern: [15:12] opcode, [11:8] source reg, [7] Ad, [6] B/W, [5:4] As, [3:0] dest reg
    /// </summary>
    FormatI = 1,

    /// <summary>
    /// Format II - Single-operand instructions.
    /// 
    /// Examples: RRC, RRA, PUSH, POP, CALL, RETI, SWPB
    /// Bit pattern: [15:7] opcode (001xxxx), [6] B/W, [5:4] As, [3:0] source/dest reg
    /// </summary>
    FormatII = 2,

    /// <summary>
    /// Format III - Jump instructions (conditional and unconditional).
    /// 
    /// Examples: JEQ, JNE, JC, JNC, JN, JGE, JL, JMP
    /// Bit pattern: [15:13] opcode (001), [12:10] condition, [9:0] 10-bit signed offset
    /// </summary>
    FormatIII = 3
}
