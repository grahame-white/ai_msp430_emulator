using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents the MSP430 XOR instruction.
/// 
/// The XOR instruction performs a bitwise exclusive OR operation between the source and destination operands.
/// The result is stored in the destination operand.
/// Format: XOR src, dst
/// Operation: dst = src ^ dst
/// Opcode: 0xE (Format I)
/// Flags affected: N, Z, C (cleared), V (cleared)
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - October 2004–Revised June 2021, Section 4: "Assembler Description" - Instruction format and operation
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.5.1.1: "MSP430 Double-Operand (Format I) Instructions" - Instruction encoding
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: "Detailed Description" - Instruction Set
/// </summary>
public class XorInstruction : LogicalInstruction
{
    /// <summary>
    /// Initializes a new instance of the XorInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public XorInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(0xE, instructionWord, sourceRegister, destinationRegister, sourceAddressingMode, destinationAddressingMode, isByteOperation)
    {
    }

    /// <summary>
    /// Gets the base mnemonic for the XOR instruction.
    /// </summary>
    protected override string BaseMnemonic => "XOR";

    /// <summary>
    /// Performs the XOR (exclusive OR) logical operation.
    /// </summary>
    /// <param name="sourceValue">The source operand value.</param>
    /// <param name="destinationValue">The destination operand value.</param>
    /// <returns>The result of the XOR operation.</returns>
    protected override ushort PerformLogicalOperation(ushort sourceValue, ushort destinationValue)
    {
        return (ushort)(sourceValue ^ destinationValue);
    }
}
