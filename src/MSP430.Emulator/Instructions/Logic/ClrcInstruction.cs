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
        : base(instructionWord, false, 0x0001) // Carry flag is bit 0
    {
    }

    /// <summary>
    /// Gets the mnemonic for the CLRC instruction.
    /// </summary>
    public override string Mnemonic => "CLRC";
}
