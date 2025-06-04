# MSP430 Register File Layout

## Register File Organization

The MSP430 CPU contains 16 16-bit registers organized as follows according to the
MSP430FR2xx/FR4xx Family User's Guide (SLAU445I, December 2016 - Revised December 2020) -
Section 3.2: "CPU Registers" - Figure 3-1.

### Register File Layout

*Reference: MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3.2.1: "Program Counter (PC)" and
Section 3.2.2: "Stack Pointer (SP)"*

```text
┌────────────────────────────────────────────────────────────────┐
│                    MSP430 Register File                       │
│                     (16 x 16-bit)                             │
├────────────┬────────┬───────────────────────┬─────────────────┤
│  Register  │  Name  │      Function         │   Special        │
│   Number   │        │                       │  Behavior        │
├────────────┼────────┼───────────────────────┼─────────────────┤
│    R0      │   PC   │ Program Counter       │ Word Aligned     │
│    R1      │   SP   │ Stack Pointer         │ Word Aligned     │
│    R2      │   SR   │ Status Register       │ Flag Management  │
│    R3      │  CG1   │ Constant Generator #1 │ Read-Only*       │
│    R4      │   -    │ General Purpose       │ Read/Write       │
│    R5      │   -    │ General Purpose       │ Read/Write       │
│    R6      │   -    │ General Purpose       │ Read/Write       │
│    R7      │   -    │ General Purpose       │ Read/Write       │
│    R8      │   -    │ General Purpose       │ Read/Write       │
│    R9      │   -    │ General Purpose       │ Read/Write       │
│    R10     │   -    │ General Purpose       │ Read/Write       │
│    R11     │   -    │ General Purpose       │ Read/Write       │
│    R12     │   -    │ General Purpose       │ Read/Write       │
│    R13     │   -    │ General Purpose       │ Read/Write       │
│    R14     │   -    │ General Purpose       │ Read/Write       │
│    R15     │   -    │ General Purpose       │ Read/Write       │
└────────────┴────────┴───────────────────────┴─────────────────┘

*CG1 is typically read-only but writes are allowed for testing

## Register Access Modes

### 16-bit Access (Full Register)

| Operation | Method | Description |
|-----------|--------|-------------|
| Read | `ReadRegister(RegisterName)` | Returns full 16-bit value |
| Write | `WriteRegister(RegisterName, ushort)` | Writes full 16-bit value |

### 8-bit Access (Byte Operations)

| Access Type | Method | Bits Affected | Description |
|-------------|--------|---------------|-------------|
| Low Byte Read | `ReadRegisterLowByte(RegisterName)` | 0-7 | Returns bits 0-7 |
| High Byte Read | `ReadRegisterHighByte(RegisterName)` | 8-15 | Returns bits 8-15 |
| Low Byte Write | `WriteRegisterLowByte(RegisterName, byte)` | 0-7 | Modifies bits 0-7 only |
| High Byte Write | `WriteRegisterHighByte(RegisterName, byte)` | 8-15 | Modifies bits 8-15 only |

## Special Register Organization

*Reference: MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3.2: "CPU Registers" - Table 3-1*

### Program Counter (R0/PC)

*Reference: MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3.2.1: "Program Counter (PC)"*

```text
┌───────────────────────────────────────────────────────────────┐
│                    R0 (Program Counter)                       │
├───────────────────────────────────────────────────────────────┤
│  15 14 13 12 11 10  9  8  7  6  5  4  3  2  1  0             │
│ ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐            │
│ │  │  │  │  │  │  │  │  │  │  │  │  │  │  │  │0 │ ← Always 0 │
│ └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘            │
│                Word Address (Bit 0 = 0)                       │
└───────────────────────────────────────────────────────────────┘

### Stack Pointer (R1/SP)

*Reference: MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3.2.2: "Stack Pointer (SP)"*

```text
┌───────────────────────────────────────────────────────────────┐
│                    R1 (Stack Pointer)                        │
├───────────────────────────────────────────────────────────────┤
│  15 14 13 12 11 10  9  8  7  6  5  4  3  2  1  0             │
│ ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐            │
│ │  │  │  │  │  │  │  │  │  │  │  │  │  │  │  │0 │ ← Always 0 │
│ └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘            │
│                Stack Address (Bit 0 = 0)                      │
└───────────────────────────────────────────────────────────────┘

### Status Register (R2/SR)

*Reference: MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3.2.3: "Status Register (SR)"*

```text
┌───────────────────────────────────────────────────────────────┐
│                    R2 (Status Register)                       │
├───────────────────────────────────────────────────────────────┤
│  15 14 13 12 11 10  9  8  7  6  5  4  3  2  1  0             │
│ ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐            │
│ │  │  │  │  │  │  │  │V │S │S │O │C │G │N │Z │C │            │
│ │  │  │  │  │  │  │  │  │C │C │S │P │I │  │  │  │            │
│ │  │  │  │  │  │  │  │  │G │G │C │U │E │  │  │  │            │
│ │  │  │  │  │  │  │  │  │1 │0 │  │  │  │  │  │  │            │
│ └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘            │
│   Reserved                │Individual Flag Control│           │
└───────────────────────────────────────────────────────────────┘

### Constant Generator (R3/CG1)

*Reference: MSP430FR2xx/FR4xx Family User's Guide (SLAU445I) - Section 3.2.4: "Constant Generator Registers (CG1 and CG2)"*

```text
┌───────────────────────────────────────────────────────────────┐
│                    R3 (Constant Generator)                    │
├───────────────────────────────────────────────────────────────┤
│ Addressing Mode Dependent Constants:                          │
│ - 00: Register mode (read stored value)                       │
│ - 01: Indexed mode (generates 0)                              │
│ - 10: Indirect mode (generates +1)                            │
│ - 11: Indirect auto-increment (generates +2)                  │
└───────────────────────────────────────────────────────────────┘

## Register File Memory Map

### Physical Layout

```text
Memory Address Space: Register File Internal
┌──────────────────┬──────────────────┬─────────────────┐
│   Internal       │     Register     │    Physical     │
│   Index          │     Name         │    Usage        │
├──────────────────┼──────────────────┼─────────────────┤
│   [0]            │   R0 (PC)        │   Word Aligned  │
│   [1]            │   R1 (SP)        │   Word Aligned  │
│   [2]            │   R2 (SR)        │   Flag Control  │
│   [3]            │   R3 (CG1)       │   Constants     │
│   [4]            │   R4             │   General Use   │
│   [5]            │   R5             │   General Use   │
│   [6]            │   R6             │   General Use   │
│   [7]            │   R7             │   General Use   │
│   [8]            │   R8             │   General Use   │
│   [9]            │   R9             │   General Use   │
│   [10]           │   R10            │   General Use   │
│   [11]           │   R11            │   General Use   │
│   [12]           │   R12            │   General Use   │
│   [13]           │   R13            │   General Use   │
│   [14]           │   R14            │   General Use   │
│   [15]           │   R15            │   General Use   │
└──────────────────┴──────────────────┴─────────────────┘

## Register Access Validation

### Valid Register Range
- **Valid**: R0 through R15 (RegisterName enum values 0-15)
- **Invalid**: Any value outside 0-15 range
- **Validation**: Automatic validation with `ArgumentException` for invalid registers

### Access Logging
- **Debug Level**: Individual register read/write operations
- **Info Level**: Reset operations and major state changes
- **Error Level**: Invalid register access attempts

## Usage Examples

### Basic Register Operations

```csharp
var registerFile = new RegisterFile(logger);

// Write to general purpose register
registerFile.WriteRegister(RegisterName.R4, 0x1234);
ushort value = registerFile.ReadRegister(RegisterName.R4);

// Byte-level access
registerFile.WriteRegisterLowByte(RegisterName.R5, 0xAB);
byte lowByte = registerFile.ReadRegisterLowByte(RegisterName.R5);
```

### Special Register Operations

```csharp
// Program Counter operations
registerFile.SetProgramCounter(0x8000);
registerFile.IncrementProgramCounter(); // Auto-increments by 2
ushort pc = registerFile.GetProgramCounter();

// Stack Pointer operations  
registerFile.SetStackPointer(0x27FE);
ushort sp = registerFile.GetStackPointer();

// Status Register operations
registerFile.StatusRegister.Carry = true;
registerFile.StatusRegister.Zero = true;

```
