using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.DataMovement;

/// <summary>
/// Implements the MSP430 MOV (Move) instruction.
/// 
/// The MOV instruction copies data from the source operand to the destination operand.
/// It supports all addressing modes for both source and destination operands.
/// This is a Format I (two-operand) instruction with opcode 0x4.
/// 
/// Operation: dst = src
/// Flags: Sets N and Z based on the value moved, clears V, preserves C
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131), Section 5.3.17: "MOV - Move" - Instruction format and operation
/// - MSP430FR2xx/FR4xx Family User's Guide (SLAU445I), Section 4.3.1: "Format I Instructions" - Instruction encoding
/// - MSP430FR2355 Datasheet (SLAS847G), Section 6.12: "Instruction Set" - Opcode definition and flag behavior
/// </summary>
public class MovInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _sourceRegister;
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _sourceAddressingMode;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Initializes a new instance of the MovInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public MovInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        AddressingMode sourceAddressingMode,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatI, 0x4, instructionWord)
    {
        _sourceRegister = sourceRegister;
        _destinationRegister = destinationRegister;
        _sourceAddressingMode = sourceAddressingMode;
        _destinationAddressingMode = destinationAddressingMode;
        _isByteOperation = isByteOperation;
    }

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
    /// Gets a value indicating whether this instruction operates on bytes (true) or words (false).
    /// </summary>
    public override bool IsByteOperation => _isByteOperation;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// </summary>
    public override int ExtensionWordCount => InstructionHelpers.CalculateFormatIExtensionWordCount(_sourceAddressingMode, _destinationAddressingMode);

    /// <summary>
    /// Gets the mnemonic for the MOV instruction.
    /// </summary>
    public override string Mnemonic => "MOV";

    /// <summary>
    /// Executes the MOV instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (immediate values, addresses, offsets).</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Determine extension words for source and destination
        ExtractExtensionWords(extensionWords, out ushort sourceExtensionWord, out ushort destinationExtensionWord);

        // Read source operand
        ushort sourceValue = InstructionHelpers.ReadOperand(
            _sourceRegister,
            _sourceAddressingMode,
            _isByteOperation,
            registerFile,
            memory,
            sourceExtensionWord);

        // Write to destination operand
        InstructionHelpers.WriteOperand(
            _destinationRegister,
            _destinationAddressingMode,
            _isByteOperation,
            sourceValue,
            registerFile,
            memory,
            destinationExtensionWord);

        // Update flags: MOV sets N and Z based on the value moved, clears V, preserves C
        // Special case: When destination is R2 (Status Register) in register mode, flags are not updated
        // to avoid modifying the value being written to the Status Register
        // For non-register modes (Absolute, Symbolic, etc.), flags are updated normally
        if (_destinationRegister != RegisterName.R2 || _destinationAddressingMode != AddressingMode.Register)
        {
            UpdateFlags(sourceValue, registerFile.StatusRegister);
        }

        // Return cycle count based on addressing modes
        return GetCycleCount();
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        string suffix = IsByteOperation ? ".B" : "";
        return $"{Mnemonic}{suffix} {FormatOperand(_sourceRegister, _sourceAddressingMode)}, {FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }

    /// <summary>
    /// Returns a detailed string representation of the instruction with actual values.
    /// </summary>
    /// <param name="extensionWords">Extension words containing immediate values, addresses, or offsets.</param>
    /// <returns>A string describing the instruction with actual values shown.</returns>
    public string ToString(ushort[] extensionWords)
    {
        string suffix = IsByteOperation ? ".B" : "";

        // Determine extension words for source and destination
        ExtractExtensionWords(extensionWords, out ushort sourceExtensionWord, out ushort destinationExtensionWord);

        return $"{Mnemonic}{suffix} {FormatOperand(_sourceRegister, _sourceAddressingMode, sourceExtensionWord)}, {FormatOperand(_destinationRegister, _destinationAddressingMode, destinationExtensionWord)}";
    }

    /// <summary>
    /// Updates CPU flags based on the moved value.
    /// MOV instruction sets N and Z flags based on the value, clears V flag, and preserves C flag.
    /// 
    /// Special case: When destination is R2 (Status Register) in register mode, flags are not updated
    /// to avoid modifying the value being written to the Status Register.
    /// For non-register modes (Absolute, Symbolic, etc.), flags are updated normally even when R2 is the target register.
    /// This follows MSP430 specification behavior where writing to SR in register mode doesn't trigger flag updates.
    /// 
    /// References:
    /// - MSP430FR2xx/FR4xx Family User's Guide (SLAU445I), Section 4.2.1: "Status Register" - Special handling when SR is destination
    /// </summary>
    /// <param name="value">The value that was moved.</param>
    /// <param name="statusRegister">The CPU status register to update.</param>
    private void UpdateFlags(ushort value, StatusRegister statusRegister)
    {
        // Zero flag: Set if result is zero
        statusRegister.Zero = value == 0;

        // Negative flag: Set if the sign bit is set
        statusRegister.Negative = (value & (_isByteOperation ? 0x80 : 0x8000)) != 0;

        // Overflow flag: Always cleared for MOV instruction
        statusRegister.Overflow = false;

        // Carry flag: Preserved (not modified by MOV)
    }

    /// <summary>
    /// Gets the number of CPU cycles required for this instruction based on addressing modes.
    /// </summary>
    /// <returns>The number of CPU cycles.</returns>
    private uint GetCycleCount()
    {
        // Base cycle count for Format I instructions
        uint baseCycles = 1;

        // Add cycles for source and destination addressing modes
        uint sourceCycles = GetAddressingModeCycles(_sourceAddressingMode);
        uint destinationCycles = GetAddressingModeCycles(_destinationAddressingMode);

        return baseCycles + sourceCycles + destinationCycles;
    }

    /// <summary>
    /// Gets the number of additional CPU cycles required for a specific addressing mode.
    /// </summary>
    /// <param name="addressingMode">The addressing mode to get cycles for.</param>
    /// <returns>The number of additional CPU cycles.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unrecognized addressing mode is provided.</exception>
    private static uint GetAddressingModeCycles(AddressingMode addressingMode)
    {
        return addressingMode switch
        {
            AddressingMode.Register => 0,
            AddressingMode.Indexed => 3,
            AddressingMode.Indirect => 2,
            AddressingMode.IndirectAutoIncrement => 2,
            AddressingMode.Immediate => 0,
            AddressingMode.Absolute => 3,
            AddressingMode.Symbolic => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(addressingMode), addressingMode, "Unrecognized addressing mode")
        };
    }

    /// <summary>
    /// Extracts extension words for source and destination operands based on their addressing modes.
    /// </summary>
    /// <param name="extensionWords">Array of extension words from the instruction.</param>
    /// <param name="sourceExtensionWord">Output parameter for the source extension word.</param>
    /// <param name="destinationExtensionWord">Output parameter for the destination extension word.</param>
    private void ExtractExtensionWords(ushort[] extensionWords, out ushort sourceExtensionWord, out ushort destinationExtensionWord)
    {
        sourceExtensionWord = 0;
        destinationExtensionWord = 0;

        if (AddressingModeDecoder.RequiresExtensionWord(_sourceAddressingMode))
        {
            sourceExtensionWord = extensionWords.Length > 0 ? extensionWords[0] : (ushort)0;
            if (AddressingModeDecoder.RequiresExtensionWord(_destinationAddressingMode))
            {
                destinationExtensionWord = extensionWords.Length > 1 ? extensionWords[1] : (ushort)0;
            }
        }
        else if (AddressingModeDecoder.RequiresExtensionWord(_destinationAddressingMode))
        {
            destinationExtensionWord = extensionWords.Length > 0 ? extensionWords[0] : (ushort)0;
        }
    }

    /// <summary>
    /// Formats an operand for display in assembly format.
    /// </summary>
    /// <param name="register">The register used by the operand.</param>
    /// <param name="addressingMode">The addressing mode of the operand.</param>
    /// <param name="extensionWord">Optional extension word containing immediate values, addresses, or offsets. If null, generic placeholders are used.</param>
    /// <returns>A formatted string representing the operand.</returns>
    private static string FormatOperand(RegisterName register, AddressingMode addressingMode, ushort? extensionWord = null)
    {
        return addressingMode switch
        {
            AddressingMode.Register => register.ToString(),
            AddressingMode.Indexed => extensionWord.HasValue ? $"0x{extensionWord.Value:X}({register})" : $"X({register})",
            AddressingMode.Indirect => $"@{register}",
            AddressingMode.IndirectAutoIncrement => $"@{register}+",
            AddressingMode.Immediate => extensionWord.HasValue ? $"#0x{extensionWord.Value:X}" : "#",
            AddressingMode.Absolute => extensionWord.HasValue ? $"&0x{extensionWord.Value:X}" : "&ADDR",
            AddressingMode.Symbolic => extensionWord.HasValue ? $"0x{extensionWord.Value:X}" : "ADDR",
            _ => "?"
        };
    }
}
