using System;
using System.Collections.Generic;
using System.Linq;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Peripherals;

/// <summary>
/// Provides base functionality for MSP430 peripheral implementations.
/// 
/// This abstract class implements common peripheral behavior including register management,
/// event handling, address validation, and lifecycle operations. Concrete peripheral
/// classes inherit from this base to implement specific MSP430FR2355 peripheral functionality.
/// 
/// Implementation based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1.4:
/// "System Resets, Interrupts, and Operating Modes" and peripheral-specific chapters.
/// </summary>
public abstract class PeripheralBase : IPeripheral
{
    private readonly Dictionary<ushort, PeripheralRegister> _registers;
    private readonly ILogger? _logger;
    private bool _isEnabled;

    /// <summary>
    /// Initializes a new instance of the PeripheralBase class.
    /// </summary>
    /// <param name="peripheralId">The unique identifier for this peripheral type.</param>
    /// <param name="baseAddress">The base address of this peripheral's register space.</param>
    /// <param name="addressSpaceSize">The size of this peripheral's register space in bytes.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when peripheralId is null.</exception>
    /// <exception cref="ArgumentException">Thrown when addressSpaceSize is zero.</exception>
    protected PeripheralBase(string peripheralId, ushort baseAddress, ushort addressSpaceSize, ILogger? logger = null)
    {
        PeripheralId = peripheralId ?? throw new ArgumentNullException(nameof(peripheralId));
        BaseAddress = baseAddress;
        AddressSpaceSize = addressSpaceSize > 0 ? addressSpaceSize : throw new ArgumentException("Address space size must be greater than zero", nameof(addressSpaceSize));
        EndAddress = (ushort)(baseAddress + addressSpaceSize - 1);
        _logger = logger;
        _registers = new Dictionary<ushort, PeripheralRegister>();
        _isEnabled = true; // Most peripherals are enabled by default
    }

    /// <inheritdoc />
    public string PeripheralId { get; }

    /// <inheritdoc />
    public ushort BaseAddress { get; }

    /// <inheritdoc />
    public ushort AddressSpaceSize { get; }

    /// <inheritdoc />
    public ushort EndAddress { get; }

    /// <inheritdoc />
    public virtual bool IsEnabled => _isEnabled;

    /// <summary>
    /// Gets the collection of registers managed by this peripheral.
    /// </summary>
    protected IReadOnlyDictionary<ushort, PeripheralRegister> Registers => _registers;

    /// <summary>
    /// Gets the logger instance for this peripheral.
    /// </summary>
    protected ILogger? Logger => _logger;

    /// <summary>
    /// Sets the enabled state of the peripheral. This should be used by derived classes
    /// to control peripheral availability.
    /// </summary>
    /// <param name="enabled">True to enable the peripheral, false to disable.</param>
    protected void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
    }

    /// <inheritdoc />
    public bool HandlesAddress(ushort address)
    {
        return address >= BaseAddress && address <= EndAddress;
    }

    /// <inheritdoc />
    public virtual byte ReadByte(ushort address)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException($"Peripheral {PeripheralId} is disabled");
        }

        if (!HandlesAddress(address))
        {
            throw new ArgumentException($"Address 0x{address:X4} is not handled by peripheral {PeripheralId}");
        }

        // Check for word-aligned register access
        ushort registerAddress = (ushort)(address & 0xFFFE); // Word-align the address
        int byteOffset = address & 1; // Get byte offset (0 or 1)

        if (_registers.TryGetValue(registerAddress, out PeripheralRegister? register))
        {
            byte value = register.ReadByte(byteOffset);
            OnRegisterRead(address, value, false);
            return value;
        }

        // If no register is found, call virtual method for subclass handling
        byte defaultValue = ReadUnmappedByte(address);
        OnRegisterRead(address, defaultValue, false);
        return defaultValue;
    }

    /// <inheritdoc />
    public virtual bool WriteByte(ushort address, byte value)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException($"Peripheral {PeripheralId} is disabled");
        }

        if (!HandlesAddress(address))
        {
            throw new ArgumentException($"Address 0x{address:X4} is not handled by peripheral {PeripheralId}");
        }

        // Check for word-aligned register access
        ushort registerAddress = (ushort)(address & 0xFFFE); // Word-align the address
        int byteOffset = address & 1; // Get byte offset (0 or 1)

        if (_registers.TryGetValue(registerAddress, out PeripheralRegister? register))
        {
            bool success = register.WriteByte(value, byteOffset);
            if (success)
            {
                OnRegisterWritten(address, value, false);
                OnRegisterChanged(register, address);
            }
            return success;
        }

        // If no register is found, call virtual method for subclass handling
        bool result = WriteUnmappedByte(address, value);
        if (result)
        {
            OnRegisterWritten(address, value, false);
        }
        return result;
    }

    /// <inheritdoc />
    public virtual ushort ReadWord(ushort address)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException($"Peripheral {PeripheralId} is disabled");
        }

        if (!HandlesAddress(address))
        {
            throw new ArgumentException($"Address 0x{address:X4} is not handled by peripheral {PeripheralId}");
        }

        if ((address & 1) != 0)
        {
            throw new ArgumentException($"Address 0x{address:X4} is not word-aligned");
        }

        if (_registers.TryGetValue(address, out PeripheralRegister? register))
        {
            ushort value = register.ReadWord();
            OnRegisterRead(address, value, true);
            return value;
        }

        // If no register is found, call virtual method for subclass handling
        ushort defaultValue = ReadUnmappedWord(address);
        OnRegisterRead(address, defaultValue, true);
        return defaultValue;
    }

    /// <inheritdoc />
    public virtual bool WriteWord(ushort address, ushort value)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException($"Peripheral {PeripheralId} is disabled");
        }

        if (!HandlesAddress(address))
        {
            throw new ArgumentException($"Address 0x{address:X4} is not handled by peripheral {PeripheralId}");
        }

        if ((address & 1) != 0)
        {
            throw new ArgumentException($"Address 0x{address:X4} is not word-aligned");
        }

        if (_registers.TryGetValue(address, out PeripheralRegister? register))
        {
            bool success = register.WriteWord(value);
            if (success)
            {
                OnRegisterWritten(address, value, true);
                OnRegisterChanged(register, address);
            }
            return success;
        }

        // If no register is found, call virtual method for subclass handling
        bool result = WriteUnmappedWord(address, value);
        if (result)
        {
            OnRegisterWritten(address, value, true);
        }
        return result;
    }

    /// <inheritdoc />
    public virtual void Reset()
    {
        _logger?.Debug($"Resetting peripheral {PeripheralId}");

        // Reset all registers to their default values
        foreach (PeripheralRegister register in _registers.Values)
        {
            register.Reset();
        }

        // Allow subclasses to perform additional reset operations
        OnReset();

        _logger?.Debug($"Peripheral {PeripheralId} reset completed");
    }

    /// <inheritdoc />
    public virtual void Initialize(object? initializationData = null)
    {
        _logger?.Debug($"Initializing peripheral {PeripheralId}");

        // Allow subclasses to perform initialization
        OnInitialize(initializationData);

        _logger?.Debug($"Peripheral {PeripheralId} initialization completed");
    }

    /// <inheritdoc />
    public event EventHandler<PeripheralRegisterAccessEventArgs>? RegisterRead;

    /// <inheritdoc />
    public event EventHandler<PeripheralRegisterAccessEventArgs>? RegisterWritten;

    /// <inheritdoc />
    public event EventHandler<PeripheralInterruptEventArgs>? InterruptRequested;

    /// <summary>
    /// Adds a register to this peripheral's register map.
    /// </summary>
    /// <param name="register">The register to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when register is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the register address is outside this peripheral's address space or already exists.</exception>
    protected void AddRegister(PeripheralRegister register)
    {
        ArgumentNullException.ThrowIfNull(register);

        if (!HandlesAddress(register.Address))
        {
            throw new ArgumentException($"Register address 0x{register.Address:X4} is outside peripheral {PeripheralId} address space (0x{BaseAddress:X4}-0x{EndAddress:X4})");
        }

        if (_registers.ContainsKey(register.Address))
        {
            throw new ArgumentException($"Register at address 0x{register.Address:X4} already exists in peripheral {PeripheralId}");
        }

        _registers[register.Address] = register;
        _logger?.Debug($"Added register {register.Name} at address 0x{register.Address:X4} to peripheral {PeripheralId}");
    }

    /// <summary>
    /// Gets a register by its address.
    /// </summary>
    /// <param name="address">The register address.</param>
    /// <returns>The register at the specified address, or null if not found.</returns>
    protected PeripheralRegister? GetRegister(ushort address)
    {
        _registers.TryGetValue(address, out PeripheralRegister? register);
        return register;
    }

    /// <summary>
    /// Called when the peripheral is reset. Override in derived classes for custom reset behavior.
    /// </summary>
    protected virtual void OnReset()
    {
        // Default implementation does nothing
    }

    /// <summary>
    /// Called when the peripheral is initialized. Override in derived classes for custom initialization.
    /// </summary>
    /// <param name="initializationData">Optional initialization data.</param>
    protected virtual void OnInitialize(object? initializationData)
    {
        // Default implementation does nothing
    }

    /// <summary>
    /// Called when a register value changes. Override in derived classes to implement register side effects.
    /// </summary>
    /// <param name="register">The register that was changed.</param>
    /// <param name="address">The specific address that was accessed (may be byte within word register).</param>
    protected virtual void OnRegisterChanged(PeripheralRegister register, ushort address)
    {
        // Default implementation does nothing
    }

    /// <summary>
    /// Handles read access to unmapped addresses within the peripheral's address space.
    /// Override in derived classes to provide custom behavior.
    /// </summary>
    /// <param name="address">The unmapped address being read.</param>
    /// <returns>The value to return for the unmapped read.</returns>
    protected virtual byte ReadUnmappedByte(ushort address)
    {
        _logger?.Warning($"Read from unmapped byte address 0x{address:X4} in peripheral {PeripheralId}");
        return 0x00; // Default to returning 0
    }

    /// <summary>
    /// Handles write access to unmapped addresses within the peripheral's address space.
    /// Override in derived classes to provide custom behavior.
    /// </summary>
    /// <param name="address">The unmapped address being written.</param>
    /// <param name="value">The value being written.</param>
    /// <returns>True if the write was handled, false otherwise.</returns>
    protected virtual bool WriteUnmappedByte(ushort address, byte value)
    {
        _logger?.Warning($"Write to unmapped byte address 0x{address:X4} = 0x{value:X2} in peripheral {PeripheralId}");
        return false; // Default to ignoring unmapped writes
    }

    /// <summary>
    /// Handles read access to unmapped word addresses within the peripheral's address space.
    /// Override in derived classes to provide custom behavior.
    /// </summary>
    /// <param name="address">The unmapped address being read.</param>
    /// <returns>The value to return for the unmapped read.</returns>
    protected virtual ushort ReadUnmappedWord(ushort address)
    {
        _logger?.Warning($"Read from unmapped word address 0x{address:X4} in peripheral {PeripheralId}");
        return 0x0000; // Default to returning 0
    }

    /// <summary>
    /// Handles write access to unmapped word addresses within the peripheral's address space.
    /// Override in derived classes to provide custom behavior.
    /// </summary>
    /// <param name="address">The unmapped address being written.</param>
    /// <param name="value">The value being written.</param>
    /// <returns>True if the write was handled, false otherwise.</returns>
    protected virtual bool WriteUnmappedWord(ushort address, ushort value)
    {
        _logger?.Warning($"Write to unmapped word address 0x{address:X4} = 0x{value:X4} in peripheral {PeripheralId}");
        return false; // Default to ignoring unmapped writes
    }

    /// <summary>
    /// Raises the RegisterRead event.
    /// </summary>
    /// <param name="address">The address that was read.</param>
    /// <param name="value">The value that was read.</param>
    /// <param name="isWordAccess">True if this was a word access, false for byte access.</param>
    protected virtual void OnRegisterRead(ushort address, ushort value, bool isWordAccess)
    {
        RegisterRead?.Invoke(this, new PeripheralRegisterAccessEventArgs(address, value, false, isWordAccess));
    }

    /// <summary>
    /// Raises the RegisterWritten event.
    /// </summary>
    /// <param name="address">The address that was written.</param>
    /// <param name="value">The value that was written.</param>
    /// <param name="isWordAccess">True if this was a word access, false for byte access.</param>
    protected virtual void OnRegisterWritten(ushort address, ushort value, bool isWordAccess)
    {
        RegisterWritten?.Invoke(this, new PeripheralRegisterAccessEventArgs(address, value, true, isWordAccess));
    }

    /// <summary>
    /// Raises the InterruptRequested event.
    /// </summary>
    /// <param name="interruptVector">The interrupt vector address.</param>
    /// <param name="interruptName">The name of the interrupt.</param>
    /// <param name="priority">The interrupt priority.</param>
    protected virtual void OnInterruptRequested(ushort interruptVector, string interruptName, byte priority)
    {
        InterruptRequested?.Invoke(this, new PeripheralInterruptEventArgs(interruptVector, interruptName, priority, PeripheralId));
    }
}
