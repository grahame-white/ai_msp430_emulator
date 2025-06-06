namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents the SETZ (Set Zero) instruction.
/// Emulated as: BIS #2, SR
/// </summary>
public class SetzInstruction : StatusBitInstruction
{
    /// <summary>
    /// Initializes a new instance of the SetzInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    public SetzInstruction(ushort instructionWord)
        : base(instructionWord, true, 0x0002) // Zero flag is bit 1
    {
    }

    /// <summary>
    /// Gets the mnemonic for the SETZ instruction.
    /// </summary>
    public override string Mnemonic => "SETZ";
}
