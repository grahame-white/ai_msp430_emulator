using System;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Implements the unified memory controller for the MSP430 emulator.
/// 
/// This class provides a single point of access for all memory operations,
/// handling address decoding, routing to appropriate memory components, access arbitration,
/// and memory-mapped peripheral support.
/// 
/// The controller integrates RAM, Flash, and Information memory into a cohesive system
/// while maintaining proper access control and timing characteristics.
/// 
/// Implementation based on MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 2:
/// "System Architecture" - Memory controller and address space organization.
/// </summary>
public class MemoryController : IMemoryController
{
    private readonly MemoryAccessValidator _validator;
    private readonly ILogger? _logger;
    private readonly MemoryStatistics _statistics;

    /// <summary>
    /// Initializes a new instance of the MemoryController class.
    /// </summary>
    /// <param name="memoryMap">The memory map for address validation and routing.</param>
    /// <param name="ram">The RAM component.</param>
    /// <param name="flash">The Flash memory component.</param>
    /// <param name="informationMemory">The Information memory component.</param>
    /// <param name="logger">Optional logger for debugging and monitoring.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public MemoryController(
        IMemoryMap memoryMap,
        IRandomAccessMemory ram,
        IFlashMemory flash,
        IInformationMemory informationMemory,
        ILogger? logger = null)
    {
        MemoryMap = memoryMap ?? throw new ArgumentNullException(nameof(memoryMap));
        Ram = ram ?? throw new ArgumentNullException(nameof(ram));
        Flash = flash ?? throw new ArgumentNullException(nameof(flash));
        InformationMemory = informationMemory ?? throw new ArgumentNullException(nameof(informationMemory));

        _logger = logger;
        _validator = new MemoryAccessValidator(memoryMap, logger);
        _statistics = new MemoryStatistics();

        _logger?.Debug("MemoryController initialized successfully");
    }

    /// <inheritdoc />
    public IMemoryMap MemoryMap { get; }

    /// <inheritdoc />
    public IRandomAccessMemory Ram { get; }

    /// <inheritdoc />
    public IFlashMemory Flash { get; }

    /// <inheritdoc />
    public IInformationMemory InformationMemory { get; }

    /// <inheritdoc />
    public bool IsOperationInProgress => Flash.IsOperationInProgress;

    /// <inheritdoc />
    public MemoryStatistics Statistics => _statistics;

    /// <inheritdoc />
    public event EventHandler<MemoryAccessEventArgs>? MemoryAccessed;

    /// <inheritdoc />
    public event EventHandler<MemoryAccessViolationEventArgs>? AccessViolation;

    /// <inheritdoc />
    public byte ReadByte(ushort address)
    {
        var context = MemoryAccessContext.CreateReadByte(address);
        return ReadByte(context);
    }

    /// <inheritdoc />
    public byte ReadByte(MemoryAccessContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            ValidateAccess(context);
            MemoryRegionInfo region = GetRegion(context.Address);
            uint cycles = GetAccessCycles(context);

            byte value = RouteReadByte(context, region);

            _statistics.TotalReads++;
            _statistics.TotalCycles += cycles;

            MemoryAccessed?.Invoke(this, new MemoryAccessEventArgs(context, region, cycles, value));

            _logger?.Debug($"ReadByte: Address=0x{context.Address:X4}, Value=0x{value:X2}, Region={region.Region}, Cycles={cycles}");

            return value;
        }
        catch (Exception ex)
        {
            _statistics.TotalViolations++;
            AccessViolation?.Invoke(this, new MemoryAccessViolationEventArgs(context, ex.Message, ex));
            throw;
        }
    }

    /// <inheritdoc />
    public ushort ReadWord(ushort address)
    {
        var context = MemoryAccessContext.CreateReadWord(address);
        return ReadWord(context);
    }

    /// <inheritdoc />
    public ushort ReadWord(MemoryAccessContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            context.ValidateAlignment();
            ValidateAccess(context);
            MemoryRegionInfo region = GetRegion(context.Address);
            uint cycles = GetAccessCycles(context);

            ushort value = RouteReadWord(context, region);

            _statistics.TotalReads++;
            _statistics.TotalCycles += cycles;

            MemoryAccessed?.Invoke(this, new MemoryAccessEventArgs(context, region, cycles, value));

            _logger?.Debug($"ReadWord: Address=0x{context.Address:X4}, Value=0x{value:X4}, Region={region.Region}, Cycles={cycles}");

            return value;
        }
        catch (Exception ex)
        {
            _statistics.TotalViolations++;
            AccessViolation?.Invoke(this, new MemoryAccessViolationEventArgs(context, ex.Message, ex));
            throw;
        }
    }

    /// <inheritdoc />
    public bool WriteByte(ushort address, byte value)
    {
        var context = MemoryAccessContext.CreateWriteByte(address);
        return WriteByte(context, value);
    }

    /// <inheritdoc />
    public bool WriteByte(MemoryAccessContext context, byte value)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            ValidateAccess(context);
            MemoryRegionInfo region = GetRegion(context.Address);
            uint cycles = GetAccessCycles(context);

            bool success = RouteWriteByte(context, region, value);

            _statistics.TotalWrites++;
            _statistics.TotalCycles += cycles;

            MemoryAccessed?.Invoke(this, new MemoryAccessEventArgs(context, region, cycles, value));

            _logger?.Debug($"WriteByte: Address=0x{context.Address:X4}, Value=0x{value:X2}, Region={region.Region}, Success={success}, Cycles={cycles}");

            return success;
        }
        catch (Exception ex)
        {
            _statistics.TotalViolations++;
            AccessViolation?.Invoke(this, new MemoryAccessViolationEventArgs(context, ex.Message, ex));
            throw;
        }
    }

    /// <inheritdoc />
    public bool WriteWord(ushort address, ushort value)
    {
        var context = MemoryAccessContext.CreateWriteWord(address);
        return WriteWord(context, value);
    }

    /// <inheritdoc />
    public bool WriteWord(MemoryAccessContext context, ushort value)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            context.ValidateAlignment();
            ValidateAccess(context);
            MemoryRegionInfo region = GetRegion(context.Address);
            uint cycles = GetAccessCycles(context);

            bool success = RouteWriteWord(context, region, value);

            _statistics.TotalWrites++;
            _statistics.TotalCycles += cycles;

            MemoryAccessed?.Invoke(this, new MemoryAccessEventArgs(context, region, cycles, value));

            _logger?.Debug($"WriteWord: Address=0x{context.Address:X4}, Value=0x{value:X4}, Region={region.Region}, Success={success}, Cycles={cycles}");

            return success;
        }
        catch (Exception ex)
        {
            _statistics.TotalViolations++;
            AccessViolation?.Invoke(this, new MemoryAccessViolationEventArgs(context, ex.Message, ex));
            throw;
        }
    }

    /// <inheritdoc />
    public ushort FetchInstruction(ushort address)
    {
        var context = MemoryAccessContext.CreateExecute(address);
        return FetchInstruction(context);
    }

    /// <inheritdoc />
    public ushort FetchInstruction(MemoryAccessContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            ValidateAccess(context);
            MemoryRegionInfo region = GetRegion(context.Address);
            uint cycles = GetAccessCycles(context);

            ushort instruction = RouteReadWord(context, region);

            _statistics.TotalInstructionFetches++;
            _statistics.TotalCycles += cycles;

            MemoryAccessed?.Invoke(this, new MemoryAccessEventArgs(context, region, cycles, instruction));

            _logger?.Debug($"FetchInstruction: Address=0x{context.Address:X4}, Instruction=0x{instruction:X4}, Region={region.Region}, Cycles={cycles}");

            return instruction;
        }
        catch (Exception ex)
        {
            _statistics.TotalViolations++;
            AccessViolation?.Invoke(this, new MemoryAccessViolationEventArgs(context, ex.Message, ex));
            throw;
        }
    }

    /// <inheritdoc />
    public void ValidateAccess(MemoryAccessContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _validator.ValidateAccess(context.Address, context.AccessType);
    }

    /// <inheritdoc />
    public MemoryRegionInfo GetRegion(ushort address)
    {
        return MemoryMap.GetRegion(address);
    }

    /// <inheritdoc />
    public uint GetAccessCycles(MemoryAccessContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        MemoryRegionInfo region = GetRegion(context.Address);

        return region.Region switch
        {
            MemoryRegion.Ram => GetRamCycles(context),
            MemoryRegion.Flash => GetFlashCycles(context),
            MemoryRegion.InformationMemory => GetInformationMemoryCycles(context),
            MemoryRegion.SpecialFunctionRegisters => 1, // SFR access is typically 1 cycle
            MemoryRegion.Peripherals8Bit => 1,          // Peripheral access is typically 1 cycle
            MemoryRegion.Peripherals16Bit => 1,         // Peripheral access is typically 1 cycle
            _ => 1 // Default to 1 cycle for other regions
        };
    }

    /// <inheritdoc />
    public bool IsValidAddress(ushort address)
    {
        return MemoryMap.IsValidAddress(address);
    }

    /// <inheritdoc />
    public bool IsAccessAllowed(ushort address, MemoryAccessPermissions accessType)
    {
        return MemoryMap.IsAccessAllowed(address, accessType);
    }

    /// <inheritdoc />
    public MemoryAccessPermissions GetPermissions(ushort address)
    {
        return MemoryMap.GetPermissions(address);
    }

    /// <inheritdoc />
    public void Reset()
    {
        _logger?.Info("Resetting memory controller and all memory components");

        Ram.Clear();
        Flash.Initialize(); // Reset to erased state (0xFF)
        InformationMemory.Initialize(); // Reset to erased state where not protected

        _statistics.Reset();

        _logger?.Info("Memory controller reset completed");
    }

    /// <summary>
    /// Routes a byte read operation to the appropriate memory component.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <param name="region">The memory region information.</param>
    /// <returns>The byte value read from memory.</returns>
    private byte RouteReadByte(MemoryAccessContext context, MemoryRegionInfo region)
    {
        return region.Region switch
        {
            MemoryRegion.Ram => Ram.ReadByte(context.Address),
            MemoryRegion.Flash => Flash.ReadByte(context.Address),
            MemoryRegion.InformationMemory => InformationMemory.ReadByte(context.Address),
            MemoryRegion.SpecialFunctionRegisters => HandlePeripheralRead(context.Address, isWord: false),
            MemoryRegion.Peripherals8Bit => HandlePeripheralRead(context.Address, isWord: false),
            MemoryRegion.Peripherals16Bit => HandlePeripheralRead(context.Address, isWord: false),
            _ => throw new MemoryAccessException(context.Address, context.AccessType, $"Reading from region {region.Region} is not supported")
        };
    }

    /// <summary>
    /// Routes a word read operation to the appropriate memory component.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <param name="region">The memory region information.</param>
    /// <returns>The word value read from memory.</returns>
    private ushort RouteReadWord(MemoryAccessContext context, MemoryRegionInfo region)
    {
        return region.Region switch
        {
            MemoryRegion.Ram => Ram.ReadWord(context.Address),
            MemoryRegion.Flash => Flash.ReadWord(context.Address),
            MemoryRegion.InformationMemory => InformationMemory.ReadWord(context.Address),
            MemoryRegion.SpecialFunctionRegisters => HandlePeripheralRead(context.Address, isWord: true),
            MemoryRegion.Peripherals8Bit => HandlePeripheralRead(context.Address, isWord: true),
            MemoryRegion.Peripherals16Bit => HandlePeripheralRead(context.Address, isWord: true),
            _ => throw new MemoryAccessException(context.Address, context.AccessType, $"Reading from region {region.Region} is not supported")
        };
    }

    /// <summary>
    /// Routes a byte write operation to the appropriate memory component.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <param name="region">The memory region information.</param>
    /// <param name="value">The value to write.</param>
    /// <returns>True if the write was successful, false if blocked.</returns>
    private bool RouteWriteByte(MemoryAccessContext context, MemoryRegionInfo region, byte value)
    {
        switch (region.Region)
        {
            case MemoryRegion.Ram:
                Ram.WriteByte(context.Address, value);
                return true;
            case MemoryRegion.Flash:
                return Flash.ProgramByte(context.Address, value);
            case MemoryRegion.InformationMemory:
                return InformationMemory.WriteByte(context.Address, value);
            case MemoryRegion.SpecialFunctionRegisters:
                return HandlePeripheralWrite(context.Address, value, isWord: false);
            case MemoryRegion.Peripherals8Bit:
                return HandlePeripheralWrite(context.Address, value, isWord: false);
            case MemoryRegion.Peripherals16Bit:
                return HandlePeripheralWrite(context.Address, value, isWord: false);
            default:
                throw new MemoryAccessException(context.Address, context.AccessType, $"Writing to region {region.Region} is not supported");
        }
    }

    /// <summary>
    /// Routes a word write operation to the appropriate memory component.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <param name="region">The memory region information.</param>
    /// <param name="value">The value to write.</param>
    /// <returns>True if the write was successful, false if blocked.</returns>
    private bool RouteWriteWord(MemoryAccessContext context, MemoryRegionInfo region, ushort value)
    {
        switch (region.Region)
        {
            case MemoryRegion.Ram:
                Ram.WriteWord(context.Address, value);
                return true;
            case MemoryRegion.Flash:
                return Flash.ProgramWord(context.Address, value);
            case MemoryRegion.InformationMemory:
                return InformationMemory.WriteWord(context.Address, value);
            case MemoryRegion.SpecialFunctionRegisters:
                return HandlePeripheralWrite(context.Address, value, isWord: true);
            case MemoryRegion.Peripherals8Bit:
                return HandlePeripheralWrite(context.Address, value, isWord: true);
            case MemoryRegion.Peripherals16Bit:
                return HandlePeripheralWrite(context.Address, value, isWord: true);
            default:
                throw new MemoryAccessException(context.Address, context.AccessType, $"Writing to region {region.Region} is not supported");
        }
    }

    /// <summary>
    /// Gets the number of CPU cycles for RAM access.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <returns>The number of CPU cycles required.</returns>
    private uint GetRamCycles(MemoryAccessContext context)
    {
        return context.AccessType switch
        {
            MemoryAccessPermissions.Read => Ram.GetReadCycles(context.Address, context.IsWordAccess),
            MemoryAccessPermissions.Write => Ram.GetWriteCycles(context.Address, context.IsWordAccess),
            MemoryAccessPermissions.Execute => Ram.GetReadCycles(context.Address, context.IsWordAccess),
            _ => 1
        };
    }

    /// <summary>
    /// Gets the number of CPU cycles for Flash memory access.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <returns>The number of CPU cycles required.</returns>
    private uint GetFlashCycles(MemoryAccessContext context)
    {
        return context.AccessType switch
        {
            MemoryAccessPermissions.Read => Flash.GetReadCycles(context.Address, context.IsWordAccess),
            MemoryAccessPermissions.Write => Flash.GetProgramCycles(context.Address, context.IsWordAccess),
            MemoryAccessPermissions.Execute => Flash.GetReadCycles(context.Address, context.IsWordAccess),
            _ => 1
        };
    }

    /// <summary>
    /// Gets the number of CPU cycles for Information memory access.
    /// Information memory typically has the same timing as Flash memory.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <returns>The number of CPU cycles required.</returns>
    private uint GetInformationMemoryCycles(MemoryAccessContext context)
    {
        // Information memory typically has similar timing to Flash memory
        // For simplicity, we'll use constant timing here
        return context.AccessType switch
        {
            MemoryAccessPermissions.Read => 1,
            MemoryAccessPermissions.Write => 30, // Programming operations are slower
            MemoryAccessPermissions.Execute => 1,
            _ => 1
        };
    }

    /// <summary>
    /// Handles peripheral read operations.
    /// This is a placeholder for memory-mapped peripheral support.
    /// </summary>
    /// <param name="address">The peripheral address.</param>
    /// <param name="isWord">True for word access, false for byte access.</param>
    /// <returns>The value read from the peripheral.</returns>
    private dynamic HandlePeripheralRead(ushort address, bool isWord)
    {
        // Placeholder implementation for memory-mapped peripherals
        // In a full implementation, this would route to specific peripheral controllers
        _logger?.Warning($"Peripheral read not implemented: Address=0x{address:X4}, IsWord={isWord}");

        return isWord ? (ushort)0x0000 : (byte)0x00; // Return default value for unimplemented peripherals
    }

    /// <summary>
    /// Handles peripheral write operations.
    /// This is a placeholder for memory-mapped peripheral support.
    /// </summary>
    /// <param name="address">The peripheral address.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="isWord">True for word access, false for byte access.</param>
    /// <returns>True if the write was successful.</returns>
    private bool HandlePeripheralWrite(ushort address, dynamic value, bool isWord)
    {
        // Placeholder implementation for memory-mapped peripherals
        // In a full implementation, this would route to specific peripheral controllers
        _logger?.Warning($"Peripheral write not implemented: Address=0x{address:X4}, Value=0x{value:X}, IsWord={isWord}");

        return true; // Return success for unimplemented peripherals
    }
}
