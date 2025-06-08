using System;
using System.IO;
using MSP430.Emulator.Configuration;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.IntegrationTests.Configuration;

/// <summary>
/// Integration tests for configuration loading and validation.
/// Tests that configuration values align with MSP430FR2355 specifications and
/// that the configuration system works end-to-end.
/// 
/// References:
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D)
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I)
/// </summary>
public class ConfigurationSystemIntegrationTests : IDisposable
{
    private readonly string _testConfigPath;

    public ConfigurationSystemIntegrationTests()
    {
        _testConfigPath = Path.GetTempFileName();
    }

    public void Dispose()
    {
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }

    [Fact]
    public void ConfigurationSystem_DefaultConfig_CreatesValidMSP430FR2355Settings()
    {
        // Arrange & Act
        var config = EmulatorConfig.CreateDefault();

        // Assert - Default configuration should be valid for MSP430FR2355
        Assert.NotNull(config.Logging);
        Assert.NotNull(config.Memory);
        Assert.NotNull(config.Cpu);

        // Logging should have reasonable defaults
        Assert.Equal(LogLevel.Info, config.Logging.MinimumLevel);
        Assert.True(config.Logging.EnableConsole);

        // Memory configuration should be appropriate for MSP430FR2355
        // Note: This test validates current behavior - actual MSP430FR2355 has more complex memory layout
        Assert.True(config.Memory.TotalSize > 0);
        Assert.True(config.Memory.EnableProtection);

        // CPU should have valid frequency for MSP430FR2355
        Assert.True(config.Cpu.Frequency > 0);
    }

    [Fact]
    public void ConfigurationSystem_SaveAndLoad_RoundTripPreservesData()
    {
        // Arrange
        var originalConfig = EmulatorConfig.CreateDefault();
        originalConfig.Logging.MinimumLevel = LogLevel.Debug;
        originalConfig.Logging.EnableFile = true;
        originalConfig.Logging.FilePath = "test.log";
        originalConfig.Memory.EnableProtection = false;
        originalConfig.Cpu.EnableTracing = true;

        // Act - Save and reload
        originalConfig.SaveToFile(_testConfigPath);
        var loadedConfig = EmulatorConfig.LoadFromFile(_testConfigPath);

        // Assert - All values should be preserved
        Assert.Equal(originalConfig.Logging.MinimumLevel, loadedConfig.Logging.MinimumLevel);
        Assert.Equal(originalConfig.Logging.EnableFile, loadedConfig.Logging.EnableFile);
        Assert.Equal(originalConfig.Logging.FilePath, loadedConfig.Logging.FilePath);
        Assert.Equal(originalConfig.Memory.EnableProtection, loadedConfig.Memory.EnableProtection);
        Assert.Equal(originalConfig.Cpu.EnableTracing, loadedConfig.Cpu.EnableTracing);
    }

    [Fact]
    public void ConfigurationSystem_InvalidJson_ThrowsAppropriateException()
    {
        // Arrange
        string invalidJson = "{ invalid json syntax }";
        File.WriteAllText(_testConfigPath, invalidJson);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => EmulatorConfig.LoadFromFile(_testConfigPath));
    }

    [Fact]
    public void ConfigurationSystem_PartialConfiguration_MergesWithDefaults()
    {
        // Arrange - Only specify logging configuration
        string partialJson = """
        {
            "logging": {
                "minimumLevel": "Warning"
            }
        }
        """;

        // Act
        var config = EmulatorConfig.LoadFromJson(partialJson);

        // Assert - Specified values should be set, others should be defaults
        Assert.Equal(LogLevel.Warning, config.Logging.MinimumLevel);

        // Memory and CPU should have default values since not specified
        Assert.NotNull(config.Memory);
        Assert.NotNull(config.Cpu);
    }
}
