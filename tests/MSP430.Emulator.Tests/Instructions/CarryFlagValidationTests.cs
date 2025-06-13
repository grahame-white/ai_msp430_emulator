using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Arithmetic;
using MSP430.Emulator.Instructions.EmulatedInstructions;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions;

/// <summary>
/// Comprehensive validation tests for carry flag behavior in MSP430 instructions.
/// 
/// Tests verify that carry-dependent instructions (ADDC, SUBC, DADD) properly
/// read and use the carry flag from the status register, ensuring compliance
/// with MSP430FR2xx FR4xx Family User's Guide (SLAU445I) specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.6.2: Instruction Set
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.3.3: Status Register (SR)
/// </summary>
public class CarryFlagValidationTests
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
    /// Tests for ADDC (Add with Carry) instruction carry flag behavior.
    /// </summary>
    public class AddcCarryTests
    {
        [Fact]
        public void ADDC_WithCarrySet_AddsCarryToResult()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x1000);
            registerFile.WriteRegister(RegisterName.R5, 0x2000);
            registerFile.StatusRegister.Carry = true; // Set carry flag

            var addcInstruction = new AddcInstruction(
                0x6445, // ADDC R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                false);

            // Act
            addcInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Expected: R5 = R4 + R5 + C = 0x1000 + 0x2000 + 1 = 0x3001
            Assert.Equal(0x3001, registerFile.ReadRegister(RegisterName.R5));
        }

        [Fact]
        public void ADDC_WithCarryClear_AddsWithoutCarry()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x1000);
            registerFile.WriteRegister(RegisterName.R5, 0x2000);
            registerFile.StatusRegister.Carry = false; // Clear carry flag

            var addcInstruction = new AddcInstruction(
                0x6445, // ADDC R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                false);

            // Act
            addcInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Expected: R5 = R4 + R5 + C = 0x1000 + 0x2000 + 0 = 0x3000
            Assert.Equal(0x3000, registerFile.ReadRegister(RegisterName.R5));
        }

        [Fact]
        public void ADDC_ByteOperation_WithCarrySet_ProducesCorrectResult()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0xFF); // Low byte = 0xFF
            registerFile.WriteRegister(RegisterName.R5, 0x01); // Low byte = 0x01
            registerFile.StatusRegister.Carry = true; // Set carry flag

            var addcInstruction = new AddcInstruction(
                0x6445, // ADDC.B R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                true); // Byte operation

            // Act
            addcInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Expected: R5 low byte = 0xFF + 0x01 + 1 = 0x101 -> 0x01 (with carry)
            Assert.Equal(0x01, registerFile.ReadRegister(RegisterName.R5) & 0xFF);
        }

        [Fact]
        public void ADDC_ByteOperation_WithCarrySet_SetsCarryFlag()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0xFF); // Low byte = 0xFF
            registerFile.WriteRegister(RegisterName.R5, 0x01); // Low byte = 0x01
            registerFile.StatusRegister.Carry = true; // Set carry flag

            var addcInstruction = new AddcInstruction(
                0x6445, // ADDC.B R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                true); // Byte operation

            // Act
            addcInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Carry); // Carry should be set
        }
    }

    /// <summary>
    /// Tests for SUBC (Subtract with Carry) instruction carry flag behavior.
    /// </summary>
    public class SubcCarryTests
    {
        [Fact]
        public void SUBC_WithCarrySet_SubtractsWithoutBorrow()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x1000); // Source
            registerFile.WriteRegister(RegisterName.R5, 0x3000); // Destination
            registerFile.StatusRegister.Carry = true; // Set carry (no borrow)

            var subcInstruction = new SubcInstruction(
                0x7445, // SUBC R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                false);

            // Act
            subcInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Expected: R5 = R5 - R4 - (1 - C) = 0x3000 - 0x1000 - 0 = 0x2000
            Assert.Equal(0x2000, registerFile.ReadRegister(RegisterName.R5));
        }

        [Fact]
        public void SUBC_WithCarryClear_SubtractsWithBorrow()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x1000); // Source
            registerFile.WriteRegister(RegisterName.R5, 0x3000); // Destination
            registerFile.StatusRegister.Carry = false; // Clear carry (borrow)

            var subcInstruction = new SubcInstruction(
                0x7445, // SUBC R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                false);

            // Act
            subcInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Expected: R5 = R5 - R4 - (1 - C) = 0x3000 - 0x1000 - 1 = 0x1FFF
            Assert.Equal(0x1FFF, registerFile.ReadRegister(RegisterName.R5));
        }
    }

    /// <summary>
    /// Tests for DADD (Decimal Add) instruction carry flag behavior.
    /// </summary>
    public class DaddCarryTests
    {
        [Fact]
        public void DADD_WithCarrySet_AddsBcdWithCarry()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x0009); // BCD 9
            registerFile.WriteRegister(RegisterName.R5, 0x0001); // BCD 1
            registerFile.StatusRegister.Carry = true; // Set carry flag

            var daddInstruction = new DaddInstruction(
                0xA445, // DADD R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                false);

            // Act
            daddInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Expected: R5 = BCD(0x0009 + 0x0001 + 1) = BCD(11) = 0x0011 (with BCD carry to next digit)
            Assert.Equal(0x0011, registerFile.ReadRegister(RegisterName.R5));
        }

        [Fact]
        public void DADD_WithCarryClear_AddsBcdWithoutCarry()
        {
            // Arrange
            (RegisterFile registerFile, byte[] memory) = CreateTestEnvironment();
            registerFile.WriteRegister(RegisterName.R4, 0x0009); // BCD 9
            registerFile.WriteRegister(RegisterName.R5, 0x0001); // BCD 1
            registerFile.StatusRegister.Carry = false; // Clear carry flag

            var daddInstruction = new DaddInstruction(
                0xA445, // DADD R4, R5
                RegisterName.R4,
                RegisterName.R5,
                AddressingMode.Register,
                AddressingMode.Register,
                false);

            // Act
            daddInstruction.Execute(registerFile, memory, System.Array.Empty<ushort>());

            // Assert
            // Expected: R5 = BCD(0x0009 + 0x0001 + 0) = BCD(10) = 0x0010 (with BCD carry to next digit)
            Assert.Equal(0x0010, registerFile.ReadRegister(RegisterName.R5));
        }
    }

    /// <summary>
    /// Tests for emulated instruction carry flag behavior.
    /// These instructions should behave identically to their core instruction equivalents.
    /// </summary>
    public class EmulatedInstructionCarryTests
    {
        /// <summary>
        /// Tests that ADC emulated instructions produce identical results to their core ADDC equivalents.
        /// </summary>
        [Theory]
        [InlineData(true, 0x1234)]  // With carry set
        [InlineData(false, 0x1234)] // With carry clear
        public void ADC_ProducesIdenticalResultToADDC(bool carryState, ushort registerValue)
        {
            // Arrange
            (RegisterFile registerFile1, byte[] memory1) = CreateTestEnvironment();
            (RegisterFile registerFile2, byte[] memory2) = CreateTestEnvironment();

            // Set up identical initial state
            registerFile1.WriteRegister(RegisterName.R4, registerValue);
            registerFile2.WriteRegister(RegisterName.R4, registerValue);
            registerFile1.StatusRegister.Carry = carryState;
            registerFile2.StatusRegister.Carry = carryState;

            // Create ADC emulated instruction (ADC R4 = ADDC #0, R4)
            var adcInstruction = new AdcInstruction(
                0x6304, // Emulated as ADDC #0, R4
                RegisterName.R4,
                AddressingMode.Register,
                false);

            // Create equivalent ADDC instruction (ADDC #0, R4)
            var addcInstruction = new AddcInstruction(
                0x6304, // ADDC #0, R4
                RegisterName.R3, // CG2 for constant #0
                RegisterName.R4,
                AddressingMode.Register, // Use register mode for constant #0
                AddressingMode.Register,
                false);

            // Act
            adcInstruction.Execute(registerFile1, memory1, System.Array.Empty<ushort>());
            addcInstruction.Execute(registerFile2, memory2, System.Array.Empty<ushort>());

            // Assert
            Assert.Equal(registerFile2.ReadRegister(RegisterName.R4), registerFile1.ReadRegister(RegisterName.R4));
        }

        /// <summary>
        /// Tests that ADC emulated instructions produce identical status flags to their core ADDC equivalents.
        /// </summary>
        [Theory]
        [InlineData(true, 0x1234, "Carry")]
        [InlineData(true, 0x1234, "Zero")]
        [InlineData(true, 0x1234, "Negative")]
        [InlineData(true, 0x1234, "Overflow")]
        [InlineData(false, 0x1234, "Carry")]
        [InlineData(false, 0x1234, "Zero")]
        [InlineData(false, 0x1234, "Negative")]
        [InlineData(false, 0x1234, "Overflow")]
        public void ADC_ProducesIdenticalStatusFlagsToADDC(bool carryState, ushort registerValue, string flagName)
        {
            // Arrange
            (RegisterFile registerFile1, byte[] memory1) = CreateTestEnvironment();
            (RegisterFile registerFile2, byte[] memory2) = CreateTestEnvironment();

            // Set up identical initial state
            registerFile1.WriteRegister(RegisterName.R4, registerValue);
            registerFile2.WriteRegister(RegisterName.R4, registerValue);
            registerFile1.StatusRegister.Carry = carryState;
            registerFile2.StatusRegister.Carry = carryState;

            // Create ADC emulated instruction (ADC R4 = ADDC #0, R4)
            var adcInstruction = new AdcInstruction(
                0x6304, // Emulated as ADDC #0, R4
                RegisterName.R4,
                AddressingMode.Register,
                false);

            // Create equivalent ADDC instruction (ADDC #0, R4)
            var addcInstruction = new AddcInstruction(
                0x6304, // ADDC #0, R4
                RegisterName.R3, // CG2 for constant #0
                RegisterName.R4,
                AddressingMode.Register, // Use register mode for constant #0
                AddressingMode.Register,
                false);

            // Act
            adcInstruction.Execute(registerFile1, memory1, System.Array.Empty<ushort>());
            addcInstruction.Execute(registerFile2, memory2, System.Array.Empty<ushort>());

            // Assert
            bool expectedFlag = GetStatusFlag(registerFile2.StatusRegister, flagName);
            bool actualFlag = GetStatusFlag(registerFile1.StatusRegister, flagName);
            Assert.Equal(expectedFlag, actualFlag);
        }

        /// <summary>
        /// Tests that SBC emulated instructions produce identical results to their core SUBC equivalents.
        /// </summary>
        [Theory]
        [InlineData(true, 0x1234)]  // With carry set (no borrow)
        [InlineData(false, 0x1234)] // With carry clear (borrow)
        public void SBC_ProducesIdenticalResultToSUBC(bool carryState, ushort registerValue)
        {
            // Arrange
            (RegisterFile registerFile1, byte[] memory1) = CreateTestEnvironment();
            (RegisterFile registerFile2, byte[] memory2) = CreateTestEnvironment();

            // Set up identical initial state
            registerFile1.WriteRegister(RegisterName.R4, registerValue);
            registerFile2.WriteRegister(RegisterName.R4, registerValue);
            registerFile1.StatusRegister.Carry = carryState;
            registerFile2.StatusRegister.Carry = carryState;

            // Create SBC emulated instruction (SBC R4 = SUBC #0, R4)
            var sbcInstruction = new SbcInstruction(
                0x7304, // Emulated as SUBC #0, R4
                RegisterName.R4,
                AddressingMode.Register,
                false);

            // Create equivalent SUBC instruction (SUBC #0, R4)
            var subcInstruction = new SubcInstruction(
                0x7304, // SUBC #0, R4
                RegisterName.R3, // CG2 for constant #0
                RegisterName.R4,
                AddressingMode.Register, // Use register mode for constant #0
                AddressingMode.Register,
                false);

            // Act
            sbcInstruction.Execute(registerFile1, memory1, System.Array.Empty<ushort>());
            subcInstruction.Execute(registerFile2, memory2, System.Array.Empty<ushort>());

            // Assert
            Assert.Equal(registerFile2.ReadRegister(RegisterName.R4), registerFile1.ReadRegister(RegisterName.R4));
        }

        /// <summary>
        /// Tests that SBC emulated instructions produce identical status flags to their core SUBC equivalents.
        /// </summary>
        [Theory]
        [InlineData(true, 0x1234, "Carry")]
        [InlineData(true, 0x1234, "Zero")]
        [InlineData(true, 0x1234, "Negative")]
        [InlineData(true, 0x1234, "Overflow")]
        [InlineData(false, 0x1234, "Carry")]
        [InlineData(false, 0x1234, "Zero")]
        [InlineData(false, 0x1234, "Negative")]
        [InlineData(false, 0x1234, "Overflow")]
        public void SBC_ProducesIdenticalStatusFlagsToSUBC(bool carryState, ushort registerValue, string flagName)
        {
            // Arrange
            (RegisterFile registerFile1, byte[] memory1) = CreateTestEnvironment();
            (RegisterFile registerFile2, byte[] memory2) = CreateTestEnvironment();

            // Set up identical initial state
            registerFile1.WriteRegister(RegisterName.R4, registerValue);
            registerFile2.WriteRegister(RegisterName.R4, registerValue);
            registerFile1.StatusRegister.Carry = carryState;
            registerFile2.StatusRegister.Carry = carryState;

            // Create SBC emulated instruction (SBC R4 = SUBC #0, R4)
            var sbcInstruction = new SbcInstruction(
                0x7304, // Emulated as SUBC #0, R4
                RegisterName.R4,
                AddressingMode.Register,
                false);

            // Create equivalent SUBC instruction (SUBC #0, R4)
            var subcInstruction = new SubcInstruction(
                0x7304, // SUBC #0, R4
                RegisterName.R3, // CG2 for constant #0
                RegisterName.R4,
                AddressingMode.Register, // Use register mode for constant #0
                AddressingMode.Register,
                false);

            // Act
            sbcInstruction.Execute(registerFile1, memory1, System.Array.Empty<ushort>());
            subcInstruction.Execute(registerFile2, memory2, System.Array.Empty<ushort>());

            // Assert
            bool expectedFlag = GetStatusFlag(registerFile2.StatusRegister, flagName);
            bool actualFlag = GetStatusFlag(registerFile1.StatusRegister, flagName);
            Assert.Equal(expectedFlag, actualFlag);
        }

        /// <summary>
        /// Tests that DADC emulated instructions produce identical results to their core DADD equivalents.
        /// </summary>
        [Theory]
        [InlineData(true, 0x0009)]  // With carry set, BCD 9
        [InlineData(false, 0x0008)] // With carry clear, BCD 8
        public void DADC_ProducesIdenticalResultToDaddInstruction(bool carryState, ushort bcdValue)
        {
            // Arrange
            (RegisterFile registerFile1, byte[] memory1) = CreateTestEnvironment();
            (RegisterFile registerFile2, byte[] memory2) = CreateTestEnvironment();

            // Set up identical initial state with BCD values
            registerFile1.WriteRegister(RegisterName.R4, bcdValue);
            registerFile2.WriteRegister(RegisterName.R4, bcdValue);
            registerFile1.StatusRegister.Carry = carryState;
            registerFile2.StatusRegister.Carry = carryState;

            // Create DADC emulated instruction (DADC R4 = DADD #0, R4)
            var dadcInstruction = new DadcInstruction(
                0xA304, // Emulated as DADD #0, R4
                RegisterName.R4,
                AddressingMode.Register,
                false);

            // Create equivalent DADD instruction (DADD #0, R4)
            var daddInstruction = new DaddInstruction(
                0xA304, // DADD #0, R4
                RegisterName.R3, // CG2 for constant #0
                RegisterName.R4,
                AddressingMode.Register, // Use register mode for constant #0
                AddressingMode.Register,
                false);

            // Act
            dadcInstruction.Execute(registerFile1, memory1, System.Array.Empty<ushort>());
            daddInstruction.Execute(registerFile2, memory2, System.Array.Empty<ushort>());

            // Assert
            Assert.Equal(registerFile2.ReadRegister(RegisterName.R4), registerFile1.ReadRegister(RegisterName.R4));
        }

        /// <summary>
        /// Tests that DADC emulated instructions produce identical status flags to their core DADD equivalents.
        /// </summary>
        [Theory]
        [InlineData(true, 0x0009, "Carry")]
        [InlineData(true, 0x0009, "Zero")]
        [InlineData(true, 0x0009, "Negative")]
        [InlineData(true, 0x0009, "Overflow")]
        [InlineData(false, 0x0008, "Carry")]
        [InlineData(false, 0x0008, "Zero")]
        [InlineData(false, 0x0008, "Negative")]
        [InlineData(false, 0x0008, "Overflow")]
        public void DADC_ProducesIdenticalStatusFlagsToDaddInstruction(bool carryState, ushort bcdValue, string flagName)
        {
            // Arrange
            (RegisterFile registerFile1, byte[] memory1) = CreateTestEnvironment();
            (RegisterFile registerFile2, byte[] memory2) = CreateTestEnvironment();

            // Set up identical initial state with BCD values
            registerFile1.WriteRegister(RegisterName.R4, bcdValue);
            registerFile2.WriteRegister(RegisterName.R4, bcdValue);
            registerFile1.StatusRegister.Carry = carryState;
            registerFile2.StatusRegister.Carry = carryState;

            // Create DADC emulated instruction (DADC R4 = DADD #0, R4)
            var dadcInstruction = new DadcInstruction(
                0xA304, // Emulated as DADD #0, R4
                RegisterName.R4,
                AddressingMode.Register,
                false);

            // Create equivalent DADD instruction (DADD #0, R4)
            var daddInstruction = new DaddInstruction(
                0xA304, // DADD #0, R4
                RegisterName.R3, // CG2 for constant #0
                RegisterName.R4,
                AddressingMode.Register, // Use register mode for constant #0
                AddressingMode.Register,
                false);

            // Act
            dadcInstruction.Execute(registerFile1, memory1, System.Array.Empty<ushort>());
            daddInstruction.Execute(registerFile2, memory2, System.Array.Empty<ushort>());

            // Assert
            bool expectedFlag = GetStatusFlag(registerFile2.StatusRegister, flagName);
            bool actualFlag = GetStatusFlag(registerFile1.StatusRegister, flagName);
            Assert.Equal(expectedFlag, actualFlag);
        }

        /// <summary>
        /// Helper method to get status flag value by name.
        /// </summary>
        private static bool GetStatusFlag(StatusRegister statusRegister, string flagName)
        {
            return flagName switch
            {
                "Carry" => statusRegister.Carry,
                "Zero" => statusRegister.Zero,
                "Negative" => statusRegister.Negative,
                "Overflow" => statusRegister.Overflow,
                _ => throw new ArgumentException($"Unknown flag name: {flagName}")
            };
        }
    }
}
