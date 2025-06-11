#!/usr/bin/env node

/**
 * GitHub Issues Creator
 *
 * Creates GitHub issues from parsed tasks using GitHub API v4 (GraphQL).
 * Handles proper formatting, metadata assignment, and rate limiting.
 */

const { Octokit } = require('@octokit/rest');
const { TaskParser } = require('./parse-tasks.js');
const {
    isPermissionError,
    executeWithPermissionHandling,
    executeWithRateLimit,
    smartDelay
} = require('./github-utils.js');
const { BOT_USER_AGENT, EXCLUDED_TASKS } = require('./config.js');

class GitHubIssuesCreator {
    constructor(token, owner, repo) {
        this.octokit = new Octokit({
            auth: token,
            userAgent: BOT_USER_AGENT
        });
        this.owner = owner;
        this.repo = repo;
        this.dryRun = false;

        // Tasks to exclude from issue creation (already implemented or actively being developed)
        this.excludedTasks = EXCLUDED_TASKS;
    }

    /**
     * Enable dry run mode (preview only, no actual API calls)
     */
    enableDryRun() {
        this.dryRun = true;
    }

    /**
     * Create GitHub issues from tasks
     */
    async createIssuesFromTasks(tasks) {
        const results = {
            created: [],
            skipped: [],
            errors: []
        };

        for (const task of tasks) {
            try {
                // Check if task is excluded from issue creation
                if (this.excludedTasks.includes(task.id)) {
                    results.skipped.push({
                        task: task,
                        reason: 'Task excluded (already implemented or actively being developed)',
                        issueNumber: null
                    });
                    continue;
                }

                // Check if issue already exists
                const existingIssue = await this.findExistingIssue(task);
                if (existingIssue) {
                    results.skipped.push({
                        task: task,
                        reason: 'Issue already exists',
                        issueNumber: existingIssue.number
                    });
                    continue;
                }

                // Create the issue
                const issueData = this.formatIssueData(task);

                if (this.dryRun) {
                    console.log(`[DRY RUN] Would create issue: ${issueData.title}`);
                    results.created.push({ task, issueNumber: null, dryRun: true });
                } else {
                    const issue = await this.createIssue(issueData);
                    results.created.push({ task, issueNumber: issue.number });

                    // Add labels and milestone
                    await this.applyMetadata(issue.number, task);

                    // Smart delay to respect rate limits
                    await smartDelay(1000);
                }
            } catch (error) {
                results.errors.push({
                    task: task,
                    error: error.message
                });
            }
        }

        return results;
    }

    /**
     * Find existing issue for a task
     */
    async findExistingIssue(task) {
        try {
            // In dry-run mode without token, skip API calls
            if (
                this.dryRun &&
                (!process.env.GITHUB_TOKEN ||
                    process.env.GITHUB_TOKEN === 'dummy-token-for-dry-run')
            ) {
                console.log(`[DRY RUN] Would check for existing issue for task ${task.id}`);
                return null;
            }

            // Use the regular issues list API instead of deprecated search API
            // This is more reliable and doesn't have the deprecation warnings
            const issues = await executeWithRateLimit(
                () =>
                    this.octokit.rest.issues.listForRepo({
                        owner: this.owner,
                        repo: this.repo,
                        state: 'all',
                        labels: 'task',
                        per_page: 100,
                        sort: 'created',
                        direction: 'desc'
                    }),
                `list issues for task ${task.id}`,
                3
            );

            // Check if any issue matches our exact task ID
            for (const issue of issues.data) {
                const titleMatch = issue.title.match(/Task (\d+\.\d+):/);
                if (titleMatch && titleMatch[1] === task.id) {
                    return issue;
                }
            }

            return null;
        } catch (error) {
            console.warn(
                `Warning: Could not check for existing issue for task ${task.id}: ${error.message}`
            );
            return null;
        }
    }

    /**
     * Format task data for GitHub issue creation
     */
    formatIssueData(task) {
        const title = `Task ${task.id}: ${task.title}`;
        const body = this.generateIssueBody(task);

        return {
            title,
            body,
            labels: this.generateLabels(task)
        };
    }

    /**
     * Generate issue body markdown from task data
     */
    generateIssueBody(task) {
        let body = '';

        // Task metadata
        body += `**Priority**: ${task.priority}\n`;
        body += `**Estimated Effort**: ${task.effort}\n`;
        body += `**Phase**: ${task.phase}\n`;

        if (task.dependencies && task.dependencies.length > 0) {
            body += `**Dependencies**: ${task.dependencies.map(dep => `Task ${dep}`).join(', ')}\n`;
        } else {
            body += '**Dependencies**: None\n';
        }
        body += '\n';

        // Description
        if (task.description) {
            body += `${task.description}\n\n`;
        }

        // Acceptance Criteria
        if (task.acceptanceCriteria && task.acceptanceCriteria.length > 0) {
            body += '## Acceptance Criteria\n\n';
            for (const criterion of task.acceptanceCriteria) {
                const checkbox = criterion.completed ? '[x]' : '[ ]';
                body += `- ${checkbox} ${criterion.text}\n`;
            }
            body += '\n';
        }

        // Files to Create
        if (task.filesToCreate && task.filesToCreate.length > 0) {
            body += '## Files to Create\n\n';
            body += '```\n';
            for (const file of task.filesToCreate) {
                body += `${file}\n`;
            }
            body += '```\n\n';
        }

        // Testing Strategy
        if (task.testingStrategy && task.testingStrategy.length > 0) {
            body += '## Testing Strategy\n\n';
            for (const strategy of task.testingStrategy) {
                body += `- ${strategy}\n`;
            }
            body += '\n';
        }

        // Automation footer
        body += '---\n\n';
        body += '*This issue was automatically generated from MSP430_EMULATOR_TASKS.md*\n';
        body += '*🤖 Managed by GitHub Issues Automation*';

        return body;
    }

    /**
     * Generate labels for the task
     */
    generateLabels(task) {
        const labels = [];

        // Phase label
        labels.push(task.phase.toLowerCase().replace(' ', '-'));

        // Priority label
        labels.push(`priority-${task.priority.toLowerCase()}`);

        // Task type label
        labels.push('task');

        // Effort label (if contains hours)
        if (task.effort.includes('hours')) {
            const hours = task.effort.match(/(\d+)-?(\d+)?/);
            if (hours) {
                const minHours = parseInt(hours[1]);
                if (minHours <= 2) {
                    labels.push('effort-small');
                } else if (minHours <= 4) {
                    labels.push('effort-medium');
                } else {
                    labels.push('effort-large');
                }
            }
        }

        // Status label
        if (task.completed) {
            labels.push('status-completed');
        } else {
            labels.push('status-pending');
        }

        return labels;
    }

    /**
     * Create the issue via GitHub API
     */
    async createIssue(issueData) {
        const response = await executeWithRateLimit(
            () =>
                this.octokit.rest.issues.create({
                    owner: this.owner,
                    repo: this.repo,
                    title: issueData.title,
                    body: issueData.body,
                    labels: issueData.labels
                }),
            `create issue: ${issueData.title}`
        );

        return response.data;
    }

    /**
     * Apply additional metadata to the issue
     */
    async applyMetadata(issueNumber, task) {
        // Apply milestone if needed
        const milestone = await this.findOrCreateMilestone(task.phase);
        if (milestone) {
            try {
                await executeWithRateLimit(
                    () =>
                        this.octokit.rest.issues.update({
                            owner: this.owner,
                            repo: this.repo,
                            issue_number: issueNumber,
                            milestone: milestone.number
                        }),
                    `assign milestone to issue #${issueNumber}`
                );
            } catch (error) {
                console.warn(
                    `Warning: Could not assign milestone to issue ${issueNumber}: ${error.message}`
                );
            }
        }
    }

    /**
     * Find or create milestone for the phase
     */
    async findOrCreateMilestone(phase) {
        try {
            // Try to find existing milestone
            const milestones = await executeWithRateLimit(
                () =>
                    this.octokit.rest.issues.listMilestones({
                        owner: this.owner,
                        repo: this.repo,
                        state: 'all'
                    }),
                `list milestones for ${phase}`
            );

            const existingMilestone = milestones.data.find(m => m.title === phase);
            if (existingMilestone) {
                return existingMilestone;
            }

            // Create new milestone if it doesn't exist
            if (!this.dryRun) {
                const response = await executeWithRateLimit(
                    () =>
                        this.octokit.rest.issues.createMilestone({
                            owner: this.owner,
                            repo: this.repo,
                            title: phase,
                            description: `Tasks for ${phase} of MSP430 Emulator development`
                        }),
                    `create milestone for ${phase}`
                );
                return response.data;
            } else {
                console.log(`[DRY RUN] Would create milestone: ${phase}`);
            }

            return null;
        } catch (error) {
            console.warn(`Warning: Could not handle milestone for ${phase}: ${error.message}`);
            return null;
        }
    }

    /**
     * Ensure required labels exist in the repository
     */
    async ensureLabelsExist() {
        const requiredLabels = [
            { name: 'task', color: '0075ca', description: 'Development task from task list' },
            {
                name: 'phase-1',
                color: 'c5def5',
                description: 'Phase 1: Project Infrastructure & Setup'
            },
            {
                name: 'phase-2',
                color: 'c5def5',
                description: 'Phase 2: Core Architecture Foundation'
            },
            {
                name: 'phase-3',
                color: 'c5def5',
                description: 'Phase 3: Memory System Implementation'
            },
            {
                name: 'phase-4',
                color: 'c5def5',
                description: 'Phase 4: CPU Core Implementation'
            },
            {
                name: 'phase-5',
                color: 'c5def5',
                description: 'Phase 5: CPU Instruction Set Implementation'
            },
            {
                name: 'phase-6',
                color: 'c5def5',
                description: 'Phase 6: CPU Instruction Set - Control Flow'
            },
            {
                name: 'phase-7',
                color: 'c5def5',
                description: 'Phase 7: CPU Instruction Set - System Control'
            },
            {
                name: 'phase-8',
                color: 'c5def5',
                description: 'Phase 8: Interrupt System'
            },
            {
                name: 'phase-9',
                color: 'c5def5',
                description: 'Phase 9: Binary File Import/Export'
            },
            {
                name: 'phase-10',
                color: 'c5def5',
                description: 'Phase 10: Hardware Validation & Testing'
            },
            {
                name: 'phase-11',
                color: 'c5def5',
                description: 'Phase 11: Documentation & Advanced Features'
            },
            { name: 'priority-critical', color: 'd73a49', description: 'Critical priority task' },
            { name: 'priority-high', color: 'fd7e14', description: 'High priority task' },
            { name: 'priority-medium', color: 'fbca04', description: 'Medium priority task' },
            { name: 'priority-low', color: '28a745', description: 'Low priority task' },
            { name: 'effort-small', color: 'e4e669', description: '1-2 hours effort' },
            { name: 'effort-medium', color: 'f9d71c', description: '3-4 hours effort' },
            { name: 'effort-large', color: 'd4ac0d', description: '5+ hours effort' },
            { name: 'status-pending', color: 'ededed', description: 'Task not yet started' },
            { name: 'status-completed', color: '28a745', description: 'Task completed' }
        ];

        for (const labelData of requiredLabels) {
            try {
                await executeWithRateLimit(
                    () =>
                        this.octokit.rest.issues.getLabel({
                            owner: this.owner,
                            repo: this.repo,
                            name: labelData.name
                        }),
                    `get label ${labelData.name}`
                );
            } catch (error) {
                if (error.status === 404) {
                    // Label doesn't exist, create it
                    if (!this.dryRun) {
                        const createResult = await executeWithPermissionHandling(
                            async () => {
                                await executeWithRateLimit(
                                    () =>
                                        this.octokit.rest.issues.createLabel({
                                            owner: this.owner,
                                            repo: this.repo,
                                            name: labelData.name,
                                            color: labelData.color,
                                            description: labelData.description
                                        }),
                                    `create label ${labelData.name}`
                                );
                            },
                            `create label ${labelData.name}`,
                            'repository'
                        );
                        if (createResult.success) {
                            console.log(`Created label: ${labelData.name}`);
                        }
                    } else {
                        console.log(`[DRY RUN] Would create label: ${labelData.name}`);
                    }
                } else if (isPermissionError(error)) {
                    console.warn(
                        `⚠️  Cannot access label ${labelData.name} due to insufficient permissions`
                    );
                } else {
                    console.warn(`⚠️  Error checking label ${labelData.name}: ${error.message}`);
                }
            }
        }
    }
}

// Export for use as module
module.exports = { GitHubIssuesCreator };

// Main function for CLI usage
async function main() {
    const token = process.env.GITHUB_TOKEN;
    const owner = process.env.GITHUB_REPOSITORY?.split('/')[0] || 'grahame-white';
    const repo = process.env.GITHUB_REPOSITORY?.split('/')[1] || 'ai_msp430_emulator';
    const tasksFile = process.argv[2] || '../../MSP430_EMULATOR_TASKS.md';
    const dryRun = process.argv.includes('--dry-run');

    if (!token) {
        console.error('Error: GITHUB_TOKEN environment variable is required');
        process.exit(1);
    }

    try {
        // Parse tasks
        const parser = new TaskParser(tasksFile);
        const tasks = await parser.parse();
        const incompleteTasks = parser.getIncompleteTasks();

        console.log(`Found ${tasks.length} total tasks, ${incompleteTasks.length} incomplete`);

        // Create issues
        const creator = new GitHubIssuesCreator(token, owner, repo);
        if (dryRun) {
            creator.enableDryRun();
            console.log('Running in DRY RUN mode - no actual changes will be made');
        }

        // Ensure labels exist
        await creator.ensureLabelsExist();

        // Create issues for incomplete tasks
        const results = await creator.createIssuesFromTasks(incompleteTasks);

        console.log('\nResults:');
        console.log(`- Created: ${results.created.length} issues`);
        console.log(`- Skipped: ${results.skipped.length} issues`);
        console.log(`- Errors: ${results.errors.length} issues`);

        if (results.errors.length > 0) {
            console.log('\nErrors:');
            results.errors.forEach(error => {
                console.log(`- Task ${error.task.id}: ${error.error}`);
            });
        }
    } catch (error) {
        console.error('Error:', error.message);
        process.exit(1);
    }
}

// CLI usage if run directly
if (require.main === module) {
    main();
}
