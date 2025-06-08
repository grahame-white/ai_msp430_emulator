using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MSP430.Emulator.Configuration;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Tests.Configuration;

/// <summary>
/// Tests for EmulatorConfig class with MSP430FR2355-specific configuration values.
/// 
/// Configuration values used in tests:
/// - Memory TotalSize: 65536 bytes (64KB) - Full 16-bit address space for MSP430FR2355
/// - CPU Frequency: 1000000 Hz (1 MHz) - Default frequency (SLASEC4D Section 5.3 verification pending)
/// 
/// MSP430FR2355 Memory Layout Reference:
/// - Total addressable space: 64KB (0x0000-0xFFFF)
/// - FRAM: 32KB (0x4000-0xBFFF) 
/// - SRAM: 4KB (0x2000-0x2FFF)
/// - Peripherals and other regions: remaining space
/// 
/// References:
/// - MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers (SLASEC4D)
/// - MSP430FR2xx FR4xx Family User's Guide (SLAU445I)
/// </summary>
public class EmulatorConfigTests : IDisposable
{
    private readonly string _testConfigPath;

    public EmulatorConfigTests()
    {
        _testConfigPath = Path.Join(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }

    [Fact]
    public void CreateDefault_ReturnsNonNullConfig()
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.NotNull(config);
    }

    [Fact]
    public void CreateDefault_ReturnsConfigWithLogging()
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.NotNull(config.Logging);
    }

    [Fact]
    public void CreateDefault_ReturnsConfigWithMemory()
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.NotNull(config.Memory);
    }

    [Fact]
    public void CreateDefault_ReturnsConfigWithCpu()
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.NotNull(config.Cpu);
    }

    [Fact]
    public void DefaultLoggingConfig_SetsMinimumLevel()
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.Equal(LogLevel.Info, config.Logging.MinimumLevel);
    }

    [Fact]
    public void DefaultLoggingConfig_EnablesConsole()
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.True(config.Logging.EnableConsole);
    }

    [Fact]
    public void DefaultLoggingConfig_DisablesFile()
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.False(config.Logging.EnableFile);
    }

    [Theory]
    [InlineData("msp430_emulator.log")]
    public void DefaultLoggingConfig_SetsFilePath(string expectedPath)
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.Equal(expectedPath, config.Logging.FilePath);
    }

    [Theory]
    [InlineData(65536)]
    public void DefaultMemoryConfig_SetsTotalSize(int expectedSize)
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.Equal(expectedSize, config.Memory.TotalSize);
    }

    [Fact]
    public void DefaultMemoryConfig_EnablesProtection()
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.True(config.Memory.EnableProtection);
    }

    [Theory]
    [InlineData(1000000)]
    public void DefaultCpuConfig_SetsFrequency(int expectedFrequency)
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.Equal(expectedFrequency, config.Cpu.Frequency);
    }

    [Fact]
    public void DefaultCpuConfig_DisablesTracing()
    {
        var config = EmulatorConfig.CreateDefault();

        Assert.False(config.Cpu.EnableTracing);
    }

    [Fact]
    public void ToJson_ReturnsNonEmptyString()
    {
        var config = EmulatorConfig.CreateDefault();
        string json = config.ToJson();

        Assert.NotEmpty(json);
    }

    [Fact]
    public void ToJson_ReturnsValidJson()
    {
        var config = EmulatorConfig.CreateDefault();
        string json = config.ToJson();

        Assert.True(IsValidJson(json));
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    public void LoadFromJson_WithValidJson_SetsLoggingMinimumLevel(LogLevel expectedLevel)
    {
        string json = """
        {
            "logging": {
                "minimumLevel": "Debug",
                "enableConsole": false,
                "enableFile": true,
                "filePath": "custom.log"
            },
            "memory": {
                "totalSize": 65536,
                "enableProtection": false
            },
            "cpu": {
                "frequency": 1000000,
                "enableTracing": true
            }
        }
        """;

        var config = EmulatorConfig.LoadFromJson(json);

        Assert.Equal(expectedLevel, config.Logging.MinimumLevel);
    }

    [Fact]
    public void LoadFromJson_WithValidJson_SetsLoggingEnableConsole()
    {
        string json = """
        {
            "logging": {
                "minimumLevel": "Debug",
                "enableConsole": false,
                "enableFile": true,
                "filePath": "custom.log"
            },
            "memory": {
                "totalSize": 65536,
                "enableProtection": false
            },
            "cpu": {
                "frequency": 1000000,
                "enableTracing": true
            }
        }
        """;

        var config = EmulatorConfig.LoadFromJson(json);

        Assert.False(config.Logging.EnableConsole);
    }

    [Fact]
    public void LoadFromJson_WithValidJson_SetsLoggingEnableFile()
    {
        string json = """
        {
            "logging": {
                "minimumLevel": "Debug",
                "enableConsole": false,
                "enableFile": true,
                "filePath": "custom.log"
            },
            "memory": {
                "totalSize": 65536,
                "enableProtection": false
            },
            "cpu": {
                "frequency": 1000000,
                "enableTracing": true
            }
        }
        """;

        var config = EmulatorConfig.LoadFromJson(json);

        Assert.True(config.Logging.EnableFile);
    }

    [Theory]
    [InlineData("custom.log")]
    public void LoadFromJson_WithValidJson_SetsLoggingFilePath(string expectedPath)
    {
        string json = """
        {
            "logging": {
                "minimumLevel": "Debug",
                "enableConsole": false,
                "enableFile": true,
                "filePath": "custom.log"
            },
            "memory": {
                "totalSize": 65536,
                "enableProtection": false
            },
            "cpu": {
                "frequency": 1000000,
                "enableTracing": true
            }
        }
        """;

        var config = EmulatorConfig.LoadFromJson(json);

        Assert.Equal(expectedPath, config.Logging.FilePath);
    }

    [Theory]
    [InlineData(65536)]
    public void LoadFromJson_WithValidJson_SetsMemoryTotalSize(int expectedSize)
    {
        string json = """
        {
            "logging": {
                "minimumLevel": "Debug",
                "enableConsole": false,
                "enableFile": true,
                "filePath": "custom.log"
            },
            "memory": {
                "totalSize": 65536,
                "enableProtection": false
            },
            "cpu": {
                "frequency": 1000000,
                "enableTracing": true
            }
        }
        """;

        var config = EmulatorConfig.LoadFromJson(json);

        Assert.Equal(expectedSize, config.Memory.TotalSize);
    }

    [Fact]
    public void LoadFromJson_WithValidJson_SetsMemoryEnableProtection()
    {
        string json = """
        {
            "logging": {
                "minimumLevel": "Debug",
                "enableConsole": false,
                "enableFile": true,
                "filePath": "custom.log"
            },
            "memory": {
                "totalSize": 65536,
                "enableProtection": false
            },
            "cpu": {
                "frequency": 1000000,
                "enableTracing": true
            }
        }
        """;

        var config = EmulatorConfig.LoadFromJson(json);

        Assert.False(config.Memory.EnableProtection);
    }

    [Theory]
    [InlineData(1000000)]
    public void LoadFromJson_WithValidJson_SetsCpuFrequency(int expectedFrequency)
    {
        string json = """
        {
            "logging": {
                "minimumLevel": "Debug",
                "enableConsole": false,
                "enableFile": true,
                "filePath": "custom.log"
            },
            "memory": {
                "totalSize": 65536,
                "enableProtection": false
            },
            "cpu": {
                "frequency": 1000000,
                "enableTracing": true
            }
        }
        """;

        var config = EmulatorConfig.LoadFromJson(json);

        Assert.Equal(expectedFrequency, config.Cpu.Frequency);
    }

    [Fact]
    public void LoadFromJson_WithValidJson_SetsCpuEnableTracing()
    {
        string json = """
        {
            "logging": {
                "minimumLevel": "Debug",
                "enableConsole": false,
                "enableFile": true,
                "filePath": "custom.log"
            },
            "memory": {
                "totalSize": 65536,
                "enableProtection": false
            },
            "cpu": {
                "frequency": 1000000,
                "enableTracing": true
            }
        }
        """;

        var config = EmulatorConfig.LoadFromJson(json);

        Assert.True(config.Cpu.EnableTracing);
    }

    [Theory]
    [InlineData(LogLevel.Info)]
    public void LoadFromJson_WithEmptyJson_SetsDefaultLoggingMinimumLevel(LogLevel expectedLevel)
    {
        var config = EmulatorConfig.LoadFromJson("{}");

        Assert.Equal(expectedLevel, config.Logging.MinimumLevel);
    }

    [Fact]
    public void LoadFromJson_WithEmptyJson_EnablesConsoleByDefault()
    {
        var config = EmulatorConfig.LoadFromJson("{}");

        Assert.True(config.Logging.EnableConsole);
    }

    [Fact]
    public void LoadFromJson_WithEmptyJson_DisablesFileByDefault()
    {
        var config = EmulatorConfig.LoadFromJson("{}");

        Assert.False(config.Logging.EnableFile);
    }

    [Fact]
    public void LoadFromJson_WithInvalidJson_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => EmulatorConfig.LoadFromJson("invalid json"));
    }

    [Fact]
    public void SaveToFile_CreatesFile()
    {
        var config = EmulatorConfig.CreateDefault();
        config.SaveToFile(_testConfigPath);

        Assert.True(File.Exists(_testConfigPath));
    }

    [Fact]
    public void SaveToFile_CreatesValidJson()
    {
        var config = EmulatorConfig.CreateDefault();
        config.SaveToFile(_testConfigPath);

        string content = File.ReadAllText(_testConfigPath);
        Assert.True(IsValidJson(content));
    }

    [Fact]
    public void LoadFromFile_WithValidFile_ReturnsCorrectLoggingMinimumLevel()
    {
        var originalConfig = EmulatorConfig.CreateDefault();
        originalConfig.Logging.MinimumLevel = LogLevel.Debug;
        originalConfig.Memory.TotalSize = 65536;
        originalConfig.SaveToFile(_testConfigPath);

        var loadedConfig = EmulatorConfig.LoadFromFile(_testConfigPath);

        Assert.Equal(LogLevel.Debug, loadedConfig.Logging.MinimumLevel);
    }

    [Fact]
    public void LoadFromFile_WithValidFile_ReturnsCorrectMemoryTotalSize()
    {
        var originalConfig = EmulatorConfig.CreateDefault();
        originalConfig.Logging.MinimumLevel = LogLevel.Debug;
        originalConfig.Memory.TotalSize = 65536;
        originalConfig.SaveToFile(_testConfigPath);

        var loadedConfig = EmulatorConfig.LoadFromFile(_testConfigPath);

        Assert.Equal(65536, loadedConfig.Memory.TotalSize);
    }

    [Fact]
    public void LoadFromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() =>
            EmulatorConfig.LoadFromFile("nonexistent.json"));
    }

    [Fact]
    public void SaveToFile_CreatesDirectory()
    {
        string testDir = Path.Join(Path.GetTempPath(), $"testdir_{Guid.NewGuid()}");
        string testFile = Path.Join(testDir, "config.json");

        try
        {
            var config = EmulatorConfig.CreateDefault();
            config.SaveToFile(testFile);

            Assert.True(Directory.Exists(testDir));
        }
        finally
        {
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    public void SaveToFile_CreatesFileInNewDirectory()
    {
        string testDir = Path.Join(Path.GetTempPath(), $"testdir_{Guid.NewGuid()}");
        string testFile = Path.Join(testDir, "config.json");

        try
        {
            var config = EmulatorConfig.CreateDefault();
            config.SaveToFile(testFile);

            Assert.True(File.Exists(testFile));
        }
        finally
        {
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Theory]
    [InlineData(LogLevel.Error)]
    public void LoggingConfig_CanSetMinimumLevel(LogLevel expectedLevel)
    {
        var config = new LoggingConfig
        {
            MinimumLevel = LogLevel.Error,
            EnableConsole = false,
            EnableFile = true,
            FilePath = "test.log"
        };

        Assert.Equal(expectedLevel, config.MinimumLevel);
    }

    [Fact]
    public void LoggingConfig_CanSetEnableConsole()
    {
        var config = new LoggingConfig
        {
            MinimumLevel = LogLevel.Error,
            EnableConsole = false,
            EnableFile = true,
            FilePath = "test.log"
        };

        Assert.False(config.EnableConsole);
    }

    [Fact]
    public void LoggingConfig_CanSetEnableFile()
    {
        var config = new LoggingConfig
        {
            MinimumLevel = LogLevel.Error,
            EnableConsole = false,
            EnableFile = true,
            FilePath = "test.log"
        };

        Assert.True(config.EnableFile);
    }

    [Theory]
    [InlineData("test.log")]
    public void LoggingConfig_CanSetFilePath(string expectedPath)
    {
        var config = new LoggingConfig
        {
            MinimumLevel = LogLevel.Error,
            EnableConsole = false,
            EnableFile = true,
            FilePath = "test.log"
        };

        Assert.Equal(expectedPath, config.FilePath);
    }

    [Theory]
    [InlineData(128000)]
    public void MemoryConfig_CanSetTotalSize(int expectedSize)
    {
        var config = new MemoryConfig
        {
            TotalSize = 128000,
            EnableProtection = false
        };

        Assert.Equal(expectedSize, config.TotalSize);
    }

    [Fact]
    public void MemoryConfig_CanSetEnableProtection()
    {
        var config = new MemoryConfig
        {
            TotalSize = 128000,
            EnableProtection = false
        };

        Assert.False(config.EnableProtection);
    }

    [Theory]
    [InlineData(8000000)]
    public void CpuConfig_CanSetFrequency(int expectedFrequency)
    {
        var config = new CpuConfig
        {
            Frequency = 8000000,
            EnableTracing = true
        };

        Assert.Equal(expectedFrequency, config.Frequency);
    }

    [Fact]
    public void CpuConfig_CanSetEnableTracing()
    {
        var config = new CpuConfig
        {
            Frequency = 8000000,
            EnableTracing = true
        };

        Assert.True(config.EnableTracing);
    }

    private static bool IsValidJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
