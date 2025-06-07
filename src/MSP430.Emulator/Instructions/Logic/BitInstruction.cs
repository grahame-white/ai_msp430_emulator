using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents the MSP430 BIT instruction.
/// 
/// The BIT instruction performs a bitwise AND operation between the source and destination operands
/// for testing purposes only. The result is not stored, but flags are updated based on the result.
/// Format: BIT src, dst
/// Operation: src & dst (flags only, dst unchanged)
/// Opcode: 0xB (Format I)
/// Flags affected: N, Z, C (cleared), V (cleared)
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131), Section 5.3.2: "BIT - Test Bits" - Instruction format and operation
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.5.1.1: "MSP430 Double-Operand (Format I) Instructions" - Instruction encoding
/// - MSP430FR2355 Datasheet (SLAS847G), Section 6.12: "Instruction Set" - Opcode definition and flag behavior
/// </summary>
public class BitInstruction : LogicalInstruction
{
    /// <summary>
    /// Initializes a new instance of the BitInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public BitInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(0xB, instructionWord, sourceRegister, destinationRegister, sourceAddressingMode, destinationAddressingMode, isByteOperation)
    {
    }

    /// <summary>
    /// Gets the base mnemonic for the BIT instruction.
    /// </summary>
    protected override string BaseMnemonic => "BIT";

    /// <summary>
    /// Performs the BIT test logical operation.
    /// </summary>
    /// <param name="sourceValue">The source operand value.</param>
    /// <param name="destinationValue">The destination operand value.</param>
    /// <returns>The result of the AND operation for flag setting purposes.</returns>
    protected override ushort PerformLogicalOperation(ushort sourceValue, ushort destinationValue)
    {
        return (ushort)(sourceValue & destinationValue);
    }

    /// <summary>
    /// Determines whether this instruction should write the result back to the destination.
    /// BIT instruction only sets flags and does not modify the destination.
    /// </summary>
    /// <returns>False, as BIT instruction does not write the result.</returns>
    protected override bool ShouldWriteResult() => false;
}
