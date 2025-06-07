using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions.DataMovement;

/// <summary>
/// Implements the MSP430 POP instruction.
/// 
/// The POP instruction loads the source operand from the current stack pointer location,
/// then increments the stack pointer by 2. This is a Format II (single-operand) instruction.
/// 
/// Operation: dst = [SP]; SP = SP + 2
/// Format: POP dst
/// Opcode: 0x13 (Format II)
/// Flags: Not affected
/// 
/// References:
/// - MSP430 Assembly Language Tools User's Guide (SLAU131Y) - October 2004–Revised June 2021, Section 4: "Assembler Description" - Instruction format and operation
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.6.2.34: "POP" - Instruction specification
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6: "Detailed Description" - Instruction Set
/// </summary>
public class PopInstruction : Instruction, IExecutableInstruction
{
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;

    /// <summary>
    /// Initializes a new instance of the PopInstruction class.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="destinationRegister">The destination register.</param>
    /// <param name="destinationAddressingMode">The destination addressing mode.</param>
    /// <param name="isByteOperation">True if this is a byte operation, false for word operation.</param>
    public PopInstruction(
        ushort instructionWord,
        RegisterName destinationRegister,
        AddressingMode destinationAddressingMode,
        bool isByteOperation)
        : base(InstructionFormat.FormatII, 0x13, instructionWord)
    {
        _destinationRegister = destinationRegister;
        _destinationAddressingMode = destinationAddressingMode;
        _isByteOperation = isByteOperation;
    }

    /// <summary>
    /// Gets the destination register for this instruction.
    /// </summary>
    public override RegisterName? DestinationRegister => _destinationRegister;

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
    public override int ExtensionWordCount => AddressingModeDecoder.RequiresExtensionWord(_destinationAddressingMode) ? 1 : 0;

    /// <summary>
    /// Gets the mnemonic for the POP instruction.
    /// </summary>
    public override string Mnemonic => _isByteOperation ? "POP.B" : "POP";

    /// <summary>
    /// Executes the POP instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (immediate values, addresses, offsets).</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    /// <exception cref="InvalidOperationException">Thrown when stack underflow is detected.</exception>
    public uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords)
    {
        // Get extension word if needed
        ushort extensionWord = ExtensionWordCount > 0 ? extensionWords[0] : (ushort)0;

        // Get current stack pointer
        ushort currentSP = registerFile.GetStackPointer();

        // Check for stack underflow - ensure SP is within valid memory range
        // For byte operations, only need to check one byte; for word operations, check two bytes
        int bytesToCheck = _isByteOperation ? 1 : 2;
        if (currentSP + bytesToCheck - 1 >= memory.Length)
        {
            throw new InvalidOperationException($"Stack underflow detected: Stack pointer 0x{currentSP:X4} accesses memory beyond bounds");
        }

        // Read value from current stack location
        // For byte operations, read as byte and sign-extend; for word operations, read as word
        ushort stackValue = ReadFromMemory(currentSP, _isByteOperation, memory);

        // Post-increment stack pointer by 2 (stack grows downward, always operates on words)
        ushort newSP = (ushort)(currentSP + 2);

        // Check for potential stack pointer overflow
        if (newSP < currentSP) // Overflow detection
        {
            throw new InvalidOperationException($"Stack pointer overflow detected: Stack pointer 0x{currentSP:X4} would overflow on POP operation");
        }

        // Update stack pointer (RegisterFile handles word-alignment automatically)
        registerFile.SetStackPointer(newSP);

        // Prepare value for destination based on operation type
        // For byte operations, apply sign extension for consistency with PUSH.B
        ushort destinationValue = _isByteOperation ? InstructionHelpers.SignExtendByte(stackValue) : stackValue;

        // Write value to destination operand
        InstructionHelpers.WriteOperand(
            _destinationRegister,
            _destinationAddressingMode,
            _isByteOperation,
            destinationValue,
            registerFile,
            memory,
            extensionWord);

        // Return cycle count based on addressing mode
        return InstructionHelpers.GetSingleOperandCycleCount(_destinationAddressingMode);
    }

    /// <summary>
    /// Returns a string representation of the instruction in assembly format.
    /// </summary>
    /// <returns>A string describing the instruction in assembly-like format.</returns>
    public override string ToString()
    {
        string suffix = IsByteOperation ? ".B" : "";
        return $"{Mnemonic.Split('.')[0]}{suffix} {InstructionHelpers.FormatOperand(_destinationRegister, _destinationAddressingMode)}";
    }

    /// <summary>
    /// Helper method to read from memory with proper error handling.
    /// </summary>
    /// <param name="address">The memory address to read from.</param>
    /// <param name="isByteOperation">True for byte operations, false for word operations.</param>
    /// <param name="memory">The system memory array.</param>
    /// <returns>The value read from memory.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when address is out of memory bounds.</exception>
    private static ushort ReadFromMemory(ushort address, bool isByteOperation, byte[] memory)
    {
        if (address >= memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(address), "Memory address out of range");
        }

        if (isByteOperation)
        {
            return memory[address];
        }
        else
        {
            if (address + 1 >= memory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Memory address out of range for word access");
            }

            // MSP430 is little-endian
            return (ushort)(memory[address] | (memory[address + 1] << 8));
        }
    }
}
