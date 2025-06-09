using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Arithmetic;

/// <summary>
/// Represents the MSP430 DEC instruction.
/// 
/// The DEC instruction decrements the destination operand by 1.
/// This is equivalent to SUB #1, dst but implemented as a single-operand instruction.
/// Format: DEC dst
/// Operation: dst = dst - 1
/// Opcode: 0xB (Format I)
/// Flags affected: N, Z, C, V
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - October 2004–Revised June 2021, Section 4: "Assembler Description" - Instruction format and operation
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.5.1.1: "MSP430 Double-Operand (Format I) Instructions" - Instruction encoding
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: "Detailed Description" - Instruction Set
/// </summary>
public class DecInstruction : Instruction, IExecutableInstruction
{
    /// <summary>
    /// Initializes a new instance of the DecInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public DecInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0xB, instructionWord)
    {
        _destinationRegister = destinationRegister;
        _destinationAddressingMode = destinationAddressingMode;
        _isByteOperation = isByteOperation;
    }

    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Gets the mnemonic for the DEC instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "DEC.B" : "DEC";

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (true) or words (false).
    /// </summary>
    public override bool IsByteOperation => _isByteOperation;

    /// <summary>
    /// Gets the destination register for this instruction.
    /// </summary>
    public override RegisterName? DestinationRegister => _destinationRegister;

    /// <summary>
    /// Gets the destination addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => _destinationAddressingMode;

    /// <summary>
    /// Gets the number of additional words required by this instruction.
    /// </summary>
    public override int ExtensionWordCount
    {
        get
        {
            // Only destination operand can require extension words
            if (_destinationAddressingMode == AddressingMode.Absolute ||
                _destinationAddressingMode == AddressingMode.Symbolic ||
                _destinationAddressingMode == AddressingMode.Indexed)
            {
                return 1;
            }

            return 0;
        }
    }

    /// <summary>
    /// Returns a string representation of the DEC instruction.
    /// </summary>
    /// <returns>A string describing the instruction in assembly format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {InstructionHelpers.FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }

    /// <summary>
    /// Executes the DEC instruction on the specified CPU state.
    /// Decrements the destination operand by 1 and updates the N, Z, C, V flags.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction.</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Get extension word if needed
        ushort extensionWord = ExtensionWordCount > 0 ? extensionWords[0] : (ushort)0;

        // Read current value from destination
        ushort currentValue = InstructionHelpers.ReadDestinationOperand(
            _destinationRegister,
            _destinationAddressingMode,
            _isByteOperation,
            registerFile,
            memory,
            extensionWord);

        // Perform decrement operation
        ushort result = (ushort)(currentValue - 1);

        // Calculate flags
        bool carry, overflow;
        if (_isByteOperation)
        {
            // For byte operations, mask to 8 bits
            result &= 0xFF;
            carry = (currentValue & 0xFF) != 0; // Carry (borrow) if not decrementing 0
            overflow = (currentValue & 0xFF) == 0x80; // Overflow if decrementing 0x80 (-128)
        }
        else
        {
            // For word operations
            carry = currentValue != 0; // Carry (borrow) if not decrementing 0
            overflow = currentValue == 0x8000; // Overflow if decrementing 0x8000 (-32768)
        }

        // Update flags
        InstructionHelpers.UpdateFlags(result, carry, overflow, _isByteOperation, registerFile.StatusRegister);

        // Write result back to destination
        InstructionHelpers.WriteOperand(
            _destinationRegister,
            _destinationAddressingMode,
            _isByteOperation,
            result,
            registerFile,
            memory,
            extensionWord);

        // Return cycle count (varies by addressing mode, but typically 1-4 cycles)
        return InstructionHelpers.GetSingleOperandCycleCount(_destinationAddressingMode);
    }
}
