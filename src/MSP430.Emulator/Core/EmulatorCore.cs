using MSP430.Emulator.Cpu;
using MSP430.Emulator.Instructions;
using MSP430.Emulator.Logging;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Core;

/// <summary>
/// The main emulator core engine that coordinates CPU execution, memory access, and system state.
/// 
/// This class provides the primary interface for controlling MSP430 emulator execution,
/// handling instruction fetch, decode, execute cycles, and managing system state.
/// </summary>
public class EmulatorCore : IEmulatorCore
{
    private readonly IRegisterFile _registerFile;
    private readonly IMemoryMap _memoryMap;
    private readonly IInstructionDecoder _instructionDecoder;
    private readonly MemoryAccessValidator _memoryValidator;
    private readonly ExecutionStateManager _stateManager;
    private readonly ExecutionStatistics _statistics;
    private readonly HashSet<ushort> _breakpoints;
    private readonly ILogger? _logger;

    // Simple memory storage for basic emulation - will be replaced with proper memory system
    private readonly byte[] _memory;
    private const int MemorySize = 0x10000; // 64KB address space

    /// <summary>
    /// Initializes a new instance of the EmulatorCore class.
    /// </summary>
    /// <param name="registerFile">The CPU register file.</param>
    /// <param name="memoryMap">The memory map for address validation.</param>
    /// <param name="instructionDecoder">The instruction decoder.</param>
    /// <param name="logger">Optional logger for debugging and diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public EmulatorCore(
        IRegisterFile registerFile,
        IMemoryMap memoryMap,
        IInstructionDecoder instructionDecoder,
        ILogger? logger = null)
    {
        _registerFile = registerFile ?? throw new ArgumentNullException(nameof(registerFile));
        _memoryMap = memoryMap ?? throw new ArgumentNullException(nameof(memoryMap));
        _instructionDecoder = instructionDecoder ?? throw new ArgumentNullException(nameof(instructionDecoder));
        _logger = logger;

        _memoryValidator = new MemoryAccessValidator(_memoryMap, _logger);
        _stateManager = new ExecutionStateManager();
        _statistics = new ExecutionStatistics();
        _breakpoints = new HashSet<ushort>();
        _memory = new byte[MemorySize];

        _logger?.Debug("EmulatorCore initialized successfully");
    }

    /// <inheritdoc />
    public ExecutionState State => _stateManager.CurrentState;

    /// <inheritdoc />
    public ExecutionStatistics Statistics => _statistics;

    /// <inheritdoc />
    public bool IsRunning => State == ExecutionState.Running;

    /// <inheritdoc />
    public bool IsHalted => State == ExecutionState.Halted;

    /// <inheritdoc />
    public IReadOnlySet<ushort> Breakpoints => _breakpoints;

    /// <inheritdoc />
    public event EventHandler<ExecutionStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <inheritdoc />
    public event EventHandler<InstructionExecutedEventArgs>? InstructionExecuted;

    /// <inheritdoc />
    public void Reset()
    {
        _logger?.Info("Resetting emulator core");

        ExecutionState previousState = _stateManager.CurrentState;

        // Reset CPU registers
        _registerFile.Reset();

        // Reset execution statistics
        _statistics.Reset();

        // Clear memory (optional - real hardware doesn't clear RAM on reset)
        Array.Clear(_memory, 0, _memory.Length);

        // Set execution state to reset
        _stateManager.Reset();

        OnStateChanged(previousState, ExecutionState.Reset);

        _logger?.Info("Emulator core reset complete");
    }

    /// <inheritdoc />
    public uint Step()
    {
        if (!_stateManager.CanExecute() && !_stateManager.CanResume() && State != ExecutionState.Reset)
        {
            throw new InvalidOperationException($"Cannot execute in current state: {State}");
        }

        _logger?.Debug("Executing single instruction step");

        ExecutionState previousState = _stateManager.CurrentState;

        // Change to single step state if not already
        if (State != ExecutionState.SingleStep)
        {
            _stateManager.TransitionTo(ExecutionState.SingleStep);
            OnStateChanged(previousState, ExecutionState.SingleStep);
        }

        return ExecuteInstruction();
    }

    /// <inheritdoc />
    public void Run()
    {
        if (!_stateManager.CanExecute() && !_stateManager.CanResume() && State != ExecutionState.Reset)
        {
            throw new InvalidOperationException($"Cannot run in current state: {State}");
        }

        _logger?.Info("Starting continuous execution");

        ExecutionState previousState = _stateManager.CurrentState;
        _stateManager.TransitionTo(ExecutionState.Running);
        OnStateChanged(previousState, ExecutionState.Running);

        _statistics.StartTimer();

        try
        {
            while (State == ExecutionState.Running)
            {
                ushort pc = _registerFile.GetProgramCounter();

                // Check for breakpoint before executing
                if (HasBreakpoint(pc))
                {
                    _logger?.Debug($"Breakpoint hit at address 0x{pc:X4}");
                    OnBreakpointHit(pc);
                    Stop();
                    break;
                }

                ExecuteInstruction();
            }
        }
        catch (Exception ex)
        {
            _logger?.Error($"Error during execution: {ex.Message}", ex);
            ExecutionState errorPreviousState = _stateManager.CurrentState;
            _stateManager.TransitionTo(ExecutionState.Error);
            OnStateChanged(errorPreviousState, ExecutionState.Error);
            throw;
        }
        finally
        {
            _statistics.StopTimer();
        }
    }

    /// <inheritdoc />
    public ulong Run(ulong instructionCount)
    {
        if (instructionCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(instructionCount), "Instruction count must be greater than zero");
        }

        if (!_stateManager.CanExecute() && !_stateManager.CanResume() && State != ExecutionState.Reset)
        {
            throw new InvalidOperationException($"Cannot run in current state: {State}");
        }

        _logger?.Debug($"Running for {instructionCount} instructions");

        ExecutionState previousState = _stateManager.CurrentState;
        _stateManager.TransitionTo(ExecutionState.Running);
        OnStateChanged(previousState, ExecutionState.Running);

        _statistics.StartTimer();
        ulong executedCount = 0;

        try
        {
            while (State == ExecutionState.Running && executedCount < instructionCount)
            {
                ushort pc = _registerFile.GetProgramCounter();

                // Check for breakpoint before executing
                if (HasBreakpoint(pc))
                {
                    _logger?.Debug($"Breakpoint hit at address 0x{pc:X4}");
                    OnBreakpointHit(pc);
                    Stop();
                    break;
                }

                ExecuteInstruction();
                executedCount++;
            }
        }
        catch (Exception ex)
        {
            _logger?.Error($"Error during execution: {ex.Message}", ex);
            ExecutionState errorPreviousState = _stateManager.CurrentState;
            _stateManager.TransitionTo(ExecutionState.Error);
            OnStateChanged(errorPreviousState, ExecutionState.Error);
            throw;
        }
        finally
        {
            _statistics.StopTimer();
            if (State == ExecutionState.Running)
            {
                Stop();
            }
        }

        return executedCount;
    }

    /// <inheritdoc />
    public TimeSpan Run(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be greater than zero");
        }

        if (!_stateManager.CanExecute() && !_stateManager.CanResume() && State != ExecutionState.Reset)
        {
            throw new InvalidOperationException($"Cannot run in current state: {State}");
        }

        _logger?.Debug($"Running for {duration.TotalMilliseconds}ms");

        ExecutionState previousState = _stateManager.CurrentState;
        _stateManager.TransitionTo(ExecutionState.Running);
        OnStateChanged(previousState, ExecutionState.Running);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _statistics.StartTimer();

        try
        {
            while (State == ExecutionState.Running && stopwatch.Elapsed < duration)
            {
                ushort pc = _registerFile.GetProgramCounter();

                // Check for breakpoint before executing
                if (HasBreakpoint(pc))
                {
                    _logger?.Debug($"Breakpoint hit at address 0x{pc:X4}");
                    OnBreakpointHit(pc);
                    Stop();
                    break;
                }

                ExecuteInstruction();
            }
        }
        catch (Exception ex)
        {
            _logger?.Error($"Error during execution: {ex.Message}", ex);
            ExecutionState errorPreviousState = _stateManager.CurrentState;
            _stateManager.TransitionTo(ExecutionState.Error);
            OnStateChanged(errorPreviousState, ExecutionState.Error);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _statistics.StopTimer();
            if (State == ExecutionState.Running)
            {
                Stop();
            }
        }

        return stopwatch.Elapsed;
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (State == ExecutionState.Stopped)
        {
            return;
        }

        _logger?.Debug("Stopping emulator execution");

        ExecutionState previousState = _stateManager.CurrentState;
        _stateManager.TransitionTo(ExecutionState.Stopped);
        OnStateChanged(previousState, ExecutionState.Stopped);

        _statistics.StopTimer();
    }

    /// <inheritdoc />
    public void Halt()
    {
        _logger?.Debug("Halting emulator execution");

        ExecutionState previousState = _stateManager.CurrentState;
        _stateManager.TransitionTo(ExecutionState.Halted);
        OnStateChanged(previousState, ExecutionState.Halted);

        _statistics.StopTimer();
    }

    /// <inheritdoc />
    public bool AddBreakpoint(ushort address)
    {
        bool added = _breakpoints.Add(address);
        if (added)
        {
            _logger?.Debug($"Breakpoint added at address 0x{address:X4}");
        }
        return added;
    }

    /// <inheritdoc />
    public bool RemoveBreakpoint(ushort address)
    {
        bool removed = _breakpoints.Remove(address);
        if (removed)
        {
            _logger?.Debug($"Breakpoint removed from address 0x{address:X4}");
        }
        return removed;
    }

    /// <inheritdoc />
    public void ClearBreakpoints()
    {
        int count = _breakpoints.Count;
        _breakpoints.Clear();
        _logger?.Debug($"Cleared {count} breakpoints");
    }

    /// <inheritdoc />
    public bool HasBreakpoint(ushort address) => _breakpoints.Contains(address);

    private uint ExecuteInstruction()
    {
        ushort pc = _registerFile.GetProgramCounter();

        // Fetch instruction
        _memoryValidator.ValidateExecute(pc);
        ushort instructionWord = ReadMemoryWord(pc);

        // Decode instruction (validate it's decodable)
        try
        {
            _instructionDecoder.Decode(instructionWord);
        }
        catch (InvalidInstructionException ex)
        {
            _logger?.Error($"Invalid instruction 0x{instructionWord:X4} at PC 0x{pc:X4}: {ex.Message}");
            throw;
        }

        // For now, we'll just increment PC and record execution
        // Real instruction execution would be implemented here
        _registerFile.IncrementProgramCounter();

        // Simulate instruction execution cycles (typically 1-5 cycles for MSP430)
        uint cycles = 1; // Basic cycle count - would be determined by instruction type

        // Record statistics
        _statistics.RecordInstruction(cycles);

        // Raise instruction executed event
        OnInstructionExecuted(pc, instructionWord, cycles);

        _logger?.Debug($"Executed instruction 0x{instructionWord:X4} at PC 0x{pc:X4}, cycles: {cycles}");

        return cycles;
    }

    private ushort ReadMemoryWord(ushort address)
    {
        _memoryValidator.ValidateRead(address);
        _memoryValidator.ValidateRead((ushort)(address + 1));

        // MSP430 is little-endian
        return (ushort)(_memory[address] | (_memory[address + 1] << 8));
    }

    private void WriteMemoryWord(ushort address, ushort value)
    {
        _memoryValidator.ValidateWrite(address);
        _memoryValidator.ValidateWrite((ushort)(address + 1));

        // MSP430 is little-endian
        _memory[address] = (byte)(value & 0xFF);
        _memory[address + 1] = (byte)((value >> 8) & 0xFF);
    }

    private byte ReadMemoryByte(ushort address)
    {
        _memoryValidator.ValidateRead(address);
        return _memory[address];
    }

    private void WriteMemoryByte(ushort address, byte value)
    {
        _memoryValidator.ValidateWrite(address);
        _memory[address] = value;
    }

    private void OnStateChanged(ExecutionState previousState, ExecutionState newState)
    {
        StateChanged?.Invoke(this, new ExecutionStateChangedEventArgs(previousState, newState));
    }

    private void OnBreakpointHit(ushort address)
    {
        BreakpointHit?.Invoke(this, new BreakpointHitEventArgs(address));
    }

    private void OnInstructionExecuted(ushort address, ushort instructionWord, uint cycles)
    {
        InstructionExecuted?.Invoke(this, new InstructionExecutedEventArgs(address, instructionWord, cycles));
    }
}
