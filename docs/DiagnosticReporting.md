# Diagnostic Report Generation for GitHub Issues

The MSP430 Emulator includes built-in diagnostic reporting functionality to streamline the process of reporting issues on GitHub. Instead of custom defect management, this system generates comprehensive diagnostic reports that can be easily attached to GitHub issues.

## Features

- **System Information**: Captures OS, hardware, and runtime details
- **Application Context**: Includes version, build, and configuration information  
- **Memory & Performance**: Reports memory usage and performance metrics
- **Recent Log History**: Optionally includes recent log entries leading up to issues
- **GitHub-Ready Format**: Outputs Markdown format perfect for GitHub issues

## Quick Start

### Basic Usage

```csharp
using MSP430.Emulator.Diagnostics;
using MSP430.Emulator.Logging;

// Use any logger (FileLogger, ConsoleLogger, etc.)
var logger = new FileLogger("emulator.log");

// Generate a diagnostic report
var reportGenerator = new DiagnosticReportGenerator(logger);
string reportPath = reportGenerator.GenerateReportToFile();

Console.WriteLine($"Report saved to: {reportPath}");
```

### Enhanced Usage with Log History

To capture recent log entries in your diagnostic reports:

```csharp
using MSP430.Emulator.Diagnostics;
using MSP430.Emulator.Logging;

// Wrap your logger with DiagnosticLogger to capture recent entries
var fileLogger = new FileLogger("emulator.log");
var diagnosticLogger = new DiagnosticLogger(fileLogger, maxRecentEntries: 100);

// Use diagnostic logger for your emulator operations
diagnosticLogger.Info("Emulator starting up");
diagnosticLogger.Warning("Some issue occurred");
diagnosticLogger.Error("Error that user might report");

// Generate report with recent log history included
var reportGenerator = new DiagnosticReportGenerator(diagnosticLogger);
string report = reportGenerator.GenerateReport();

// Or save to file
string reportPath = reportGenerator.GenerateReportToFile();
```

## How to Report Issues

When users encounter problems with the emulator:

1. **Enable Diagnostic Logging** (if not already enabled):
   ```csharp
   var logger = new DiagnosticLogger(new FileLogger("emulator.log"));
   ```

2. **Reproduce the Issue** while logging is active

3. **Generate Diagnostic Report**:
   ```csharp
   var reportGenerator = new DiagnosticReportGenerator(logger);
   string reportPath = reportGenerator.GenerateReportToFile();
   ```

4. **Create GitHub Issue**:
   - Go to the MSP430 Emulator GitHub repository
   - Click "New Issue"
   - Describe the problem and steps to reproduce
   - Attach the generated `.md` file or copy/paste its contents

## Example Report Contents

A typical diagnostic report includes:

```markdown
# MSP430 Emulator Diagnostic Report

Generated: 2025-06-02 22:55:39 UTC

## System Information

**Operating System:** Linux 6.5.0-1025-azure #26~22.04.1-Ubuntu SMP Thu Jul 11 22:33:04 UTC 2024
**Architecture:** X64
**Process Architecture:** X64
**Machine Name:** fv-az1345-350
**Processor Count:** 4

## Application Information

**Assembly:** MSP430.Emulator
**Version:** 1.0.0.0
**Framework:** .NETCoreApp,Version=v8.0

## Runtime Information

**.NET Version:** .NET 8.0.12
**Runtime Identifier:** linux-x64
**GC Memory (bytes):** 3,456,789
**Working Set (bytes):** 45,678,901

## Recent Log Entries

Recent Log Entries (last 25 entries):
```
[2025-06-02T22:55:39.751Z] [INFO] Emulator starting up
[2025-06-02T22:55:39.751Z] [WARNING] Some issue occurred  
[2025-06-02T22:55:39.751Z] [ERROR] Error that user might report
```

---

**Instructions for GitHub Issue:**
1. Copy this entire report
2. Create a new issue in the MSP430 Emulator repository
3. Paste this report in the issue description
4. Add steps to reproduce the problem above this diagnostic information
5. Include any error messages or unexpected behavior you observed
```

## Classes

### DiagnosticReportGenerator

Main class for generating diagnostic reports.

**Methods:**
- `GenerateReport()` - Returns report as string
- `GenerateReportToFile(string? filePath = null)` - Saves report to file

### DiagnosticLogger

Logger wrapper that maintains a buffer of recent log entries.

**Constructor:**
- `DiagnosticLogger(ILogger innerLogger, int maxRecentEntries = 100)`

**Additional Methods:**
- `GetRecentEntries(int maxEntries = 0)` - Get recent log entries
- `FormatRecentEntries(int maxEntries = 50)` - Format entries for reports

## Benefits Over Custom Defect Management

- **Simplicity**: No complex defect tracking system to maintain
- **GitHub Integration**: Uses GitHub's proven issue tracking
- **Easy Adoption**: Builds on existing logging infrastructure  
- **Rich Context**: Provides comprehensive diagnostic information
- **Zero Configuration**: Works out of the box with any ILogger implementation

## Files

- `src/MSP430.Emulator/Diagnostics/DiagnosticReportGenerator.cs`
- `src/MSP430.Emulator/Diagnostics/DiagnosticLogger.cs`
- `tests/MSP430.Emulator.Tests/Diagnostics/DiagnosticReportGeneratorTests.cs`
- `tests/MSP430.Emulator.Tests/Diagnostics/DiagnosticLoggerTests.cs`
- `examples/DiagnosticReportExample.cs`