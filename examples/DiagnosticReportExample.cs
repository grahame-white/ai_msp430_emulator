#!/usr/bin/env dotnet run --project ../src/MSP430.Emulator/MSP430.Emulator.csproj

using MSP430.Emulator.Diagnostics;
using MSP430.Emulator.Logging;

// Example: How to generate a diagnostic report for GitHub issues

Console.WriteLine("MSP430 Emulator Diagnostic Report Generator Example");
Console.WriteLine("===================================================");
Console.WriteLine();

// 1. Set up diagnostic logging (recommended for production use)
var fileLogger = new FileLogger("emulator.log");
var diagnosticLogger = new DiagnosticLogger(fileLogger);

// 2. Use the diagnostic logger for your emulator operations
diagnosticLogger.Info("Emulator starting up");
diagnosticLogger.Info("Loading configuration");
diagnosticLogger.Warning("Some non-critical issue occurred");
diagnosticLogger.Error("An error happened that users might report");

Console.WriteLine("Simulated some emulator operations with logging...");
Console.WriteLine();

// 3. Generate diagnostic report when needed (e.g., when user encounters an issue)
var reportGenerator = new DiagnosticReportGenerator(diagnosticLogger);

Console.WriteLine("Generating diagnostic report...");
string reportPath = reportGenerator.GenerateReportToFile();

Console.WriteLine($"✅ Diagnostic report generated: {reportPath}");
Console.WriteLine();
Console.WriteLine("📋 Instructions for users experiencing issues:");
Console.WriteLine("1. Run your emulator with DiagnosticLogger enabled");
Console.WriteLine("2. Reproduce the issue");
Console.WriteLine("3. Generate a diagnostic report using DiagnosticReportGenerator");
Console.WriteLine("4. Attach the generated .md file to your GitHub issue");
Console.WriteLine();

// Show a preview of the report
Console.WriteLine("📄 Report preview:");
Console.WriteLine(new string('=', 50));
string reportContent = File.ReadAllText(reportPath);
var lines = reportContent.Split('\n');
foreach (var line in lines.Take(20)) // Show first 20 lines
{
    Console.WriteLine(line);
}
if (lines.Length > 20)
{
    Console.WriteLine("... (truncated)");
}

// Cleanup
diagnosticLogger.Dispose();

Console.WriteLine();
Console.WriteLine($"🗑️  Cleaning up temporary files...");
if (File.Exists("emulator.log"))
{
    File.Delete("emulator.log");
}
if (File.Exists(reportPath))
{
    File.Delete(reportPath);
}

Console.WriteLine("✅ Example completed!");