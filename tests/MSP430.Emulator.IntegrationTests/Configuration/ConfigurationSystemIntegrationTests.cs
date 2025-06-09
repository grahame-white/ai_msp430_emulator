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
    public void ConfigurationSystem_DefaultConfig_CreatesValidLoggingConfiguration()
    {
        // Arrange & Act
        var config = EmulatorConfig.CreateDefault();

        // Assert - Logging configuration should not be null
        Assert.NotNull(config.Logging);
    }

    [Fact]
    public void ConfigurationSystem_DefaultConfig_CreatesValidMemoryConfiguration()
    {
        // Arrange & Act
        var config = EmulatorConfig.CreateDefault();

        // Assert - Memory configuration should not be null
        Assert.NotNull(config.Memory);
    }

    [Fact]
    public void ConfigurationSystem_DefaultConfig_CreatesValidCpuConfiguration()
    {
        // Arrange & Act
        var config = EmulatorConfig.CreateDefault();

        // Assert - CPU configuration should not be null
        Assert.NotNull(config.Cpu);
    }

    [Fact]
    public void ConfigurationSystem_DefaultLogging_HasInfoLevel()
    {
        // Arrange & Act
        var config = EmulatorConfig.CreateDefault();

        // Assert - Logging should have reasonable defaults
        Assert.Equal(LogLevel.Info, config.Logging.MinimumLevel);
    }

    [Fact]
    public void ConfigurationSystem_DefaultLogging_EnablesConsole()
    {
        // Arrange & Act
        var config = EmulatorConfig.CreateDefault();

        // Assert - Console logging should be enabled by default
        Assert.True(config.Logging.EnableConsole);
    }

    [Fact]
    public void ConfigurationSystem_DefaultMemory_HasPositiveTotalSize()
    {
        // Arrange & Act
        var config = EmulatorConfig.CreateDefault();

        // Assert - Memory configuration should be appropriate for MSP430FR2355
        // Note: This test validates current behavior - actual MSP430FR2355 has more complex memory layout
        Assert.True(config.Memory.TotalSize > 0);
    }

    [Fact]
    public void ConfigurationSystem_DefaultMemory_EnablesProtection()
    {
        // Arrange & Act
        var config = EmulatorConfig.CreateDefault();

        // Assert - Memory protection should be enabled by default
        Assert.True(config.Memory.EnableProtection);
    }

    [Fact]
    public void ConfigurationSystem_DefaultCpu_HasValidFrequency()
    {
        // Arrange & Act
        var config = EmulatorConfig.CreateDefault();

        // Assert - CPU should have valid frequency for MSP430FR2355
        Assert.True(config.Cpu.Frequency > 0);
    }

    [Fact]
    public void ConfigurationSystem_SaveAndLoad_PreservesLoggingMinimumLevel()
    {
        // Arrange
        var originalConfig = EmulatorConfig.CreateDefault();
        originalConfig.Logging.MinimumLevel = LogLevel.Debug;

        // Act - Save and reload
        originalConfig.SaveToFile(_testConfigPath);
        var loadedConfig = EmulatorConfig.LoadFromFile(_testConfigPath);

        // Assert - Logging minimum level should be preserved
        Assert.Equal(originalConfig.Logging.MinimumLevel, loadedConfig.Logging.MinimumLevel);
    }

    [Fact]
    public void ConfigurationSystem_SaveAndLoad_PreservesLoggingFileSettings()
    {
        // Arrange
        var originalConfig = EmulatorConfig.CreateDefault();
        originalConfig.Logging.EnableFile = true;
        originalConfig.Logging.FilePath = "test.log";

        // Act - Save and reload
        originalConfig.SaveToFile(_testConfigPath);
        var loadedConfig = EmulatorConfig.LoadFromFile(_testConfigPath);

        // Assert - Logging file settings should be preserved
        Assert.Equal(originalConfig.Logging.EnableFile, loadedConfig.Logging.EnableFile);
    }

    [Fact]
    public void ConfigurationSystem_SaveAndLoad_PreservesLoggingFilePath()
    {
        // Arrange
        var originalConfig = EmulatorConfig.CreateDefault();
        originalConfig.Logging.FilePath = "test.log";

        // Act - Save and reload
        originalConfig.SaveToFile(_testConfigPath);
        var loadedConfig = EmulatorConfig.LoadFromFile(_testConfigPath);

        // Assert - Logging file path should be preserved
        Assert.Equal(originalConfig.Logging.FilePath, loadedConfig.Logging.FilePath);
    }

    [Fact]
    public void ConfigurationSystem_SaveAndLoad_PreservesMemoryProtectionSetting()
    {
        // Arrange
        var originalConfig = EmulatorConfig.CreateDefault();
        originalConfig.Memory.EnableProtection = false;

        // Act - Save and reload
        originalConfig.SaveToFile(_testConfigPath);
        var loadedConfig = EmulatorConfig.LoadFromFile(_testConfigPath);

        // Assert - Memory protection setting should be preserved
        Assert.Equal(originalConfig.Memory.EnableProtection, loadedConfig.Memory.EnableProtection);
    }

    [Fact]
    public void ConfigurationSystem_SaveAndLoad_PreservesCpuTracingSetting()
    {
        // Arrange
        var originalConfig = EmulatorConfig.CreateDefault();
        originalConfig.Cpu.EnableTracing = true;

        // Act - Save and reload
        originalConfig.SaveToFile(_testConfigPath);
        var loadedConfig = EmulatorConfig.LoadFromFile(_testConfigPath);

        // Assert - CPU tracing setting should be preserved
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
    public void ConfigurationSystem_PartialConfiguration_MergesLoggingLevel()
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

        // Assert - Specified values should be set
        Assert.Equal(LogLevel.Warning, config.Logging.MinimumLevel);
    }

    [Fact]
    public void ConfigurationSystem_PartialConfiguration_MergesWithMemoryDefaults()
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

        // Assert - Memory should have default values since not specified
        Assert.NotNull(config.Memory);
    }

    [Fact]
    public void ConfigurationSystem_PartialConfiguration_MergesWithCpuDefaults()
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

        // Assert - CPU should have default values since not specified
        Assert.NotNull(config.Cpu);
    }
}
