using System;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Cpu;

/// <summary>
/// Implements the MSP430 register file with 16 16-bit registers and special register behaviors.
/// 
/// Provides access to all CPU registers including:
/// - R0 (PC): Program Counter with automatic alignment
/// - R1 (SP): Stack Pointer with word alignment
/// - R2 (SR): Status Register with flag management
/// - R3 (CG1): Constant Generator #1
/// - R4-R15: General Purpose Registers
/// </summary>
public class RegisterFile : IRegisterFile
{
    private readonly ushort[] _registers;
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
        _registers = new ushort[16];
        _statusRegister = new StatusRegister();
        _logger = logger;

        // Initialize registers to their power-up state
        Reset();
    }

    /// <summary>
    /// Reads the 16-bit value from the specified register.
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>The 16-bit value stored in the register.</returns>
    /// <exception cref="ArgumentException">Thrown when the register is invalid.</exception>
    public ushort ReadRegister(RegisterName register)
    {
        ValidateRegister(register);

        int index = (int)register;
        ushort value;

        // Handle special register behaviors
        switch (register)
        {
            case RegisterName.R2: // Status Register
                value = _statusRegister.Value;
                break;
            case RegisterName.R3: // Constant Generator #1
                // CG1 typically generates constants based on addressing mode
                // For basic register access, return stored value
                value = _registers[index];
                break;
            default:
                value = _registers[index];
                break;
        }

        LogRegisterAccess("read", register, value);
        return value;
    }

    /// <summary>
    /// Writes a 16-bit value to the specified register.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The 16-bit value to write.</param>
    /// <exception cref="ArgumentException">Thrown when the register is invalid.</exception>
    public void WriteRegister(RegisterName register, ushort value)
    {
        ValidateRegister(register);

        int index = (int)register;

        // Handle special register behaviors
        switch (register)
        {
            case RegisterName.R0: // Program Counter
                // PC should be word-aligned for instruction fetch
                _registers[index] = (ushort)(value & 0xFFFE);
                break;
            case RegisterName.R1: // Stack Pointer
                // SP should be word-aligned 
                _registers[index] = (ushort)(value & 0xFFFE);
                break;
            case RegisterName.R2: // Status Register
                _statusRegister.Value = value;
                _registers[index] = value;
                break;
            case RegisterName.R3: // Constant Generator #1
                // CG1 is typically read-only in normal operation
                // But allow writes for completeness and testing
                _registers[index] = value;
                break;
            default:
                _registers[index] = value;
                break;
        }

        LogRegisterAccess("write", register, value);
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
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The byte value to write to the low byte.</param>
    public void WriteRegisterLowByte(RegisterName register, byte value)
    {
        ushort currentValue = ReadRegister(register);
        ushort newValue = (ushort)((currentValue & 0xFF00) | value);
        WriteRegister(register, newValue);

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Register {register} low byte written: 0x{value:X2}",
                new { Register = register, LowByte = value, NewValue = newValue, PreviousValue = currentValue });
        }
    }

    /// <summary>
    /// Writes a value to the high byte (bits 8-15) of the specified register.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The byte value to write to the high byte.</param>
    public void WriteRegisterHighByte(RegisterName register, byte value)
    {
        ushort currentValue = ReadRegister(register);
        ushort newValue = (ushort)((currentValue & 0x00FF) | (value << 8));
        WriteRegister(register, newValue);

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Register {register} high byte written: 0x{value:X2}",
                new { Register = register, HighByte = value, NewValue = newValue, PreviousValue = currentValue });
        }
    }

    /// <summary>
    /// Gets the Program Counter (PC/R0) value.
    /// </summary>
    /// <returns>The current program counter value.</returns>
    public ushort GetProgramCounter()
    {
        return _registers[0];
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
    /// Increments the Program Counter by the specified amount.
    /// Typically used for instruction fetch operations.
    /// </summary>
    /// <param name="increment">The amount to increment (default is 2 for word alignment).</param>
    public void IncrementProgramCounter(ushort increment = 2)
    {
        ushort currentPc = _registers[0];
        ushort newPc = (ushort)(currentPc + increment);
        WriteRegister(RegisterName.R0, newPc);

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.Debug($"Program Counter incremented by {increment}: 0x{currentPc:X4} -> 0x{newPc:X4}",
                new { Increment = increment, PreviousPC = currentPc, NewPC = newPc });
        }
    }

    /// <summary>
    /// Gets the Stack Pointer (SP/R1) value.
    /// </summary>
    /// <returns>The current stack pointer value.</returns>
    public ushort GetStackPointer()
    {
        return _registers[1];
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
    /// <returns>An array of 16 register values (R0-R15).</returns>
    public ushort[] GetAllRegisters()
    {
        ushort[] snapshot = new ushort[16];
        Array.Copy(_registers, snapshot, 16);

        // Ensure SR value is current
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
