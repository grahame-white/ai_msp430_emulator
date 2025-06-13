using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for string formatting across all conditional jump instructions.
/// 
/// Tests that all conditional jump instructions format their string representations
/// correctly in assembly-like format.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// </summary>
public class ConditionalJumpStringFormatTests
{
    [Theory]
    [InlineData(5, "JZ +5")]
    [InlineData(-10, "JZ -10")]
    [InlineData(0, "JZ 0")]
    public void JzInstruction_ToString_FormatsCorrectly(short offset, string expected)
    {
        // Arrange & Act
        var instruction = new JzInstruction(0x2000, offset);

        // Assert
        Assert.Equal(expected, instruction.ToString());
    }

    [Theory]
    [InlineData(5, "JNZ +5")]
    [InlineData(-10, "JNZ -10")]
    [InlineData(0, "JNZ 0")]
    public void JnzInstruction_ToString_FormatsCorrectly(short offset, string expected)
    {
        // Arrange & Act
        var instruction = new JnzInstruction(0x2000, offset);

        // Assert
        Assert.Equal(expected, instruction.ToString());
    }

    [Theory]
    [InlineData(5, "JC +5")]
    [InlineData(-10, "JC -10")]
    [InlineData(0, "JC 0")]
    public void JcInstruction_ToString_FormatsCorrectly(short offset, string expected)
    {
        // Arrange & Act
        var instruction = new JcInstruction(0x2000, offset);

        // Assert
        Assert.Equal(expected, instruction.ToString());
    }

    [Theory]
    [InlineData(5, "JNC +5")]
    [InlineData(-10, "JNC -10")]
    [InlineData(0, "JNC 0")]
    public void JncInstruction_ToString_FormatsCorrectly(short offset, string expected)
    {
        // Arrange & Act
        var instruction = new JncInstruction(0x2000, offset);

        // Assert
        Assert.Equal(expected, instruction.ToString());
    }

    [Theory]
    [InlineData(5, "JN +5")]
    [InlineData(-10, "JN -10")]
    [InlineData(0, "JN 0")]
    public void JnInstruction_ToString_FormatsCorrectly(short offset, string expected)
    {
        // Arrange & Act
        var instruction = new JnInstruction(0x2000, offset);

        // Assert
        Assert.Equal(expected, instruction.ToString());
    }

    [Theory]
    [InlineData(5, "JGE +5")]
    [InlineData(-10, "JGE -10")]
    [InlineData(0, "JGE 0")]
    public void JgeInstruction_ToString_FormatsCorrectly(short offset, string expected)
    {
        // Arrange & Act
        var instruction = new JgeInstruction(0x2000, offset);

        // Assert
        Assert.Equal(expected, instruction.ToString());
    }

    [Theory]
    [InlineData(5, "JL +5")]
    [InlineData(-10, "JL -10")]
    [InlineData(0, "JL 0")]
    public void JlInstruction_ToString_FormatsCorrectly(short offset, string expected)
    {
        // Arrange & Act
        var instruction = new JlInstruction(0x2000, offset);

        // Assert
        Assert.Equal(expected, instruction.ToString());
    }
}
