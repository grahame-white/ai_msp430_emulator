# Preface

## About this manual

This manual describes the modules and peripherals of the MSP430FR4xx and MSP430FR2xx family of devices. Each description presents the module or peripheral in a general sense. Not all features and functions of all modules or peripherals may be present on all devices. In addition, modules or peripherals may differ in their exact implementation between device families, or may not be fully implemented on an individual device or device family.

Pin functions, internal signal connections, and operational parameters differ from device to device. Consult the device-specific data sheet for these details.

## Related documentation from Texas Instruments

For related documentation, see the [MSP430 web site](http://www.ti.com/msp430)

## Notational conventions

Program examples are shown in a special typeface; for example:

```asm
MOV  #255,  R10
XOR  @R5,   R6
```

## Glossary

| Term     | Description |
| -------- | ----------- |
| ACLK     | Auxiliary clock |
| ADC      | Analog-to-digital converter |
| BOR      | Brownout reset |
| BSL      | Bootloader |
| CPU      | Central processing unit |
| DAC      | Digital-to-analog converter |
| DCO      | Digitally controlled oscillator |
| dst      | Destination |
| FLL      | Frequency locked loop |
| GIE      | General interrupt enable |
| INT(N/2) | Integer portion of N/2 |
| I/O      | Input/output |
| ISR      | Interrupt service routine |
| LSB      | Least-significant bit |
| LSD      | Least-significant digit |
| LPM      | Low-power mode; also named PM for power mode |
| MAB      | Memory address bus |
| MCLK     | Master clock |
| MDB      | Memory data bus |
| MSB      | Most-significant bit |
| MSD      | Most-significant digit |
| NMI      | (Non)-Maskable interrupt; also split to UNMI (user NMI) and SNMI (system NMI) |
| PC       | Program counter |
| vPM      | Power mode |
| POR      | Power-on reset |
| PUC      | Power-up clear |
| RAM      | Random access memory |
| SCG      | System clock generator |
| SFR      | Special function register |
| SMCLK    | Subsystem master clock |
| SNMI     | System NMI |
| SP       | Stack pointer |
| SR       | Status register |
| src      | Source |
| TOS      | Top of stack |
| UNMI     | User NMI |
| WDT      | Watchdog timer |
| z16      | 16-bit address space |

## Register bit conventions

Each register is shown with a key indicating the accessibility of the each individual bit and the initial
condition

<a id="table-0-1" />

| Key | Bit Accessibility |
| --- | ----------------- |
| rw | Read/write |
| r | Read only |
| r0 | Read as 0 |
| r1 | Read as 1 |
| w | Write only |
| w0 | Write as 0 |
| w1 | Write as 1 |
| (w) | No register bit implemented; writing a 1 results in a pulse. The register bit always reads as 0. |
| h0 | Cleared by hardware |
| h1 | Set by hardware |
| -0,-1 | Condition after PUC |
| -(0),-(1) | Condition after POR |
| -[0],-[1] | Condition after BOR |
| -{0},-{1} | Condition after brownout |

**Table 0-1. Register Bit Accessibility and Initial Condition**
