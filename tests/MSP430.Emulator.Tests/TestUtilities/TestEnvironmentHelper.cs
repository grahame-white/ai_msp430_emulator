using MSP430.Emulator.Core;
using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Tests.TestUtilities;

/// <summary>
/// Provides helper methods for creating test environments used across multiple test classes.
/// </summary>
public static class TestEnvironmentHelper
{
    /// <summary>
    /// Creates a fresh test environment with a new register file and memory array.
    /// </summary>
    /// <returns>A tuple containing a new RegisterFile and a 64KB byte array for memory.</returns>
    public static (RegisterFile registerFile, byte[] memory) CreateTestEnvironment()
    {
        return (new RegisterFile(), new byte[65536]);
    }

    /// <summary>
    /// Creates a test environment with initialized register file and memory for addressing mode tests.
    /// </summary>
    /// <returns>A tuple containing the register file and memory array with test data.</returns>
    public static (RegisterFile registerFile, byte[] memory) CreateAddressingTestEnvironment()
    {
        var registerFile = new RegisterFile();
        byte[] memory = new byte[0x10000]; // 64KB memory space

        // Initialize stack pointer to a safe location
        registerFile.SetStackPointer(0x8000);

        // Set up some test data in memory
        memory[0x0200] = 0x34; // Low byte at absolute address 0x0200
        memory[0x0201] = 0x12; // High byte at absolute address 0x0200
        memory[0x0202] = 0x78; // Low byte at absolute address 0x0202
        memory[0x0203] = 0x56; // High byte at absolute address 0x0202

        return (registerFile, memory);
    }

    /// <summary>
    /// Creates a basic instruction test environment with register file and memory.
    /// </summary>
    /// <returns>A tuple containing the register file and memory array for instruction tests.</returns>
    public static (RegisterFile registerFile, byte[] memory) CreateInstructionTestEnvironment()
    {
        return (new RegisterFile(), new byte[0x10000]); // 64KB memory space
    }
}
