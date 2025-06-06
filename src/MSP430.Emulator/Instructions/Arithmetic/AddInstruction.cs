using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Arithmetic;

/// <summary>
/// Represents the MSP430 ADD instruction.
/// 
/// The ADD instruction adds the source operand to the destination operand.
/// Format: ADD src, dst
/// Operation: dst = dst + src
/// Opcode: 0x5 (Format I)
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131), Section 5.3.2: "ADD - Add" - Instruction format and operation
/// - MSP430FR2xx/FR4xx Family User's Guide (SLAU445I), Section 4.3.1: "Format I Instructions" - Instruction encoding
/// - MSP430FR2355 Datasheet (SLAS847G), Section 6.12: "Instruction Set" - Opcode definition and flag behavior
/// </summary>
public class AddInstruction : ArithmeticInstruction
{
    /// <summary>
    /// Initializes a new instance of the AddInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public AddInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(0x5, instructionWord, sourceRegister, destinationRegister, sourceAddressingMode, destinationAddressingMode, isByteOperation)
    {
    }

    /// <summary>
    /// Gets the base mnemonic for the ADD instruction.
    /// </summary>
    protected override string BaseMnemonic => "ADD";
}
