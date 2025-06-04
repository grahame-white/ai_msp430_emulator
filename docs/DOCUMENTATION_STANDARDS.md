# Documentation Standards

All technical documentation for the MSP430 emulator must adhere to the following standards to ensure accuracy and traceability.

## Technical Documentation Requirements

### Texas Instruments Documentation Referencing Policy

**Primary Source Requirements:**

- All MSP430FR2355 implementation details **must** reference official Texas Instruments documentation
- Include the **exact document title**, **revision/version number**, and **publication date**
- Provide **specific section numbers, table numbers, and/or figure numbers** for referenced material
- Use the format: `Document Title (Document Number, Date) - Section X.Y: "Title" - Table/Figure reference`

**Example Reference Format:**

```text
MSP430FR2355 Mixed-Signal Microcontroller Datasheet (SLAS847G, October 2016 - Revised December 2019)
- Section 6: "Specifications" - Memory organization (Table 6-4, p. 20)
- Section 7.3: "Memory Map" - Address space layout (Figure 7-1)
```

**Required Primary Documents:**

- **MSP430FR2355 Datasheet** (SLAS847G): Hardware specifications and electrical characteristics
- **MSP430FR2xx/FR4xx Family User's Guide** (SLAU445I): Programming model and architecture details
- **MSP430 Assembly Language Tools User's Guide** (SLAU131): Instruction set and assembly syntax

**Secondary Source Usage:**

- Third-party sources may **only** be used when official TI documentation is insufficient
- Include a **footnote explaining** why the third-party source supersedes or supplements TI documentation
- Still provide the TI reference when available, noting any discrepancies

**Documentation Verification:**

- Contributors **must** verify that referenced sections exist in the specified document versions
- Reviewers **must** validate that implementation matches the referenced TI specifications
- Any conflicts between TI documentation versions should be noted and resolved using the latest revision

## Markdown Standards

**Formatting Requirements:**

- Use consistent markdown formatting validated by markdownlint
- Maximum line length: 120 characters (configurable in `.markdownlint.jsonc`)
- Use proper heading hierarchy (no skipped levels)
- Include blank lines around headings, lists, and code blocks

**Code Examples:**

- Specify language for all code blocks (`csharp`, `text`, etc.)
- Use consistent indentation (4 spaces)
- Include inline comments explaining TI specification compliance

**Visual Documentation:**

- Use GitHub-native Mermaid diagrams instead of external tools when possible
- Include alt text for all images and diagrams
- Ensure diagrams are accessible and maintain readability in both light and dark themes

## Information Duplication Management

**Avoiding Unnecessary Duplication:**

To maintain consistency and reduce maintenance overhead, follow these guidelines:

- **Centralize detailed information** in specialized documentation files
- **Use cross-references** instead of duplicating content across multiple files
- **Maintain a single source of truth** for each type of information
- **Create overview documents** that reference detailed specifications rather than repeating them

**Content Organization Guidelines:**

- **High-level overviews** should summarize and link to detailed documentation
- **Technical specifications** should exist in dedicated technical documents
- **Implementation details** should reference official specifications with specific document citations
- **API documentation** should be generated from code comments when possible

**Examples of Good Organization:**

```text
README.md                     → Brief overview + links to detailed docs
docs/architecture/           → Detailed technical specifications
docs/api/                   → API reference documentation  
CONTRIBUTING.md             → Development guidelines + cross-references
```

**Review Process:**

- Before adding new documentation, check if similar information already exists
- When updating documentation, verify that related documents don't need updates
- Use `grep -r "keyword" docs/` to find potential duplications
- Consolidate overlapping content and add cross-references

## Documentation Validation

Run the following commands to validate documentation changes:

```bash
# Lint all markdown files
npm run lint:docs

# Full validation (includes markdown linting)
./script/lint
```

## Tooling Integration Policy

**All new development tools must be properly integrated into the "Scripts to Rule Them All" pattern.**

### Requirements for New Tooling

When adding new development tools, linters, formatters, or validation utilities:

1. **Installation Integration**
   - Add installation to `script/bootstrap` (for global/system tools)
   - Add installation to `script/setup` if environment-specific setup is needed
   - Never require manual installation steps in CI workflows

2. **Usage Integration**
   - Add tool execution to appropriate existing scripts:
     - `script/lint` - for code analysis, formatting checks, documentation validation
     - `script/format` - for automatic code/document formatting
     - `script/test` - for test-related tooling
     - `script/security` - for security scanning tools

3. **CI Integration**
   - CI workflows should **only** call existing scripts, never install or run tools directly
   - All tool installations should happen via `script/bootstrap`
   - Tools should be available for use by the time `script/lint` or other scripts are called

4. **Error Handling**
   - Provide clear error messages when tools are missing
   - Direct users to run `script/bootstrap` or `script/setup` to resolve dependencies
   - Graceful degradation when optional tools are unavailable

5. **Documentation**
   - Update script descriptions in CONTRIBUTING.md if applicable
   - Update tool requirements in README.md if applicable
   - Include validation commands in relevant documentation sections

### Examples of Proper Integration

**Good ✅ - Tool integrated into scripts:**

```bash
# Tool installed in script/bootstrap
npm install markdownlint-cli2

# Tool used in script/lint  
npx markdownlint-cli2 "docs/**/*.md" "*.md"

# CI calls existing script
- name: Lint code
  run: ./script/lint
```

**Bad ❌ - Tool usage scattered across files:**

```bash
# CI installs tool directly
- name: Install tool
  run: npm install -g some-tool

# CI runs tool directly
- name: Run tool
  run: some-tool --check
```
