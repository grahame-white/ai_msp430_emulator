using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Implements the MSP430 CALL instruction.
/// 
/// The CALL instruction performs a subroutine call by:
/// 1. Evaluating the destination address
/// 2. Decrementing the stack pointer by 2
/// 3. Pushing the current PC (return address) onto the stack
/// 4. Setting PC to the destination address
/// 
/// This is a Format II (single-operand) instruction that supports all source addressing modes.
/// 
/// Operation: dst → tmp; SP = SP - 2; PC → [SP]; tmp → PC
/// Format: CALL src
/// Opcode: 0x12 (Format II), bits 7:6 = 10 (distinguishes from PUSH)
/// Flags: Not affected
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2.9: CALL
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: "Detailed Description" - Instruction Set
/// </summary>
public class CallInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _sourceRegister;
    private readonly AddressingMode _sourceAddressingMode;

    /// <summary>
    /// Initializes a new instance of the CallInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceRegister">The source register.</param>
    /// <param name="sourceAddressingMode">The source addressing mode.</param>
    public CallInstruction(
        ushort instructionWord,
        RegisterName sourceRegister,
        AddressingMode sourceAddressingMode)
        : base(InstructionFormat.FormatII, 0x12, instructionWord)
    {
        _sourceRegister = sourceRegister;
        _sourceAddressingMode = sourceAddressingMode;
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
    /// Gets a value indicating whether this instruction operates on bytes (false) or words (true).
    /// CALL instructions are always word operations.
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// </summary>
    public override int ExtensionWordCount => AddressingModeDecoder.RequiresExtensionWord(_sourceAddressingMode) ? 1 : 0;

    /// <summary>
    /// Gets the mnemonic for the CALL instruction.
    /// </summary>
    public override string Mnemonic => "CALL";

    /// <summary>
    /// Executes the CALL instruction on the specified CPU state.
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

        // 1. Evaluate destination address and store temporarily
        ushort destinationAddress = InstructionHelpers.ReadOperand(
            _sourceRegister,
            _sourceAddressingMode,
            false, // Always word operation
            registerFile,
            memory,
            extensionWord);

        // 2. Get current PC (will be return address after incrementing for next instruction)
        ushort returnAddress = registerFile.GetProgramCounter();

        // 3. Get current stack pointer
        ushort currentSP = registerFile.GetStackPointer();

        // 4. Check for stack overflow - ensure SP-2 is still within valid memory range
        if (currentSP < 2)
        {
            throw new InvalidOperationException($"Stack overflow detected: Stack pointer 0x{currentSP:X4} would underflow on CALL operation");
        }

        // 5. Pre-decrement stack pointer by 2 (stack grows downward, always operates on words)
        ushort newSP = (ushort)(currentSP - 2);

        // 6. Check that the new stack pointer location is within memory bounds
        if (newSP + 1 >= memory.Length)
        {
            throw new InvalidOperationException($"Stack overflow detected: Stack pointer 0x{newSP:X4} would access memory beyond bounds");
        }

        // 7. Update stack pointer (RegisterFile handles word-alignment automatically)
        registerFile.SetStackPointer(newSP);

        // 8. Push return address onto stack
        // Write to stack (always as word) - direct memory access for stack operations
        // MSP430 is little-endian
        memory[newSP] = (byte)(returnAddress & 0xFF);
        memory[newSP + 1] = (byte)((returnAddress >> 8) & 0xFF);

        // 9. Set PC to destination address
        // The PC will be word-aligned automatically by RegisterFile.SetProgramCounter
        registerFile.SetProgramCounter(destinationAddress);

        // CALL instructions do not affect status flags per SLAU445I specification

        // Return cycle count based on addressing mode per SLAU445I Table 4-9
        return GetCallInstructionCycleCount(_sourceAddressingMode);
    }

    /// <summary>
    /// Gets the number of CPU cycles for a CALL instruction based on addressing mode.
    /// Based on SLAU445I Table 4-9: MSP430 Format II Instruction Cycles and Length.
    /// </summary>
    /// <param name="addressingMode">The addressing mode.</param>
    /// <returns>The number of CPU cycles per SLAU445I Table 4-9.</returns>
    private static uint GetCallInstructionCycleCount(AddressingMode addressingMode)
    {
        return addressingMode switch
        {
            AddressingMode.Register => 4,                  // Rn
            AddressingMode.Indirect => 4,                  // @Rn  
            AddressingMode.IndirectAutoIncrement => 4,     // @Rn+
            AddressingMode.Immediate => 4,                 // #N
            AddressingMode.Indexed => 5,                   // X(Rn)
            AddressingMode.Symbolic => 5,                  // EDE (PC-relative)
            AddressingMode.Absolute => 6,                  // &EDE
            _ => 4 // Default for unknown modes
        };
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        return $"{Mnemonic} {InstructionHelpers.FormatOperand(_sourceRegister, _sourceAddressingMode)}";
    }
}
