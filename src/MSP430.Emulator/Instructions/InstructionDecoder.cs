using System;
using System.Collections.Generic;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions.Arithmetic;
using MSP430.Emulator.Instructions.DataMovement;
using MSP430.Emulator.Instructions.Logic;

namespace MSP430.Emulator.Instructions;

/// <summary>
/// Implements instruction decoding for MSP430 machine code.
/// 
/// The decoder analyzes 16-bit instruction words and converts them into
/// structured instruction objects based on the MSP430 instruction formats.
/// </summary>
public class InstructionDecoder : IInstructionDecoder
{
    /// <summary>
    /// Delegate for creating Format I instructions.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="sourceReg">The source register.</param>
    /// <param name="destReg">The destination register.</param>
    /// <param name="sourceMode">The source addressing mode.</param>
    /// <param name="destMode">The destination addressing mode.</param>
    /// <param name="byteWord">True for byte operations, false for word operations.</param>
    /// <returns>The created instruction.</returns>
    private delegate Instruction FormatIInstructionFactory(
        ushort instructionWord,
        RegisterName sourceReg,
        RegisterName destReg,
        AddressingMode sourceMode,
        AddressingMode destMode,
        bool byteWord);

    /// <summary>
    /// Lookup table for Format I instruction creation based on opcode.
    /// </summary>
    private static readonly Dictionary<ushort, FormatIInstructionFactory> FormatIInstructionFactories = new()
    {
        [0x4] = (word, src, dst, srcMode, dstMode, bw) => new MovInstruction(word, src, dst, srcMode, dstMode, bw),
        [0x5] = (word, src, dst, srcMode, dstMode, bw) => new AddInstruction(word, src, dst, srcMode, dstMode, bw),
        [0x6] = (word, src, dst, srcMode, dstMode, bw) => new AddcInstruction(word, src, dst, srcMode, dstMode, bw),
        [0x7] = (word, src, dst, srcMode, dstMode, bw) => new SubcInstruction(word, src, dst, srcMode, dstMode, bw),
        [0x8] = (word, src, dst, srcMode, dstMode, bw) => new SubInstruction(word, src, dst, srcMode, dstMode, bw),
        [0x9] = (word, src, dst, srcMode, dstMode, bw) => new CmpInstruction(word, src, dst, srcMode, dstMode, bw),
        [0xA] = (word, src, dst, srcMode, dstMode, bw) => new DaddInstruction(word, src, dst, srcMode, dstMode, bw),
        [0xB] = (word, src, dst, srcMode, dstMode, bw) => new BitInstruction(word, src, dst, srcMode, dstMode, bw),
        [0xC] = (word, src, dst, srcMode, dstMode, bw) => new BicInstruction(word, src, dst, srcMode, dstMode, bw),
        [0xD] = (word, src, dst, srcMode, dstMode, bw) => new BisInstruction(word, src, dst, srcMode, dstMode, bw),
        [0xE] = (word, src, dst, srcMode, dstMode, bw) => new XorInstruction(word, src, dst, srcMode, dstMode, bw),
        [0xF] = (word, src, dst, srcMode, dstMode, bw) => new AndInstruction(word, src, dst, srcMode, dstMode, bw)
    };
    /// <summary>
    /// Decodes a 16-bit instruction word into an instruction object.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word to decode.</param>
    /// <returns>The decoded instruction object.</returns>
    /// <exception cref="InvalidInstructionException">Thrown when the instruction word is invalid or unsupported.</exception>
    public Instruction Decode(ushort instructionWord)
    {
        if (!IsValidInstruction(instructionWord))
        {
            throw new InvalidInstructionException(instructionWord,
                $"Invalid instruction word: 0x{instructionWord:X4}");
        }

        InstructionFormat format = GetInstructionFormat(instructionWord);
        ushort opcode = ExtractOpcode(instructionWord, format);

        return format switch
        {
            InstructionFormat.FormatI => DecodeFormatI(instructionWord, opcode),
            InstructionFormat.FormatII => DecodeFormatII(instructionWord, opcode),
            InstructionFormat.FormatIII => DecodeFormatIII(instructionWord, opcode),
            _ => throw new InvalidInstructionException(instructionWord,
                $"Unknown instruction format for word: 0x{instructionWord:X4}")
        };
    }

    /// <summary>
    /// Determines the instruction format of a 16-bit instruction word.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word to analyze.</param>
    /// <returns>The instruction format (I, II, or III).</returns>
    public InstructionFormat GetInstructionFormat(ushort instructionWord)
    {
        // Format III: Jump instructions start with 001 (bits 15:13)
        if ((instructionWord & 0xE000) == 0x2000)
        {
            return InstructionFormat.FormatIII;
        }

        // Format II: Single-operand instructions have specific opcodes
        // Bits 15:8 = 0x10-0x13 for various single-operand instructions
        if ((instructionWord & 0xFF00) >= 0x1000 && (instructionWord & 0xFF00) <= 0x1300)
        {
            return InstructionFormat.FormatII;
        }

        // Default to Format I (two-operand instructions)
        return InstructionFormat.FormatI;
    }

    /// <summary>
    /// Validates whether a 16-bit instruction word represents a valid MSP430 instruction.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word to validate.</param>
    /// <returns>True if the instruction is valid, false otherwise.</returns>
    public bool IsValidInstruction(ushort instructionWord)
    {
        InstructionFormat format = GetInstructionFormat(instructionWord);

        return format switch
        {
            InstructionFormat.FormatI => IsValidFormatI(instructionWord),
            InstructionFormat.FormatII => IsValidFormatII(instructionWord),
            InstructionFormat.FormatIII => IsValidFormatIII(instructionWord),
            _ => false
        };
    }

    /// <summary>
    /// Extracts the opcode from a 16-bit instruction word based on its format.
    /// </summary>
    /// <param name="instructionWord">The 16-bit instruction word.</param>
    /// <param name="format">The instruction format.</param>
    /// <returns>The extracted opcode.</returns>
    public ushort ExtractOpcode(ushort instructionWord, InstructionFormat format)
    {
        return format switch
        {
            InstructionFormat.FormatI => (ushort)((instructionWord & 0xF000) >> 12),     // Bits 15:12
            InstructionFormat.FormatII => (ushort)((instructionWord & 0xFF00) >> 8),    // Bits 15:8
            InstructionFormat.FormatIII => (ushort)((instructionWord & 0x1C00) >> 10),  // Bits 12:10
            _ => 0
        };
    }

    /// <summary>
    /// Decodes a Format I (two-operand) instruction.
    /// </summary>
    private Instruction DecodeFormatI(ushort instructionWord, ushort opcode)
    {
        // Extract fields from Format I instruction
        var sourceReg = (RegisterName)((instructionWord & 0x0F00) >> 8);    // Bits 11:8
        bool adBit = (instructionWord & 0x0080) != 0;                        // Bit 7
        bool byteWord = (instructionWord & 0x0040) != 0;                     // Bit 6
        byte asBits = (byte)((instructionWord & 0x0030) >> 4);               // Bits 5:4
        var destReg = (RegisterName)(instructionWord & 0x000F);             // Bits 3:0

        // Validate register numbers
        if (!AddressingModeDecoder.IsValidRegister((byte)sourceReg) ||
            !AddressingModeDecoder.IsValidRegister((byte)destReg))
        {
            throw new InvalidInstructionException(instructionWord, "Invalid register number in Format I instruction");
        }

        // Decode addressing modes
        AddressingMode sourceMode = AddressingModeDecoder.DecodeSourceAddressingMode(sourceReg, asBits);
        AddressingMode destMode = AddressingModeDecoder.DecodeDestinationAddressingMode(destReg, adBit);

        if (sourceMode == AddressingMode.Invalid || destMode == AddressingMode.Invalid)
        {
            throw new InvalidInstructionException(instructionWord, "Invalid addressing mode in Format I instruction");
        }

        // Create specific instruction type based on opcode using lookup table
        if (FormatIInstructionFactories.TryGetValue(opcode, out FormatIInstructionFactory? factory))
        {
            return factory(instructionWord, sourceReg, destReg, sourceMode, destMode, byteWord);
        }

        throw new InvalidInstructionException(instructionWord,
            $"Unsupported Format I instruction opcode: 0x{opcode:X}");
    }

    /// <summary>
    /// Decodes a Format II (single-operand) instruction.
    /// </summary>
    private Instruction DecodeFormatII(ushort instructionWord, ushort opcode)
    {
        // Extract fields from Format II instruction
        bool byteWord = (instructionWord & 0x0040) != 0;                     // Bit 6
        byte asBits = (byte)((instructionWord & 0x0030) >> 4);               // Bits 5:4
        var sourceReg = (RegisterName)(instructionWord & 0x000F);           // Bits 3:0

        // Validate register number
        if (!AddressingModeDecoder.IsValidRegister((byte)sourceReg))
        {
            throw new InvalidInstructionException(instructionWord, "Invalid register number in Format II instruction");
        }

        // Decode addressing mode
        AddressingMode sourceMode = AddressingModeDecoder.DecodeSourceAddressingMode(sourceReg, asBits);

        if (sourceMode == AddressingMode.Invalid)
        {
            throw new InvalidInstructionException(instructionWord, "Invalid addressing mode in Format II instruction");
        }

        // Create specific instruction type based on opcode
        // Note: This implementation uses separate opcodes for SWPB (0x10) and SXT (0x11)
        // rather than the standard MSP430 sub-opcode approach within 0x10
        return opcode switch
        {
            0x10 when (instructionWord & 0x00C0) == 0x0080 => new SwpbInstruction(instructionWord, sourceReg, sourceMode), // SWPB: bits 7:6 = 10
            0x11 when (instructionWord & 0x00C0) == 0x0080 => new SxtInstruction(instructionWord, sourceReg, sourceMode),  // SXT: bits 7:6 = 10  
            _ => new FormatIIInstruction(opcode, instructionWord, sourceReg, sourceMode, byteWord,
                AddressingModeDecoder.RequiresExtensionWord(sourceMode) ? 1 : 0)
        };
    }

    /// <summary>
    /// Decodes a Format III (jump) instruction.
    /// </summary>
    private FormatIIIInstruction DecodeFormatIII(ushort instructionWord, ushort opcode)
    {
        // Extract the 10-bit signed offset (bits 9:0)
        short offset = (short)(instructionWord & 0x03FF);

        // Sign extend from 10 bits to 16 bits
        if ((offset & 0x0200) != 0)
        {
            offset |= unchecked((short)0xFC00);
        }

        return new FormatIIIInstruction(opcode, instructionWord, offset);
    }

    /// <summary>
    /// Validates a Format I instruction.
    /// </summary>
    private static bool IsValidFormatI(ushort instructionWord)
    {
        int opcode = (instructionWord & 0xF000) >> 12;

        // Format I opcodes: 4-15 (0x4-0xF)
        // Opcode 0-3 are reserved for Format II and Format III
        return opcode >= 4 && opcode <= 15;
    }

    /// <summary>
    /// Validates a Format II instruction.
    /// </summary>
    private static bool IsValidFormatII(ushort instructionWord)
    {
        // Format II instructions have opcodes 0x10-0x13 (bits 15:8)
        int opcode = (instructionWord & 0xFF00) >> 8;

        // Valid Format II opcodes: 0x10-0x13 (covering RRC, SWPB, RRA, SXT, PUSH, CALL, RETI)
        return opcode >= 0x10 && opcode <= 0x13;
    }

    /// <summary>
    /// Validates a Format III instruction.
    /// </summary>
    private static bool IsValidFormatIII(ushort instructionWord)
    {
        // Format III instructions start with 001 (bits 15:13)
        int prefix = (instructionWord & 0xE000) >> 13;
        if (prefix != 1)
        {
            return false;
        }

        // Check condition codes (bits 12:10): 0-7 are valid
        int condition = (instructionWord & 0x1C00) >> 10;
        return condition <= 7;
    }
}

/// <summary>
/// Placeholder implementation for Format I instructions.
/// This will be expanded in future tasks with specific instruction implementations.
/// </summary>
internal class FormatIInstruction : Instruction
{
    private readonly RegisterName _sourceRegister;
    private readonly RegisterName _destinationRegister;
    private readonly AddressingMode _sourceAddressingMode;
    private readonly AddressingMode _destinationAddressingMode;
    private readonly bool _isByteOperation;
    private readonly int _extensionWordCount;

    public FormatIInstruction(ushort opcode, ushort instructionWord, RegisterName sourceReg,
        RegisterName destReg, AddressingMode sourceMode, AddressingMode destMode, bool isByteOperation, int extensionWordCount)
        : base(InstructionFormat.FormatI, opcode, instructionWord)
    {
        _sourceRegister = sourceReg;
        _destinationRegister = destReg;
        _sourceAddressingMode = sourceMode;
        _destinationAddressingMode = destMode;
        _isByteOperation = isByteOperation;
        _extensionWordCount = extensionWordCount;
    }

    public override RegisterName? SourceRegister => _sourceRegister;
    public override RegisterName? DestinationRegister => _destinationRegister;
    public override AddressingMode? SourceAddressingMode => _sourceAddressingMode;
    public override AddressingMode? DestinationAddressingMode => _destinationAddressingMode;
    public override bool IsByteOperation => _isByteOperation;
    public override int ExtensionWordCount => _extensionWordCount;

    public override string Mnemonic => $"FORMAT_I_{Opcode:X}";

    public override string ToString()
    {
        string suffix = IsByteOperation ? ".B" : "";
        return $"{Mnemonic}{suffix} {SourceAddressingMode}, {DestinationAddressingMode}";
    }
}

/// <summary>
/// Placeholder implementation for Format II instructions.
/// This will be expanded in future tasks with specific instruction implementations.
/// </summary>
internal class FormatIIInstruction : Instruction
{
    private readonly RegisterName _sourceRegister;
    private readonly AddressingMode _sourceAddressingMode;
    private readonly bool _isByteOperation;
    private readonly int _extensionWordCount;

    public FormatIIInstruction(ushort opcode, ushort instructionWord, RegisterName sourceReg,
        AddressingMode sourceMode, bool isByteOperation, int extensionWordCount)
        : base(InstructionFormat.FormatII, opcode, instructionWord)
    {
        _sourceRegister = sourceReg;
        _sourceAddressingMode = sourceMode;
        _isByteOperation = isByteOperation;
        _extensionWordCount = extensionWordCount;
    }

    public override RegisterName? SourceRegister => _sourceRegister;
    public override AddressingMode? SourceAddressingMode => _sourceAddressingMode;
    public override bool IsByteOperation => _isByteOperation;
    public override int ExtensionWordCount => _extensionWordCount;

    public override string Mnemonic => $"FORMAT_II_{Opcode:X}";

    public override string ToString()
    {
        string suffix = IsByteOperation ? ".B" : "";
        return $"{Mnemonic}{suffix} {SourceAddressingMode}";
    }
}


