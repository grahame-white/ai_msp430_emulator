using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Quality;

/// <summary>
/// Provides functionality for tracking and managing defects in the system.
/// </summary>
public class DefectTracker
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, Defect> _defects;

    /// <summary>
    /// Initializes a new instance of the DefectTracker class.
    /// </summary>
    /// <param name="logger">The logger instance for tracking operations.</param>
    public DefectTracker(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defects = new Dictionary<string, Defect>();
    }

    /// <summary>
    /// Creates a new defect in the tracking system.
    /// </summary>
    /// <param name="defect">The defect to create.</param>
    /// <returns>The created defect with assigned ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown when defect is null.</exception>
    /// <exception cref="ArgumentException">Thrown when defect ID already exists.</exception>
    public Defect CreateDefect(Defect defect)
    {
        if (defect == null)
        {
            throw new ArgumentNullException(nameof(defect));
        }

        if (string.IsNullOrWhiteSpace(defect.Id))
        {
            defect.Id = GenerateDefectId();
        }

        if (_defects.ContainsKey(defect.Id))
        {
            throw new ArgumentException($"Defect with ID '{defect.Id}' already exists.", nameof(defect));
        }

        defect.CreatedDate = DateTime.UtcNow;
        defect.LastUpdated = DateTime.UtcNow;
        defect.Status = DefectStatus.Open;

        _defects[defect.Id] = defect;

        _logger.Log(LogLevel.Info, $"Created defect: {defect.Id} - {defect.Title} (Severity: {defect.Severity})");

        return defect;
    }

    /// <summary>
    /// Updates an existing defect in the tracking system.
    /// </summary>
    /// <param name="defectId">The ID of the defect to update.</param>
    /// <param name="updatedDefect">The updated defect information.</param>
    /// <returns>The updated defect.</returns>
    /// <exception cref="ArgumentNullException">Thrown when updatedDefect is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when defect ID is not found.</exception>
    public Defect UpdateDefect(string defectId, Defect updatedDefect)
    {
        if (string.IsNullOrWhiteSpace(defectId))
        {
            throw new ArgumentException("Defect ID cannot be null or empty.", nameof(defectId));
        }

        if (updatedDefect == null)
        {
            throw new ArgumentNullException(nameof(updatedDefect));
        }

        if (!_defects.ContainsKey(defectId))
        {
            throw new KeyNotFoundException($"Defect with ID '{defectId}' not found.");
        }

        Defect existingDefect = _defects[defectId];
        DefectStatus oldStatus = existingDefect.Status;

        updatedDefect.Id = defectId;
        updatedDefect.CreatedDate = existingDefect.CreatedDate;
        updatedDefect.LastUpdated = DateTime.UtcNow;

        _defects[defectId] = updatedDefect;

        _logger.Log(LogLevel.Info, $"Updated defect: {defectId} - Status changed from {oldStatus} to {updatedDefect.Status}");

        return updatedDefect;
    }

    /// <summary>
    /// Retrieves a defect by its ID.
    /// </summary>
    /// <param name="defectId">The ID of the defect to retrieve.</param>
    /// <returns>The defect if found.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when defect ID is not found.</exception>
    public Defect GetDefect(string defectId)
    {
        if (string.IsNullOrWhiteSpace(defectId))
        {
            throw new ArgumentException("Defect ID cannot be null or empty.", nameof(defectId));
        }

        if (!_defects.ContainsKey(defectId))
        {
            throw new KeyNotFoundException($"Defect with ID '{defectId}' not found.");
        }

        return _defects[defectId];
    }

    /// <summary>
    /// Gets all defects with the specified status.
    /// </summary>
    /// <param name="status">The status to filter by.</param>
    /// <returns>Collection of defects with the specified status.</returns>
    public IEnumerable<Defect> GetDefectsByStatus(DefectStatus status)
    {
        return _defects.Values.Where(d => d.Status == status);
    }

    /// <summary>
    /// Gets all defects with the specified severity.
    /// </summary>
    /// <param name="severity">The severity to filter by.</param>
    /// <returns>Collection of defects with the specified severity.</returns>
    public IEnumerable<Defect> GetDefectsBySeverity(DefectSeverity severity)
    {
        return _defects.Values.Where(d => d.Severity == severity);
    }

    /// <summary>
    /// Gets all defects assigned to a specific person.
    /// </summary>
    /// <param name="assignee">The assignee to filter by.</param>
    /// <returns>Collection of defects assigned to the specified person.</returns>
    public IEnumerable<Defect> GetDefectsByAssignee(string assignee)
    {
        if (string.IsNullOrWhiteSpace(assignee))
        {
            return Enumerable.Empty<Defect>();
        }

        return _defects.Values.Where(d => string.Equals(d.Assignee, assignee, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets metrics about defects in the system.
    /// </summary>
    /// <returns>Defect metrics including counts by status and severity.</returns>
    public DefectMetrics GetDefectMetrics()
    {
        var metrics = new DefectMetrics();

        foreach (Defect defect in _defects.Values)
        {
            metrics.TotalDefects++;

            switch (defect.Status)
            {
                case DefectStatus.Open:
                    metrics.OpenDefects++;
                    break;
                case DefectStatus.InProgress:
                    metrics.InProgressDefects++;
                    break;
                case DefectStatus.Testing:
                    metrics.TestingDefects++;
                    break;
                case DefectStatus.Closed:
                    metrics.ClosedDefects++;
                    break;
            }

            switch (defect.Severity)
            {
                case DefectSeverity.Critical:
                    metrics.CriticalDefects++;
                    break;
                case DefectSeverity.High:
                    metrics.HighDefects++;
                    break;
                case DefectSeverity.Medium:
                    metrics.MediumDefects++;
                    break;
                case DefectSeverity.Low:
                    metrics.LowDefects++;
                    break;
            }
        }

        return metrics;
    }

    /// <summary>
    /// Advances a defect to the next status in the lifecycle.
    /// </summary>
    /// <param name="defectId">The ID of the defect to advance.</param>
    /// <returns>True if the status was advanced, false if already at final status.</returns>
    public bool AdvanceDefectStatus(string defectId)
    {
        Defect defect = GetDefect(defectId);
        DefectStatus originalStatus = defect.Status;

        defect.Status = defect.Status switch
        {
            DefectStatus.Open => DefectStatus.InProgress,
            DefectStatus.InProgress => DefectStatus.Testing,
            DefectStatus.Testing => DefectStatus.Closed,
            DefectStatus.Closed => DefectStatus.Closed,
            _ => defect.Status
        };

        if (defect.Status != originalStatus)
        {
            defect.LastUpdated = DateTime.UtcNow;
            _logger.Log(LogLevel.Info, $"Advanced defect {defectId} from {originalStatus} to {defect.Status}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets all defects in the system.
    /// </summary>
    /// <returns>Collection of all defects.</returns>
    public IEnumerable<Defect> GetAllDefects()
    {
        return _defects.Values.ToList();
    }

    /// <summary>
    /// Generates a unique defect ID.
    /// </summary>
    /// <returns>A unique defect ID.</returns>
    private string GenerateDefectId()
    {
        return $"DEF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}
