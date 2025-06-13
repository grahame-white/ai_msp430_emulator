using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.Logic;

namespace MSP430.Emulator.Instructions.EmulatedInstructions;

/// <summary>
/// Implements the MSP430 INV (Invert destination) emulated instruction.
/// 
/// The INV instruction inverts the bits of the destination operand (one's complement).
/// This instruction is emulated as XOR(.B) #-1, dst per SLAU445I Table 4-7.
/// 
/// Operation: dst = ~dst (one's complement)
/// Emulation: XOR(.B) #-1, dst
/// Cycles: Same as underlying XOR instruction
/// Flags affected: N, Z, C=1, V=0
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class InvInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Initializes a new instance of the InvInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public InvInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0xE, instructionWord) // Emulated as XOR instruction
    {
        _destinationRegister = destinationRegister;
        _destinationAddressingMode = destinationAddressingMode;
        _isByteOperation = isByteOperation;
    }

    /// <summary>
    /// Gets the destination register for this instruction.
    /// </summary>
    public override RegisterName? DestinationRegister => _destinationRegister;

    /// <summary>
    /// Gets the source register for this instruction (always CG2/R3 for constant #-1).
    /// </summary>
    public override RegisterName? SourceRegister => RegisterName.R3;

    /// <summary>
    /// Gets the source addressing mode for this instruction (always Indirect mode for #-1).
    /// </summary>
    public override AddressingMode? SourceAddressingMode => AddressingMode.Indirect;

    /// <summary>
    /// Gets the destination addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => _destinationAddressingMode;

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (true) or words (false).
    /// </summary>
    public override bool IsByteOperation => _isByteOperation;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// </summary>
    public override int ExtensionWordCount =>
        InstructionHelpers.CalculateDestinationOnlyExtensionWordCount(_destinationAddressingMode);

    /// <summary>
    /// Gets the mnemonic for the INV instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "INV.B" : "INV";

    /// <summary>
    /// Executes the INV instruction on the specified CPU state.
    /// This emulates XOR #-1, dst to invert all bits in the destination.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction.</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Create an underlying XOR instruction to perform the actual operation
        // INV dst is emulated as XOR #-1, dst (using R3 as constant generator with AS=01)
        var xorInstruction = new XorInstruction(
            InstructionWord,
            RegisterName.R3, // Source: CG2 for constant #-1 (using AS=01)
            _destinationRegister,
            AddressingMode.Indirect, // Source: indirect mode for constant #-1
            _destinationAddressingMode,
            _isByteOperation);

        // Execute the underlying XOR instruction
        // The constant #-1 will be provided by the constant generator
        return xorInstruction.Execute(registerFile, memory, extensionWords);
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
