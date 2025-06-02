namespace MSP430.Emulator.Quality;

/// <summary>
/// Represents the current status of a defect in its lifecycle.
/// </summary>
public enum DefectStatus
{
    /// <summary>
    /// Open - defect has been reported and is awaiting triage.
    /// </summary>
    Open = 0,

    /// <summary>
    /// In Progress - defect is actively being investigated or fixed.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Testing - defect fix is implemented and undergoing testing.
    /// </summary>
    Testing = 2,

    /// <summary>
    /// Closed - defect has been resolved and verified.
    /// </summary>
    Closed = 3
}