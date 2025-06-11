using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions;

/// <summary>
/// Comprehensive tests for instruction cycle counts based on SLAU445I Table 4-10.
/// 
/// Tests the specific requirements mentioned in the task:
/// - Rn→PC (3 cycles)
/// - @Rn→Rm (2 cycles) 
/// - @Rn+→Rm (2 cycles)
/// - #N→Rm (2 cycles)
/// </summary>
public class InstructionCycleTests
{
    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, RegisterName.R1, RegisterName.R0, false, 3u)] // Rn→PC
    [InlineData(AddressingMode.Indirect, AddressingMode.Register, RegisterName.R1, RegisterName.R2, false, 2u)] // @Rn→Rm
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Register, RegisterName.R1, RegisterName.R2, false, 2u)] // @Rn+→Rm
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, RegisterName.R1, RegisterName.R2, false, 2u)] // #N→Rm
    [InlineData(AddressingMode.Register, AddressingMode.Register, RegisterName.R1, RegisterName.R2, false, 1u)] // Rn→Rm
    public void GetCycleCount_SpecificRequirements_ReturnsCorrectCycles(
        AddressingMode sourceMode,
        AddressingMode destMode,
        RegisterName sourceReg,
        RegisterName destReg,
        bool isMovBitOrCmp,
        uint expectedCycles)
    {
        // Act
        uint actualCycles = InstructionCycleLookup.GetCycleCount(
            sourceMode, destMode, sourceReg, destReg, isMovBitOrCmp);

        // Assert
        Assert.Equal(expectedCycles, actualCycles);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Register, RegisterName.R1, RegisterName.R0, true, 3u)] // Rn→PC (MOV, no reduction for PC)
    [InlineData(AddressingMode.Indirect, AddressingMode.Register, RegisterName.R1, RegisterName.R2, true, 2u)] // @Rn→Rm (MOV, no reduction for register dest)
    [InlineData(AddressingMode.IndirectAutoIncrement, AddressingMode.Register, RegisterName.R1, RegisterName.R2, true, 2u)] // @Rn+→Rm (MOV, no reduction for register dest)
    [InlineData(AddressingMode.Immediate, AddressingMode.Register, RegisterName.R1, RegisterName.R2, true, 2u)] // #N→Rm (MOV, no reduction for register dest)
    [InlineData(AddressingMode.Register, AddressingMode.Indexed, RegisterName.R1, RegisterName.R2, true, 3u)] // Rn→x(Rm) (MOV, reduced from 4 to 3)
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute, RegisterName.R1, RegisterName.R2, true, 5u)] // &EDE→&TONI (MOV, reduced from 6 to 5)
    [InlineData(AddressingMode.Register, AddressingMode.Register, RegisterName.R1, RegisterName.R2, true, 1u)] // Rn→Rm (MOV)
    public void GetCycleCount_MovBitCmpInstructions_ReturnsCorrectCycles(
        AddressingMode sourceMode,
        AddressingMode destMode,
        RegisterName sourceReg,
        RegisterName destReg,
        bool isMovBitOrCmp,
        uint expectedCycles)
    {
        // Act
        uint actualCycles = InstructionCycleLookup.GetCycleCount(
            sourceMode, destMode, sourceReg, destReg, isMovBitOrCmp);

        // Assert
        Assert.Equal(expectedCycles, actualCycles);
    }

    [Theory]
    [InlineData(AddressingMode.Absolute, AddressingMode.Absolute, RegisterName.R1, RegisterName.R2, false, 6u)] // &EDE→&TONI
    [InlineData(AddressingMode.Symbolic, AddressingMode.Symbolic, RegisterName.R1, RegisterName.R2, false, 6u)] // EDE→TONI
    [InlineData(AddressingMode.Register, AddressingMode.Indexed, RegisterName.R1, RegisterName.R2, false, 4u)] // Rn→x(Rm)
    [InlineData(AddressingMode.Indexed, AddressingMode.Register, RegisterName.R1, RegisterName.R2, false, 3u)] // x(Rn)→Rm
    public void GetCycleCount_ComplexAddressingModes_ReturnsCorrectCycles(
        AddressingMode sourceMode,
        AddressingMode destMode,
        RegisterName sourceReg,
        RegisterName destReg,
        bool isMovBitOrCmp,
        uint expectedCycles)
    {
        // Act
        uint actualCycles = InstructionCycleLookup.GetCycleCount(
            sourceMode, destMode, sourceReg, destReg, isMovBitOrCmp);

        // Assert
        Assert.Equal(expectedCycles, actualCycles);
    }

    [Theory]
    [InlineData(AddressingMode.Register, AddressingMode.Indirect)]
    [InlineData(AddressingMode.Register, AddressingMode.IndirectAutoIncrement)]
    [InlineData(AddressingMode.Immediate, AddressingMode.Indirect)]
    public void GetCycleCount_InvalidDestinationModes_ThrowsException(
        AddressingMode sourceMode,
        AddressingMode destMode)
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            InstructionCycleLookup.GetCycleCount(
                sourceMode, destMode, RegisterName.R1, RegisterName.R2, false));
    }

    [Theory]
    [InlineData(RegisterName.R2, AddressingMode.Indirect, RegisterName.R1, 1u)] // Constant generator +4 → Rm
    [InlineData(RegisterName.R2, AddressingMode.IndirectAutoIncrement, RegisterName.R1, 1u)] // Constant generator +8 → Rm
    [InlineData(RegisterName.R3, AddressingMode.Register, RegisterName.R1, 1u)] // Constant generator +0 → Rm
    [InlineData(RegisterName.R3, AddressingMode.Immediate, RegisterName.R1, 1u)] // Constant generator +1/+2/-1 → Rm
    public void GetCycleCount_ConstantGenerators_ReturnsCorrectCycles(
        RegisterName sourceReg,
        AddressingMode sourceMode,
        RegisterName destReg,
        uint expectedCycles)
    {
        // Act
        uint actualCycles = InstructionCycleLookup.GetCycleCount(
            sourceMode, AddressingMode.Register, sourceReg, destReg, false);

        // Assert
        Assert.Equal(expectedCycles, actualCycles);
    }
}
