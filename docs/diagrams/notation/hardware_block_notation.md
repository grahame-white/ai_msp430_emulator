# Hardware Block Notation System

This document defines the notation system used to describe hardware block diagrams in the MSP430 emulator
documentation. This notation was developed to represent complex digital logic systems in a text-based format
that is both human-readable and suitable for version control.

## Purpose and Scope

This notation system enables:

- Clear representation of digital hardware blocks and their interconnections
- Version-controllable documentation of complex logic diagrams
- Consistent documentation across different hardware modules
- Easy translation to visual formats (Mermaid, SVG, etc.)

## Basic Syntax

### Block Declaration

```text
BlockName: type
```

Where `type` can be:

- `block` - Generic hardware block
- `multiplexor` - Multiplexer/selector
- `divider` - Clock divider
- Logic gates: `AND`, `OR`, `NAND`, `NOR`, `XOR`, `NOT`
- Storage: `RS FlipFlop`, `D FlipFlop`, `Latch`
- `register` - Data register
- `counter` - Count register

### Block Properties

#### Width Specification

```text
BlockName: type
  Width: N-bit
```

Defines the bit width of data paths through the block.

#### Configuration Options

```text
BlockName: type
  Options:
    Description1: binary_value
    Description2: binary_value
```

Enumerates possible configurations with their binary encodings.

#### Input/Output Definitions

```text
BlockName: type
  Inputs:
    SignalName: type @attributes
  Outputs:
    SignalName: type @attributes
```

### Signal Types

- `1-bit` - Single bit signal
- `N-bit` - Multi-bit signal (where N is a number)
- `clock` - Clock signal
- `reset` - Reset signal
- `enable` - Enable signal

### Signal Attributes

- `@bus` - Multi-bit bus signal
- `@edge` - Edge-triggered signal (responds to transitions rather than steady levels)
- `@rising` - Rising edge triggered (0→1 transition)
- `@falling` - Falling edge triggered (1→0 transition)
- `@set` - Sets a flag/bit
- `@clear` - Clears a flag/bit
- `@bi` - Bidirectional signal

#### Edge-Triggered vs Level-Triggered Signals

The `@edge` attribute specifically marks signals that are **edge-triggered** as opposed to **level-triggered**:

- **Edge-triggered** (`@edge`): Responds to signal transitions (changes from 0→1 or 1→0)
  - Clock inputs to registers, flip-flops, and latches
  - Capture trigger signals that activate on transitions
  - Synchronization signals that require edge detection

- **Level-triggered**: Responds to steady signal levels (high or low state)
  - Enable signals that gate operations while active
  - Select signals for multiplexers
  - Control signals that maintain state

**Example Usage in Timer A:**

```text
TAxR: block
  Inputs:
    CLK: 1-bit @edge     # Clock responds to transitions
    CLR: 1-bit           # Clear responds to level (active high/low)
    Mode: 2-bit @bi      # Mode is bidirectional level signal
```

This distinction is critical for accurate hardware implementation as edge-triggered signals require edge detection
circuitry while level-triggered signals use simple combinational logic.

## Connection Syntax

### Basic Connections

```text
Source -> Target
```

Direct connection from source to target.

### Qualified Connections

```text
Block.Output -> Target.Input
```

Connection between specific block pins.

### Input Connections

```text
Signal ->| Target.Input
```

Connection to a block input (pipe notation indicates input).

### Array/Bus Connections

```text
Signal @ bus
```

Indicates bus-type connection.

### Conditional Connections

```text
Signal ->| Target.Input @condition
```

Connection active under specific conditions.

### Signal Fanout

```text
Signal ->| Target1.Input
Signal ->| Target2.Input
```

Indicates that one signal drives multiple destinations (fanout). The `->|` notation specifically
represents signal fanout in visual diagrams where a single source connects to multiple targets.

## Array and Indexing Notation

### Array Declaration

```text
BlockArray[start..end]: type @qualifier
```

Declares an array of blocks.

### Index Mapping

```text
@BlockArray[].property = index
```

Maps array indices to block properties. Each element of the array has its specified property set to the array index value.

**Example**:

```text
CCRn[0..6]: CCR @CCRn[].n = index
```

This creates an array of CCR blocks where:

- `CCRn[0].n = 0`
- `CCRn[1].n = 1`
- `CCRn[2].n = 2`
- ... and so on through `CCRn[6].n = 6`

### Bus Distribution

```text
Signal -> BlockArray[].Input @ bus
```

Distributes signal to all array elements.

**Example**:

```text
Timer Block.Count ->| CCRn[].Count @ bus
```

This connects the `Timer Block.Count` signal to the `Count` input of every element in the CCRn array:

- `Timer Block.Count -> CCRn[0].Count`
- `Timer Block.Count -> CCRn[1].Count`
- `Timer Block.Count -> CCRn[2].Count`
- ... and so on for all array elements

### Bit Indexing

```text
Signal[bit_index] ->| Target.Input
```

Connects a specific bit of a multi-bit signal to a single-bit input. Uses zero-based indexing where
bit 0 is the least significant bit.

**Example**:

```text
OUTMOD[0] ->| CCR.OutputLogic.Nor1.A
OUTMOD[1] ->| CCR.OutputLogic.Nor1.B
OUTMOD[2] ->| CCR.OutputLogic.Nor1.C
```

This connects individual bits of the 3-bit OUTMOD signal to separate single-bit inputs:

- `OUTMOD[0]` (bit 0, LSB) connects to `CCR.OutputLogic.Nor1.A`
- `OUTMOD[1]` (bit 1) connects to `CCR.OutputLogic.Nor1.B`
- `OUTMOD[2]` (bit 2, MSB) connects to `CCR.OutputLogic.Nor1.C`

**Usage Guidelines**:

- Use zero-based indexing (`[0]`, `[1]`, `[2]`, etc.)
- Bit 0 is always the least significant bit (LSB)
- Use fanout notation (`->|`) when the same bit drives multiple destinations
- Document the bit width and meaning of multi-bit signals clearly

## Special Notations

### Multiplexor Inputs

```text
MuxName: multiplexor
  Signal1 -> MuxName.00
  Signal2 -> MuxName.01
  Signal3 -> MuxName.10
  Signal4 -> MuxName.11
```

Binary notation indicates multiplexor select values.

### Clock Dividers

```text
DividerName: divider
  Options:
    1: 00
    2: 01
    4: 10
    8: 11
```

Maps division ratios to control bit patterns.

### Logic Gate Inputs

```text
GateName: AND
  Inputs:
    A: 1-bit
    B: 1-bit
    C: 1-bit
  Outputs:
    Y: 1-bit
```

## Connection Sections

### Global Connections

```text
Connections:
  Signal1 -> Target1
  Signal2 -> Target2
```

Groups related connections together.

### Local Block Connections

Connections within a block's definition are local to that block.

## Advanced Features

### Signal Fanout Patterns

Signal fanout occurs when one signal source drives multiple destinations. This is common in digital
systems where clock signals, data buses, and control signals need to reach multiple blocks.

#### Clock Fanout Example

```text
TimerClock ->| TAxR.CLK
TimerClock ->| Sync.CLK
TimerClock ->| DataLatch.CLK
```

#### Data Bus Fanout Example

```text
Count ->| CCR0.Count @ bus
Count ->| CCR1.Count @ bus
Count ->| CCRn.Count @ bus
```

#### Control Signal Fanout Example

```text
TACLR ->| ID.Clear
TACLR ->| IDEX.Clear
TACLR ->| TAxR.Clear
```

In visual representations (Mermaid diagrams), the `->|` notation specifically indicates fanout
connections where the label can describe the signal type or connection properties.

### Hierarchical Blocks

```text
MainBlock: block
  SubBlock1: type
    Inputs:
      ...
    Outputs:
      ...
  SubBlock2: type
    ...
  Connections:
    SubBlock1.Out -> SubBlock2.In
```

### Conditional Logic

```text
Signal ->| Target @condition
```

Where condition can be edge types, states, or other qualifiers.

### Feedback Paths

```text
Block.Output <-> Block.Input
```

Bidirectional or feedback connections.

## Best Practices

### Naming Conventions

- Use descriptive names that match hardware documentation
- Maintain consistency with official documentation (TI manuals)
- Use standard abbreviations (CLK for clock, RST for reset)

### Organization

- Declare blocks before connections
- Group related blocks together
- Use consistent indentation
- Add comments for complex logic

### Signal Flow

- Follow logical signal flow in connection order
- Group inputs, outputs, and internal connections separately
- Use qualified names for clarity in complex diagrams

## Translation Guidelines

### To Mermaid Diagrams

- Blocks become nodes
- Connections become arrows
- Attributes become node styling

### To SVG/Visual

- Block types determine visual representation
- Signal attributes affect line styling
- Hierarchical structure defines layout

## Example Usage

See `docs/references/SLAU445/13.1_timer_a_introduction.md` for a complete example of this notation
system describing the Timer A module.

## Extension Guidelines

When extending this notation:

1. Maintain backward compatibility
2. Use consistent syntax patterns
3. Document new features clearly
4. Provide examples of usage
5. Consider translation impact to visual formats

This notation system provides a foundation for documenting complex hardware systems in a maintainable,
version-controllable format while preserving the detailed information needed for accurate implementation.
