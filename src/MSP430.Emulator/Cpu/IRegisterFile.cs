namespace MSP430.Emulator.Cpu;

/// <summary>
/// Defines the contract for the MSP430 register file operations.
/// 
/// The register file provides access to the 16 16-bit registers (R0-R15)
/// with special behavior for certain registers (PC, SP, SR/CG1, CG2).
/// 
/// Implementation based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019,
/// Section 4.3: "CPU Registers".
/// </summary>
public interface IRegisterFile
{
    /// <summary>
    /// Gets the Status Register (SR/R2) for direct flag manipulation.
    /// </summary>
    StatusRegister StatusRegister { get; }

    /// <summary>
    /// Reads the 16-bit value from the specified register.
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The 16-bit value stored in the register.</returns>
    ushort ReadRegister(RegisterName register);

    /// <summary>
    /// Writes a 16-bit value to the specified register.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The 16-bit value to write.</param>
    void WriteRegister(RegisterName register, ushort value);

    /// <summary>
    /// Reads the low byte (bits 0-7) from the specified register.
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The low byte value.</returns>
    byte ReadRegisterLowByte(RegisterName register);

    /// <summary>
    /// Reads the high byte (bits 8-15) from the specified register.
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The high byte value.</returns>
    byte ReadRegisterHighByte(RegisterName register);

    /// <summary>
    /// Writes a value to the low byte (bits 0-7) of the specified register.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The byte value to write to the low byte.</param>
    void WriteRegisterLowByte(RegisterName register, byte value);

    /// <summary>
    /// Writes a value to the high byte (bits 8-15) of the specified register.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The byte value to write to the high byte.</param>
    void WriteRegisterHighByte(RegisterName register, byte value);

    /// <summary>
    /// Gets the Program Counter (PC/R0) value.
    /// </summary>
    /// <returns>The current program counter value.</returns>
    ushort GetProgramCounter();

    /// <summary>
    /// Sets the Program Counter (PC/R0) value.
    /// </summary>
    /// <param name="address">The new program counter value.</param>
    void SetProgramCounter(ushort address);

    /// <summary>
    /// Increments the Program Counter by the specified amount.
    /// Typically used for instruction fetch operations.
    /// </summary>
    /// <param name="increment">The amount to increment (default is 2 for word alignment).</param>
    void IncrementProgramCounter(ushort increment = 2);

    /// <summary>
    /// Gets the Stack Pointer (SP/R1) value.
    /// </summary>
    /// <returns>The current stack pointer value.</returns>
    ushort GetStackPointer();

    /// <summary>
    /// Sets the Stack Pointer (SP/R1) value.
    /// The value will be aligned to word boundaries as required by the MSP430.
    /// </summary>
    /// <param name="address">The new stack pointer value.</param>
    void SetStackPointer(ushort address);

    /// <summary>
    /// Resets all registers to their default power-up state.
    /// </summary>
    void Reset();

    /// <summary>
    /// Validates that the specified register name is valid.
    /// </summary>
    /// <param name="register">The register name to validate.</param>
    /// <returns>True if the register is valid, false otherwise.</returns>
    bool IsValidRegister(RegisterName register);

    /// <summary>
    /// Gets all register values for debugging or state inspection purposes.
    /// </summary>
    /// <returns>An array of 16 register values (R0-R15).</returns>
    ushort[] GetAllRegisters();
}
