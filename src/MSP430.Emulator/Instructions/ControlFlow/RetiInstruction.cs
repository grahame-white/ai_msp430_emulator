using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Implements the MSP430 RETI (Return from Interrupt) instruction.
/// 
/// The RETI instruction returns from an interrupt service routine by:
/// 1. Popping the status register (SR) from the stack and restoring it
/// 2. Popping the program counter (PC) from the stack and jumping to that address
/// 3. Incrementing the stack pointer by 4 total (2 for SR + 2 for PC)
/// 
/// This restores the complete processor state that was saved during interrupt acceptance.
/// 
/// Operation: TOS → SR, SP + 2 → SP; TOS → PC, SP + 2 → SP
/// Format: Special Format II instruction
/// Opcode: 0x1300 (binary: 0001001100000000)
/// Cycles: 5 (per SLAU445I Table 4-8)
/// Flags: All flags restored from stack (*, *, *, *)
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1.3.4.2: Return From Interrupt
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.2: Table 4-5 MSP430 Single-Operand Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.1: Table 4-8 Interrupt, Return, and Reset Cycles and Length
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.1: Extended Instruction Binary Descriptions
/// </summary>
public class RetiInstruction : Instruction, IExecutableInstruction
{
    /// <summary>
    /// Initializes a new instance of the RetiInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word (should be 0x1300).</param>
    public RetiInstruction(ushort instructionWord)
        : base(InstructionFormat.FormatII, 0x13, instructionWord)
    {
    }

    /// <summary>
    /// Gets the source register for this instruction (N/A for RETI).
    /// </summary>
    public override RegisterName? SourceRegister => null;

    /// <summary>
    /// Gets the destination register for this instruction (N/A for RETI).
    /// </summary>
    public override RegisterName? DestinationRegister => null;

    /// <summary>
    /// Gets the source addressing mode for this instruction (N/A for RETI).
    /// </summary>
    public override AddressingMode? SourceAddressingMode => null;

    /// <summary>
    /// Gets the destination addressing mode for this instruction (N/A for RETI).
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => null;

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (false) or words (true).
    /// RETI instructions are always word operations.
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// RETI does not require extension words.
    /// </summary>
    public override int ExtensionWordCount => 0;

    /// <summary>
    /// Gets the mnemonic for the RETI instruction.
    /// </summary>
    public override string Mnemonic => "RETI";

    /// <summary>
    /// Executes the RETI instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (not used for RETI).</param>
    /// <returns>The number of CPU cycles consumed by this instruction (always 5 per SLAU445I Table 4-8).</returns>
    /// <exception cref="InvalidOperationException">Thrown when stack underflow is detected.</exception>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Get current stack pointer
        ushort currentSP = registerFile.GetStackPointer();

        // Check for stack overflow - ensure SP+3 is within valid memory range
        // Note: This checks if SP would access beyond memory bounds but uses
        // standardized "Stack overflow detected" message per CONTRIBUTING.md guidelines
        // RETI needs to read 4 bytes total (SR + PC), so we need SP+3 to be valid
        if (currentSP + 3 >= memory.Length)
        {
            throw new InvalidOperationException($"Stack overflow detected: Stack pointer 0x{currentSP:X4} would access memory beyond bounds");
        }

        // Step 1: Pop status register from stack (TOS → SR, SP + 2 → SP)
        // Read SR from current stack location (MSP430 is little-endian)
        ushort statusRegisterValue = (ushort)(memory[currentSP] | (memory[currentSP + 1] << 8));

        // Increment stack pointer by 2 for SR
        ushort newSP = (ushort)(currentSP + 2);

        // Check for potential stack pointer overflow after first increment
        if (newSP < currentSP) // Overflow detection
        {
            throw new InvalidOperationException($"Stack overflow detected: Stack pointer 0x{currentSP:X4} would overflow on RETI operation");
        }

        // Step 2: Pop program counter from stack (TOS → PC, SP + 2 → SP)
        // Read PC from new stack location (MSP430 is little-endian)
        ushort programCounterValue = (ushort)(memory[newSP] | (memory[newSP + 1] << 8));

        // Increment stack pointer by 2 more for PC (total +4 from original SP)
        ushort finalSP = (ushort)(newSP + 2);

        // Check for potential stack pointer overflow after second increment
        if (finalSP < newSP) // Overflow detection
        {
            throw new InvalidOperationException($"Stack overflow detected: Stack pointer 0x{newSP:X4} would overflow on RETI operation");
        }

        // Step 3: Restore the status register (this restores all flags and CPU state)
        registerFile.StatusRegister.Value = statusRegisterValue;

        // Step 4: Update stack pointer
        registerFile.SetStackPointer(finalSP);

        // Step 5: Set PC to return address (interrupt returns to where it was interrupted)
        // The PC will be word-aligned automatically by RegisterFile.SetProgramCounter
        registerFile.SetProgramCounter(programCounterValue);

        // RETI restores all status flags from the stack per SLAU445I specification
        // The flags are already restored by setting the SR value above

        // Return fixed cycle count per SLAU445I Table 4-8
        return 5;
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        return Mnemonic;
    }
}
