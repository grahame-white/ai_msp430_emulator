# MSP430 Memory Architecture Documentation

## MSP430FR2355 Reference Implementation

This emulator's memory system is specifically designed around the **MSP430FR2355** mixed-signal microcontroller.
This choice was made to represent modern MSP430 FRAM devices and their unified memory architecture.

### Why MSP430FR2355?

The MSP430FR2355 was chosen as the reference implementation for several key reasons:

1. **Modern FRAM Architecture**: Represents the latest MSP430 family with ferroelectric RAM technology
2. **Unified Memory Model**: FRAM provides a unified code/data memory space with byte-level write access
3. **Educational Value**: Demonstrates both traditional and modern MSP430 memory concepts
4. **Developer Clarity**: Provides a specific, well-documented target for implementation

### Key Characteristics of MSP430FR2355

- **32KB FRAM**: Non-volatile ferroelectric memory for unified code/data storage
- **4KB SRAM**: High-speed volatile memory for variables and stack operations
- **FRAM Write Access**: Unlike traditional Flash, FRAM supports byte-level write operations
- **16-bit Address Space**: Complete 0x0000-0xFFFF addressing with optimized layout

For a detailed comparison of FRAM vs traditional Flash memory benefits, see the
[FRAM Technology Benefits section](diagrams/architecture/memory_layout.md#fram-technology-benefits).

### Memory Layout (MSP430FR2355)

The MSP430FR2355 uses a unified 16-bit address space with the following key memory regions:

- **32KB FRAM** (0x4000-0xBFFF): Non-volatile unified code/data storage
- **4KB SRAM** (0x2000-0x2FFF): High-speed volatile memory  
- **System Registers** (0x0000-0x027F): Special function and peripheral registers
- **Information Memory** (0x1800-0x19FF): Device calibration data
- **Interrupt Vectors** (0xFFE0-0xFFFF): ISR addresses

For complete memory region details including unmapped areas and precise address boundaries,
see the [comprehensive memory layout documentation](diagrams/architecture/memory_layout.md).

### Supporting Other MSP430 Variants

While this implementation defaults to MSP430FR2355, other MSP430 family members can be supported using the
custom memory map constructor:

```csharp
// Create custom memory layout for other MSP430 variants
var customRegions = new List<MemoryRegionInfo>
{
    // Define custom memory regions here...
};
var customMemoryMap = new MemoryMap(customRegions);
```

### Implementation Notes for Future Developers

1. **FRAM vs Flash**: This implementation treats the main memory as FRAM (Read/Write/Execute) rather than
   traditional Flash (Read/Execute only)
2. **Address Mapping**: Memory regions are pre-computed for O(1) lookup performance
3. **Validation**: All memory access is validated against region permissions and boundaries
4. **Extensibility**: The interface-based design allows for other MSP430 variants through custom memory maps

## References

This implementation is based on official Texas Instruments documentation:

- **MSP430FR235x, MSP430FR215x Mixed-Signal Microcontrollers** (SLASEC4D, May 2018–Revised December 2019)
- **MSP430FR2xx FR4xx Family User's Guide** (SLAU445I) - October 2014–Revised March 2019

For detailed section and table references used in the implementation, see the
[comprehensive memory layout documentation](diagrams/architecture/memory_layout.md#references).
