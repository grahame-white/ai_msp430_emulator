using System;
using System.Collections.Generic;
using System.Linq;
using MSP430.Emulator.Logging;

namespace MSP430.Emulator.Peripherals;

/// <summary>
/// Manages peripheral device registration, lifecycle, and memory access routing.
/// 
/// The PeripheralManager serves as the central coordinator for all peripheral devices
/// in the MSP430 emulator, handling registration, address routing, lifecycle management,
/// and event aggregation.
/// 
/// Implementation based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1.9:
/// "Memory Map" and peripheral-specific chapters.
/// </summary>
public class PeripheralManager
{
    private readonly Dictionary<string, IPeripheral> _peripherals;
    private readonly List<IPeripheral> _peripheralsList; // For ordered iteration
    private readonly ILogger? _logger;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the PeripheralManager class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public PeripheralManager(ILogger? logger = null)
    {
        _peripherals = new Dictionary<string, IPeripheral>();
        _peripheralsList = new List<IPeripheral>();
        _logger = logger;
        _isInitialized = false;
    }

    /// <summary>
    /// Gets the collection of registered peripherals.
    /// </summary>
    public IReadOnlyList<IPeripheral> Peripherals => _peripheralsList.AsReadOnly();

    /// <summary>
    /// Gets the number of registered peripherals.
    /// </summary>
    public int Count => _peripherals.Count;

    /// <summary>
    /// Gets a value indicating whether the peripheral manager has been initialized.
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Event raised when a peripheral register is read.
    /// </summary>
    public event EventHandler<PeripheralRegisterAccessEventArgs>? RegisterRead;

    /// <summary>
    /// Event raised when a peripheral register is written.
    /// </summary>
    public event EventHandler<PeripheralRegisterAccessEventArgs>? RegisterWritten;

    /// <summary>
    /// Event raised when a peripheral generates an interrupt request.
    /// </summary>
    public event EventHandler<PeripheralInterruptEventArgs>? InterruptRequested;

    /// <summary>
    /// Registers a peripheral device with the manager.
    /// </summary>
    /// <param name="peripheral">The peripheral to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when peripheral is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a peripheral with the same ID is already registered or address space conflicts exist.</exception>
    public void RegisterPeripheral(IPeripheral peripheral)
    {
        ArgumentNullException.ThrowIfNull(peripheral);

        if (_peripherals.ContainsKey(peripheral.PeripheralId))
        {
            throw new InvalidOperationException($"Peripheral with ID '{peripheral.PeripheralId}' is already registered");
        }

        // Check for address space conflicts
        IPeripheral? conflictingPeripheral = _peripheralsList.Where(existingPeripheral => HasAddressConflict(peripheral, existingPeripheral)).FirstOrDefault();
        if (conflictingPeripheral != null)
        {
            throw new InvalidOperationException(
                $"Address space conflict: Peripheral '{peripheral.PeripheralId}' (0x{peripheral.BaseAddress:X4}-0x{peripheral.EndAddress:X4}) " +
                $"conflicts with '{conflictingPeripheral.PeripheralId}' (0x{conflictingPeripheral.BaseAddress:X4}-0x{conflictingPeripheral.EndAddress:X4})");
        }

        _peripherals[peripheral.PeripheralId] = peripheral;
        _peripheralsList.Add(peripheral);

        // Subscribe to peripheral events
        peripheral.RegisterRead += OnPeripheralRegisterRead;
        peripheral.RegisterWritten += OnPeripheralRegisterWritten;
        peripheral.InterruptRequested += OnPeripheralInterruptRequested;

        _logger?.Info($"Registered peripheral '{peripheral.PeripheralId}' at address range 0x{peripheral.BaseAddress:X4}-0x{peripheral.EndAddress:X4}");
    }

    /// <summary>
    /// Unregisters a peripheral device from the manager.
    /// </summary>
    /// <param name="peripheralId">The ID of the peripheral to unregister.</param>
    /// <returns>True if the peripheral was found and unregistered, false otherwise.</returns>
    public bool UnregisterPeripheral(string peripheralId)
    {
        if (string.IsNullOrEmpty(peripheralId))
        {
            return false;
        }

        if (!_peripherals.TryGetValue(peripheralId, out IPeripheral? peripheral))
        {
            return false;
        }

        // Unsubscribe from peripheral events
        peripheral.RegisterRead -= OnPeripheralRegisterRead;
        peripheral.RegisterWritten -= OnPeripheralRegisterWritten;
        peripheral.InterruptRequested -= OnPeripheralInterruptRequested;

        _peripherals.Remove(peripheralId);
        _peripheralsList.Remove(peripheral);

        _logger?.Info($"Unregistered peripheral '{peripheralId}'");
        return true;
    }

    /// <summary>
    /// Gets a peripheral by its ID.
    /// </summary>
    /// <param name="peripheralId">The ID of the peripheral to retrieve.</param>
    /// <returns>The peripheral with the specified ID, or null if not found.</returns>
    public IPeripheral? GetPeripheral(string peripheralId)
    {
        if (string.IsNullOrEmpty(peripheralId))
        {
            return null;
        }

        _peripherals.TryGetValue(peripheralId, out IPeripheral? peripheral);
        return peripheral;
    }

    /// <summary>
    /// Gets the peripheral that handles the specified memory address.
    /// </summary>
    /// <param name="address">The memory address to look up.</param>
    /// <returns>The peripheral that handles the address, or null if no peripheral is found.</returns>
    public IPeripheral? GetPeripheralForAddress(ushort address)
    {
        return _peripheralsList.FirstOrDefault(p => p.HandlesAddress(address));
    }

    /// <summary>
    /// Determines if the specified address is handled by any registered peripheral.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>True if a peripheral handles the address, false otherwise.</returns>
    public bool HandlesAddress(ushort address)
    {
        return GetPeripheralForAddress(address) != null;
    }

    /// <summary>
    /// Reads a byte from the specified peripheral address.
    /// </summary>
    /// <param name="address">The address to read from.</param>
    /// <returns>The byte value at the specified address.</returns>
    /// <exception cref="ArgumentException">Thrown when no peripheral handles the specified address.</exception>
    public byte ReadByte(ushort address)
    {
        IPeripheral? peripheral = GetPeripheralForAddress(address);
        if (peripheral == null)
        {
            throw new ArgumentException($"No peripheral handles address 0x{address:X4}");
        }

        return peripheral.ReadByte(address);
    }

    /// <summary>
    /// Writes a byte to the specified peripheral address.
    /// </summary>
    /// <param name="address">The address to write to.</param>
    /// <param name="value">The byte value to write.</param>
    /// <returns>True if the write was successful, false if blocked by peripheral protection.</returns>
    /// <exception cref="ArgumentException">Thrown when no peripheral handles the specified address.</exception>
    public bool WriteByte(ushort address, byte value)
    {
        IPeripheral? peripheral = GetPeripheralForAddress(address);
        if (peripheral == null)
        {
            throw new ArgumentException($"No peripheral handles address 0x{address:X4}");
        }

        return peripheral.WriteByte(address, value);
    }

    /// <summary>
    /// Reads a 16-bit word from the specified peripheral address.
    /// </summary>
    /// <param name="address">The address to read from (must be word-aligned).</param>
    /// <returns>The 16-bit word value at the specified address.</returns>
    /// <exception cref="ArgumentException">Thrown when no peripheral handles the specified address or address is not word-aligned.</exception>
    public ushort ReadWord(ushort address)
    {
        if ((address & 1) != 0)
        {
            throw new ArgumentException($"Address 0x{address:X4} is not word-aligned");
        }

        IPeripheral? peripheral = GetPeripheralForAddress(address);
        if (peripheral == null)
        {
            throw new ArgumentException($"No peripheral handles address 0x{address:X4}");
        }

        return peripheral.ReadWord(address);
    }

    /// <summary>
    /// Writes a 16-bit word to the specified peripheral address.
    /// </summary>
    /// <param name="address">The address to write to (must be word-aligned).</param>
    /// <param name="value">The 16-bit word value to write.</param>
    /// <returns>True if the write was successful, false if blocked by peripheral protection.</returns>
    /// <exception cref="ArgumentException">Thrown when no peripheral handles the specified address or address is not word-aligned.</exception>
    public bool WriteWord(ushort address, ushort value)
    {
        if ((address & 1) != 0)
        {
            throw new ArgumentException($"Address 0x{address:X4} is not word-aligned");
        }

        IPeripheral? peripheral = GetPeripheralForAddress(address);
        if (peripheral == null)
        {
            throw new ArgumentException($"No peripheral handles address 0x{address:X4}");
        }

        return peripheral.WriteWord(address, value);
    }

    /// <summary>
    /// Resets all registered peripherals to their initial state.
    /// </summary>
    public void Reset()
    {
        _logger?.Info("Resetting all peripherals");

        foreach (IPeripheral peripheral in _peripheralsList)
        {
            try
            {
                peripheral.Reset();
            }
            catch (InvalidOperationException ex)
            {
                _logger?.Error($"Error resetting peripheral '{peripheral.PeripheralId}': {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                _logger?.Error($"Error resetting peripheral '{peripheral.PeripheralId}': {ex.Message}");
            }
        }

        _logger?.Info("All peripherals reset completed");
    }

    /// <summary>
    /// Initializes all registered peripherals.
    /// </summary>
    /// <param name="initializationData">Optional initialization data to pass to peripherals.</param>
    public void Initialize(object? initializationData = null)
    {
        _logger?.Info("Initializing all peripherals");

        foreach (IPeripheral peripheral in _peripheralsList)
        {
            try
            {
                peripheral.Initialize(initializationData);
            }
            catch (InvalidOperationException ex)
            {
                _logger?.Error($"Error initializing peripheral '{peripheral.PeripheralId}': {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                _logger?.Error($"Error initializing peripheral '{peripheral.PeripheralId}': {ex.Message}");
            }
        }

        _isInitialized = true;
        _logger?.Info("All peripherals initialization completed");
    }

    /// <summary>
    /// Gets diagnostic information about all registered peripherals.
    /// </summary>
    /// <returns>A dictionary containing peripheral information.</returns>
    public Dictionary<string, object> GetDiagnosticInfo()
    {
        var info = new Dictionary<string, object>
        {
            ["TotalPeripherals"] = _peripherals.Count,
            ["IsInitialized"] = _isInitialized,
            ["Peripherals"] = _peripheralsList.Select(p => new
            {
                Id = p.PeripheralId,
                BaseAddress = $"0x{p.BaseAddress:X4}",
                EndAddress = $"0x{p.EndAddress:X4}",
                AddressSpaceSize = p.AddressSpaceSize,
                IsEnabled = p.IsEnabled
            }).ToList()
        };

        return info;
    }

    /// <summary>
    /// Checks if two peripherals have conflicting address spaces.
    /// </summary>
    /// <param name="peripheral1">The first peripheral.</param>
    /// <param name="peripheral2">The second peripheral.</param>
    /// <returns>True if the peripherals have conflicting address spaces, false otherwise.</returns>
    private static bool HasAddressConflict(IPeripheral peripheral1, IPeripheral peripheral2)
    {
        // Check if address ranges overlap
        return !(peripheral1.EndAddress < peripheral2.BaseAddress || peripheral2.EndAddress < peripheral1.BaseAddress);
    }

    /// <summary>
    /// Handles register read events from peripherals.
    /// </summary>
    /// <param name="sender">The peripheral that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnPeripheralRegisterRead(object? sender, PeripheralRegisterAccessEventArgs e)
    {
        RegisterRead?.Invoke(sender, e);
    }

    /// <summary>
    /// Handles register written events from peripherals.
    /// </summary>
    /// <param name="sender">The peripheral that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnPeripheralRegisterWritten(object? sender, PeripheralRegisterAccessEventArgs e)
    {
        RegisterWritten?.Invoke(sender, e);
    }

    /// <summary>
    /// Handles interrupt request events from peripherals.
    /// </summary>
    /// <param name="sender">The peripheral that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnPeripheralInterruptRequested(object? sender, PeripheralInterruptEventArgs e)
    {
        InterruptRequested?.Invoke(sender, e);
    }
}
