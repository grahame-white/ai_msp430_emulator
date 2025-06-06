namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents the SETN (Set Negative) instruction.
/// Emulated as: BIS #4, SR
/// </summary>
public class SetnInstruction : StatusBitInstruction
{
    /// <summary>
    /// Initializes a new instance of the SetnInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    public SetnInstruction(ushort instructionWord)
        : base(instructionWord, true, 0x0004) // Negative flag is bit 2
    {
    }

    /// <summary>
    /// Gets the mnemonic for the SETN instruction.
    /// </summary>
    public override string Mnemonic => "SETN";
}
