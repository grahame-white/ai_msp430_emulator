# MSP430 Status Register Bit Field Layout

## Status Register (R2/SR) Overview

The Status Register (SR) is a 16-bit register that contains processor status flags and control bits. Only the
lower 9 bits (0-8) are used for flags, with the upper bits reserved.

*Reference: MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019 -
Section 3.2.3: "Status Register (SR)" - Figure 3-2*

## Bit Field Layout

### Visual Bit Representation

```text
Status Register (R2/SR) - 16 bits
┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
│R │R │R │R │R │R │R │V │S │S │O │C │G │N │Z │C │  Bit Position
│E │E │E │E │E │E │E │  │C │C │S │P │I │  │  │  │
│S │S │S │S │S │S │S │  │G │G │C │U │E │  │  │  │  Flag Name
│E │E │E │E │E │E │E │  │1 │0 │  │  │  │  │  │  │
│R │R │R │R │R │R │R │  │  │  │  │  │  │  │  │  │
│V │V │V │V │V │V │V │  │  │  │  │  │  │  │  │  │
│E │E │E │E │E │E │E │  │  │  │  │  │  │  │  │  │
│D │D │D │D │D │D │D │  │  │  │  │  │  │  │  │  │
└──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
15 14 13 12 11 10  9  8  7  6  5  4  3  2  1  0
```

### Bit Field Table

*Reference: MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019, Section 4.3.3:
"Status Register (SR)" -
Table 3-2*

| Bit | Name | Symbol | Description | Reset Value |
|-----|------|--------|-------------|-------------|
| 15-9 | Reserved | - | Reserved for future use | 0 |
| 8 | Overflow | V | Set when arithmetic operation generates overflow | 0 |
| 7 | System Clock Generator 1 | SCG1 | Controls the system clock generator | 0 |
| 6 | System Clock Generator 0 | SCG0 | Controls the system clock generator | 0 |
| 5 | Oscillator Off | OSC | When set, turns off the LFXT1 oscillator | 0 |
| 4 | CPU Off | CPUOFF | When set, turns off the CPU | 0 |
| 3 | General Interrupt Enable | GIE | When set, enables maskable interrupts | 0 |
| 2 | Negative | N | Set when arithmetic/logical operation result is negative | 0 |
| 1 | Zero | Z | Set when arithmetic/logical operation result equals zero | 0 |
| 0 | Carry | C | Set when arithmetic operation generates carry or borrow | 0 |

## Individual Flag Details

### Arithmetic Flags (Bits 0-2, 8)

### Carry Flag (C) - Bit 0

```text
┌───────────────────────────────────────────────────────────────┐
│ Carry Flag (C) - Bit 0                                        │
├───────────────────────────────────────────────────────────────┤
│ Set when:                                                     │
│ • Addition generates a carry from bit 15                      │
│ • Subtraction requires a borrow to bit 15                     │
│ • Shift/rotate operations shift a 1 out of MSB/LSB            │
├───────────────────────────────────────────────────────────────┤
│ Mask: 0x0001                                                  │
│ Property: StatusRegister.Carry                                │
└───────────────────────────────────────────────────────────────┘
```

### Zero Flag (Z) - Bit 1

```text
┌───────────────────────────────────────────────────────────────┐
│ Zero Flag (Z) - Bit 1                                         │
├───────────────────────────────────────────────────────────────┤
│ Set when:                                                     │
│ • Arithmetic or logical operation result equals zero          │
│ • Compare operation finds equal values                        │
│ • Test operation finds all tested bits are zero               │
├───────────────────────────────────────────────────────────────┤
│ Mask: 0x0002                                                  │
│ Property: StatusRegister.Zero                                 │
└───────────────────────────────────────────────────────────────┘
```

### Negative Flag (N) - Bit 2

```text
┌───────────────────────────────────────────────────────────────┐
│ Negative Flag (N) - Bit 2                                     │
├───────────────────────────────────────────────────────────────┤
│ Set when:                                                     │
│ • Arithmetic or logical operation result is negative          │
│ • Most significant bit (bit 15) of result is 1                │
│ • Signed comparison finds first operand less than second      │
├───────────────────────────────────────────────────────────────┤
│ Mask: 0x0004                                                  │
│ Property: StatusRegister.Negative                             │
└───────────────────────────────────────────────────────────────┘
```

### Overflow Flag (V) - Bit 8

```text
┌───────────────────────────────────────────────────────────────┐
│ Overflow Flag (V) - Bit 8                                     │
├───────────────────────────────────────────────────────────────┤
│ Set when:                                                     │
│ • Signed arithmetic operation result exceeds range            │
│ • Addition of same sign operands produces opposite sign       │
│ • Subtraction causes sign change when it shouldn't            │
├───────────────────────────────────────────────────────────────┤
│ Mask: 0x0100                                                  │
│ Property: StatusRegister.Overflow                             │
└───────────────────────────────────────────────────────────────┘
```

### Control Flags (Bits 3-7)

### General Interrupt Enable (GIE) - Bit 3

```text
┌───────────────────────────────────────────────────────────────┐
│ General Interrupt Enable (GIE) - Bit 3                        │
├───────────────────────────────────────────────────────────────┤
│ When set (1):                                                 │
│ • Maskable interrupts are enabled                             │
│ • CPU will respond to interrupt requests                      │
│ When clear (0):                                               │
│ • Maskable interrupts are disabled                            │
│ • Only non-maskable interrupts are processed                  │
├───────────────────────────────────────────────────────────────┤
│ Mask: 0x0008                                                  │
│ Property: StatusRegister.GeneralInterruptEnable               │
└───────────────────────────────────────────────────────────────┘
```

### CPU Off (CPUOFF) - Bit 4

```text
┌───────────────────────────────────────────────────────────────┐
│ CPU Off (CPUOFF) - Bit 4                                      │
├───────────────────────────────────────────────────────────────┤
│ When set (1):                                                 │
│ • CPU is turned off (low power mode)                          │
│ • CPU stops executing instructions                            │
│ • System clocks continue running                              │
│ When clear (0):                                               │
│ • CPU is active and executing instructions                    │
├───────────────────────────────────────────────────────────────┤
│ Mask: 0x0010                                                  │
│ Property: StatusRegister.CpuOff                               │
└───────────────────────────────────────────────────────────────┘
```

### Oscillator Off (OSC) - Bit 5

```text
┌───────────────────────────────────────────────────────────────┐
│ Oscillator Off (OSC) - Bit 5                                  │
├───────────────────────────────────────────────────────────────┤
│ When set (1):                                                 │
│ • LFXT1 oscillator is turned off                              │
│ • Reduces power consumption                                   │
│ • Affects timer and peripheral clocks                         │
│ When clear (0):                                               │
│ • LFXT1 oscillator is active                                  │
├───────────────────────────────────────────────────────────────┤
│ Mask: 0x0020                                                  │
│ Property: StatusRegister.OscillatorOff                        │
└───────────────────────────────────────────────────────────────┘
```

### System Clock Generator 0 (SCG0) - Bit 6

```text
┌───────────────────────────────────────────────────────────────┐
│ System Clock Generator 0 (SCG0) - Bit 6                       │
├───────────────────────────────────────────────────────────────┤
│ When set (1):                                                 │
│ • Turns off SMCLK (sub-main clock)                            │
│ • Reduces power consumption                                   │
│ • Affects peripheral operation                                │
│ When clear (0):                                               │
│ • SMCLK is active                                             │
├───────────────────────────────────────────────────────────────┤
│ Mask: 0x0040                                                  │
│ Property: StatusRegister.SystemClockGenerator0                │
└───────────────────────────────────────────────────────────────┘
```

### System Clock Generator 1 (SCG1) - Bit 7

```text
┌───────────────────────────────────────────────────────────────┐
│ System Clock Generator 1 (SCG1) - Bit 7                       │
├───────────────────────────────────────────────────────────────┤
│ When set (1):                                                 │
│ • Turns off DCO (digitally controlled oscillator)             │
│ • Affects MCLK and SMCLK sources                              │
│ • Enables low power mode                                      │
│ When clear (0):                                               │
│ • DCO is active                                               │
├───────────────────────────────────────────────────────────────┤
│ Mask: 0x0080                                                  │
│ Property: StatusRegister.SystemClockGenerator1                │
└───────────────────────────────────────────────────────────────┘
```

## Flag Combinations and Power Modes

### Low Power Mode Combinations

| Mode | CPUOFF | OSC | SCG0 | SCG1 | Description |
|------|--------|-----|------|------|-------------|
| LPM0 | 1 | 0 | 0 | 0 | CPU off, all clocks active |
| LPM1 | 1 | 0 | 1 | 0 | CPU off, SMCLK off |
| LPM2 | 1 | 1 | 1 | 1 | CPU off, SMCLK off, DCO off |
| LPM3 | 1 | 1 | 1 | 1 | CPU off, all clocks off except ACLK |
| LPM4 | 1 | 1 | 1 | 1 | All clocks off (with additional control) |

### Common Flag Patterns

### After Reset

```text
Status Register = 0x0000
┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
│0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │
└──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
  All flags clear
```

### After Addition with Carry

```text
Status Register = 0x0001 (Carry set)
┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
│0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │1 │
└──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
                                              C=1
```

### After Zero Result

```text
Status Register = 0x0002 (Zero set)
┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
│0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │1 │0 │
└──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
                                           Z=1
```

### Interrupts Enabled

```text
Status Register = 0x0008 (GIE set)
┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
│0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │0 │1 │0 │0 │0 │
└──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
                                     GIE=1
```

## Programming Interface

### Bit Manipulation Methods

### Individual Flag Access

```csharp
// Reading flags
bool isCarrySet = statusRegister.Carry;
bool isZero = statusRegister.Zero;
bool interruptsEnabled = statusRegister.GeneralInterruptEnable;

// Setting flags
statusRegister.Carry = true;
statusRegister.Zero = false;
statusRegister.GeneralInterruptEnable = true;
```

### Direct Value Access

```csharp
// Read complete register value
ushort srValue = statusRegister.Value;

// Write complete register value
statusRegister.Value = 0x000F; // Set C, Z, N, GIE flags

// Reset all flags
statusRegister.Reset(); // Sets value to 0x0000
```

### Conditional Flag Updates

```csharp
// Update flags based on arithmetic result
ushort result = 0x8000;
statusRegister.UpdateFlags(result, updateCarry: true, updateOverflow: true);

// Check resulting flags
Console.WriteLine(statusRegister.ToString());
// Output: "SR: 0x0004 [N]"
```

## Flag Interaction Examples

### Arithmetic Operation Flag Updates

```csharp
// Example: Add with carry detection
ushort a = 0xFFFF;
ushort b = 0x0001;
uint result = (uint)a + (uint)b;

// Update flags
statusRegister.Carry = (result & 0x10000) != 0;  // Carry out of bit 15
statusRegister.Zero = (result & 0xFFFF) == 0;    // Result is zero
statusRegister.Negative = (result & 0x8000) != 0; // MSB set
// Result: C=1, Z=1, N=0 (0xFFFF + 0x0001 = 0x0000 with carry)
```

### Power Mode Configuration

```csharp
// Enter LPM0 (CPU off, clocks active)
statusRegister.CpuOff = true;
// Other flags remain unchanged

// Enter LPM3 (most clocks off)
statusRegister.CpuOff = true;
statusRegister.OscillatorOff = true;
statusRegister.SystemClockGenerator0 = true;
statusRegister.SystemClockGenerator1 = true;

```
