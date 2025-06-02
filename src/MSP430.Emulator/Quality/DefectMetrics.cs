namespace MSP430.Emulator.Quality;

/// <summary>
/// Represents metrics about defects in the system.
/// </summary>
public class DefectMetrics
{
    /// <summary>
    /// Gets or sets the total number of defects.
    /// </summary>
    public int TotalDefects { get; set; }

    /// <summary>
    /// Gets or sets the number of open defects.
    /// </summary>
    public int OpenDefects { get; set; }

    /// <summary>
    /// Gets or sets the number of defects in progress.
    /// </summary>
    public int InProgressDefects { get; set; }

    /// <summary>
    /// Gets or sets the number of defects in testing.
    /// </summary>
    public int TestingDefects { get; set; }

    /// <summary>
    /// Gets or sets the number of closed defects.
    /// </summary>
    public int ClosedDefects { get; set; }

    /// <summary>
    /// Gets or sets the number of critical defects.
    /// </summary>
    public int CriticalDefects { get; set; }

    /// <summary>
    /// Gets or sets the number of high severity defects.
    /// </summary>
    public int HighDefects { get; set; }

    /// <summary>
    /// Gets or sets the number of medium severity defects.
    /// </summary>
    public int MediumDefects { get; set; }

    /// <summary>
    /// Gets or sets the number of low severity defects.
    /// </summary>
    public int LowDefects { get; set; }

    /// <summary>
    /// Gets the percentage of defects that are closed.
    /// </summary>
    public double ClosureRate => TotalDefects > 0 ? (double)ClosedDefects / TotalDefects * 100 : 0;

    /// <summary>
    /// Gets the percentage of defects that are critical or high severity.
    /// </summary>
    public double HighPriorityRate => TotalDefects > 0 ? (double)(CriticalDefects + HighDefects) / TotalDefects * 100 : 0;
}