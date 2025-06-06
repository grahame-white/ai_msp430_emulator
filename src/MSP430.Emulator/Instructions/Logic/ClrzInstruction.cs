namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents the CLRZ (Clear Zero) instruction.
/// Emulated as: BIC #2, SR
/// </summary>
public class ClrzInstruction : StatusBitInstruction
{
    /// <summary>
    /// Initializes a new instance of the ClrzInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    public ClrzInstruction(ushort instructionWord)
        : base(instructionWord, false, 0x0002) // Zero flag is bit 1
    {
    }

    /// <summary>
    /// Gets the mnemonic for the CLRZ instruction.
    /// </summary>
    public override string Mnemonic => "CLRZ";
}
