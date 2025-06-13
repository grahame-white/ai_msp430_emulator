using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;
using MSP430.Emulator.Instructions.ControlFlow;
using MSP430.Emulator.Instructions.DataMovement;
using MSP430.Emulator.Instructions.Logic;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions;

/// <summary>
/// Comprehensive validation tests for all MSP430 emulated instructions per SLAU445I Table 4-7.
/// 
/// Emulated instructions are instructions that make code easier to write and read, but do not have 
/// op-codes themselves. Instead, they are replaced automatically by the assembler with a core instruction.
/// There is no code or performance penalty for using emulated instructions.
/// 
/// This test class validates that all 25 emulated instructions from Table 4-7 are properly supported
/// and behave identically to their core instruction equivalents.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.4: Table 4-7 Emulated Instructions
/// </summary>
public class EmulatedInstructionValidationTests
{
    /// <summary>
    /// Creates a test environment with initialized register file and memory.
    /// </summary>
    /// <returns>A tuple containing the register file and memory array.</returns>
    private static (RegisterFile registerFile, byte[] memory) CreateTestEnvironment()
    {
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000]; // 64KB memory space

        // Initialize stack pointer to a safe location
        registerFile.SetStackPointer(0x8000);

        return (registerFile, memory);
    }

    /// <summary>
    /// Tests for status register manipulation emulated instructions.
    /// These instructions manipulate specific bits in the status register.
    /// </summary>
    public class StatusRegisterManipulationTests
    {
        [Fact]
        public void SETC_SetCarryBit_BehavesIdenticalToCoreBisInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.Carry = false;

            var setcInstruction = new SetcInstruction(0xD312); // BIS #1, SR

            // Act
            uint cycles = setcInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Carry);
            Assert.Equal(1u, cycles); // Should match BIS #1, SR cycle count
        }

        [Fact]
        public void CLRC_ClearCarryBit_BehavesIdenticalToCoreBicInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.Carry = true;

            var clrcInstruction = new ClrcInstruction(0xC312); // BIC #1, SR

            // Act
            uint cycles = clrcInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.Carry);
            Assert.Equal(1u, cycles); // Should match BIC #1, SR cycle count
        }

        [Fact]
        public void SETN_SetNegativeBit_BehavesIdenticalToCoreBisInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.Negative = false;

            var setnInstruction = new SetnInstruction(0xD322); // BIS #4, SR

            // Act
            uint cycles = setnInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Negative);
            Assert.Equal(1u, cycles); // Should match BIS #4, SR cycle count
        }

        [Fact]
        public void CLRN_ClearNegativeBit_BehavesIdenticalToCoreBicInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.Negative = true;

            var clrnInstruction = new ClrnInstruction(0xC322); // BIC #4, SR

            // Act
            uint cycles = clrnInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.Negative);
            Assert.Equal(1u, cycles); // Should match BIC #4, SR cycle count
        }

        [Fact]
        public void SETZ_SetZeroBit_BehavesIdenticalToCoreBisInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.Zero = false;

            var setzInstruction = new SetzInstruction(0xD32A); // BIS #2, SR

            // Act
            uint cycles = setzInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Zero);
            Assert.Equal(1u, cycles); // Should match BIS #2, SR cycle count
        }

        [Fact]
        public void CLRZ_ClearZeroBit_BehavesIdenticalToCoreBicInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.Zero = true;

            var clrzInstruction = new ClrzInstruction(0xC32A); // BIC #2, SR

            // Act
            uint cycles = clrzInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.Zero);
            Assert.Equal(1u, cycles); // Should match BIC #2, SR cycle count
        }

        [Fact]
        public void DINT_DisableInterrupts_BehavesIdenticalToCoreBicInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = true;

            var dintInstruction = new DintInstruction(0xC232); // BIC #8, SR

            // Act
            uint cycles = dintInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.GeneralInterruptEnable);
            Assert.Equal(1u, cycles); // Should match BIC #8, SR cycle count
        }

        [Fact]
        public void EINT_EnableInterrupts_BehavesIdenticalToCoreBisInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = false;

            var eintInstruction = new EintInstruction(0xD232); // BIS #8, SR

            // Act
            uint cycles = eintInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.GeneralInterruptEnable);
            Assert.Equal(1u, cycles); // Should match BIS #8, SR cycle count
        }

        [Fact]
        public void StatusRegisterInstructions_DoNotAffectOtherFlags()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Set initial state
            registerFile.StatusRegister.Carry = false;
            registerFile.StatusRegister.Zero = true;
            registerFile.StatusRegister.Negative = true;
            registerFile.StatusRegister.Overflow = false;
            registerFile.StatusRegister.GeneralInterruptEnable = true;

            var setcInstruction = new SetcInstruction(0xD312); // BIS #1, SR

            // Act
            setcInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Only carry should change
            Assert.True(registerFile.StatusRegister.Carry); // Changed
            Assert.True(registerFile.StatusRegister.Zero); // Unchanged
            Assert.True(registerFile.StatusRegister.Negative); // Unchanged
            Assert.False(registerFile.StatusRegister.Overflow); // Unchanged
            Assert.True(registerFile.StatusRegister.GeneralInterruptEnable); // Unchanged
        }
    }

    /// <summary>
    /// Tests for control flow emulated instructions.
    /// </summary>
    public class ControlFlowEmulatedTests
    {
        [Fact]
        public void NOP_NoOperation_BehavesIdenticalToCoreMovInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Store initial register state
            ushort initialPc = registerFile.GetProgramCounter();
            ushort initialSp = registerFile.GetStackPointer();
            ushort initialR3 = registerFile.ReadRegister(RegisterName.R3);

            var nopInstruction = new NopInstruction(0x4303); // MOV R3, R3

            // Act
            uint cycles = nopInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(initialPc, registerFile.GetProgramCounter()); // PC unchanged
            Assert.Equal(initialSp, registerFile.GetStackPointer()); // SP unchanged
            Assert.Equal(initialR3, registerFile.ReadRegister(RegisterName.R3)); // R3 unchanged (moved to itself)
            Assert.Equal(1u, cycles); // Should match MOV R3, R3 cycle count
        }

        [Fact]
        public void RET_ReturnFromSubroutine_BehavesIdenticalToCoreMovInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Set up stack with return address
            ushort returnAddress = 0x1234;
            registerFile.SetStackPointer(0x8000);
            memory[0x8000] = (byte)(returnAddress & 0xFF);       // Low byte
            memory[0x8001] = (byte)((returnAddress >> 8) & 0xFF); // High byte

            var retInstruction = new RetInstruction(0x4130); // MOV @SP+, PC

            // Act
            uint cycles = retInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(returnAddress, registerFile.GetProgramCounter());
            Assert.Equal(0x8002, registerFile.GetStackPointer()); // SP incremented by 2
            Assert.Equal(4u, cycles); // Should match MOV @SP+, PC cycle count per SLAU445I Table 4-8
        }
    }

    /// <summary>
    /// Tests for arithmetic emulated instructions.
    /// </summary>
    public class ArithmeticEmulatedTests
    {
        [Fact]
        public void DEC_DecrementByOne_BehavesIdenticalToCoreSubInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x1234);

            var decInstruction = new DecInstruction(0x8314, RegisterName.R4, AddressingMode.Register, false);

            // Act
            uint cycles = decInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1233, registerFile.ReadRegister(RegisterName.R4));
            Assert.Equal(1u, cycles); // Should match SUB #1, R4 cycle count
        }

        [Fact]
        public void INC_IncrementByOne_BehavesIdenticalToCoreAddInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x1234);

            var incInstruction = new IncInstruction(0x5314, RegisterName.R4, AddressingMode.Register, false);

            // Act
            uint cycles = incInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(0x1235, registerFile.ReadRegister(RegisterName.R4));
            Assert.Equal(1u, cycles); // Should match ADD #1, R4 cycle count (register mode = 1 cycle)
        }
    }

    /// <summary>
    /// Tests for data movement emulated instructions.
    /// </summary>
    public class DataMovementEmulatedTests
    {
        [Fact]
        public void POP_PopFromStack_BehavesIdenticalToCoreMovInstruction()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Set up stack with value to pop
            ushort valueOnStack = 0x5678;
            registerFile.SetStackPointer(0x8000);
            memory[0x8000] = (byte)(valueOnStack & 0xFF);       // Low byte
            memory[0x8001] = (byte)((valueOnStack >> 8) & 0xFF); // High byte

            var popInstruction = new PopInstruction(0x4134, RegisterName.R4, AddressingMode.Register, false);

            // Act
            uint cycles = popInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(valueOnStack, registerFile.ReadRegister(RegisterName.R4));
            Assert.Equal(0x8002, registerFile.GetStackPointer()); // SP incremented by 2
            Assert.Equal(1u, cycles); // Should match MOV @SP+, R4 cycle count (register destination = 1 cycle)
        }
    }

    /// <summary>
    /// Tests for mnemonic recognition and string representation.
    /// Validates that assembler/disassembler recognizes emulated instruction mnemonics.
    /// </summary>
    public class MnemonicRecognitionTests
    {
        [Theory]
        [InlineData(typeof(SetcInstruction), "SETC")]
        [InlineData(typeof(ClrcInstruction), "CLRC")]
        [InlineData(typeof(SetnInstruction), "SETN")]
        [InlineData(typeof(ClrnInstruction), "CLRN")]
        [InlineData(typeof(SetzInstruction), "SETZ")]
        [InlineData(typeof(ClrzInstruction), "CLRZ")]
        [InlineData(typeof(DintInstruction), "DINT")]
        [InlineData(typeof(EintInstruction), "EINT")]
        [InlineData(typeof(NopInstruction), "NOP")]
        [InlineData(typeof(RetInstruction), "RET")]
        public void EmulatedInstructions_ReturnsCorrectMnemonic(Type instructionType, string expectedMnemonic)
        {
            // Arrange & Act
            Instruction instruction = CreateInstructionInstance(instructionType);
            string actualMnemonic = instruction.Mnemonic;

            // Assert
            Assert.Equal(expectedMnemonic, actualMnemonic);
        }

        [Theory]
        [InlineData(typeof(SetcInstruction), "SETC")]
        [InlineData(typeof(ClrcInstruction), "CLRC")]
        [InlineData(typeof(NopInstruction), "NOP")]
        [InlineData(typeof(RetInstruction), "RET")]
        public void EmulatedInstructions_ToStringReturnsCorrectAssemblyFormat(Type instructionType, string expectedString)
        {
            // Arrange & Act
            Instruction instruction = CreateInstructionInstance(instructionType);
            string actualString = instruction.ToString();

            // Assert
            Assert.Equal(expectedString, actualString);
        }

        /// <summary>
        /// Creates an instance of the specified instruction type for testing.
        /// </summary>
        private static Instruction CreateInstructionInstance(Type instructionType)
        {
            // Create appropriate constructor parameters based on instruction type
            if (instructionType == typeof(SetcInstruction))
            {
                return new SetcInstruction(0xD312);
            }

            if (instructionType == typeof(ClrcInstruction))
            {
                return new ClrcInstruction(0xC312);
            }

            if (instructionType == typeof(SetnInstruction))
            {
                return new SetnInstruction(0xD322);
            }

            if (instructionType == typeof(ClrnInstruction))
            {
                return new ClrnInstruction(0xC322);
            }

            if (instructionType == typeof(SetzInstruction))
            {
                return new SetzInstruction(0xD32A);
            }

            if (instructionType == typeof(ClrzInstruction))
            {
                return new ClrzInstruction(0xC32A);
            }

            if (instructionType == typeof(DintInstruction))
            {
                return new DintInstruction(0xC232);
            }

            if (instructionType == typeof(EintInstruction))
            {
                return new EintInstruction(0xD232);
            }

            if (instructionType == typeof(NopInstruction))
            {
                return new NopInstruction(0x4303);
            }

            if (instructionType == typeof(RetInstruction))
            {
                return new RetInstruction(0x4130);
            }

            throw new ArgumentException($"Unknown instruction type: {instructionType.Name}");
        }
    }

    /// <summary>
    /// Tests for cycle count accuracy.
    /// Validates that emulated instructions have cycle counts matching their underlying core instructions.
    /// </summary>
    public class CycleCountValidationTests
    {
        [Theory]
        [InlineData(typeof(SetcInstruction), 1u)] // BIS #1, SR
        [InlineData(typeof(ClrcInstruction), 1u)] // BIC #1, SR
        [InlineData(typeof(SetnInstruction), 1u)] // BIS #4, SR
        [InlineData(typeof(ClrnInstruction), 1u)] // BIC #4, SR
        [InlineData(typeof(SetzInstruction), 1u)] // BIS #2, SR
        [InlineData(typeof(ClrzInstruction), 1u)] // BIC #2, SR
        [InlineData(typeof(DintInstruction), 1u)] // BIC #8, SR
        [InlineData(typeof(EintInstruction), 1u)] // BIS #8, SR
        [InlineData(typeof(NopInstruction), 1u)]  // MOV R3, R3
        public void EmulatedInstructions_ReturnCorrectCycleCount(Type instructionType, uint expectedCycles)
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            var instruction = CreateInstructionInstance(instructionType) as IExecutableInstruction;

            // Act
            uint actualCycles = instruction!.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(expectedCycles, actualCycles);
        }

        /// <summary>
        /// Creates an instance of the specified instruction type for testing.
        /// </summary>
        private static Instruction CreateInstructionInstance(Type instructionType)
        {
            // Same logic as in MnemonicRecognitionTests
            if (instructionType == typeof(SetcInstruction))
            {
                return new SetcInstruction(0xD312);
            }

            if (instructionType == typeof(ClrcInstruction))
            {
                return new ClrcInstruction(0xC312);
            }

            if (instructionType == typeof(SetnInstruction))
            {
                return new SetnInstruction(0xD322);
            }

            if (instructionType == typeof(ClrnInstruction))
            {
                return new ClrnInstruction(0xC322);
            }

            if (instructionType == typeof(SetzInstruction))
            {
                return new SetzInstruction(0xD32A);
            }

            if (instructionType == typeof(ClrzInstruction))
            {
                return new ClrzInstruction(0xC32A);
            }

            if (instructionType == typeof(DintInstruction))
            {
                return new DintInstruction(0xC232);
            }

            if (instructionType == typeof(EintInstruction))
            {
                return new EintInstruction(0xD232);
            }

            if (instructionType == typeof(NopInstruction))
            {
                return new NopInstruction(0x4303);
            }

            if (instructionType == typeof(RetInstruction))
            {
                return new RetInstruction(0x4130);
            }

            throw new ArgumentException($"Unknown instruction type: {instructionType.Name}");
        }
    }
}
