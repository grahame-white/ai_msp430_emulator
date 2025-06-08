namespace MSP430.Emulator.Cpu;

/// <summary>
/// Defines the contract for the MSP430X CPUX register file operations.
/// 
/// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU455) - Chapter 4: CPUX.
/// The register file provides access to the 16 registers (R0-R15) supporting both
/// 16-bit backward compatibility and 20-bit MSP430X CPUX operations.
/// </summary>
public interface IRegisterFile
{
    /// <summary>
    /// Gets the Status Register (SR/R2) for direct flag manipulation.
    /// </summary>
    StatusRegister StatusRegister { get; }

    /// <summary>
    /// Reads the 16-bit value from the specified register.
    /// For MSP430X CPUX registers containing 20-bit values, returns the lower 16 bits.
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The 16-bit value stored in the register (lower 16 bits for 20-bit registers).</returns>
    ushort ReadRegister(RegisterName register);

    /// <summary>
    /// Reads the full 20-bit value from the specified register.
    /// For MSP430X CPUX compatibility (SLAU455 4.3.5).
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The 20-bit value stored in the register.</returns>
    uint ReadRegister20Bit(RegisterName register);

    /// <summary>
    /// Writes a 16-bit value to the specified register.
    /// For MSP430X CPUX general purpose registers (R4-R15), this clears bits 19:16 per SLAU455 4.3.5.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The 16-bit value to write.</param>
    void WriteRegister(RegisterName register, ushort value);

    /// <summary>
    /// Writes a 20-bit value to the specified register.
    /// For MSP430X CPUX address-word operations (SLAU455 4.3.5).
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The 20-bit value to write (only lower 20 bits used).</param>
    void WriteRegister20Bit(RegisterName register, uint value);

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
    /// For MSP430X CPUX general purpose registers, this clears bits 19:8 per SLAU455 4.3.5.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The byte value to write to the low byte.</param>
    void WriteRegisterLowByte(RegisterName register, byte value);

    /// <summary>
    /// Writes a value to the high byte (bits 8-15) of the specified register.
    /// For MSP430X CPUX general purpose registers, this clears bits 19:16 per SLAU455 4.3.5.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The byte value to write to the high byte.</param>
    void WriteRegisterHighByte(RegisterName register, byte value);

    /// <summary>
    /// Gets the Program Counter (PC/R0) value.
    /// </summary>
    /// <returns>The current program counter value (16-bit for backward compatibility).</returns>
    ushort GetProgramCounter();

    /// <summary>
    /// Gets the full 20-bit Program Counter (PC/R0) value.
    /// For MSP430X CPUX compatibility (SLAU455 4.3.1).
    /// </summary>
    /// <returns>The current 20-bit program counter value.</returns>
    uint GetProgramCounter20Bit();

    /// <summary>
    /// Sets the Program Counter (PC/R0) value.
    /// </summary>
    /// <param name="address">The new program counter value.</param>
    void SetProgramCounter(ushort address);

    /// <summary>
    /// Sets the 20-bit Program Counter (PC/R0) value.
    /// For MSP430X CPUX compatibility (SLAU455 4.3.1).
    /// </summary>
    /// <param name="address">The new 20-bit program counter value.</param>
    void SetProgramCounter20Bit(uint address);

    /// <summary>
    /// Increments the Program Counter by the specified amount.
    /// Typically used for instruction fetch operations.
    /// </summary>
    /// <param name="increment">The amount to increment (default is 2 for word alignment).</param>
    void IncrementProgramCounter(ushort increment = 2);

    /// <summary>
    /// Gets the Stack Pointer (SP/R1) value.
    /// </summary>
    /// <returns>The current stack pointer value (16-bit for backward compatibility).</returns>
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
    /// <returns>An array of 16 register values (R0-R15) - 16-bit values for backward compatibility.</returns>
    ushort[] GetAllRegisters();

    /// <summary>
    /// Gets all register values as 20-bit values for debugging or state inspection purposes.
    /// For MSP430X CPUX compatibility (SLAU455).
    /// </summary>
    /// <returns>An array of 16 register values (R0-R15) as 20-bit values.</returns>
    uint[] GetAllRegisters20Bit();
}
