using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.DataMovement;

/// <summary>
/// Implements the MSP430 SWPB (Swap Bytes) instruction.
/// 
/// The SWPB instruction swaps the upper and lower bytes of a 16-bit word.
/// This is a Format II (single-operand) instruction that only operates on words.
/// 
/// Operation: dst[15:8] = src[7:0]; dst[7:0] = src[15:8]
/// Format: SWPB dst
/// Opcode: 0x10 (Format II, sub-opcode determined by bits 7:6)
/// Flags: Sets N and Z based on result, clears V, preserves C
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - October 2004–Revised June 2021, Section 4: "Assembler Description" - Instruction format and operation
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.6.2.48: "SWPB" - Instruction specification
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: "Detailed Description" - Instruction Set
/// </summary>
public class SwpbInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;

    /// <summary>
    /// Initializes a new instance of the SwpbInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    public SwpbInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode)
        : base(InstructionFormat.FormatII, 0x10, instructionWord)
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
    /// Gets a value indicating whether this instruction operates on bytes (false - SWPB always operates on words).
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// </summary>
    public override int ExtensionWordCount => AddressingModeDecoder.RequiresExtensionWord(_destinationAddressingMode) ? 1 : 0;

    /// <summary>
    /// Gets the mnemonic for the SWPB instruction.
    /// </summary>
    public override string Mnemonic => "SWPB";

    /// <summary>
    /// Executes the SWPB instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (immediate values, addresses, offsets).</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Get extension word if needed
        ushort extensionWord = ExtensionWordCount > 0 ? extensionWords[0] : (ushort)0;

        // Read operand (always as word since SWPB operates on words)
        ushort value = InstructionHelpers.ReadOperand(
            _destinationRegister,
            _destinationAddressingMode,
            false, // Always word operation
            registerFile,
            memory,
            extensionWord);

        // Perform byte swap: swap upper and lower bytes
        ushort result = (ushort)(((value & 0xFF) << 8) | ((value & 0xFF00) >> 8));

        // Write result back to destination
        InstructionHelpers.WriteOperand(
            _destinationRegister,
            _destinationAddressingMode,
            false, // Always word operation
            result,
            registerFile,
            memory,
            extensionWord);

        // Update flags: SWPB sets N and Z based on result, clears V, preserves C
        InstructionHelpers.UpdateFlagsForSingleOperandInstruction(result, registerFile.StatusRegister);

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
}
