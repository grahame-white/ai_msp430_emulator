# MSP430 Instruction Format Layouts

This document provides detailed bit-level layouts for all three MSP430 instruction formats, showing how fields
are encoded within the 16-bit instruction word.

## Format I - Two-Operand Instructions

Format I instructions operate on two operands (source and destination) and include arithmetic, logical, and
data movement operations.

### Bit Layout

| Bits | 15 | 14 | 13 | 12 | 11 | 10 | 9 | 8 | 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
|------|----|----|----|----|----|----|---|---|---|---|---|---|---|---|---|---|
| **Field** | **Opcode[3:0]** | | | | **Source Reg[3:0]** | | | | **Ad** | **B/W** | **As[1:0]** | | **Dest Reg[3:0]** | | | |

### Field Descriptions

| Field | Bits | Description | Values |
|-------|------|-------------|---------|
| **Opcode** | 15:12 | Instruction opcode | 4-15 (0x4-0xF) |
| **Source Reg** | 11:8 | Source register number | 0-15 (R0-R15) |
| **Ad** | 7 | Destination addressing mode | 0=Register, 1=Indexed/Symbolic/Absolute |
| **B/W** | 6 | Byte/Word operation | 0=Word, 1=Byte |
| **As** | 5:4 | Source addressing mode | 00=Register, 01=Indexed, 10=Indirect, 11=Immediate |
| **Dest Reg** | 3:0 | Destination register number | 0-15 (R0-R15) |

### Opcode Table

| Opcode | Hex | Mnemonic | Operation |
|--------|-----|----------|-----------|
| 4 | 0x4 | MOV | Move source to destination |
| 5 | 0x5 | ADD | Add source to destination |
| 6 | 0x6 | ADDC | Add source + carry to destination |
| 7 | 0x7 | SUBC | Subtract source + carry from destination |
| 8 | 0x8 | SUB | Subtract source from destination |
| 9 | 0x9 | CMP | Compare source with destination |
| 10 | 0xA | DADD | Decimal add source to destination |
| 11 | 0xB | BIT | Test bits in destination |
| 12 | 0xC | BIC | Clear bits in destination |
| 13 | 0xD | BIS | Set bits in destination |
| 14 | 0xE | XOR | Exclusive OR source with destination |
| 15 | 0xF | AND | AND source with destination |

### Examples

```text
MOV R1, R2        : 0100 0001 0000 0010 = 0x4102
ADD #5, R3        : 0101 0000 1101 0011 = 0x50D3 (with extension word 0x0005)
MOV.B @R4+, 2(R5) : 0100 0100 1101 0101 = 0x44D5 (with extension word 0x0002)
```

## Format II - Single-Operand Instructions

Format II instructions operate on a single operand and include shifts, rotates, stack operations, and control flow instructions.

### Bit Layout

| Bits | 15 | 14 | 13 | 12 | 11 | 10 | 9 | 8 | 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
|------|----|----|----|----|----|----|---|---|---|---|---|---|---|---|---|---|
| **Field** | **Opcode[7:0]** | | | | | | | | **0** | **B/W** | **As[1:0]** | | **Source Reg[3:0]** | | | |

### Field Descriptions

| Field | Bits | Description | Values |
|-------|------|-------------|---------|
| **Opcode** | 15:8 | Instruction opcode | 0x10-0x13 |
| **Reserved** | 7 | Always 0 | 0 |
| **B/W** | 6 | Byte/Word operation | 0=Word, 1=Byte |
| **As** | 5:4 | Source addressing mode | 00=Register, 01=Indexed, 10=Indirect, 11=Immediate |
| **Source Reg** | 3:0 | Source register number | 0-15 (R0-R15) |

### Opcode Table

| Opcode | Hex | Instruction Type | Examples |
|--------|-----|------------------|----------|
| 0x10 | 0x10XX | RRC, SWPB, RRA | Rotate/Shift operations |
| 0x11 | 0x11XX | SXT | Sign extend |
| 0x12 | 0x12XX | PUSH, CALL | Stack/Call operations |
| 0x13 | 0x13XX | RETI | Return from interrupt |

### Sub-Instruction Decoding

For opcode 0x10, the specific instruction is determined by bits 7:6:

- 0x1000-0x103F: RRC (Rotate right through carry)
- 0x1040-0x107F: SWPB (Swap bytes)
- 0x1080-0x10BF: RRA (Rotate right arithmetic)
- 0x10C0-0x10FF: SXT (Sign extend byte to word)

### Examples

```text
RRC R5         : 0001 0000 0000 0101 = 0x1005
PUSH #0x1234   : 0001 0010 0011 0000 = 0x1230 (with extension word 0x1234)
CALL @R12      : 0001 0010 1000 1100 = 0x128C
```

## Format III - Jump Instructions

Format III instructions provide conditional and unconditional jumps with PC-relative addressing.

### Bit Layout

| Bits | 15 | 14 | 13 | 12 | 11 | 10 | 9 | 8 | 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
|------|----|----|----|----|----|----|---|---|---|---|---|---|---|---|---|---|
| **Field** | **001** | | | **Condition[2:0]** | | | **10-bit Signed Offset** | | | | | | | | | |

### Field Descriptions

| Field | Bits | Description | Values |
|-------|------|-------------|---------|
| **Prefix** | 15:13 | Always 001 | 001 (binary) |
| **Condition** | 12:10 | Jump condition code | 0-7 |
| **Offset** | 9:0 | Signed offset (words) | -511 to +512 words |

### Condition Codes

| Code | Condition | Mnemonic | Test |
|------|-----------|----------|------|
| 0 | Jump if equal/zero | JEQ/JZ | Z = 1 |
| 1 | Jump if not equal/not zero | JNE/JNZ | Z = 0 |
| 2 | Jump if carry set | JC | C = 1 |
| 3 | Jump if carry clear | JNC | C = 0 |
| 4 | Jump if negative | JN | N = 1 |
| 5 | Jump if greater or equal | JGE | N ⊕ V = 0 |
| 6 | Jump if less than | JL | N ⊕ V = 1 |
| 7 | Jump unconditional | JMP | Always |

### Offset Calculation

The 10-bit offset is sign-extended to 16 bits and represents the number of **words** (not bytes) to jump:

- Effective address = PC + 2 + (offset × 2)
- Range: -1022 to +1024 bytes from current instruction

### Examples

```text
JEQ label      : 001 000 xxxxxxxxxx (offset depends on label distance)
JMP $+10       : 001 111 0000000101 = 0x3C05 (jump forward 5 words = 10 bytes)
JL $-4         : 001 110 1111111110 = 0x3BFE (jump back 2 words = 4 bytes)
```

## Addressing Mode Encoding

### Source Addressing Modes (As bits)

| As | Register | Mode | Description | Extension Word |
|----|----------|------|-------------|----------------|
| 00 | Any | Register | Rn | No |
| 01 | Any | Indexed | X(Rn) | Yes (X) |
| 01 | R0 | Symbolic | ADDR | Yes (ADDR) |
| 01 | R2 | Absolute | &ADDR | Yes (ADDR) |
| 10 | Any | Indirect | @Rn | No |
| 11 | Any | Indirect++ | @Rn+ | No |
| 11 | R0 | Immediate | #N | Yes (N) |
| 11 | R2 | Immediate | #4 | No |
| 00 | R3 | Immediate | #0 | No |
| 01 | R3 | Immediate | #1 | No |
| 10 | R3 | Immediate | #2 | No |
| 11 | R3 | Immediate | #-1 | No |

### Destination Addressing Modes (Ad bit)

| Ad | Register | Mode | Description | Extension Word |
|----|----------|------|-------------|----------------|
| 0 | Any | Register | Rn | No |
| 1 | Any | Indexed | X(Rn) | Yes (X) |
| 1 | R0 | Symbolic | ADDR | Yes (ADDR) |
| 1 | R2 | Absolute | &ADDR | Yes (ADDR) |

## Extension Words

Instructions may require additional 16-bit words following the instruction word:

1. **Indexed addressing**: Extension word contains the offset
2. **Immediate operands**: Extension word contains the immediate value
3. **Absolute addressing**: Extension word contains the absolute address
4. **Symbolic addressing**: Extension word contains the PC-relative offset

Multiple extension words are read in source-first, destination-second order for Format I instructions.
