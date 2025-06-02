using MSP430.Emulator.Logging;
using MSP430.Emulator.Quality;

namespace MSP430.Emulator.Tests.Quality;

public class DefectTrackerTests : IDisposable
{
    private readonly DefectTracker _defectTracker;
    private readonly ConsoleLogger _logger;

    public DefectTrackerTests()
    {
        _logger = new ConsoleLogger { MinimumLevel = LogLevel.Debug };
        _defectTracker = new DefectTracker(_logger);
    }

    [Fact]
    public void Constructor_WithValidLogger_CreatesInstance()
    {
        var logger = new ConsoleLogger();
        var tracker = new DefectTracker(logger);

        Assert.NotNull(tracker);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DefectTracker(null!));
    }

    [Fact]
    public void CreateDefect_WithValidDefect_ReturnsDefectWithId()
    {
        var defect = new Defect
        {
            Title = "Test defect",
            Description = "Test description",
            Severity = DefectSeverity.High,
            Reporter = "test-user"
        };

        Defect result = _defectTracker.CreateDefect(defect);

        Assert.NotEmpty(result.Id);
        Assert.Equal(DefectStatus.Open, result.Status);
        Assert.True(result.CreatedDate > DateTime.MinValue);
        Assert.True(result.LastUpdated > DateTime.MinValue);
    }

    [Fact]
    public void CreateDefect_WithNullDefect_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _defectTracker.CreateDefect(null!));
    }

    [Fact]
    public void CreateDefect_WithExistingId_ThrowsArgumentException()
    {
        var defect1 = new Defect { Id = "TEST-001", Title = "First defect" };
        var defect2 = new Defect { Id = "TEST-001", Title = "Second defect" };

        _defectTracker.CreateDefect(defect1);

        Assert.Throws<ArgumentException>(() => _defectTracker.CreateDefect(defect2));
    }

    [Fact]
    public void UpdateDefect_WithValidDefect_UpdatesSuccessfully()
    {
        var defect = new Defect
        {
            Title = "Original title",
            Severity = DefectSeverity.Low
        };

        Defect created = _defectTracker.CreateDefect(defect);
        Thread.Sleep(10); // Ensure different timestamps

        var updated = new Defect
        {
            Title = "Updated title",
            Severity = DefectSeverity.Critical,
            Status = DefectStatus.InProgress
        };

        Defect result = _defectTracker.UpdateDefect(created.Id, updated);

        Assert.Equal("Updated title", result.Title);
        Assert.Equal(DefectSeverity.Critical, result.Severity);
        Assert.Equal(DefectStatus.InProgress, result.Status);
        Assert.Equal(created.CreatedDate, result.CreatedDate);
        Assert.True(result.LastUpdated > created.LastUpdated);
    }

    [Fact]
    public void UpdateDefect_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        var updatedDefect = new Defect { Title = "Updated" };

        Assert.Throws<KeyNotFoundException>(() =>
            _defectTracker.UpdateDefect("NON-EXISTENT", updatedDefect));
    }

    [Fact]
    public void UpdateDefect_WithNullDefect_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _defectTracker.UpdateDefect("TEST-001", null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateDefect_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        var updatedDefect = new Defect { Title = "Updated" };

        Assert.Throws<ArgumentException>(() =>
            _defectTracker.UpdateDefect(invalidId!, updatedDefect));
    }

    [Fact]
    public void GetDefect_WithValidId_ReturnsDefect()
    {
        var defect = new Defect { Title = "Test defect" };
        Defect created = _defectTracker.CreateDefect(defect);

        Defect result = _defectTracker.GetDefect(created.Id);

        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test defect", result.Title);
    }

    [Fact]
    public void GetDefect_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            _defectTracker.GetDefect("NON-EXISTENT"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetDefect_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        Assert.Throws<ArgumentException>(() =>
            _defectTracker.GetDefect(invalidId!));
    }

    [Fact]
    public void GetDefectsByStatus_WithMultipleDefects_ReturnsCorrectFiltering()
    {
        var defect1 = new Defect { Title = "Defect 1" };
        var defect2 = new Defect { Title = "Defect 2" };
        var defect3 = new Defect { Title = "Defect 3" };

        Defect created1 = _defectTracker.CreateDefect(defect1);
        Defect created2 = _defectTracker.CreateDefect(defect2);
        Defect created3 = _defectTracker.CreateDefect(defect3);

        // Update one defect to InProgress status
        created2.Status = DefectStatus.InProgress;
        _defectTracker.UpdateDefect(created2.Id, created2);

        var openDefects = _defectTracker.GetDefectsByStatus(DefectStatus.Open).ToList();
        var inProgressDefects = _defectTracker.GetDefectsByStatus(DefectStatus.InProgress).ToList();

        Assert.Equal(2, openDefects.Count);
        Assert.Single(inProgressDefects);
        Assert.Contains(openDefects, d => d.Title == "Defect 1");
        Assert.Contains(openDefects, d => d.Title == "Defect 3");
        Assert.Contains(inProgressDefects, d => d.Title == "Defect 2");
    }

    [Fact]
    public void GetDefectsBySeverity_WithMultipleDefects_ReturnsCorrectFiltering()
    {
        var defect1 = new Defect { Title = "Critical Bug", Severity = DefectSeverity.Critical };
        var defect2 = new Defect { Title = "Minor Issue", Severity = DefectSeverity.Low };
        var defect3 = new Defect { Title = "Another Critical Bug", Severity = DefectSeverity.Critical };

        _defectTracker.CreateDefect(defect1);
        _defectTracker.CreateDefect(defect2);
        _defectTracker.CreateDefect(defect3);

        var criticalDefects = _defectTracker.GetDefectsBySeverity(DefectSeverity.Critical).ToList();
        var lowDefects = _defectTracker.GetDefectsBySeverity(DefectSeverity.Low).ToList();

        Assert.Equal(2, criticalDefects.Count);
        Assert.Single(lowDefects);
    }

    [Fact]
    public void GetDefectsByAssignee_WithMultipleDefects_ReturnsCorrectFiltering()
    {
        var defect1 = new Defect { Title = "Defect 1", Assignee = "john.doe" };
        var defect2 = new Defect { Title = "Defect 2", Assignee = "jane.smith" };
        var defect3 = new Defect { Title = "Defect 3", Assignee = "john.doe" };

        _defectTracker.CreateDefect(defect1);
        _defectTracker.CreateDefect(defect2);
        _defectTracker.CreateDefect(defect3);

        var johnDefects = _defectTracker.GetDefectsByAssignee("john.doe").ToList();
        var janeDefects = _defectTracker.GetDefectsByAssignee("jane.smith").ToList();

        Assert.Equal(2, johnDefects.Count);
        Assert.Single(janeDefects);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetDefectsByAssignee_WithInvalidAssignee_ReturnsEmpty(string? invalidAssignee)
    {
        var defects = _defectTracker.GetDefectsByAssignee(invalidAssignee!).ToList();

        Assert.Empty(defects);
    }

    [Fact]
    public void AdvanceDefectStatus_ThroughFullLifecycle_AdvancesCorrectly()
    {
        var defect = new Defect { Title = "Test defect" };
        Defect created = _defectTracker.CreateDefect(defect);

        // Open -> InProgress
        Assert.True(_defectTracker.AdvanceDefectStatus(created.Id));
        Assert.Equal(DefectStatus.InProgress, _defectTracker.GetDefect(created.Id).Status);

        // InProgress -> Testing
        Assert.True(_defectTracker.AdvanceDefectStatus(created.Id));
        Assert.Equal(DefectStatus.Testing, _defectTracker.GetDefect(created.Id).Status);

        // Testing -> Closed
        Assert.True(_defectTracker.AdvanceDefectStatus(created.Id));
        Assert.Equal(DefectStatus.Closed, _defectTracker.GetDefect(created.Id).Status);

        // Closed -> Closed (no change)
        Assert.False(_defectTracker.AdvanceDefectStatus(created.Id));
        Assert.Equal(DefectStatus.Closed, _defectTracker.GetDefect(created.Id).Status);
    }

    [Fact]
    public void GetDefectMetrics_WithMultipleDefects_ReturnsCorrectMetrics()
    {
        Defect[] defects = new[]
        {
            new Defect { Title = "D1", Severity = DefectSeverity.Critical },
            new Defect { Title = "D2", Severity = DefectSeverity.High },
            new Defect { Title = "D3", Severity = DefectSeverity.Medium },
            new Defect { Title = "D4", Severity = DefectSeverity.Low },
            new Defect { Title = "D5", Severity = DefectSeverity.Critical }
        };

        var createdDefects = new List<Defect>();
        foreach (Defect? defect in defects)
        {
            createdDefects.Add(_defectTracker.CreateDefect(defect));
        }

        // Update statuses after creation
        createdDefects[1].Status = DefectStatus.InProgress;
        _defectTracker.UpdateDefect(createdDefects[1].Id, createdDefects[1]);

        createdDefects[2].Status = DefectStatus.Testing;
        _defectTracker.UpdateDefect(createdDefects[2].Id, createdDefects[2]);

        createdDefects[3].Status = DefectStatus.Closed;
        _defectTracker.UpdateDefect(createdDefects[3].Id, createdDefects[3]);

        createdDefects[4].Status = DefectStatus.Closed;
        _defectTracker.UpdateDefect(createdDefects[4].Id, createdDefects[4]);

        DefectMetrics metrics = _defectTracker.GetDefectMetrics();

        Assert.Equal(5, metrics.TotalDefects);
        Assert.Equal(1, metrics.OpenDefects);
        Assert.Equal(1, metrics.InProgressDefects);
        Assert.Equal(1, metrics.TestingDefects);
        Assert.Equal(2, metrics.ClosedDefects);
        Assert.Equal(2, metrics.CriticalDefects);
        Assert.Equal(1, metrics.HighDefects);
        Assert.Equal(1, metrics.MediumDefects);
        Assert.Equal(1, metrics.LowDefects);
        Assert.Equal(40.0, metrics.ClosureRate); // 2/5 * 100
        Assert.Equal(60.0, metrics.HighPriorityRate); // 3/5 * 100 (2 critical + 1 high)
    }

    [Fact]
    public void GetDefectMetrics_WithNoDefects_ReturnsZeroMetrics()
    {
        DefectMetrics metrics = _defectTracker.GetDefectMetrics();

        Assert.Equal(0, metrics.TotalDefects);
        Assert.Equal(0, metrics.OpenDefects);
        Assert.Equal(0, metrics.InProgressDefects);
        Assert.Equal(0, metrics.TestingDefects);
        Assert.Equal(0, metrics.ClosedDefects);
        Assert.Equal(0, metrics.CriticalDefects);
        Assert.Equal(0, metrics.HighDefects);
        Assert.Equal(0, metrics.MediumDefects);
        Assert.Equal(0, metrics.LowDefects);
        Assert.Equal(0.0, metrics.ClosureRate);
        Assert.Equal(0.0, metrics.HighPriorityRate);
    }

    [Fact]
    public void GetAllDefects_WithMultipleDefects_ReturnsAllDefects()
    {
        var defect1 = new Defect { Title = "Defect 1" };
        var defect2 = new Defect { Title = "Defect 2" };
        var defect3 = new Defect { Title = "Defect 3" };

        _defectTracker.CreateDefect(defect1);
        _defectTracker.CreateDefect(defect2);
        _defectTracker.CreateDefect(defect3);

        var allDefects = _defectTracker.GetAllDefects().ToList();

        Assert.Equal(3, allDefects.Count);
    }

    [Fact]
    public void GetAllDefects_WithNoDefects_ReturnsEmptyCollection()
    {
        var allDefects = _defectTracker.GetAllDefects().ToList();

        Assert.Empty(allDefects);
    }

    public void Dispose()
    {
        // ConsoleLogger doesn't need disposal
    }
}
