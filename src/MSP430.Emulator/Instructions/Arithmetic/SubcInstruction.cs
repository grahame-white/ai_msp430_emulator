using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Arithmetic;

/// <summary>
/// Implements the MSP430 SUBC (Subtract with Carry) instruction.
/// 
/// The SUBC instruction subtracts the source operand from the destination operand,
/// with the carry bit representing a borrow from the previous operation.
/// This is a Format I (two-operand) instruction that supports all addressing modes.
/// 
/// Operation: dst = dst - src - (1 - C)  [equivalent to dst = dst + ~src + C]
/// Format: SUBC(.B) src, dst
/// Opcode: 0x7 (Format I)
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.47: SUBC
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.1: Format I Instructions
/// </summary>
public class SubcInstruction : ArithmeticInstruction
{
    /// <summary>
    /// Initializes a new instance of the SubcInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public SubcInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(0x7, instructionWord, sourceRegister, destinationRegister,
               sourceAddressingMode, destinationAddressingMode, isByteOperation)
    {
    }

    /// <summary>
    /// Gets the base mnemonic for the SUBC instruction.
    /// </summary>
    protected override string BaseMnemonic => "SUBC";

    /// <summary>
    /// Performs the SUBC arithmetic operation.
    /// Subtracts the source operand from the destination operand with borrow (carry).
    /// Operation: dst = dst - src - (1 - C) which is equivalent to dst = dst + ~src + C
    /// </summary>
    /// <param name="sourceValue">The source operand value.</param>
    /// <param name="destinationValue">The destination operand value.</param>
    /// <param name="isByteOperation">True for byte operations, false for word operations.</param>
    /// <param name="registerFile">The register file for accessing the carry flag.</param>
    /// <returns>A tuple containing the result and flags (carry, overflow).</returns>
    protected override (ushort result, bool carry, bool overflow) PerformArithmeticOperation(
        ushort sourceValue, ushort destinationValue, bool isByteOperation, IRegisterFile registerFile)
    {
        // Read the current carry flag from the status register
        // In MSP430, carry acts as "no borrow" flag for subtract operations
        uint carryInput = registerFile.StatusRegister.Carry ? 1u : 0u;

        // Perform SUBC: dst = dst - src - (1 - C) = dst + ~src + C
        uint result = (uint)destinationValue + (uint)(~sourceValue) + carryInput;

        bool carry, overflow;
        if (isByteOperation)
        {
            // For byte operations, check borrow (inverse carry) at 8-bit boundaries
            // Carry is set when no borrow is needed (result >= 0)
            carry = result > 0xFF;

            // Overflow occurs when subtracting numbers of different signs yields wrong sign
            byte src8 = (byte)(sourceValue & 0xFF);
            byte dest8 = (byte)(destinationValue & 0xFF);
            byte result8 = (byte)(result & 0xFF);

            overflow = ((src8 & 0x80) != (dest8 & 0x80)) && ((dest8 & 0x80) != (result8 & 0x80));
        }
        else
        {
            // For word operations, check borrow (inverse carry) at 16-bit boundaries
            carry = result > 0xFFFF;

            // Overflow occurs when subtracting numbers of different signs yields wrong sign
            overflow = ((sourceValue & 0x8000) != (destinationValue & 0x8000)) && ((destinationValue & 0x8000) != (result & 0x8000));
        }

        return ((ushort)result, carry, overflow);
    }
}
