using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Logic;

/// <summary>
/// Base class for MSP430 logical operation instructions.
/// 
/// Provides common implementation for bitwise logical operations (AND, BIS, XOR, BIT)
/// that share similar instruction format, addressing modes, and execution patterns.
/// All logical instructions use Format I encoding with two operands.
/// </summary>
public abstract class LogicalInstruction : Instruction, IExecutableInstruction
{
    /// <summary>
    /// Initializes a new instance of the LogicalInstruction class.
    /// </summary>
    /// <param name="opcode">The instruction opcode.</param>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    protected LogicalInstruction(
        byte opcode,
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, opcode, instructionWord)
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
    /// Gets the base mnemonic for this logical instruction (to be overridden by derived classes).
    /// </summary>
    protected abstract string BaseMnemonic { get; }

    /// <summary>
    /// Gets the mnemonic for this instruction, with optional .B suffix for byte operations.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? $"{BaseMnemonic}.B" : BaseMnemonic;

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
    /// Returns a string representation of the logical instruction.
    /// </summary>
    /// <returns>A string describing the instruction in assembly format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {InstructionHelpers.FormatOperand(_sourceRegister, _sourceAddressingMode)}, {InstructionHelpers.FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }

    /// <summary>
    /// Executes the logical instruction on the specified CPU state.
    /// Performs the logical operation between source and destination operands and updates flags accordingly.
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
            if (extensionIndex >= extensionWords.Length)
            {
                throw new ArgumentException($"Extension words array does not contain enough elements. Expected at least {extensionIndex + 1}, but got {extensionWords.Length}.", nameof(extensionWords));
            }
            sourceExtensionWord = extensionWords[extensionIndex++];
        }

        // Destination operand extension word
        if (_destinationAddressingMode == AddressingMode.Absolute ||
            _destinationAddressingMode == AddressingMode.Symbolic ||
            _destinationAddressingMode == AddressingMode.Indexed)
        {
            if (extensionIndex >= extensionWords.Length)
            {
                throw new ArgumentException($"Extension words array does not contain enough elements. Expected at least {extensionIndex + 1}, but got {extensionWords.Length}.", nameof(extensionWords));
            }
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

        // Perform the logical operation (implemented by derived classes)
        ushort result = PerformLogicalOperation(sourceValue, destinationValue);

        // For byte operations, mask to 8 bits
        if (_isByteOperation)
        {
            result &= 0xFF;
        }

        // Update flags: All logical instructions affect N, Z; clear C, V
        registerFile.StatusRegister.Zero = result == 0;
        registerFile.StatusRegister.Negative = _isByteOperation ? (result & 0x80) != 0 : (result & 0x8000) != 0;
        registerFile.StatusRegister.Carry = false;  // Logical operations always clear carry
        registerFile.StatusRegister.Overflow = false;  // Logical operations always clear overflow

        // Write result back to destination (unless this is a BIT instruction which only sets flags)
        if (ShouldWriteResult())
        {
            InstructionHelpers.WriteOperand(
                _destinationRegister,
                _destinationAddressingMode,
                _isByteOperation,
                result,
                registerFile,
                memory,
                destinationExtensionWord);
        }

        // Return cycle count (varies by addressing mode combination)
        return GetCycleCount();
    }

    /// <summary>
    /// Performs the specific logical operation for this instruction type.
    /// </summary>
    /// <param name="sourceValue">The source operand value.</param>
    /// <param name="destinationValue">The destination operand value.</param>
    /// <returns>The result of the logical operation.</returns>
    protected abstract ushort PerformLogicalOperation(ushort sourceValue, ushort destinationValue);

    /// <summary>
    /// Determines whether this instruction should write the result back to the destination.
    /// Most logical instructions write the result, but BIT instruction only sets flags.
    /// </summary>
    /// <returns>True if the result should be written to the destination, false otherwise.</returns>
    protected virtual bool ShouldWriteResult() => true;

    /// <summary>
    /// Determines whether this logical instruction is a BIT instruction
    /// that gets cycle count reduction per SLAU445I Table 4-10 footnote [1].
    /// 
    /// By default, only BIT instructions get this reduction.
    /// </summary>
    /// <returns>True if this instruction gets BIT cycle reduction, false otherwise.</returns>
    protected virtual bool IsBitInstruction => !ShouldWriteResult();

    /// <summary>
    /// Gets the number of CPU cycles required for this instruction based on addressing modes.
    /// Uses SLAU445I Table 4-10 lookup instead of additive cycle calculation.
    /// </summary>
    /// <returns>The number of CPU cycles.</returns>
    private uint GetCycleCount()
    {
        // All logical instructions now use SLAU445I Table 4-10 lookup
        // BIT instruction gets MOV/BIT/CMP cycle reduction per footnote [1]
        return InstructionCycleLookup.GetCycleCount(
            _sourceAddressingMode,
            _destinationAddressingMode,
            _sourceRegister,
            _destinationRegister,
            isMovBitOrCmpInstruction: IsBitInstruction);
    }
}
