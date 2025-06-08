using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions;

/// <summary>
/// Decodes addressing modes from MSP430 instruction words.
/// 
/// The MSP430 uses a combination of register number and As/Ad bits to determine
/// the addressing mode. Special combinations of register and As/Ad bits create
/// additional addressing modes beyond the basic four.
/// 
/// Implementation based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019,
/// Section 4.4: "Addressing Modes" and Section 4.3.4: "Constant Generator Registers (CG1 and CG2)" - Table 4-2.
/// See docs/references/SLAU445/4.4_addressing_modes.md and 
/// docs/references/SLAU445/4.3.4_constant_generator_registers_(cg1_and_cg2).md for detailed specifications.
/// </summary>
public static class AddressingModeDecoder
{
    /// <summary>
    /// Decodes the source addressing mode from instruction bits.
    /// </summary>
    /// <param name="sourceRegister">The source register (bits 11:8).</param>
    /// <param name="asBits">The As bits (bits 5:4) specifying source addressing mode.</param>
    /// <returns>The decoded source addressing mode.</returns>
    public static AddressingMode DecodeSourceAddressingMode(RegisterName sourceRegister, byte asBits)
    {
        // Validate As bits (should be 0-3)
        if (asBits > 3)
        {
            return AddressingMode.Invalid;
        }

        // Handle special cases for specific registers
        return sourceRegister switch
        {
            RegisterName.R0 => asBits switch
            {
                0 => AddressingMode.Register,           // R0
                1 => AddressingMode.Symbolic,           // ADDR (PC relative)
                2 => AddressingMode.Indirect,           // @R0
                3 => AddressingMode.Immediate,          // #N
                _ => AddressingMode.Invalid
            },
            RegisterName.R2 => asBits switch
            {
                0 => AddressingMode.Register,           // R2 (SR)
                1 => AddressingMode.Absolute,           // &ADDR
                2 => AddressingMode.Indirect,           // @R2
                3 => AddressingMode.IndirectAutoIncrement, // @R2+
                _ => AddressingMode.Invalid
            },
            RegisterName.R3 => asBits switch
            {
                0 => AddressingMode.Register,           // R3 (Constant 0)
                1 => AddressingMode.Immediate,          // Constant +1
                2 => AddressingMode.Immediate,          // Constant +2
                3 => AddressingMode.Immediate,          // Constant -1
                _ => AddressingMode.Invalid
            },
            _ => asBits switch
            {
                0 => AddressingMode.Register,           // Rn
                1 => AddressingMode.Indexed,            // X(Rn)
                2 => AddressingMode.Indirect,           // @Rn
                3 => AddressingMode.IndirectAutoIncrement, // @Rn+
                _ => AddressingMode.Invalid
            }
        };
    }

    /// <summary>
    /// Decodes the destination addressing mode from instruction bits.
    /// </summary>
    /// <param name="destinationRegister">The destination register (bits 3:0).</param>
    /// <param name="adBit">The Ad bit (bit 7) specifying destination addressing mode.</param>
    /// <returns>The decoded destination addressing mode.</returns>
    public static AddressingMode DecodeDestinationAddressingMode(RegisterName destinationRegister, bool adBit)
    {
        // Destination addressing modes are more limited (only Ad = 0 or 1)
        return destinationRegister switch
        {
            RegisterName.R0 => adBit switch
            {
                false => AddressingMode.Register,       // R0 (PC)
                true => AddressingMode.Symbolic,        // ADDR (PC relative)
            },
            RegisterName.R2 => adBit switch
            {
                false => AddressingMode.Register,       // R2 (SR)
                true => AddressingMode.Absolute,        // &ADDR
            },
            _ => adBit switch
            {
                false => AddressingMode.Register,       // Rn
                true => AddressingMode.Indexed,         // X(Rn)
            }
        };
    }

    /// <summary>
    /// Determines if the addressing mode requires an extension word.
    /// </summary>
    /// <param name="mode">The addressing mode to check.</param>
    /// <returns>True if an extension word is required, false otherwise.</returns>
    public static bool RequiresExtensionWord(AddressingMode mode)
    {
        return mode switch
        {
            AddressingMode.Indexed => true,
            AddressingMode.Immediate => true,
            AddressingMode.Absolute => true,
            AddressingMode.Symbolic => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the constant value for R3 constant generator modes.
    /// </summary>
    /// <param name="asBits">The As bits for R3.</param>
    /// <returns>The constant value, or null if not a constant generator mode.</returns>
    public static ushort? GetConstantGeneratorValue(byte asBits)
    {
        return asBits switch
        {
            0 => 0,      // R3 as register mode gives constant 0
            1 => 1,      // R3 indexed mode gives constant +1
            2 => 2,      // R3 indirect mode gives constant +2
            3 => 0xFFFF, // R3 indirect auto-increment gives constant -1
            _ => null
        };
    }

    /// <summary>
    /// Gets the constant value for R2 constant generator modes.
    /// Per SLAU445I Table 4-2, R2 can generate constants +4 and +8.
    /// </summary>
    /// <param name="asBits">The As bits for R2.</param>
    /// <returns>The constant value, or null if not a constant generator mode.</returns>
    public static ushort? GetR2ConstantGeneratorValue(byte asBits)
    {
        return asBits switch
        {
            2 => 4,      // R2 indirect mode gives constant +4
            3 => 8,      // R2 indirect auto-increment gives constant +8
            _ => null    // As=00 is register mode, As=01 is absolute address mode
        };
    }

    /// <summary>
    /// Validates that a register number is within the valid range (0-15).
    /// </summary>
    /// <param name="registerBits">The register bits to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValidRegister(byte registerBits)
    {
        return registerBits <= 15;
    }
}
