---
name: Bug Report
about: Create a report to help us improve the MSP430 Emulator
title: '[BUG] '
labels: ['bug', 'triage']
assignees: ''
---

## Bug Description
**Describe the bug**
A clear and concise description of what the bug is.

## Severity Level
**Please select the severity level:**
- [ ] Critical - System crashes, data loss, or core functionality completely broken
- [ ] High - Major functionality affected, significant impact on usability
- [ ] Medium - Minor functionality affected, has workaround available
- [ ] Low - Cosmetic issues, documentation errors, or very minor functionality

## Reproduction Steps
**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '...'
3. Scroll down to '...'
4. See error

## Expected Behavior
**Expected behavior**
A clear and concise description of what you expected to happen.

## Actual Behavior
**Actual behavior**
A clear and concise description of what actually happened.

## Environment Details
**Environment (please complete the following information):**
- OS: [e.g. Windows 10, Ubuntu 20.04, macOS 12.0]
- .NET Version: [e.g. .NET 8.0]
- Emulator Version: [e.g. 1.0.0]
- Hardware: [e.g. x64, ARM64]

**MSP430 Configuration:**
- Target MSP430 Model: [e.g. MSP430G2553]
- Memory Configuration: [e.g. 16KB Flash, 512B RAM]
- Clock Frequency: [e.g. 1MHz]

## Code Sample
**Code that reproduces the issue**
```csharp
// Provide minimal code that reproduces the issue
var emulator = new MSP430Emulator();
// ... rest of the code
```

## Additional Context
**Screenshots**
If applicable, add screenshots to help explain your problem.

**Log Output**
```
Paste relevant log output here
```

**Additional context**
Add any other context about the problem here.

## Potential Impact
- [ ] Blocks development
- [ ] Causes incorrect emulation results
- [ ] Performance impact
- [ ] Security concern
- [ ] Other: ____________

## Checklist
- [ ] I have searched existing issues to ensure this is not a duplicate
- [ ] I have provided all required environment details
- [ ] I have included steps to reproduce the issue
- [ ] I have tested with the latest version of the emulator