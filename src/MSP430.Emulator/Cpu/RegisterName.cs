namespace MSP430.Emulator.Cpu;

/// <summary>
/// Defines the names of the MSP430X CPUX 16 registers.
/// 
/// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU455) - Chapter 4: CPUX.
/// The MSP430X CPU has 16 registers that can contain 8-bit, 16-bit, or 20-bit values:
/// - R0 (PC): 20-bit Program Counter (SLAU455 4.3.1)
/// - R1 (SP): Stack Pointer  
/// - R2 (SR): 16-bit Status Register (SLAU455 4.3.3)
/// - R3 (CG1): Constant Generator #1
/// - R4-R15: General Purpose Registers supporting 20-bit addressing (SLAU455 4.3.5)
/// </summary>
public enum RegisterName : byte
{
    /// <summary>
    /// R0 - Program Counter (PC).
    /// 20-bit register that points to the next instruction to be executed (SLAU455 4.3.1).
    /// </summary>
    R0 = 0,

    /// <summary>
    /// R1 - Stack Pointer (SP).
    /// Points to the top of the stack in memory.
    /// </summary>
    R1 = 1,

    /// <summary>
    /// R2 - Status Register (SR).
    /// 16-bit register containing processor status flags and control bits (SLAU455 4.3.3).
    /// </summary>
    R2 = 2,

    /// <summary>
    /// R3 - Constant Generator #1 (CG1).
    /// Used by the CPU to generate common constants efficiently.
    /// </summary>
    R3 = 3,

    /// <summary>
    /// R4 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R4 = 4,

    /// <summary>
    /// R5 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R5 = 5,

    /// <summary>
    /// R6 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R6 = 6,

    /// <summary>
    /// R7 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R7 = 7,

    /// <summary>
    /// R8 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R8 = 8,

    /// <summary>
    /// R9 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R9 = 9,

    /// <summary>
    /// R10 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R10 = 10,

    /// <summary>
    /// R11 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R11 = 11,

    /// <summary>
    /// R12 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R12 = 12,

    /// <summary>
    /// R13 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R13 = 13,

    /// <summary>
    /// R14 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R14 = 14,

    /// <summary>
    /// R15 - General Purpose Register.
    /// Supports 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5).
    /// </summary>
    R15 = 15,

    // Aliases for special function registers
    /// <summary>
    /// PC - Program Counter (alias for R0).
    /// 20-bit register for MSP430X CPUX (SLAU455 4.3.1).
    /// </summary>
    PC = R0,

    /// <summary>
    /// SP - Stack Pointer (alias for R1).
    /// </summary>
    SP = R1,

    /// <summary>
    /// SR - Status Register (alias for R2).
    /// 16-bit register for MSP430X CPUX (SLAU455 4.3.3).
    /// </summary>
    SR = R2,

    /// <summary>
    /// CG1 - Constant Generator #1 (alias for R3).
    /// </summary>
    CG1 = R3
}
