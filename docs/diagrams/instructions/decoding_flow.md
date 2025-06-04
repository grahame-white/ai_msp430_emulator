# MSP430 Instruction Decoding Flow

This document provides flowcharts showing the decision-making process for decoding MSP430 instructions.

## Main Instruction Decoding Flow

```mermaid
flowchart TD
    Start([16-bit Instruction Word]) --> FormatDetect{Determine Format}
    
    FormatDetect -->|Bits 15:13 = 001| FormatIII[Format III - Jump]
    FormatDetect -->|Bits 15:8 = 0x10-0x13| FormatII[Format II - Single Operand]
    FormatDetect -->|Bits 15:12 = 0x4-0xF| FormatI[Format I - Two Operand]
    FormatDetect -->|Other patterns| Invalid[Invalid Instruction]
    
    FormatI --> ExtractI[Extract Format I Fields]
    FormatII --> ExtractII[Extract Format II Fields]
    FormatIII --> ExtractIII[Extract Format III Fields]
    
    ExtractI --> ValidateI{Validate Fields}
    ExtractII --> ValidateII{Validate Fields}
    ExtractIII --> ValidateIII{Validate Fields}
    
    ValidateI -->|Valid| DecodeI[Decode Addressing Modes]
    ValidateI -->|Invalid| Invalid
    ValidateII -->|Valid| DecodeII[Decode Addressing Mode]
    ValidateII -->|Invalid| Invalid
    ValidateIII -->|Valid| DecodeIII[Sign Extend Offset]
    ValidateIII -->|Invalid| Invalid
    
    DecodeI --> CreateI[Create Format I Instruction]
    DecodeII --> CreateII[Create Format II Instruction]
    DecodeIII --> CreateIII[Create Format III Instruction]
    
    CreateI --> Success([Instruction Object])
    CreateII --> Success
    CreateIII --> Success
    
    Invalid --> Exception[Throw InvalidInstructionException]
    
    style Start fill:#e3f2fd
    style Success fill:#c8e6c9
    style Invalid fill:#ffebee
    style Exception fill:#f44336,color:#fff
```

## Format I Decoding Detail

```mermaid
flowchart TD
    FormatI[Format I Instruction Word] --> ExtractOpcode[Extract Opcode<br/>Bits 15:12]
    ExtractOpcode --> ExtractSrc[Extract Source Register<br/>Bits 11:8]
    ExtractSrc --> ExtractAd[Extract Ad Bit<br/>Bit 7]
    ExtractAd --> ExtractBW[Extract B/W Bit<br/>Bit 6]
    ExtractBW --> ExtractAs[Extract As Bits<br/>Bits 5:4]
    ExtractAs --> ExtractDst[Extract Destination Register<br/>Bits 3:0]
    
    ExtractDst --> ValidateOpcode{Opcode 4-15?}
    ValidateOpcode -->|No| Invalid[Invalid Instruction]
    ValidateOpcode -->|Yes| ValidateRegs{Valid Registers?}
    
    ValidateRegs -->|No| Invalid
    ValidateRegs -->|Yes| DecodeSrcMode[Decode Source Addressing Mode]
    
    DecodeSrcMode --> DecodeDstMode[Decode Destination Addressing Mode]
    DecodeDstMode --> CheckModes{Valid Modes?}
    
    CheckModes -->|No| Invalid
    CheckModes -->|Yes| CalcExtWords[Calculate Extension Words]
    
    CalcExtWords --> CreateInstruction[Create Format I Instruction Object]
    CreateInstruction --> Success([Complete])
    
    Invalid --> Exception[Throw Exception]
    
    style FormatI fill:#e3f2fd
    style Success fill:#c8e6c9
    style Invalid fill:#ffebee
    style Exception fill:#f44336,color:#fff
```

## Format II Decoding Detail

```mermaid
flowchart TD
    FormatII[Format II Instruction Word] --> ExtractOpcode[Extract Opcode<br/>Bits 15:8]
    ExtractOpcode --> ExtractBW[Extract B/W Bit<br/>Bit 6]
    ExtractBW --> ExtractAs[Extract As Bits<br/>Bits 5:4]
    ExtractAs --> ExtractSrc[Extract Source Register<br/>Bits 3:0]
    
    ExtractSrc --> ValidateOpcode{Opcode 0x10-0x13?}
    ValidateOpcode -->|No| Invalid[Invalid Instruction]
    ValidateOpcode -->|Yes| ValidateReg{Valid Register?}
    
    ValidateReg -->|No| Invalid
    ValidateReg -->|Yes| DecodeSrcMode[Decode Source Addressing Mode]
    
    DecodeSrcMode --> CheckMode{Valid Mode?}
    CheckMode -->|No| Invalid
    CheckMode -->|Yes| CalcExtWords[Calculate Extension Words]
    
    CalcExtWords --> CreateInstruction[Create Format II Instruction Object]
    CreateInstruction --> Success([Complete])
    
    Invalid --> Exception[Throw Exception]
    
    style FormatII fill:#e3f2fd
    style Success fill:#c8e6c9
    style Invalid fill:#ffebee
    style Exception fill:#f44336,color:#fff
```

## Format III Decoding Detail

```mermaid
flowchart TD
    FormatIII[Format III Instruction Word] --> CheckPrefix[Check Prefix<br/>Bits 15:13 = 001?]
    CheckPrefix -->|No| Invalid[Invalid Instruction]
    CheckPrefix -->|Yes| ExtractCond[Extract Condition<br/>Bits 12:10]
    
    ExtractCond --> ExtractOffset[Extract Offset<br/>Bits 9:0]
    ExtractOffset --> ValidateCond{Condition 0-7?}
    
    ValidateCond -->|No| Invalid
    ValidateCond -->|Yes| SignExtend[Sign Extend Offset<br/>10 bits → 16 bits]
    
    SignExtend --> CreateInstruction[Create Format III Instruction Object]
    CreateInstruction --> Success([Complete])
    
    Invalid --> Exception[Throw Exception]
    
    style FormatIII fill:#e3f2fd
    style Success fill:#c8e6c9
    style Invalid fill:#ffebee
    style Exception fill:#f44336,color:#fff
```

## Format Detection Logic

```mermaid
flowchart TD
    Word[16-bit Instruction Word] --> CheckJump{Bits 15:13<br/>== 001?}
    
    CheckJump -->|Yes| FormatIII[Format III<br/>Jump Instructions]
    CheckJump -->|No| CheckSingle{Bits 15:8<br/>== 0x10-0x13?}
    
    CheckSingle -->|Yes| FormatII[Format II<br/>Single Operand]
    CheckSingle -->|No| CheckTwo{Bits 15:12<br/>== 0x4-0xF?}
    
    CheckTwo -->|Yes| FormatI[Format I<br/>Two Operand]
    CheckTwo -->|No| Invalid[Invalid/Reserved]
    
    style Word fill:#e3f2fd
    style FormatI fill:#e8f5e8
    style FormatII fill:#fff3e0
    style FormatIII fill:#f3e5f5
    style Invalid fill:#ffebee
```

## Error Handling Flow

```mermaid
flowchart TD
    Error[Decoding Error Detected] --> ErrorType{Error Type}
    
    ErrorType -->|Invalid Opcode| LogOpcode[Log: Invalid opcode<br/>Include instruction word]
    ErrorType -->|Invalid Register| LogRegister[Log: Invalid register number<br/>Include register field]
    ErrorType -->|Invalid Addressing Mode| LogMode[Log: Invalid addressing mode<br/>Include mode bits]
    ErrorType -->|Format Mismatch| LogFormat[Log: Unrecognized format<br/>Include pattern]
    
    LogOpcode --> CreateException[Create InvalidInstructionException]
    LogRegister --> CreateException
    LogMode --> CreateException
    LogFormat --> CreateException
    
    CreateException --> SetProperties[Set Exception Properties:<br/>- InstructionWord<br/>- Error Message]
    SetProperties --> ThrowException[Throw Exception]
    
    ThrowException --> Caller[Return to Caller<br/>with Exception]
    
    style Error fill:#ffebee
    style CreateException fill:#ff9800
    style ThrowException fill:#f44336,color:#fff
    style Caller fill:#9e9e9e
```

## Validation Process

```mermaid
flowchart TD
    Validate[Start Validation] --> ValidateFormat[Check Format Recognition]
    ValidateFormat -->|Pass| ValidateOpcode[Check Opcode Range]
    ValidateFormat -->|Fail| FailFormat[Format Error]
    
    ValidateOpcode -->|Pass| ValidateRegisters[Check Register Numbers]
    ValidateOpcode -->|Fail| FailOpcode[Opcode Error]
    
    ValidateRegisters -->|Pass| ValidateAddressing[Check Addressing Modes]
    ValidateRegisters -->|Fail| FailRegister[Register Error]
    
    ValidateAddressing -->|Pass| Success[Validation Passed]
    ValidateAddressing -->|Fail| FailAddressing[Addressing Error]
    
    FailFormat --> Error[Validation Failed]
    FailOpcode --> Error
    FailRegister --> Error
    FailAddressing --> Error
    
    Success --> Continue[Continue Decoding]
    Error --> Exception[Throw Exception]
    
    style Validate fill:#e3f2fd
    style Success fill:#c8e6c9
    style Continue fill:#c8e6c9
    style Error fill:#ffebee
    style Exception fill:#f44336,color:#fff
```

## Performance Considerations

The decoding process is optimized for performance:

1. **Early Exit**: Invalid instructions are detected as early as possible
2. **Bit Masking**: Efficient bit operations for field extraction
3. **Lookup Tables**: Fast opcode and addressing mode validation
4. **Minimal Allocations**: Reuse of temporary variables where possible

## Debugging Support

The decoder provides comprehensive error information:

- **Original instruction word** preserved in all instruction objects
- **Detailed error messages** with specific failure reasons
- **Bit-level field values** for debugging instruction encoding
- **Exception context** includes the problematic instruction word