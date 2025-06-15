# Hardware Block Notation System

This document defines the notation system used to describe hardware block diagrams in the MSP430 emulator documentation. This notation was developed to represent complex digital logic systems in a text-based format that is both human-readable and suitable for version control.

## Purpose and Scope

This notation system enables:
- Clear representation of digital hardware blocks and their interconnections
- Version-controllable documentation of complex logic diagrams  
- Consistent documentation across different hardware modules
- Easy translation to visual formats (Mermaid, SVG, etc.)

## Basic Syntax

### Block Declaration
```
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
```
BlockName: type
  Width: N-bit
```
Defines the bit width of data paths through the block.

#### Configuration Options  
```
BlockName: type
  Options:
    Description1: binary_value
    Description2: binary_value
```
Enumerates possible configurations with their binary encodings.

#### Input/Output Definitions
```
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
- `@edge` - Edge-triggered signal
- `@rising` - Rising edge triggered
- `@falling` - Falling edge triggered  
- `@set` - Sets a flag/bit
- `@clear` - Clears a flag/bit
- `@bi` - Bidirectional signal

## Connection Syntax

### Basic Connections
```
Source -> Target
```
Direct connection from source to target.

### Qualified Connections
```
Block.Output -> Target.Input
```
Connection between specific block pins.

### Input Connections
```
Signal ->| Target.Input
```
Connection to a block input (pipe notation indicates input).

### Array/Bus Connections
```
Signal @ bus
```
Indicates bus-type connection.

### Conditional Connections
```
Signal ->| Target.Input @condition
```
Connection active under specific conditions.

## Array and Indexing Notation

### Array Declaration
```
BlockArray[start..end]: type @qualifier
```
Declares an array of blocks.

### Index Mapping
```
@BlockArray[].property = variable
```
Maps array indices to variables.

### Bus Distribution
```
Signal -> BlockArray[].Input @ bus
```
Distributes signal to all array elements.

## Special Notations

### Multiplexor Inputs
```
MuxName: multiplexor
  Signal1 -> MuxName.00
  Signal2 -> MuxName.01
  Signal3 -> MuxName.10
  Signal4 -> MuxName.11
```
Binary notation indicates multiplexor select values.

### Clock Dividers
```
DividerName: divider
  Options:
    1: 00
    2: 01
    4: 10  
    8: 11
```
Maps division ratios to control bit patterns.

### Logic Gate Inputs
```
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
```
Connections:
  Signal1 -> Target1
  Signal2 -> Target2
```
Groups related connections together.

### Local Block Connections
Connections within a block's definition are local to that block.

## Advanced Features

### Hierarchical Blocks
```
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
```
Signal ->| Target @condition
```
Where condition can be edge types, states, or other qualifiers.

### Feedback Paths
```
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

See `docs/references/SLAU445/13.1_timer_a_introduction.md` for a complete example of this notation system describing the Timer A module.

## Extension Guidelines

When extending this notation:
1. Maintain backward compatibility
2. Use consistent syntax patterns
3. Document new features clearly
4. Provide examples of usage
5. Consider translation impact to visual formats

This notation system provides a foundation for documenting complex hardware systems in a maintainable, version-controllable format while preserving the detailed information needed for accurate implementation.