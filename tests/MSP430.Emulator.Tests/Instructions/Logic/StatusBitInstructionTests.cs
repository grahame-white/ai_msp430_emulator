using System;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.Logic;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.Logic;

/// <summary>
/// Unit tests for the status bit manipulation instruction classes.
/// </summary>
public class StatusBitInstructionTests
{
    /// <summary>
    /// Tests for the SETC (Set Carry) instruction.
    /// </summary>
    public class SetcInstructionTests
    {
        [Theory]
        [InlineData(InstructionFormat.FormatI)]
        public void Constructor_ValidParameters_SetsFormat(InstructionFormat expectedFormat)
        {
            // Arrange & Act
            var instruction = new SetcInstruction(0x0000);

            // Assert
            Assert.Equal(expectedFormat, instruction.Format);
        }

        [Theory]
        [InlineData(0x4)]
        public void Constructor_ValidParameters_SetsOpcode(ushort expectedOpcode)
        {
            // Arrange & Act
            var instruction = new SetcInstruction(0x0000);

            // Assert
            Assert.Equal(expectedOpcode, instruction.Opcode); // MOV instruction opcode
        }

        [Theory]
        [InlineData("SETC")]
        public void Constructor_ValidParameters_SetsMnemonic(string expectedMnemonic)
        {
            // Arrange & Act
            var instruction = new SetcInstruction(0x0000);

            // Assert
            Assert.Equal(expectedMnemonic, instruction.Mnemonic);
        }

        [Theory]
        [InlineData(false)]
        public void Constructor_ValidParameters_SetsByteOperation(bool expectedByteOp)
        {
            // Arrange & Act
            var instruction = new SetcInstruction(0x0000);

            // Assert
            Assert.Equal(expectedByteOp, instruction.IsByteOperation);
        }

        [Theory]
        [InlineData(0)]
        public void Constructor_ValidParameters_SetsExtensionWordCount(int expectedCount)
        {
            // Arrange & Act
            var instruction = new SetcInstruction(0x0000);

            // Assert
            Assert.Equal(expectedCount, instruction.ExtensionWordCount);
        }

        [Fact]
        public void Execute_ClearCarryFlag_SetsCarryFlag()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new SetcInstruction(0x0000);

            // Ensure carry is initially clear
            registerFile.StatusRegister.Carry = false;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Carry);
        }

        [Fact]
        public void Execute_SetCarryFlag_Takes1Cycle()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new SetcInstruction(0x0000);

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(1u, cycles);
        }

        [Fact]
        public void Execute_CarryAlreadySet_KeepsCarrySet()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new SetcInstruction(0x0000);

            // Set carry initially
            registerFile.StatusRegister.Carry = true;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Carry);
        }

        [Fact]
        public void Execute_SetsCarryFlag()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new SetcInstruction(0x0000);

            // Set other flags
            registerFile.StatusRegister.Zero = true;
            registerFile.StatusRegister.Negative = true;
            registerFile.StatusRegister.Overflow = true;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Carry);
        }

        [Theory]
        [InlineData(true, true, true)] // Zero, Negative, Overflow
        public void Execute_DoesNotAffectOtherFlags(bool expectedZero, bool expectedNegative, bool expectedOverflow)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new SetcInstruction(0x0000);

            // Set other flags
            registerFile.StatusRegister.Zero = expectedZero;
            registerFile.StatusRegister.Negative = expectedNegative;
            registerFile.StatusRegister.Overflow = expectedOverflow;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
            Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
            Assert.Equal(expectedOverflow, registerFile.StatusRegister.Overflow);
        }

        [Fact]
        public void ToString_ReturnsCorrectMnemonic()
        {
            // Arrange
            var instruction = new SetcInstruction(0x0000);

            // Act
            string result = instruction.ToString();

            // Assert
            Assert.Equal("SETC", result);
        }
    }

    /// <summary>
    /// Tests for the CLRC (Clear Carry) instruction.
    /// </summary>
    public class ClrcInstructionTests
    {
        [Theory]
        [InlineData(InstructionFormat.FormatI)]
        public void Constructor_ValidParameters_SetsFormat(InstructionFormat expectedFormat)
        {
            // Arrange & Act
            var instruction = new ClrcInstruction(0x0000);

            // Assert
            Assert.Equal(expectedFormat, instruction.Format);
        }

        [Theory]
        [InlineData("CLRC")]
        public void Constructor_ValidParameters_SetsMnemonic(string expectedMnemonic)
        {
            // Arrange & Act
            var instruction = new ClrcInstruction(0x0000);

            // Assert
            Assert.Equal(expectedMnemonic, instruction.Mnemonic);
        }

        [Theory]
        [InlineData(false)]
        public void Constructor_ValidParameters_SetsIsByteOperation(bool expectedByteOperation)
        {
            // Arrange & Act
            var instruction = new ClrcInstruction(0x0000);

            // Assert
            Assert.Equal(expectedByteOperation, instruction.IsByteOperation);
        }

        [Theory]
        [InlineData(0x4)]
        public void Constructor_ValidParameters_SetsOpcode(int expectedOpcode)
        {
            // Arrange & Act
            var instruction = new ClrcInstruction(0x0000);

            // Assert
            Assert.Equal(expectedOpcode, instruction.Opcode);
        }

        [Theory]
        [InlineData(0)]
        public void Constructor_ValidParameters_SetsExtensionWordCount(int expectedCount)
        {
            // Arrange & Act
            var instruction = new ClrcInstruction(0x0000);

            // Assert
            Assert.Equal(expectedCount, instruction.ExtensionWordCount);
        }

        [Fact]
        public void Execute_SetCarryFlag_ClearsCarryFlag()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new ClrcInstruction(0x0000);

            // Set carry initially
            registerFile.StatusRegister.Carry = true;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.Carry);
        }

        [Fact]
        public void Execute_SetCarryFlag_Returns1Cycle()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new ClrcInstruction(0x0000);

            // Set carry initially
            registerFile.StatusRegister.Carry = true;

            // Act
            uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(1u, cycles);
        }

        [Fact]
        public void Execute_CarryAlreadyClear_KeepsCarryClear()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new ClrcInstruction(0x0000);

            // Ensure carry is initially clear
            registerFile.StatusRegister.Carry = false;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.Carry);
        }

        [Fact]
        public void ToString_ReturnsCorrectMnemonic()
        {
            // Arrange
            var instruction = new ClrcInstruction(0x0000);

            // Act
            string result = instruction.ToString();

            // Assert
            Assert.Equal("CLRC", result);
        }
    }

    /// <summary>
    /// Tests for the SETZ (Set Zero) instruction.
    /// </summary>
    public class SetzInstructionTests
    {
        [Fact]
        public void Constructor_ValidParameters_CreatesInstruction()
        {
            // Arrange & Act
            var instruction = new SetzInstruction(0x0000);

            // Assert
            Assert.Equal("SETZ", instruction.Mnemonic);
        }

        [Fact]
        public void Execute_ClearZeroFlag_SetsZeroFlag()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new SetzInstruction(0x0000);

            // Ensure zero is initially clear
            registerFile.StatusRegister.Zero = false;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Zero);
        }

        [Fact]
        public void ToString_ReturnsCorrectMnemonic()
        {
            // Arrange
            var instruction = new SetzInstruction(0x0000);

            // Act
            string result = instruction.ToString();

            // Assert
            Assert.Equal("SETZ", result);
        }
    }

    /// <summary>
    /// Tests for the CLRZ (Clear Zero) instruction.
    /// </summary>
    public class ClrzInstructionTests
    {
        [Fact]
        public void Constructor_ValidParameters_CreatesInstruction()
        {
            // Arrange & Act
            var instruction = new ClrzInstruction(0x0000);

            // Assert
            Assert.Equal("CLRZ", instruction.Mnemonic);
        }

        [Fact]
        public void Execute_SetZeroFlag_ClearsZeroFlag()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new ClrzInstruction(0x0000);

            // Set zero initially
            registerFile.StatusRegister.Zero = true;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.Zero);
        }

        [Fact]
        public void ToString_ReturnsCorrectMnemonic()
        {
            // Arrange
            var instruction = new ClrzInstruction(0x0000);

            // Act
            string result = instruction.ToString();

            // Assert
            Assert.Equal("CLRZ", result);
        }
    }

    /// <summary>
    /// Tests for the SETN (Set Negative) instruction.
    /// </summary>
    public class SetnInstructionTests
    {
        [Fact]
        public void Constructor_ValidParameters_CreatesInstruction()
        {
            // Arrange & Act
            var instruction = new SetnInstruction(0x0000);

            // Assert
            Assert.Equal("SETN", instruction.Mnemonic);
        }

        [Fact]
        public void Execute_ClearNegativeFlag_SetsNegativeFlag()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new SetnInstruction(0x0000);

            // Ensure negative is initially clear
            registerFile.StatusRegister.Negative = false;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Negative);
        }

        [Fact]
        public void ToString_ReturnsCorrectMnemonic()
        {
            // Arrange
            var instruction = new SetnInstruction(0x0000);

            // Act
            string result = instruction.ToString();

            // Assert
            Assert.Equal("SETN", result);
        }
    }

    /// <summary>
    /// Tests for the CLRN (Clear Negative) instruction.
    /// </summary>
    public class ClrnInstructionTests
    {
        [Fact]
        public void Constructor_ValidParameters_CreatesInstruction()
        {
            // Arrange & Act
            var instruction = new ClrnInstruction(0x0000);

            // Assert
            Assert.Equal("CLRN", instruction.Mnemonic);
        }

        [Fact]
        public void Execute_SetNegativeFlag_ClearsNegativeFlag()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var instruction = new ClrnInstruction(0x0000);

            // Set negative initially
            registerFile.StatusRegister.Negative = true;

            // Act
            instruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.False(registerFile.StatusRegister.Negative);
        }

        [Fact]
        public void ToString_ReturnsCorrectMnemonic()
        {
            // Arrange
            var instruction = new ClrnInstruction(0x0000);

            // Act
            string result = instruction.ToString();

            // Assert
            Assert.Equal("CLRN", result);
        }
    }

    /// <summary>
    /// Integration tests for multiple status bit operations.
    /// </summary>
    public class StatusBitIntegrationTests
    {
        [Fact]
        public void Execute_SetcInstruction_SetsCarryFlag()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var setcInstruction = new SetcInstruction(0x0000);

            // Ensure carry is initially clear
            registerFile.StatusRegister.Carry = false;

            // Act
            setcInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Carry);
        }

        [Fact]
        public void Execute_SetzInstruction_SetsZeroFlag()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var setzInstruction = new SetzInstruction(0x0000);

            // Ensure zero is initially clear
            registerFile.StatusRegister.Zero = false;

            // Act
            setzInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Zero);
        }

        [Fact]
        public void Execute_SetnInstruction_SetsNegativeFlag()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var setnInstruction = new SetnInstruction(0x0000);

            // Ensure negative is initially clear
            registerFile.StatusRegister.Negative = false;

            // Act
            setnInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.True(registerFile.StatusRegister.Negative);
        }

        [Theory]
        [InlineData(false)]
        public void Execute_ClrcInstruction_AffectsCarryFlag(bool expectedCarry)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var clrcInstruction = new ClrcInstruction(0x0000);

            // Set all flags initially to opposite values to ensure proper testing
            registerFile.StatusRegister.Carry = true;
            registerFile.StatusRegister.Zero = true;
            registerFile.StatusRegister.Negative = true;

            // Act
            clrcInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(expectedCarry, registerFile.StatusRegister.Carry);
        }

        [Theory]
        [InlineData(true)]
        public void Execute_ClrcInstruction_AffectsZeroFlag(bool expectedZero)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var clrcInstruction = new ClrcInstruction(0x0000);

            // Set all flags initially to opposite values to ensure proper testing
            registerFile.StatusRegister.Carry = true;
            registerFile.StatusRegister.Zero = true;
            registerFile.StatusRegister.Negative = true;

            // Act
            clrcInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(expectedZero, registerFile.StatusRegister.Zero);
        }

        [Theory]
        [InlineData(true)]
        public void Execute_ClrcInstruction_AffectsNegativeFlag(bool expectedNegative)
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];
            var clrcInstruction = new ClrcInstruction(0x0000);

            // Set all flags initially to opposite values to ensure proper testing
            registerFile.StatusRegister.Carry = true;
            registerFile.StatusRegister.Zero = true;
            registerFile.StatusRegister.Negative = true;

            // Act
            clrcInstruction.Execute(registerFile, memory, Array.Empty<ushort>());

            // Assert
            Assert.Equal(expectedNegative, registerFile.StatusRegister.Negative);
        }

        [Fact]
        public void Execute_AllStatusInstructions_OneCycleEach()
        {
            // Arrange
            var registerFile = new RegisterFile();
            byte[] memory = new byte[65536];

            var instructions = new StatusBitInstruction[]
            {
                new SetcInstruction(0x0000),
                new ClrcInstruction(0x0000),
                new SetzInstruction(0x0000),
                new ClrzInstruction(0x0000),
                new SetnInstruction(0x0000),
                new ClrnInstruction(0x0000)
            };

            // Act & Assert
            foreach (StatusBitInstruction instruction in instructions)
            {
                uint cycles = instruction.Execute(registerFile, memory, Array.Empty<ushort>());
                Assert.Equal(1u, cycles);
            }
        }
    }
}
