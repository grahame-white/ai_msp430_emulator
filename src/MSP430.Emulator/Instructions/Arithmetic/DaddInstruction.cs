using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Arithmetic;

/// <summary>
/// Implements the MSP430 DADD (Decimal Add) instruction.
/// 
/// The DADD instruction adds the source operand to the destination operand using
/// Binary Coded Decimal (BCD) arithmetic. Each nibble (4 bits) represents a decimal digit (0-9).
/// This is a Format I (two-operand) instruction that supports all addressing modes.
/// 
/// Operation: dst = src + dst (BCD addition with carry)
/// Format: DADD(.B) src, dst
/// Opcode: 0xA (Format I)
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.13: DADD
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.1: Format I Instructions
/// </summary>
public class DaddInstruction : ArithmeticInstruction
{
    /// <summary>
    /// Initializes a new instance of the DaddInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public DaddInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(0xA, instructionWord, sourceRegister, destinationRegister,
               sourceAddressingMode, destinationAddressingMode, isByteOperation)
    {
    }

    /// <summary>
    /// Gets the base mnemonic for the DADD instruction.
    /// </summary>
    protected override string BaseMnemonic => "DADD";

    /// <summary>
    /// Performs the DADD arithmetic operation.
    /// Adds the source operand to the destination operand using BCD arithmetic with carry.
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
        // NOTE: Need to access status register carry flag for proper DADD implementation
        // For now, implementing simple BCD addition without carry input
        if (isByteOperation)
        {
            // Mask to 8 bits for byte operations
            byte src = (byte)(sourceValue & 0xFF);
            byte dst = (byte)(destinationValue & 0xFF);

            // Perform BCD addition for byte (2 nibbles)
            int result = PerformBcdAddition(src, dst, 0, 2); // 0 = no carry input for now
            byte finalResult = (byte)(result & 0xFF);

            // Calculate flags for byte operation
            bool carry = (result & 0x100) != 0; // Carry out of upper nibble

            // For BCD operations, overflow is set when the result cannot be represented
            // in BCD format (each nibble > 9), but this is corrected during BCD addition
            // V flag behavior for DADD is generally undefined in MSP430 documentation
            bool overflow = false; // BCD overflow handling is implementation-specific

            return (finalResult, carry, overflow);
        }
        else
        {
            // Word operation (4 nibbles)
            int result = PerformBcdAddition(sourceValue, destinationValue, 0, 4); // 0 = no carry input for now
            ushort finalResult = (ushort)(result & 0xFFFF);

            // Calculate flags for word operation
            bool carry = (result & 0x10000) != 0; // Carry out of highest nibble

            // V flag behavior for DADD is generally undefined in MSP430 documentation
            bool overflow = false; // BCD overflow handling is implementation-specific

            return (finalResult, carry, overflow);
        }
    }

    /// <summary>
    /// Performs BCD addition on the specified values.
    /// </summary>
    /// <param name="src">The source value.</param>
    /// <param name="dst">The destination value.</param>
    /// <param name="carryIn">The carry input (0 or 1).</param>
    /// <param name="nibbleCount">The number of nibbles to process (2 for byte, 4 for word).</param>
    /// <returns>The BCD addition result.</returns>
    private static int PerformBcdAddition(int src, int dst, int carryIn, int nibbleCount)
    {
        int result = 0;
        int carry = carryIn;

        for (int i = 0; i < nibbleCount; i++)
        {
            // Extract current nibbles (4 bits each)
            int srcNibble = (src >> (i * 4)) & 0xF;
            int dstNibble = (dst >> (i * 4)) & 0xF;

            // Add nibbles with carry
            int nibbleSum = srcNibble + dstNibble + carry;

            // BCD correction: if sum > 9, subtract 10 and set carry
            if (nibbleSum > 9)
            {
                nibbleSum -= 10;
                carry = 1;
            }
            else
            {
                carry = 0;
            }

            // Store corrected nibble in result
            result |= (nibbleSum & 0xF) << (i * 4);
        }

        // Add final carry to result
        if (carry != 0)
        {
            result |= carry << (nibbleCount * 4);
        }

        return result;
    }
}
