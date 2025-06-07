using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents MSP430 status bit manipulation instructions.
/// 
/// These instructions manipulate individual bits in the Status Register (SR/R2).
/// They are typically emulated instructions that use underlying MOV or BIS/BIC operations
/// with the status register as the target.
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131), Section 5.3.3: "Emulated Instructions"
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.5.1.4: "Emulated Instructions"
/// - MSP430FR2355 Datasheet (SLAS847G), Section 6.12: "Instruction Set" - Status register bit definitions
/// </summary>
public abstract class StatusBitInstruction : Instruction, IExecutableInstruction
{
    private readonly bool _setBit;
    private readonly ushort _bitMask;

    /// <summary>
    /// Initializes a new instance of the StatusBitInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="setBit">True to set the bit, false to clear it.</param>
    /// <param name="bitMask">The bit mask for the status flag to manipulate.</param>
    protected StatusBitInstruction(ushort instructionWord, bool setBit, ushort bitMask)
        : base(InstructionFormat.FormatI, 0x4, instructionWord) // MOV instruction opcode
    {
        _setBit = setBit;
        _bitMask = bitMask;
    }

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (always false for status instructions).
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of extension words required for this instruction (always 0).
    /// </summary>
    public override int ExtensionWordCount => 0;

    /// <summary>
    /// Executes the status bit manipulation instruction.
    /// </summary>
    /// <param name="registerFile">The processor register file.</param>
    /// <param name="memory">The system memory.</param>
    /// <param name="extensionWords">Extension words (not used for status instructions).</param>
    /// <returns>The number of CPU cycles consumed.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        if (_setBit)
        {
            registerFile.StatusRegister.Value |= _bitMask;
        }
        else
        {
            registerFile.StatusRegister.Value &= (ushort)~_bitMask;
        }

        return 1; // Status bit instructions take 1 cycle
    }

    /// <summary>
    /// Returns a string representation of this instruction.
    /// </summary>
    /// <returns>The instruction mnemonic.</returns>
    public override string ToString()
    {
        return Mnemonic;
    }
}
