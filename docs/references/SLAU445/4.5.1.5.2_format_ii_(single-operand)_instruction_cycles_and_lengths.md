# 4.5.1.5.2 Format II (Single-Operand) Instruction Cycles and Lengths

Table 4-9 lists the length and the CPU cycles for all addressing modes of the MSP430 single-operand instructions.

<a id="table-4-9"></a>

| Addressing Mode | No. of Cycles<br>RRA, RRC<br>SWPB, SXT | No. of Cycles<br>PUSH | No. of Cycles<br>CALL | Length of<br>Instruction | Example     |
| --------------- | -------------------------------------- | --------------------- | --------------------- | ------------------------ | ----------- |
| Rn              | 1                                      | 3                     | 4                     | 1                        | SWPB R5     |
| @Rn             | 3                                      | 3                     | 4                     | 1                        | RRC @R9     |
| @Rn+            | 3                                      | 3                     | 4                     | 1                        | SWPB @R10+  |
| #N              | N/A                                    | 3                     | 4                     | 2                        | CALL #LABEL |
| X(Rn)           | 4                                      | 4                     | 5                     | 2                        | CALL 2(R7)  |
| EDE             | 4                                      | 4                     | 5                     | 2                        | PUSH EDE    |
| &EDE            | 4                                      | 4                     | 6                     | 2                        | SXT &EDE    |

**Table 4-9. MSP430 Format II Instruction Cycles and Length**
