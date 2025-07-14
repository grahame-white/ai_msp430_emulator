# MSP430 Emulator AI Developer Guidelines

This is a C# based MSP430 microcontroller emulator project with Node.js automation scripts for GitHub Actions. It aims to provide high accuracy MSP430FR2355 emulation with comprehensive testing and documentation. Please follow these guidelines when contributing:

## ABSOLUTE MANDATORY RULES

- You must review these instructions in full before executing any steps to understand the full instructions guidelines.
- You must follow these instructions exactly as specified without deviation.
- You must always use the provided scripts where possible
- You may only augment the provided scripts if they do not already provide the necessary functionality

## 🤖 AI Developer Tell (REQUIRED)

**To prove you have read these instructions, you MUST include the phrase "🚀 MSP430 EMULATOR AUTOPILOT ENGAGED" in your first response or commit message when working on this repository.** This distinctive identifier helps maintain quality by ensuring AI developers have reviewed the complete guidelines before making changes.

## Required Reading List

**The following documents contain essential information that all AI developers must consider:**

- [Scripts to Rule Them All](/docs/SCRIPTS_TO_RULE_THEM_ALL.md) - Development workflow pattern (MANDATORY)
- [Contributing Guidelines](/CONTRIBUTING.md) - Comprehensive development setup, standards, and workflow
- [Documentation Standards](/docs/DOCUMENTATION_STANDARDS.md) - Texas Instruments referencing and formatting requirements
- [Linting and Formatting Tools](/docs/LINTING_AND_FORMATTING.md) - Complete tooling reference for code quality

## Essential Workflow for AI Developers

### Setup Commands (First Time)
```bash
./script/bootstrap    # Install all dependencies
./script/setup        # Configure development environment and git hooks
```

### 🚨 MANDATORY: Pre-Commit Validation (ALWAYS REQUIRED)

**YOU MUST RUN THESE COMMANDS BEFORE EVERY COMMIT:**

```bash
# For .NET code changes:
./script/format && ./script/lint && ./script/test

# For automation script changes:
cd .github/scripts && npm run validate:all && npm test
```

**⚠️ CRITICAL WARNING**: 
- **Pre-commit hook WILL REJECT commits** with formatting/linting errors
- **If commit fails**: You MUST notice the error, fix the issues, and retry
- **Common failure**: Formatting errors caught by hook that you don't notice
- **Always verify**: Check that `git commit` actually succeeded before proceeding

**🔧 If pre-commit hook fails:**
1. Read the error message carefully
2. Run the suggested fix commands (usually `./script/format`)
3. Stage the formatting changes with `git add .`
4. Retry the commit

## Core Development Standards (Summary)

**📋 See [Contributing Guidelines](/CONTRIBUTING.md) for complete details**

### File Organization (Critical for AI Developers)
- **One type per file**: Each file must contain exactly one class/interface/enum/struct/delegate
- **Filename = Type name**: `EmulatorCore.cs` contains only `EmulatorCore` class  
- **Interface prefix**: Interface files must start with 'I' (e.g., `IEmulatorCore.cs`)

### Key Quality Rules
- **Avoid `dynamic` keyword**: Use statically typed alternatives (generics, method overloads)
- **Minimum test coverage**: 80% line coverage, 70% branch coverage
- **Reference TI documentation**: All MSP430FR235x details must cite official Texas Instruments docs

### Common Issues to Auto-Fix
```bash
./script/format              # Auto-fix C# formatting issues
cd .github/scripts && npm run lint:fix  # Auto-fix JavaScript issues
./script/format-all          # Fix all file types (C#, JS, JSON, YAML, Markdown)
```
