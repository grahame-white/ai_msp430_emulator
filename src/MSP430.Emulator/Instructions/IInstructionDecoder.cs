namespace MSP430.Emulator.Instructions;

/// <summary>
/// Interface for decoding MSP430 machine code instructions into executable operations.
/// 
/// The instruction decoder is responsible for parsing raw 16-bit instruction words
/// and converting them into structured instruction objects that can be executed
/// by the emulator.
/// </summary>
public interface IInstructionDecoder
{
    /// <summary>
    /// Decodes a 16-bit instruction word into an instruction object.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word to decode.</param>
    /// <returns>The decoded instruction object.</returns>
    /// <exception cref="InvalidInstructionException">Thrown when the instruction word is invalid or unsupported.</exception>
    Instruction Decode(ushort instructionWord);

    /// <summary>
    /// Determines the instruction format of a 16-bit instruction word.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word to analyze.</param>
    /// <returns>The instruction format (I, II, or III).</returns>
    InstructionFormat GetInstructionFormat(ushort instructionWord);

    /// <summary>
    /// Validates whether a 16-bit instruction word represents a valid MSP430 instruction.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word to validate.</param>
    /// <returns>True if the instruction is valid, false otherwise.</returns>
    bool IsValidInstruction(ushort instructionWord);

    /// <summary>
    /// Extracts the opcode from a 16-bit instruction word based on its format.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="format">The instruction format.</param>
    /// <returns>The extracted opcode.</returns>
    ushort ExtractOpcode(ushort instructionWord, InstructionFormat format);
}
