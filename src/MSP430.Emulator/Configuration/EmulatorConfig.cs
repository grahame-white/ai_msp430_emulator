using System.Text.Json;
using System.Text.Json.Serialization;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Configuration;

/// <summary>
/// Configuration settings for the MSP430 emulator.
/// </summary>
public class EmulatorConfig
{
    /// <summary>
    /// Gets or sets the logging configuration.
    /// </summary>
    public LoggingConfig Logging { get; set; } = new LoggingConfig();

    /// <summary>
    /// Gets or sets the memory configuration.
    /// </summary>
    public MemoryConfig Memory { get; set; } = new MemoryConfig();

    /// <summary>
    /// Gets or sets the CPU configuration.
    /// </summary>
    public CpuConfig Cpu { get; set; } = new CpuConfig();

    /// <summary>
    /// Creates a default EmulatorConfig instance.
    /// </summary>
    /// <returns>A new EmulatorConfig with default values.</returns>
    public static EmulatorConfig CreateDefault()
    {
        return new EmulatorConfig();
    }

    /// <summary>
    /// Loads configuration from a JSON file.
    /// </summary>
    /// <param name="filePath">The path to the JSON configuration file.</param>
    /// <returns>The loaded EmulatorConfig instance.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file is not found.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static EmulatorConfig LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        string json = File.ReadAllText(filePath);
        return LoadFromJson(json);
    }

    /// <summary>
    /// Loads configuration from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string containing configuration data.</param>
    /// <returns>The loaded EmulatorConfig instance.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static EmulatorConfig LoadFromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Deserialize<EmulatorConfig>(json, options) ?? CreateDefault();
    }

    /// <summary>
    /// Saves the configuration to a JSON file.
    /// </summary>
    /// <param name="filePath">The path to save the configuration file.</param>
    public void SaveToFile(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = ToJson();
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Converts the configuration to a JSON string.
    /// </summary>
    /// <returns>A JSON representation of the configuration.</returns>
    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Serialize(this, options);
    }
}

/// <summary>
/// Configuration settings for logging.
/// </summary>
public class LoggingConfig
{
    /// <summary>
    /// Gets or sets the minimum log level.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Gets or sets whether to enable console logging.
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable file logging.
    /// </summary>
    public bool EnableFile { get; set; } = false;

    /// <summary>
    /// Gets or sets the file path for file logging.
    /// </summary>
    public string? FilePath { get; set; } = "msp430_emulator.log";
}

/// <summary>
/// Configuration settings for memory.
/// </summary>
public class MemoryConfig
{
    /// <summary>
    /// Gets or sets the total memory size in bytes.
    /// </summary>
    public int TotalSize { get; set; } = 65536; // 64KB

    /// <summary>
    /// Gets or sets whether to enable memory protection.
    /// </summary>
    public bool EnableProtection { get; set; } = true;
}

/// <summary>
/// Configuration settings for the CPU.
/// </summary>
public class CpuConfig
{
    /// <summary>
    /// Gets or sets the CPU frequency in Hz.
    /// </summary>
    public int Frequency { get; set; } = 1000000; // 1MHz

    /// <summary>
    /// Gets or sets whether to enable instruction tracing.
    /// </summary>
    public bool EnableTracing { get; set; } = false;
}
