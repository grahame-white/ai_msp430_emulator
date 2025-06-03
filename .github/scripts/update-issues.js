#!/usr/bin/env node

/**
 * GitHub Issues Updater
 *
 * Updates existing GitHub issues when tasks change in MSP430_EMULATOR_TASKS.md.
 * Compares current task state with GitHub issue state and updates only changed fields.
 */

const { Octokit } = require('@octokit/rest');
const { TaskParser } = require('./parse-tasks.js');

class GitHubIssuesUpdater {
    constructor(token, owner, repo) {
        this.octokit = new Octokit({
            auth: token,
            userAgent: 'MSP430-Emulator-Issues-Bot v1.0.0'
        });
        this.owner = owner;
        this.repo = repo;
        this.dryRun = false;

        // Tasks to exclude from issue updates (already implemented or actively being developed)
        this.excludedTasks = ['1.1', '1.2', '1.3', '1.4', '1.5'];
    }

    /**
     * Enable dry run mode (preview only, no actual API calls)
     */
    enableDryRun() {
        this.dryRun = true;
    }

    /**
     * Update GitHub issues based on task changes
     */
    async updateIssuesFromTasks(tasks) {
        const results = {
            updated: [],
            closed: [],
            skipped: [],
            errors: []
        };

        // Get all task-related issues
        const existingIssues = await this.getAllTaskIssues();

        for (const task of tasks) {
            try {
                // Check if task is excluded from issue updates
                if (this.excludedTasks.includes(task.id)) {
                    results.skipped.push({
                        task: task,
                        reason: 'Task excluded (already implemented or actively being developed)',
                        issueNumber: null
                    });
                    continue;
                }

                const issue = this.findIssueForTask(existingIssues, task);

                if (!issue) {
                    results.skipped.push({
                        task: task,
                        reason: 'No corresponding issue found'
                    });
                    continue;
                }

                // Check if issue is manually created (protected)
                if (this.isManuallyCreated(issue)) {
                    results.skipped.push({
                        task: task,
                        reason: 'Issue is manually created and protected',
                        issueNumber: issue.number
                    });
                    continue;
                }

                const changes = this.detectChanges(issue, task);

                if (changes.length === 0) {
                    results.skipped.push({
                        task: task,
                        reason: 'No changes detected',
                        issueNumber: issue.number
                    });
                    continue;
                }

                // Apply updates
                if (this.dryRun) {
                    console.log(`[DRY RUN] Would update issue #${issue.number} for Task ${task.id}:`);
                    changes.forEach(change => console.log(`  - ${change.field}: ${change.description}`));
                    results.updated.push({ task, issueNumber: issue.number, changes, dryRun: true });
                } else {
                    await this.applyUpdates(issue, task, changes);
                    results.updated.push({ task, issueNumber: issue.number, changes });

                    // Small delay to respect rate limits
                    await this.delay(500);
                }

                // Handle completion status
                if (task.completed && issue.state === 'open') {
                    if (this.dryRun) {
                        console.log(`[DRY RUN] Would close completed issue #${issue.number}`);
                        results.closed.push({ task, issueNumber: issue.number, dryRun: true });
                    } else {
                        await this.closeIssue(issue.number, 'Task completed');
                        results.closed.push({ task, issueNumber: issue.number });
                    }
                } else if (!task.completed && issue.state === 'closed') {
                    if (this.dryRun) {
                        console.log(`[DRY RUN] Would reopen issue #${issue.number}`);
                    } else {
                        await this.reopenIssue(issue.number);
                    }
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
     * Get all task-related issues from the repository
     */
    async getAllTaskIssues() {
        try {
            const searchQuery = `repo:${this.owner}/${this.repo} in:title "Task" label:task`;
            const searchResults = await this.octokit.rest.search.issuesAndPullRequests({
                q: searchQuery,
                sort: 'created',
                order: 'desc',
                per_page: 100
            });

            return searchResults.data.items;
        } catch (error) {
            console.warn(`Warning: Could not search for task issues: ${error.message}`);
            return [];
        }
    }

    /**
     * Find the GitHub issue corresponding to a task
     */
    findIssueForTask(issues, task) {
        return issues.find(issue => {
            const titleMatch = issue.title.match(/Task (\d+\.\d+):/);
            return titleMatch && titleMatch[1] === task.id;
        });
    }

    /**
     * Check if an issue was manually created (not managed by automation)
     */
    isManuallyCreated(issue) {
        // Check if issue body contains automation footer
        const automationFooter = '🤖 Managed by GitHub Issues Automation';
        return !issue.body || !issue.body.includes(automationFooter);
    }

    /**
     * Detect changes between the GitHub issue and the task
     */
    detectChanges(issue, task) {
        const changes = [];

        // Check title changes
        const expectedTitle = `Task ${task.id}: ${task.title}`;
        if (issue.title !== expectedTitle) {
            changes.push({
                field: 'title',
                current: issue.title,
                expected: expectedTitle,
                description: 'Title updated'
            });
        }

        // Check body changes
        const expectedBody = this.generateIssueBody(task);
        if (issue.body !== expectedBody) {
            changes.push({
                field: 'body',
                current: issue.body?.length || 0,
                expected: expectedBody.length,
                description: 'Body content updated'
            });
        }

        // Check label changes
        const expectedLabels = this.generateLabels(task);
        const currentLabels = issue.labels.map(label => typeof label === 'string' ? label : label.name);

        const missingLabels = expectedLabels.filter(label => !currentLabels.includes(label));
        const extraLabels = currentLabels.filter(label =>
            !expectedLabels.includes(label) &&
            !label.startsWith('automated-') // Keep automation-related labels
        );

        if (missingLabels.length > 0 || extraLabels.length > 0) {
            changes.push({
                field: 'labels',
                current: currentLabels,
                expected: expectedLabels,
                description: `Labels updated (add: ${missingLabels.join(', ')}, remove: ${extraLabels.join(', ')})`
            });
        }

        return changes;
    }

    /**
     * Apply updates to the GitHub issue
     */
    async applyUpdates(issue, task, changes) {
        const updateData = {};

        for (const change of changes) {
            switch (change.field) {
            case 'title':
                updateData.title = change.expected;
                break;
            case 'body':
                updateData.body = this.generateIssueBody(task);
                break;
            case 'labels':
                updateData.labels = change.expected;
                break;
            }
        }

        if (Object.keys(updateData).length > 0) {
            await this.octokit.rest.issues.update({
                owner: this.owner,
                repo: this.repo,
                issue_number: issue.number,
                ...updateData
            });
        }
    }

    /**
     * Generate issue body markdown from task data (same as create-issues.js)
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
     * Generate labels for the task (same as create-issues.js)
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
     * Close an issue with a comment
     */
    async closeIssue(issueNumber, reason) {
        // Add closing comment
        await this.octokit.rest.issues.createComment({
            owner: this.owner,
            repo: this.repo,
            issue_number: issueNumber,
            body: `🎉 **Task completed!**\n\n${reason}\n\n*Automatically closed by GitHub Issues Automation*`
        });

        // Close the issue
        await this.octokit.rest.issues.update({
            owner: this.owner,
            repo: this.repo,
            issue_number: issueNumber,
            state: 'closed'
        });
    }

    /**
     * Reopen an issue
     */
    async reopenIssue(issueNumber) {
        await this.octokit.rest.issues.createComment({
            owner: this.owner,
            repo: this.repo,
            issue_number: issueNumber,
            body: '🔄 **Task reopened**\n\nTask status changed back to incomplete.\n\n*Automatically reopened by GitHub Issues Automation*'
        });

        await this.octokit.rest.issues.update({
            owner: this.owner,
            repo: this.repo,
            issue_number: issueNumber,
            state: 'open'
        });
    }

    /**
     * Utility function for delays
     */
    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}

// Export for use as module
module.exports = { GitHubIssuesUpdater };

// Main function for CLI usage
async function main() {
    const token = process.env.GITHUB_TOKEN;
    const owner = process.env.GITHUB_REPOSITORY?.split('/')[0] || 'grahame-white';
    const repo = process.env.GITHUB_REPOSITORY?.split('/')[1] || 'ai_msp430_emulator';
    const tasksFile = process.argv[2] || './MSP430_EMULATOR_TASKS.md';
    const dryRun = process.argv.includes('--dry-run');

    if (!token) {
        console.error('Error: GITHUB_TOKEN environment variable is required');
        process.exit(1);
    }

    try {
        // Parse tasks
        const parser = new TaskParser(tasksFile);
        const tasks = await parser.parse();

        console.log(`Found ${tasks.length} tasks to check for updates`);

        // Update issues
        const updater = new GitHubIssuesUpdater(token, owner, repo);
        if (dryRun) {
            updater.enableDryRun();
            console.log('Running in DRY RUN mode - no actual changes will be made');
        }

        const results = await updater.updateIssuesFromTasks(tasks);

        console.log('\nResults:');
        console.log(`- Updated: ${results.updated.length} issues`);
        console.log(`- Closed: ${results.closed.length} issues`);
        console.log(`- Skipped: ${results.skipped.length} issues`);
        console.log(`- Errors: ${results.errors.length} issues`);

        if (results.errors.length > 0) {
            console.log('\nErrors:');
            results.errors.forEach(error => {
                console.log(`- Task ${error.task.id}: ${error.error}`);
            });
            process.exit(1);
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
