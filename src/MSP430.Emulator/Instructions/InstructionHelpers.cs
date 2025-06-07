using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions;

/// <summary>
/// Provides helper methods for instruction operand formatting and manipulation.
/// 
/// Contains shared functionality used across multiple instruction implementations
/// to avoid code duplication and ensure consistent behavior.
/// </summary>
public static class InstructionHelpers
{
    /// <summary>
    /// Formats an operand for display based on its register and addressing mode.
    /// </summary>
    /// <param name="register">The register.</param>
    /// <param name="addressingMode">The addressing mode.</param>
    /// <returns>A formatted string representation of the operand.</returns>
    public static string FormatOperand(RegisterName register, AddressingMode addressingMode)
    {
        return addressingMode switch
        {
            AddressingMode.Register => $"R{(int)register}",
            AddressingMode.Indexed => $"X(R{(int)register})",
            AddressingMode.Indirect => $"@R{(int)register}",
            AddressingMode.IndirectAutoIncrement => $"@R{(int)register}+",
            AddressingMode.Immediate => "#N",
            AddressingMode.Absolute => "&ADDR",
            AddressingMode.Symbolic => "ADDR",
            _ => "?"
        };
    }

    /// <summary>
    /// Calculates the number of extension words required by a Format I instruction.
    /// </summary>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <returns>The number of extension words required (0-2).</returns>
    public static int CalculateFormatIExtensionWordCount(AddressingMode sourceAddressingMode, AddressingMode destinationAddressingMode)
    {
        int count = 0;

        // Source operand extension words
        if (sourceAddressingMode == AddressingMode.Immediate ||
            sourceAddressingMode == AddressingMode.Absolute ||
            sourceAddressingMode == AddressingMode.Symbolic ||
            sourceAddressingMode == AddressingMode.Indexed)
        {
            count++;
        }

        // Destination operand extension words
        if (destinationAddressingMode == AddressingMode.Absolute ||
            destinationAddressingMode == AddressingMode.Symbolic ||
            destinationAddressingMode == AddressingMode.Indexed)
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Reads an operand value based on the addressing mode.
    /// </summary>
    /// <param name="register">The register used by the addressing mode.</param>
    /// <param name="addressingMode">The addressing mode.</param>
    /// <param name="isByteOperation">True for byte operations, false for word operations.</param>
    /// <param name="registerFile">The CPU register file.</param>
    /// <param name="memory">The system memory.</param>
    /// <param name="extensionWord">Extension word for indexed, absolute, symbolic, or immediate modes.</param>
    /// <returns>The value read from the operand.</returns>
    public static ushort ReadOperand(
        RegisterName register,
        AddressingMode addressingMode,
        bool isByteOperation,
        IRegisterFile registerFile,
        byte[] memory,
        ushort extensionWord)
    {
        switch (addressingMode)
        {
            case AddressingMode.Register:
                return isByteOperation
                    ? (ushort)(registerFile.ReadRegister(register) & 0xFF)
                    : registerFile.ReadRegister(register);

            case AddressingMode.Indirect:
                return ReadFromMemory(registerFile.ReadRegister(register), isByteOperation, memory);

            case AddressingMode.IndirectAutoIncrement:
                ushort address = registerFile.ReadRegister(register);
                ushort value = ReadFromMemory(address, isByteOperation, memory);
                // Post-increment by 1 for byte operations, 2 for word operations
                registerFile.WriteRegister(register, (ushort)(address + (isByteOperation ? 1 : 2)));
                return value;

            case AddressingMode.Indexed:
                return ReadFromMemory((ushort)(registerFile.ReadRegister(register) + extensionWord), isByteOperation, memory);

            case AddressingMode.Absolute:
                return ReadFromMemory(extensionWord, isByteOperation, memory);

            case AddressingMode.Symbolic:
                return ReadFromMemory((ushort)(registerFile.GetProgramCounter() + extensionWord), isByteOperation, memory);

            case AddressingMode.Immediate:
                return isByteOperation ? (ushort)(extensionWord & 0xFF) : extensionWord;

            default:
                throw new ArgumentException($"Unsupported addressing mode: {addressingMode}");
        }
    }

    /// <summary>
    /// Writes an operand value based on the addressing mode.
    /// </summary>
    /// <param name="register">The register used by the addressing mode.</param>
    /// <param name="addressingMode">The addressing mode.</param>
    /// <param name="isByteOperation">True for byte operations, false for word operations.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="registerFile">The CPU register file.</param>
    /// <param name="memory">The system memory.</param>
    /// <param name="extensionWord">Extension word for indexed, absolute, or symbolic modes.</param>
    public static void WriteOperand(
        RegisterName register,
        AddressingMode addressingMode,
        bool isByteOperation,
        ushort value,
        IRegisterFile registerFile,
        byte[] memory,
        ushort extensionWord)
    {
        switch (addressingMode)
        {
            case AddressingMode.Register:
                // For byte operations, only write to the low byte, preserve high byte
                ushort valueToWrite = isByteOperation
                    ? (ushort)((registerFile.ReadRegister(register) & 0xFF00) | (value & 0xFF))
                    : value;
                registerFile.WriteRegister(register, valueToWrite);
                break;

            case AddressingMode.Indirect:
                WriteToMemory(registerFile.ReadRegister(register), value, isByteOperation, memory);
                break;

            case AddressingMode.IndirectAutoIncrement:
                ushort address = registerFile.ReadRegister(register);
                WriteToMemory(address, value, isByteOperation, memory);
                // Post-increment by 1 for byte operations, 2 for word operations
                registerFile.WriteRegister(register, (ushort)(address + (isByteOperation ? 1 : 2)));
                break;

            case AddressingMode.Indexed:
                WriteToMemory((ushort)(registerFile.ReadRegister(register) + extensionWord), value, isByteOperation, memory);
                break;

            case AddressingMode.Absolute:
                WriteToMemory(extensionWord, value, isByteOperation, memory);
                break;

            case AddressingMode.Symbolic:
                WriteToMemory((ushort)(registerFile.GetProgramCounter() + extensionWord), value, isByteOperation, memory);
                break;

            default:
                throw new ArgumentException($"Cannot write to addressing mode: {addressingMode}");
        }
    }

    /// <summary>
    /// Updates CPU flags based on the result of an arithmetic operation.
    /// </summary>
    /// <param name="result">The result value.</param>
    /// <param name="carry">Whether a carry/borrow occurred.</param>
    /// <param name="overflow">Whether an overflow occurred.</param>
    /// <param name="isByteOperation">True for byte operations, false for word operations.</param>
    /// <param name="statusRegister">The CPU status register to update.</param>
    public static void UpdateFlags(ushort result, bool carry, bool overflow, bool isByteOperation, StatusRegister statusRegister)
    {
        // Zero flag: Set if result is zero
        statusRegister.Zero = result == 0;

        // Negative flag: Set if the sign bit is set
        statusRegister.Negative = (result & (isByteOperation ? 0x80 : 0x8000)) != 0;

        // Carry flag
        statusRegister.Carry = carry;

        // Overflow flag
        statusRegister.Overflow = overflow;
    }

    private static ushort ReadFromMemory(ushort address, bool isByteOperation, byte[] memory)
    {
        if (address >= memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(address), "Memory address out of range");
        }

        if (isByteOperation)
        {
            return memory[address];
        }
        else
        {
            if (address + 1 >= memory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Memory address out of range for word access");
            }

            // MSP430 is little-endian
            return (ushort)(memory[address] | (memory[address + 1] << 8));
        }
    }

    private static void WriteToMemory(ushort address, ushort value, bool isByteOperation, byte[] memory)
    {
        if (address >= memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(address), "Memory address out of range");
        }

        if (isByteOperation)
        {
            memory[address] = (byte)(value & 0xFF);
        }
        else
        {
            if (address + 1 >= memory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Memory address out of range for word access");
            }

            // MSP430 is little-endian
            memory[address] = (byte)(value & 0xFF);
            memory[address + 1] = (byte)((value >> 8) & 0xFF);
        }
    }

    /// <summary>
    /// Gets the number of CPU cycles required for a single-operand instruction based on addressing mode.
    /// </summary>
    /// <param name="addressingMode">The addressing mode.</param>
    /// <returns>The number of CPU cycles.</returns>
    public static uint GetSingleOperandCycleCount(AddressingMode addressingMode)
    {
        return addressingMode switch
        {
            AddressingMode.Register => 1,
            AddressingMode.Indexed => 4,
            AddressingMode.Indirect => 3,
            AddressingMode.IndirectAutoIncrement => 3,
            AddressingMode.Absolute => 4,
            AddressingMode.Symbolic => 4,
            _ => 1
        };
    }

    /// <summary>
    /// Sign-extends an 8-bit value to 16 bits.
    /// Used by PUSH.B and POP.B instructions to maintain consistency in byte operations.
    /// </summary>
    /// <param name="byteValue">The 8-bit value to sign-extend.</param>
    /// <returns>The sign-extended 16-bit value.</returns>
    public static ushort SignExtendByte(ushort byteValue)
    {
        return (ushort)((sbyte)(byte)(byteValue & 0xFF));
    }
}
