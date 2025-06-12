using System;
using System.Collections.Generic;
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

    // MSP430FR235x RAM region constants (SLAU445I)
    private const ushort RamStartAddress = 0x2000;
    private const ushort RamSize = 0x1000; // 4KB RAM region

    /// <summary>
    /// Internal accessor for memory - used for testing purposes.
    /// </summary>
    internal byte[] Memory => _memory;

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

        // Load PC from reset vector BEFORE clearing memory (SLAU445I Section 1.2.1)
        // "Upon completion of the boot code, the PC is loaded with the address 
        // contained at the SYSRSTIV reset location (0FFFEh)."
        LoadProgramCounterFromResetVector();

        // Clear RAM memory only (optional - real hardware doesn't clear RAM on reset)
        // Preserve interrupt vector table area (0xFFE0-0xFFFF) and other non-volatile memory
        // Only clear RAM region to simulate realistic reset behavior
        Array.Clear(_memory, RamStartAddress, RamSize);

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

    /// <summary>
    /// Loads the Program Counter from the reset vector at address 0xFFFE-0xFFFF.
    /// 
    /// According to SLAU445I Section 1.2.1: "Upon completion of the boot code, 
    /// the PC is loaded with the address contained at the SYSRSTIV reset location (0FFFEh)."
    /// 
    /// This method handles cases where the reset vector is uninitialized (0x0000) 
    /// by leaving the PC at 0x0000, which is a valid default behavior.
    /// </summary>
    private void LoadProgramCounterFromResetVector()
    {
        const ushort ResetVectorAddress = 0xFFFE;

        try
        {
            // Read 16-bit reset vector from memory (little-endian)
            ushort resetVectorValue = ReadMemoryWord(ResetVectorAddress);

            // Load PC with the reset vector value
            _registerFile.SetProgramCounter(resetVectorValue);

            _logger?.Info($"Program Counter loaded from reset vector: PC = 0x{resetVectorValue:X4} (from address 0x{ResetVectorAddress:X4})");
        }
        catch (MemoryAccessException ex)
        {
            // If we can't read the reset vector due to memory access validation failures,
            // leave PC at 0x0000 as a safe default
            _logger?.Warning($"Failed to load reset vector from 0x{ResetVectorAddress:X4}: {ex.Message}. PC remains at 0x0000.");
        }
        catch (ArgumentException ex)
        {
            // If there are argument validation issues with memory access,
            // leave PC at 0x0000 as a safe default
            _logger?.Warning($"Invalid memory access for reset vector at 0x{ResetVectorAddress:X4}: {ex.Message}. PC remains at 0x0000.");
        }
    }

    private uint ExecuteInstruction()
    {
        ushort pc = _registerFile.GetProgramCounter();

        // Fetch instruction
        _memoryValidator.ValidateExecute(pc);
        ushort instructionWord = ReadMemoryWord(pc);

        // Decode instruction
        Instruction instruction;
        try
        {
            instruction = _instructionDecoder.Decode(instructionWord);
        }
        catch (InvalidInstructionException ex)
        {
            _logger?.Error($"Invalid instruction 0x{instructionWord:X4} at PC 0x{pc:X4}: {ex.Message}");
            throw;
        }

        // Increment PC to point past the instruction word
        _registerFile.IncrementProgramCounter();

        // Fetch extension words if needed
        ushort[] extensionWords = new ushort[instruction.ExtensionWordCount];
        for (int i = 0; i < instruction.ExtensionWordCount; i++)
        {
            ushort extWordAddress = (ushort)(_registerFile.GetProgramCounter() + (i * 2));
            _memoryValidator.ValidateRead(extWordAddress);
            extensionWords[i] = ReadMemoryWord(extWordAddress);
        }

        // Increment PC past extension words
        _registerFile.SetProgramCounter((ushort)(_registerFile.GetProgramCounter() + (instruction.ExtensionWordCount * 2)));

        // Execute the instruction if it's executable
        uint cycles;
        if (instruction is IExecutableInstruction executableInstruction)
        {
            cycles = executableInstruction.Execute(_registerFile, _memory, extensionWords);
        }
        else
        {
            // For non-executable instructions (future use), just use a default cycle count
            cycles = 1;
            _logger?.Warning($"Instruction 0x{instructionWord:X4} at PC 0x{pc:X4} is not executable - skipping execution");
        }

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
