# PlantUML Diagrams

This directory contains PlantUML diagram source files and their rendered outputs that document the MSP430 emulator's memory architecture.

## Directory Structure

```
docs/diagrams/
├── architecture/          # PlantUML source files (.puml)
│   ├── memory_layout.puml
│   └── memory_access_flow.puml
├── rendered/              # Auto-generated diagram images
│   ├── *.svg             # Vector graphics (preferred for documentation)
│   └── *.png             # Raster graphics (fallback)
└── README.md             # This file
```

## Automatic Diagram Rendering

A GitHub Actions workflow automatically renders PlantUML diagrams whenever `.puml` files are modified:

- **Workflow**: `.github/workflows/plantuml.yml`
- **Triggers**: Push/PR with changes to `docs/diagrams/**/*.puml`
- **Output**: SVG and PNG images in `docs/diagrams/rendered/`
- **Auto-commit**: Generated images are automatically committed to the repository

## Available Diagrams

### Memory Layout (`memory_layout.puml`)
Visual representation of the MSP430FR2355 memory regions with address ranges, sizes, and permissions.

### Memory Access Flow (`memory_access_flow.puml`)
Contains three diagrams:
1. **Memory Access Validation Flow** - Flowchart showing access validation process
2. **Memory Access Permissions Matrix** - Table of regions and their permissions
3. **Memory Component Architecture** - Class diagram showing system components

## Using Diagrams in Documentation

Reference the auto-generated images in markdown files:

```markdown
![Memory Layout](diagrams/rendered/memory_layout.svg)
```

Use SVG format when possible for better scalability and quality.

## Local Development

To render diagrams locally during development:

1. **Install Java 17+** and **Graphviz**:
   ```bash
   sudo apt-get install -y openjdk-17-jdk graphviz
   ```

2. **Download PlantUML**:
   ```bash
   wget -O plantuml.jar https://github.com/plantuml/plantuml/releases/latest/download/plantuml.jar
   ```

3. **Render diagrams**:
   ```bash
   # SVG format (recommended)
   find docs/diagrams -name "*.puml" -exec java -jar plantuml.jar -tsvg -o ../rendered {} \;
   
   # PNG format
   find docs/diagrams -name "*.puml" -exec java -jar plantuml.jar -tpng -o ../rendered {} \;
   ```

## PlantUML Syntax Guidelines

- Use `!theme plain` for consistent styling
- Include descriptive titles
- Add notes for clarification when needed
- Test complex diagrams locally before committing
- Keep diagram files focused on single concepts

## Troubleshooting

**Common Issues:**
- **Syntax errors**: Validate PlantUML syntax using online editor
- **Missing Graphviz**: Install `graphviz` package for complex diagrams
- **Table formatting**: Use `salt` format for tables in PlantUML

**Resources:**
- [PlantUML Documentation](https://plantuml.com/)
- [PlantUML Online Editor](https://www.plantuml.com/plantuml/uml/)
- [Salt (table) Syntax](https://plantuml.com/salt)