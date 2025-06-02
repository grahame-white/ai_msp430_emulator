using MSP430.Emulator.Configuration;
using MSP430.Emulator.Logging;
using System.Text.Json;

namespace MSP430.Emulator.Tests.Configuration;

public class EmulatorConfigTests : IDisposable
{
    private readonly string _testConfigPath;

    public EmulatorConfigTests()
    {
        _testConfigPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }

    [Fact]
    public void CreateDefault_ReturnsConfigWithDefaultValues()
    {
        var config = EmulatorConfig.CreateDefault();
        
        Assert.NotNull(config);
        Assert.NotNull(config.Logging);
        Assert.NotNull(config.Memory);
        Assert.NotNull(config.Cpu);
    }

    [Fact]
    public void DefaultLoggingConfig_HasExpectedValues()
    {
        var config = EmulatorConfig.CreateDefault();
        
        Assert.Equal(LogLevel.Info, config.Logging.MinimumLevel);
        Assert.True(config.Logging.EnableConsole);
        Assert.False(config.Logging.EnableFile);
        Assert.Equal("msp430_emulator.log", config.Logging.FilePath);
    }

    [Fact]
    public void DefaultMemoryConfig_HasExpectedValues()
    {
        var config = EmulatorConfig.CreateDefault();
        
        Assert.Equal(65536, config.Memory.TotalSize);
        Assert.True(config.Memory.EnableProtection);
    }

    [Fact]
    public void DefaultCpuConfig_HasExpectedValues()
    {
        var config = EmulatorConfig.CreateDefault();
        
        Assert.Equal(1000000, config.Cpu.Frequency);
        Assert.False(config.Cpu.EnableTracing);
    }

    [Fact]
    public void ToJson_ReturnsValidJson()
    {
        var config = EmulatorConfig.CreateDefault();
        var json = config.ToJson();
        
        Assert.NotEmpty(json);
        Assert.True(IsValidJson(json));
    }

    [Fact]
    public void LoadFromJson_WithValidJson_ReturnsCorrectConfig()
    {
        var json = """
        {
            "logging": {
                "minimumLevel": "Debug",
                "enableConsole": false,
                "enableFile": true,
                "filePath": "custom.log"
            },
            "memory": {
                "totalSize": 32768,
                "enableProtection": false
            },
            "cpu": {
                "frequency": 2000000,
                "enableTracing": true
            }
        }
        """;
        
        var config = EmulatorConfig.LoadFromJson(json);
        
        Assert.Equal(LogLevel.Debug, config.Logging.MinimumLevel);
        Assert.False(config.Logging.EnableConsole);
        Assert.True(config.Logging.EnableFile);
        Assert.Equal("custom.log", config.Logging.FilePath);
        Assert.Equal(32768, config.Memory.TotalSize);
        Assert.False(config.Memory.EnableProtection);
        Assert.Equal(2000000, config.Cpu.Frequency);
        Assert.True(config.Cpu.EnableTracing);
    }

    [Fact]
    public void LoadFromJson_WithEmptyJson_ReturnsDefaultConfig()
    {
        var config = EmulatorConfig.LoadFromJson("{}");
        
        Assert.Equal(LogLevel.Info, config.Logging.MinimumLevel);
        Assert.True(config.Logging.EnableConsole);
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
        
        var content = File.ReadAllText(_testConfigPath);
        Assert.True(IsValidJson(content));
    }

    [Fact]
    public void LoadFromFile_WithValidFile_ReturnsCorrectConfig()
    {
        var originalConfig = EmulatorConfig.CreateDefault();
        originalConfig.Logging.MinimumLevel = LogLevel.Debug;
        originalConfig.Memory.TotalSize = 32768;
        originalConfig.SaveToFile(_testConfigPath);
        
        var loadedConfig = EmulatorConfig.LoadFromFile(_testConfigPath);
        
        Assert.Equal(LogLevel.Debug, loadedConfig.Logging.MinimumLevel);
        Assert.Equal(32768, loadedConfig.Memory.TotalSize);
    }

    [Fact]
    public void LoadFromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => 
            EmulatorConfig.LoadFromFile("nonexistent.json"));
    }

    [Fact]
    public void SaveToFile_CreatesDirectoryIfNotExists()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"testdir_{Guid.NewGuid()}");
        var testFile = Path.Combine(testDir, "config.json");
        
        try
        {
            var config = EmulatorConfig.CreateDefault();
            config.SaveToFile(testFile);
            
            Assert.True(Directory.Exists(testDir));
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

    [Fact]
    public void LoggingConfig_PropertiesCanBeSet()
    {
        var config = new LoggingConfig
        {
            MinimumLevel = LogLevel.Error,
            EnableConsole = false,
            EnableFile = true,
            FilePath = "test.log"
        };
        
        Assert.Equal(LogLevel.Error, config.MinimumLevel);
        Assert.False(config.EnableConsole);
        Assert.True(config.EnableFile);
        Assert.Equal("test.log", config.FilePath);
    }

    [Fact]
    public void MemoryConfig_PropertiesCanBeSet()
    {
        var config = new MemoryConfig
        {
            TotalSize = 128000,
            EnableProtection = false
        };
        
        Assert.Equal(128000, config.TotalSize);
        Assert.False(config.EnableProtection);
    }

    [Fact]
    public void CpuConfig_PropertiesCanBeSet()
    {
        var config = new CpuConfig
        {
            Frequency = 8000000,
            EnableTracing = true
        };
        
        Assert.Equal(8000000, config.Frequency);
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