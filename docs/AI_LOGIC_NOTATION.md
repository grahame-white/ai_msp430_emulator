# AI Logic Notation System

A comprehensive textual notation system for expressing microcontroller logic constructions in a format optimized
for AI model analysis and understanding. This notation supports unlimited levels of composition and abstraction
while remaining within GitHub markdown constraints.

## Overview

This notation system provides a structured, parseable format for describing:

- **Logic Elements**: Basic gates, registers, multiplexors, blackbox components
- **Interconnections**: Signals, buses, pins with direction and type information  
- **Hierarchical Design**: Unlimited composition levels with clear boundaries
- **Control Elements**: Switches, enable signals, unconnected ports
- **Timing Semantics**: Level vs edge-triggered behaviors

The notation is designed to be both human-readable and machine-parseable, enabling AI models to analyze
complex logic constructions efficiently.

## Core Notation Principles

### 1. Element Declaration Syntax

```text
ElementType:ElementName[Port1,Port2,...] {Properties}
```

- **ElementType**: Specifies the type of logic element (AND, OR, REG, MUX, etc.)
- **ElementName**: Unique identifier within scope
- **Ports**: Input and output connections in square brackets
- **Properties**: Optional configuration in curly braces

### 2. Signal Declaration Syntax

```text
Signal:SignalName[Width:Direction:Type] {Properties}
```

- **Width**: Bit width (1 for single bit, N for multi-bit)
- **Direction**: I (input), O (output), IO (bidirectional)  
- **Type**: L (level), E (edge), C (clock), R (reset)

### 3. Connection Syntax

```text
Source.Port -> Destination.Port
Source.Port ->| Destination.Port (fanout connection)
Source.Port <-> Destination.Port (bidirectional)
```

## Basic Logic Elements

### Logic Gates

```text
# Basic gates with arbitrary inputs/outputs
AND:gate1[a,b,c -> out] {delay:2ns}
OR:gate2[x,y -> z]
NOT:inv1[in -> out]
XOR:xor1[a,b -> result]
NAND:nand1[i1,i2,i3 -> o1,o2] {outputs:2}

# Custom truth table gates
GATE:custom1[a,b,c -> x,y] {
  truth_table: [
    "000 -> 01",
    "001 -> 10", 
    "010 -> 11",
    "011 -> 00",
    "100 -> 01",
    "101 -> 01",
    "110 -> 10",
    "111 -> 11"
  ]
}
```

### Registers and Storage Elements

```text
# Basic register
REG:reg1[D,CLK,RST -> Q,QN] {width:8, trigger:posedge}

# Register with field definitions
REG:status_reg[din,clk,rst -> dout] {
  width: 16,
  fields: [
    "15:9 reserved",
    "8 overflow_flag", 
    "7 negative_flag",
    "6:4 unused",
    "3 zero_flag",
    "2 carry_flag", 
    "1:0 mode_bits"
  ]
}

# Shift register
SHIFT:shr1[din,clk,enable -> dout,serial_out] {
  width: 8,
  direction: left,
  fill: 0
}
```

### Multiplexors and Demultiplexors

```text
# 4:1 Multiplexor
MUX:mux1[i0,i1,i2,i3,sel -> out] {inputs:4, select_width:2}

# 1:4 Demultiplexor  
DEMUX:demux1[in,sel -> o0,o1,o2,o3] {outputs:4, select_width:2}

# Bus multiplexor
MUX:bus_mux[bus_a[7:0],bus_b[7:0],sel -> bus_out[7:0]] {width:8}
```

### Blackbox Components

```text
# Opaque component with defined interface
BLACKBOX:cpu_core[
  clk,rst,irq[7:0] -> 
  addr[15:0],data[7:0],rd,wr,ack
] {
  description: "MSP430 CPU Core",
  timing: {setup:5ns, hold:2ns},
  power: {static:10mW, dynamic:50mW}
}

# Custom peripheral
BLACKBOX:uart[
  clk,rst,tx_data[7:0],tx_enable,rx_ack ->
  rx_data[7:0],rx_ready,tx_ready,tx,rx
] {
  description: "UART Controller",
  baud_rates: [9600, 19200, 38400, 115200]
}
```

## Signal and Interconnect Notation

### Signal Types and Properties

```text
# Basic signals
Signal:clk[1:I:C] {freq:16MHz}
Signal:reset[1:I:R] {active:low, duration:100ns}
Signal:data_bus[8:IO:L] {tristate:true}
Signal:address_bus[16:O:L] {drive_strength:high}

# Edge-triggered signals
Signal:irq[1:I:E] {edge:rising, latency:50ns}
Signal:strobe[1:O:E] {edge:falling, pulse_width:20ns}
```

### Connection Patterns

```text
# Point-to-point connections
cpu_core.clk <- clk
cpu_core.data[7:0] <-> memory.data[7:0]

# Fanout connections (one source, multiple destinations)
clk ->| cpu_core.clk
clk ->| timer.clk  
clk ->| uart.clk

# Bus connections
cpu_core.addr[15:0] -> address_bus[15:0]
address_bus[15:0] -> memory.addr[15:0]
address_bus[15:0] -> peripherals.addr[15:0]

# Partial bus connections
cpu_core.addr[7:0] -> memory.addr_low[7:0]
cpu_core.addr[15:8] -> memory.addr_high[7:0]
```

### Unconnected Ports

```text
# Unconnected inputs (with default values)
AND:gate1[a,UNCON:0,c -> out]  # Input b tied to logic 0
OR:gate2[x,UNCON:1,z -> result]  # Input y tied to logic 1
REG:reg1[data[7:0],clk,UNCON:RST -> q[7:0]] # Reset unconnected

# Unconnected outputs
BLACKBOX:test_gen[-> data[7:0],UNCON:debug,status]  # Debug output unused
```

## Control Elements and Switches

### Switches with Enable Control

```text
# Transmission gate / switch
SWITCH:sw1[in -> out] {
  enable: control_signal,
  default: open,
  resistance: {on:100ohm, off:1Mohm}
}

# Bus switch
SWITCH:bus_sw[bus_a[7:0] <-> bus_b[7:0]] {
  enable: bus_enable,
  default: closed,
  isolation: high_z
}

# Analog switch with multiple controls
SWITCH:analog_sw[ain -> aout] {
  enable: [en1 AND en2 AND NOT(disable)],
  default: open,
  type: analog,
  bandwidth: 10MHz
}
```

### Tri-state Buffers

```text
# Tri-state buffer
TRISTATE:buf1[in -> out] {
  enable: output_enable,
  default: high_z
}

# Bidirectional buffer
TRISTATE:bidir[a <-> b] {
  dir_control: direction,
  default: high_z
}
```

## Hierarchical Design and Composition

### Component Boundaries

```text
# Boundary definition
BOUNDARY:cpu_subsystem {
  # Internal components
  REG:pc[data[15:0],clk,rst -> addr[15:0]]
  REG:sp[data[15:0],clk,rst -> stack_ptr[15:0]]
  ALU:alu[a[15:0],b[15:0],op[3:0] -> result[15:0],flags[3:0]]
  
  # Internal connections
  pc.addr[15:0] -> instruction_fetch.addr[15:0]
  alu.result[15:0] -> pc.data[15:0]
  
  # External interface
  INTERFACE {
    inputs: [clk, rst, interrupt[7:0]],
    outputs: [mem_addr[15:0], mem_data[7:0], mem_rd, mem_wr],
    bidirectional: [data_bus[7:0]]
  }
}
```

### Multi-Level Composition

```text
# Level 1: Basic functional units
BOUNDARY:register_file {
  REG:r0[din[15:0],clk,rst -> dout[15:0]] {description:"PC"}
  REG:r1[din[15:0],clk,rst -> dout[15:0]] {description:"SP"}
  REG:r2[din[15:0],clk,rst -> dout[15:0]] {description:"SR"}
  # ... additional registers
  
  INTERFACE {
    inputs: [read_addr[3:0], write_addr[3:0], write_data[15:0], clk, rst],
    outputs: [read_data[15:0]]
  }
}

# Level 2: CPU subsystems  
BOUNDARY:cpu_core {
  USE:register_file as rf
  USE:alu_unit as alu
  USE:control_unit as ctrl
  
  # Interconnections
  rf.read_data[15:0] -> alu.operand_a[15:0]
  alu.result[15:0] -> rf.write_data[15:0]
  ctrl.reg_write_addr[3:0] -> rf.write_addr[3:0]
}

# Level 3: Complete microcontroller
BOUNDARY:msp430_system {
  USE:cpu_core as cpu
  USE:memory_subsystem as mem
  USE:peripheral_subsystem as periph
  
  # System-level connections
  cpu.mem_addr[15:0] -> mem.addr[15:0]
  cpu.data_bus[7:0] <-> mem.data[7:0]
  periph.interrupt[7:0] -> cpu.interrupt[7:0]
}
```

## Advanced Features

### Feedback Loops

```text
# Simple feedback
REG:counter[data[7:0],clk,rst -> count[7:0]]
ADD:inc[a[7:0],b[7:0] -> sum[7:0]]

# Feedback connection
counter.count[7:0] -> inc.a[7:0]
inc.sum[7:0] -> counter.data[7:0]
CONST:one[-> 1] {value:1, width:8}
one -> inc.b[7:0]

# Feedback with delay
BOUNDARY:feedback_loop {
  DELAY:fb_delay[in[7:0] -> out[7:0]] {cycles:1}
  XOR:feedback_xor[a[7:0],b[7:0] -> result[7:0]]
  
  # External input
  INTERFACE.input[7:0] -> feedback_xor.a[7:0]
  
  # Feedback path
  feedback_xor.result[7:0] -> fb_delay.in[7:0]
  fb_delay.out[7:0] -> feedback_xor.b[7:0]
  
  # Output
  feedback_xor.result[7:0] -> INTERFACE.output[7:0]
}
```

### Bus Systems

```text
# Multi-master bus
BUS:system_bus[16:8] {
  protocol: "AHB-Lite",
  masters: [cpu_core, dma_controller],
  slaves: [memory, peripherals],
  arbitration: priority
}

# Bus connections
cpu_core.bus_req -> system_bus.master1_req
system_bus.master1_grant -> cpu_core.bus_grant
cpu_core.addr[15:0] -> system_bus.addr[15:0]
cpu_core.data[7:0] <-> system_bus.data[7:0]

# Multiple slave connections
system_bus.addr[15:0] ->| memory.addr[15:0]
system_bus.addr[15:0] ->| peripherals.addr[15:0]
system_bus.data[7:0] <->| memory.data[7:0]
system_bus.data[7:0] <->| peripherals.data[7:0]
```

### Timing and Clocking

```text
# Clock domains
CLOCK:main_clk[-> clk] {freq:16MHz, duty:50%, jitter:10ps}
CLOCK:peripheral_clk[-> clk] {freq:8MHz, duty:50%}

# Clock dividers
DIVIDER:clk_div[clk_in -> clk_out] {ratio:2}
main_clk.clk -> clk_div.clk_in
clk_div.clk_out -> peripheral_clk.clk

# Timing constraints
TIMING:setup_constraint {
  signal: data_bus[7:0],
  reference: clk,
  constraint: setup,
  value: 5ns
}
```

## AI Analysis Guidelines

### Parseable Structure

The notation is designed for AI analysis with these characteristics:

1. **Hierarchical Parse Tree**: Elements can be parsed into a tree structure
2. **Type Safety**: All connections specify width and type compatibility  
3. **Dependency Analysis**: Signal flow can be traced through the design
4. **Timing Analysis**: Clock domains and timing constraints are explicit
5. **Resource Analysis**: Component instances and their properties are quantified

### Example Analysis Queries

```text
# Find all feedback loops
QUERY: FIND CYCLES IN signal_graph

# Identify clock domains  
QUERY: FIND ALL clock_sources AND trace_fanout

# Calculate propagation delay
QUERY: FIND PATH from input_pin to output_pin 
       CALCULATE total_delay

# Verify timing constraints
QUERY: CHECK ALL setup_constraints AND hold_constraints

# Resource utilization
QUERY: COUNT instances BY component_type
       CALCULATE total_power
```

### Composition Validation

```text
# Hierarchical consistency checks
VALIDATE:port_compatibility {
  rule: "connecting_ports_must_match_width_and_type"
}

VALIDATE:clock_domain_crossing {
  rule: "signals_crossing_domains_must_use_synchronizers" 
}

VALIDATE:unconnected_detection {
  rule: "all_inputs_must_be_driven_or_explicitly_unconnected"
}
```

## Usage Examples

### Simple Counter Circuit

```text
# 8-bit up counter with enable
BOUNDARY:counter_8bit {
  REG:count_reg[d[7:0],clk,rst -> q[7:0]]
  ADD:incrementer[a[7:0],b[7:0] -> sum[7:0]]
  MUX:enable_mux[d0[7:0],d1[7:0],sel -> out[7:0]]
  
  # Constants
  CONST:one[-> 1] {value:1, width:8}
  CONST:zero[-> 0] {value:0, width:8}
  
  # Connections
  count_reg.q[7:0] -> incrementer.a[7:0]
  one -> incrementer.b[7:0]
  incrementer.sum[7:0] -> enable_mux.d1[7:0]
  count_reg.q[7:0] -> enable_mux.d0[7:0]
  enable_mux.out[7:0] -> count_reg.d[7:0]
  
  # Interface
  INTERFACE {
    inputs: [clk, rst, enable],
    outputs: [count[7:0]]
  }
  
  # Control connections
  INTERFACE.enable -> enable_mux.sel
  INTERFACE.clk -> count_reg.clk
  INTERFACE.rst -> count_reg.rst
  count_reg.q[7:0] -> INTERFACE.count[7:0]
}
```

### Memory Interface

```text
# Simple memory controller
BOUNDARY:memory_controller {
  # Address decode
  DECODER:addr_decode[addr[15:0] -> cs[3:0]] {
    ranges: ["0x0000-0x3FFF", "0x4000-0x7FFF", "0x8000-0xBFFF", "0xC000-0xFFFF"]
  }
  
  # Memory banks  
  MEMORY:bank0[addr[13:0],data[7:0],cs,rd,wr <-> data[7:0]] {size:16KB, type:SRAM}
  MEMORY:bank1[addr[13:0],data[7:0],cs,rd,wr <-> data[7:0]] {size:16KB, type:FRAM}
  
  # Bus arbitration
  ARBITER:bus_arb[req[1:0] -> grant[1:0]] {priority:[0,1]}
  
  # Interface
  INTERFACE {
    inputs: [addr[15:0], rd, wr, req[1:0]],
    bidirectional: [data[7:0]],
    outputs: [ack, grant[1:0]]
  }
  
  # Connections
  INTERFACE.addr[15:0] -> addr_decode.addr[15:0]
  addr_decode.cs[0] -> bank0.cs
  addr_decode.cs[1] -> bank1.cs
  INTERFACE.data[7:0] <-> bank0.data[7:0]
  INTERFACE.data[7:0] <-> bank1.data[7:0]
}
```

This comprehensive notation system provides a structured, AI-friendly way to express complex microcontroller logic
designs while maintaining readability and GitHub markdown compatibility. The hierarchical approach supports unlimited
composition levels, making it suitable for everything from simple gates to complete system-on-chip designs.
