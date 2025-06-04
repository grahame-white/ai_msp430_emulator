# GitHub Actions Workflow Validation

This project uses [actionlint](https://github.com/rhymond/actionlint) for comprehensive GitHub Actions workflow validation.

## What actionlint provides

actionlint is a static checker for GitHub Actions workflow files that catches:

- **Syntax errors**: Invalid YAML, missing required fields
- **Type errors**: Wrong types in expressions, invalid contexts
- **Logic errors**: Unreachable jobs, invalid dependencies
- **Action validation**: Invalid action references, wrong input/output usage
- **Shell script validation**: Integration with shellcheck for run steps
- **Security issues**: Potentially dangerous patterns

## Usage

```bash
# Validate all workflows
npm run lint:workflows

# Validate YAML syntax
npm run lint:yaml

# Run all validation checks
npm run validate:all
```

## Configuration

actionlint configuration is in `.github/actionlint.yml`. The tool automatically:

- Validates workflow syntax and structure
- Checks action references and versions
- Validates expressions and contexts
- Runs shellcheck on shell scripts
- Checks for common security issues

## Why actionlint vs custom validation?

- **Comprehensive**: Covers all GitHub Actions features and edge cases
- **Maintained**: Actively developed and updated with new GitHub Actions features
- **Industry standard**: Used by many open source projects
- **Performance**: Fast native binary, no runtime dependencies
- **Integration**: Works well with CI/CD pipelines

The previous custom validator has been replaced with actionlint for better coverage and maintainability.
