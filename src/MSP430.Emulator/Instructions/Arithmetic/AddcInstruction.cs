using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Arithmetic;

/// <summary>
/// Implements the MSP430 ADDC (Add with Carry) instruction.
/// 
/// The ADDC instruction adds the source operand and the carry bit to the destination operand.
/// This is a Format I (two-operand) instruction that supports all addressing modes.
/// 
/// Operation: dst = src + dst + C
/// Format: ADDC(.B) src, dst
/// Opcode: 0x6 (Format I)
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.1: ADDC
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.1: Format I Instructions
/// </summary>
public class AddcInstruction : ArithmeticInstruction
{
    /// <summary>
    /// Initializes a new instance of the AddcInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public AddcInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(0x6, instructionWord, sourceRegister, destinationRegister,
               sourceAddressingMode, destinationAddressingMode, isByteOperation)
    {
    }

    /// <summary>
    /// Gets the base mnemonic for the ADDC instruction.
    /// </summary>
    protected override string BaseMnemonic => "ADDC";

    /// <summary>
    /// Performs the ADDC arithmetic operation.
    /// Adds the source operand and the carry bit to the destination operand.
    /// NOTE: Current implementation does not include carry input. This will be enhanced
    /// in a future version to properly access the status register carry flag.
    /// </summary>
    /// <param name="sourceValue">The source operand value.</param>
    /// <param name="destinationValue">The destination operand value.</param>
    /// <param name="isByteOperation">True for byte operations, false for word operations.</param>
    /// <returns>A tuple containing the result and flags (carry, overflow).</returns>
    protected override (ushort result, bool carry, bool overflow) PerformArithmeticOperation(
        ushort sourceValue, ushort destinationValue, bool isByteOperation)
    {
        // NOTE: Need to access status register carry flag for proper ADDC implementation
        // For now, implementing as simple ADD (this is a temporary limitation)
        uint result = (uint)sourceValue + (uint)destinationValue;

        bool carry, overflow;
        if (isByteOperation)
        {
            // For byte operations, check carry and overflow at 8-bit boundaries
            carry = result > 0xFF;

            // Overflow occurs when two positive numbers sum to negative or two negative numbers sum to positive
            byte src8 = (byte)(sourceValue & 0xFF);
            byte dest8 = (byte)(destinationValue & 0xFF);
            byte result8 = (byte)(result & 0xFF);

            overflow = ((src8 & 0x80) == (dest8 & 0x80)) && ((src8 & 0x80) != (result8 & 0x80));
        }
        else
        {
            // For word operations, check carry and overflow at 16-bit boundaries
            carry = result > 0xFFFF;

            // Overflow occurs when two positive numbers sum to negative or two negative numbers sum to positive
            overflow = ((sourceValue & 0x8000) == (destinationValue & 0x8000)) && ((sourceValue & 0x8000) != (result & 0x8000));
        }

        return ((ushort)result, carry, overflow);
    }
}
