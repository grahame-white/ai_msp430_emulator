using System;
using MSP430.Emulator.Memory;

namespace MSP430.Emulator.Tests.TestUtilities;

/// <summary>
/// Shared utilities for testing memory exception patterns across different memory implementations.
/// </summary>
public static class MemoryTestHelpers
{
    /// <summary>
    /// Validates that ReadByte throws ArgumentOutOfRangeException for invalid addresses.
    /// </summary>
    /// <param name="memory">The memory instance to test</param>
    /// <param name="invalidAddress">Invalid address that should throw exception</param>
    public static void ValidateReadByteThrowsForInvalidAddress(dynamic memory, ushort invalidAddress)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => memory.ReadByte(invalidAddress));
    }

    /// <summary>
    /// Validates that WriteByte throws ArgumentOutOfRangeException for invalid addresses.
    /// </summary>
    /// <param name="memory">The memory instance to test</param>
    /// <param name="invalidAddress">Invalid address that should throw exception</param>
    /// <param name="value">Value to attempt to write</param>
    public static void ValidateWriteByteThrowsForInvalidAddress(dynamic memory, ushort invalidAddress, byte value = 0xFF)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => { memory.WriteByte(invalidAddress, value); });
    }

    /// <summary>
    /// Validates that ReadWord throws ArgumentException for unaligned addresses.
    /// </summary>
    /// <param name="memory">The memory instance to test</param>
    /// <param name="unalignedAddress">Unaligned address that should throw exception</param>
    public static void ValidateReadWordThrowsForUnalignedAddress(dynamic memory, ushort unalignedAddress)
    {
        Assert.Throws<ArgumentException>(() => memory.ReadWord(unalignedAddress));
    }

    /// <summary>
    /// Validates that WriteWord throws ArgumentException for unaligned addresses.
    /// </summary>
    /// <param name="memory">The memory instance to test</param>
    /// <param name="unalignedAddress">Unaligned address that should throw exception</param>
    /// <param name="value">Value to attempt to write</param>
    public static void ValidateWriteWordThrowsForUnalignedAddress(dynamic memory, ushort unalignedAddress, ushort value = 0x1234)
    {
        Assert.Throws<ArgumentException>(() => { memory.WriteWord(unalignedAddress, value); });
    }

    /// <summary>
    /// Validates that ReadWord throws ArgumentOutOfRangeException for invalid addresses.
    /// </summary>
    /// <param name="memory">The memory instance to test</param>
    /// <param name="invalidAddress">Invalid address that should throw exception</param>
    public static void ValidateReadWordThrowsForInvalidAddress(dynamic memory, ushort invalidAddress)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => memory.ReadWord(invalidAddress));
    }

    /// <summary>
    /// Validates that WriteWord throws ArgumentOutOfRangeException for invalid addresses.
    /// </summary>
    /// <param name="memory">The memory instance to test</param>
    /// <param name="invalidAddress">Invalid address that should throw exception</param>
    /// <param name="value">Value to attempt to write</param>
    public static void ValidateWriteWordThrowsForInvalidAddress(dynamic memory, ushort invalidAddress, ushort value = 0x1234)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => { memory.WriteWord(invalidAddress, value); });
    }
}
