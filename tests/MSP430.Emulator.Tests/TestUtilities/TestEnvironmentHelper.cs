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
}
