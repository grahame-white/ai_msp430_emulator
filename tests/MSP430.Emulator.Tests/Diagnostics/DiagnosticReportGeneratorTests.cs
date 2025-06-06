using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MSP430.Emulator.Diagnostics;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Tests.Diagnostics;

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
        Assert.NotEmpty(report);
        Assert.Contains("# MSP430 Emulator Diagnostic Report", report);
        Assert.Contains("## System Information", report);
        Assert.Contains("## Application Information", report);
        Assert.Contains("## Runtime Information", report);
        Assert.Contains("**Instructions for GitHub Issue:**", report);
    }

    [Fact]
    public void GenerateReport_ShouldIncludeSystemInfo()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("**Operating System:**", report);
        Assert.Contains("**Architecture:**", report);
        Assert.Contains("**Machine Name:**", report);
        Assert.Contains("**Processor Count:**", report);
    }

    [Fact]
    public void GenerateReport_ShouldIncludeApplicationInfo()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("**Assembly:**", report);
        Assert.Contains("**Version:**", report);
        Assert.Contains("**Framework:**", report);
    }

    [Fact]
    public void GenerateReport_ShouldIncludeRuntimeInfo()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("**.NET Version:**", report);
        Assert.Contains("**Runtime Identifier:**", report);
        Assert.Contains("**GC Memory (bytes):**", report);
        Assert.Contains("**Working Set (bytes):**", report);
    }

    [Fact]
    public void GenerateReport_WithDiagnosticLogger_ShouldIncludeRecentEntries()
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
        Assert.Contains("Test log entry 1", report);
        Assert.Contains("Test log entry 2", report);
        Assert.Contains("Test log entry 3", report);

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
    public void GenerateReport_WithStandardLogger_ShouldIndicateNoRecentEntries()
    {
        // Arrange
        var logger = new ConsoleLogger { IsOutputSuppressed = true };
        var generator = new DiagnosticReportGenerator(logger);

        // Act
        string report = generator.GenerateReport();

        // Assert
        Assert.Contains("## Recent Log Entries", report);
        Assert.Contains("Recent log entries not available", report);
    }

    [Fact]
    public void GenerateReportToFile_ShouldCreateFile()
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
            Assert.True(File.Exists(testFile));

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
    public void GenerateReportToFile_WithNullPath_ShouldCreateDefaultNamedFile()
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
            Assert.Contains("msp430-diagnostic-", Path.GetFileName(generatedPath));
            Assert.EndsWith(".md", generatedPath);

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
