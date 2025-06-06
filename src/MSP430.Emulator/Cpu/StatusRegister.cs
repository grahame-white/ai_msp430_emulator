using System.Collections.Generic;

namespace MSP430.Emulator.Cpu;

/// <summary>
/// Represents the MSP430 Status Register (SR/R2) with individual flag management.
/// 
/// The Status Register contains various flags that indicate the processor state
/// and control certain operations.
/// </summary>
public class StatusRegister
{
    private ushort _value;

    /// <summary>
    /// Gets or sets the complete 16-bit value of the status register.
    /// </summary>
    public ushort Value
    {
        get => _value;
        set => _value = value;
    }

    /// <summary>
    /// Gets or sets the Carry flag (bit 0).
    /// Set when an arithmetic operation generates a carry or borrow.
    /// </summary>
    public bool Carry
    {
        get => (_value & 0x0001) != 0;
        set => _value = value ? (ushort)(_value | 0x0001) : (ushort)(_value & ~0x0001);
    }

    /// <summary>
    /// Gets or sets the Zero flag (bit 1).
    /// Set when an arithmetic or logical operation result equals zero.
    /// </summary>
    public bool Zero
    {
        get => (_value & 0x0002) != 0;
        set => _value = value ? (ushort)(_value | 0x0002) : (ushort)(_value & ~0x0002);
    }

    /// <summary>
    /// Gets or sets the Negative flag (bit 2).
    /// Set when an arithmetic or logical operation result is negative.
    /// </summary>
    public bool Negative
    {
        get => (_value & 0x0004) != 0;
        set => _value = value ? (ushort)(_value | 0x0004) : (ushort)(_value & ~0x0004);
    }

    /// <summary>
    /// Gets or sets the General Interrupt Enable flag (bit 3).
    /// When set, enables maskable interrupts.
    /// </summary>
    public bool GeneralInterruptEnable
    {
        get => (_value & 0x0008) != 0;
        set => _value = value ? (ushort)(_value | 0x0008) : (ushort)(_value & ~0x0008);
    }

    /// <summary>
    /// Gets or sets the CPU Off flag (bit 4).
    /// When set, turns off the CPU.
    /// </summary>
    public bool CpuOff
    {
        get => (_value & 0x0010) != 0;
        set => _value = value ? (ushort)(_value | 0x0010) : (ushort)(_value & ~0x0010);
    }

    /// <summary>
    /// Gets or sets the Oscillator Off flag (bit 5).
    /// When set, turns off the LFXT1 oscillator.
    /// </summary>
    public bool OscillatorOff
    {
        get => (_value & 0x0020) != 0;
        set => _value = value ? (ushort)(_value | 0x0020) : (ushort)(_value & ~0x0020);
    }

    /// <summary>
    /// Gets or sets the System Clock Generator 0 flag (bit 6).
    /// Controls the system clock generator.
    /// </summary>
    public bool SystemClockGenerator0
    {
        get => (_value & 0x0040) != 0;
        set => _value = value ? (ushort)(_value | 0x0040) : (ushort)(_value & ~0x0040);
    }

    /// <summary>
    /// Gets or sets the System Clock Generator 1 flag (bit 7).
    /// Controls the system clock generator.
    /// </summary>
    public bool SystemClockGenerator1
    {
        get => (_value & 0x0080) != 0;
        set => _value = value ? (ushort)(_value | 0x0080) : (ushort)(_value & ~0x0080);
    }

    /// <summary>
    /// Gets or sets the Overflow flag (bit 8).
    /// Set when an arithmetic operation generates an overflow.
    /// </summary>
    public bool Overflow
    {
        get => (_value & 0x0100) != 0;
        set => _value = value ? (ushort)(_value | 0x0100) : (ushort)(_value & ~0x0100);
    }

    /// <summary>
    /// Initializes a new instance of the StatusRegister class.
    /// </summary>
    /// <param name="initialValue">The initial value of the status register (defaults to 0).</param>
    public StatusRegister(ushort initialValue = 0)
    {
        _value = initialValue;
    }

    /// <summary>
    /// Resets all flags to their default state (0).
    /// </summary>
    public void Reset()
    {
        _value = 0;
    }

    /// <summary>
    /// Updates flags based on an arithmetic or logical operation result.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    /// <param name="updateCarry">Whether to update the carry flag.</param>
    /// <param name="updateOverflow">Whether to update the overflow flag.</param>
    public void UpdateFlags(ushort result, bool updateCarry = false, bool updateOverflow = false)
    {
        Zero = result == 0;
        Negative = (result & 0x8000) != 0;

        if (updateCarry)
        {
            // Note: Carry logic would need to be implemented based on the specific operation
            // This is a placeholder for the interface
        }

        if (updateOverflow)
        {
            // Note: Overflow logic would need to be implemented based on the specific operation
            // This is a placeholder for the interface
        }
    }

    /// <summary>
    /// Returns a string representation of the status register showing active flags.
    /// </summary>
    /// <returns>A formatted string showing the active flags.</returns>
    public override string ToString()
    {
        var flags = new List<string>();

        if (Carry)
        {
            flags.Add("C");
        }

        if (Zero)
        {
            flags.Add("Z");
        }

        if (Negative)
        {
            flags.Add("N");
        }

        if (GeneralInterruptEnable)
        {
            flags.Add("GIE");
        }

        if (CpuOff)
        {
            flags.Add("CPU_OFF");
        }

        if (OscillatorOff)
        {
            flags.Add("OSC_OFF");
        }

        if (SystemClockGenerator0)
        {
            flags.Add("SCG0");
        }

        if (SystemClockGenerator1)
        {
            flags.Add("SCG1");
        }

        if (Overflow)
        {
            flags.Add("V");
        }

        return $"SR: 0x{_value:X4} [{string.Join(", ", flags)}]";
    }
}
