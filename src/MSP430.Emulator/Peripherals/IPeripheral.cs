using System;

namespace MSP430.Emulator.Peripherals;

/// <summary>
/// Defines the contract for peripheral devices in the MSP430 emulator.
/// 
/// Peripherals are memory-mapped devices that interact with the CPU through
/// special function registers (SFRs) and peripheral registers. This interface
/// provides the foundation for implementing MSP430FR2355 peripherals including
/// digital I/O ports, timers, communication interfaces, and analog modules.
/// 
/// Implementation based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - Section 1.4:
/// "System Resets, Interrupts, and Operating Modes" and peripheral-specific chapters.
/// </summary>
public interface IPeripheral
{
    /// <summary>
    /// Gets the unique identifier for this peripheral type.
    /// </summary>
    string PeripheralId { get; }

    /// <summary>
    /// Gets the base address of this peripheral's register space.
    /// </summary>
    ushort BaseAddress { get; }

    /// <summary>
    /// Gets the size of this peripheral's register space in bytes.
    /// </summary>
    ushort AddressSpaceSize { get; }

    /// <summary>
    /// Gets the ending address of this peripheral's register space (inclusive).
    /// </summary>
    ushort EndAddress { get; }

    /// <summary>
    /// Gets a value indicating whether this peripheral is currently enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Determines if the specified address falls within this peripheral's address space.
    /// </summary>
    /// <param name="address">The memory address to check.</param>
    /// <returns>True if the address is handled by this peripheral, false otherwise.</returns>
    bool HandlesAddress(ushort address);

    /// <summary>
    /// Reads a byte value from the specified peripheral register address.
    /// </summary>
    /// <param name="address">The register address to read from.</param>
    /// <returns>The byte value at the specified register address.</returns>
    /// <exception cref="ArgumentException">Thrown when the address is not handled by this peripheral.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the register is write-only or peripheral is disabled.</exception>
    byte ReadByte(ushort address);

    /// <summary>
    /// Writes a byte value to the specified peripheral register address.
    /// </summary>
    /// <param name="address">The register address to write to.</param>
    /// <param name="value">The byte value to write.</param>
    /// <returns>True if the write was successful, false if blocked by register protection or peripheral state.</returns>
    /// <exception cref="ArgumentException">Thrown when the address is not handled by this peripheral.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the register is read-only or peripheral is disabled.</exception>
    bool WriteByte(ushort address, byte value);

    /// <summary>
    /// Reads a 16-bit word value from the specified peripheral register address.
    /// Uses little-endian byte ordering (low byte at address, high byte at address+1).
    /// </summary>
    /// <param name="address">The register address to read from (must be word-aligned).</param>
    /// <returns>The 16-bit word value at the specified register address.</returns>
    /// <exception cref="ArgumentException">Thrown when the address is not handled by this peripheral or not word-aligned.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the register is write-only or peripheral is disabled.</exception>
    ushort ReadWord(ushort address);

    /// <summary>
    /// Writes a 16-bit word value to the specified peripheral register address.
    /// Uses little-endian byte ordering (low byte at address, high byte at address+1).
    /// </summary>
    /// <param name="address">The register address to write to (must be word-aligned).</param>
    /// <param name="value">The 16-bit word value to write.</param>
    /// <returns>True if the write was successful, false if blocked by register protection or peripheral state.</returns>
    /// <exception cref="ArgumentException">Thrown when the address is not handled by this peripheral or not word-aligned.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the register is read-only or peripheral is disabled.</exception>
    bool WriteWord(ushort address, ushort value);

    /// <summary>
    /// Resets the peripheral to its power-on reset state.
    /// This includes resetting all registers to their default values and clearing any pending operations.
    /// </summary>
    void Reset();

    /// <summary>
    /// Initializes the peripheral with optional configuration data.
    /// Called after reset to set up peripheral-specific initialization.
    /// </summary>
    /// <param name="initializationData">Optional peripheral-specific initialization data.</param>
    void Initialize(object? initializationData = null);

    /// <summary>
    /// Event raised when a peripheral register is read.
    /// This can be used for debugging, tracing, and implementing side-effects of register access.
    /// </summary>
    event EventHandler<PeripheralRegisterAccessEventArgs>? RegisterRead;

    /// <summary>
    /// Event raised when a peripheral register is written.
    /// This can be used for debugging, tracing, and implementing side-effects of register access.
    /// </summary>
    event EventHandler<PeripheralRegisterAccessEventArgs>? RegisterWritten;

    /// <summary>
    /// Event raised when the peripheral generates an interrupt request.
    /// </summary>
    event EventHandler<PeripheralInterruptEventArgs>? InterruptRequested;
}
