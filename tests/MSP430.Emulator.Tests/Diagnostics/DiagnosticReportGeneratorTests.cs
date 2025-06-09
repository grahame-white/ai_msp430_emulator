using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MSP430.Emulator.Diagnostics;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Tests.Diagnostics;

/// <summary>
/// Unit tests for the DiagnosticReportGenerator class.
/// 
/// Tests validate diagnostic report generation for MSP430 emulator analysis.
/// Diagnostic reports provide comprehensive emulator state information including:
/// - CPU register states and execution statistics
/// - Memory usage patterns and access history
/// - Instruction execution frequency analysis
/// - Performance metrics and timing information
/// - Error summaries and exception details
/// </summary>
public class DiagnosticReportGeneratorTests
{
    [Fact]
    public void GenerateReport_ShouldReturnValidReport()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.NotNull(report);
    }

    [Fact]
    public void GenerateReport_ShouldReturnNonEmptyReport()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.NotEmpty(report);
    }

    [Fact]
    public void GenerateReport_ShouldContainTitle()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("# MSP430 Emulator Diagnostic Report", report);
    }

    [Fact]
    public void GenerateReport_ShouldContainSystemSection()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("## System Information", report);
    }

    [Fact]
    public void GenerateReport_ShouldContainApplicationSection()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("## Application Information", report);
    }

    [Fact]
    public void GenerateReport_ShouldContainRuntimeSection()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("## Runtime Information", report);
    }

    [Fact]
    public void GenerateReport_ShouldContainInstructions()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("**Instructions for GitHub Issue:**", report);
    }

    [Theory]
    [InlineData("**Operating System:**")]
    [InlineData("**Architecture:**")]
    [InlineData("**Machine Name:**")]
    [InlineData("**Processor Count:**")]
    public void GenerateReport_ShouldIncludeSystemInfo_ContainsSystemProperty(string expectedProperty)
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains(expectedProperty, report);
    }

    [Theory]
    [InlineData("**Assembly:**")]
    [InlineData("**Version:**")]
    [InlineData("**Framework:**")]
    public void GenerateReport_ShouldIncludeApplicationInfo_ContainsApplicationProperty(string expectedProperty)
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains(expectedProperty, report);
    }

    [Theory]
    [InlineData("**.NET Version:**")]
    [InlineData("**Runtime Identifier:**")]
    [InlineData("**GC Memory (bytes):**")]
    [InlineData("**Working Set (bytes):**")]
    public void GenerateReport_ShouldIncludeRuntimeInfo_ContainsRuntimeProperty(string expectedProperty)
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains(expectedProperty, report);
    }

    [Fact]
    public void GenerateReport_WithDiagnosticLogger_ShouldIncludeRecentEntriesHeader()
    {
        // Arrange
        using var fileLogger = new FileLogger("test.log");
        using var diagnosticLogger = new DiagnosticLogger(fileLogger);
        var generator = new DiagnosticReportGenerator(diagnosticLogger);

        // Add some log entries
        diagnosticLogger.Info("Test log entry 1");
        diagnosticLogger.Warning("Test log entry 2");
        diagnosticLogger.Error("Test log entry 3");

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("## Recent Log Entries", report);

        // Cleanup test file
        try
        {
            if (File.Exists("test.log"))
            {
                File.Delete("test.log");
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore file access errors during test cleanup
        }
        catch (IOException)
        {
            // Ignore I/O errors during test cleanup
        }
    }

    [Theory]
    [InlineData("Test log entry 1")]
    [InlineData("Test log entry 2")]
    [InlineData("Test log entry 3")]
    public void GenerateReport_WithDiagnosticLogger_ShouldIncludeSpecificLogEntry(string expectedEntry)
    {
        // Arrange
        using var fileLogger = new FileLogger("test.log");
        using var diagnosticLogger = new DiagnosticLogger(fileLogger);
        var generator = new DiagnosticReportGenerator(diagnosticLogger);

        // Add some log entries
        diagnosticLogger.Info("Test log entry 1");
        diagnosticLogger.Warning("Test log entry 2");
        diagnosticLogger.Error("Test log entry 3");

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains(expectedEntry, report);

        // Cleanup test file
        try
        {
            if (File.Exists("test.log"))
            {
                File.Delete("test.log");
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore file access errors during test cleanup
        }
        catch (IOException)
        {
            // Ignore I/O errors during test cleanup
        }
    }

    [Fact]
    public void GenerateReport_WithStandardLogger_ShouldIndicateNoRecentEntries_ContainsSection()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("## Recent Log Entries", report);
    }

    [Fact]
    public void GenerateReport_WithStandardLogger_ShouldIndicateNoRecentEntries_ContainsMessage()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("Recent log entries not available", report);
    }

    [Fact]
    public void GenerateReportToFile_ShouldReturnCorrectPath()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);
        string testFile = Path.Join(Path.GetTempPath(), "test-diagnostic-report.md");

        try
        {
            // Act
            string generatedPath = generator.GenerateReportToFile(testFile);

            // Assert
            Assert.Equal(Path.GetFullPath(testFile), generatedPath);
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public void GenerateReportToFile_ShouldCreateFileAtPath()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);
        string testFile = Path.Join(Path.GetTempPath(), "test-diagnostic-report.md");

        try
        {
            // Act
            generator.GenerateReportToFile(testFile);

            // Assert
            Assert.True(File.Exists(testFile));
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public void GenerateReportToFile_ShouldWriteCorrectContent()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);
        string testFile = Path.Join(Path.GetTempPath(), "test-diagnostic-report.md");

        try
        {
            // Act
            generator.GenerateReportToFile(testFile);

            // Assert
            string content = File.ReadAllText(testFile);
            Assert.Contains("# MSP430 Emulator Diagnostic Report", content);
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public void GenerateReportToFile_WithNullPath_ShouldCreateFile()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        try
        {
            // Act
            string generatedPath = generator.GenerateReportToFile();

            // Assert
            Assert.True(File.Exists(generatedPath));
        }
        finally
        {
            // Cleanup - find and delete any files we created
            string currentDir = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(currentDir, "msp430-diagnostic-*.md");
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignore file access errors during test cleanup
                }
                catch (IOException)
                {
                    // Ignore I/O errors during test cleanup
                }
            }
        }
    }

    [Fact]
    public void GenerateReportToFile_WithNullPath_ShouldUseDefaultNamePattern()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        try
        {
            // Act
            string generatedPath = generator.GenerateReportToFile();

            // Assert
            Assert.Contains("msp430-diagnostic-", Path.GetFileName(generatedPath));
        }
        finally
        {
            // Cleanup - find and delete any files we created
            string currentDir = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(currentDir, "msp430-diagnostic-*.md");
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignore file access errors during test cleanup
                }
                catch (IOException)
                {
                    // Ignore I/O errors during test cleanup
                }
            }
        }
    }

    [Fact]
    public void GenerateReportToFile_WithNullPath_ShouldUseMarkdownExtension()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        try
        {
            // Act
            string generatedPath = generator.GenerateReportToFile();

            // Assert
            Assert.EndsWith(".md", generatedPath);
        }
        finally
        {
            // Cleanup - find and delete any files we created
            string currentDir = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(currentDir, "msp430-diagnostic-*.md");
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignore file access errors during test cleanup
                }
                catch (IOException)
                {
                    // Ignore I/O errors during test cleanup
                }
            }
        }
    }

    [Fact]
    public void GenerateReportToFile_WithNullPath_ShouldWriteCorrectContent()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        try
        {
            // Act
            string generatedPath = generator.GenerateReportToFile();

            // Assert
            string content = File.ReadAllText(generatedPath);
            Assert.Contains("# MSP430 Emulator Diagnostic Report", content);
        }
        finally
        {
            // Cleanup - find and delete any files we created
            string currentDir = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(currentDir, "msp430-diagnostic-*.md");
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignore file access errors during test cleanup
                }
                catch (IOException)
                {
                    // Ignore I/O errors during test cleanup
                }
            }
        }
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DiagnosticReportGenerator(null!));
    }
}
