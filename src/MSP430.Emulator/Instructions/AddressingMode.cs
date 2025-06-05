namespace MSP430.Emulator.Instructions;

/// <summary>
/// Defines the addressing modes supported by the MSP430 processor.
/// 
/// The MSP430 supports various addressing modes for flexible memory access:
/// - Register mode: Direct register access
/// - Indexed mode: Register + offset addressing
/// - Indirect mode: Register contains address
/// - Indirect auto-increment: Register contains address, then increments
/// - Immediate mode: Constant value follows instruction
/// - Absolute mode: Direct memory addressing
/// - Symbolic mode: PC-relative addressing
/// </summary>
public enum AddressingMode : byte
{
    /// <summary>
    /// Register mode (Rn).
    /// Direct register access - operand is in the register.
    /// As/Ad = 00
    /// </summary>
    Register = 0,

    /// <summary>
    /// Indexed mode (X(Rn)).
    /// Register + offset addressing - operand is at address Rn + X.
    /// As/Ad = 01
    /// </summary>
    Indexed = 1,

    /// <summary>
    /// Indirect register mode (@Rn).
    /// Register contains address - operand is at address contained in Rn.
    /// As/Ad = 10
    /// </summary>
    Indirect = 2,

    /// <summary>
    /// Indirect auto-increment mode (@Rn+).
    /// Register contains address, then increments - operand is at address in Rn, then Rn += 1/2.
    /// As/Ad = 11
    /// </summary>
    IndirectAutoIncrement = 3,

    /// <summary>
    /// Immediate mode (#N).
    /// Constant value follows instruction - operand is the immediate value.
    /// Special case: R0 (PC) with As = 11
    /// </summary>
    Immediate = 4,

    /// <summary>
    /// Absolute mode (&ADDR).
    /// Direct memory addressing - operand is at absolute address.
    /// Special case: R2 (SR) with As/Ad = 01
    /// </summary>
    Absolute = 5,

    /// <summary>
    /// Symbolic mode (ADDR).
    /// PC-relative addressing - operand is at PC + offset.
    /// Special case: R0 (PC) with As/Ad = 01
    /// </summary>
    Symbolic = 6,

    /// <summary>
    /// Invalid or unsupported addressing mode.
    /// Used for error handling and validation.
    /// </summary>
    Invalid = 255
}
