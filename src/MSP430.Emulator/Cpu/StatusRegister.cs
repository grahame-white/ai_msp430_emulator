using System.Collections.Generic;

namespace MSP430.Emulator.Cpu;

/// <summary>
/// Represents the MSP430 Status Register (SR/R2) with individual flag management.
/// 
/// The Status Register contains various flags that indicate the processor state
/// and control certain operations.
/// 
/// Implementation based on MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019,
/// Section 4.3.3: "Status Register (SR)" - Figure 4-9 and Table 4-1.
/// </summary>
public class StatusRegister
{
    /// <summary>
    /// Bit mask for the Carry flag (bit 0).
    /// </summary>
    public const ushort CarryMask = 0x0001;

    /// <summary>
    /// Bit mask for the Zero flag (bit 1).
    /// </summary>
    public const ushort ZeroMask = 0x0002;

    /// <summary>
    /// Bit mask for the Negative flag (bit 2).
    /// </summary>
    public const ushort NegativeMask = 0x0004;

    /// <summary>
    /// Bit mask for the General Interrupt Enable flag (bit 3).
    /// </summary>
    public const ushort GeneralInterruptEnableMask = 0x0008;

    /// <summary>
    /// Bit mask for the CPU Off flag (bit 4).
    /// </summary>
    public const ushort CpuOffMask = 0x0010;

    /// <summary>
    /// Bit mask for the Oscillator Off flag (bit 5).
    /// </summary>
    public const ushort OscillatorOffMask = 0x0020;

    /// <summary>
    /// Bit mask for the System Clock Generator 0 flag (bit 6).
    /// </summary>
    public const ushort SystemClockGenerator0Mask = 0x0040;

    /// <summary>
    /// Bit mask for the System Clock Generator 1 flag (bit 7).
    /// </summary>
    public const ushort SystemClockGenerator1Mask = 0x0080;

    /// <summary>
    /// Bit mask for the Overflow flag (bit 8).
    /// </summary>
    public const ushort OverflowMask = 0x0100;

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
        get => (_value & CarryMask) != 0;
        set => _value = value ? (ushort)(_value | CarryMask) : (ushort)(_value & ~CarryMask);
    }

    /// <summary>
    /// Gets or sets the Zero flag (bit 1).
    /// Set when an arithmetic or logical operation result equals zero.
    /// </summary>
    public bool Zero
    {
        get => (_value & ZeroMask) != 0;
        set => _value = value ? (ushort)(_value | ZeroMask) : (ushort)(_value & ~ZeroMask);
    }

    /// <summary>
    /// Gets or sets the Negative flag (bit 2).
    /// Set when an arithmetic or logical operation result is negative.
    /// </summary>
    public bool Negative
    {
        get => (_value & NegativeMask) != 0;
        set => _value = value ? (ushort)(_value | NegativeMask) : (ushort)(_value & ~NegativeMask);
    }

    /// <summary>
    /// Gets or sets the General Interrupt Enable flag (bit 3).
    /// When set, enables maskable interrupts.
    /// </summary>
    public bool GeneralInterruptEnable
    {
        get => (_value & GeneralInterruptEnableMask) != 0;
        set => _value = value ? (ushort)(_value | GeneralInterruptEnableMask) : (ushort)(_value & ~GeneralInterruptEnableMask);
    }

    /// <summary>
    /// Gets or sets the CPU Off flag (bit 4).
    /// When set, turns off the CPU.
    /// </summary>
    public bool CpuOff
    {
        get => (_value & CpuOffMask) != 0;
        set => _value = value ? (ushort)(_value | CpuOffMask) : (ushort)(_value & ~CpuOffMask);
    }

    /// <summary>
    /// Gets or sets the Oscillator Off flag (bit 5).
    /// When set, turns off the LFXT1 oscillator.
    /// </summary>
    public bool OscillatorOff
    {
        get => (_value & OscillatorOffMask) != 0;
        set => _value = value ? (ushort)(_value | OscillatorOffMask) : (ushort)(_value & ~OscillatorOffMask);
    }

    /// <summary>
    /// Gets or sets the System Clock Generator 0 flag (bit 6).
    /// Controls the system clock generator.
    /// </summary>
    public bool SystemClockGenerator0
    {
        get => (_value & SystemClockGenerator0Mask) != 0;
        set => _value = value ? (ushort)(_value | SystemClockGenerator0Mask) : (ushort)(_value & ~SystemClockGenerator0Mask);
    }

    /// <summary>
    /// Gets or sets the System Clock Generator 1 flag (bit 7).
    /// Controls the system clock generator.
    /// </summary>
    public bool SystemClockGenerator1
    {
        get => (_value & SystemClockGenerator1Mask) != 0;
        set => _value = value ? (ushort)(_value | SystemClockGenerator1Mask) : (ushort)(_value & ~SystemClockGenerator1Mask);
    }

    /// <summary>
    /// Gets or sets the Overflow flag (bit 8).
    /// Set when an arithmetic operation generates an overflow.
    /// </summary>
    public bool Overflow
    {
        get => (_value & OverflowMask) != 0;
        set => _value = value ? (ushort)(_value | OverflowMask) : (ushort)(_value & ~OverflowMask);
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
    /// Updates flags after an addition operation.
    /// </summary>
    /// <param name="operand1">The first operand (destination).</param>
    /// <param name="operand2">The second operand (source).</param>
    /// <param name="result">The result of the addition.</param>
    /// <param name="isByteOperation">True if this is a byte operation.</param>
    public void UpdateFlagsAfterAddition(ushort operand1, ushort operand2, uint result, bool isByteOperation)
    {
        if (isByteOperation)
        {
            byte byteResult = (byte)(result & 0xFF);
            Zero = byteResult == 0;
            Negative = (byteResult & 0x80) != 0;
            Carry = (result & 0x100) != 0;

            // Overflow occurs when adding two positive numbers gives negative result
            // or adding two negative numbers gives positive result
            bool op1Sign = (operand1 & 0x80) != 0;
            bool op2Sign = (operand2 & 0x80) != 0;
            bool resultSign = (byteResult & 0x80) != 0;
            Overflow = (op1Sign == op2Sign) && (op1Sign != resultSign);
        }
        else
        {
            ushort wordResult = (ushort)(result & 0xFFFF);
            Zero = wordResult == 0;
            Negative = (wordResult & 0x8000) != 0;
            Carry = (result & 0x10000) != 0;

            // Overflow occurs when adding two positive numbers gives negative result
            // or adding two negative numbers gives positive result
            bool op1Sign = (operand1 & 0x8000) != 0;
            bool op2Sign = (operand2 & 0x8000) != 0;
            bool resultSign = (wordResult & 0x8000) != 0;
            Overflow = (op1Sign == op2Sign) && (op1Sign != resultSign);
        }
    }

    /// <summary>
    /// Updates flags after a subtraction operation.
    /// </summary>
    /// <param name="operand1">The first operand (destination).</param>
    /// <param name="operand2">The second operand (source).</param>
    /// <param name="result">The result of the subtraction.</param>
    /// <param name="isByteOperation">True if this is a byte operation.</param>
    public void UpdateFlagsAfterSubtraction(ushort operand1, ushort operand2, uint result, bool isByteOperation)
    {
        if (isByteOperation)
        {
            byte byteResult = (byte)(result & 0xFF);
            Zero = byteResult == 0;
            Negative = (byteResult & 0x80) != 0;
            // For subtraction, carry is set when no borrow is required (result >= 0)
            Carry = operand1 >= operand2;

            // Overflow occurs when subtracting negative from positive gives negative result
            // or subtracting positive from negative gives positive result
            bool op1Sign = (operand1 & 0x80) != 0;
            bool op2Sign = (operand2 & 0x80) != 0;
            bool resultSign = (byteResult & 0x80) != 0;
            Overflow = (op1Sign != op2Sign) && (op1Sign != resultSign);
        }
        else
        {
            ushort wordResult = (ushort)(result & 0xFFFF);
            Zero = wordResult == 0;
            Negative = (wordResult & 0x8000) != 0;
            // For subtraction, carry is set when no borrow is required (result >= 0)
            Carry = operand1 >= operand2;

            // Overflow occurs when subtracting negative from positive gives negative result
            // or subtracting positive from negative gives positive result
            bool op1Sign = (operand1 & 0x8000) != 0;
            bool op2Sign = (operand2 & 0x8000) != 0;
            bool resultSign = (wordResult & 0x8000) != 0;
            Overflow = (op1Sign != op2Sign) && (op1Sign != resultSign);
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
