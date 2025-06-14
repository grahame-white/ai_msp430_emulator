# 1 - System Resets, Interrupts, and Operating Modes, System Control Module (SYS)

The system control module (SYS) is available on all devices. The basic features of SYS are:

- Brownout reset (BOR) and power on reset (POR) handling
- Power-up clear (PUC) handling
- (Non)maskable interrupt (SNMI and UNMI) event source selection and management
- User data-exchange mechanism through the JTAG mailbox (JMB)
- Bootloader (BSL) entry mechanism
- Configuration management (device descriptors)
- Providing interrupt vector generators for reset and NMIs
- FRAM write protection
- On-chip module-to-module signaling control

