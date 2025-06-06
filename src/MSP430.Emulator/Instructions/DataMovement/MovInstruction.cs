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
        ushort sourceExtensionWord = 0;
        ushort destinationExtensionWord = 0;

        if (AddressingModeDecoder.RequiresExtensionWord(_sourceAddressingMode))
        {
            sourceExtensionWord = extensionWords[0];
            if (AddressingModeDecoder.RequiresExtensionWord(_destinationAddressingMode))
            {
                destinationExtensionWord = extensionWords[1];
            }
        }
        else if (AddressingModeDecoder.RequiresExtensionWord(_destinationAddressingMode))
        {
            destinationExtensionWord = extensionWords[0];
        }

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
        // Note: There are edge cases with R2 (Status Register) that need further investigation
        UpdateFlags(sourceValue, registerFile.StatusRegister);

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
    /// Updates CPU flags based on the moved value.
    /// MOV instruction sets N and Z flags based on the value, clears V flag, and preserves C flag.
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

    /// <summary>
    /// Formats an operand for display in assembly format.
    /// </summary>
    /// <param name="register">The register used by the operand.</param>
    /// <param name="addressingMode">The addressing mode of the operand.</param>
    /// <returns>A formatted string representing the operand.</returns>
    private static string FormatOperand(RegisterName register, AddressingMode addressingMode)
    {
        return addressingMode switch
        {
            AddressingMode.Register => register.ToString(),
            AddressingMode.Indexed => $"X({register})",
            AddressingMode.Indirect => $"@{register}",
            AddressingMode.IndirectAutoIncrement => $"@{register}+",
            AddressingMode.Immediate => "#N",
            AddressingMode.Absolute => "&ADDR",
            AddressingMode.Symbolic => "ADDR",
            _ => "?"
        };
    }
}
