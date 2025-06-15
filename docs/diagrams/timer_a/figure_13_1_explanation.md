# Timer A Figure 13-1 Block Diagram Explanation

This document provides a comprehensive explanation of Timer A Figure 13-1 from the
MSP430FR2xx/FR4xx Family User's Guide (SLAU445I), Section 13.1.

## Overview

Figure 13-1 depicts the complete Timer A module architecture, showing the relationships
between clock sources, timer counter, capture/compare registers, and output units. The diagram
illustrates both the main timer block and the capture/compare register (CCR) blocks that can be
instantiated up to 7 times (CCR0 through CCR6).

## Main Timer Block Components

### Clock Path

The timer's clock path consists of several stages:

1. **TASSEL (Timer A Source Select)**: A 2-bit multiplexor that selects one of four clock
   sources:
   - `00`: TAxCLK (external clock input)
   - `01`: ACLK (auxiliary clock)
   - `10`: SMCLK (sub-main clock)
   - `11`: INCLK (inverted clock)

2. **ID (Input Divider)**: A 2-bit clock divider with division ratios:
   - `00`: Divide by 1
   - `01`: Divide by 2
   - `10`: Divide by 4
   - `11`: Divide by 8

3. **IDEX (Input Divider Extension)**: A 3-bit additional divider with ratios 1-8:
   - `000`: Divide by 1
   - `001`: Divide by 2
   - `010`: Divide by 3
   - `011`: Divide by 4
   - `100`: Divide by 5
   - `101`: Divide by 6
   - `110`: Divide by 7
   - `111`: Divide by 8

The combined division factor can range from 1 to 64 (8 × 8).

### Timer Counter (TAxR)

The TAxR block is the core 16-bit counter with:

- **Inputs**:
  - Clock signal from IDEX
  - Clear signal (TACLR)
  - Mode control (2-bit bidirectional)
- **Outputs**:
  - 16-bit count value (distributed to all CCR blocks)
  - Roll-over/carry signal (RC) that sets the timer interrupt flag (TAIFG)
  - Mode feedback to mode control logic

### Mode Control (MC)

The MC block manages timer operating modes based on:

- Compare signal from CCR0 (EQU0)
- 2-bit mode setting determining count behavior (stop, up, continuous, up/down)

## Capture/Compare Register (CCR) Blocks

Each CCR block (instantiated 0-6 times) contains:

### Input Signal Selection

- **CCIS (Capture/Compare Input Select)**: 2-bit multiplexor selecting:
  - `00`: CCInA (Capture input A)
  - `01`: CCInB (Capture input B)
  - `10`: GND (Ground)
  - `11`: VCC (Supply voltage)

### Capture Logic

- **CM (Capture Mode)**: 2-bit control defining capture trigger:
  - `00`: No capture
  - `01`: Capture on rising edge
  - `10`: Capture on falling edge
  - `11`: Capture on both edges

- **SCS (Synchronize Capture Source)**: Synchronizes capture signal with timer clock
- **CAP (Capture)**: Multiplexor selecting between compare mode (0) and capture mode (1)

### Compare Logic

- **Comparatorn**: Compares 16-bit timer count with CCR register value
- **TAxCCRn**: 16-bit register storing capture/compare value
- Output equality signal (Equ) triggers various actions

### Output Unit

- **OutputUnit4**: 3-bit mode-controlled output logic supporting 8 output modes
- **Logic gates**: NOR, AND, NAND, OR gates implement output mode logic
- **DataLatch**: RS flip-flop storing output state, clocked by timer clock

## Signal Flow Analysis

### Capture Mode Operation

1. External signal enters through CCIS multiplexor
2. CM block detects configured edge transitions
3. SCS synchronizes capture with timer clock
4. Timer count value is latched into TAxCCRn register
5. CCIFG interrupt flag is set
6. SCCI reflects synchronized capture input state

### Compare Mode Operation

1. TAxCCRn register holds compare value
2. Comparatorn continuously compares with timer count
3. Equality generates interrupt and drives output unit
4. Output unit generates PWM or other timed outputs based on OUTMOD setting

### Output Generation

The output unit implements 8 different output modes (OUTMOD 0-7):

- Mode 0: Output (OUT bit controls output directly)
- Mode 1: Set (output set when timer reaches CCR)
- Mode 2: Toggle/Reset (output toggles at CCR, resets at timer overflow)
- Mode 3: Set/Reset (output set at CCR, reset at timer overflow)
- Mode 4: Toggle (output toggles when timer reaches CCR)
- Mode 5: Reset (output reset when timer reaches CCR)
- Mode 6: Toggle/Set (output toggles at CCR, sets at timer overflow)
- Mode 7: Reset/Set (output reset at CCR, set at timer overflow)

## Timer Interrupt Generation

The timer generates interrupts from:

- **Timer overflow**: TAxR.RC signal sets TAIFG in TAxCTL register
- **Capture/Compare events**: Each CCR block can set individual CCIFG flags
- **Capture overflow**: COV flag indicates capture occurred before previous capture was read

## Block Interconnections

The diagram shows extensive interconnections:

- Timer count bus connects to all CCR blocks
- Timer clock distributes to synchronization logic
- EQU0 from CCR0 provides period control for up mode
- Output signals from each CCR feed back to output mode logic
- Clear and mode signals coordinate overall timer behavior

This architecture provides flexible timing, capturing, and PWM generation capabilities
essential for microcontroller applications.

## References

- MSP430FR2xx/FR4xx Family User's Guide (SLAU445I), Section 13.1
- Original Figure 13-1: Timer_A Block Diagram
