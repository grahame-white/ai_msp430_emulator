# MSP430 Emulator AI Developer Guidelines

This is a C# based MSP430 microcontroller emulator project with Node.js automation scripts for GitHub Actions. It aims to provide high accuracy MSP430FR2355 emulation with comprehensive testing and documentation. Please follow these guidelines when contributing:

## Prerequisites
- .NET 8.0 SDK or later
- Node.js 18.0 or later (for automation scripts)
- Git

## Code Standards

### Required Before Each Commit
- Run `./script/lint && ./script/test` before committing any .NET changes to ensure proper code formatting and passing tests
- Run `cd .github/scripts && npm run validate:all && npm test` before committing automation script changes
- This will run static analysis, formatting checks, and comprehensive testing

### Development Flow
- Bootstrap: `./script/bootstrap` (install all required dependencies)
- Build: `./script/build`
- Test: `./script/test` (includes coverage reporting)
- Format: `./script/format` (auto-fix C# formatting)
- Lint: `./script/lint` (check formatting, run static analysis, validate markdown)
- Full validation: `./script/format && ./script/lint && ./script/test`

### Script Options and Modes
Many scripts support helpful options for different scenarios:

```bash
# Test with different modes
./script/test --coverage     # Explicit coverage (default)
./script/test --fast         # Skip coverage for faster testing

# CI build with different modes  
./script/cibuild             # Full CI pipeline
./script/cibuild --fast      # Skip security and coverage
./script/cibuild --skip-security  # Skip only security checks

# Security scanning options
./script/security            # Run all security checks
./script/security --skip-dotnet    # Skip .NET vulnerability scan
./script/security --skip-nodejs    # Skip Node.js vulnerability scan
./script/security --skip-licenses  # Skip license compliance check
```

### Automated Git Hooks Setup
Git hooks are automatically configured during setup to catch issues before commit:

```bash
# Git hooks are configured when you run:
./script/setup

# This automatically:
# - Formats code before each commit
# - Prevents commits with formatting issues
# - Saves time by catching formatting problems early
```

### Auto-fix Common Issues
- C# formatting: `./script/format` or `dotnet format`
- JavaScript linting: `cd .github/scripts && npm run lint:fix`
- Comprehensive formatting: `./script/format-all` (formats C#, JS, JSON, YAML, Markdown)

## Repository Structure
- `src/`: Main emulator library and core components
- `tests/`: Unit tests and integration tests
- `examples/`: Example usage and diagnostic tools
- `docs/`: Technical documentation and specifications
- `script/`: Development workflow scripts following "Scripts to Rule Them All" pattern
- `.github/scripts/`: GitHub Actions automation and validation scripts
- `.github/workflows/`: CI/CD pipeline definitions

## Key Guidelines

### Code Quality and Self-Healing Principles
1. **Run linting early**: Use `./script/lint` before committing to catch issues early and prevent CI failures
2. **Auto-format when possible**: Use `./script/format` or `./script/format-all` to automatically fix style issues
3. **Understand tool purposes**:
   - Formatters (dotnet format, prettier) fix style issues automatically
   - Linters (eslint, markdownlint-cli2, yamllint) catch syntax and style issues
   - Static analyzers (dotnet build) catch potential code issues
4. **Focus on relevant tools**: Only run linters for file types you're modifying
5. **Use comprehensive validation**: Run `./script/check-format-all` for complete formatting validation

### C# Development Standards
- Follow .NET best practices and idiomatic patterns
- Maintain existing code structure and organization
- Use dependency injection patterns where appropriate
- Write unit tests for new functionality using xUnit and table-driven tests when possible
- Each file must contain exactly one type (class, interface, enum, struct, delegate)
- Filename must match the type name exactly
- Interface files must be prefixed with 'I'
- Minimum test coverage: 80%

### File Organization Rules
**Each file must contain exactly one type** (class, interface, enum, struct, delegate, etc.):

- ✅ **Correct**: `RegisterName.cs` contains only the `RegisterName` enum
- ✅ **Correct**: `IEmulatorCore.cs` contains only the `IEmulatorCore` interface  
- ✅ **Correct**: `EmulatorCore.cs` contains only the `EmulatorCore` class
- ❌ **Incorrect**: One file containing both a class and an enum
- ❌ **Incorrect**: One file containing an interface and multiple event argument classes

**Additional file organization rules:**
- Filename must match the type name exactly (e.g., `EmulatorCore.cs` for `EmulatorCore` class)
- Interface files must be prefixed with 'I' (e.g., `IEmulatorCore.cs`)
- Use descriptive folder structure to group related types logically
- Place types in appropriate namespaces that match their folder structure

### Documentation Standards
- Reference official Texas Instruments documentation with specific document versions and section numbers
- Use GitHub-native Mermaid diagrams and markdown formatting validated by markdownlint
- Avoid unnecessary duplication of information across documents
- Include inline comments explaining TI specification compliance
- All MSP430FR2355 implementation details must reference official TI documentation

### JavaScript/Node.js Standards (Automation Scripts)
- Follow ESLint configuration in `.github/scripts/.eslintrc.json`
- Use Prettier for consistent formatting
- Validate YAML files with yamllint
- Validate GitHub Actions workflows with actionlint

### Common JavaScript Linting Rules
- Use single quotes for strings
- Use 4-space indentation
- No trailing spaces
- No space before function parentheses: `function()` not `function ()`
- Always use semicolons
- Use `const` and `let` instead of `var`
- Remove unused variables or prefix with underscore if intentionally unused

### Tooling Integration
- All new development tools must be integrated into the "Scripts to Rule Them All" pattern
- Add tool installation to `script/bootstrap` 
- Add tool execution to appropriate existing scripts (`script/lint`, `script/format`, `script/test`)
- CI workflows should only call existing scripts, never install or run tools directly
- Provide clear error messages when tools are missing

### Error Prevention and Self-Healing
When review comments highlight problems, consider if tools could have caught them earlier:
- **Formatting issues**: Should be caught by `./script/lint` and auto-fixed by `./script/format`
- **Code style violations**: Should be caught by static analysis in `dotnet build`
- **Documentation issues**: Should be caught by markdownlint-cli2
- **YAML syntax errors**: Should be caught by yamllint and actionlint
- **Test failures**: Should be caught by `./script/test` before committing

### Quick Commands Reference

```bash
# Most common workflow for .NET changes:
./script/format && ./script/lint && ./script/test

# Most common workflow for automation script changes:
cd .github/scripts && npm run lint:fix && npm run validate:all && npm test

# Fix specific issues:
./script/format                    # Auto-fix C# formatting
cd .github/scripts && npm run lint:fix  # Auto-fix JavaScript issues
./script/format-all               # Auto-fix all file types

# Check what files are staged for commit:
git diff --cached --name-only

# Complete setup for new environment:
./script/bootstrap && ./script/setup
```

### Common Troubleshooting
- **"Resource not accessible by integration" errors**: Handled gracefully by automation scripts
- **ESLint failures**: Run `npm run lint:fix` in `.github/scripts` to auto-fix most issues
- **dotnet format failures**: Run `dotnet format` to auto-fix formatting issues
- **Test failures**: Check specific test output and ensure changes don't break existing functionality
- **Missing dependencies**: Run `./script/bootstrap` to install all required tools

### Pull Request Workflow
1. Create a feature branch from `main`
2. Make changes following the guidelines above
3. Run all pre-commit checks locally: `./script/format && ./script/lint && ./script/test`
4. For automation script changes: `cd .github/scripts && npm run validate:all && npm test`
5. Submit pull request with clear description
6. Address any feedback from code review
7. Ensure CI passes before requesting final review

### Testing Commands
```bash
# .NET Tests
./script/test                           # Run all tests with coverage
./script/test --fast                    # Skip coverage for faster testing
dotnet test tests/MSP430.Emulator.Tests/            # Run specific test project
dotnet test tests/MSP430.Emulator.IntegrationTests/ # Run integration tests

# Automation Scripts Tests  
cd .github/scripts && npm test         # Run JavaScript tests
```