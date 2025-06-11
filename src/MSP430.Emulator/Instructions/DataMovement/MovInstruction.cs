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
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - October 2004–Revised June 2021, Section 4: "Assembler Description" - Instruction format and operation
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.5.1.1: "MSP430 Double-Operand (Format I) Instructions" - Instruction encoding
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: "Detailed Description" - Instruction Set
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
        if (ShouldUpdateFlags())
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
    /// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.3.3: "Status Register (SR)" - Special handling when SR is destination
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
    /// Determines whether flags should be updated for this MOV instruction.
    /// 
    /// Flags are not updated when the destination is R2 (Status Register) in register mode
    /// to avoid modifying the value being written to the Status Register.
    /// For non-register modes (Absolute, Symbolic, etc.), flags are updated normally 
    /// even when R2 is the target register.
    /// 
    /// References:
    /// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.3.3: "Status Register (SR)" - Special handling when SR is destination
    /// </summary>
    /// <returns>True if flags should be updated, false otherwise.</returns>
    private bool ShouldUpdateFlags()
    {
        return _destinationRegister != RegisterName.R2 || _destinationAddressingMode != AddressingMode.Register;
    }

    /// <summary>
    /// Gets the number of CPU cycles required for this instruction based on addressing modes.
    /// Uses SLAU445I Table 4-10 lookup instead of additive cycle calculation.
    /// </summary>
    /// <returns>The number of CPU cycles.</returns>
    private uint GetCycleCount()
    {
        return InstructionCycleLookup.GetCycleCount(
            _sourceAddressingMode,
            _destinationAddressingMode,
            _sourceRegister,
            _destinationRegister,
            isMovBitOrCmpInstruction: true); // MOV gets cycle reduction per Table 4-10 footnote [1]
    }


    /// <summary>
    /// Extracts extension words for source and destination operands based on their addressing modes.
    /// </summary>
    /// <param name="extensionWords">Array of extension words from the instruction.</param>
    /// <param name="sourceExtensionWord">Output parameter for the source extension word.</param>
    /// <param name="destinationExtensionWord">Output parameter for the destination extension word.</param>
    /// <exception cref="ArgumentException">Thrown when required extension words are missing.</exception>
    private void ExtractExtensionWords(ushort[] extensionWords, out ushort sourceExtensionWord, out ushort destinationExtensionWord)
    {
        sourceExtensionWord = 0;
        destinationExtensionWord = 0;

        int index = 0;
        bool sourceRequiresExtension = AddressingModeDecoder.RequiresExtensionWord(_sourceAddressingMode);
        bool destinationRequiresExtension = AddressingModeDecoder.RequiresExtensionWord(_destinationAddressingMode);

        // Validate that we have enough extension words
        int requiredWords = (sourceRequiresExtension ? 1 : 0) + (destinationRequiresExtension ? 1 : 0);
        if (extensionWords.Length < requiredWords)
        {
            throw new ArgumentException($"Instruction requires {requiredWords} extension words but only {extensionWords.Length} provided", nameof(extensionWords));
        }

        // Extract source extension word if needed
        if (sourceRequiresExtension)
        {
            sourceExtensionWord = extensionWords[index++];
        }

        // Extract destination extension word if needed
        if (destinationRequiresExtension)
        {
            destinationExtensionWord = extensionWords[index];
        }
    }

    /// <summary>
    /// Formats an operand for display in assembly format.
    /// </summary>
    /// <param name="register">The register used by the operand.</param>
    /// <param name="addressingMode">The addressing mode of the operand.</param>
    /// <param name="extensionWord">Optional extension word containing immediate values, addresses, or offsets. If null, generic placeholders are used.</param>
    /// <returns>A formatted string representing the operand.</returns>
    private string FormatOperand(RegisterName register, AddressingMode addressingMode, ushort? extensionWord = null)
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
            _ => throw new ArgumentOutOfRangeException(nameof(addressingMode), addressingMode, "Unrecognized addressing mode")
        };
    }
}
