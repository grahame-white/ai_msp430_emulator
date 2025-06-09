using System;
using System.Collections.Generic;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Tests.Cpu;

/// <summary>
/// Comprehensive tests for MSP430 CPU register behaviors based on MSP430FR2355 specifications.
/// 
/// MSP430FR2355 CPU Register Specifications:
/// - 16 registers total: R0-R15, each 16-bit
/// - R0 (PC): Program Counter - automatically word-aligned (even addresses only)
/// - R1 (SP): Stack Pointer - should be word-aligned for proper operation
/// - R2 (SR/CG1): Status Register and Constant Generator #1
/// - R3 (CG2): Constant Generator #2
/// - R4-R15: General-purpose registers
/// 
/// References:
/// - docs/references/SLAU445/4.3_cpu_registers.md - CPU Registers overview
/// - docs/references/SLAU445/4.3.3_status_register_(sr).md - Status Register (SR) specification
/// - docs/references/SLAU445/4.3.4_constant_generator_registers_(cg1_and_cg2).md - Constant Generator specification
/// - docs/references/SLAU445/4.3.5_general_purpose_registers_(r4_to_r15).md - General-purpose registers specification
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.3: CPU Registers
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.3.1: Program Counter (PC)
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.3.2: Stack Pointer (SP)
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.3.3: Status Register (SR)
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.3.4: Constant Generator Registers
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.3.5: General-Purpose Registers
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D) - Section 6.1
/// </summary>
public class CpuRegisterBehaviorTests
{
    [Theory]
    [InlineData(RegisterName.R0, RegisterName.PC)]    // Program Counter aliases
    [InlineData(RegisterName.R1, RegisterName.SP)]    // Stack Pointer aliases
    [InlineData(RegisterName.R2, RegisterName.SR)]    // Status Register aliases
    [InlineData(RegisterName.R3, RegisterName.CG2)]   // Constant Generator #2 aliases
    public void RegisterAliases_SpecialRegisters_HaveCorrectValues(RegisterName register, RegisterName alias)
    {
        // Test validates that register aliases map to correct numeric values
        // SLAU445I Section 4.3: CPU register definitions and aliases
        Assert.Equal((byte)register, (byte)alias);
    }

    [Fact]
    public void RegisterAliases_R2_AlsoMapsToCG1()
    {
        // Test validates that R2 also serves as CG1 (Constant Generator #1)
        // SLAU445I Section 4.3.4: R2 serves dual purpose as SR and CG1
        Assert.Equal((byte)RegisterName.R2, (byte)RegisterName.CG1);
    }

    [Theory]
    [InlineData(0x1000)]   // Even address - should remain unchanged
    [InlineData(0x2000)]   // Even address - should remain unchanged  
    [InlineData(0x4000)]   // Even address - should remain unchanged
    [InlineData(0xFFFE)]   // Even address - should remain unchanged
    public void ProgramCounter_EvenAddresses_RemainUnchanged(ushort address)
    {
        // Test validates that even addresses in PC are preserved
        // SLAU445I Section 4.3.1: PC automatically word-aligned (even addresses)
        var registerFile = new RegisterFile();

        registerFile.WriteRegister(RegisterName.PC, address);
        ushort result = registerFile.ReadRegister(RegisterName.PC);

        Assert.Equal(address, result);
        Assert.Equal(0, result % 2); // Verify it's even
    }

    [Theory]
    [InlineData(0x1001, 0x1000)]   // Odd address aligned down
    [InlineData(0x2001, 0x2000)]   // Odd address aligned down
    [InlineData(0x4001, 0x4000)]   // Odd address aligned down
    [InlineData(0xFFFF, 0xFFFE)]   // Odd address aligned down
    public void ProgramCounter_OddAddresses_AreWordAligned(ushort oddAddress, ushort expectedEvenAddress)
    {
        // Test validates that odd addresses in PC are automatically aligned to even addresses
        // SLAU445I Section 4.3.1: PC is automatically word-aligned to even addresses
        var registerFile = new RegisterFile();

        registerFile.WriteRegister(RegisterName.PC, oddAddress);
        ushort result = registerFile.ReadRegister(RegisterName.PC);

        Assert.Equal(expectedEvenAddress, result);
        Assert.Equal(0, result % 2); // Verify it's even
    }

    [Theory]
    [InlineData(0x2000)]   // Stack in SRAM region
    [InlineData(0x2FFE)]   // Near end of SRAM, word-aligned
    [InlineData(0x1000)]   // Different valid memory region
    public void StackPointer_EvenAddresses_RemainUnchanged(ushort address)
    {
        // Test validates that even addresses in SP are preserved
        // SLAU445I Section 4.3.2: SP should be word-aligned for proper stack operation
        var registerFile = new RegisterFile();

        registerFile.WriteRegister(RegisterName.SP, address);
        ushort result = registerFile.ReadRegister(RegisterName.SP);

        Assert.Equal(address, result);
        Assert.Equal(0, result % 2); // Verify it's even
    }

    [Theory]
    [InlineData(0x2001, 0x2000)]   // SRAM region - align down
    [InlineData(0x2FFF, 0x2FFE)]   // End of SRAM - align down
    [InlineData(0x1001, 0x1000)]   // Other region - align down
    public void StackPointer_OddAddresses_AreWordAligned(ushort oddAddress, ushort expectedEvenAddress)
    {
        // Test validates that odd addresses in SP are word-aligned
        // SLAU445I Section 4.3.2: SP should be word-aligned for proper stack operation
        var registerFile = new RegisterFile();

        registerFile.WriteRegister(RegisterName.SP, oddAddress);
        ushort result = registerFile.ReadRegister(RegisterName.SP);

        Assert.Equal(expectedEvenAddress, result);
        Assert.Equal(0, result % 2); // Verify it's even
    }

    [Fact]
    public void StatusRegister_Integration_StatusRegisterObjectReflectsR2Value()
    {
        // Test validates that SR register value is synchronized with StatusRegister object
        // SLAU445I Section 4.3.3: R2 serves as Status Register
        var registerFile = new RegisterFile();

        // Set flags in StatusRegister object
        registerFile.StatusRegister.Carry = true;
        registerFile.StatusRegister.Zero = true;

        // Read R2 register value
        ushort r2Value = registerFile.ReadRegister(RegisterName.R2);

        // Verify flags are reflected in R2 value
        Assert.Equal((ushort)0x0003, r2Value); // Carry (bit 0) + Zero (bit 1)
    }

    [Fact]
    public void StatusRegister_Integration_R2ValueReflectsStatusRegisterObject()
    {
        // Test validates that R2 register value affects StatusRegister object
        // SLAU445I Section 4.3.3: R2 serves as Status Register
        var registerFile = new RegisterFile();

        // Write value to R2 register
        registerFile.WriteRegister(RegisterName.R2, 0x000F); // Set multiple flags

        // Verify flags are reflected in StatusRegister object
        Assert.True(registerFile.StatusRegister.Carry);
        Assert.True(registerFile.StatusRegister.Zero);
        Assert.True(registerFile.StatusRegister.Negative);
        Assert.True(registerFile.StatusRegister.GeneralInterruptEnable);
    }

    [Theory]
    [InlineData(RegisterName.R4, 0x1234)]
    [InlineData(RegisterName.R5, 0x5678)]
    [InlineData(RegisterName.R6, 0x9ABC)]
    [InlineData(RegisterName.R7, 0xDEF0)]
    [InlineData(RegisterName.R8, 0x1111)]
    [InlineData(RegisterName.R9, 0x2222)]
    [InlineData(RegisterName.R10, 0x3333)]
    [InlineData(RegisterName.R11, 0x4444)]
    [InlineData(RegisterName.R12, 0x5555)]
    [InlineData(RegisterName.R13, 0x6666)]
    [InlineData(RegisterName.R14, 0x7777)]
    [InlineData(RegisterName.R15, 0x8888)]
    public void GeneralPurposeRegisters_ReadWrite_PreserveValues(RegisterName register, ushort value)
    {
        // Test validates that general-purpose registers store and retrieve values correctly
        // SLAU445I Section 4.3.5: R4-R15 are general-purpose registers
        var registerFile = new RegisterFile();

        registerFile.WriteRegister(register, value);
        ushort result = registerFile.ReadRegister(register);

        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R5)]
    [InlineData(RegisterName.R6)]
    [InlineData(RegisterName.R7)]
    [InlineData(RegisterName.R8)]
    [InlineData(RegisterName.R9)]
    [InlineData(RegisterName.R10)]
    [InlineData(RegisterName.R11)]
    [InlineData(RegisterName.R12)]
    [InlineData(RegisterName.R13)]
    [InlineData(RegisterName.R14)]
    [InlineData(RegisterName.R15)]
    public void GeneralPurposeRegisters_AfterReset_AreZero(RegisterName register)
    {
        // Test validates that general-purpose registers are initialized to zero after reset
        // SLAU445I Section 4.3.5: Registers should be in known state after reset
        var registerFile = new RegisterFile();

        // Set register to non-zero value first
        registerFile.WriteRegister(register, 0xFFFF);

        // Reset and verify it's zero
        registerFile.Reset();
        ushort result = registerFile.ReadRegister(register);

        Assert.Equal((ushort)0x0000, result);
    }

    [Fact]
    public void RegisterFile_Reset_ProgramCounterIsZero()
    {
        // Test validates that PC is reset to zero
        // SLAU445I Section 4.3.1: PC behavior during reset
        var registerFile = new RegisterFile();

        // Set PC to non-zero value
        registerFile.WriteRegister(RegisterName.PC, 0x1000);

        // Reset and verify PC is zero
        registerFile.Reset();
        ushort pc = registerFile.ReadRegister(RegisterName.PC);

        Assert.Equal((ushort)0x0000, pc);
    }

    [Fact]
    public void RegisterFile_Reset_StackPointerIsZero()
    {
        // Test validates that SP is reset to zero
        // SLAU445I Section 4.3.2: SP behavior during reset
        var registerFile = new RegisterFile();

        // Set SP to non-zero value
        registerFile.WriteRegister(RegisterName.SP, 0x2000);

        // Reset and verify SP is zero
        registerFile.Reset();
        ushort sp = registerFile.ReadRegister(RegisterName.SP);

        Assert.Equal((ushort)0x0000, sp);
    }

    [Fact]
    public void RegisterFile_Reset_StatusRegisterIsReset()
    {
        // Test validates that SR is properly reset
        // SLAU445I Section 4.3.3: SR behavior during reset
        var registerFile = new RegisterFile();

        // Set flags in SR
        registerFile.StatusRegister.Carry = true;
        registerFile.StatusRegister.GeneralInterruptEnable = true;

        // Reset and verify SR is cleared
        registerFile.Reset();

        Assert.False(registerFile.StatusRegister.Carry);
        Assert.False(registerFile.StatusRegister.GeneralInterruptEnable);
        Assert.Equal((ushort)0x0000, registerFile.ReadRegister(RegisterName.SR));
    }

    [Theory]
    [InlineData(RegisterName.R0)]  // PC
    [InlineData(RegisterName.R1)]  // SP  
    [InlineData(RegisterName.R2)]  // SR/CG1
    [InlineData(RegisterName.R3)]  // CG2
    [InlineData(RegisterName.R4)]  // General purpose
    [InlineData(RegisterName.R15)] // General purpose
    public void RegisterFile_AllRegisters_SupportFullRange(RegisterName register)
    {
        // Test validates that all registers can store full 16-bit values
        // SLAU445I Section 4.3: All registers are 16-bit
        var registerFile = new RegisterFile();

        // Test edge values
        registerFile.WriteRegister(register, 0x0000);
        Assert.Equal((ushort)0x0000, registerFile.ReadRegister(register));

        registerFile.WriteRegister(register, 0xFFFF);

        // Note: PC and SP may be word-aligned, so we check the actual stored value
        ushort result = registerFile.ReadRegister(register);
        if (register == RegisterName.PC || register == RegisterName.SP)
        {
            // Should be word-aligned (even)
            Assert.Equal(0, result % 2);
        }
        else
        {
            // Should store full value
            Assert.Equal((ushort)0xFFFF, result);
        }
    }

    [Fact]
    public void RegisterFile_IndependentOperation_RegistersDoNotInterfere()
    {
        // Test validates that writing to one register doesn't affect others
        // SLAU445I Section 4.3: Independent register operation
        var registerFile = new RegisterFile();

        // Set unique values in multiple registers
        registerFile.WriteRegister(RegisterName.R4, 0x1111);
        registerFile.WriteRegister(RegisterName.R5, 0x2222);
        registerFile.WriteRegister(RegisterName.R6, 0x3333);
        registerFile.WriteRegister(RegisterName.R7, 0x4444);

        // Modify one register
        registerFile.WriteRegister(RegisterName.R5, 0x9999);

        // Verify other registers are unchanged
        Assert.Equal((ushort)0x1111, registerFile.ReadRegister(RegisterName.R4));
        Assert.Equal((ushort)0x9999, registerFile.ReadRegister(RegisterName.R5));
        Assert.Equal((ushort)0x3333, registerFile.ReadRegister(RegisterName.R6));
        Assert.Equal((ushort)0x4444, registerFile.ReadRegister(RegisterName.R7));
    }
}
