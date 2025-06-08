using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.DataMovement;

/// <summary>
/// Implements the MSP430 SXT (Sign Extend) instruction.
/// 
/// The SXT instruction sign-extends a byte value to a word by copying the sign bit (bit 7) 
/// to all bits of the upper byte (bits 15:8).
/// This is a Format II (single-operand) instruction.
/// 
/// Operation: dst[15:8] = dst[7] ? 0xFF : 0x00; dst[7:0] unchanged
/// Format: SXT dst
/// Opcode: 0x11 (Format II)
/// Flags: Sets N and Z based on result, clears V, preserves C
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - October 2004–Revised June 2021, Section 4: "Assembler Description" - Instruction format and operation
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.6.2.49: "SXT" - Instruction specification
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: "Detailed Description" - Instruction Set
/// </summary>
public class SxtInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;

    /// <summary>
    /// Initializes a new instance of the SxtInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    public SxtInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode)
        : base(InstructionFormat.FormatII, 0x11, instructionWord)
    {
        _destinationRegister = destinationRegister;
        _destinationAddressingMode = destinationAddressingMode;
    }

    /// <summary>
    /// Gets the destination register for this instruction.
    /// </summary>
    public override RegisterName? DestinationRegister => _destinationRegister;

    /// <summary>
    /// Gets the destination addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => _destinationAddressingMode;

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (false - SXT extends byte to word).
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// </summary>
    public override int ExtensionWordCount => AddressingModeDecoder.RequiresExtensionWord(_destinationAddressingMode) ? 1 : 0;

    /// <summary>
    /// Gets the mnemonic for the SXT instruction.
    /// </summary>
    public override string Mnemonic => "SXT";

    /// <summary>
    /// Executes the SXT instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (immediate values, addresses, offsets).</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Get extension word if needed
        ushort extensionWord = ExtensionWordCount > 0 ? extensionWords[0] : (ushort)0;

        // Read operand (always as word since SXT operates on words)
        ushort value = InstructionHelpers.ReadOperand(
            _destinationRegister,
            _destinationAddressingMode,
            false, // Always word operation
            registerFile,
            memory,
            extensionWord);

        // Perform sign extension: if bit 7 is set, set bits 15:8 to 0xFF, otherwise clear them
        ushort result = (ushort)((value & 0x80) != 0 ? (value | 0xFF00) : (value & 0x00FF));

        // Write result back to destination
        InstructionHelpers.WriteOperand(
            _destinationRegister,
            _destinationAddressingMode,
            false, // Always word operation
            result,
            registerFile,
            memory,
            extensionWord);

        // Update flags: SXT sets N and Z based on result, clears V, preserves C
        UpdateFlags(result, registerFile.StatusRegister);

        // Return cycle count based on addressing mode
        return InstructionHelpers.GetSingleOperandCycleCount(_destinationAddressingMode);
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {InstructionHelpers.FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }

    /// <summary>
    /// Updates CPU flags based on the sign-extended value.
    /// SXT instruction sets N and Z flags based on the result, clears V flag, and preserves C flag.
    /// 
    /// References:
    /// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.6.2.49: "SXT" - Flag behavior
    /// </summary>
    /// <param name="result">The result value after sign extension.</param>
    /// <param name="statusRegister">The CPU status register to update.</param>
    private static void UpdateFlags(ushort result, StatusRegister statusRegister)
    {
        // Zero flag: Set if result is zero
        statusRegister.Zero = result == 0;

        // Negative flag: Set if the sign bit (bit 15) is set
        statusRegister.Negative = (result & 0x8000) != 0;

        // Overflow flag: Always cleared for SXT instruction
        statusRegister.Overflow = false;

        // Carry flag: Preserved (not modified by SXT)
    }
}
