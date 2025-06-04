# Linting and Formatting Tools

This document provides a comprehensive list of all linters and formatters used in the MSP430 Emulator project to help AI
developers make informed decisions about code quality tools.

## Tools Overview

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

## AI Developer Guidelines

When making code changes:

1. **Run linting early**: Use `./script/lint` before committing to catch issues early
2. **Auto-format when possible**: Use `./script/format` or `./script/format-all` to automatically fix formatting
3. **Check comprehensive formatting**: Use `./script/check-format-all` for complete validation
4. **Focus on relevant tools**: Only run linters for file types you're modifying
5. **Understand tool purposes**:
   - Formatters (dotnet format, prettier) fix style issues automatically
   - Linters (eslint, markdownlint-cli2, yamllint) catch syntax and style issues
   - Static analyzers (dotnet build) catch potential code issues

## Dependencies

- **.NET SDK**: Required for `dotnet format` and `dotnet build`
- **Node.js/npm**: Required for JavaScript tooling and markdownlint-cli2
- **Package dependencies**: Installed via `npm install` in root and `.github/scripts/`
