namespace MSP430.Emulator.Core;

/// <summary>
/// Provides data for the InstructionExecuted event.
/// </summary>
public class InstructionExecutedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the InstructionExecutedEventArgs class.
    /// </summary>
    /// <param name="address">The address of the executed instruction.</param>
    /// <param name="instructionWord">The 16-bit instruction word that was executed.</param>
    /// <param name="cycles">The number of CPU cycles consumed.</param>
    public InstructionExecutedEventArgs(ushort address, ushort instructionWord, uint cycles)
    {
        Address = address;
        InstructionWord = instructionWord;
        Cycles = cycles;
    }

    /// <summary>
    /// Gets the address of the executed instruction.
    /// </summary>
    public ushort Address { get; }

    /// <summary>
    /// Gets the 16-bit instruction word that was executed.
    /// </summary>
    public ushort InstructionWord { get; }

    /// <summary>
    /// Gets the number of CPU cycles consumed by the instruction.
    /// </summary>
    public uint Cycles { get; }
}
