# Timer A Figure 13-1 Visual Representation

This document provides visual representations of Timer A Figure 13-1. The documentation includes
both Mermaid diagrams for quick reference and detailed SVG diagrams that meet professional circuit
documentation standards.

## Professional SVG Diagrams

The following native SVG diagrams provide detailed technical illustrations:

- **[Timer A Main Block](timer_a_main_block.svg)** - Clock path and timer core with proper
  electrical symbols
- **[CCR Block Detail](timer_a_ccr_block.svg)** - Complete capture/compare register architecture
- **[Complete Timer A System](timer_a_complete_system.svg)** - System overview with all
  interconnections

These SVG diagrams feature:

- ✅ Appropriate symbols for each logic block
- ✅ No overlapping blocks  
- ✅ No crossing connections
- ✅ Branched style connections with multiple endpoints
- ✅ Connection dots showing connected vs crossing lines
- ✅ Orthogonal line routing
- ✅ Professional circuit diagram standards

## Mermaid Reference Diagrams

The original logic notation has been translated to standard flowcharts and block diagrams for
improved clarity.

## Main Timer Block Architecture

```mermaid
graph TB
    %% Clock Source Selection and Division
    subgraph "Clock Path"
        TAxCLK[TAxCLK]
        ACLK[ACLK]
        SMCLK[SMCLK]
        INCLK[INCLK]
        
        TASSEL[TASSEL<br/>2-bit MUX]
        ID[ID<br/>Clock Divider<br/>÷1,2,4,8]
        IDEX[IDEX<br/>Clock Divider<br/>÷1-8]
        
        TAxCLK --> TASSEL
        ACLK --> TASSEL
        SMCLK --> TASSEL
        INCLK --> TASSEL
        
        TASSEL --> ID
        ID --> IDEX
        IDEX --> TimerClock[Timer Clock]
    end
    
    %% Timer Core
    subgraph "Timer Core"
        TimerClock --> TAxR[TAxR<br/>16-bit Counter]
        TACLR[TACLR] --> TAxR
        TAxR --> Count[Count<br/>16-bit Bus]
        TAxR --> RC[RC<br/>Roll-over]
        RC --> TAIFG[TAIFG<br/>Timer Interrupt]
        
        MC[MC<br/>Mode Control]
        TAxR -.-> MC
        MC -.-> TAxR
        EQU0[EQU0] --> MC
    end
    
    %% Count Distribution
    Count --> CCR0[CCR0 Block]
    Count --> CCR1[CCR1 Block]
    Count --> CCRN[CCRn Blocks<br/>n=2-6]
    
    style TASSEL fill:#e1f5fe
    style ID fill:#e8f5e8
    style IDEX fill:#e8f5e8
    style TAxR fill:#fff3e0
    style MC fill:#f3e5f5
```

## Capture/Compare Register (CCR) Block Detail

```mermaid
graph TB
    %% Input Selection and Capture Logic
    subgraph "CCR Block (n=0-6)"
        subgraph "Input Selection"
            CCInA[CCInA]
            CCInB[CCInB]
            GND[GND]
            VCC[VCC]
            
            CCIS[CCIS<br/>2-bit MUX]
            
            CCInA --> CCIS
            CCInB --> CCIS
            GND --> CCIS
            VCC --> CCIS
            CCIS --> CCI[CCI Signal]
        end
        
        subgraph "Capture Logic"
            CCI --> CM[CM<br/>Capture Mode<br/>00: No capture<br/>01: Rising<br/>10: Falling<br/>11: Both]
            CM --> Logic[Logic Block]
            Logic --> COV[COV<br/>Capture Overflow]
            
            CM --> Sync[Sync Block]
            TimerClk[Timer Clock] --> Sync
            Sync --> SCS[SCS<br/>Sync Select]
            
            SCS --> CAP[CAP<br/>Capture MUX]
            Comparator[Comparator<br/>Equality] --> CAP
        end
        
        subgraph "Storage and Compare"
            CAP --> TAxCCRn[TAxCCRn<br/>16-bit Register]
            TimerCount[Timer Count<br/>16-bit Bus] --> TAxCCRn
            TAxCCRn --> Comparator
            TimerCount --> Comparator
            
            Comparator --> Equ[Equ Signal]
            CAP --> CCIFG[CCIFG<br/>Interrupt Flag]
        end
        
        subgraph "Output Logic"
            Equ --> OutputUnit[OutputUnit<br/>8 Modes]
            OUTMOD[OUTMOD<br/>3-bit] --> OutputUnit
            
            OUTMOD --> NOR1[NOR Gate]
            NOR1 --> NAND1[NAND Gate]
            NOR1 --> AND1[AND Gate]
            OUT[OUT Bit] --> AND1
            OUT --> NAND1
            
            NAND1 --> OR1[OR Gate]
            AND1 --> DataLatch[Data Latch<br/>RS Flip-Flop]
            OR1 --> DataLatch
            POR[POR] --> OR1
            
            OutputUnit --> DataLatch
            TimerClk --> DataLatch
            DataLatch --> OUT6[OUT6 Signal]
            DataLatch --> OutputUnit
            
            Sync --> CCILatch[CCI Latch]
            Equ --> CCILatch
            CCILatch --> SCCI[SCCI Signal]
        end
    end
    
    style CCIS fill:#e1f5fe
    style CM fill:#e8f5e8
    style TAxCCRn fill:#fff3e0
    style OutputUnit fill:#f3e5f5
    style DataLatch fill:#ffebee
```

## Complete Timer A System Overview

```mermaid
graph TB
    %% External Inputs
    subgraph "External Interfaces"
        ExtClks[External Clocks<br/>TAxCLK, ACLK<br/>SMCLK, INCLK]
        ExtInputs[External Inputs<br/>CCInA, CCInB]
        Controls[Control Signals<br/>TACLR, OUTMOD<br/>OUT, POR]
    end
    
    %% Main Timer
    subgraph "Timer A Main Block"
        ClockPath[Clock Selection<br/>& Division]
        TimerCore[16-bit Counter<br/>& Mode Control]
    end
    
    %% CCR Array
    subgraph "Capture/Compare Registers"
        CCR0Block[CCR0<br/>Period Control]
        CCR1Block[CCR1<br/>Compare/Capture]
        CCR2Block[CCR2<br/>Compare/Capture]
        CCRNBlocks[CCR3-6<br/>Compare/Capture]
    end
    
    %% Outputs and Interrupts
    subgraph "Outputs"
        TimerInt[Timer Interrupts<br/>TAIFG, CCIFG]
        OutputPins[Output Pins<br/>OUT0-6]
        StatusSigs[Status Signals<br/>SCCI, COV]
    end
    
    %% Connections
    ExtClks --> ClockPath
    ClockPath --> TimerCore
    TimerCore --> CCR0Block
    TimerCore --> CCR1Block
    TimerCore --> CCR2Block
    TimerCore --> CCRNBlocks
    
    ExtInputs --> CCR0Block
    ExtInputs --> CCR1Block
    ExtInputs --> CCR2Block
    ExtInputs --> CCRNBlocks
    
    Controls --> TimerCore
    Controls --> CCR0Block
    Controls --> CCR1Block
    Controls --> CCR2Block
    Controls --> CCRNBlocks
    
    CCR0Block --> TimerInt
    CCR1Block --> TimerInt
    CCR2Block --> TimerInt
    CCRNBlocks --> TimerInt
    
    CCR0Block --> OutputPins
    CCR1Block --> OutputPins
    CCR2Block --> OutputPins
    CCRNBlocks --> OutputPins
    
    CCR0Block --> StatusSigs
    CCR1Block --> StatusSigs
    CCR2Block --> StatusSigs
    CCRNBlocks --> StatusSigs
    
    %% Special connection for period control
    CCR0Block -.-> TimerCore
    
    style ClockPath fill:#e1f5fe
    style TimerCore fill:#fff3e0
    style CCR0Block fill:#e8f5e8
    style CCR1Block fill:#f3e5f5
    style CCR2Block fill:#f3e5f5
    style CCRNBlocks fill:#f3e5f5
```

## Output Mode Truth Table

The Timer A output unit supports 8 different output modes controlled by the 3-bit OUTMOD field:

| OUTMOD | Mode | Description | OUT Behavior |
|--------|------|-------------|--------------|
| 000 | Output | Direct output control | Follows OUT bit |
| 001 | Set | Set output on compare | Set when Count = CCR |
| 010 | Toggle/Reset | Toggle on compare, reset on overflow | Toggle at CCR, reset at timer overflow |
| 011 | Set/Reset | Set on compare, reset on overflow | Set at CCR, reset at timer overflow |
| 100 | Toggle | Toggle on compare | Toggle when Count = CCR |
| 101 | Reset | Reset output on compare | Reset when Count = CCR |
| 110 | Toggle/Set | Toggle on compare, set on overflow | Toggle at CCR, set at timer overflow |
| 111 | Reset/Set | Reset on compare, set on overflow | Reset at CCR, set at timer overflow |

## Timing Relationships

```mermaid
sequenceDiagram
    participant Clock as Timer Clock
    participant Counter as TAxR Counter
    participant CCR as TAxCCRn
    participant Output as Output Unit
    participant Int as Interrupts
    
    Clock->>Counter: Clock Edge
    Counter->>Counter: Increment Count
    Counter->>CCR: Count Value
    CCR->>CCR: Compare Count vs CCR
    alt Count = CCR Value
        CCR->>Output: Equality Signal
        CCR->>Int: Set CCIFG
        Output->>Output: Update Output per OUTMOD
    end
    alt Timer Overflow
        Counter->>Int: Set TAIFG
        Counter->>Output: Overflow Signal
    end
```

## Clock Division Example

For maximum flexibility, Timer A provides two-stage clock division:

```mermaid
graph LR
    InputClk[Input Clock<br/>1 MHz] --> ID[ID Divider<br/>÷8]
    ID --> IDEX[IDEX Divider<br/>÷7]
    IDEX --> FinalClk[Final Clock<br/>≈1.79 kHz]
    
    note1[Total Division: 8 × 7 = 56]
    note2[Final Frequency: 1MHz ÷ 56 ≈ 17.9 kHz]
```

This visual representation clarifies the complex interconnections shown in the original
Figure 13-1 and demonstrates how the various Timer A components work together to provide
flexible timing, capture, and PWM generation capabilities.

## References

- Original Figure 13-1 from MSP430FR2xx/FR4xx Family User's Guide (SLAU445I)
- Hardware Block Notation documentation: `docs/diagrams/notation/hardware_block_notation.md`
- Detailed explanation: `docs/diagrams/timer_a/figure_13_1_explanation.md`
