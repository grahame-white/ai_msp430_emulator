using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions;

/// <summary>
/// Base class for all MSP430 instructions.
/// 
/// Provides common properties and functionality shared across all instruction types,
/// including format information, operand details, and execution context.
/// </summary>
public abstract class Instruction
{
    /// <summary>
    /// Initializes a new instance of the Instruction class.
    /// </summary>
    /// <param name="format">The instruction format (I, II, or III).</param>
    /// <param name="opcode">The instruction opcode.</param>
    /// <param name="instructionWord">The original 16-bit instruction word.</param>
    protected Instruction(InstructionFormat format, ushort opcode, ushort instructionWord)
    {
        Format = format;
        Opcode = opcode;
        InstructionWord = instructionWord;
    }

    /// <summary>
    /// Gets the instruction format (I, II, or III).
    /// </summary>
    public InstructionFormat Format { get; }

    /// <summary>
    /// Gets the instruction opcode.
    /// </summary>
    public ushort Opcode { get; }

    /// <summary>
    /// Gets the original 16-bit instruction word.
    /// </summary>
    public ushort InstructionWord { get; }

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (true) or words (false).
    /// Only applicable to Format I and Format II instructions.
    /// </summary>
    public virtual bool IsByteOperation => false;

    /// <summary>
    /// Gets the source register for this instruction.
    /// May be null for instructions without a source operand.
    /// </summary>
    public virtual RegisterName? SourceRegister => null;

    /// <summary>
    /// Gets the destination register for this instruction.
    /// May be null for instructions without a destination operand.
    /// </summary>
    public virtual RegisterName? DestinationRegister => null;

    /// <summary>
    /// Gets the source addressing mode for this instruction.
    /// May be null for instructions without a source operand.
    /// </summary>
    public virtual AddressingMode? SourceAddressingMode => null;

    /// <summary>
    /// Gets the destination addressing mode for this instruction.
    /// May be null for instructions without a destination operand.
    /// </summary>
    public virtual AddressingMode? DestinationAddressingMode => null;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// This includes immediate values, absolute addresses, and indexed offsets.
    /// </summary>
    public virtual int ExtensionWordCount => 0;

    /// <summary>
    /// Gets a human-readable string representation of the instruction.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public abstract override string ToString();

    /// <summary>
    /// Gets the mnemonic (name) of this instruction.
    /// </summary>
    public abstract string Mnemonic { get; }
}
