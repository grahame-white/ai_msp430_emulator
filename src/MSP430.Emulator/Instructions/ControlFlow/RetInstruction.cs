using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.ControlFlow;

/// <summary>
/// Implements the MSP430 RET (Return from subroutine) instruction.
/// 
/// The RET instruction is an emulated instruction that returns from a subroutine by:
/// 1. Popping the return address from the stack
/// 2. Setting the PC to the return address
/// 3. Incrementing the stack pointer by 2
/// 
/// This instruction is emulated as MOV @SP+, PC and takes 4 cycles according to SLAU445I Table 4-8.
/// 
/// Operation: [SP] → PC; SP = SP + 2
/// Emulation: MOV @SP+, PC
/// Cycles: 4 (per SLAU445I Table 4-8)
/// Flags: Not affected
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.1: Table 4-8 Interrupt, Return, and Reset Cycles and Length
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: "Detailed Description" - Instruction Set
/// </summary>
public class RetInstruction : Instruction, IExecutableInstruction
{
    /// <summary>
    /// Initializes a new instance of the RetInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word (RET is emulated as MOV @SP+, PC).</param>
    public RetInstruction(ushort instructionWord)
        : base(InstructionFormat.FormatI, 0x4, instructionWord) // Emulated as MOV instruction
    {
    }

    /// <summary>
    /// Gets the source register for this instruction (always SP/R1 for stack operations).
    /// </summary>
    public override RegisterName? SourceRegister => RegisterName.R1;

    /// <summary>
    /// Gets the destination register for this instruction (always PC/R0).
    /// </summary>
    public override RegisterName? DestinationRegister => RegisterName.R0;

    /// <summary>
    /// Gets the source addressing mode for this instruction (always Indirect Auto-increment for @SP+).
    /// </summary>
    public override AddressingMode? SourceAddressingMode => AddressingMode.IndirectAutoIncrement;

    /// <summary>
    /// Gets the destination addressing mode for this instruction (always Register mode for PC).
    /// </summary>
    public override AddressingMode? DestinationAddressingMode => AddressingMode.Register;

    /// <summary>
    /// Gets a value indicating whether this instruction operates on bytes (false) or words (true).
    /// RET instructions are always word operations.
    /// </summary>
    public override bool IsByteOperation => false;

    /// <summary>
    /// Gets the number of additional words (extension words) required by this instruction.
    /// RET does not require extension words.
    /// </summary>
    public override int ExtensionWordCount => 0;

    /// <summary>
    /// Gets the mnemonic for the RET instruction.
    /// </summary>
    public override string Mnemonic => "RET";

    /// <summary>
    /// Executes the RET instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (not used for RET).</param>
    /// <returns>The number of CPU cycles consumed by this instruction (always 4 per SLAU445I Table 4-8).</returns>
    /// <exception cref="InvalidOperationException">Thrown when stack underflow is detected.</exception>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Get current stack pointer
        ushort currentSP = registerFile.GetStackPointer();

        // Check for stack overflow - ensure SP+1 is within valid memory range
        // Check that the current stack pointer location is within valid bounds
        // Stack should not go below 0x0200 (system registers area) or beyond memory length
        if (currentSP < 0x0200)
        {
            throw new InvalidOperationException($"Stack overflow detected: Stack pointer 0x{currentSP:X4} would access memory beyond bounds");
        }
        // RET always operates on words (2 bytes)
        if (currentSP + 1 >= memory.Length)
        {
            throw new InvalidOperationException($"Stack overflow detected: Stack pointer 0x{currentSP:X4} would access memory beyond bounds");
        }

        // Read return address from current stack location
        // MSP430 is little-endian
        ushort returnAddress = (ushort)(memory[currentSP] | (memory[currentSP + 1] << 8));

        // Post-increment stack pointer by 2 (stack grows downward, always operates on words)
        ushort newSP = (ushort)(currentSP + 2);

        // Check for potential stack pointer overflow
        if (newSP < currentSP) // Overflow detection
        {
            throw new InvalidOperationException($"Stack overflow detected: Stack pointer 0x{currentSP:X4} would overflow on RET operation");
        }

        // Update stack pointer (RegisterFile handles word-alignment automatically)
        registerFile.SetStackPointer(newSP);

        // Set PC to return address
        // The PC will be word-aligned automatically by RegisterFile.SetProgramCounter
        registerFile.SetProgramCounter(returnAddress);

        // RET instructions do not affect status flags per SLAU445I specification

        // Return fixed cycle count per SLAU445I Table 4-8
        return 4;
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
