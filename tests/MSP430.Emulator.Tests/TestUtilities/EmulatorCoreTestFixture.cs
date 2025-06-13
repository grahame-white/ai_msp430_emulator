using MSP430.Emulator.Core;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.TestUtilities;

/// <summary>
/// Shared test fixture for EmulatorCore tests that provides common setup
/// to eliminate duplication across EmulatorCore test classes.
/// </summary>
public class EmulatorCoreTestFixture
{
    public RegisterFile RegisterFile { get; }
    public MemoryMap MemoryMap { get; }
    public InstructionDecoder InstructionDecoder { get; }
    public TestLogger Logger { get; }
    public EmulatorCore EmulatorCore { get; }

    public EmulatorCoreTestFixture()
    {
        Logger = new TestLogger();
        RegisterFile = new RegisterFile(Logger);
        MemoryMap = new MemoryMap();
        InstructionDecoder = new InstructionDecoder();

        EmulatorCore = new EmulatorCore(RegisterFile, MemoryMap, InstructionDecoder, Logger);
    }
}
