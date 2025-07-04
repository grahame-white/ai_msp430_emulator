#!/usr/bin/env node

/**
 * GitHub Issues Updater
 *
 * Updates existing GitHub issues when tasks change in MSP430_EMULATOR_TASKS.md.
 * Compares current task state with GitHub issue state and updates only changed fields.
 */

const { Octokit } = require('@octokit/rest');
const { TaskParser } = require('./parse-tasks.js');
const { BOT_USER_AGENT, EXCLUDED_TASKS, TASK_UTILS } = require('./config.js');

class GitHubIssuesUpdater {
    constructor(token, owner, repo) {
        this.octokit = new Octokit({
            auth: token,
            userAgent: BOT_USER_AGENT
        });
        this.owner = owner;
        this.repo = repo;
        this.dryRun = false;

        // Tasks to exclude from issue updates (already implemented or actively being developed)
        this.excludedTasks = EXCLUDED_TASKS;
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
                    console.log(
                        `[DRY RUN] Would update issue #${issue.number} for Task ${task.id}:`
                    );
                    changes.forEach(change =>
                        console.log(`  - ${change.field}: ${change.description}`)
                    );
                    results.updated.push({
                        task,
                        issueNumber: issue.number,
                        changes,
                        dryRun: true
                    });
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
                        console.log(
                            `[DRY RUN] Would create impact analysis for issue #${issue.number}`
                        );
                    } else {
                        await this.createImpactAnalysis(issue, task);
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
            // In dry-run mode without token, return empty array
            if (this.dryRun && !process.env.GITHUB_TOKEN) {
                console.log('[DRY RUN] Would fetch task issues from GitHub API');
                return [];
            }

            // Add delay to respect rate limits
            await this.delay(1000);

            // Use the regular issues list API instead of deprecated search API
            const issues = await this.octokit.rest.issues.listForRepo({
                owner: this.owner,
                repo: this.repo,
                state: 'all',
                labels: 'task',
                per_page: 100,
                sort: 'created',
                direction: 'desc'
            });

            // Filter to only issues that have "Task" in the title (to match the previous search behavior)
            return issues.data.filter(
                issue => issue.title.includes('Task') && TASK_UTILS.isTaskIssueTitle(issue.title)
            );
        } catch (error) {
            console.warn(`Warning: Could not fetch task issues: ${error.message}`);
            // If rate limited, wait longer and return empty array
            if (error.status === 403 && error.message.includes('rate limit')) {
                console.log('Rate limited, waiting 60 seconds before continuing...');
                await this.delay(60000);
            }
            return [];
        }
    }

    /**
     * Find the GitHub issue corresponding to a task
     */
    findIssueForTask(issues, task) {
        return issues.find(issue => TASK_UTILS.issueMatchesTaskId(issue, task.id));
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
        const currentLabels = issue.labels.map(label =>
            typeof label === 'string' ? label : label.name
        );

        const missingLabels = expectedLabels.filter(label => !currentLabels.includes(label));
        const extraLabels = currentLabels.filter(
            label => !expectedLabels.includes(label) && !label.startsWith('automated-') // Keep automation-related labels
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
     * Create an impact analysis issue for a completed task that became incomplete
     */
    async createImpactAnalysis(originalIssue, task) {
        const analysisTitle = `Impact Analysis: Task ${task.id} Requirements Changed`;
        const analysisBody = this.generateImpactAnalysisBody(originalIssue, task);
        const analysisLabels = [
            'impact-analysis',
            'requirements-change',
            ...this.generateLabels(task)
        ];

        // Create the impact analysis issue
        const result = await this.octokit.rest.issues.create({
            owner: this.owner,
            repo: this.repo,
            title: analysisTitle,
            body: analysisBody,
            labels: analysisLabels
        });

        // Add explanatory comment to original issue without reopening
        await this.octokit.rest.issues.createComment({
            owner: this.owner,
            repo: this.repo,
            issue_number: originalIssue.number,
            body: `📊 **Requirements Changed**: Task ${task.id} has changed from completed to incomplete.\n\nAn impact analysis has been created: #${result.data.number}\n\n*This issue remains closed. Please review the impact analysis for next steps.*`
        });

        return result.data;
    }

    /**
     * Generate body content for impact analysis issue
     */
    generateImpactAnalysisBody(originalIssue, task) {
        let body = `🔄 **Task Requirements Changed**\n\n`;
        body += `**Original Issue**: #${originalIssue.number} - ${originalIssue.title}\n`;
        body += `**Task**: ${task.id}\n`;
        body += `**Status Change**: Completed → Incomplete\n\n`;

        body += `## Issue Summary\n\n`;
        body += `Task ${task.id} was previously marked as completed and issue #${originalIssue.number} was closed. `;
        body += `However, the task requirements have changed and the task is now marked as incomplete.\n\n`;

        body += `## Impact Analysis Checklist\n\n`;
        body += `- [ ] Review changes to task requirements in MSP430_EMULATOR_TASKS.md\n`;
        body += `- [ ] Assess if previous implementation is still valid\n`;
        body += `- [ ] Determine if additional work is needed\n`;
        body += `- [ ] Update or create new issue if work is required\n`;
        body += `- [ ] Close this impact analysis once review is complete\n\n`;

        body += `## Current Task Details\n\n`;
        body += this.generateIssueBody(task);

        return body;
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
    const dryRun = process.argv.includes('--dry-run');

    // Get tasks file path from arguments, excluding flags
    const taskFileArg = process.argv.slice(2).find(arg => !arg.startsWith('--'));
    const tasksFile = taskFileArg || '../../MSP430_EMULATOR_TASKS.md';

    // For dry-run mode, we can operate with a dummy token since no API calls will be made
    if (!token && !dryRun) {
        console.error('Error: GITHUB_TOKEN environment variable is required for live operations');
        console.error('Hint: Use --dry-run flag to preview changes without authentication');
        process.exit(1);
    }

    try {
        // Use dummy token for dry-run mode if no real token is provided
        const effectiveToken = token || (dryRun ? 'dummy-token-for-dry-run' : null);

        if (!effectiveToken) {
            console.error('Error: GITHUB_TOKEN environment variable is required');
            process.exit(1);
        }

        // Parse tasks
        const parser = new TaskParser(tasksFile);
        const tasks = await parser.parse();

        console.log(`Found ${tasks.length} tasks to check for updates`);

        // Update issues
        const updater = new GitHubIssuesUpdater(effectiveToken, owner, repo);
        if (dryRun) {
            updater.enableDryRun();
            console.log('🔍 Running in DRY RUN mode - no actual changes will be made\n');
            if (!token) {
                console.log('ℹ️  No GITHUB_TOKEN provided - running in offline preview mode\n');
            }
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

            // Exit with error code if there were errors (but ignore API errors in dry-run mode without token)
            const hasNonApiErrors = results.errors.some(
                error =>
                    !dryRun ||
                    (!error.error.includes('API') && !error.error.includes('authentication'))
            );
            if (hasNonApiErrors || !dryRun) {
                process.exit(1);
            }
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
