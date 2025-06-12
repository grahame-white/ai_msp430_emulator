namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Defines the conditional jump codes for Format III instructions.
/// 
/// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019,
/// Section 4.5.1.3: "Jump Instructions" - Table 4-11 and Figure 4-16.
/// 
/// Each 3-bit condition code (bits 12:10 in Format III instruction) determines
/// which status register flags are tested for the conditional jump.
/// </summary>
public enum JumpCondition : byte
{
    /// <summary>
    /// Jump if equal/zero (JEQ/JZ) - Test: Z = 1
    /// Jump when the Zero flag is set.
    /// </summary>
    JEQ = 0,

    /// <summary>
    /// Jump if not equal/not zero (JNE/JNZ) - Test: Z = 0
    /// Jump when the Zero flag is clear.
    /// </summary>
    JNE = 1,

    /// <summary>
    /// Jump if carry set (JC) - Test: C = 1
    /// Jump when the Carry flag is set.
    /// </summary>
    JC = 2,

    /// <summary>
    /// Jump if carry clear (JNC) - Test: C = 0
    /// Jump when the Carry flag is clear.
    /// </summary>
    JNC = 3,

    /// <summary>
    /// Jump if negative (JN) - Test: N = 1
    /// Jump when the Negative flag is set.
    /// </summary>
    JN = 4,

    /// <summary>
    /// Jump if greater or equal (JGE) - Test: N ⊕ V = 0
    /// Jump when (Negative XOR Overflow) equals 0.
    /// Used for signed comparisons.
    /// </summary>
    JGE = 5,

    /// <summary>
    /// Jump if less than (JL) - Test: N ⊕ V = 1
    /// Jump when (Negative XOR Overflow) equals 1.
    /// Used for signed comparisons.
    /// </summary>
    JL = 6,

    /// <summary>
    /// Jump unconditional (JMP) - Test: Always
    /// Always jump regardless of flag states.
    /// </summary>
    JMP = 7
}