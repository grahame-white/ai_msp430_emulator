namespace MSP430.Emulator.Quality;

/// <summary>
/// Represents the severity level of a defect.
/// </summary>
public enum DefectSeverity
{
    /// <summary>
    /// Low severity - minor issues that don't affect core functionality.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium severity - issues that affect functionality but have workarounds.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High severity - significant issues that affect major functionality.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical severity - severe issues that block core functionality or cause system failures.
    /// </summary>
    Critical = 3
}
