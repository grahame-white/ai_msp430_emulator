# AI Developer Guidance for MSP430 Emulator Project

This document contains essential guidance for AI developers working on the MSP430 emulator project. This guidance should be referenced when processing any prompt or request related to this project.

## Core Development Principles

### Continuous Task List Assessment

AI developers working on this project should regularly reassess and update this task list based on:

**New Information Sources**:

- MSP430 family datasheets and errata documents
- Real hardware testing results and behavioral discoveries
- Community feedback and bug reports
- Performance profiling and optimization insights
- Security vulnerability assessments

**Task List Update Triggers**:

- When discovering MSP430 behavioral edge cases not covered in current tasks
- When identifying more efficient implementation approaches
- When encountering technical blockers requiring task restructuring
- When new testing strategies reveal gaps in acceptance criteria
- When defects indicate missing validation steps

**Update Process**:

1. **Document Discovery**: Log new information source and key findings
2. **Impact Assessment**: Evaluate which existing tasks need modification
3. **Task Revision**: Update acceptance criteria, dependencies, or effort estimates
4. **New Task Creation**: Add tasks for newly identified requirements
5. **Priority Rebalancing**: Adjust task ordering based on risk and dependencies
6. **Validation**: Ensure updated tasks maintain consistency and completeness

### Adaptive Development Approach

- **Fail Fast**: If a task approach proves ineffective, document lessons learned and revise
- **Iterative Refinement**: Use insights from completed tasks to improve remaining tasks
- **Cross-Reference Validation**: Regularly verify task implementation against MSP430 specifications
- **Community Integration**: Incorporate feedback from hardware experts and emulation community

## Code Quality Standards

- **One Class Per File**: Each class, interface, or enum should be in its own file
- **Consistent Naming**: Use PascalCase for public members, camelCase for private members
- **Comprehensive Testing**: Minimum 80% code coverage with one assertion per test
- **Parametric Testing**: Use xUnit theories to reduce test duplication
- **Logging**: Include structured logging at appropriate levels
- **Error Handling**: Implement comprehensive exception handling with custom exceptions

## Visual Documentation Standards

### GitHub-Native Visualization Requirements

- Use consistent color coding across all diagrams (ensure colorblind accessibility)
- Standardize symbols and notation (IEEE/UML standards where applicable)
- Include diagram legends and annotations inline within markdown
- Ensure diagrams are accessible and render properly in GitHub's markdown viewer
- Version control all diagram source files as markdown with embedded Mermaid/ASCII
- Use GitHub-native markdown visualizations (Mermaid diagrams, markdown tables, ASCII art)
- Design diagrams for GitHub's content width constraints (not full screen width)
- Prefer inline visualizations over external image files when possible

### GitHub-Native Visualization Options

- **Mermaid diagrams**: Flowcharts, sequence diagrams, state diagrams, system architecture
- **Markdown tables**: Memory layouts, register bit fields, instruction formats
- **ASCII art**: Simple block diagrams, timing charts, visual separators
- **Code blocks with comments**: Annotated data structures, configuration examples
- **Nested lists with symbols**: Decision trees, hierarchical relationships

## Testing Strategy

### Unit Testing Requirements

- Test each instruction with all addressing modes
- Test flag setting for all possible outcomes
- Test edge cases (overflow, underflow, zero results)
- One assertion per test method
- Use xUnit theories for parametric testing
- Achieve minimum 80% code coverage

### Integration Testing Requirements

- Test complete instruction sequences
- Test peripheral interactions
- Test real program execution
- Validate timing accuracy
- Test interrupt handling

## Development Workflow

1. **Branch Creation**: Create feature branch for each task
2. **Implementation**: Follow TDD approach where possible
3. **Testing**: Write comprehensive unit tests with single assertions
4. **Code Review**: Ensure adherence to coding standards
5. **Integration**: Merge only after all tests pass and code coverage maintained
6. **Documentation**: Update relevant documentation with changes

## Success Metrics

- **Test Coverage**: Maintain minimum 80% code coverage
- **Build Success**: All builds must pass without warnings
- **Performance**: Emulator should achieve reasonable execution speed
- **Accuracy**: High fidelity to actual MSP430 behavior
- **Maintainability**: Clean, well-documented, and extensible code

## Task Ordering Philosophy

Tasks are ordered to establish a solid foundation first, then build complexity incrementally:

1. **Infrastructure & Setup** - Essential project foundation including defect management and automation
2. **Core Architecture** - Basic emulator framework with visual documentation
3. **CPU Core** - Fundamental processing unit with state diagrams
4. **Memory System** - Data storage and access with layout diagrams
5. **Instruction Set** - CPU operations (ordered by complexity) with flow diagrams
6. **Peripherals** - External interfaces and devices with interaction diagrams
7. **Advanced Features** - Debugging, optimization, validation with workflow diagrams
8. **Documentation & Polish** - User-facing materials with comprehensive visual guides

---

**Note**: This guidance should be consulted for every development task to ensure consistency and quality across the MSP430 emulator project.