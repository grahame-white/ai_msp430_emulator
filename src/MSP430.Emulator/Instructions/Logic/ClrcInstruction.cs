using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents the CLRC (Clear Carry) instruction.
/// Emulated as: BIC #1, SR
/// </summary>
public class ClrcInstruction : StatusBitInstruction
{
    /// <summary>
    /// Initializes a new instance of the ClrcInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    public ClrcInstruction(ushort instructionWord)
        : base(instructionWord, false, StatusRegister.CarryMask)
    {
    }

    /// <summary>
    /// Gets the mnemonic for the CLRC instruction.
    /// </summary>
    public override string Mnemonic => "CLRC";
}
