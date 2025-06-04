# MSP430 Addressing Mode Decision Trees

This document provides decision flowcharts for decoding MSP430 addressing modes based on register numbers and As/Ad bits.

## Source Addressing Mode Decoding

```mermaid
flowchart TD
    Start[As Bits + Source Register] --> CheckAs{As Bits Value}
    
    CheckAs -->|00| Register[Register Mode<br/>Operand in Rn]
    CheckAs -->|01| CheckR0_01{Register == R0?}
    CheckAs -->|10| CheckR0_10{Register == R0?}
    CheckAs -->|11| CheckR0_11{Register == R0?}
    
    CheckR0_01 -->|Yes| Symbolic[Symbolic Mode<br/>PC + Offset]
    CheckR0_01 -->|No| CheckR2_01{Register == R2?}
    
    CheckR2_01 -->|Yes| Absolute[Absolute Mode<br/>Direct Address]
    CheckR2_01 -->|No| Indexed[Indexed Mode<br/>Rn + Offset]
    
    CheckR0_10 -->|Yes| Indirect_R0[Indirect Mode<br/>@R0]
    CheckR0_10 -->|No| IndirectSrc[Indirect Mode<br/>@Rn]
    
    CheckR0_11 -->|Yes| Immediate[Immediate Mode<br/>#Value]
    CheckR0_11 -->|No| CheckR2_11{Register == R2?}
    
    CheckR2_11 -->|Yes| Const4[Constant #4<br/>Hardcoded]
    CheckR2_11 -->|No| CheckR3_11{Register == R3?}
    
    CheckR3_11 -->|Yes| ConstM1[Constant #-1<br/>0xFFFF]
    CheckR3_11 -->|No| IndirectInc[Indirect Auto-increment<br/>@Rn+]
    
    style Start fill:#e3f2fd
    style Register fill:#e8f5e8
    style Symbolic fill:#fff3e0
    style Absolute fill:#f3e5f5
    style Indexed fill:#e1f5fe
    style IndirectSrc fill:#fce4ec
    style Indirect_R0 fill:#fce4ec
    style Immediate fill:#e8f5e8
    style Const4 fill:#e8f5e8
    style ConstM1 fill:#e8f5e8
    style IndirectInc fill:#fce4ec
```

## R3 Constant Generator Special Cases

```mermaid
flowchart TD
    R3[Register R3] --> CheckAs{As Bits}
    
    CheckAs -->|00| Const0[Constant #0<br/>Register Mode]
    CheckAs -->|01| Const1[Constant #1<br/>Indexed Mode Special]
    CheckAs -->|10| Const2[Constant #2<br/>Indirect Mode Special]
    CheckAs -->|11| ConstNeg1[Constant #-1<br/>Auto-increment Special]
    
    Const0 --> Usage0[Used for: CLR operations,<br/>Zero comparisons]
    Const1 --> Usage1[Used for: INC operations,<br/>Loop counters]
    Const2 --> Usage2[Used for: Word arithmetic,<br/>Pointer increments]
    ConstNeg1 --> UsageNeg1[Used for: DEC operations,<br/>Bit masks]
    
    style R3 fill:#e3f2fd
    style Const0 fill:#c8e6c9
    style Const1 fill:#c8e6c9
    style Const2 fill:#c8e6c9
    style ConstNeg1 fill:#c8e6c9
    style Usage0 fill:#f0f4c3
    style Usage1 fill:#f0f4c3
    style Usage2 fill:#f0f4c3
    style UsageNeg1 fill:#f0f4c3
```

## Destination Addressing Mode Decoding

```mermaid
flowchart TD
    Start[Ad Bit + Destination Register] --> CheckAd{Ad Bit Value}
    
    CheckAd -->|0| Register[Register Mode<br/>Result to Rn]
    CheckAd -->|1| CheckR0{Register == R0?}
    
    CheckR0 -->|Yes| Symbolic[Symbolic Mode<br/>PC + Offset]
    CheckR0 -->|No| CheckR2{Register == R2?}
    
    CheckR2 -->|Yes| Absolute[Absolute Mode<br/>Direct Address]
    CheckR2 -->|No| Indexed[Indexed Mode<br/>Rn + Offset]
    
    style Start fill:#e3f2fd
    style Register fill:#e8f5e8
    style Symbolic fill:#fff3e0
    style Absolute fill:#f3e5f5
    style Indexed fill:#e1f5fe
```

## Extension Word Requirements

```mermaid
flowchart TD
    Mode[Addressing Mode] --> CheckMode{Mode Type}
    
    CheckMode --> Register[Register]
    CheckMode --> Indexed[Indexed]
    CheckMode --> IndirectExt[Indirect]
    CheckMode --> IndirectInc[Indirect Auto-increment]
    CheckMode --> Immediate[Immediate]
    CheckMode --> Absolute[Absolute]
    CheckMode --> Symbolic[Symbolic]
    
    Register --> NoExtension[No Extension Word]
    IndirectExt --> NoExtension
    IndirectInc --> NoExtension
    
    Indexed --> NeedsExtension[Needs Extension Word<br/>Contains Offset]
    Immediate --> NeedsExtension2[Needs Extension Word<br/>Contains Immediate Value]
    Absolute --> NeedsExtension3[Needs Extension Word<br/>Contains Absolute Address]
    Symbolic --> NeedsExtension4[Needs Extension Word<br/>Contains PC Offset]
    
    style Mode fill:#e3f2fd
    style NoExtension fill:#c8e6c9
    style NeedsExtension fill:#ffeb3b
    style NeedsExtension2 fill:#ffeb3b
    style NeedsExtension3 fill:#ffeb3b
    style NeedsExtension4 fill:#ffeb3b
```

## Complete Addressing Mode Matrix

```mermaid
flowchart TD
    subgraph "Source Modes (As Bits)"
        As00[As=00<br/>Register Direct]
        As01[As=01<br/>Indexed/Symbolic/Absolute]
        As10[As=10<br/>Indirect]
        As11[As=11<br/>Indirect++/Immediate]
    end
    
    subgraph "Special Register Handling"
        R0[R0 - PC]
        R2[R2 - SR]
        R3[R3 - CG1]
        RN[R4-R15 - General]
    end
    
    subgraph "Destination Modes (Ad Bit)"
        Ad0[Ad=0<br/>Register Direct]
        Ad1[Ad=1<br/>Indexed/Symbolic/Absolute]
    end
    
    As00 --> RegDirect[Register Direct<br/>@Rn]
    
    As01 --> R0 --> Symbolic[Symbolic<br/>PC+offset]
    As01 --> R2 --> Absolute[Absolute<br/>Direct address]
    As01 --> RN --> Indexed[Indexed<br/>Rn+offset]
    
    As10 --> IndirectMatrix[Indirect<br/>@Rn]
    
    As11 --> R0_Imm[R0: Immediate<br/>#constant]
    As11 --> R2_C4[R2: Constant #4]
    As11 --> R3_CN1[R3: Constant #-1]
    As11 --> RN_Inc[Rn: Indirect++<br/>@Rn+]
    
    Ad0 --> RegDirectDst[Register Direct<br/>Rn]
    Ad1 --> R0Dst[R0: Symbolic] 
    Ad1 --> R2Dst[R2: Absolute]
    Ad1 --> RNDst[Rn: Indexed]
    
    style As00 fill:#e8f5e8
    style As01 fill:#fff3e0
    style As10 fill:#fce4ec
    style As11 fill:#e1f5fe
    style Ad0 fill:#e8f5e8
    style Ad1 fill:#fff3e0
```

## Addressing Mode Validation

```mermaid
flowchart TD
    Input[Register + Mode Bits] --> ValidateReg{Register 0-15?}
    
    ValidateReg -->|No| InvalidReg[Invalid Register<br/>Error]
    ValidateReg -->|Yes| ValidateAs{As Bits 0-3?}
    
    ValidateAs -->|No| InvalidAs[Invalid As Bits<br/>Error]
    ValidateAs -->|Yes| CheckSpecial{Special Register?}
    
    CheckSpecial -->|R0| ValidateR0[Validate R0 Modes]
    CheckSpecial -->|R2| ValidateR2[Validate R2 Modes]
    CheckSpecial -->|R3| ValidateR3[Validate R3 Modes]
    CheckSpecial -->|R4-R15| ValidateGeneral[Validate General Modes]
    
    ValidateR0 --> AllValid[All R0 modes valid<br/>00,01,10,11]
    ValidateR2 --> AllValid2[All R2 modes valid<br/>00,01,10,11]
    ValidateR3 --> AllValid3[All R3 modes valid<br/>00,01,10,11<br/>Generate constants]
    ValidateGeneral --> AllValid4[All general modes valid<br/>00,01,10,11]
    
    AllValid --> Success[Valid Mode]
    AllValid2 --> Success
    AllValid3 --> Success
    AllValid4 --> Success
    
    InvalidReg --> Error[Return Invalid]
    InvalidAs --> Error
    
    style Input fill:#e3f2fd
    style Success fill:#c8e6c9
    style Error fill:#ffebee
    style InvalidReg fill:#ffebee
    style InvalidAs fill:#ffebee
```

## Implementation Notes

### Register-Specific Behaviors

**R0 (Program Counter):**
- As=00: Register mode (current PC value)
- As=01: Symbolic mode (PC + extension word)
- As=10: Indirect mode (@PC - unusual but valid)
- As=11: Immediate mode (#constant from extension word)

**R2 (Status Register):**
- As=00: Register mode (current SR value)
- As=01: Absolute mode (&address from extension word)
- As=10: Indirect mode (@SR - unusual but valid)
- As=11: Constant #4 (no extension word needed)

**R3 (Constant Generator):**
- As=00: Constant #0
- As=01: Constant #1
- As=10: Constant #2
- As=11: Constant #-1 (0xFFFF)

**R4-R15 (General Purpose):**
- As=00: Register mode (register content)
- As=01: Indexed mode (register + extension word offset)
- As=10: Indirect mode (@register)
- As=11: Indirect auto-increment (@register, then register++)

### Extension Word Usage

When an addressing mode requires an extension word:
1. **Source operand extension word** comes immediately after instruction
2. **Destination operand extension word** comes after source extension word
3. Extension words are 16-bit values
4. Multiple extension words are read in program order

### Performance Optimization

The addressing mode decoder is optimized using:
1. **Switch expressions** for fast register-specific dispatching
2. **Bit masking** for efficient field extraction
3. **Early validation** to catch errors quickly
4. **Lookup tables** for constant generator values