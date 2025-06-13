using System;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Instructions.ControlFlow;
using Xunit;

namespace MSP430.Emulator.Tests.Instructions.ControlFlow;

/// <summary>
/// Unit tests for offset range validation across all conditional jump instructions.
/// 
/// Tests that all conditional jump instructions properly validate their offset ranges
/// according to SLAU445I specifications.
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 4.5.1.3: Jump Instructions
/// </summary>
public class ConditionalJumpOffsetValidationTests
{
    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset)]
    [InlineData(ConditionalJumpInstruction.MaxOffset)]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-100)]
    public void JzInstruction_ValidOffsetRange_DoesNotThrow(short offset)
    {
        // Arrange & Act & Assert
        Assert.Null(Record.Exception(() => new JzInstruction(0x2000, offset)));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset)]
    [InlineData(ConditionalJumpInstruction.MaxOffset)]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-100)]
    public void JnzInstruction_ValidOffsetRange_DoesNotThrow(short offset)
    {
        // Arrange & Act & Assert
        Assert.Null(Record.Exception(() => new JnzInstruction(0x2000, offset)));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset)]
    [InlineData(ConditionalJumpInstruction.MaxOffset)]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-100)]
    public void JcInstruction_ValidOffsetRange_DoesNotThrow(short offset)
    {
        // Arrange & Act & Assert
        Assert.Null(Record.Exception(() => new JcInstruction(0x2000, offset)));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset)]
    [InlineData(ConditionalJumpInstruction.MaxOffset)]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-100)]
    public void JncInstruction_ValidOffsetRange_DoesNotThrow(short offset)
    {
        // Arrange & Act & Assert
        Assert.Null(Record.Exception(() => new JncInstruction(0x2000, offset)));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset)]
    [InlineData(ConditionalJumpInstruction.MaxOffset)]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-100)]
    public void JnInstruction_ValidOffsetRange_DoesNotThrow(short offset)
    {
        // Arrange & Act & Assert
        Assert.Null(Record.Exception(() => new JnInstruction(0x2000, offset)));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset)]
    [InlineData(ConditionalJumpInstruction.MaxOffset)]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-100)]
    public void JgeInstruction_ValidOffsetRange_DoesNotThrow(short offset)
    {
        // Arrange & Act & Assert
        Assert.Null(Record.Exception(() => new JgeInstruction(0x2000, offset)));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset)]
    [InlineData(ConditionalJumpInstruction.MaxOffset)]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-100)]
    public void JlInstruction_ValidOffsetRange_DoesNotThrow(short offset)
    {
        // Arrange & Act & Assert
        Assert.Null(Record.Exception(() => new JlInstruction(0x2000, offset)));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset - 1)]
    [InlineData(ConditionalJumpInstruction.MaxOffset + 1)]
    [InlineData(-1000)]
    [InlineData(1000)]
    public void JzInstruction_InvalidOffsetRange_ThrowsArgumentOutOfRangeException(short offset)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new JzInstruction(0x2000, offset));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset - 1)]
    [InlineData(ConditionalJumpInstruction.MaxOffset + 1)]
    [InlineData(-1000)]
    [InlineData(1000)]
    public void JnzInstruction_InvalidOffsetRange_ThrowsArgumentOutOfRangeException(short offset)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new JnzInstruction(0x2000, offset));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset - 1)]
    [InlineData(ConditionalJumpInstruction.MaxOffset + 1)]
    [InlineData(-1000)]
    [InlineData(1000)]
    public void JcInstruction_InvalidOffsetRange_ThrowsArgumentOutOfRangeException(short offset)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new JcInstruction(0x2000, offset));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset - 1)]
    [InlineData(ConditionalJumpInstruction.MaxOffset + 1)]
    [InlineData(-1000)]
    [InlineData(1000)]
    public void JncInstruction_InvalidOffsetRange_ThrowsArgumentOutOfRangeException(short offset)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new JncInstruction(0x2000, offset));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset - 1)]
    [InlineData(ConditionalJumpInstruction.MaxOffset + 1)]
    [InlineData(-1000)]
    [InlineData(1000)]
    public void JnInstruction_InvalidOffsetRange_ThrowsArgumentOutOfRangeException(short offset)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new JnInstruction(0x2000, offset));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset - 1)]
    [InlineData(ConditionalJumpInstruction.MaxOffset + 1)]
    [InlineData(-1000)]
    [InlineData(1000)]
    public void JgeInstruction_InvalidOffsetRange_ThrowsArgumentOutOfRangeException(short offset)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new JgeInstruction(0x2000, offset));
    }

    [Theory]
    [InlineData(ConditionalJumpInstruction.MinOffset - 1)]
    [InlineData(ConditionalJumpInstruction.MaxOffset + 1)]
    [InlineData(-1000)]
    [InlineData(1000)]
    public void JlInstruction_InvalidOffsetRange_ThrowsArgumentOutOfRangeException(short offset)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new JlInstruction(0x2000, offset));
    }
}
