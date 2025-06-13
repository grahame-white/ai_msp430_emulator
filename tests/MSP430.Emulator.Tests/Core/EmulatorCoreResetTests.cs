using System;
using System.Collections.Generic;
using MSP430.Emulator.Core;
using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;
using MSP430.Emulator.Tests.TestUtilities;

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
    private readonly EmulatorCoreTestFixture _fixture;

    public EmulatorCoreResetTests()
    {
        _fixture = new EmulatorCoreTestFixture();
    }

    [Fact]
    public void Reset_WithValidResetVector_LoadsPCFromResetVector()
    {
        // Arrange
        ushort expectedPCValue = 0x4000; // Valid program memory address
        SetResetVector(expectedPCValue);

        // Act
        _fixture.EmulatorCore.Reset();

        // Assert
        Assert.Equal(expectedPCValue, _fixture.RegisterFile.GetProgramCounter());
    }

    [Fact]
    public void Reset_WithUninitializedResetVector_LoadsPCToZero()
    {
        // Arrange - memory starts as all zeros, so reset vector will be 0x0000

        // Act
        _fixture.EmulatorCore.Reset();

        // Assert
        Assert.Equal((ushort)0x0000, _fixture.RegisterFile.GetProgramCounter());
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
        _fixture.EmulatorCore.Reset();

        // Assert
        Assert.Equal(resetVectorValue, _fixture.RegisterFile.GetProgramCounter());
    }

    [Fact]
    public void Reset_AfterLoadingResetVector_ClearsOtherRegisters()
    {
        // Arrange
        ushort resetVectorValue = 0x4000;
        SetResetVector(resetVectorValue);
        _fixture.RegisterFile.WriteRegister(RegisterName.R4, 0x1234);

        // Act
        _fixture.EmulatorCore.Reset();

        // Assert
        Assert.Equal((ushort)0x0000, _fixture.RegisterFile.ReadRegister(RegisterName.R4));
    }

    [Fact]
    public void Reset_AfterLoadingResetVector_ClearsAllGeneralPurposeRegisters()
    {
        // Arrange
        ushort resetVectorValue = 0x4000;
        SetResetVector(resetVectorValue);
        _fixture.RegisterFile.WriteRegister(RegisterName.R10, 0x5678);

        // Act
        _fixture.EmulatorCore.Reset();

        // Assert
        Assert.Equal((ushort)0x0000, _fixture.RegisterFile.ReadRegister(RegisterName.R10));
    }

    [Fact]
    public void Reset_WithResetVector_LogsResetVectorLoading()
    {
        // Arrange
        ushort resetVectorValue = 0x4000;
        SetResetVector(resetVectorValue);
        _fixture.Logger.MinimumLevel = LogLevel.Debug;

        // Act
        _fixture.EmulatorCore.Reset();

        // Assert
        bool hasResetVectorLog = _fixture.Logger.LogEntries.Exists(entry =>
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
        _fixture.RegisterFile.SetProgramCounter(0x1234);

        // Act
        _fixture.EmulatorCore.Reset();

        // Assert
        Assert.Equal(resetVectorValue, _fixture.RegisterFile.GetProgramCounter());
    }

    /// <summary>
    /// Helper method to set the reset vector in memory.
    /// The MSP430 uses little-endian format, so low byte at 0xFFFE, high byte at 0xFFFF.
    /// </summary>
    /// <param name="vectorAddress">The 16-bit address to store in the reset vector.</param>
    private void SetResetVector(ushort vectorAddress)
    {
        // Access memory through internal accessor instead of reflection
        byte[] memory = _fixture.EmulatorCore.Memory;

        // MSP430 is little-endian: low byte at 0xFFFE, high byte at 0xFFFF
        memory[0xFFFE] = (byte)(vectorAddress & 0xFF);
        memory[0xFFFF] = (byte)((vectorAddress >> 8) & 0xFF);
    }
}
