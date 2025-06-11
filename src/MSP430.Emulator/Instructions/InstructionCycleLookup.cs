using System;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions;

/// <summary>
/// Provides lookup table for instruction cycle counts based on SLAU445I Table 4-10.
/// 
/// Replaces the additive cycle calculation (base + source + destination) with 
/// specification-compliant lookup table for Format I (Double-Operand) instructions.
/// 
/// References:
/// - SLAU445I Section 4.5.1.5.4 Table 4-10 - Format I (Double-Operand) Instruction Cycles and Lengths
/// </summary>
public static class InstructionCycleLookup
{
    /// <summary>
    /// Gets the CPU cycle count for Format I instructions based on source and destination addressing modes.
    /// 
    /// Implementation follows SLAU445I Table 4-10 specifications exactly.
    /// Note: MOV, BIT, and CMP instructions execute in one fewer cycle for certain addressing mode combinations.
    /// </summary>
    /// <param name="sourceMode">Source operand addressing mode.</param>
    /// <param name="destinationMode">Destination operand addressing mode.</param>
    /// <param name="sourceRegister">Source register (used to check for PC destination special case).</param>
    /// <param name="destinationRegister">Destination register (used to check for PC destination special case).</param>
    /// <param name="isMovBitOrCmpInstruction">True for MOV, BIT, or CMP instructions that execute in one fewer cycle.</param>
    /// <returns>Number of CPU cycles required for the instruction.</returns>
    public static uint GetCycleCount(
        AddressingMode sourceMode,
        AddressingMode destinationMode,
        RegisterName sourceRegister,
        RegisterName destinationRegister,
        bool isMovBitOrCmpInstruction = false)
    {
        // Handle constant generator special cases for source
        bool isSourceConstantGenerator = InstructionHelpers.IsConstantGenerator(sourceRegister, sourceMode);

        // Map constant generators to their effective addressing modes for cycle lookup
        AddressingMode effectiveSourceMode = isSourceConstantGenerator
            ? MapConstantGeneratorToEffectiveMode(sourceRegister, sourceMode)
            : sourceMode;

        // Get base cycle count from lookup table
        uint cycles = GetBaseCycleCount(effectiveSourceMode, destinationMode, destinationRegister);

        // Apply MOV/BIT/CMP reduction if applicable
        if (isMovBitOrCmpInstruction && ShouldReduceCycleForMovBitCmp(effectiveSourceMode, destinationMode))
        {
            cycles = Math.Max(1u, cycles - 1u); // Ensure we don't go below 1 cycle
        }

        return cycles;
    }

    /// <summary>
    /// Maps constant generator combinations to their effective addressing modes for cycle lookup.
    /// 
    /// All constant generators should behave like register operations (0 additional cycles)
    /// since they provide immediate constants without memory access.
    /// </summary>
    /// <param name="register">The register used in constant generation.</param>
    /// <param name="mode">The addressing mode used in constant generation.</param>
    /// <returns>The effective addressing mode for cycle calculation purposes.</returns>
    private static AddressingMode MapConstantGeneratorToEffectiveMode(RegisterName register, AddressingMode mode)
    {
        // All constant generators should map to register mode for cycle purposes
        // since they provide immediate constants without additional memory access cycles
        return AddressingMode.Register;
    }

    /// <summary>
    /// Gets the base cycle count from SLAU445I Table 4-10.
    /// </summary>
    /// <param name="sourceMode">Effective source addressing mode.</param>
    /// <param name="destinationMode">Destination addressing mode.</param>
    /// <param name="destinationRegister">Destination register (for PC special case).</param>
    /// <returns>Base cycle count from Table 4-10.</returns>
    private static uint GetBaseCycleCount(AddressingMode sourceMode, AddressingMode destinationMode, RegisterName destinationRegister)
    {
        // Special case: PC as destination
        bool isDestinationPC = destinationRegister == RegisterName.R0 && destinationMode == AddressingMode.Register;

        // Handle non-standard destination modes for backward compatibility
        // Note: Indirect and IndirectAutoIncrement are not valid destination modes per SLAU445I Table 4-10
        if (destinationMode == AddressingMode.Indirect || destinationMode == AddressingMode.IndirectAutoIncrement)
        {
            // Return estimated cycle counts for non-standard combinations to maintain compatibility
            // These values are based on the old additive calculation approach
            return GetLegacyCycleCount(sourceMode, destinationMode);
        }

        return sourceMode switch
        {
            // Rn source mode
            AddressingMode.Register when destinationMode == AddressingMode.Register && !isDestinationPC => 1,
            AddressingMode.Register when isDestinationPC => 3,
            AddressingMode.Register when destinationMode == AddressingMode.Indexed => 4,
            AddressingMode.Register when destinationMode == AddressingMode.Symbolic => 4,
            AddressingMode.Register when destinationMode == AddressingMode.Absolute => 4,

            // @Rn source mode  
            AddressingMode.Indirect when destinationMode == AddressingMode.Register && !isDestinationPC => 2,
            AddressingMode.Indirect when isDestinationPC => 4,
            AddressingMode.Indirect when destinationMode == AddressingMode.Indexed => 5,
            AddressingMode.Indirect when destinationMode == AddressingMode.Symbolic => 5,
            AddressingMode.Indirect when destinationMode == AddressingMode.Absolute => 5,

            // @Rn+ source mode
            AddressingMode.IndirectAutoIncrement when destinationMode == AddressingMode.Register && !isDestinationPC => 2,
            AddressingMode.IndirectAutoIncrement when isDestinationPC => 4,
            AddressingMode.IndirectAutoIncrement when destinationMode == AddressingMode.Indexed => 5,
            AddressingMode.IndirectAutoIncrement when destinationMode == AddressingMode.Symbolic => 5,
            AddressingMode.IndirectAutoIncrement when destinationMode == AddressingMode.Absolute => 5,

            // #N source mode (immediate)
            AddressingMode.Immediate when destinationMode == AddressingMode.Register && !isDestinationPC => 2,
            AddressingMode.Immediate when isDestinationPC => 3,
            AddressingMode.Immediate when destinationMode == AddressingMode.Indexed => 5,
            AddressingMode.Immediate when destinationMode == AddressingMode.Symbolic => 5,
            AddressingMode.Immediate when destinationMode == AddressingMode.Absolute => 5,

            // x(Rn) source mode (indexed)
            AddressingMode.Indexed when destinationMode == AddressingMode.Register && !isDestinationPC => 3,
            AddressingMode.Indexed when isDestinationPC => 5,
            AddressingMode.Indexed when destinationMode == AddressingMode.Indexed => 6,
            AddressingMode.Indexed when destinationMode == AddressingMode.Symbolic => 6,
            AddressingMode.Indexed when destinationMode == AddressingMode.Absolute => 6,

            // EDE source mode (symbolic)
            AddressingMode.Symbolic when destinationMode == AddressingMode.Register && !isDestinationPC => 3,
            AddressingMode.Symbolic when isDestinationPC => 5,
            AddressingMode.Symbolic when destinationMode == AddressingMode.Indexed => 6,
            AddressingMode.Symbolic when destinationMode == AddressingMode.Symbolic => 6,
            AddressingMode.Symbolic when destinationMode == AddressingMode.Absolute => 6,

            // &EDE source mode (absolute)
            AddressingMode.Absolute when destinationMode == AddressingMode.Register && !isDestinationPC => 3,
            AddressingMode.Absolute when isDestinationPC => 5,
            AddressingMode.Absolute when destinationMode == AddressingMode.Indexed => 6,
            AddressingMode.Absolute when destinationMode == AddressingMode.Symbolic => 6,
            AddressingMode.Absolute when destinationMode == AddressingMode.Absolute => 6,

            _ => throw new InvalidOperationException($"Unsupported addressing mode combination: {sourceMode} -> {destinationMode}")
        };
    }

    /// <summary>
    /// Determines if MOV, BIT, or CMP instructions should execute in one fewer cycle.
    /// Based on SLAU445I Table 4-10 footnote [1].
    /// 
    /// The reduction applies only to specific destination addressing modes:
    /// - x(Rm) (Indexed)
    /// - EDE (Symbolic) 
    /// - &EDE (Absolute)
    /// 
    /// It does NOT apply to register destinations.
    /// </summary>
    /// <param name="sourceMode">Source addressing mode.</param>
    /// <param name="destinationMode">Destination addressing mode.</param>
    /// <returns>True if cycle count should be reduced by one.</returns>
    private static bool ShouldReduceCycleForMovBitCmp(AddressingMode sourceMode, AddressingMode destinationMode)
    {
        // MOV/BIT/CMP reduction only applies to non-register destination modes
        return destinationMode == AddressingMode.Indexed ||
               destinationMode == AddressingMode.Symbolic ||
               destinationMode == AddressingMode.Absolute;
    }

    /// <summary>
    /// Gets cycle count for non-standard addressing mode combinations using legacy additive approach.
    /// These combinations are not valid per SLAU445I but are maintained for backward compatibility.
    /// </summary>
    /// <param name="sourceMode">Source addressing mode.</param>
    /// <param name="destinationMode">Destination addressing mode.</param>
    /// <returns>Estimated cycle count based on old additive calculation.</returns>
    private static uint GetLegacyCycleCount(AddressingMode sourceMode, AddressingMode destinationMode)
    {
        // Use the old additive calculation: base + source + destination
        uint baseCycles = 1;

        uint sourceCycles = sourceMode switch
        {
            AddressingMode.Register => 0u,
            AddressingMode.Indexed => 3u,
            AddressingMode.Indirect => 2u,
            AddressingMode.IndirectAutoIncrement => 2u,
            AddressingMode.Immediate => 0u,
            AddressingMode.Absolute => 3u,
            AddressingMode.Symbolic => 3u,
            _ => 0u
        };

        uint destinationCycles = destinationMode switch
        {
            AddressingMode.Register => 0u,
            AddressingMode.Indexed => 3u,
            AddressingMode.Indirect => 2u,  // Non-standard but preserved for compatibility
            AddressingMode.IndirectAutoIncrement => 2u,  // Non-standard but preserved for compatibility
            AddressingMode.Absolute => 3u,
            AddressingMode.Symbolic => 3u,
            _ => 0u
        };

        return baseCycles + sourceCycles + destinationCycles;
    }
}
