# ai_msp430_emulator

<p align="center">
  <a href="https://github.com/grahame-white/ai_msp430_emulator/actions/workflows/ci.yml">
    <img src="https://github.com/grahame-white/ai_msp430_emulator/workflows/CI%2FCD%20Pipeline/badge.svg" alt="CI/CD Pipeline">
  </a>
  <a href="https://github.com/grahame-white/ai_msp430_emulator/actions/workflows/security.yml">
    <img src="https://github.com/grahame-white/ai_msp430_emulator/workflows/Security%20Scanning/badge.svg"
         alt="Security Scanning">
  </a>
</p>

An exploration of using an AI agent to develop a high accuracy MSP430 emulator

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

## Contributing

Please see [CONTRIBUTING.md](CONTRIBUTING.md) for development setup, coding standards, and workflow guidelines.

## Documentation

- [MSP430 Memory Architecture](docs/MSP430_MEMORY_ARCHITECTURE.md) - MSP430FR2355 memory layout and architecture details
- [Diagnostic Reporting](docs/DiagnosticReporting.md) - Generate comprehensive diagnostic reports for GitHub issues

## AI Development Guidelines

For AI/Copilot developers, please see [AI Developer Guidelines](.github/copilot-instructions.md) for
comprehensive guidance on code standards, workflow, and best practices.
