# Code Formatting with Prettier

This project now uses Prettier alongside existing tools to ensure consistent code formatting across the codebase.

## Tools Used

### C# Files

- **dotnet format**: Formats C# code (.cs, .csproj, .sln files)
- **EditorConfig**: Provides formatting rules for C# and other files

### JavaScript/JSON/YAML/Markdown Files

- **Prettier**: Formats JavaScript (.js), JSON (.json), YAML (.yml/.yaml), and Markdown (.md) files
- **ESLint**: Provides JavaScript linting for code quality (formatting rules removed to avoid conflicts)

## Quick Commands

### Format All Code

```bash
# Format everything (C# + Prettier files)
./script/format-all

# Format only C# code
./script/format

# Format only Prettier-supported files
cd .github/scripts && npm run format
```

### Check Formatting

```bash
# Check all formatting
./script/check-format-all

# Check only C# formatting
./script/lint

# Check only Prettier formatting
cd .github/scripts && npm run format:check
```

### JavaScript Linting

```bash
cd .github/scripts
npm run lint              # Check for issues
npm run lint:fix          # Auto-fix issues
```

## Integration Points

### Pre-commit Hook

The `.githooks/pre-commit` hook automatically checks formatting before commits:

- Detects ANSI escape sequences in all files
- Validates C# formatting with `dotnet format --verify-no-changes`
- Validates JavaScript/JSON/YAML/Markdown with Prettier
- Runs ESLint on JavaScript files

### CI Pipeline

The GitHub Actions CI pipeline includes:

- C# formatting verification
- Prettier formatting verification
- ESLint checks
- YAML validation
- Workflow validation with actionlint

## Configuration Files

### Prettier Configuration

- **Root**: `.prettierrc.json` - Main Prettier config
- **Scripts**: `.github/scripts/.prettierrc.json` - Scripts-specific overrides
- **Ignore**: `.prettierignore` and `.github/scripts/.prettierignore`

### ESLint Configuration

- **JavaScript**: `.github/scripts/.eslintrc.json` - Focused on code quality, not formatting

### EditorConfig

- **All files**: `.editorconfig` - Base formatting rules for all file types

## Development Workflow

1. **Before committing**: Run `./script/check-format-all` to verify formatting
2. **If formatting issues**: Run `./script/format-all` to auto-fix
3. **For JavaScript issues**: Use `cd .github/scripts && npm run lint:fix`
4. **Commit**: Pre-commit hook will verify formatting automatically

## Benefits

- **Consistent formatting** across JavaScript, JSON, YAML, and Markdown files
- **Automated fixing** with `prettier --write`
- **CI integration** prevents unformatted code from being merged
- **Tool separation** - Prettier for formatting, ESLint for code quality
- **GitHub-friendly** - YAML workflows are consistently formatted
- **Documentation** - Markdown files are consistently formatted
