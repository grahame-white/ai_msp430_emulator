using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Arithmetic;

/// <summary>
/// Represents the MSP430 CMP instruction.
/// 
/// The CMP instruction compares the source operand with the destination operand.
/// The comparison is performed by subtracting the source from the destination,
/// but the destination operand is not modified. Only flags are affected.
/// Format: CMP src, dst
/// Operation: dst - src (result discarded, only flags set)
/// Opcode: 0x9 (Format I)
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131), Section 5.3.2: "CMP - Compare" - Instruction format and operation
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.5.1.1: "MSP430 Double-Operand (Format I) Instructions" - Instruction encoding
/// - MSP430FR2355 Datasheet (SLAS847G), Section 6.12: "Instruction Set" - Opcode definition and flag behavior
/// </summary>
public class CmpInstruction : ArithmeticInstruction
{
    /// <summary>
    /// Initializes a new instance of the CmpInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public CmpInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(0x9, instructionWord, sourceRegister, destinationRegister, sourceAddressingMode, destinationAddressingMode, isByteOperation)
    {
    }

    /// <summary>
    /// Gets the base mnemonic for the CMP instruction.
    /// </summary>
    protected override string BaseMnemonic => "CMP";

    /// <summary>
    /// Performs the CMP operation: dst - src (result discarded, only flags set)
    /// </summary>
    /// <param name="sourceValue">The source operand value.</param>
    /// <param name="destinationValue">The destination operand value.</param>
    /// <param name="isByteOperation">True for byte operations, false for word operations.</param>
    /// <returns>A tuple containing the result, carry flag, and overflow flag.</returns>
    protected override (ushort result, bool carry, bool overflow) PerformArithmeticOperation(ushort sourceValue, ushort destinationValue, bool isByteOperation)
    {
        // CMP performs the same operation as SUB: dst - src
        uint result = (uint)destinationValue - (uint)sourceValue;

        bool carry, overflow;
        if (isByteOperation)
        {
            // For byte operations, check carry and overflow at 8-bit boundaries
            carry = sourceValue > destinationValue; // No borrow means carry is set

            // Overflow occurs when subtracting a negative from positive gives negative, or positive from negative gives positive
            byte src8 = (byte)(sourceValue & 0xFF);
            byte dest8 = (byte)(destinationValue & 0xFF);
            byte result8 = (byte)(result & 0xFF);

            overflow = ((src8 & 0x80) != (dest8 & 0x80)) && ((dest8 & 0x80) != (result8 & 0x80));
        }
        else
        {
            // For word operations, check carry and overflow at 16-bit boundaries
            carry = sourceValue > destinationValue; // No borrow means carry is set

            // Overflow occurs when subtracting a negative from positive gives negative, or positive from negative gives positive
            overflow = ((sourceValue & 0x8000) != (destinationValue & 0x8000)) && ((destinationValue & 0x8000) != (result & 0x8000));
        }

        return ((ushort)result, carry, overflow);
    }

    /// <summary>
    /// CMP instruction does not write the result back to the destination.
    /// It only sets flags based on the comparison result.
    /// </summary>
    /// <returns>False, indicating the result should not be written to the destination.</returns>
    protected override bool ShouldWriteResult() => false;
}
