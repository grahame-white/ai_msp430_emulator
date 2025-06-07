using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Tests.Cpu;

public class RegisterFileTests
{
    private readonly TestLogger _logger;
    private readonly RegisterFile _registerFile;

    public RegisterFileTests()
    {
        _logger = new TestLogger();
        _registerFile = new RegisterFile(_logger);
    }

    [Fact]
    public void Constructor_WithoutLogger_CreatesValidInstance()
    {
        var registerFile = new RegisterFile();
        Assert.NotNull(registerFile);
    }

    [Fact]
    public void Constructor_WithoutLogger_CreatesStatusRegister()
    {
        var registerFile = new RegisterFile();
        Assert.NotNull(registerFile.StatusRegister);
    }

    [Fact]
    public void Constructor_WithLogger_CreatesValidInstance()
    {
        var registerFile = new RegisterFile(_logger);
        Assert.NotNull(registerFile);
    }

    [Fact]
    public void Constructor_WithLogger_CreatesStatusRegister()
    {
        var registerFile = new RegisterFile(_logger);
        Assert.NotNull(registerFile.StatusRegister);
    }

    [Theory]
    [InlineData(RegisterName.R0)]
    [InlineData(RegisterName.R1)]
    [InlineData(RegisterName.R2)]
    [InlineData(RegisterName.R3)]
    [InlineData(RegisterName.R4)]
    [InlineData(RegisterName.R5)]
    [InlineData(RegisterName.R6)]
    [InlineData(RegisterName.R7)]
    [InlineData(RegisterName.R8)]
    [InlineData(RegisterName.R9)]
    [InlineData(RegisterName.R10)]
    [InlineData(RegisterName.R11)]
    [InlineData(RegisterName.R12)]
    [InlineData(RegisterName.R13)]
    [InlineData(RegisterName.R14)]
    [InlineData(RegisterName.R15)]
    public void ReadRegister_ValidRegister_ReturnsZeroAfterReset(RegisterName register)
    {
        _registerFile.Reset();
        ushort value = _registerFile.ReadRegister(register);
        Assert.Equal((ushort)0, value);
    }

    [Theory]
    [InlineData(RegisterName.R0, 0x1234)]
    [InlineData(RegisterName.R4, 0xABCD)]
    [InlineData(RegisterName.R15, 0xFFFF)]
    public void WriteRegister_ValidRegister_StoresValue(RegisterName register, ushort value)
    {
        _registerFile.WriteRegister(register, value);
        ushort readValue = _registerFile.ReadRegister(register);
        Assert.Equal(value, readValue);
    }

    [Fact]
    public void WriteRegister_ProgramCounter_WordAligns()
    {
        // Write odd address to PC
        _registerFile.WriteRegister(RegisterName.R0, 0x1235);

        // Should be word-aligned (even address)
        ushort pcValue = _registerFile.ReadRegister(RegisterName.R0);
        Assert.Equal((ushort)0x1234, pcValue);
    }

    [Fact]
    public void WriteRegister_StackPointer_WordAligns()
    {
        // Write odd address to SP
        _registerFile.WriteRegister(RegisterName.R1, 0x2FFF);

        // Should be word-aligned (even address)
        ushort spValue = _registerFile.ReadRegister(RegisterName.R1);
        Assert.Equal((ushort)0x2FFE, spValue);
    }

    [Fact]
    public void WriteRegister_StatusRegister_UpdatesRegisterValue()
    {
        ushort testValue = 0x0007; // Set C, Z, N flags
        _registerFile.WriteRegister(RegisterName.R2, testValue);

        Assert.Equal(testValue, _registerFile.ReadRegister(RegisterName.R2));
    }

    [Fact]
    public void WriteRegister_StatusRegister_UpdatesStatusRegisterValue()
    {
        ushort testValue = 0x0007; // Set C, Z, N flags
        _registerFile.WriteRegister(RegisterName.R2, testValue);

        Assert.Equal(testValue, _registerFile.StatusRegister.Value);
    }

    [Fact]
    public void WriteRegister_StatusRegister_SetsCarryFlag()
    {
        ushort testValue = 0x0007; // Set C, Z, N flags
        _registerFile.WriteRegister(RegisterName.R2, testValue);

        Assert.True(_registerFile.StatusRegister.Carry);
    }

    [Fact]
    public void WriteRegister_StatusRegister_SetsZeroFlag()
    {
        ushort testValue = 0x0007; // Set C, Z, N flags
        _registerFile.WriteRegister(RegisterName.R2, testValue);

        Assert.True(_registerFile.StatusRegister.Zero);
    }

    [Fact]
    public void WriteRegister_StatusRegister_SetsNegativeFlag()
    {
        ushort testValue = 0x0007; // Set C, Z, N flags
        _registerFile.WriteRegister(RegisterName.R2, testValue);

        Assert.True(_registerFile.StatusRegister.Negative);
    }

    [Theory]
    [InlineData(RegisterName.R0, 0x1234, 0x34)]
    [InlineData(RegisterName.R4, 0xABCD, 0xCD)]
    [InlineData(RegisterName.R15, 0xFFEE, 0xEE)]
    public void ReadRegisterLowByte_ValidRegister_ReturnsCorrectByte(RegisterName register, ushort fullValue, byte expectedLowByte)
    {
        _registerFile.WriteRegister(register, fullValue);
        byte lowByte = _registerFile.ReadRegisterLowByte(register);
        Assert.Equal(expectedLowByte, lowByte);
    }

    [Theory]
    [InlineData(RegisterName.R0, 0x1234, 0x12)]
    [InlineData(RegisterName.R4, 0xABCD, 0xAB)]
    [InlineData(RegisterName.R15, 0xFFEE, 0xFF)]
    public void ReadRegisterHighByte_ValidRegister_ReturnsCorrectByte(RegisterName register, ushort fullValue, byte expectedHighByte)
    {
        _registerFile.WriteRegister(register, fullValue);
        byte highByte = _registerFile.ReadRegisterHighByte(register);
        Assert.Equal(expectedHighByte, highByte);
    }

    [Fact]
    public void WriteRegisterLowByte_ValidRegister_UpdatesLowByteOnly()
    {
        _registerFile.WriteRegister(RegisterName.R4, 0x1234);
        _registerFile.WriteRegisterLowByte(RegisterName.R4, 0xAB);

        ushort newValue = _registerFile.ReadRegister(RegisterName.R4);
        Assert.Equal((ushort)0x12AB, newValue);
    }

    [Fact]
    public void WriteRegisterHighByte_ValidRegister_UpdatesHighByteOnly()
    {
        _registerFile.WriteRegister(RegisterName.R4, 0x1234);
        _registerFile.WriteRegisterHighByte(RegisterName.R4, 0xAB);

        ushort newValue = _registerFile.ReadRegister(RegisterName.R4);
        Assert.Equal((ushort)0xAB34, newValue);
    }

    [Fact]
    public void GetProgramCounter_ReturnsR0Value()
    {
        ushort testValue = 0x8000;
        _registerFile.WriteRegister(RegisterName.R0, testValue);

        ushort pcValue = _registerFile.GetProgramCounter();
        Assert.Equal(testValue & 0xFFFE, pcValue); // Should be word-aligned
    }

    [Fact]
    public void SetProgramCounter_UpdatesR0()
    {
        ushort testValue = 0x8000;
        _registerFile.SetProgramCounter(testValue);

        ushort r0Value = _registerFile.ReadRegister(RegisterName.R0);
        Assert.Equal(testValue, r0Value);
    }

    [Fact]
    public void IncrementProgramCounter_DefaultIncrement_IncrementsBy2()
    {
        _registerFile.SetProgramCounter(0x8000);
        _registerFile.IncrementProgramCounter();

        ushort pcValue = _registerFile.GetProgramCounter();
        Assert.Equal((ushort)0x8002, pcValue);
    }

    [Fact]
    public void IncrementProgramCounter_CustomIncrement_IncrementsCorrectly()
    {
        _registerFile.SetProgramCounter(0x8000);
        _registerFile.IncrementProgramCounter(4);

        ushort pcValue = _registerFile.GetProgramCounter();
        Assert.Equal((ushort)0x8004, pcValue);
    }

    [Fact]
    public void GetStackPointer_ReturnsR1Value()
    {
        ushort testValue = 0x2FFE;
        _registerFile.WriteRegister(RegisterName.R1, testValue);

        ushort spValue = _registerFile.GetStackPointer();
        Assert.Equal(testValue, spValue);
    }

    [Fact]
    public void SetStackPointer_UpdatesR1()
    {
        ushort testValue = 0x2FFE;
        _registerFile.SetStackPointer(testValue);

        ushort r1Value = _registerFile.ReadRegister(RegisterName.R1);
        Assert.Equal(testValue, r1Value);
    }

    [Fact]
    public void Reset_ClearsAllRegisters()
    {
        // Set some test values
        _registerFile.WriteRegister(RegisterName.R0, 0x1234);
        _registerFile.WriteRegister(RegisterName.R4, 0x5678);
        _registerFile.WriteRegister(RegisterName.R15, 0x9ABC);

        // Reset
        _registerFile.Reset();

        // All registers should be zero
        for (int i = 0; i <= 15; i++)
        {
            ushort value = _registerFile.ReadRegister((RegisterName)i);
            Assert.Equal((ushort)0, value);
        }
    }

    [Theory]
    [InlineData(RegisterName.R0, true)]
    [InlineData(RegisterName.R15, true)]
    [InlineData((RegisterName)16, false)]
    [InlineData((RegisterName)255, false)]
    public void IsValidRegister_VariousInputs_ReturnsExpectedResult(RegisterName register, bool expected)
    {
        bool result = _registerFile.IsValidRegister(register);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ReadRegister_InvalidRegister_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _registerFile.ReadRegister((RegisterName)16));
    }

    [Fact]
    public void ReadRegister_InvalidRegister_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _registerFile.ReadRegister((RegisterName)16));
    }

    [Fact]
    public void ReadRegister_InvalidRegister_ExceptionMessageContainsExpectedText()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            _registerFile.ReadRegister((RegisterName)16));

        Assert.Contains("Invalid register", exception.Message);
    }

    [Fact]
    public void ReadRegister_InvalidRegister_ExceptionParameterNameIsCorrect()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            _registerFile.ReadRegister((RegisterName)16));

        Assert.Equal("register", exception.ParamName);
    }

    [Fact]
    public void WriteRegister_InvalidRegister_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _registerFile.WriteRegister((RegisterName)16, 0x1234));
    }

    [Fact]
    public void WriteRegister_InvalidRegister_ExceptionContainsCorrectMessage()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            _registerFile.WriteRegister((RegisterName)16, 0x1234));

        Assert.Contains("Invalid register", exception.Message);
    }

    [Fact]
    public void WriteRegister_InvalidRegister_ExceptionHasCorrectParameterName()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            _registerFile.WriteRegister((RegisterName)16, 0x1234));

        Assert.Equal("register", exception.ParamName);
    }

    [Fact]
    public void GetAllRegisters_ReturnsCorrectArrayLength()
    {
        // Set some test values
        _registerFile.WriteRegister(RegisterName.R0, 0x1000);
        _registerFile.WriteRegister(RegisterName.R4, 0x4000);
        _registerFile.WriteRegister(RegisterName.R15, 0xF000);

        ushort[] snapshot = _registerFile.GetAllRegisters();

        Assert.Equal(16, snapshot.Length);
    }

    [Fact]
    public void GetAllRegisters_ReturnsCorrectR0Value()
    {
        // Set some test values
        _registerFile.WriteRegister(RegisterName.R0, 0x1000);
        _registerFile.WriteRegister(RegisterName.R4, 0x4000);
        _registerFile.WriteRegister(RegisterName.R15, 0xF000);

        ushort[] snapshot = _registerFile.GetAllRegisters();

        Assert.Equal((ushort)0x1000, snapshot[0]);
    }

    [Fact]
    public void GetAllRegisters_ReturnsCorrectR4Value()
    {
        // Set some test values
        _registerFile.WriteRegister(RegisterName.R0, 0x1000);
        _registerFile.WriteRegister(RegisterName.R4, 0x4000);
        _registerFile.WriteRegister(RegisterName.R15, 0xF000);

        ushort[] snapshot = _registerFile.GetAllRegisters();

        Assert.Equal((ushort)0x4000, snapshot[4]);
    }

    [Fact]
    public void GetAllRegisters_ReturnsCorrectR15Value()
    {
        // Set some test values
        _registerFile.WriteRegister(RegisterName.R0, 0x1000);
        _registerFile.WriteRegister(RegisterName.R4, 0x4000);
        _registerFile.WriteRegister(RegisterName.R15, 0xF000);

        ushort[] snapshot = _registerFile.GetAllRegisters();

        Assert.Equal((ushort)0xF000, snapshot[15]);
    }

    [Fact]
    public void GetAllRegisters_ReturnsArrayCopy()
    {
        // Set some test values
        _registerFile.WriteRegister(RegisterName.R0, 0x1000);
        _registerFile.WriteRegister(RegisterName.R4, 0x4000);
        _registerFile.WriteRegister(RegisterName.R15, 0xF000);

        ushort[] snapshot = _registerFile.GetAllRegisters();

        // Verify it's a copy, not the original array
        snapshot[0] = 0x9999;
        Assert.NotEqual((ushort)0x9999, _registerFile.ReadRegister(RegisterName.R0));
    }

    [Fact]
    public void RegisterAccess_WithDebugLogging_LogsWriteOperation()
    {
        _logger.MinimumLevel = LogLevel.Debug;

        _registerFile.WriteRegister(RegisterName.R4, 0x1234);
        _registerFile.ReadRegister(RegisterName.R4);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug &&
            entry.Message.Contains("Register write: R4 = 0x1234"));
    }

    [Fact]
    public void RegisterAccess_WithDebugLogging_LogsReadOperation()
    {
        _logger.MinimumLevel = LogLevel.Debug;

        _registerFile.WriteRegister(RegisterName.R4, 0x1234);
        _registerFile.ReadRegister(RegisterName.R4);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug &&
            entry.Message.Contains("Register read: R4 = 0x1234"));
    }

    [Fact]
    public void ProgramCounterIncrement_WithDebugLogging_LogsOperation()
    {
        _logger.MinimumLevel = LogLevel.Debug;

        _registerFile.SetProgramCounter(0x8000);
        _registerFile.IncrementProgramCounter(4);

        Assert.Contains(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug &&
            entry.Message.Contains("Program Counter incremented by 4"));
    }

    [Fact]
    public void RegisterAccess_WithoutDebugLogging_DoesNotLog()
    {
        _logger.MinimumLevel = LogLevel.Info;

        _registerFile.WriteRegister(RegisterName.R4, 0x1234);
        _registerFile.ReadRegister(RegisterName.R4);

        Assert.DoesNotContain(_logger.LogEntries, entry =>
            entry.Level == LogLevel.Debug);
    }

    // Test helper class for logging
    private class TestLogger : ILogger
    {
        public List<LogEntry> LogEntries { get; } = new();
        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

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
        public void Fatal(string message) => Log(LogLevel.Fatal, message);
        public void Fatal(string message, object? context) => Log(LogLevel.Fatal, message, context);

        public bool IsEnabled(LogLevel level) => level >= MinimumLevel;
    }

    private record LogEntry(LogLevel Level, string Message, object? Context);
}
