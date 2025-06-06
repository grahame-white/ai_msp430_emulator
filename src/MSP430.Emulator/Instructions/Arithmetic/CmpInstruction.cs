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
/// - MSP430FR2xx/FR4xx Family User's Guide (SLAU445I), Section 4.3.1: "Format I Instructions" - Instruction encoding
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
}
