namespace MSP430.Emulator.Memory;

/// <summary>
/// Provides statistics about memory access operations.
/// </summary>
public class MemoryStatistics
{
    /// <summary>
    /// Gets or sets the total number of read operations performed.
    /// </summary>
    public ulong TotalReads { get; set; }

    /// <summary>
    /// Gets or sets the total number of write operations performed.
    /// </summary>
    public ulong TotalWrites { get; set; }

    /// <summary>
    /// Gets or sets the total number of instruction fetch operations performed.
    /// </summary>
    public ulong TotalInstructionFetches { get; set; }

    /// <summary>
    /// Gets or sets the total number of access violations that occurred.
    /// </summary>
    public ulong TotalViolations { get; set; }

    /// <summary>
    /// Gets or sets the total number of CPU cycles consumed by memory operations.
    /// </summary>
    public ulong TotalCycles { get; set; }

    /// <summary>
    /// Gets the total number of memory operations performed.
    /// </summary>
    public ulong TotalOperations => TotalReads + TotalWrites + TotalInstructionFetches;

    /// <summary>
    /// Resets all statistics to zero.
    /// </summary>
    public void Reset()
    {
        TotalReads = 0;
        TotalWrites = 0;
        TotalInstructionFetches = 0;
        TotalViolations = 0;
        TotalCycles = 0;
    }

    /// <summary>
    /// Returns a string representation of the memory statistics.
    /// </summary>
    /// <returns>A formatted string with memory operation statistics.</returns>
    public override string ToString()
    {
        return $"Memory Statistics: Reads={TotalReads}, Writes={TotalWrites}, " +
               $"Fetches={TotalInstructionFetches}, Violations={TotalViolations}, " +
               $"Total Operations={TotalOperations}, Total Cycles={TotalCycles}";
    }
}
