# Figure 4-1: MSP430X CPU Block Diagram

This document provides the block diagram referenced as Figure 4-1 in the MSP430X CPU (CPUX) documentation,
showing the internal architecture and data flow of the MSP430X CPU suitable for AI-assisted development.

## MSP430X CPU Block Diagram

*Reference: MSP430x5xx and MSP430x6xx Family User's Guide (SLAU455) - Section 4.1: "MSP430X CPU (CPUX) Introduction"*

```mermaid
block-beta
    columns 12
    
    block:cpu["MSP430X CPU (CPUX)"]:12
        block:fetch["Instruction Fetch Unit"]:3
            if["Instruction Fetch"]
            pc["Program Counter (R0)\n20-bit"]
            ic["Instruction Cache"]
        end
        
        block:decode["Instruction Decoder"]:3
            id["Instruction Decoder"]
            cu["Control Unit"]
            seq["Sequencer"]
        end
        
        block:exec["Execution Unit"]:3
            alu["Arithmetic Logic Unit\n(ALU)"]
            sf["Status Flags"]
            cg["Constant Generator\n(R2/R3)"]
        end
        
        block:regs["Register File"]:3
            rf["Register File\n(R0-R15)\n16 x 20-bit"]
            sp["Stack Pointer (R1)\n20-bit"]
            sr["Status Register (R2)\n16-bit"]
        end
        
        space:12
        
        block:memory["Memory Interface"]:12
            block:addr["Address Generation"]:4
                ag["Address Generator"]
                mab["Memory Address Bus\n(MAB)\n20-bit"]
                am["Address Modes"]
            end
            
            block:data["Data Interface"]:4
                mdb["Memory Data Bus\n(MDB)\n16-bit"]
                dma["Data Alignment"]
                bw["Byte/Word Control"]
            end
            
            block:ctrl["Memory Control"]:4
                mc["Memory Controller"]
                r["Read Control"]
                w["Write Control"]
            end
        end
        
        space:12
        
        block:buses["External Interface"]:12
            block:ext_addr["External Addressing"]:6
                ea["External Address Bus\n20-bit (A0-A19)"]
                eas["Address Strobe"]
            end
            
            block:ext_data["External Data"]:6
                ed["External Data Bus\n16-bit (D0-D15)"]
                rw["Read/Write Control"]
            end
        end
    end
    
    %% Data Flow Connections
    pc --> if
    if --> id
    id --> alu
    alu --> rf
    rf --> ag
    ag --> mab
    mdb --> rf
    
    %% Control Flow
    cu --> alu
    cu --> ag
    cu --> mc
    seq --> cu
    
    %% Memory Interface
    mab --> ea
    mdb --> ed
    mc --> rw
    
    %% Status and Flags
    alu --> sf
    sf --> sr
    sr --> cu
    
    %% Constant Generator
    cg --> alu
    
    style fetch fill:#e3f2fd
    style decode fill:#fff3e0  
    style exec fill:#e8f5e8
    style regs fill:#fce4ec
    style memory fill:#f3e5f5
    style buses fill:#e1f5fe
    
    style pc fill:#bbdefb
    style sp fill:#ffccbc
    style sr fill:#ffcccb
    style alu fill:#c8e6c9
    style rf fill:#f8bbd9
    style mab fill:#d1c4e9
    style mdb fill:#d1c4e9
```

## Component Descriptions

### Core Processing Units

| Component | Description | Width | Function |
|-----------|-------------|-------|----------|
| **Instruction Fetch Unit** | Fetches instructions from memory using PC | 16-bit | Instruction pipeline management |
| **Instruction Decoder** | Decodes fetched instructions and generates control signals | N/A | Instruction format analysis |
| **Arithmetic Logic Unit (ALU)** | Performs arithmetic and logical operations | 20-bit | Computation and comparison |
| **Register File** | Storage for CPU registers R0-R15 | 16 x 20-bit | Data storage and addressing |

### Special Purpose Registers

| Register | Name | Width | Special Function |
|----------|------|-------|------------------|
| **R0** | Program Counter (PC) | 20-bit | Instruction address pointer |
| **R1** | Stack Pointer (SP) | 20-bit | Stack management |
| **R2** | Status Register (SR) | 16-bit | CPU flags and control bits |
| **R3** | Constant Generator | 20-bit | Hardware constants generation |

### Memory Interface

| Component | Description | Width | Function |
|-----------|-------------|-------|----------|
| **Memory Address Bus (MAB)** | Internal address bus | 20-bit | 1MB address space access |
| **Memory Data Bus (MDB)** | Internal data bus | 16-bit | Data transfer |
| **Address Generator** | Calculates effective addresses | 20-bit | Addressing mode support |
| **Memory Controller** | Controls memory access timing | N/A | Read/write coordination |

### Key Architecture Features

#### 20-bit Extended Addressing

- **1MB Address Space**: Full 20-bit addressing without paging
- **Backward Compatibility**: 16-bit operations preserved
- **Extended Instructions**: Support for 20-bit operands

#### Constant Generator Optimization

- **Hardware Constants**: Most common values (0, 1, 2, 4, 8, -1) in hardware
- **Code Density**: Reduces instruction size and execution time
- **Register Aliasing**: R2/R3 provide constant values based on addressing mode

#### Memory Access Optimization

- **Direct Memory-to-Memory**: Operations without intermediate registers
- **Byte/Word/Address-Word**: Multiple data widths supported
- **Aligned Access**: Automatic word alignment for performance

## Data Flow Architecture

### Instruction Execution Flow

1. **Fetch**: PC provides instruction address via MAB
2. **Decode**: Instruction decoder analyzes instruction format
3. **Address**: Address generator calculates operand addresses
4. **Execute**: ALU performs operation with register file data
5. **Writeback**: Results stored in register file or memory via MDB

### Memory Access Patterns

- **Program Memory**: Instructions fetched via PC and MAB
- **Data Memory**: Variables accessed via computed addresses
- **Stack Memory**: SP manages automatic stack operations
- **Peripheral Memory**: Memory-mapped I/O access

## AI Developer Notes

### Emulation Considerations

1. **20-bit Address Space**: Requires careful handling of extended addressing
2. **Register File**: All 16 registers support 20-bit operations
3. **Memory Alignment**: Word operations require even addresses
4. **Constant Generation**: Hardware optimization affects instruction timing
5. **Pipeline Behavior**: Instruction fetch overlaps with execution

### Implementation Requirements

- **Address Bus Emulation**: Full 20-bit MAB simulation
- **Register File**: 16 x 20-bit register storage
- **Instruction Decoder**: Support for MSP430 and MSP430X instruction sets
- **Memory Controller**: Proper timing and access control
- **Status Register**: Complete flag management

## References

This block diagram is based on the official Texas Instruments documentation:

**Primary References:**

- **MSP430x5xx and MSP430x6xx Family User's Guide** (SLAU455) - Section 4.1: "MSP430X CPU (CPUX) Introduction"
  - Figure 4-1: "MSP430X CPU Block Diagram" - CPU architecture overview
  - Section 4.3: "CPU Registers" - Register file organization
  - Section 4.5: "MSP430 and MSP430X Instructions" - Instruction set architecture

**Supporting References:**

- **MSP430FR2xx FR4xx Family User's Guide** (SLAU445I) - October 2014–Revised March 2019
  - Section 1.3: "CPU" - Core CPU functionality
  - Section 4: "CPU" - Detailed CPU operation
