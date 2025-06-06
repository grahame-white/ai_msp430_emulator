using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Represents the MSP430 XOR instruction.
/// 
/// The XOR instruction performs a bitwise exclusive OR operation between the source and destination operands.
/// The result is stored in the destination operand.
/// Format: XOR src, dst
/// Operation: dst = src ^ dst
/// Opcode: 0xE (Format I)
/// Flags affected: N, Z, C (cleared), V (cleared)
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131), Section 5.3.2: "XOR - Exclusive OR" - Instruction format and operation
/// - MSP430FR2xx/FR4xx Family User's Guide (SLAU445I), Section 4.3.1: "Format I Instructions" - Instruction encoding
/// - MSP430FR2355 Datasheet (SLAS847G), Section 6.12: "Instruction Set" - Opcode definition and flag behavior
/// </summary>
public class XorInstruction : Instruction, IExecutableInstruction
{
    /// <summary>
    /// Initializes a new instance of the XorInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public XorInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0xE, instructionWord)
    {
        _sourceRegister = sourceRegister;
        _destinationRegister = destinationRegister;
        _sourceAddressingMode = sourceAddressingMode;
        _destinationAddressingMode = destinationAddressingMode;
        _isByteOperation = isByteOperation;
    }

    private readonly RegisterName _sourceRegister;
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _sourceAddressingMode;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Gets the mnemonic for the XOR instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "XOR.B" : "XOR";

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (true) or words (false).
    /// </summary>
    public override bool IsByteOperation => _isByteOperation;

    /// <summary>
    /// Gets the source register for this instruction.
    /// </summary>
    public override RegisterName? SourceRegister => _sourceRegister;

    /// <summary>
    /// Gets the destination register for this instruction.
    /// </summary>
    public override RegisterName? DestinationRegister => _destinationRegister;

    /// <summary>
    /// Gets the source addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? SourceAddressingMode => _sourceAddressingMode;

    /// <summary>
    /// Gets the destination addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => _destinationAddressingMode;

    /// <summary>
    /// Gets the number of additional words required by this instruction.
    /// </summary>
    public override int ExtensionWordCount => InstructionHelpers.CalculateFormatIExtensionWordCount(_sourceAddressingMode, _destinationAddressingMode);

    /// <summary>
    /// Returns a string representation of the XOR instruction.
    /// </summary>
    /// <returns>A string describing the instruction in assembly format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {InstructionHelpers.FormatOperand(_sourceRegister, _sourceAddressingMode)}, {InstructionHelpers.FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }

    /// <summary>
    /// Executes the XOR instruction on the specified CPU state.
    /// Performs a bitwise exclusive OR operation between source and destination operands and updates the N, Z flags.
    /// The C and V flags are cleared.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction.</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Get extension words for source and destination if needed
        ushort sourceExtensionWord = 0;
        ushort destinationExtensionWord = 0;
        int extensionIndex = 0;

        // Source operand extension word
        if (_sourceAddressingMode == AddressingMode.Immediate ||
            _sourceAddressingMode == AddressingMode.Absolute ||
            _sourceAddressingMode == AddressingMode.Symbolic ||
            _sourceAddressingMode == AddressingMode.Indexed)
        {
            sourceExtensionWord = extensionWords[extensionIndex++];
        }

        // Destination operand extension word
        if (_destinationAddressingMode == AddressingMode.Absolute ||
            _destinationAddressingMode == AddressingMode.Symbolic ||
            _destinationAddressingMode == AddressingMode.Indexed)
        {
            destinationExtensionWord = extensionWords[extensionIndex];
        }

        // Read source operand
        ushort sourceValue = InstructionHelpers.ReadOperand(
            _sourceRegister,
            _sourceAddressingMode,
            _isByteOperation,
            registerFile,
            memory,
            sourceExtensionWord);

        // Read destination operand
        ushort destinationValue = InstructionHelpers.ReadOperand(
            _destinationRegister,
            _destinationAddressingMode,
            _isByteOperation,
            registerFile,
            memory,
            destinationExtensionWord);

        // Perform XOR operation: dst = src ^ dst
        ushort result = (ushort)(sourceValue ^ destinationValue);

        // For byte operations, mask to 8 bits
        if (_isByteOperation)
        {
            result &= 0xFF;
        }

        // Update flags: XOR instruction affects N, Z; clears C, V
        registerFile.StatusRegister.Zero = result == 0;
        registerFile.StatusRegister.Negative = _isByteOperation ? (result & 0x80) != 0 : (result & 0x8000) != 0;
        registerFile.StatusRegister.Carry = false;  // XOR always clears carry
        registerFile.StatusRegister.Overflow = false;  // XOR always clears overflow

        // Write result back to destination
        InstructionHelpers.WriteOperand(
            _destinationRegister,
            _destinationAddressingMode,
            _isByteOperation,
            result,
            registerFile,
            memory,
            destinationExtensionWord);

        // Return cycle count (varies by addressing mode combination)
        return GetCycleCount();
    }

    /// <summary>
    /// Gets the number of CPU cycles required for this instruction based on addressing modes.
    /// </summary>
    /// <returns>The number of CPU cycles.</returns>
    private uint GetCycleCount()
    {
        // Base cycle count for Format I instructions
        uint baseCycles = 1;

        // Add cycles for source addressing mode
        uint sourceCycles = _sourceAddressingMode switch
        {
            AddressingMode.Register => 0,
            AddressingMode.Indexed => 3,
            AddressingMode.Indirect => 2,
            AddressingMode.IndirectAutoIncrement => 2,
            AddressingMode.Immediate => 0,
            AddressingMode.Absolute => 3,
            AddressingMode.Symbolic => 3,
            _ => 0
        };

        // Add cycles for destination addressing mode
        uint destinationCycles = _destinationAddressingMode switch
        {
            AddressingMode.Register => 0,
            AddressingMode.Indexed => 3,
            AddressingMode.Indirect => 2,
            AddressingMode.IndirectAutoIncrement => 2,
            AddressingMode.Absolute => 3,
            AddressingMode.Symbolic => 3,
            _ => 0
        };

        return baseCycles + sourceCycles + destinationCycles;
    }
}
