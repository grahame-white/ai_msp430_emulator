# Linting and Formatting Tools

This document provides a comprehensive list of all linters and formatters used in the MSP430 Emulator project to help AI
developers make informed decisions about code quality tools.

## Tools Overview

The linting and formatting tools support GitHub Actions annotations to display inline error messages in pull requests and
workflow runs.

### Annotation Support

| Tool | Annotation Support | Notes |
|------|-------------------|-------|
| **dotnet format** | ✅ Yes | Via `script/github-annotations` helper |
| **markdownlint-cli2** | ✅ Yes | Parsed output format: `file:line:col rule message` |
| **eslint** | ✅ Yes | JSON output format parsed for annotations |
| **prettier** | ✅ Yes | File listing converted to annotations |
| **actionlint** | ✅ Yes | Native support via reviewdog action |
| **yamllint** | ⚠️ Limited | Basic error display (no structured output) |

### Enabling Annotations

Annotations are automatically enabled when running in GitHub Actions. You can also manually enable them:

```bash
# Enable annotations for main linting script
./script/lint --annotations

# Enable annotations for comprehensive formatting check
./script/check-format-all --annotations
```

#### Automation Scripts with Annotations

For JavaScript automation scripts in `.github/scripts/`, use the dedicated annotation-enabled linting script:

```bash
# Run automation script linting with annotations
cd .github/scripts && ./lint-with-annotations.sh

# This script automatically:
# - Detects GitHub Actions environment
# - Runs ESLint with JSON output for structured annotations  
# - Runs Prettier check with annotation support
# - Runs YAML linting with basic error reporting
# - Sources shared annotation functions from script/github-annotations
```

The automation script linting includes:

- **ESLint**: JavaScript code quality and style checks
- **Prettier**: Code formatting verification for JS, JSON, MD, YAML files
- **yamllint**: YAML syntax and style validation

### Annotation Format

The tools output GitHub Actions annotations in the standard format:

```text
::error file={file},line={line},col={col}::{message}
::warning file={file},line={line},col={col}::{message}
::notice file={file},line={line},col={col}::{message}
```

These annotations appear as:

- Inline comments in the Files Changed view of pull requests
- Grouped error/warning messages in the Actions summary
- Check annotations in the Checks tab

## Error Prevention and CI Reliability

To prevent automation script CI failures, several preventative measures are in place:

### Local Validation Script

Use `./script/check-automation-scripts` before committing changes that might affect the automation scripts CI workflow:

```bash
# Run comprehensive automation scripts validation
./script/check-automation-scripts
```

This script validates:

- package-lock.json synchronization with package.json
- Proper error handling in scripts
- Required Node.js dependencies availability
- Automation script test execution
- lint-with-annotations.sh functionality
- Annotation test execution

### Enhanced Error Handling

The automation scripts CI workflow includes:

- **Strict error handling**: Uses `set -euo pipefail` for robust error detection
- **Dependency validation**: Checks package-lock.json synchronization before running
- **Enhanced logging**: Debug mode available with `DEBUG=1` environment variable
- **Improved JSON processing**: More robust ESLint output parsing with validation
- **Automatic cleanup**: Temporary files are cleaned up on script exit or interruption
- **Comprehensive testing**: Extended timeout periods and better error reporting

### CI Workflow Improvements

The GitHub Actions workflow has been enhanced with:

- `npm ci --no-audit --no-fund` for faster, more reliable dependency installation
- Enhanced security audit checks with appropriate failure levels
- Better error reporting with detailed logs
- Artifact collection for debugging failures
- Comprehensive validation of package.json structure

### C# Code Quality

| Tool | Purpose | Script Usage | Configuration |
|------|---------|-------------|---------------|
| **dotnet format** | C# code formatting and style enforcement | `./script/format`, `./script/lint`, `./script/check-format-all` | `.editorconfig`, `Directory.Build.props` |
| **dotnet build** | Static analysis via compiler warnings/errors | `./script/lint` | `CodeAnalysis.ruleset`, `Directory.Build.props` |

### Documentation Quality

| Tool | Purpose | Script Usage | Configuration |
|------|---------|-------------|---------------|
| **markdownlint-cli2** | Markdown linting for documentation | `./script/lint` | `.markdownlint.jsonc` |
| **prettier** | Markdown, JSON, YAML formatting | `./script/format-all`, `./script/check-format-all` | `.prettierrc.json`, `.prettierignore` |

### JavaScript/Node.js Quality

| Tool | Purpose | Script Usage | Configuration |
|------|---------|-------------|---------------|
| **eslint** | JavaScript code linting and formatting | `./script/check-format`, `./script/check-format-all` | `.github/scripts/.eslintrc.json` |
| **prettier** | JavaScript, JSON formatting | `./script/format-all`, `./script/check-format-all` | `.github/scripts/.prettierrc.json` |

### YAML and Workflows

| Tool | Purpose | Script Usage | Configuration |
|------|---------|-------------|---------------|
| **yamllint** | YAML file linting | `./script/check-format-all` (via npm) | `.github/scripts/.yamllint.yml` |
| **actionlint** | GitHub Actions workflow linting | `./script/check-format-all` (via npm) | Built-in rules |

## Script Commands

### Primary Commands

- `./script/lint` - Run all linting checks (C#, Markdown)
- `./script/format` - Auto-format C# code only
- `./script/format-all` - Auto-format all file types (C#, JS, JSON, YAML, Markdown)
- `./script/check-format` - Check formatting without modifications
- `./script/check-format-all` - Comprehensive formatting check for all file types

### Individual Tool Commands

#### C# (.NET)

```bash
# Format C# code
dotnet format --verbosity minimal

# Check C# formatting without changes
dotnet format --verify-no-changes --verbosity diagnostic

# Static analysis
dotnet build --configuration Release --verbosity quiet
```

#### Markdown

```bash
# Lint Markdown files
npx markdownlint-cli2 "docs/**/*.md" "*.md"
```

#### JavaScript/Node.js (from .github/scripts/)

```bash
# Lint JavaScript
npm run lint          # eslint *.js
npm run lint:fix      # eslint *.js --fix

# Format with Prettier
npm run format        # prettier --write *.js *.json *.md ../**/*.yml ../**/*.yaml
npm run format:check  # prettier --check *.js *.json *.md ../**/*.yml ../**/*.yaml
```

#### YAML and Workflows (from .github/scripts/)

```bash
# Lint YAML files
npm run lint:yaml     # yamllint -c .yamllint.yml ../**/*.yml ../**/*.yaml

# Lint GitHub Actions workflows
npm run lint:workflows # actionlint on workflow files
```

## Configuration Files

| File | Purpose | Affects |
|------|---------|---------|
| `.editorconfig` | Editor formatting settings | All file types |
| `Directory.Build.props` | MSBuild properties and analyzers | C# projects |
| `CodeAnalysis.ruleset` | C# static analysis rules | C# projects |
| `.markdownlint.jsonc` | Markdown linting rules | Markdown files |
| `.prettierrc.json` | Prettier formatting configuration | JS, JSON, YAML, Markdown |
| `.prettierignore` | Files to exclude from Prettier | All Prettier-handled files |
| `.github/scripts/.eslintrc.json` | ESLint configuration | JavaScript files |
| `.github/scripts/.yamllint.yml` | YAML linting configuration | YAML files |

## Dependencies

- **.NET SDK**: Required for `dotnet format` and `dotnet build`
- **Node.js/npm**: Required for JavaScript tooling and markdownlint-cli2
- **Package dependencies**: Installed via `npm install` in root and `.github/scripts/`
