namespace MSP430.Emulator.Logging;

/// <summary>
/// Represents the severity level of a log message.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Debug level - detailed information for diagnosing problems.
    /// </summary>
    Debug = 0,

    /// <summary>
    /// Info level - general information about program execution.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning level - potentially harmful situations.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error level - error events that might still allow the application to continue.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Fatal level - very severe error events that will presumably lead the application to abort.
    /// </summary>
    Fatal = 4
}