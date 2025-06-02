namespace MSP430.Emulator.Quality;

/// <summary>
/// Represents a defect tracked in the system.
/// </summary>
public class Defect
{
    /// <summary>
    /// Gets or sets the unique identifier for the defect.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the defect.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the defect.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the severity level of the defect.
    /// </summary>
    public DefectSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets the current status of the defect.
    /// </summary>
    public DefectStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the assignee responsible for the defect.
    /// </summary>
    public string Assignee { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reporter who identified the defect.
    /// </summary>
    public string Reporter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the defect was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the defect was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the steps to reproduce the defect.
    /// </summary>
    public string ReproductionSteps { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the environment details where the defect was observed.
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected behavior.
    /// </summary>
    public string ExpectedBehavior { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the actual behavior observed.
    /// </summary>
    public string ActualBehavior { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional tags associated with the defect.
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();
}
