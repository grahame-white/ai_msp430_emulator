namespace MSP430.Emulator.Cpu;

/// <summary>
/// Defines the names of the MSP430's 16 registers.
/// 
/// The MSP430 has 16 16-bit registers, with some having special functions:
/// - R0 (PC): Program Counter
/// - R1 (SP): Stack Pointer  
/// - R2 (SR/CG1): Status Register / Constant Generator #1
/// - R3 (CG2): Constant Generator #2
/// - R4-R15: General Purpose Registers
/// 
/// Implementation based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019,
/// Section 4.3: "CPU Registers".
/// See docs/references/SLAU445/4.3_cpu_registers.md for detailed specifications.
/// </summary>
public enum RegisterName : byte
{
    /// <summary>
    /// R0 - Program Counter (PC).
    /// Contains the address of the next instruction to be executed.
    /// </summary>
    R0 = 0,

    /// <summary>
    /// R1 - Stack Pointer (SP).
    /// Points to the top of the stack in memory.
    /// </summary>
    R1 = 1,

    /// <summary>
    /// R2 - Status Register (SR) / Constant Generator #1 (CG1).
    /// Contains processor status flags and control bits when used as SR.
    /// Generates constants +4 and +8 when used as CG1 in certain addressing modes.
    /// </summary>
    R2 = 2,

    /// <summary>
    /// R3 - Constant Generator #2 (CG2).
    /// Used by the CPU to generate common constants efficiently.
    /// </summary>
    R3 = 3,

    /// <summary>
    /// R4 - General Purpose Register.
    /// </summary>
    R4 = 4,

    /// <summary>
    /// R5 - General Purpose Register.
    /// </summary>
    R5 = 5,

    /// <summary>
    /// R6 - General Purpose Register.
    /// </summary>
    R6 = 6,

    /// <summary>
    /// R7 - General Purpose Register.
    /// </summary>
    R7 = 7,

    /// <summary>
    /// R8 - General Purpose Register.
    /// </summary>
    R8 = 8,

    /// <summary>
    /// R9 - General Purpose Register.
    /// </summary>
    R9 = 9,

    /// <summary>
    /// R10 - General Purpose Register.
    /// </summary>
    R10 = 10,

    /// <summary>
    /// R11 - General Purpose Register.
    /// </summary>
    R11 = 11,

    /// <summary>
    /// R12 - General Purpose Register.
    /// </summary>
    R12 = 12,

    /// <summary>
    /// R13 - General Purpose Register.
    /// </summary>
    R13 = 13,

    /// <summary>
    /// R14 - General Purpose Register.
    /// </summary>
    R14 = 14,

    /// <summary>
    /// R15 - General Purpose Register.
    /// </summary>
    R15 = 15,

    // Aliases for special function registers
    /// <summary>
    /// PC - Program Counter (alias for R0).
    /// </summary>
    PC = R0,

    /// <summary>
    /// SP - Stack Pointer (alias for R1).
    /// </summary>
    SP = R1,

    /// <summary>
    /// SR - Status Register (alias for R2).
    /// </summary>
    SR = R2,

    /// <summary>
    /// CG1 - Constant Generator #1 (alias for R2).
    /// </summary>
    CG1 = R2,

    /// <summary>
    /// CG2 - Constant Generator #2 (alias for R3).
    /// </summary>
    CG2 = R3
}
