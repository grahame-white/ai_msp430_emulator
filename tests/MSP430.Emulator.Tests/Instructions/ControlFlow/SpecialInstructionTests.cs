using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using MSP430.Emulator.Tests.TestUtilities;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for special control flow instructions including NOP, DINT, and EINT.
/// 
/// Tests cover:
/// - NOP (No Operation) instruction behavior
/// - DINT (Disable Interrupts) instruction behavior
/// - EINT (Enable Interrupts) instruction behavior
/// - Status register flag manipulation
/// - Cycle count verification
/// </summary>
public class SpecialInstructionTests
{
    /// <summary>
    /// Creates a test environment with initialized register file and memory.
    /// </summary>
    /// <returns>A tuple containing the register file and memory array.</returns>
    private static (RegisterFile registerFile, byte[] memory) CreateTestEnvironment()
    {
        (RegisterFile registerFile, byte[] memory) = TestEnvironmentHelper.CreateInstructionTestEnvironment();
        return (registerFile, memory);
    }

    /// <summary>
    /// Tests for the NOP (No Operation) instruction.
    /// </summary>
    public class NopInstructionTests
    {
        [Fact]
        public void Mnemonic_ReturnsNOP()
        {
            var instruction = new NopInstruction(0x4343); // MOV R3, R3 encoding
            Assert.Equal("NOP", instruction.Mnemonic);
        }

        [Fact]
        public void Format_ReturnsFormatI()
        {
            var instruction = new NopInstruction(0x4343);
            Assert.Equal(InstructionFormat.FormatI, instruction.Format);
        }

        [Fact]
        public void Opcode_ReturnsCorrectValue()
        {
            var instruction = new NopInstruction(0x4343);
            Assert.Equal(0x4, instruction.Opcode); // MOV instruction opcode
        }

        [Fact]
        public void IsByteOperation_ReturnsFalse()
        {
            var instruction = new NopInstruction(0x4343);
            Assert.False(instruction.IsByteOperation);
        }

        [Fact]
        public void ExtensionWordCount_ReturnsZero()
        {
            var instruction = new NopInstruction(0x4343);
            Assert.Equal(0, instruction.ExtensionWordCount);
        }

        [Fact]
        public void SourceRegister_ReturnsR3()
        {
            var instruction = new NopInstruction(0x4343);
            Assert.Equal(RegisterName.R3, instruction.SourceRegister);
        }

        [Fact]
        public void DestinationRegister_ReturnsR3()
        {
            var instruction = new NopInstruction(0x4343);
            Assert.Equal(RegisterName.R3, instruction.DestinationRegister);
        }

        [Fact]
        public void SourceAddressingMode_ReturnsRegister()
        {
            var instruction = new NopInstruction(0x4343);
            Assert.Equal(AddressingMode.Register, instruction.SourceAddressingMode);
        }

        [Fact]
        public void DestinationAddressingMode_ReturnsRegister()
        {
            var instruction = new NopInstruction(0x4343);
            Assert.Equal(AddressingMode.Register, instruction.DestinationAddressingMode);
        }

        [Fact]
        public void ToString_ReturnsNOP()
        {
            var instruction = new NopInstruction(0x4343);
            Assert.Equal("NOP", instruction.ToString());
        }

        [Fact]
        public void Execute_DoesNotModifyRegisters()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Set initial values
            registerFile.WriteRegister(RegisterName.R3, 0x1234);
            registerFile.SetProgramCounter(0x8000);
            registerFile.SetStackPointer(0x1000);
            registerFile.StatusRegister.Value = 0x0055;

            // Store initial state
            ushort[] initialRegisters = registerFile.GetAllRegisters();

            var instruction = new NopInstruction(0x4343);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - No registers should change
            ushort[] finalRegisters = registerFile.GetAllRegisters();
            for (int i = 0; i < initialRegisters.Length; i++)
            {
                Assert.Equal(initialRegisters[i], finalRegisters[i]);
            }
        }

        [Theory]
        [InlineData(0x0200, 0xAA)]
        [InlineData(0x0201, 0xBB)]
        [InlineData(0x1000, 0xCC)]
        [InlineData(0x1001, 0xDD)]
        public void Execute_DoesNotModifyMemory(ushort address, byte expectedValue)
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Set test data in memory
            memory[0x0200] = 0xAA;
            memory[0x0201] = 0xBB;
            memory[0x1000] = 0xCC;
            memory[0x1001] = 0xDD;

            var instruction = new NopInstruction(0x4343);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Memory should be unchanged
            Assert.Equal(expectedValue, memory[address]);
        }

        [Fact]
        public void Execute_DoesNotModifyStatusFlags()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Set various status flags
            registerFile.StatusRegister.Carry = true;
            registerFile.StatusRegister.Zero = true;
            registerFile.StatusRegister.Negative = false;
            registerFile.StatusRegister.GeneralInterruptEnable = true;
            registerFile.StatusRegister.Overflow = false;

            ushort initialSR = registerFile.StatusRegister.Value;

            var instruction = new NopInstruction(0x4343);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Status register should be unchanged
            Assert.Equal(initialSR, registerFile.StatusRegister.Value);
        }

        [Fact]
        public void Execute_AlwaysReturns1Cycle()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            var instruction = new NopInstruction(0x4343);

            // Act & Assert
            uint cycleCount = instruction.Execute(registerFile, memory, Array.Empty<ushort>());
            Assert.Equal(1U, cycleCount);
        }
    }

    /// <summary>
    /// Tests for the DINT (Disable Interrupts) instruction.
    /// </summary>
    public class DintInstructionTests
    {
        [Fact]
        public void Mnemonic_ReturnsDINT()
        {
            var instruction = new DintInstruction(0xC232); // BIC #8, SR encoding
            Assert.Equal("DINT", instruction.Mnemonic);
        }

        [Fact]
        public void Format_ReturnsFormatI()
        {
            var instruction = new DintInstruction(0xC232);
            Assert.Equal(InstructionFormat.FormatI, instruction.Format);
        }

        [Fact]
        public void Opcode_ReturnsCorrectValue()
        {
            var instruction = new DintInstruction(0xC232);
            Assert.Equal(0xC, instruction.Opcode); // BIC instruction opcode
        }

        [Fact]
        public void Execute_ClearsGIEFlag()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = true; // Initially enabled

            var instruction = new DintInstruction(0xC232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.GeneralInterruptEnable);
        }

        [Fact]
        public void Execute_DoesNotAffectOtherFlags()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();

            // Set various flags
            registerFile.StatusRegister.Carry = true;
            registerFile.StatusRegister.Zero = false;
            registerFile.StatusRegister.Negative = true;
            registerFile.StatusRegister.GeneralInterruptEnable = true;
            registerFile.StatusRegister.Overflow = true;
            registerFile.StatusRegister.CpuOff = false;

            var instruction = new DintInstruction(0xC232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Other flags should remain unchanged
            Assert.True(registerFile.StatusRegister.Carry);
            Assert.False(registerFile.StatusRegister.Zero);
            Assert.True(registerFile.StatusRegister.Negative);
            Assert.False(registerFile.StatusRegister.GeneralInterruptEnable); // This should be cleared
            Assert.True(registerFile.StatusRegister.Overflow);
            Assert.False(registerFile.StatusRegister.CpuOff);
        }

        [Fact]
        public void Execute_GIEAlreadyClear_RemainsCleared()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = false; // Already disabled

            var instruction = new DintInstruction(0xC232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.GeneralInterruptEnable);
        }

        [Fact]
        public void Execute_AlwaysReturns1Cycle()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            var instruction = new DintInstruction(0xC232);

            // Act
            uint cycleCount = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(1U, cycleCount);
        }
    }

    /// <summary>
    /// Tests for the EINT (Enable Interrupts) instruction.
    /// </summary>
    public class EintInstructionTests
    {
        [Fact]
        public void Mnemonic_ReturnsEINT()
        {
            var instruction = new EintInstruction(0xD232); // BIS #8, SR encoding
            Assert.Equal("EINT", instruction.Mnemonic);
        }

        [Fact]
        public void Format_ReturnsFormatI()
        {
            var instruction = new EintInstruction(0xD232);
            Assert.Equal(InstructionFormat.FormatI, instruction.Format);
        }

        [Fact]
        public void Opcode_ReturnsCorrectValue()
        {
            var instruction = new EintInstruction(0xD232);
            Assert.Equal(0xD, instruction.Opcode); // BIS instruction opcode
        }

        [Fact]
        public void Execute_SetsGIEFlag()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = false; // Initially disabled

            var instruction = new EintInstruction(0xD232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.GeneralInterruptEnable);
        }

        [Fact]
        public void Execute_SetsGeneralInterruptEnable()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = false;

            var instruction = new EintInstruction(0xD232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - GIE should be set
            Assert.True(registerFile.StatusRegister.GeneralInterruptEnable);
        }

        [Fact]
        public void Execute_DoesNotAffectCarryFlag()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.Carry = false;
            registerFile.StatusRegister.GeneralInterruptEnable = false;

            var instruction = new EintInstruction(0xD232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Carry flag should remain unchanged
            Assert.False(registerFile.StatusRegister.Carry);
        }

        [Fact]
        public void Execute_DoesNotAffectZeroFlag()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.Zero = true;
            registerFile.StatusRegister.GeneralInterruptEnable = false;

            var instruction = new EintInstruction(0xD232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Zero flag should remain unchanged
            Assert.True(registerFile.StatusRegister.Zero);
        }

        [Fact]
        public void Execute_DoesNotAffectNegativeFlag()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.Negative = false;
            registerFile.StatusRegister.GeneralInterruptEnable = false;

            var instruction = new EintInstruction(0xD232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Negative flag should remain unchanged
            Assert.False(registerFile.StatusRegister.Negative);
        }

        [Fact]
        public void Execute_DoesNotAffectOverflowFlag()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.Overflow = false;
            registerFile.StatusRegister.GeneralInterruptEnable = false;

            var instruction = new EintInstruction(0xD232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - Overflow flag should remain unchanged
            Assert.False(registerFile.StatusRegister.Overflow);
        }

        [Fact]
        public void Execute_DoesNotAffectCpuOffFlag()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.CpuOff = true;
            registerFile.StatusRegister.GeneralInterruptEnable = false;

            var instruction = new EintInstruction(0xD232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert - CpuOff flag should remain unchanged
            Assert.True(registerFile.StatusRegister.CpuOff);
        }

        [Fact]
        public void Execute_GIEAlreadySet_RemainsSet()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = true; // Already enabled

            var instruction = new EintInstruction(0xD232);

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.GeneralInterruptEnable);
        }

        [Fact]
        public void Execute_AlwaysReturns1Cycle()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            var instruction = new EintInstruction(0xD232);

            // Act
            uint cycleCount = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(1U, cycleCount);
        }
    }

    /// <summary>
    /// Tests for DINT/EINT interaction scenarios.
    /// </summary>
    public class InterruptControlInteractionTests
    {
        [Fact]
        public void DintFollowedByEint_RestoresGIEFlag()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = true; // Initially enabled

            var dintInstruction = new DintInstruction(0xC232);
            var eintInstruction = new EintInstruction(0xD232);

            // Act
            dintInstruction.Execute(registerFile, memory, Array.Empty<ushort>());
            Assert.False(registerFile.StatusRegister.GeneralInterruptEnable); // Verify DINT worked

            eintInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.GeneralInterruptEnable); // Should be restored
        }

        [Fact]
        public void EintFollowedByDint_ClearsGIEFlag()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = false; // Initially disabled

            var eintInstruction = new EintInstruction(0xD232);
            var dintInstruction = new DintInstruction(0xC232);

            // Act
            eintInstruction.Execute(registerFile, memory, Array.Empty<ushort>());
            Assert.True(registerFile.StatusRegister.GeneralInterruptEnable); // Verify EINT worked

            dintInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.GeneralInterruptEnable); // Should be cleared
        }

        [Fact]
        public void EintInstruction_FromDisabledState_EnablesInterrupts()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = false;

            var eintInstruction = new EintInstruction(0xD232);

            // Act
            eintInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.GeneralInterruptEnable);
        }

        [Fact]
        public void EintInstruction_FromEnabledState_RemainsEnabled()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = true;

            var eintInstruction = new EintInstruction(0xD232);

            // Act
            eintInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.GeneralInterruptEnable);
        }

        [Fact]
        public void DintInstruction_FromEnabledState_DisablesInterrupts()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = true;

            var dintInstruction = new DintInstruction(0xC232);

            // Act
            dintInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.GeneralInterruptEnable);
        }

        [Fact]
        public void DintInstruction_FromDisabledState_RemainsDisabled()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.StatusRegister.GeneralInterruptEnable = false;

            var dintInstruction = new DintInstruction(0xC232);

            // Act
            dintInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.GeneralInterruptEnable);
        }
    }
}
