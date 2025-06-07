using System;
using System.Collections.Generic;

namespace MSP430.Emulator.Memory;

/// <summary>
/// Defines the contract for the unified memory controller in the MSP430 emulator.
/// 
/// The memory controller provides a single point of access for all memory operations,
/// handling address decoding, routing to appropriate memory components, access arbitration,
/// and memory-mapped peripheral support.
/// 
/// This interface integrates RAM, Flash, and Information memory into a cohesive system
/// while maintaining proper access control and timing characteristics.
/// 
/// Implementation based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 1.9.1:
/// "Memory Map" - Memory controller and address space organization.
/// </summary>
public interface IMemoryController
{
    /// <summary>
    /// Gets the memory map used for address decoding and validation.
    /// </summary>
    IMemoryMap MemoryMap { get; }

    /// <summary>
    /// Gets the RAM component managed by this controller.
    /// </summary>
    IRandomAccessMemory Ram { get; }

    /// <summary>
    /// Gets the Flash memory component managed by this controller.
    /// </summary>
    IFlashMemory Flash { get; }

    /// <summary>
    /// Gets the Information memory component managed by this controller.
    /// </summary>
    IInformationMemory InformationMemory { get; }

    /// <summary>
    /// Gets a value indicating whether any memory operation is currently in progress.
    /// This includes long-running operations like flash programming or erasing.
    /// </summary>
    bool IsOperationInProgress { get; }

    /// <summary>
    /// Gets statistics about memory access operations.
    /// </summary>
    MemoryStatistics Statistics { get; }

    /// <summary>
    /// Reads a byte from the specified address.
    /// Automatically routes the request to the appropriate memory component.
    /// </summary>
    /// <param name="address">The memory address to read from.</param>
    /// <returns>The byte value at the specified address.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is invalid or access is denied.</exception>
    byte ReadByte(ushort address);

    /// <summary>
    /// Reads a byte from the specified address with context information.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <returns>The byte value at the specified address.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is invalid or access is denied.</exception>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    byte ReadByte(MemoryAccessContext context);

    /// <summary>
    /// Reads a 16-bit word from the specified address.
    /// Uses little-endian byte ordering (low byte at address, high byte at address+1).
    /// Automatically routes the request to the appropriate memory component.
    /// </summary>
    /// <param name="address">The memory address to read from (must be word-aligned).</param>
    /// <returns>The 16-bit word value at the specified address.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is invalid or access is denied.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is not word-aligned.</exception>
    ushort ReadWord(ushort address);

    /// <summary>
    /// Reads a 16-bit word from the specified address with context information.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <returns>The 16-bit word value at the specified address.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is invalid or access is denied.</exception>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is not word-aligned.</exception>
    ushort ReadWord(MemoryAccessContext context);

    /// <summary>
    /// Writes a byte to the specified address.
    /// Automatically routes the request to the appropriate memory component.
    /// </summary>
    /// <param name="address">The memory address to write to.</param>
    /// <param name="value">The byte value to write.</param>
    /// <returns>True if the write was successful, false if blocked by protection or controller state.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is invalid or access is denied.</exception>
    bool WriteByte(ushort address, byte value);

    /// <summary>
    /// Writes a byte to the specified address with context information.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <param name="value">The byte value to write.</param>
    /// <returns>True if the write was successful, false if blocked by protection or controller state.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is invalid or access is denied.</exception>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    bool WriteByte(MemoryAccessContext context, byte value);

    /// <summary>
    /// Writes a 16-bit word to the specified address.
    /// Uses little-endian byte ordering (low byte at address, high byte at address+1).
    /// Automatically routes the request to the appropriate memory component.
    /// </summary>
    /// <param name="address">The memory address to write to (must be word-aligned).</param>
    /// <param name="value">The 16-bit word value to write.</param>
    /// <returns>True if the write was successful, false if blocked by protection or controller state.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is invalid or access is denied.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is not word-aligned.</exception>
    bool WriteWord(ushort address, ushort value);

    /// <summary>
    /// Writes a 16-bit word to the specified address with context information.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <param name="value">The 16-bit word value to write.</param>
    /// <returns>True if the write was successful, false if blocked by protection or controller state.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is invalid or access is denied.</exception>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the address is not word-aligned.</exception>
    bool WriteWord(MemoryAccessContext context, ushort value);

    /// <summary>
    /// Fetches an instruction word from the specified address.
    /// This is used by the CPU for instruction fetch operations and may have
    /// different timing characteristics than normal read operations.
    /// </summary>
    /// <param name="address">The address to fetch the instruction from (typically word-aligned).</param>
    /// <returns>The instruction word at the specified address.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is invalid or execute access is denied.</exception>
    ushort FetchInstruction(ushort address);

    /// <summary>
    /// Fetches an instruction word with context information.
    /// </summary>
    /// <param name="context">The memory access context for the instruction fetch.</param>
    /// <returns>The instruction word at the specified address.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is invalid or execute access is denied.</exception>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    ushort FetchInstruction(MemoryAccessContext context);

    /// <summary>
    /// Validates that the specified memory access is allowed.
    /// </summary>
    /// <param name="context">The memory access context to validate.</param>
    /// <exception cref="MemoryAccessException">Thrown when the access is not allowed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    void ValidateAccess(MemoryAccessContext context);

    /// <summary>
    /// Determines the memory region that contains the specified address.
    /// </summary>
    /// <param name="address">The memory address to look up.</param>
    /// <returns>The memory region information.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is not mapped to any region.</exception>
    MemoryRegionInfo GetRegion(ushort address);

    /// <summary>
    /// Gets the number of CPU cycles required for the specified memory access.
    /// Different memory types and operations have different timing characteristics.
    /// </summary>
    /// <param name="context">The memory access context.</param>
    /// <returns>The number of CPU cycles required for the access.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    uint GetAccessCycles(MemoryAccessContext context);

    /// <summary>
    /// Determines if the specified address is valid (mapped to a memory region).
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>True if the address is valid, false otherwise.</returns>
    bool IsValidAddress(ushort address);

    /// <summary>
    /// Determines if the specified access type is allowed for the given address.
    /// </summary>
    /// <param name="address">The memory address.</param>
    /// <param name="accessType">The type of access requested.</param>
    /// <returns>True if the access is allowed, false otherwise.</returns>
    bool IsAccessAllowed(ushort address, MemoryAccessPermissions accessType);

    /// <summary>
    /// Gets the access permissions for the specified address.
    /// </summary>
    /// <param name="address">The memory address.</param>
    /// <returns>The access permissions for the address.</returns>
    /// <exception cref="MemoryAccessException">Thrown when the address is not mapped to any region.</exception>
    MemoryAccessPermissions GetPermissions(ushort address);

    /// <summary>
    /// Resets all memory components to their initial state.
    /// This includes clearing RAM, resetting flash controller state, and
    /// restoring information memory to default values where appropriate.
    /// </summary>
    void Reset();

    /// <summary>
    /// Event raised when a memory access operation is performed.
    /// This can be used for debugging, tracing, and performance monitoring.
    /// </summary>
    event EventHandler<MemoryAccessEventArgs>? MemoryAccessed;

    /// <summary>
    /// Event raised when a memory access violation occurs.
    /// This includes attempts to access invalid addresses or perform
    /// unauthorized operations.
    /// </summary>
    event EventHandler<MemoryAccessViolationEventArgs>? AccessViolation;
}
