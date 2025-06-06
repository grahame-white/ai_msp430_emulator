using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents the SETC (Set Carry) instruction.
/// Emulated as: BIS #1, SR
/// </summary>
public class SetcInstruction : StatusBitInstruction
{
    /// <summary>
    /// Initializes a new instance of the SetcInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    public SetcInstruction(ushort instructionWord)
        : base(instructionWord, true, StatusRegister.CarryMask)
    {
    }

    /// <summary>
    /// Gets the mnemonic for the SETC instruction.
    /// </summary>
    public override string Mnemonic => "SETC";
}
