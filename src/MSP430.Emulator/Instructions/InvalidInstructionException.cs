using System;

namespace MSP430.Emulator.Instructions;

/// <summary>
/// Exception thrown when an invalid or unsupported instruction is encountered during decoding.
/// </summary>
public class InvalidInstructionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the InvalidInstructionException class.
    /// </summary>
    /// <param name="instructionWord">The invalid instruction word.</param>
    /// <param name="message">The error message.</param>
    public InvalidInstructionException(ushort instructionWord, string message)
        : base(message)
    {
        InstructionWord = instructionWord;
    }

    /// <summary>
    /// Initializes a new instance of the InvalidInstructionException class.
    /// </summary>
    /// <param name="instructionWord">The invalid instruction word.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidInstructionException(ushort instructionWord, string message, Exception innerException)
        : base(message, innerException)
    {
        InstructionWord = instructionWord;
    }

    /// <summary>
    /// Gets the instruction word that caused the exception.
    /// </summary>
    public ushort InstructionWord { get; }
}
