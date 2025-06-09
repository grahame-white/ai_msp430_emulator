using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.Arithmetic;

/// <summary>
/// Base class for MSP430 two-operand arithmetic instructions.
/// 
/// Provides common implementation for arithmetic operations (ADD, SUB, CMP)
/// that share similar instruction format, addressing modes, and basic structure.
/// All two-operand arithmetic instructions use Format I encoding.
/// </summary>
public abstract class ArithmeticInstruction : Instruction, IExecutableInstruction
{
    /// <summary>
    /// Initializes a new instance of the ArithmeticInstruction class.
    /// </summary>
    /// <param name="opcode">The instruction opcode.</param>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    protected ArithmeticInstruction(
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
    /// Gets the base mnemonic for this arithmetic instruction (to be overridden by derived classes).
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
    /// Returns a string representation of the arithmetic instruction.
    /// </summary>
    /// <returns>A string describing the instruction in assembly format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {InstructionHelpers.FormatOperand(_sourceRegister, _sourceAddressingMode)}, {InstructionHelpers.FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }

    /// <summary>
    /// Executes the arithmetic instruction on the specified CPU state.
    /// Performs the arithmetic operation between source and destination operands and updates flags accordingly.
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
        if (NeedsExtensionWord(_sourceRegister, _sourceAddressingMode))
        {
            if (extensionIndex >= extensionWords.Length)
            {
                throw new ArgumentException($"Extension words array does not contain enough elements. Expected at least {extensionIndex + 1}, but got {extensionWords.Length}.", nameof(extensionWords));
            }
            sourceExtensionWord = extensionWords[extensionIndex++];
        }

        // Destination operand extension word
        if (NeedsExtensionWord(_destinationRegister, _destinationAddressingMode))
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

        // Read destination operand (note: destination operands never use constant generators)
        ushort destinationValue = InstructionHelpers.ReadDestinationOperand(
            _destinationRegister,
            _destinationAddressingMode,
            _isByteOperation,
            registerFile,
            memory,
            destinationExtensionWord);

        // Perform the arithmetic operation (implemented by derived classes)
        (ushort result, bool carry, bool overflow) = PerformArithmeticOperation(sourceValue, destinationValue, _isByteOperation);

        // For byte operations, mask to 8 bits
        if (_isByteOperation)
        {
            result &= 0xFF;
        }

        // Update flags: All arithmetic instructions affect N, Z, C, V
        InstructionHelpers.UpdateFlags(result, carry, overflow, _isByteOperation, registerFile.StatusRegister);

        // Write result back to destination (unless this is a CMP instruction which only sets flags)
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
    /// Performs the specific arithmetic operation for this instruction type.
    /// </summary>
    /// <param name="sourceValue">The source operand value.</param>
    /// <param name="destinationValue">The destination operand value.</param>
    /// <param name="isByteOperation">True for byte operations, false for word operations.</param>
    /// <returns>A tuple containing the result, carry flag, and overflow flag.</returns>
    protected abstract (ushort result, bool carry, bool overflow) PerformArithmeticOperation(ushort sourceValue, ushort destinationValue, bool isByteOperation);

    /// <summary>
    /// Determines whether this instruction should write the result back to the destination.
    /// Most arithmetic instructions write the result, but CMP instruction only sets flags.
    /// </summary>
    /// <returns>True if the result should be written to the destination, false otherwise.</returns>
    protected virtual bool ShouldWriteResult() => true;

    /// <summary>
    /// Gets the number of CPU cycles required for this instruction based on addressing modes.
    /// </summary>
    /// <returns>The number of CPU cycles.</returns>
    private uint GetCycleCount()
    {
        // Base cycle count for Format I instructions
        uint baseCycles = 1;

        // Add cycles for source addressing mode (account for constant generators)
        uint sourceCycles = IsConstantGenerator(_sourceRegister, _sourceAddressingMode)
            ? 0u  // Constant generators take 0 additional cycles
            : _sourceAddressingMode switch
            {
                AddressingMode.Register => 0u,
                AddressingMode.Indexed => 3u,
                AddressingMode.Indirect => 2u,
                AddressingMode.IndirectAutoIncrement => 2u,
                AddressingMode.Immediate => 0u,
                AddressingMode.Absolute => 3u,
                AddressingMode.Symbolic => 3u,
                _ => 0u
            };

        // Add cycles for destination addressing mode (never constant generators)
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

    /// <summary>
    /// Determines if a register and addressing mode combination needs an extension word.
    /// Constant generators don't need extension words even if they use Immediate mode.
    /// </summary>
    /// <param name="register">The register.</param>
    /// <param name="addressingMode">The addressing mode.</param>
    /// <returns>True if an extension word is needed, false otherwise.</returns>
    private static bool NeedsExtensionWord(RegisterName register, AddressingMode addressingMode)
    {
        // Constant generators don't need extension words
        if (IsConstantGenerator(register, addressingMode))
        {
            return false;
        }

        // Regular extension word requirements
        return addressingMode == AddressingMode.Immediate ||
               addressingMode == AddressingMode.Absolute ||
               addressingMode == AddressingMode.Symbolic ||
               addressingMode == AddressingMode.Indexed;
    }

    /// <summary>
    /// Determines if a register and addressing mode combination represents a constant generator.
    /// </summary>
    /// <param name="register">The register.</param>
    /// <param name="addressingMode">The addressing mode.</param>
    /// <returns>True if this is a constant generator, false otherwise.</returns>
    private static bool IsConstantGenerator(RegisterName register, AddressingMode addressingMode)
    {
        return register switch
        {
            // R2 constant generators: As=10 (Indirect) → +4, As=11 (IndirectAutoIncrement) → +8
            // R2 As=01 (Absolute) is NOT a constant generator - it's legitimate absolute addressing
            RegisterName.R2 => addressingMode == AddressingMode.Indirect || addressingMode == AddressingMode.IndirectAutoIncrement,

            // R3 constant generators: As=00 (Register) → 0, As=01/10/11 (Immediate) → 1/2/-1
            RegisterName.R3 => addressingMode == AddressingMode.Register || addressingMode == AddressingMode.Immediate,
            _ => false
        };
    }
}
