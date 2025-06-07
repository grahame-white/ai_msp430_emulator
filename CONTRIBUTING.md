# Contributing to MSP430 Emulator

Thank you for your interest in contributing to the MSP430 Emulator project!
This guide will help you set up your development environment and understand our development workflow.

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Node.js 18.0 or later (for automation scripts)
- Git

### Initial Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/grahame-white/ai_msp430_emulator.git
   cd ai_msp430_emulator
   ```

2. Run the setup script:

   ```bash
   ./script/setup
   ```

3. Install automation script dependencies:

   ```bash
   cd .github/scripts
   npm install
   cd ../..
   ```

## Development Workflow

### Pre-Commit Checks

**⚠️ IMPORTANT: Always run these checks before committing to prevent CI failures:**

#### For .NET Code Changes

```bash
# Bootstrap dependencies (install all required dependencies)
./script/bootstrap

# Auto-format code (recommended - fixes issues automatically)
./script/format

# Check code formatting and run static analysis (check-only mode)
./script/lint

# Run all tests
./script/test

# Test core scripts (smoke tests)
./script/test-scripts

# Build the project
./script/build
```

#### Automatic Formatting Setup (Included in Setup)

Automatic formatting is set up when you run the main setup script:

```bash
# Git hooks are automatically configured during setup
./script/setup
```

This will:

- Automatically format code before each commit
- Prevent commits with formatting issues
- Save time by catching formatting problems early

#### For Automation Scripts (.github/scripts)

```bash
cd .github/scripts

# Lint JavaScript code
npm run lint

# Auto-fix linting issues (when possible)
npm run lint:fix

# Validate YAML files
npm run lint:yaml

# Validate GitHub Actions workflows
npm run validate:workflows

# Run all validations
npm run validate:all

# Run tests
npm test
```

### Git Hooks (Recommended)

To automatically run checks before each commit, set up a pre-commit hook:

```bash
# Create the pre-commit hook
cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
set -e

echo "Running pre-commit checks..."

# Check for .NET code changes
if git diff --cached --name-only | grep -E '\.(cs|csproj|sln)$' > /dev/null; then
    echo "==> .NET code changes detected, running linter..."
    ./script/lint
    echo "==> Running .NET tests..."
    ./script/test
fi

# Check for automation script changes
if git diff --cached --name-only | grep -E '^\.github/scripts/.*\.js$' > /dev/null; then
    echo "==> Automation script changes detected, running validation..."
    cd .github/scripts
    npm run validate:all
    npm test
    cd ../..
fi

# Check for workflow changes
if git diff --cached --name-only | grep -E '^\.github/workflows/.*\.yml$' > /dev/null; then
    echo "==> Workflow changes detected, running validation..."
    cd .github/scripts
    npm run validate:workflows
    cd ../..
fi

echo "✅ All pre-commit checks passed!"
EOF

# Make the hook executable
chmod +x .git/hooks/pre-commit
```

## Documentation Standards

All technical documentation must follow our established standards to ensure accuracy and consistency.

📖 **See [Documentation Standards](docs/DOCUMENTATION_STANDARDS.md) for complete requirements**

Key requirements:

- Reference official Texas Instruments documentation with specific document versions and section numbers
- Use GitHub-native Mermaid diagrams and markdown formatting validated by markdownlint
- Avoid unnecessary duplication of information across documents
- Integrate new documentation tools following the "Scripts to Rule Them All" pattern

### Quick Validation

```bash
# Lint all markdown files
npm run lint:docs

# Full validation (includes markdown linting)
./script/lint
```

## Code Style Guidelines

📋 **See [Linting and Formatting Tools](docs/LINTING_AND_FORMATTING.md) for comprehensive tool reference**

### File Organization

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

### .NET Code

- Follow the existing code formatting (enforced by `dotnet format`)
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and single-purpose
- Use appropriate error handling

### JavaScript Code (Automation Scripts)

- Follow the ESLint configuration in `.github/scripts/.eslintrc.json`
- Use single quotes for strings
- Use 4-space indentation
- No trailing spaces
- No space before function parentheses: `function()` not `function ()`
- Always use semicolons
- Use `const` and `let` instead of `var`

### Common Linting Issues to Avoid

1. **Trailing spaces**: Remove all trailing whitespace
2. **Function spacing**: No space before parentheses in function declarations
3. **Inconsistent indentation**: Use 4 spaces consistently
4. **Missing semicolons**: Always end statements with semicolons
5. **Unused variables**: Remove or prefix with underscore if intentionally unused

## Testing

### Test Policy

All tests must follow these guidelines to ensure maintainability and clarity:

1. **One Test One Assert**: Each test method should contain exactly one assertion
2. **Parametric Testing**: Use `[Theory]` and `[InlineData]` attributes to test multiple scenarios instead of
   multiple assertions in one test
3. **Descriptive Test Names**: Test method names should clearly describe what is being tested and the expected outcome
4. **Focused Testing**: Each test should validate one specific behavior or outcome
5. **No Reflection in Tests**: Avoid using reflection in tests as it is brittle and can slow test execution. 
   Access properties directly (e.g., `memory.Size`, `memory.BaseAddress`) for clearer intent and better performance

### Example of Compliant Test Structure

```csharp
// ❌ Bad - Multiple assertions in one test
[Fact]
public void Constructor_ValidParameters_CreatesObject()
{
    var obj = new SomeClass(param1, param2);
    Assert.Equal(expectedValue1, obj.Property1);
    Assert.Equal(expectedValue2, obj.Property2);
    Assert.Equal(expectedValue3, obj.Property3);
}

// ✅ Good - Separate tests with one assertion each
[Theory]
[InlineData(param1, expectedValue1)]
[InlineData(param2, expectedValue2)]
public void Constructor_ValidParameters_SetsProperty1(int param, int expected)
{
    var obj = new SomeClass(param);
    Assert.Equal(expected, obj.Property1);
}

[Fact]
public void Constructor_ValidParameters_SetsProperty2()
{
    var obj = new SomeClass(param1, param2);
    Assert.Equal(expectedValue2, obj.Property2);
}
```

### .NET Tests

```bash
# Run all tests
./script/test

# Run specific test project
dotnet test tests/MSP430.Emulator.Tests/
dotnet test tests/MSP430.Emulator.IntegrationTests/
```

### Automation Scripts Tests

```bash
cd .github/scripts
npm test
```

## Continuous Integration

Our CI pipeline runs the following checks:

1. **Code formatting** (dotnet format)
2. **Static analysis** (dotnet build with warnings as errors)
3. **Unit and integration tests** with coverage reporting
4. **JavaScript linting** (ESLint)
5. **YAML validation** (yamllint)
6. **Workflow validation** (actionlint)

**All of these checks must pass for your PR to be merged.**

## Scripts Reference

This project follows the ["Scripts to Rule Them All"](https://github.com/github/scripts-to-rule-them-all)
pattern for consistent development workflows.

### Core Scripts

- **`script/setup`** - Set up the development environment (includes git hooks)
- **`script/bootstrap`** - Install dependencies only
- **`script/update`** - Update dependencies and packages
- **`script/build`** - Build the project
- **`script/test`** - Run all tests (with coverage by default)
- **`script/server`** - Start the MSP430 emulator application
- **`script/console`** - Interactive C# console with emulator libraries

### Quality Assurance Scripts

- **`script/format`** - Auto-format code (fixes formatting issues)
- **`script/lint`** - Check code formatting and style
- **`script/coverage`** - Generate test coverage reports
- **`script/security`** - Run security vulnerability scans

### CI/CD Scripts

- **`script/cibuild`** - Complete CI pipeline (bootstrap → lint → build → test → security)

### Script Options

Most scripts support helpful options:

```bash
# Test with different modes
script/test --coverage     # Explicit coverage (default)
script/test --fast         # Skip coverage for faster testing

# CI build with different modes  
script/cibuild             # Full CI pipeline
script/cibuild --fast      # Skip security and coverage
script/cibuild --skip-security  # Skip only security checks

# Security scanning options
script/security            # Run all security checks
script/security --skip-dotnet    # Skip .NET vulnerability scan
script/security --skip-nodejs    # Skip Node.js vulnerability scan
script/security --skip-licenses  # Skip license compliance check
```

## Scripts Reference

This project follows the ["Scripts to Rule Them All"](https://github.com/github/scripts-to-rule-them-all)
pattern for consistent development workflows.

## Quick Reference

### Most Common Commands

```bash
# Before committing .NET changes:
./script/lint && ./script/test

# Before committing automation script changes:
cd .github/scripts && npm run validate:all && npm test

# Fix JavaScript linting issues automatically:
cd .github/scripts && npm run lint:fix

# Check what files are staged for commit:
git diff --cached --name-only
```

### Troubleshooting

#### "Resource not accessible by integration" errors

These are handled gracefully by the automation scripts and won't cause failures.

#### ESLint failures

Run `npm run lint:fix` in `.github/scripts` to auto-fix most issues.

#### dotnet format failures

Run `dotnet format` to auto-fix formatting issues.

#### Test failures

Check the specific test output and ensure your changes don't break existing functionality.

## Getting Help

- Check existing issues and discussions
- Look at recent commits for examples
- Ensure you're following the coding standards outlined above
- Run the pre-commit checks to catch issues early

## Pull Request Process

1. Create a feature branch from `main`
2. Make your changes following the guidelines above
3. Run all pre-commit checks locally
4. Submit your pull request with a clear description
5. Address any feedback from code review
6. Ensure CI passes before requesting final review

Thank you for contributing to the MSP430 Emulator project!
