using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for instruction properties across all conditional jump instructions.
/// 
/// Tests that all conditional jump instructions have correct format, extension word count,
/// and other common properties according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.5.3: Jump Instructions Cycles and Lengths
/// </summary>
public class ConditionalJumpInstructionPropertiesTests
{
    [Fact]
    public void JzInstruction_HasCorrectFormat()
    {
        // Arrange & Act
        var instruction = new JzInstruction(0x2000, 0);

        // Assert
        Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
    }

    [Fact]
    public void JnzInstruction_HasCorrectFormat()
    {
        // Arrange & Act
        var instruction = new JnzInstruction(0x2000, 0);

        // Assert
        Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
    }

    [Fact]
    public void JcInstruction_HasCorrectFormat()
    {
        // Arrange & Act
        var instruction = new JcInstruction(0x2000, 0);

        // Assert
        Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
    }

    [Fact]
    public void JncInstruction_HasCorrectFormat()
    {
        // Arrange & Act
        var instruction = new JncInstruction(0x2000, 0);

        // Assert
        Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
    }

    [Fact]
    public void JnInstruction_HasCorrectFormat()
    {
        // Arrange & Act
        var instruction = new JnInstruction(0x2000, 0);

        // Assert
        Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
    }

    [Fact]
    public void JgeInstruction_HasCorrectFormat()
    {
        // Arrange & Act
        var instruction = new JgeInstruction(0x2000, 0);

        // Assert
        Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
    }

    [Fact]
    public void JlInstruction_HasCorrectFormat()
    {
        // Arrange & Act
        var instruction = new JlInstruction(0x2000, 0);

        // Assert
        Assert.Equal(InstructionFormat.FormatIII, instruction.Format);
    }

    [Fact]
    public void JzInstruction_HasZeroExtensionWords()
    {
        // Arrange & Act
        var instruction = new JzInstruction(0x2000, 0);

        // Assert
        Assert.Equal(0, instruction.ExtensionWordCount);
    }

    [Fact]
    public void JnzInstruction_HasZeroExtensionWords()
    {
        // Arrange & Act
        var instruction = new JnzInstruction(0x2000, 0);

        // Assert
        Assert.Equal(0, instruction.ExtensionWordCount);
    }

    [Fact]
    public void JcInstruction_HasZeroExtensionWords()
    {
        // Arrange & Act
        var instruction = new JcInstruction(0x2000, 0);

        // Assert
        Assert.Equal(0, instruction.ExtensionWordCount);
    }

    [Fact]
    public void JncInstruction_HasZeroExtensionWords()
    {
        // Arrange & Act
        var instruction = new JncInstruction(0x2000, 0);

        // Assert
        Assert.Equal(0, instruction.ExtensionWordCount);
    }

    [Fact]
    public void JnInstruction_HasZeroExtensionWords()
    {
        // Arrange & Act
        var instruction = new JnInstruction(0x2000, 0);

        // Assert
        Assert.Equal(0, instruction.ExtensionWordCount);
    }

    [Fact]
    public void JgeInstruction_HasZeroExtensionWords()
    {
        // Arrange & Act
        var instruction = new JgeInstruction(0x2000, 0);

        // Assert
        Assert.Equal(0, instruction.ExtensionWordCount);
    }

    [Fact]
    public void JlInstruction_HasZeroExtensionWords()
    {
        // Arrange & Act
        var instruction = new JlInstruction(0x2000, 0);

        // Assert
        Assert.Equal(0, instruction.ExtensionWordCount);
    }
}
