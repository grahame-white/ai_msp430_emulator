#!/usr/bin/env node

/**
 * Task Parser for MSP430 Emulator Task List
 *
 * Parses MSP430_EMULATOR_TASKS.md to extract task information for GitHub issues automation.
 * Each task is parsed with its metadata, acceptance criteria, files to create, and testing strategy.
 */

const fs = require('fs');
const path = require('path');

class TaskParser {
    constructor(filePath) {
        this.filePath = filePath;
        this.content = '';
        this.tasks = [];
    }

    /**
     * Load and parse the task markdown file
     */
    async parse() {
        try {
            this.content = fs.readFileSync(this.filePath, 'utf8');
            this.extractTasks();
            return this.tasks;
        } catch (error) {
            throw new Error(`Failed to parse tasks file: ${error.message}`);
        }
    }

    /**
     * Extract all tasks from the markdown content
     */
    extractTasks() {
        // Split content into sections by task headers (### Task X.Y:)
        const taskRegex = /### Task (\d+\.\d+): (.+?)(?=### Task|\n## |$)/gs;
        let match;

        while ((match = taskRegex.exec(this.content)) !== null) {
            const taskId = match[1];
            // Extract just the title line, not the entire content
            const titleLine = match[2].split('\n')[0].trim();
            const taskContent = match[0];

            const task = this.parseTaskSection(taskId, titleLine, taskContent);
            if (task) {
                this.tasks.push(task);
            }
        }
    }

    /**
     * Parse individual task section to extract metadata
     */
    parseTaskSection(taskId, title, content) {
        const task = {
            id: taskId,
            title: title,
            phase: this.extractPhase(taskId),
            priority: this.extractPriority(content),
            effort: this.extractEffort(content),
            dependencies: this.extractDependencies(content),
            requiredReading: this.extractRequiredReading(content),
            description: this.extractDescription(content),
            acceptanceCriteria: this.extractAcceptanceCriteria(content),
            filesToCreate: this.extractFilesToCreate(content),
            testingStrategy: this.extractTestingStrategy(content),
            completed: this.checkCompletion(content)
        };

        return task;
    }

    /**
     * Extract phase number from task ID
     */
    extractPhase(taskId) {
        const phaseNum = taskId.split('.')[0];
        return `Phase ${phaseNum}`;
    }

    /**
     * Extract priority from task content
     */
    extractPriority(content) {
        const match = content.match(/\*\*Priority\*\*:\s*(.+)/);
        return match ? match[1].trim() : 'Medium';
    }

    /**
     * Extract estimated effort from task content
     */
    extractEffort(content) {
        const match = content.match(/\*\*Estimated Effort\*\*:\s*(.+)/);
        return match ? match[1].trim() : 'TBD';
    }

    /**
     * Extract dependencies from task content
     */
    extractDependencies(content) {
        const match = content.match(/\*\*Dependencies\*\*:\s*(.+)/);
        if (!match || match[1].trim() === 'None') {
            return [];
        }

        // Parse dependencies like "Task 1.1" or "Task 1.1, Task 1.2"
        const depString = match[1].trim();
        const deps = depString
            .split(',')
            .map(dep => {
                const taskMatch = dep.trim().match(/Task (\d+\.\d+)/);
                return taskMatch ? taskMatch[1] : null;
            })
            .filter(Boolean);

        return deps;
    }

    /**
     * Extract required reading list from task content
     */
    extractRequiredReading(content) {
        const requiredReading = [];
        const requiredReadingMatch = content.match(
            /\*\*Required Reading\*\*:\s*([\s\S]*?)(?=\n\n|$|\*\*[A-Z])/
        );

        if (requiredReadingMatch) {
            const readingText = requiredReadingMatch[1];
            const lines = readingText.split('\n');

            for (const line of lines) {
                const trimmed = line.trim();
                if (trimmed.startsWith('- ')) {
                    requiredReading.push(trimmed.substring(2).trim());
                }
            }
        }

        return requiredReading;
    }

    /**
     * Extract main task description (first paragraph after title)
     */
    extractDescription(content) {
        const lines = content.split('\n');
        let description = '';
        let foundDependencies = false;
        let foundRequiredReading = false;
        let fallbackDescription = '';
        let inFallbackCapture = false;

        for (const line of lines) {
            const trimmed = line.trim();

            // Start looking for description after Dependencies line
            if (trimmed.startsWith('**Dependencies**')) {
                foundDependencies = true;
                inFallbackCapture = false;
                continue;
            }

            // Skip Required Reading section
            if (trimmed.startsWith('**Required Reading**')) {
                foundRequiredReading = true;
                inFallbackCapture = false;
                continue;
            }

            // Start fallback capture after task metadata (Priority, Effort, Dependencies, Required Reading)
            if (trimmed.startsWith('**Priority**') || trimmed.startsWith('**Estimated Effort**')) {
                inFallbackCapture = false;
                continue;
            }

            // Stop at acceptance criteria or other sections
            if (
                trimmed.startsWith('**Acceptance Criteria**') ||
                trimmed.startsWith('**Files to Create**') ||
                trimmed.startsWith('**Testing Strategy**')
            ) {
                break;
            }

            // Capture description after dependencies OR required reading (primary method)
            if (
                (foundDependencies || foundRequiredReading) &&
                trimmed &&
                !trimmed.startsWith('**') &&
                !trimmed.startsWith('- [')
            ) {
                description += trimmed + ' ';
            }

            // Fallback: capture content that looks like description
            if (
                !foundDependencies &&
                !foundRequiredReading &&
                !inFallbackCapture &&
                trimmed &&
                !trimmed.startsWith('**') &&
                !trimmed.startsWith('#')
            ) {
                inFallbackCapture = true;
            }

            if (inFallbackCapture && trimmed && !trimmed.startsWith('**')) {
                fallbackDescription += trimmed + ' ';
            }
        }

        // Return primary description if found, otherwise fallback
        const result = description.trim() || fallbackDescription.trim();
        return result;
    }

    /**
     * Extract acceptance criteria checkboxes
     */
    extractAcceptanceCriteria(content) {
        const criteria = [];
        const criteriaMatch = content.match(
            /\*\*Acceptance Criteria\*\*:\s*([\s\S]*?)(?=\*\*Files to Create\*\*|\*\*Testing Strategy\*\*|$)/
        );

        if (criteriaMatch) {
            const criteriaText = criteriaMatch[1];
            const lines = criteriaText.split('\n');

            for (const line of lines) {
                const trimmed = line.trim();
                if (trimmed.startsWith('- [ ]') || trimmed.startsWith('- [x]')) {
                    criteria.push({
                        text: trimmed.substring(5).trim(),
                        completed: trimmed.startsWith('- [x]')
                    });
                }
            }
        }

        return criteria;
    }

    /**
     * Extract files to create from code block
     */
    extractFilesToCreate(content) {
        const files = [];
        const filesMatch = content.match(/\*\*Files to Create\*\*:\s*```[\s\S]*?\n([\s\S]*?)```/);

        if (filesMatch) {
            const filesText = filesMatch[1];
            const lines = filesText.split('\n');

            for (const line of lines) {
                const trimmed = line.trim();
                if (trimmed && !trimmed.startsWith('#') && !trimmed.startsWith('//')) {
                    files.push(trimmed);
                }
            }
        }

        return files;
    }

    /**
     * Extract testing strategy
     */
    extractTestingStrategy(content) {
        const strategy = [];
        const strategyMatch = content.match(
            /\*\*Testing Strategy\*\*:\s*([\s\S]*?)(?=\*\*Script-Specific Test Steps\*\*|---|\n## |$)/
        );

        if (strategyMatch) {
            const strategyText = strategyMatch[1];
            const lines = strategyText.split('\n');

            for (const line of lines) {
                const trimmed = line.trim();
                if (trimmed.startsWith('- ')) {
                    strategy.push(trimmed.substring(2).trim());
                }
            }
        }

        return strategy;
    }

    /**
     * Check if task is completed based on all acceptance criteria being checked
     */
    checkCompletion(content) {
        const criteria = this.extractAcceptanceCriteria(content);
        if (criteria.length === 0) {
            return false;
        }

        return criteria.every(criterion => criterion.completed);
    }

    /**
     * Get tasks by phase
     */
    getTasksByPhase() {
        const tasksByPhase = {};

        for (const task of this.tasks) {
            if (!tasksByPhase[task.phase]) {
                tasksByPhase[task.phase] = [];
            }
            tasksByPhase[task.phase].push(task);
        }

        return tasksByPhase;
    }

    /**
     * Get incomplete tasks
     */
    getIncompleteTasks() {
        return this.tasks.filter(task => !task.completed);
    }

    /**
     * Get completed tasks
     */
    getCompletedTasks() {
        return this.tasks.filter(task => task.completed);
    }

    /**
     * Get task by ID
     */
    getTaskById(taskId) {
        return this.tasks.find(task => task.id === taskId);
    }
}

// Export for use as module
module.exports = { TaskParser };

// Main function for CLI usage
async function main() {
    const filePath = process.argv[2] || path.resolve(__dirname, '../../MSP430_EMULATOR_TASKS.md');

    try {
        const parser = new TaskParser(filePath);
        const tasks = await parser.parse();

        console.log(
            JSON.stringify(
                {
                    totalTasks: tasks.length,
                    completedTasks: parser.getCompletedTasks().length,
                    incompleteTasks: parser.getIncompleteTasks().length,
                    tasksByPhase: parser.getTasksByPhase(),
                    tasks: tasks
                },
                null,
                2
            )
        );
    } catch (error) {
        console.error('Error:', error.message);
        process.exit(1);
    }
}

// CLI usage if run directly
if (require.main === module) {
    main();
}
