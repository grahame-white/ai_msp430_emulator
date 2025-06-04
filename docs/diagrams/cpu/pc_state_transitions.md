# MSP430 Program Counter State Transitions

## Program Counter (PC/R0) State Management

The Program Counter (PC) operates in distinct states with specific transition conditions. This document provides comprehensive state diagrams and flowcharts for PC behavior.

## PC State Overview

### Primary PC States

```mermaid
stateDiagram-v2
    [*] --> Reset
    Reset --> Normal : System startup/power-on
    Normal --> Branch : Branch/Jump instruction
    Normal --> Call : CALL instruction  
    Normal --> Interrupt : Interrupt request (GIE=1)
    Normal --> Error : Invalid condition
    
    Branch --> Normal : Branch completed
    Call --> Subroutine : Enter subroutine
    Subroutine --> Return : RET instruction
    Return --> Normal : Return completed
    
    Interrupt --> InterruptHandler : Save context
    InterruptHandler --> Normal : RETI instruction
    
    Error --> Reset : System reset
    Error --> Normal : Error recovery
    
    state Reset {
        [*] --> PowerOn
        PowerOn --> Initialize
        Initialize --> SetPC
        SetPC --> [*]
        
        PowerOn : PC = 0x0000
        Initialize : Load reset vector
        SetPC : PC = reset vector address
    }
    
    state Normal {
        [*] --> Fetch
        Fetch --> Decode
        Decode --> Execute
        Execute --> Increment
        Increment --> Fetch
        
        Fetch : Read instruction at PC
        Decode : Decode instruction
        Execute : Execute instruction  
        Increment : PC += 2 (word aligned)
    }
    
    state Branch {
        [*] --> CheckCondition
        CheckCondition --> SetTarget : Condition true
        CheckCondition --> NoAction : Condition false
        SetTarget --> AlignPC
        AlignPC --> [*]
        NoAction --> [*]
        
        CheckCondition : Evaluate branch condition
        SetTarget : PC = target address
        AlignPC : PC &= 0xFFFE
        NoAction : Continue with next instruction
    }
    
    state Call {
        [*] --> PushReturn
        PushReturn --> SetTarget
        SetTarget --> [*]
        
        PushReturn : Push PC+2 to stack
        SetTarget : PC = call target
    }
    
    state Interrupt {
        [*] --> DisableGIE
        DisableGIE --> PushContext
        PushContext --> LoadVector
        LoadVector --> [*]
        
        DisableGIE : GIE = 0
        PushContext : Push PC and SR to stack
        LoadVector : PC = interrupt vector
    }
```

## Detailed State Behaviors

### Reset State Operations

```mermaid
flowchart TD
    A[System Reset/Power-On] --> B[PC = 0x0000]
    B --> C[Read Reset Vector]
    C --> D{Valid Vector?}
    D -->|Yes| E[PC = Vector Address]
    D -->|No| F[PC = 0x0000]
    E --> G[PC Word Aligned]
    F --> G
    G --> H[Enter Normal State]
    
    subgraph "Reset Vector Process"
        C --> C1[Read from 0xFFFE]
        C1 --> C2[Combine low/high bytes]
        C2 --> C3[Validate address range]
    end
```

### Normal Execution Cycle

```mermaid
flowchart TD
    A[Fetch Instruction] --> B[PC points to instruction]
    B --> C[Read 16-bit instruction]
    C --> D[Decode Instruction]
    D --> E[Execute Instruction]
    E --> F[Increment PC by 2]
    F --> G[PC Word Alignment Check]
    G --> H{PC & 1 == 0?}
    H -->|Yes| A
    H -->|No| I[Force PC &= 0xFFFE]
    I --> A
    
    subgraph "PC Increment Details"
        F --> F1[Current PC + 2]
        F1 --> F2[Handle overflow]
        F2 --> F3[Check bounds]
    end
```

### Branch/Jump Operations

```mermaid
flowchart TD
    A[Branch/Jump Instruction] --> B{Conditional Branch?}
    B -->|Yes| C[Evaluate Condition]
    B -->|No| D[Unconditional Jump]
    
    C --> E{Condition True?}
    E -->|Yes| F[Calculate Target]
    E -->|No| G[Continue Sequential]
    
    D --> F
    F --> H[Set PC = Target]
    H --> I[Force Word Alignment]
    I --> J[PC &= 0xFFFE]
    J --> K[Validate Address Range]
    K --> L{Valid Address?}
    L -->|Yes| M[Continue Execution]
    L -->|No| N[Error State]
    
    G --> O[PC += 2]
    O --> M
    
    subgraph "Target Calculation"
        F --> F1[Base Address]
        F1 --> F2[Add Offset]
        F2 --> F3[Apply Addressing Mode]
    end
```

### Subroutine Call/Return Flow

```mermaid
flowchart TD
    A[CALL Instruction] --> B[Calculate Return Address]
    B --> C[Return = PC + 2]
    C --> D[Push Return to Stack]
    D --> E[SP -= 2]
    E --> F[Memory[SP] = Return]
    F --> G[Set PC = Call Target]
    G --> H[Word Align PC]
    H --> I[Enter Subroutine]
    
    I --> J[Execute Subroutine]
    J --> K[RET Instruction]
    K --> L[Pop Return Address]
    L --> M[Return = Memory[SP]]
    M --> N[SP += 2]
    N --> O[PC = Return Address]
    O --> P[Word Align PC]
    P --> Q[Continue Main Program]
    
    subgraph "Stack Operations"
        D --> D1[Check Stack Bounds]
        D1 --> D2[Verify SP Alignment]
        L --> L1[Check Stack Bounds]
        L1 --> L2[Verify SP Alignment]
    end
```

### Interrupt Processing

```mermaid
flowchart TD
    A[Interrupt Request] --> B{GIE Flag Set?}
    B -->|No| C[Ignore Interrupt]
    B -->|Yes| D[Complete Current Instruction]
    
    D --> E[Save Current PC]
    E --> F[Push PC to Stack]
    F --> G[Push SR to Stack]
    G --> H[Clear GIE Flag]
    H --> I[Read Interrupt Vector]
    I --> J[PC = Vector Address]
    J --> K[Word Align PC]
    K --> L[Execute ISR]
    
    L --> M[RETI Instruction]
    M --> N[Pop SR from Stack]
    N --> O[Pop PC from Stack]
    O --> P[Restore GIE Flag]
    P --> Q[Continue Normal Execution]
    
    C --> R[Continue Normal Execution]
    
    subgraph "Context Save/Restore"
        F --> F1[SP -= 2, Memory[SP] = PC]
        G --> G1[SP -= 2, Memory[SP] = SR]
        N --> N1[SR = Memory[SP], SP += 2]
        O --> O1[PC = Memory[SP], SP += 2]
    end
```

## Stack Pointer Interaction

### Stack Operations During PC Changes

```mermaid
flowchart TD
    A[Stack Operation Required] --> B{Operation Type}
    B -->|CALL| C[CALL Process]
    B -->|RET| D[RET Process]  
    B -->|Interrupt| E[Interrupt Process]
    B -->|RETI| F[RETI Process]
    
    C --> C1[SP Alignment Check]
    C1 --> C2[SP &= 0xFFFE]
    C2 --> C3[SP -= 2]
    C3 --> C4[Push Return Address]
    C4 --> C5[Set PC = Target]
    
    D --> D1[SP Alignment Check]
    D1 --> D2[Pop Return Address]
    D2 --> D3[SP += 2]
    D3 --> D4[PC = Return Address]
    D4 --> D5[PC Word Alignment]
    
    E --> E1[Push PC and SR]
    E1 --> E2[SP -= 4]
    E2 --> E3[Set PC = Vector]
    
    F --> F1[Pop SR and PC]
    F1 --> F2[SP += 4]
    F2 --> F3[Restore Context]
    
    subgraph "SP Validation"
        C1 --> V1[Check SP is even]
        D1 --> V1
        V1 --> V2{SP & 1 == 0?}
        V2 -->|No| V3[Force SP &= 0xFFFE]
        V2 -->|Yes| V4[Continue]
        V3 --> V4
    end
```

## PC Word Alignment Enforcement

### Alignment Rules and Enforcement

```mermaid
flowchart TD
    A[PC Value Assignment] --> B[New PC Value]
    B --> C{Alignment Check}
    C -->|PC & 1 == 0| D[Already Aligned]
    C -->|PC & 1 == 1| E[Force Alignment]
    
    E --> F[PC = PC & 0xFFFE]
    F --> G[Log Alignment Warning]
    G --> D
    
    D --> H[Validate Address Range]
    H --> I{Valid Memory Range?}
    I -->|Yes| J[Accept PC Value]
    I -->|No| K[Error Handling]
    
    K --> L[Log Error]
    L --> M[Set Error State]
    
    J --> N[Update PC Register]
    N --> O[Continue Operation]
    
    subgraph "Alignment Examples"
        A1[PC = 0x8001] --> A2[Becomes 0x8000]
        A3[PC = 0x8003] --> A4[Becomes 0x8002]
        A5[PC = 0x8000] --> A6[Remains 0x8000]
    end
```

## Error Conditions and Recovery

### PC Error State Management

```mermaid
stateDiagram-v2
    [*] --> NormalOperation
    NormalOperation --> ErrorDetected : Invalid condition
    
    state ErrorDetected {
        [*] --> InvalidAddress
        [*] --> Misalignment
        [*] --> OutOfBounds
        
        InvalidAddress --> LogError
        Misalignment --> ForceAlignment
        OutOfBounds --> BoundsCheck
        
        LogError --> ErrorRecovery
        ForceAlignment --> ErrorRecovery
        BoundsCheck --> ErrorRecovery
        
        ErrorRecovery --> [*]
    }
    
    ErrorDetected --> AutoCorrect : Correctable error
    ErrorDetected --> SystemReset : Fatal error
    
    AutoCorrect --> NormalOperation : Correction applied
    SystemReset --> [*] : System restart required
    
    NormalOperation --> NormalOperation : Valid operations
```

### Error Recovery Flowchart

```mermaid
flowchart TD
    A[PC Error Detected] --> B{Error Type}
    B -->|Misalignment| C[Auto-correct to even address]
    B -->|Invalid Range| D[Check if correctable]
    B -->|Stack Overflow| E[Stack pointer recovery]
    
    C --> F[PC &= 0xFFFE]
    F --> G[Log Warning]
    G --> H[Continue Operation]
    
    D --> I{Within valid memory?}
    I -->|Yes| J[Allow with warning]
    I -->|No| K[Force to safe address]
    
    E --> L[Reset SP to safe value]
    L --> M[Clear call stack]
    M --> N[Return to main program]
    
    J --> H
    K --> O[PC = 0x0000 or Reset Vector]
    O --> P[Log Error]
    P --> Q[System Reset]
    
    H --> R[Resume Normal Operation]
    N --> R
    Q --> S[Complete System Restart]
```

## Implementation Notes

### Register File Integration

1. **PC Storage**: Stored in register array index 0 (`_registers[0]`)
2. **Word Alignment**: Enforced on all PC writes (`value & 0xFFFE`)
3. **Range Validation**: Checked against valid memory regions
4. **Logging**: Debug-level logging for all PC changes

### Special Behaviors

1. **Automatic Increment**: PC increments by 2 during instruction fetch
2. **Alignment Enforcement**: Odd addresses automatically rounded down
3. **Stack Integration**: PC saved/restored during calls and interrupts
4. **Error Handling**: Invalid addresses logged and corrected where possible

### Usage Examples

```csharp
// Direct PC manipulation
registerFile.SetProgramCounter(0x8000);
ushort pc = registerFile.GetProgramCounter();

// Instruction fetch simulation
registerFile.IncrementProgramCounter(); // PC += 2

// Automatic alignment
registerFile.SetProgramCounter(0x8001); // Becomes 0x8000

// Call/return simulation
ushort returnAddress = registerFile.GetProgramCounter() + 2;
// Push returnAddress to stack via SP operations
registerFile.SetProgramCounter(callTarget);
```