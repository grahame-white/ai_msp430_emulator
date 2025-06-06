using MSP430.Emulator.Cpu;

namespace MSP430.Emulator.Instructions;

/// <summary>
/// Interface for instructions that can be executed on the MSP430 CPU.
/// 
/// Provides the Execute method that performs the actual instruction operation,
/// including reading operands, performing calculations, updating flags, and
/// writing results back to the destination.
/// </summary>
public interface IExecutableInstruction
{
    /// <summary>
    /// Executes the instruction on the specified CPU state.
    /// </summary>
    /// <param name="registerFile">The CPU register file for reading/writing registers.</param>
    /// <param name="memory">The system memory for reading/writing memory locations.</param>
    /// <param name="extensionWords">Extension words associated with this instruction (immediate values, addresses, offsets).</param>
    /// <returns>The number of CPU cycles consumed by this instruction.</returns>
    uint Execute(IRegisterFile registerFile, byte[] memory, ushort[] extensionWords);
}
