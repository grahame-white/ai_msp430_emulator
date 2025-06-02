using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Diagnostics;

/// <summary>
/// Generates diagnostic reports that can be attached to GitHub issues for troubleshooting.
/// </summary>
public class DiagnosticReportGenerator
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the DiagnosticReportGenerator class.
    /// </summary>
    /// <param name="logger">The logger instance to use for capturing log history.</param>
    public DiagnosticReportGenerator(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a comprehensive diagnostic report for GitHub issue reporting.
    /// </summary>
    /// <returns>A formatted diagnostic report string.</returns>
    public string GenerateReport()
    {
        var report = new StringBuilder();

        report.AppendLine("# MSP430 Emulator Diagnostic Report");
        report.AppendLine();
        report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        report.AppendLine();

        // System Information
        AppendSystemInformation(report);

        // Application Information  
        AppendApplicationInformation(report);

        // Runtime Information
        AppendRuntimeInformation(report);

        // Recent Log Entries (if available)
        AppendRecentLogEntries(report);

        report.AppendLine();
        report.AppendLine("---");
        report.AppendLine();
        report.AppendLine("**Instructions for GitHub Issue:**");
        report.AppendLine("1. Copy this entire report");
        report.AppendLine("2. Create a new issue in the MSP430 Emulator repository");
        report.AppendLine("3. Paste this report in the issue description");
        report.AppendLine("4. Add steps to reproduce the problem above this diagnostic information");
        report.AppendLine("5. Include any error messages or unexpected behavior you observed");

        return report.ToString();
    }

    /// <summary>
    /// Generates a diagnostic report and saves it to a file.
    /// </summary>
    /// <param name="filePath">The path where the report should be saved.</param>
    /// <returns>The path to the generated report file.</returns>
    public string GenerateReportToFile(string? filePath = null)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            filePath = $"msp430-diagnostic-{timestamp}.md";
        }

        string report = GenerateReport();

        // Ensure directory exists
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, report);

        _logger.Info($"Diagnostic report generated: {Path.GetFullPath(filePath)}");

        return Path.GetFullPath(filePath);
    }

    private void AppendSystemInformation(StringBuilder report)
    {
        report.AppendLine("## System Information");
        report.AppendLine();

        try
        {
            report.AppendLine($"**Operating System:** {RuntimeInformation.OSDescription}");
            report.AppendLine($"**Architecture:** {RuntimeInformation.OSArchitecture}");
            report.AppendLine($"**Process Architecture:** {RuntimeInformation.ProcessArchitecture}");
            report.AppendLine($"**Machine Name:** {Environment.MachineName}");
            report.AppendLine($"**User Name:** {Environment.UserName}");
            report.AppendLine($"**Working Directory:** {Environment.CurrentDirectory}");
            report.AppendLine($"**System Directory:** {Environment.SystemDirectory}");
            report.AppendLine($"**Processor Count:** {Environment.ProcessorCount}");

            if (OperatingSystem.IsWindows())
            {
                report.AppendLine($"**Windows Version:** {Environment.OSVersion}");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            report.AppendLine($"**Error gathering system information (access denied):** {ex.Message}");
        }
        catch (SecurityException ex)
        {
            report.AppendLine($"**Error gathering system information (security):** {ex.Message}");
        }
        catch (PlatformNotSupportedException ex)
        {
            report.AppendLine($"**Error gathering system information (platform):** {ex.Message}");
        }

        report.AppendLine();
    }

    private void AppendApplicationInformation(StringBuilder report)
    {
        report.AppendLine("## Application Information");
        report.AppendLine();

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            Version? version = assembly.GetName().Version;
            string location = assembly.Location;

            report.AppendLine($"**Assembly:** {assembly.GetName().Name}");
            report.AppendLine($"**Version:** {version}");
            report.AppendLine($"**Location:** {location}");
            report.AppendLine($"**Framework:** {Assembly.GetExecutingAssembly().GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName ?? "Unknown"}");

            // Get file version if available
            if (!string.IsNullOrEmpty(location) && File.Exists(location))
            {
                var fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(location);
                report.AppendLine($"**File Version:** {fileVersion.FileVersion}");
                report.AppendLine($"**Product Version:** {fileVersion.ProductVersion}");
            }
        }
        catch (FileNotFoundException ex)
        {
            report.AppendLine($"**Error gathering application information (file not found):** {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            report.AppendLine($"**Error gathering application information (access denied):** {ex.Message}");
        }
        catch (SecurityException ex)
        {
            report.AppendLine($"**Error gathering application information (security):** {ex.Message}");
        }

        report.AppendLine();
    }

    private void AppendRuntimeInformation(StringBuilder report)
    {
        report.AppendLine("## Runtime Information");
        report.AppendLine();

        try
        {
            report.AppendLine($"**.NET Version:** {RuntimeInformation.FrameworkDescription}");
            report.AppendLine($"**Runtime Identifier:** {RuntimeInformation.RuntimeIdentifier}");
            report.AppendLine($"**GC Memory (bytes):** {GC.GetTotalMemory(false):N0}");

            // Memory usage
            var process = System.Diagnostics.Process.GetCurrentProcess();
            report.AppendLine($"**Working Set (bytes):** {process.WorkingSet64:N0}");
            report.AppendLine($"**Private Memory (bytes):** {process.PrivateMemorySize64:N0}");
            report.AppendLine($"**Process Start Time:** {process.StartTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"**Total Processor Time:** {process.TotalProcessorTime}");

            // Environment variables (selected ones that might be relevant)
            report.AppendLine();
            report.AppendLine("### Relevant Environment Variables");
            string[] relevantVars = new[] { "DOTNET_ROOT", "DOTNET_CLI_TELEMETRY_OPTOUT", "PATH" };
            foreach (string? varName in relevantVars)
            {
                string? value = Environment.GetEnvironmentVariable(varName);
                if (!string.IsNullOrEmpty(value))
                {
                    // Truncate PATH if it's very long
                    if (varName == "PATH" && value.Length > 200)
                    {
                        value = value.Substring(0, 200) + "...";
                    }
                    report.AppendLine($"**{varName}:** {value}");
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            report.AppendLine($"**Error gathering runtime information (access denied):** {ex.Message}");
        }
        catch (SecurityException ex)
        {
            report.AppendLine($"**Error gathering runtime information (security):** {ex.Message}");
        }
        catch (PlatformNotSupportedException ex)
        {
            report.AppendLine($"**Error gathering runtime information (platform):** {ex.Message}");
        }

        report.AppendLine();
    }

    private void AppendRecentLogEntries(StringBuilder report)
    {
        report.AppendLine("## Recent Log Entries");
        report.AppendLine();

        try
        {
            // Check if the logger is a DiagnosticLogger that can provide recent entries
            if (_logger is DiagnosticLogger diagnosticLogger)
            {
                string recentEntries = diagnosticLogger.FormatRecentEntries(25);
                report.AppendLine(recentEntries);
            }
            else
            {
                report.AppendLine("*Recent log entries not available. To capture log history, use DiagnosticLogger instead of the standard logger.*");
            }
        }
        catch (InvalidOperationException ex)
        {
            report.AppendLine($"**Error retrieving recent log entries (invalid state):** {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            report.AppendLine($"**Error retrieving recent log entries (invalid argument):** {ex.Message}");
        }

        report.AppendLine();
    }
}
