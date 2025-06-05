# MSP430 Addressing Mode Decision Trees

This document provides decision flowcharts for decoding MSP430 addressing modes based on register numbers and As/Ad bits.

## Source Addressing Mode Decoding

```mermaid
flowchart TD
    Start1[As Bits + Source Register] --> CheckAs1{As Bits Value}
    
    CheckAs1 -->|00| Register1[Register Mode<br/>Operand in Rn]
    CheckAs1 -->|01| CheckR0_01{Register == R0?}
    CheckAs1 -->|10| CheckR0_10{Register == R0?}
    CheckAs1 -->|11| CheckR0_11{Register == R0?}
    
    CheckR0_01 -->|Yes| Symbolic1[Symbolic Mode<br/>PC + Offset]
    CheckR0_01 -->|No| CheckR2_01{Register == R2?}
    
    CheckR2_01 -->|Yes| Absolute1[Absolute Mode<br/>Direct Address]
    CheckR2_01 -->|No| Indexed1[Indexed Mode<br/>Rn + Offset]
    
    CheckR0_10 -->|Yes| Indirect_R0[Indirect Mode<br/>@R0]
    CheckR0_10 -->|No| IndirectSrc[Indirect Mode<br/>@Rn]
    
    CheckR0_11 -->|Yes| Immediate1[Immediate Mode<br/>#Value]
    CheckR0_11 -->|No| CheckR2_11{Register == R2?}
    
    CheckR2_11 -->|Yes| Const4_1[Constant #4<br/>Hardcoded]
    CheckR2_11 -->|No| CheckR3_11{Register == R3?}
    
    CheckR3_11 -->|Yes| ConstM1_1[Constant #-1<br/>0xFFFF]
    CheckR3_11 -->|No| IndirectInc1[Indirect Auto-increment<br/>@Rn+]
    
    style Start1 fill:#e3f2fd
    style Register1 fill:#e8f5e8
    style Symbolic1 fill:#fff3e0
    style Absolute1 fill:#f3e5f5
    style Indexed1 fill:#e1f5fe
    style IndirectSrc fill:#fce4ec
    style Indirect_R0 fill:#fce4ec
    style Immediate1 fill:#e8f5e8
    style Const4_1 fill:#e8f5e8
    style ConstM1_1 fill:#e8f5e8
    style IndirectInc1 fill:#fce4ec
```

## R3 Constant Generator Special Cases

```mermaid
flowchart TD
    R3_2[Register R3] --> CheckAs2{As Bits}
    
    CheckAs2 -->|00| Const0_2[Constant #0<br/>Register Mode]
    CheckAs2 -->|01| Const1_2[Constant #1<br/>Indexed Mode Special]
    CheckAs2 -->|10| Const2_2[Constant #2<br/>Indirect Mode Special]
    CheckAs2 -->|11| ConstNeg1_2[Constant #-1<br/>Auto-increment Special]
    
    Const0_2 --> Usage0_2[Used for: CLR operations,<br/>Zero comparisons]
    Const1_2 --> Usage1_2[Used for: INC operations,<br/>Loop counters]
    Const2_2 --> Usage2_2[Used for: Word arithmetic,<br/>Pointer increments]
    ConstNeg1_2 --> UsageNeg1_2[Used for: DEC operations,<br/>Bit masks]
    
    style R3_2 fill:#e3f2fd
    style Const0_2 fill:#c8e6c9
    style Const1_2 fill:#c8e6c9
    style Const2_2 fill:#c8e6c9
    style ConstNeg1_2 fill:#c8e6c9
    style Usage0_2 fill:#f0f4c3
    style Usage1_2 fill:#f0f4c3
    style Usage2_2 fill:#f0f4c3
    style UsageNeg1_2 fill:#f0f4c3
```

## Destination Addressing Mode Decoding

```mermaid
flowchart TD
    Start3[Ad Bit + Destination Register] --> CheckAd3{Ad Bit Value}
    
    CheckAd3 -->|0| Register3[Register Mode<br/>Result to Rn]
    CheckAd3 -->|1| CheckR0_3{Register == R0?}
    
    CheckR0_3 -->|Yes| Symbolic3[Symbolic Mode<br/>PC + Offset]
    CheckR0_3 -->|No| CheckR2_3{Register == R2?}
    
    CheckR2_3 -->|Yes| Absolute3[Absolute Mode<br/>Direct Address]
    CheckR2_3 -->|No| Indexed3[Indexed Mode<br/>Rn + Offset]
    
    style Start3 fill:#e3f2fd
    style Register3 fill:#e8f5e8
    style Symbolic3 fill:#fff3e0
    style Absolute3 fill:#f3e5f5
    style Indexed3 fill:#e1f5fe
```

## Extension Word Requirements

```mermaid
flowchart TD
    Mode4[Addressing Mode] --> CheckMode4{Mode Type}
    
    CheckMode4 --> Register4[Register]
    CheckMode4 --> Indexed4[Indexed]
    CheckMode4 --> IndirectExt[Indirect]
    CheckMode4 --> IndirectInc4[Indirect Auto-increment]
    CheckMode4 --> Immediate4[Immediate]
    CheckMode4 --> Absolute4[Absolute]
    CheckMode4 --> Symbolic4[Symbolic]
    
    Register4 --> NoExtension4[No Extension Word]
    IndirectExt --> NoExtension4
    IndirectInc4 --> NoExtension4
    
    Indexed4 --> NeedsExtension[Needs Extension Word<br/>Contains Offset]
    Immediate4 --> NeedsExtension2[Needs Extension Word<br/>Contains Immediate Value]
    Absolute4 --> NeedsExtension3[Needs Extension Word<br/>Contains Absolute Address]
    Symbolic4 --> NeedsExtension4[Needs Extension Word<br/>Contains PC Offset]
    
    style Mode4 fill:#e3f2fd
    style NoExtension4 fill:#c8e6c9
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
        R0_5[R0 - PC]
        R2_5[R2 - SR]
        R3_5[R3 - CG1]
        RN_5[R4-R15 - General]
    end
    
    subgraph "Destination Modes (Ad Bit)"
        Ad0[Ad=0<br/>Register Direct]
        Ad1[Ad=1<br/>Indexed/Symbolic/Absolute]
    end
    
    As00 --> RegDirect5[Register Direct<br/>@Rn]
    
    As01 --> R0_5 --> Symbolic5[Symbolic<br/>PC+offset]
    As01 --> R2_5 --> Absolute5[Absolute<br/>Direct address]
    As01 --> RN_5 --> Indexed5[Indexed<br/>Rn+offset]
    
    As10 --> IndirectMatrix5[Indirect Mode<br/>@Rn]
    
    As11 --> R0_Imm5[R0: Immediate<br/>#constant]
    As11 --> R2_C4_5[R2: Constant #4]
    As11 --> R3_CN1_5[R3: Constant #-1]
    As11 --> RN_Inc5[Rn: Indirect++<br/>@Rn+]
    
    Ad0 --> RegDirectDst5[Register Direct<br/>Rn]
    Ad1 --> R0Dst5[R0: Symbolic] 
    Ad1 --> R2Dst5[R2: Absolute]
    Ad1 --> RNDst5[Rn: Indexed]
    
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
    Input6[Register + Mode Bits] --> ValidateReg6{Register 0-15?}
    
    ValidateReg6 -->|No| InvalidReg6[Invalid Register<br/>Error]
    ValidateReg6 -->|Yes| ValidateAs6{As Bits 0-3?}
    
    ValidateAs6 -->|No| InvalidAs6[Invalid As Bits<br/>Error]
    ValidateAs6 -->|Yes| CheckSpecial6{Special Register?}
    
    CheckSpecial6 -->|R0| ValidateR0_6[Validate R0 Modes]
    CheckSpecial6 -->|R2| ValidateR2_6[Validate R2 Modes]
    CheckSpecial6 -->|R3| ValidateR3_6[Validate R3 Modes]
    CheckSpecial6 -->|R4-R15| ValidateGeneral6[Validate General Modes]
    
    ValidateR0_6 --> AllValid6[All R0 modes valid<br/>00,01,10,11]
    ValidateR2_6 --> AllValid2_6[All R2 modes valid<br/>00,01,10,11]
    ValidateR3_6 --> AllValid3_6[All R3 modes valid<br/>00,01,10,11<br/>Generate constants]
    ValidateGeneral6 --> AllValid4_6[All general modes valid<br/>00,01,10,11]
    
    AllValid6 --> Success6[Valid Mode]
    AllValid2_6 --> Success6
    AllValid3_6 --> Success6
    AllValid4_6 --> Success6
    
    InvalidReg6 --> Error6[Return Invalid]
    InvalidAs6 --> Error6
    
    style Input6 fill:#e3f2fd
    style Success6 fill:#c8e6c9
    style Error6 fill:#ffebee
    style InvalidReg6 fill:#ffebee
    style InvalidAs6 fill:#ffebee
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