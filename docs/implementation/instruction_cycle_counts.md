# Instruction Cycle Counts Implementation

## Overview

This document describes the implementation of instruction cycle counts based on SLAU445I Table 4-10 specifications.

## Implementation

The cycle count implementation uses a lookup table approach in `InstructionCycleLookup.cs` that directly maps
addressing mode combinations to their correct cycle counts as specified in SLAU445I Section 4.5.1.5.4 Table 4-10.

### Key Features

- **SLAU445 Table 4-10 Compliance**: Direct implementation of the specification table
- **MOV/BIT/CMP Reduction**: Implements footnote [1] cycle reduction for specific destination modes
- **Constant Generator Support**: Proper handling of R2/R3 constant generators
- **Validation**: Rejects invalid destination addressing modes (Indirect, IndirectAutoIncrement)

### Cycle Count Examples

Based on SLAU445I Table 4-10:

| Source → Destination | Base Cycles | MOV/BIT/CMP Cycles | Notes |
|---------------------|-------------|-------------------|-------|
| Rn → Rm | 1 | 1 | No reduction for single-cycle operations |
| Rn → PC | 3 | 3 | Special case for PC destination |
| @Rn → Rm | 2 | 2 | No reduction for register destinations |
| @Rn+ → Rm | 2 | 2 | No reduction for register destinations |
| #N → Rm | 2 | 2 | No reduction for register destinations |
| Rn → x(Rm) | 4 | 3 | MOV/BIT/CMP reduction applies |
| &EDE → &TONI | 6 | 5 | MOV/BIT/CMP reduction applies |

### MOV/BIT/CMP Cycle Reduction

Per SLAU445I Table 4-10 footnote [1], MOV, BIT, and CMP instructions execute in one fewer cycle for certain addressing modes:

- **Applies to**: Indexed, Symbolic, and Absolute destination modes
- **Does NOT apply to**: Register destinations or PC destinations

### Constant Generators

Constant generators (R2/R3 combinations) are mapped to register addressing mode for cycle counting purposes
since they provide immediate constants without additional memory access cycles.

### Validation

The implementation validates addressing mode combinations:

- **Valid destinations**: Register, Indexed, Symbolic, Absolute
- **Invalid destinations**: Indirect, IndirectAutoIncrement (throws InvalidOperationException)

This matches SLAU445 Table 4-10 which only defines cycles for valid Format I instruction addressing mode combinations.

## References

- SLAU445I Section 4.5.1.5.4 Table 4-10 - Format I (Double-Operand) Instruction Cycles and Lengths
- MSP430FR2xx FR4xx Family User's Guide (SLAU445I) - October 2014–Revised March 2019
