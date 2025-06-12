using System;
using System.Collections.Generic;
using MSP430.Emulator.Core;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.Core;

/// <summary>
/// Unit tests for EmulatorCore reset vector loading functionality.
/// 
/// Tests MSP430 reset behavior according to SLAU445I Section 1.2.1:
/// "Upon completion of the boot code, the PC is loaded with the address 
/// contained at the SYSRSTIV reset location (0FFFEh)."
/// 
/// References:
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1.2.1: Device Initial Conditions After System Reset
/// </summary>
public class EmulatorCoreResetTests
{
    private readonly RegisterFile _registerFile;
    private readonly MemoryMap _memoryMap;
    private readonly InstructionDecoder _instructionDecoder;
    private readonly TestLogger _logger;
    private readonly EmulatorCore _emulatorCore;

    public EmulatorCoreResetTests()
    {
        _logger = new TestLogger();
        _registerFile = new RegisterFile(_logger);
        _memoryMap = new MemoryMap();
        _instructionDecoder = new InstructionDecoder();

        _emulatorCore = new EmulatorCore(_registerFile, _memoryMap, _instructionDecoder, _logger);
    }

    [Fact]
    public void Reset_WithValidResetVector_LoadsPCFromResetVector()
    {
        // Arrange
        ushort expectedPCValue = 0x4000; // Valid program memory address
        SetResetVector(expectedPCValue);

        // Act
        _emulatorCore.Reset();

        // Assert
        Assert.Equal(expectedPCValue, _registerFile.GetProgramCounter());
    }

    [Fact]
    public void Reset_WithUninitializedResetVector_LoadsPCToZero()
    {
        // Arrange - memory starts as all zeros, so reset vector will be 0x0000

        // Act
        _emulatorCore.Reset();

        // Assert
        Assert.Equal((ushort)0x0000, _registerFile.GetProgramCounter());
    }

    [Theory]
    [InlineData(0x4000)]  // Start of FRAM
    [InlineData(0x8000)]  // Typical program start
    [InlineData(0xBFFE)]  // End of FRAM - 2 (word aligned)
    [InlineData(0x2000)]  // Start of RAM (executable)
    public void Reset_WithValidResetVector_LoadsPCCorrectly(ushort resetVectorValue)
    {
        // Arrange
        SetResetVector(resetVectorValue);

        // Act
        _emulatorCore.Reset();

        // Assert
        Assert.Equal(resetVectorValue, _registerFile.GetProgramCounter());
    }

    [Fact]
    public void Reset_AfterLoadingResetVector_ClearsOtherRegisters()
    {
        // Arrange
        ushort resetVectorValue = 0x4000;
        SetResetVector(resetVectorValue);
        _registerFile.WriteRegister(RegisterName.R4, 0x1234);

        // Act
        _emulatorCore.Reset();

        // Assert
        Assert.Equal((ushort)0x0000, _registerFile.ReadRegister(RegisterName.R4));
    }

    [Fact]
    public void Reset_AfterLoadingResetVector_ClearsAllGeneralPurposeRegisters()
    {
        // Arrange
        ushort resetVectorValue = 0x4000;
        SetResetVector(resetVectorValue);
        _registerFile.WriteRegister(RegisterName.R10, 0x5678);

        // Act
        _emulatorCore.Reset();

        // Assert
        Assert.Equal((ushort)0x0000, _registerFile.ReadRegister(RegisterName.R10));
    }

    [Fact]
    public void Reset_WithResetVector_LogsResetVectorLoading()
    {
        // Arrange
        ushort resetVectorValue = 0x4000;
        SetResetVector(resetVectorValue);
        _logger.MinimumLevel = LogLevel.Debug;

        // Act
        _emulatorCore.Reset();

        // Assert
        bool hasResetVectorLog = _logger.LogEntries.Exists(entry =>
            entry.Level == LogLevel.Info &&
            entry.Message.Contains("reset vector") &&
            entry.Message.Contains("0x4000"));

        Assert.True(hasResetVectorLog);
    }

    [Fact]
    public void Reset_LoadsVectorAfterRegisterReset()
    {
        // Arrange
        ushort resetVectorValue = 0x6000;
        SetResetVector(resetVectorValue);
        _registerFile.SetProgramCounter(0x1234);

        // Act
        _emulatorCore.Reset();

        // Assert
        Assert.Equal(resetVectorValue, _registerFile.GetProgramCounter());
    }

    /// <summary>
    /// Helper method to set the reset vector in memory.
    /// The MSP430 uses little-endian format, so low byte at 0xFFFE, high byte at 0xFFFF.
    /// </summary>
    /// <param name="vectorAddress">The 16-bit address to store in the reset vector.</param>
    private void SetResetVector(ushort vectorAddress)
    {
        // Access memory through internal accessor instead of reflection
        byte[] memory = _emulatorCore.Memory;

        // MSP430 is little-endian: low byte at 0xFFFE, high byte at 0xFFFF
        memory[0xFFFE] = (byte)(vectorAddress & 0xFF);
        memory[0xFFFF] = (byte)((vectorAddress >> 8) & 0xFF);
    }

    private class TestLogger : ILogger
    {
        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;
        public List<LogEntry> LogEntries { get; } = new();

        public void Log(LogLevel level, string message)
        {
            if (IsEnabled(level))
            {
                LogEntries.Add(new LogEntry(level, message, null));
            }
        }

        public void Log(LogLevel level, string message, object? context)
        {
            if (IsEnabled(level))
            {
                LogEntries.Add(new LogEntry(level, message, context));
            }
        }

        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Debug(string message, object? context) => Log(LogLevel.Debug, message, context);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Info(string message, object? context) => Log(LogLevel.Info, message, context);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Warning(string message, object? context) => Log(LogLevel.Warning, message, context);
        public void Error(string message) => Log(LogLevel.Error, message);
        public void Error(string message, object? context) => Log(LogLevel.Error, message, context);
        public void Error(string message, Exception exception) => Log(LogLevel.Error, $"{message}: {exception}");
        public void Fatal(string message) => Log(LogLevel.Fatal, message);
        public void Fatal(string message, object? context) => Log(LogLevel.Fatal, message, context);

        public bool IsEnabled(LogLevel level) => level >= MinimumLevel;
    }

    private record LogEntry(LogLevel Level, string Message, object? Context);
}
