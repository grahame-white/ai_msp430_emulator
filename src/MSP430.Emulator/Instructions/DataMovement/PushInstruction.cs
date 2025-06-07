using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.DataMovement;

/// <summary>
/// Implements the MSP430 PUSH instruction.
/// 
/// The PUSH instruction decrements the stack pointer by 2, then stores the source operand
/// at the new stack pointer location. This is a Format II (single-operand) instruction.
/// 
/// Operation: SP = SP - 2; [SP] = src
/// Format: PUSH src
/// Opcode: 0x12 (Format II)
/// Flags: Not affected
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - October 2004–Revised June 2021, Section 4: "Assembler Description" - Instruction format and operation
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.6.2.35: "PUSH" - Instruction specification
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: "Detailed Description" - Instruction Set
/// </summary>
public class PushInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _sourceRegister;
    private readonly AddressingMode _sourceAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Initializes a new instance of the PushInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public PushInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        AddressingMode sourceAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatII, 0x12, instructionWord)
    {
        _sourceRegister = sourceRegister;
        _sourceAddressingMode = sourceAddressingMode;
        _isByteOperation = isByteOperation;
    }

    /// <summary>
    /// Gets the source register for this instruction.
    /// </summary>
    public override RegisterName? SourceRegister => _sourceRegister;

    /// <summary>
    /// Gets the source addressing mode for this instruction.
    /// </summary>
    public override AddressingMode? SourceAddressingMode => _sourceAddressingMode;

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (true) or words (false).
    /// </summary>
    public override bool IsByteOperation => _isByteOperation;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// </summary>
    public override int ExtensionWordCount => AddressingModeDecoder.RequiresExtensionWord(_sourceAddressingMode) ? 1 : 0;

    /// <summary>
    /// Gets the mnemonic for the PUSH instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "PUSH.B" : "PUSH";

    /// <summary>
    /// Executes the PUSH instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (immediate values, addresses, offsets).</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    /// <exception cref="InvalidOperationException">Thrown when stack overflow is detected.</exception>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Get extension word if needed
        ushort extensionWord = ExtensionWordCount > 0 ? extensionWords[0] : (ushort)0;

        // Read source operand
        ushort sourceValue = InstructionHelpers.ReadOperand(
            _sourceRegister,
            _sourceAddressingMode,
            _isByteOperation,
            registerFile,
            memory,
            extensionWord);

        // Get current stack pointer
        ushort currentSP = registerFile.GetStackPointer();

        // Check for stack overflow - ensure SP-2 is still within valid memory range
        if (currentSP < 2)
        {
            throw new InvalidOperationException($"Stack overflow detected: Stack pointer 0x{currentSP:X4} would underflow on PUSH operation");
        }

        // Pre-decrement stack pointer by 2 (stack grows downward, always operates on words)
        ushort newSP = (ushort)(currentSP - 2);

        // Check that the new stack pointer location is within memory bounds
        if (newSP + 1 >= memory.Length)
        {
            throw new InvalidOperationException($"Stack overflow detected: Stack pointer 0x{newSP:X4} would access memory beyond bounds");
        }

        // Update stack pointer (RegisterFile handles word-alignment automatically)
        registerFile.SetStackPointer(newSP);

        // Store the value at the new stack location
        // Note: PUSH always stores a full word (16-bit), even for byte operations
        // For byte operations, the byte value is sign-extended to 16 bits
        ushort valueToStore = _isByteOperation ? InstructionHelpers.SignExtendByte(sourceValue) : sourceValue;

        // Write to stack (always as word) - direct memory access for stack operations
        // MSP430 is little-endian
        memory[newSP] = (byte)(valueToStore & 0xFF);
        memory[newSP + 1] = (byte)((valueToStore >> 8) & 0xFF);

        // Return cycle count based on addressing mode
        return InstructionHelpers.GetSingleOperandCycleCount(_sourceAddressingMode);
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        string suffix = IsByteOperation ? ".B" : "";
        return $"{Mnemonic.Split('.')[0]}{suffix} {InstructionHelpers.FormatOperand(_sourceRegister, _sourceAddressingMode)}";
    }
}
