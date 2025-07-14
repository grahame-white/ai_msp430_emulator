# ai_msp430_emulator

<p align="center">
  <a href="https://github.com/grahame-white/ai_msp430_emulator/actions/workflows/ci.yml">
    <img src="https://github.com/grahame-white/ai_msp430_emulator/actions/workflows/ci.yml/badge.svg" alt="CI/CD Pipeline">
  </a>
  <a href="https://github.com/grahame-white/ai_msp430_emulator/actions/workflows/security.yml">
    <img src="https://github.com/grahame-white/ai_msp430_emulator/actions/workflows/security.yml/badge.svg"
         alt="Security Scanning">
  </a>
</p>

An exploration of using an AI agent to develop a high accuracy MSP430 emulator

## Quick Start for Contributors

### 🤖 For AI/Copilot Developers

**Start here**: [AI Developer Guidelines](.github/copilot-instructions.md) - Comprehensive guidance designed
specifically for AI agents, including required reading list and development standards.

### 👥 For Human Developers

**Start here**: [Contributing Guidelines](CONTRIBUTING.md) - Complete development setup, coding standards, and
workflow for human contributors.

## Code Coverage

This project enforces both line and branch coverage validation:

- **Line coverage**: Minimum 80% (configurable via `COVERAGE_THRESHOLD`)
- **Branch coverage**: Minimum 70% (configurable via `BRANCH_COVERAGE_THRESHOLD`)

```bash
# Run coverage analysis with default thresholds
./script/coverage

# Custom thresholds
COVERAGE_THRESHOLD=85 BRANCH_COVERAGE_THRESHOLD=75 ./script/coverage
```

## Documentation

- [MSP430 Memory Architecture](docs/MSP430_MEMORY_ARCHITECTURE.md) - MSP430FR2355 memory layout and architecture details
- [Diagnostic Reporting](docs/DiagnosticReporting.md) - Generate comprehensive diagnostic reports for GitHub issues
