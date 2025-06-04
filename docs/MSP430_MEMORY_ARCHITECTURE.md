# MSP430 Memory Architecture Documentation

## MSP430FR2355 Reference Implementation

This emulator's memory system is specifically designed around the **MSP430FR2355** mixed-signal microcontroller. This choice was made to represent modern MSP430 FRAM devices and their unified memory architecture.

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

### Memory Layout (MSP430FR2355)

| Region | Address Range | Size | Permissions | Description |
|--------|---------------|------|-------------|-------------|
| Special Function Registers | 0x0000-0x00FF | 256B | Read/Write | System control registers |
| 8-bit Peripherals | 0x0100-0x01FF | 256B | Read/Write | Memory-mapped 8-bit peripherals |
| 16-bit Peripherals | 0x0200-0x027F | 128B | Read/Write | Memory-mapped 16-bit peripherals |
| Bootstrap Loader FRAM | 0x1000-0x17FF | 2KB | Read/Execute | Bootstrap loader in FRAM |
| Information Memory FRAM | 0x1800-0x19FF | 512B | Read/Write | Device calibration data |
| SRAM | 0x2000-0x2FFF | 4KB | Read/Write/Execute | High-speed volatile memory |
| FRAM Memory | 0x4000-0xBFFF | 32KB | Read/Write/Execute | Non-volatile unified code/data |
| Interrupt Vector Table | 0xFFE0-0xFFFF | 32B | Read/Execute | Interrupt service routine addresses |

### Supporting Other MSP430 Variants

While this implementation defaults to MSP430FR2355, other MSP430 family members can be supported using the custom memory map constructor:

```csharp
// Create custom memory layout for other MSP430 variants
var customRegions = new List<MemoryRegionInfo>
{
    // Define custom memory regions here...
};
var customMemoryMap = new MemoryMap(customRegions);
```

### Implementation Notes for Future Developers

1. **FRAM vs Flash**: This implementation treats the main memory as FRAM (Read/Write/Execute) rather than traditional Flash (Read/Execute only)
2. **Address Mapping**: Memory regions are pre-computed for O(1) lookup performance
3. **Validation**: All memory access is validated against region permissions and boundaries
4. **Extensibility**: The interface-based design allows for other MSP430 variants through custom memory maps

For detailed technical specifications, refer to the official Texas Instruments documentation:

**Primary References:**
- **MSP430FR2355 Mixed-Signal Microcontroller Datasheet** (SLAS847G, October 2016 - Revised December 2019)
- **MSP430FR2xx and FR4xx Family User's Guide** (SLAU445I, June 2015 - Revised October 2019)

These documents provide comprehensive technical specifications, memory maps, and implementation details used in this emulator.