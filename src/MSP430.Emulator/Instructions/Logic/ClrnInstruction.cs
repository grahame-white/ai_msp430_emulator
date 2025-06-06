namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents the CLRN (Clear Negative) instruction.
/// Emulated as: BIC #4, SR
/// </summary>
public class ClrnInstruction : StatusBitInstruction
{
    /// <summary>
    /// Initializes a new instance of the ClrnInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    public ClrnInstruction(ushort instructionWord)
        : base(instructionWord, false, 0x0004) // Negative flag is bit 2
    {
    }

    /// <summary>
    /// Gets the mnemonic for the CLRN instruction.
    /// </summary>
    public override string Mnemonic => "CLRN";
}
