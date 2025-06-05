using System.Diagnostics;

namespace MSP430.Emulator.Core;

/// <summary>
/// Provides execution statistics and performance monitoring for the MSP430 emulator core.
/// 
/// This class tracks various performance metrics during emulator execution including
/// instruction counts, execution time, and cycle counts.
/// </summary>
public class ExecutionStatistics
{
    private readonly Stopwatch _executionTimer;
    private ulong _instructionsExecuted;
    private ulong _totalCycles;
    private DateTime _startTime;
    private DateTime _lastResetTime;

    /// <summary>
    /// Initializes a new instance of the ExecutionStatistics class.
    /// </summary>
    public ExecutionStatistics()
    {
        _executionTimer = new Stopwatch();
        _startTime = DateTime.UtcNow;
        _lastResetTime = DateTime.UtcNow;
        Reset();
    }

    /// <summary>
    /// Gets the total number of instructions executed since the last reset.
    /// </summary>
    public ulong InstructionsExecuted => _instructionsExecuted;

    /// <summary>
    /// Gets the total number of CPU cycles consumed since the last reset.
    /// </summary>
    public ulong TotalCycles => _totalCycles;

    /// <summary>
    /// Gets the total execution time since the last reset.
    /// </summary>
    public TimeSpan ExecutionTime => _executionTimer.Elapsed;

    /// <summary>
    /// Gets the time when statistics collection started.
    /// </summary>
    public DateTime StartTime => _startTime;

    /// <summary>
    /// Gets the time when statistics were last reset.
    /// </summary>
    public DateTime LastResetTime => _lastResetTime;

    /// <summary>
    /// Gets the average instructions per second since the last reset.
    /// </summary>
    public double InstructionsPerSecond
    {
        get
        {
            double totalSeconds = ExecutionTime.TotalSeconds;
            return totalSeconds > 0 ? _instructionsExecuted / totalSeconds : 0;
        }
    }

    /// <summary>
    /// Gets the average cycles per second since the last reset.
    /// </summary>
    public double CyclesPerSecond
    {
        get
        {
            double totalSeconds = ExecutionTime.TotalSeconds;
            return totalSeconds > 0 ? _totalCycles / totalSeconds : 0;
        }
    }

    /// <summary>
    /// Gets the average cycles per instruction since the last reset.
    /// </summary>
    public double CyclesPerInstruction => _instructionsExecuted > 0 ? (double)_totalCycles / _instructionsExecuted : 0;

    /// <summary>
    /// Starts the execution timer if it's not already running.
    /// </summary>
    public void StartTimer()
    {
        if (!_executionTimer.IsRunning)
        {
            _executionTimer.Start();
        }
    }

    /// <summary>
    /// Stops the execution timer.
    /// </summary>
    public void StopTimer()
    {
        _executionTimer.Stop();
    }

    /// <summary>
    /// Records the execution of an instruction.
    /// </summary>
    /// <param name="cycles">The number of CPU cycles consumed by the instruction.</param>
    public void RecordInstruction(uint cycles = 1)
    {
        _instructionsExecuted++;
        _totalCycles += cycles;
    }

    /// <summary>
    /// Records additional CPU cycles without counting as an instruction.
    /// </summary>
    /// <param name="cycles">The number of CPU cycles to add.</param>
    public void RecordCycles(uint cycles)
    {
        _totalCycles += cycles;
    }

    /// <summary>
    /// Resets all statistics to their initial values.
    /// </summary>
    public void Reset()
    {
        _executionTimer.Reset();
        _instructionsExecuted = 0;
        _totalCycles = 0;
        _lastResetTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets a summary of the current execution statistics.
    /// </summary>
    /// <returns>A formatted string containing the statistics summary.</returns>
    public override string ToString()
    {
        return $"Instructions: {InstructionsExecuted:N0}, " +
               $"Cycles: {TotalCycles:N0}, " +
               $"Time: {ExecutionTime.TotalMilliseconds:F2}ms, " +
               $"IPS: {InstructionsPerSecond:F0}, " +
               $"CPI: {CyclesPerInstruction:F2}";
    }

    /// <summary>
    /// Gets detailed execution statistics as a dictionary.
    /// </summary>
    /// <returns>A dictionary containing all available statistics.</returns>
    public Dictionary<string, object> GetDetailedStatistics()
    {
        return new Dictionary<string, object>
        {
            { "InstructionsExecuted", InstructionsExecuted },
            { "TotalCycles", TotalCycles },
            { "ExecutionTimeMs", ExecutionTime.TotalMilliseconds },
            { "InstructionsPerSecond", InstructionsPerSecond },
            { "CyclesPerSecond", CyclesPerSecond },
            { "CyclesPerInstruction", CyclesPerInstruction },
            { "StartTime", StartTime },
            { "LastResetTime", LastResetTime },
            { "IsTimerRunning", _executionTimer.IsRunning }
        };
    }
}
