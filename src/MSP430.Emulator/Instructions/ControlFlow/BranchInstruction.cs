using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Represents the BR (Branch) instruction for the MSP430 CPU.
/// 
/// The BR instruction performs an unconditional branch to an address anywhere in the
/// lower 64K address space. It supports all source addressing modes and is implemented
/// as a MOV instruction with the Program Counter (PC) as the destination.
/// 
/// Operation: src → PC
/// Emulation: MOV src,PC
/// 
/// The branch instruction is a word instruction that does not affect status flags.
/// All source addressing modes can be used (register, indexed, indirect, immediate, etc.).
/// 
/// Implementation based on:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.8: BR, BRANCH
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Instruction Set
/// </summary>
public class BranchInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _sourceRegister;
    private readonly AddressingMode _sourceAddressingMode;

    /// <summary>
    /// Initializes a new instance of the BranchInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    public BranchInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        AddressingMode sourceAddressingMode)
        : base(InstructionFormat.FormatI, 0x4, instructionWord) // Same opcode as MOV instruction
    {
        _sourceRegister = sourceRegister;
        _sourceAddressingMode = sourceAddressingMode;
    }

    /// <summary>
    /// Gets the source register for this instruction.
    /// </summary>
    public override RegisterName? SourceRegister => _sourceRegister;

    /// <summary>
    /// Gets the destination register for this instruction (always PC/R0).
    /// </summary>
    public override RegisterName? DestinationRegister => RegisterName.R0;

    /// <summary>
    /// Gets the source addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? SourceAddressingMode => _sourceAddressingMode;

    /// <summary>
    /// Gets the destination addressing mode for this instruction (always Register mode for PC).
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => AddressingMode.Register;

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (false) or words (true).
    /// Branch instructions are always word operations.
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// </summary>
    public override int ExtensionWordCount =>
        AddressingModeDecoder.RequiresExtensionWord(_sourceAddressingMode) ? 1 : 0;

    /// <summary>
    /// Gets the mnemonic for the BR instruction.
    /// </summary>
    public override string Mnemonic => "BR";

    /// <summary>
    /// Executes the BR instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (immediate values, addresses, offsets).</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Extract extension word if needed
        ushort sourceExtensionWord = 0;
        if (AddressingModeDecoder.RequiresExtensionWord(_sourceAddressingMode))
        {
            if (extensionWords.Length == 0)
            {
                throw new ArgumentException("Instruction requires 1 extension word but none provided", nameof(extensionWords));
            }
            if (extensionWords.Length > 1)
            {
                throw new ArgumentException("Instruction requires exactly 1 extension word but multiple provided", nameof(extensionWords));
            }
            sourceExtensionWord = extensionWords[0];
        }

        // Read source operand
        ushort sourceValue = InstructionHelpers.ReadOperand(
            _sourceRegister,
            _sourceAddressingMode,
            false, // Always word operation
            registerFile,
            memory,
            sourceExtensionWord);

        // Branch: Set PC to source value
        // The PC will be word-aligned automatically by RegisterFile.SetProgramCounter
        registerFile.SetProgramCounter(sourceValue);

        // Branch instructions do not affect status flags per SLAU445I specification

        // Return cycle count based on source addressing mode
        return GetCycleCount();
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {FormatOperand(_sourceRegister, _sourceAddressingMode)}";
    }

    /// <summary>
    /// Returns a detailed string representation of the instruction with actual values.
    /// </summary>
    /// <param name="extensionWords">Extension words containing immediate values, addresses, or offsets.</param>
    /// <returns>A string describing the instruction with actual values shown.</returns>
    public string ToString(ushort[] extensionWords)
    {
        ushort? sourceExtensionWord = null;
        if (AddressingModeDecoder.RequiresExtensionWord(_sourceAddressingMode) && extensionWords.Length > 0)
        {
            sourceExtensionWord = extensionWords[0];
        }

        return $"{Mnemonic} {FormatOperand(_sourceRegister, _sourceAddressingMode, sourceExtensionWord)}";
    }

    /// <summary>
    /// Gets the number of CPU cycles required for this instruction based on source addressing mode.
    /// Uses SLAU445I Table 4-10 lookup for MOV instruction cycles with PC as destination.
    /// </summary>
    /// <returns>The number of CPU cycles.</returns>
    private uint GetCycleCount()
    {
        return InstructionCycleLookup.GetCycleCount(
            _sourceAddressingMode,
            AddressingMode.Register, // Destination is always PC in register mode
            _sourceRegister,
            RegisterName.R0, // Destination is always PC
            isMovBitOrCmpInstruction: true); // BR gets same cycle reduction as MOV per Table 4-10 footnote [1]
    }

    /// <summary>
    /// Formats an operand for display in assembly format.
    /// 
    /// MSP430 Assembly Conventions:
    /// - Absolute mode: Uses '&' prefix (e.g., &0x8000)
    /// - Symbolic mode: No prefix (e.g., 0x1000) - follows MSP430 assembly standard
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
