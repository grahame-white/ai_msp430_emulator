# Microcontroller Logic Notation for AI Developers

This document defines a simple textual markdown notation for representing microcontroller logic elements,
their interconnections, and behaviors in a format suitable for AI developers to understand complex logic
interactions within microcontroller systems.

## Table of Contents

- [Design Principles](#design-principles)
- [Basic Logic Gates](#basic-logic-gates)
- [Signals and Interconnects](#signals-and-interconnects)
- [Registers and Register Fields](#registers-and-register-fields)
- [Multiplexors and Switches](#switches-and-configurable-elements)
- [Switches and Configurable Elements](#switches-and-configurable-elements)
- [Blackbox Logic Blocks](#blackbox-logic-blocks)
- [Pins and External Connections](#pins-and-external-connections)
- [Feedback Loops and Timing](#feedback-loops-and-timing)
- [Boundaries and Grouping](#boundaries-and-grouping)
- [Bus Systems](#bus-systems)
- [Complex Example: MSP430 CPU Core Logic](#complex-example-msp430-cpu-core-logic)
- [Additional Practical Examples](#additional-practical-examples)
- [Usage Guidelines](#usage-guidelines)
- [Notation Reference](#notation-reference)

## Design Principles

- **Textual over graphical**: Uses simple ASCII characters within markdown code blocks
- **Line-length compliant**: All notation fits within 120-character markdown line limits
- **AI-readable**: Clear, consistent patterns that AI can easily parse and understand
- **Hierarchical**: Supports grouping and boundaries for complex system organization
- **Extensible**: Basic notation can be extended for specific microcontroller features

## Basic Logic Gates

Logic gates use simple ASCII representations with clear input/output indicators:

```text
Basic Gates:
AND:    A─┐     OR:     A─┐     NOT:    A─[>o─ Y
        B─┤&├─ Y        B─┤≥1├─ Y
          └─┘             └─┘

XOR:    A─┐     NAND:   A─┐     NOR:    A─┐
        B─┤=1├─ Y       B─┤&├o─ Y       B─┤≥1├o─ Y
          └─┘             └─┘             └─┘

Multi-input gates (arbitrary inputs):
4-input AND:    A─┐
                B─┤
                C─┤&├─ Y
                D─┤
                  └─┘

3-input OR:     A─┐
                B─┤≥1├─ Y
                C─┤
                  └─┘
```

## Signals and Interconnects

Signals are represented with clear direction and type indicators:

```text
Signal Types:
─────    Wire/connection
═════    Bus (multi-bit)
- - -    Clock signal
~~~~~    Analog signal
↑↓       Edge-triggered (↑=rising, ↓=falling)
■        High level
□        Low level
◊        Tri-state/floating

Direction and branching:
A──→B           Unidirectional A to B
A←──→B          Bidirectional
A──┬──→B        Signal splits to multiple destinations
   └──→C

Multi-destination signals:
CLK──┬──→REG1
     ├──→REG2
     └──→REG3

Signal labels and properties:
DATA[7:0]════→  8-bit bus labeled DATA
CLK↑─────────→  Rising edge clock signal
RST■─────────→  Active high reset signal
EN□──────────→  Active low enable signal
```

## Registers and Register Fields

Registers use bit field notation with clear labeling:

```text
Basic register:
REG_NAME[15:0] = [FIELD_A(15:12) | FIELD_B(11:8) | FIELD_C(7:4) | FIELD_D(3:0)]

Register with specific bit assignments:
STATUS[7:0] = [IRQ(7) | OVF(6) | ZERO(5) | CARRY(4) | reserved(3:0)]

Register connections:
DATA_IN[7:0]════→[REG_A[7:0]]──→[MUX_SEL]──→DATA_OUT[7:0]═══→

Register arrays:
REG_FILE[15:0] = {R0[15:0], R1[15:0], R2[15:0], ..., R15[15:0]}
ADDR[3:0]═══→[REG_FILE]═══→DATA[15:0]
```

## Blackbox Logic Blocks

Multiplexors use clear input selection notation:

```text
Basic 2:1 MUX:
A────┐
     ├──MUX──→Y
B────┤     ↑
     └─────SEL

4:1 MUX:
IN0──┐
IN1──┼──MUX──→OUT
IN2──┤     ↑
IN3──┘  SEL[1:0]

Switch with enable:
A────[SW]────B    (switch notation)
      ↑
     EN■

Tri-state buffer:
A────[>]────B◊    (enabled)
      ↑
     EN■

A────[>]────B◊    (disabled/tri-state)
      ↑
     EN□
```

## Switches and Configurable Elements

Advanced switch notation with default states and enable signals:

```text
Basic switch with enable:
A────[SW]────B    (switch open by default)
      ↑
     EN■

A════[SW]════B    (switch closed by default)
      ↑
     EN■

Configurable switches:
A────[SW:OPEN]────B     (explicitly open by default)
A────[SW:CLOSED]──B     (explicitly closed by default)
A────[SW:TOGGLE]──B     (toggleable switch)

Multi-position switch:
      ┌─→OUT_A
IN────┤SW├→OUT_B
      └─→OUT_C
       ↑
    SEL[1:0]

Transmission gates and pass transistors:
A────[TG]────B     (transmission gate)
      ↑
     EN■

A────[PT_N]──B     (NMOS pass transistor)
      ↑
     EN■

A────[PT_P]──B     (PMOS pass transistor)
      ↑
     EN□
```

Custom logic blocks use labeled boxes with defined interfaces:

```text
Simple blackbox:
A[7:0]═══→┌─────────┐
B[3:0]═══→│  ALU    │═══→RESULT[7:0]
CLK──────→│         │
RST■─────→└─────────┘

Complex blackbox with multiple interfaces:
DATA_IN[15:0]════→┌─────────────────┐
ADDR[19:0]═══════→│                 │═══→DATA_OUT[15:0]
CLK──────────────→│   MEMORY_CTRL   │
WE■──────────────→│                 │──→READY■
OE□──────────────→│                 │──→ERROR■
CS■──────────────→└─────────────────┘

Nested blackboxes:
INPUTS═══→┌────┌──────┐────┌───────┐────┐═══→OUTPUTS
         │    │ DEC  │    │  ALU  │    │
         │    └──────┘    └───────┘    │
         │      CTRL_UNIT              │
         └─────────────────────────────┘
```

## Pins and External Connections

Pin representations show microcontroller external interfaces:

```text
Pin notation:
MCU_PIN_1●──→EXTERNAL_DEVICE
MCU_PIN_2●←──
MCU_PIN_3●↔── (bidirectional)

Unconnected pins:
MCU_PIN_4●    (floating/unconnected)
MCU_PIN_5●NC  (not connected, explicit)

Pin with internal pull-up/down:
VCC
 │
 ┌─┐  Pull-up resistor
 └─┤
   │
MCU_PIN●──→

MCU_PIN●──┬──→
          │
         ┌─┐  Pull-down resistor
         └─┤
           │
          GND
```

## Feedback Loops and Timing

Feedback connections and timing relationships:

```text
Simple feedback:
A──→┌───────┐──→B
    │  PROC │
    └───┬───┘
        │
        └────FB──→  (feedback signal)

Feedback with delay:
A──→┌───────┐──→B
    │  PROC │
    └───┬───┘
        │
        [D]  (delay element)
        │
        └────FB──→

Clock domains:
CLK1─ ─ ─ ┬ ─ ─ ─REG1──→
           │
CLK2─ ─ ─ ─┴ ─ ─ ─REG2──→  (different clock domains)

State machines:
STATE[1:0]──→┌────────┐──→NEXT_STATE[1:0]
             │  FSM   │
INPUTS──────→│  LOGIC │──→OUTPUTS
             └────────┘
```

## Boundaries and Grouping

Group related elements with clear boundaries:

```text
Simple grouping:
╔═══════════════════════╗
║ CPU_CORE              ║
║ PC[15:0]══→┌──────┐   ║
║ INST═════→│ EXEC │══→║═══→RESULT
║ CLK──────→│ UNIT │   ║
║           └──────┘   ║
╚═══════════════════════╝

Nested groupings:
╔═══════════════════════════════════════╗
║ MICROCONTROLLER                       ║
║ ┌─────────────┐  ┌─────────────────┐  ║
║ │ CPU_CORE    │  │ MEMORY_SYSTEM   │  ║
║ │ [logic...]  │←→│ [logic...]      │  ║
║ └─────────────┘  └─────────────────┘  ║
║ ┌─────────────────────────────────────┐ ║
║ │ PERIPHERAL_BUS                    │ ║
║ │ [bus logic...]                    │ ║
║ └─────────────────────────────────────┘ ║
╚═══════════════════════════════════════╝

Module interfaces:
┌─MODULE_A─┐
│ IN[7:0]═→│═→OUT[7:0]
│ CLK────→ │
└─────────┘
     ║ (interface boundary)
┌─MODULE_B─┐
│ IN[7:0]←═│
│ CTRL───→ │
└─────────┘
```

## Bus Systems

Bus notation for multi-master, multi-slave systems:

```text
Simple bus:
MASTER1═══╤═══════════════════╤═══SLAVE1
          │    DATA_BUS       │
MASTER2═══╧═══════════════════╧═══SLAVE2

Hierarchical bus:
HIGH_SPEED_BUS════════════════════
    ║
    ╠═══BRIDGE═══╤═══PERIPHERAL_BUS═══DEVICE1
    ║            │                   
    ║            ├═══DEVICE2
    ║            └═══DEVICE3
    ║
    ╠═══MEMORY
    ╚═══CPU

Address/Data separated:
ADDR[19:0]═══════════════════╤═══→MEMORY
                             │
DATA[15:0]═══════════════════╧←═→

Control signals:
WE■──────────────────────────────→
OE□──────────────────────────────→
CS■──────────────────────────────→
```

## Complex Example: MSP430 CPU Core Logic

```text
╔═══════════════════════════════════════════════════════════════════════════════╗
║ MSP430_CPU_CORE                                                               ║
║                                                                               ║
║ ┌─INSTRUCTION_FETCH─┐    ┌─DECODE─┐    ┌─EXECUTE─┐    ┌─WRITEBACK─┐          ║
║ │ PC[15:0]═════════→│═══→│       │═══→│        │═══→│          │          ║
║ │ INST[15:0]←══════ │    │ CTRL  │    │  ALU   │    │ REG_FILE │          ║
║ │ CLK─────────────→ │    │ UNIT  │    │        │    │          │          ║
║ └─────────────────── ┘    └───┬───┘    └───┬────┘    └────┬─────┘          ║
║                               │            │              │                ║
║ ┌─REGISTER_FILE─────┐         │            │              │                ║
║ │ R0[15:0]──────────│←────────┼────────────┼──────────────┤                ║
║ │ R1[15:0]──────────│←────────┼────────────┼──────────────┤                ║
║ │ ...               │         │            │              │                ║
║ │ R15[15:0]─────────│←────────┼────────────┼──────────────┤                ║
║ │ SR[15:0]──────────│←────────┴─STATUS─────┴──────────────┤                ║
║ └───────────────────┘                                     │                ║
║                                                           │                ║
║ MEMORY_INTERFACE                                          │                ║
║ ADDR[19:0]══════════════════════════════════════════════►│                ║
║ DATA[15:0]◄═══════════════════════════════════════════════╪══════════════►║
║ WE■──────────────────────────────────────────────────────→                ║
║ OE□──────────────────────────────────────────────────────→                ║
║                                                                           ║
║ INTERRUPT_CONTROLLER                                                      ║
║ IRQ[15:0]────┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─→┌────────┐──→PC■              ║
║ GIE■─────────┼─┼─┼─┼─┼─┼─┼─┼─┼─┼─┼─┼─┼─┼─┼─┼──→│PRIORITY│                  ║
║              └─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴───→│ ENCODER│                  ║
║                                                  └────────┘                  ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

## Additional Practical Examples

### GPIO Port Logic

```text
╔═══════════════════════════════════════════════════════════╗
║ GPIO_PORT_A                                               ║
║                                                           ║
║ PAxSEL[7:0]═══→┌─────────┐     ┌────────┐                ║
║ PAxDIR[7:0]═══→│ PIN_MUX │────→│ OUTPUT │──→PAxOUT[7:0]● ║
║ PAxOUT[7:0]═══→│         │     │ DRIVER │                ║
║ PAxREN[7:0]═══→└─────────┘     └────────┘                ║
║                     │                                    ║
║                     │          ┌────────┐                ║
║                     └─────────→│ INPUT  │←──PAxIN[7:0]●  ║
║                                │ BUFFER │                ║
║ PAxIFG[7:0]←══════════════════←│        │                ║
║ PAxIE[7:0]═══→┌────────┐       └────────┘                ║
║ PAxIES[7:0]══→│ INTR   │                                 ║
║               │ CTRL   │──→IRQ■                          ║
║ CLK─ ─ ─ ─ ─→│        │                                 ║
║               └────────┘                                 ║
╚═══════════════════════════════════════════════════════════╝
```

### Clock System

```text
╔═══════════════════════════════════════════════════════════════════════════╗
║ CLOCK_SYSTEM                                                              ║
║                                                                           ║
║ LFXT●───┬─→[LFXT_OSC]─ ─ ─ ─┬─→┌─────┐                                   ║
║ HFXT●───┼─→[HFXT_OSC]─ ─ ─ ─┼─→│     │                                   ║
║         │                   │  │ MUX │─→MCLK─ ─ ─ ─ ─ ─→CPU              ║
║ DCO─────┼──────────────────┼─→│     │                                   ║
║ VLO─ ─ ─┴─ ─ ─ ─ ─ ─ ─ ─ ─ ─┴─→└─────┘                                   ║
║                                   ↑                                      ║
║ SELM[1:0]═════════════════════════╪═══════════════════════════════════════║
║                                   │                                      ║
║ DIVS[1:0]═══→[÷2^N]─→SMCLK─ ─ ─ ─ ─┼ ─ ─ ─ ─→PERIPHERALS                ║
║ DIVA[1:0]═══→[÷2^N]─→ACLK─ ─ ─ ─ ─ ┴ ─ ─ ─ ─→TIMERS                     ║
║                                                                           ║
║ ┌─FAULT_DETECTION─┐                                                       ║
║ │ OSC_FAULT■─────→│──→OFIFG■                                             ║
║ │ DCO_FAULT■─────→│                                                      ║
║ └─────────────────┘                                                       ║
╚═══════════════════════════════════════════════════════════════════════════╝
```

### Timer Module with Capture/Compare

```text
╔══════════════════════════════════════════════════════════════════════════════╗
║ TIMER_A                                                                      ║
║                                                                              ║
║ TACLK─ ─ ─ ─ ─ ─ ─┬─→┌──────┐        ┌─────────────┐                       ║
║ ACLK─ ─ ─ ─ ─ ─ ─ ┼─→│ MUX  │─ ─ ─ ─→│   COUNTER   │                       ║
║ SMCLK─ ─ ─ ─ ─ ─ ─┼─→│      │        │ TAR[15:0]   │                       ║
║ INCLK──────────────┘  └──────┘        └──────┬──────┘                       ║
║                          ↑                   │                              ║
║ TASSEL[1:0]══════════════╪═══════════════════│══════════════════════════════║
║ ID[1:0]═══→[÷2^N]────────┘                   │                              ║
║                                              │                              ║
║ ┌─COMPARE_UNIT_0─┐   ┌─COMPARE_UNIT_1─┐     │   ┌─COMPARE_UNIT_2─┐         ║
║ │ TACCR0[15:0]   │   │ TACCR1[15:0]   │     │   │ TACCR2[15:0]   │         ║
║ │ ┌────────────┐ │   │ ┌────────────┐ │     │   │ ┌────────────┐ │         ║
║ │ │    CMP     │═│←══│═│    CMP     │═│←════│═══│═│    CMP     │ │         ║
║ │ └─────┬──────┘ │   │ └─────┬──────┘ │     │   │ └─────┬──────┘ │         ║
║ │       │ OUT0─  │   │       │ OUT1─  │     │   │       │ OUT2─  │         ║
║ │ ┌─────▼──────┐ │   │ ┌─────▼──────┐ │     │   │ ┌─────▼──────┐ │         ║
║ │ │ OUTPUT_UNIT│─│──→│─│ OUTPUT_UNIT│─│────→│──→│─│ OUTPUT_UNIT│ │         ║
║ │ └────────────┘ │   │ └────────────┘ │     │   │ └────────────┘ │         ║
║ │      CCIFG0■──→│   │      CCIFG1■──→│     │   │      CCIFG2■──→│         ║
║ └────────────────┘   └────────────────┘     │   └────────────────┘         ║
║                                             │                              ║
║ TAIFG■←─────────────────────────────────────┘                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
```

## Usage Guidelines

1. **Start simple**: Begin with basic gates and connections, add complexity incrementally
2. **Use consistent spacing**: Align elements for readability within 120-character limits
3. **Label everything**: Clear signal names and bit widths prevent confusion
4. **Group logically**: Use boundaries to separate functional blocks
5. **Show data flow**: Use arrows and connection patterns to indicate signal direction
6. **Document conventions**: Include a legend when using custom notation elements

## Notation Reference

### Symbol Summary

```text
Logic Gates:    &(AND) ≥1(OR) =1(XOR) >o(NOT) &o(NAND) ≥1o(NOR)
Connections:    ─(wire) ═(bus) -(clock) ~(analog) ┌┐└┘┬┴┼├┤╔╗╚╝╦╩╬╠╣
Directions:     ←→↑↓ (arrows) ═══→ (bus direction)
States:         ■(high/active) □(low/inactive) ◊(tri-state) NC(not connected)
Elements:       [](logic block) {}(register) ●(pin) [D](delay) [SW](switch)
Groupings:      ╔═══╗ (primary boundary) ┌───┐ (secondary boundary)
```

This notation system provides a comprehensive yet simple way to represent complex microcontroller
logic in a format that is both human-readable and suitable for AI analysis and understanding.
