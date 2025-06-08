using System;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Cpu;

/// <summary>
/// Implements the MSP430X CPUX register file with 16 registers supporting 20-bit addressing.
/// 
/// Based on MSP430FR2xx FR4xx Family User's Guide (SLAU455) - Chapter 4: CPUX.
/// The MSP430X CPU can address a 1MB address range without paging and is completely 
/// backward compatible with the MSP430 CPU.
/// 
/// Provides access to all CPU registers including:
/// - R0 (PC): 20-bit Program Counter with automatic alignment (SLAU455 4.3.1)
/// - R1 (SP): Stack Pointer with word alignment
/// - R2 (SR): 16-bit Status Register with flag management (SLAU455 4.3.3)
/// - R3 (CG1): Constant Generator #1
/// - R4-R15: General Purpose Registers supporting 8-bit, 16-bit, and 20-bit values (SLAU455 4.3.5)
/// </summary>
public class RegisterFile : IRegisterFile
{
    // MSP430X CPUX registers support 20-bit values (SLAU455 4.3.5)
    private readonly uint[] _registers;
    private readonly StatusRegister _statusRegister;
    private readonly ILogger? _logger;

    /// <summary>
    /// Gets the Status Register (SR/R2) for direct flag manipulation.
    /// </summary>
    public StatusRegister StatusRegister => _statusRegister;

    /// <summary>
    /// Initializes a new instance of the RegisterFile class.
    /// </summary>
    /// <param name="logger">Optional logger for register access operations.</param>
    public RegisterFile(ILogger? logger = null)
    {
        _registers = new uint[16];
        _statusRegister = new StatusRegister();
        _logger = logger;

        // Initialize registers to their power-up state
        Reset();
    }

    /// <summary>
    /// Reads the 16-bit value from the specified register.
    /// For MSP430X CPUX registers containing 20-bit values, returns the lower 16 bits.
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The 16-bit value stored in the register (lower 16 bits for 20-bit registers).</returns>
    /// <exception cref="ArgumentException">Thrown when the register is invalid.</exception>
    public ushort ReadRegister(RegisterName register)
    {
        ValidateRegister(register);

        int index = (int)register;
        ushort value;

        // Handle special register behaviors
        switch (register)
        {
            case RegisterName.R2: // Status Register (always 16-bit per SLAU455 4.3.3)
                value = _statusRegister.Value;
                break;
            case RegisterName.R3: // Constant Generator #1
                // CG1 typically generates constants based on addressing mode
                // For basic register access, return stored value
                value = (ushort)(_registers[index] & 0xFFFF);
                break;
            default:
                // For MSP430X CPUX, return lower 16 bits of 20-bit register value
                value = (ushort)(_registers[index] & 0xFFFF);
                break;
        }

        LogRegisterAccess("read", register, value);
        return value;
    }

    /// <summary>
    /// Reads the full 20-bit value from the specified register.
    /// For MSP430X CPUX compatibility (SLAU455 4.3.5).
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The 20-bit value stored in the register.</returns>
    /// <exception cref="ArgumentException">Thrown when the register is invalid.</exception>
    public uint ReadRegister20Bit(RegisterName register)
    {
        ValidateRegister(register);

        int index = (int)register;
        uint value;

        // Handle special register behaviors
        switch (register)
        {
            case RegisterName.R2: // Status Register (always 16-bit per SLAU455 4.3.3)
                value = _statusRegister.Value;
                break;
            case RegisterName.R3: // Constant Generator #1
                value = _registers[index] & 0xFFFFF; // 20-bit mask
                break;
            default:
                value = _registers[index] & 0xFFFFF; // 20-bit mask
                break;
        }

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Register {register} 20-bit read: 0x{value:X5}",
                new { Register = register, Value = value });
        }

        return value;
    }
    /// <summary>
    /// Writes a 16-bit value to the specified register.
    /// For MSP430X CPUX general purpose registers (R4-R15), this clears bits 19:16 per SLAU455 4.3.5.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The 16-bit value to write.</param>
    /// <exception cref="ArgumentException">Thrown when the register is invalid.</exception>
    public void WriteRegister(RegisterName register, ushort value)
    {
        ValidateRegister(register);

        int index = (int)register;

        // Handle special register behaviors per MSP430X CPUX (SLAU455)
        switch (register)
        {
            case RegisterName.R0: // Program Counter (SLAU455 4.3.1)
                // PC should be word-aligned for instruction fetch
                // For 16-bit write, clear upper 4 bits (19:16) per SLAU455
                _registers[index] = (uint)(value & 0xFFFE);
                break;
            case RegisterName.R1: // Stack Pointer
                // SP should be word-aligned 
                // For 16-bit write, clear upper 4 bits (19:16)
                _registers[index] = (uint)(value & 0xFFFE);
                break;
            case RegisterName.R2: // Status Register (SLAU455 4.3.3)
                _statusRegister.Value = value;
                _registers[index] = value; // SR is always 16-bit
                break;
            case RegisterName.R3: // Constant Generator #1
                // CG1 is typically read-only in normal operation
                // For 16-bit write, clear upper 4 bits (19:16)
                _registers[index] = (uint)(value & 0xFFFF);
                break;
            default: // General Purpose Registers R4-R15 (SLAU455 4.3.5)
                // Word write to register clears bits 19:16
                _registers[index] = (uint)(value & 0xFFFF);
                break;
        }

        LogRegisterAccess("write", register, value);
    }

    /// <summary>
    /// Writes a 20-bit value to the specified register.
    /// For MSP430X CPUX address-word operations (SLAU455 4.3.5).
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The 20-bit value to write (only lower 20 bits used).</param>
    /// <exception cref="ArgumentException">Thrown when the register is invalid.</exception>
    public void WriteRegister20Bit(RegisterName register, uint value)
    {
        ValidateRegister(register);

        int index = (int)register;
        uint maskedValue = value & 0xFFFFF; // Ensure only 20 bits

        // Handle special register behaviors per MSP430X CPUX (SLAU455)
        switch (register)
        {
            case RegisterName.R0: // Program Counter (SLAU455 4.3.1)
                // PC should be word-aligned for instruction fetch
                _registers[index] = maskedValue & 0xFFFFE; // Clear bit 0
                break;
            case RegisterName.R1: // Stack Pointer
                // SP should be word-aligned 
                _registers[index] = maskedValue & 0xFFFFE; // Clear bit 0
                break;
            case RegisterName.R2: // Status Register (SLAU455 4.3.3)
                // Do not write 20-bit values to SR per SLAU455 - unpredictable operation
                throw new ArgumentException("Cannot write 20-bit values to Status Register (SLAU455 4.3.3)", nameof(value));
            case RegisterName.R3: // Constant Generator #1
                _registers[index] = maskedValue;
                break;
            default: // General Purpose Registers R4-R15 (SLAU455 4.3.5)
                _registers[index] = maskedValue;
                break;
        }

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Register {register} 20-bit write: 0x{maskedValue:X5}",
                new { Register = register, Value = maskedValue });
        }
    }

    /// <summary>
    /// Reads the low byte (bits 0-7) from the specified register.
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The low byte value.</returns>
    public byte ReadRegisterLowByte(RegisterName register)
    {
        ushort value = ReadRegister(register);
        byte lowByte = (byte)(value & 0xFF);

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Register {register} low byte read: 0x{lowByte:X2}",
                new { Register = register, LowByte = lowByte, FullValue = value });
        }

        return lowByte;
    }

    /// <summary>
    /// Reads the high byte (bits 8-15) from the specified register.
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The high byte value.</returns>
    public byte ReadRegisterHighByte(RegisterName register)
    {
        ushort value = ReadRegister(register);
        byte highByte = (byte)((value >> 8) & 0xFF);

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Register {register} high byte read: 0x{highByte:X2}",
                new { Register = register, HighByte = highByte, FullValue = value });
        }

        return highByte;
    }

    /// <summary>
    /// Writes a value to the low byte (bits 0-7) of the specified register.
    /// For MSP430X CPUX general purpose registers, this clears bits 19:8 per SLAU455 4.3.5.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The byte value to write to the low byte.</param>
    public void WriteRegisterLowByte(RegisterName register, byte value)
    {
        ValidateRegister(register);

        int index = (int)register;

        // Handle special register behaviors
        switch (register)
        {
            case RegisterName.R0: // Program Counter
                // For byte write, clear upper bits and maintain word alignment
                _registers[index] = (uint)(value & 0xFE); // Clear bit 0 for alignment
                break;
            case RegisterName.R1: // Stack Pointer  
                // For byte write, clear upper bits and maintain word alignment
                _registers[index] = (uint)(value & 0xFE); // Clear bit 0 for alignment
                break;
            case RegisterName.R2: // Status Register
                // Preserve high byte of status register, update low byte
                ushort currentValue = _statusRegister.Value;
                ushort newValue = (ushort)((currentValue & 0xFF00) | value);
                _statusRegister.Value = newValue;
                _registers[index] = newValue;
                break;
            case RegisterName.R3: // Constant Generator #1
                // Byte write clears bits 19:8 per SLAU455 4.3.5
                _registers[index] = (uint)value;
                break;
            default: // General Purpose Registers R4-R15 (SLAU455 4.3.5)
                // Any byte-write to a CPU register clears bits 19:8
                _registers[index] = (uint)value;
                break;
        }

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Register {register} low byte written: 0x{value:X2}",
                new { Register = register, LowByte = value, NewValue = _registers[index] });
        }
    }

    /// <summary>
    /// Writes a value to the high byte (bits 8-15) of the specified register.
    /// For MSP430X CPUX general purpose registers, this clears bits 19:16 per SLAU455 4.3.5.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The byte value to write to the high byte.</param>
    public void WriteRegisterHighByte(RegisterName register, byte value)
    {
        ValidateRegister(register);

        int index = (int)register;

        // Handle special register behaviors
        switch (register)
        {
            case RegisterName.R0: // Program Counter
                // For high byte write, preserve low byte and clear upper bits
                uint currentLowByte = _registers[index] & 0x00FF;
                _registers[index] = currentLowByte | ((uint)value << 8);
                break;
            case RegisterName.R2: // Status Register
                // Preserve low byte of status register, update high byte
                ushort currentValue = _statusRegister.Value;
                ushort newValue = (ushort)((currentValue & 0x00FF) | (value << 8));
                _statusRegister.Value = newValue;
                _registers[index] = newValue;
                break;
            default: // R1 Stack Pointer, R3 Constant Generator, R4-R15 General Purpose (SLAU455 4.3.5)
                // High byte write creates 16-bit value, clears bits 19:16
                currentLowByte = _registers[index] & 0x00FF;
                _registers[index] = currentLowByte | ((uint)value << 8);
                break;
        }

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Register {register} high byte written: 0x{value:X2}",
                new { Register = register, HighByte = value, NewValue = _registers[index] });
        }
    }

    /// <summary>
    /// Gets the Program Counter (PC/R0) value.
    /// </summary>
    /// <returns>The current program counter value (16-bit for backward compatibility).</returns>
    public ushort GetProgramCounter()
    {
        return (ushort)(_registers[0] & 0xFFFF);
    }

    /// <summary>
    /// Gets the full 20-bit Program Counter (PC/R0) value.
    /// For MSP430X CPUX compatibility (SLAU455 4.3.1).
    /// </summary>
    /// <returns>The current 20-bit program counter value.</returns>
    public uint GetProgramCounter20Bit()
    {
        return _registers[0] & 0xFFFFF;
    }

    /// <summary>
    /// Sets the Program Counter (PC/R0) value.
    /// </summary>
    /// <param name="address">The new program counter value.</param>
    public void SetProgramCounter(ushort address)
    {
        WriteRegister(RegisterName.R0, address);
    }

    /// <summary>
    /// Sets the 20-bit Program Counter (PC/R0) value.
    /// For MSP430X CPUX compatibility (SLAU455 4.3.1).
    /// </summary>
    /// <param name="address">The new 20-bit program counter value.</param>
    public void SetProgramCounter20Bit(uint address)
    {
        WriteRegister20Bit(RegisterName.R0, address);
    }

    /// <summary>
    /// Increments the Program Counter by the specified amount.
    /// Typically used for instruction fetch operations.
    /// </summary>
    /// <param name="increment">The amount to increment (default is 2 for word alignment).</param>
    public void IncrementProgramCounter(ushort increment = 2)
    {
        uint currentPc = _registers[0] & 0xFFFFF; // 20-bit mask
        uint newPc = (currentPc + increment) & 0xFFFFF; // Keep within 20-bit range
        _registers[0] = newPc & 0xFFFFE; // Ensure word alignment

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Program Counter incremented by {increment}: 0x{currentPc:X5} -> 0x{newPc:X5}",
                new { Increment = increment, PreviousPC = currentPc, NewPC = newPc });
        }
    }

    /// <summary>
    /// Gets the Stack Pointer (SP/R1) value.
    /// </summary>
    /// <returns>The current stack pointer value (16-bit for backward compatibility).</returns>
    public ushort GetStackPointer()
    {
        return (ushort)(_registers[1] & 0xFFFF);
    }

    /// <summary>
    /// Sets the Stack Pointer (SP/R1) value.
    /// The value will be aligned to word boundaries as required by the MSP430.
    /// </summary>
    /// <param name="address">The new stack pointer value.</param>
    public void SetStackPointer(ushort address)
    {
        WriteRegister(RegisterName.R1, address);
    }

    /// <summary>
    /// Resets all registers to their default power-up state.
    /// </summary>
    public void Reset()
    {
        // Clear all registers
        Array.Clear(_registers, 0, _registers.Length);

        // Reset status register
        _statusRegister.Reset();

        // Set typical power-up values
        // PC is typically initialized by hardware/bootloader
        // SP is typically initialized to top of RAM
        // Other registers start at 0

        _logger?.Info("Register file reset - all registers cleared to 0x0000");
    }

    /// <summary>
    /// Validates that the specified register name is valid.
    /// </summary>
    /// <param name="register">The register name to validate.</param>
    /// <returns>True if the register is valid, false otherwise.</returns>
    public bool IsValidRegister(RegisterName register)
    {
        int value = (int)register;
        return value >= 0 && value <= 15;
    }

    /// <summary>
    /// Gets all register values for debugging or state inspection purposes.
    /// </summary>
    /// <returns>An array of 16 register values (R0-R15) - 16-bit values for backward compatibility.</returns>
    public ushort[] GetAllRegisters()
    {
        ushort[] snapshot = new ushort[16];
        for (int i = 0; i < 16; i++)
        {
            snapshot[i] = (ushort)(_registers[i] & 0xFFFF);
        }

        // Ensure SR value is current
        snapshot[2] = _statusRegister.Value;

        return snapshot;
    }

    /// <summary>
    /// Gets all register values as 20-bit values for debugging or state inspection purposes.
    /// For MSP430X CPUX compatibility (SLAU455).
    /// </summary>
    /// <returns>An array of 16 register values (R0-R15) as 20-bit values.</returns>
    public uint[] GetAllRegisters20Bit()
    {
        uint[] snapshot = new uint[16];
        for (int i = 0; i < 16; i++)
        {
            snapshot[i] = _registers[i] & 0xFFFFF;
        }

        // Ensure SR value is current (still 16-bit)
        snapshot[2] = _statusRegister.Value;

        return snapshot;
    }

    /// <summary>
    /// Validates the register parameter and throws an exception if invalid.
    /// </summary>
    /// <param name="register">The register to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the register is invalid.</exception>
    private void ValidateRegister(RegisterName register)
    {
        if (!IsValidRegister(register))
        {
            string message = $"Invalid register: {register} (value: {(int)register})";
            _logger?.Error(message, new { Register = register, RegisterValue = (int)register });
            throw new ArgumentException(message, nameof(register));
        }
    }

    /// <summary>
    /// Logs register access operations when debug logging is enabled.
    /// </summary>
    /// <param name="operation">The type of operation (read/write).</param>
    /// <param name="register">The register being accessed.</param>
    /// <param name="value">The value being read or written.</param>
    private void LogRegisterAccess(string operation, RegisterName register, ushort value)
    {
        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Register {operation}: {register} = 0x{value:X4}",
                new { Operation = operation, Register = register, Value = value });
        }
    }
}
