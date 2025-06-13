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
    public void AllConditionalJumpInstructions_HaveCorrectFormat()
    {
        // Arrange & Act
        var jz = new JzInstruction(0x2000, 0);
        var jnz = new JnzInstruction(0x2000, 0);
        var jc = new JcInstruction(0x2000, 0);
        var jnc = new JncInstruction(0x2000, 0);
        var jn = new JnInstruction(0x2000, 0);
        var jge = new JgeInstruction(0x2000, 0);
        var jl = new JlInstruction(0x2000, 0);

        // Assert
        Assert.Equal(InstructionFormat.FormatIII, jz.Format);
        Assert.Equal(InstructionFormat.FormatIII, jnz.Format);
        Assert.Equal(InstructionFormat.FormatIII, jc.Format);
        Assert.Equal(InstructionFormat.FormatIII, jnc.Format);
        Assert.Equal(InstructionFormat.FormatIII, jn.Format);
        Assert.Equal(InstructionFormat.FormatIII, jge.Format);
        Assert.Equal(InstructionFormat.FormatIII, jl.Format);
    }

    [Fact]
    public void AllConditionalJumpInstructions_HaveZeroExtensionWords()
    {
        // Arrange & Act
        var jz = new JzInstruction(0x2000, 0);
        var jnz = new JnzInstruction(0x2000, 0);
        var jc = new JcInstruction(0x2000, 0);
        var jnc = new JncInstruction(0x2000, 0);
        var jn = new JnInstruction(0x2000, 0);
        var jge = new JgeInstruction(0x2000, 0);
        var jl = new JlInstruction(0x2000, 0);

        // Assert
        Assert.Equal(0, jz.ExtensionWordCount);
        Assert.Equal(0, jnz.ExtensionWordCount);
        Assert.Equal(0, jc.ExtensionWordCount);
        Assert.Equal(0, jnc.ExtensionWordCount);
        Assert.Equal(0, jn.ExtensionWordCount);
        Assert.Equal(0, jge.ExtensionWordCount);
        Assert.Equal(0, jl.ExtensionWordCount);
    }
}
