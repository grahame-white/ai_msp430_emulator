# Timer A SVG Diagrams

This directory contains professional SVG diagrams that visualize the Timer A logic blocks
described in the MSP430FR2xx/FR4xx Family User's Guide (SLAU445I).

## Available Diagrams

### 1. Timer A Main Block

**File**: [timer_a_main_block.svg](timer_a_main_block.svg)

Illustrates the main Timer A block containing:

- **Clock Path**: TASSEL multiplexor, ID divider (÷1,2,4,8), IDEX divider (÷1-8)
- **Timer Core**: TAxR 16-bit counter, Mode Control (MC)
- **Signal Distribution**: Timer clock and count bus distribution to CCR blocks

### 2. CCR (Capture/Compare Register) Block Detail

**File**: [timer_a_ccr_block.svg](timer_a_ccr_block.svg)

Detailed view of a single CCR block showing:

- **Input Selection**: CCIS multiplexor for signal routing
- **Capture Logic**: CM capture mode control, sync logic, SCS selection
- **Storage and Compare**: TAxCCRn register, 16-bit comparator, CCILatch
- **Output Logic**: OutputUnit4 with 8 modes, logic gates (NOR, AND, NAND, OR), DataLatch
  (RS flip-flop)

### 3. Complete Timer A System

**File**: [timer_a_complete_system.svg](timer_a_complete_system.svg)

System-level view showing:

- Main Timer A block with clock sources
- Multiple CCR blocks (CCR0-6) with their interconnections
- External inputs (clock sources, capture inputs, control signals)
- Outputs (PWM outputs OUT0-6, interrupts TAIFG/CCIFG, status signals SCCI/COV)
- Signal distribution and routing

## Diagram Features

All diagrams adhere to professional circuit documentation standards:

- ✅ **Appropriate symbols**: Uses standard electrical/logic symbols for each block type
- ✅ **No overlapping blocks**: Clear separation and spacing of all components  
- ✅ **No crossing connections**: All signal paths route without crossing each other
- ✅ **Branched connections**: Multi-endpoint signals use proper branching with connection dots
- ✅ **Connection dots**: Clear indication where lines connect vs. where they cross
- ✅ **Orthogonal routing**: All connections use horizontal/vertical lines for clarity
- ✅ **SVG format**: Native scalable vector graphics (not exported from other tools)

## Usage

These diagrams can be:

- Viewed directly in web browsers
- Embedded in documentation  
- Printed for reference
- Used in presentations
- Modified for educational purposes

## References

- MSP430FR2xx/FR4xx Family User's Guide (SLAU445I), Section 13.1
- [Timer A Introduction](../../references/SLAU445/13.1_timer_a_introduction.md)
- [Figure 13-1 Explanation](figure_13_1_explanation.md)
- [Figure 13-1 Analysis](figure_13_1_analysis.md)
