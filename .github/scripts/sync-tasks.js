#!/usr/bin/env node

/**
 * GitHub Issues Synchronizer
 *
 * Orchestrates the full synchronization workflow between MSP430_EMULATOR_TASKS.md
 * and GitHub issues. Handles dependency linking, milestone organization, and cleanup.
 */

const { Octokit } = require('@octokit/rest');
const { TaskParser } = require('./parse-tasks.js');
const { GitHubIssuesCreator } = require('./create-issues.js');
const { GitHubIssuesUpdater } = require('./update-issues.js');
const { executeWithRateLimit, smartDelay } = require('./github-utils.js');
const { BOT_USER_AGENT, EXCLUDED_TASKS } = require('./config.js');

class GitHubIssuesSynchronizer {
    constructor(token, owner, repo) {
        this.octokit = new Octokit({
            auth: token,
            userAgent: BOT_USER_AGENT
        });
        this.owner = owner;
        this.repo = repo;
        this.dryRun = false;
        this.creator = new GitHubIssuesCreator(token, owner, repo);
        this.updater = new GitHubIssuesUpdater(token, owner, repo);

        // Tasks to exclude from synchronization (already implemented or actively being developed)
        this.excludedTasks = EXCLUDED_TASKS;
    }

    /**
     * Enable dry run mode (preview only, no actual API calls)
     */
    enableDryRun() {
        this.dryRun = true;
        this.creator.enableDryRun();
        this.updater.enableDryRun();
    }

    /**
     * Filter out excluded tasks
     */
    filterIncludedTasks(tasks) {
        return tasks.filter(task => !this.excludedTasks.includes(task.id));
    }

    /**
     * Perform full synchronization of tasks with GitHub issues
     */
    async synchronize(tasksFile) {
        const results = {
            created: [],
            updated: [],
            closed: [],
            linked: [],
            cleaned: [],
            errors: []
        };

        try {
            console.log('🔄 Starting GitHub Issues Synchronization...');

            // Step 1: Parse tasks
            console.log('\n📖 Parsing tasks from markdown...');
            const parser = new TaskParser(tasksFile);
            const allTasks = await parser.parse();
            const tasks = this.filterIncludedTasks(allTasks);
            console.log(
                `Found ${allTasks.length} tasks (${allTasks.length - tasks.length} excluded, ${tasks.length} included)`
            );

            // Step 2: Ensure labels and milestones exist
            console.log('\n🏷️  Setting up labels and milestones...');
            await this.creator.ensureLabelsExist();
            await this.ensurePhaseMilestonesExist(tasks);

            // Step 3: Create new issues for tasks without issues
            console.log('\n➕ Creating new issues...');
            const incompleteTasks = tasks.filter(task => !task.completed);
            const createResults = await this.creator.createIssuesFromTasks(incompleteTasks);
            results.created = createResults.created;
            results.errors.push(...createResults.errors);

            // Step 4: Update existing issues
            console.log('\n🔄 Updating existing issues...');
            const updateResults = await this.updater.updateIssuesFromTasks(tasks);
            results.updated = updateResults.updated;
            results.closed = updateResults.closed;
            results.errors.push(...updateResults.errors);

            // Step 5: Link dependencies between issues
            console.log('\n🔗 Linking dependencies...');
            const linkResults = await this.linkDependencies(tasks);
            results.linked = linkResults.linked;
            results.errors.push(...linkResults.errors);

            // Step 6: Clean up obsolete issues
            console.log('\n🧹 Cleaning up obsolete issues...');
            const cleanResults = await this.cleanupObsoleteIssues(tasks);
            results.cleaned = cleanResults.cleaned;
            results.errors.push(...cleanResults.errors);

            // Step 7: Organize milestones
            console.log('\n📊 Organizing milestones...');
            await this.organizeMilestones(tasks);

            console.log('\n✅ Synchronization completed!');
            this.printSummary(results);

            return results;
        } catch (error) {
            console.error('❌ Synchronization failed:', error.message);
            results.errors.push({ error: error.message });
            return results;
        }
    }

    /**
     * Ensure all phase milestones exist
     */
    async ensurePhaseMilestonesExist(tasks) {
        const phases = [...new Set(tasks.map(task => task.phase))];

        for (const phase of phases) {
            try {
                await this.findOrCreateMilestone(phase);
            } catch (error) {
                console.warn(`Warning: Could not ensure milestone for ${phase}: ${error.message}`);
            }
        }
    }

    /**
     * Find or create milestone for a phase
     */
    async findOrCreateMilestone(phase) {
        try {
            // Try to find existing milestone with proper rate limiting
            const milestones = await executeWithRateLimit(
                () =>
                    this.octokit.rest.issues.listMilestones({
                        owner: this.owner,
                        repo: this.repo,
                        state: 'all'
                    }),
                `find milestone for ${phase}`
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
                console.log(`Created milestone: ${phase}`);
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
     * Link dependencies between issues
     */
    async linkDependencies(tasks) {
        const results = { linked: [], errors: [] };

        // Get all task issues
        const allIssues = await this.getAllTaskIssues();

        for (const task of tasks) {
            if (!task.dependencies || task.dependencies.length === 0) {
                continue;
            }

            try {
                const taskIssue = this.findIssueForTask(allIssues, task);
                if (!taskIssue) {
                    continue;
                }

                for (const depTaskId of task.dependencies) {
                    const depTask = tasks.find(t => t.id === depTaskId);
                    if (!depTask) {
                        continue;
                    }

                    const depIssue = this.findIssueForTask(allIssues, depTask);
                    if (!depIssue) {
                        continue;
                    }

                    // Check if dependency link already exists
                    const linkExists = await this.checkDependencyLinkExists(taskIssue, depIssue);
                    if (linkExists) {
                        continue;
                    }

                    // Add dependency comment
                    if (this.dryRun) {
                        console.log(
                            `[DRY RUN] Would link issue #${taskIssue.number} -> #${depIssue.number}`
                        );
                        results.linked.push({ task: task.id, dependency: depTaskId, dryRun: true });
                    } else {
                        await this.createDependencyLink(taskIssue, depIssue, depTask);
                        results.linked.push({ task: task.id, dependency: depTaskId });

                        // Smart delay to respect rate limits
                        await smartDelay(1000);
                    }
                }
            } catch (error) {
                results.errors.push({
                    task: task.id,
                    error: `Failed to link dependencies: ${error.message}`
                });
            }
        }

        return results;
    }

    /**
     * Check if dependency link already exists in issue comments
     */
    async checkDependencyLinkExists(taskIssue, depIssue) {
        try {
            const comments = await executeWithRateLimit(
                () =>
                    this.octokit.rest.issues.listComments({
                        owner: this.owner,
                        repo: this.repo,
                        issue_number: taskIssue.number,
                        per_page: 100
                    }),
                `check comments for issue #${taskIssue.number}`
            );

            return comments.data.some(
                comment =>
                    comment.body &&
                    comment.body.includes(`#${depIssue.number}`) &&
                    comment.body.includes('depends on')
            );
        } catch (error) {
            console.warn(`Warning: Could not check existing dependency links: ${error.message}`);
            return false;
        }
    }

    /**
     * Create dependency link between issues
     */
    async createDependencyLink(taskIssue, depIssue, depTask) {
        const comment = `🔗 **Dependency Link**\n\nThis task depends on:\n- #${depIssue.number} (Task ${depTask.id}: ${depTask.title})\n\n*Automatically linked by GitHub Issues Automation*`;

        await executeWithRateLimit(
            () =>
                this.octokit.rest.issues.createComment({
                    owner: this.owner,
                    repo: this.repo,
                    issue_number: taskIssue.number,
                    body: comment
                }),
            `create dependency comment for issue #${taskIssue.number}`
        );
    }

    /**
     * Clean up issues for tasks that no longer exist
     */
    async cleanupObsoleteIssues(tasks) {
        const results = { cleaned: [], errors: [] };

        try {
            const allIssues = await this.getAllTaskIssues();
            const taskIds = new Set(tasks.map(task => task.id));

            for (const issue of allIssues) {
                const titleMatch = issue.title.match(/Task (\d+\.\d+):/);
                if (!titleMatch) {
                    continue;
                }

                const issueTaskId = titleMatch[1];
                if (!taskIds.has(issueTaskId)) {
                    // This issue corresponds to a task that no longer exists

                    if (this.dryRun) {
                        console.log(
                            `[DRY RUN] Would mark obsolete issue #${issue.number} for Task ${issueTaskId}`
                        );
                        results.cleaned.push({
                            issueNumber: issue.number,
                            taskId: issueTaskId,
                            dryRun: true
                        });
                    } else {
                        await this.markIssueObsolete(issue, issueTaskId);
                        results.cleaned.push({ issueNumber: issue.number, taskId: issueTaskId });
                    }
                }
            }
        } catch (error) {
            results.errors.push({
                error: `Failed to cleanup obsolete issues: ${error.message}`
            });
        }

        return results;
    }

    /**
     * Mark an issue as obsolete
     */
    async markIssueObsolete(issue, taskId) {
        // Add obsolete label
        const currentLabels = issue.labels.map(label =>
            typeof label === 'string' ? label : label.name
        );

        if (!currentLabels.includes('obsolete')) {
            await executeWithRateLimit(
                () =>
                    this.octokit.rest.issues.addLabels({
                        owner: this.owner,
                        repo: this.repo,
                        issue_number: issue.number,
                        labels: ['obsolete']
                    }),
                `add obsolete label to issue #${issue.number}`
            );
        }

        // Add comment explaining obsolescence
        const comment = `⚠️ **Task Obsolete**\n\nTask ${taskId} is no longer present in MSP430_EMULATOR_TASKS.md.\n\nThis issue has been marked as obsolete and will be closed.\n\n*Automatically detected by GitHub Issues Automation*`;

        await executeWithRateLimit(
            () =>
                this.octokit.rest.issues.createComment({
                    owner: this.owner,
                    repo: this.repo,
                    issue_number: issue.number,
                    body: comment
                }),
            `add obsolete comment to issue #${issue.number}`
        );

        // Close the issue
        await executeWithRateLimit(
            () =>
                this.octokit.rest.issues.update({
                    owner: this.owner,
                    repo: this.repo,
                    issue_number: issue.number,
                    state: 'closed'
                }),
            `close obsolete issue #${issue.number}`
        );
    }

    /**
     * Organize milestones by updating issue assignments and milestone progress
     */
    async organizeMilestones(tasks) {
        const tasksByPhase = {};

        // Group tasks by phase
        for (const task of tasks) {
            if (!tasksByPhase[task.phase]) {
                tasksByPhase[task.phase] = [];
            }
            tasksByPhase[task.phase].push(task);
        }

        // Update milestone progress
        for (const [phase, phaseTasks] of Object.entries(tasksByPhase)) {
            try {
                const milestone = await this.findOrCreateMilestone(phase);
                if (!milestone) {
                    continue;
                }

                const completedTasks = phaseTasks.filter(task => task.completed).length;
                const totalTasks = phaseTasks.length;
                const progressPercent = Math.round((completedTasks / totalTasks) * 100);

                // Update milestone description with progress
                const description = `Tasks for ${phase} of MSP430 Emulator development\n\n📊 Progress: ${completedTasks}/${totalTasks} tasks completed (${progressPercent}%)`;

                if (!this.dryRun) {
                    await executeWithRateLimit(
                        () =>
                            this.octokit.rest.issues.updateMilestone({
                                owner: this.owner,
                                repo: this.repo,
                                milestone_number: milestone.number,
                                description: description
                            }),
                        `update milestone for ${phase}`
                    );
                }
            } catch (error) {
                console.warn(`Warning: Could not update milestone for ${phase}: ${error.message}`);
            }
        }
    }

    /**
     * Get all task-related issues
     */
    async getAllTaskIssues() {
        try {
            // In dry-run mode without token, return empty array
            if (
                this.dryRun &&
                (!process.env.GITHUB_TOKEN ||
                    process.env.GITHUB_TOKEN === 'dummy-token-for-dry-run')
            ) {
                console.log(`[DRY RUN] Would fetch all task issues`);
                return [];
            }

            // Use the regular issues list API instead of deprecated search API
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
                'list all task issues',
                3
            );

            // Filter to only issues that have "Task" in the title (to match the previous search behavior)
            return issues.data.filter(
                issue => issue.title.includes('Task') && issue.title.match(/Task \d+\.\d+:/)
            );
        } catch (error) {
            console.warn(`Warning: Could not fetch task issues: ${error.message}`);
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
     * Print synchronization summary
     */
    printSummary(results) {
        console.log('\n📊 Synchronization Summary:');
        console.log(`   Created: ${results.created.length} issues`);
        console.log(`   Updated: ${results.updated.length} issues`);
        console.log(`   Closed: ${results.closed.length} issues`);
        console.log(`   Linked: ${results.linked.length} dependencies`);
        console.log(`   Cleaned: ${results.cleaned.length} obsolete issues`);
        console.log(`   Errors: ${results.errors.length} errors`);

        if (results.errors.length > 0) {
            console.log('\n❌ Errors encountered:');
            results.errors.forEach((error, index) => {
                console.log(
                    `   ${index + 1}. ${error.task ? `Task ${error.task}: ` : ''}${error.error}`
                );
            });
        }
    }
}

// Export for use as module
module.exports = { GitHubIssuesSynchronizer };

// Main function for CLI usage
async function main() {
    const token = process.env.GITHUB_TOKEN;
    const owner = process.env.GITHUB_REPOSITORY?.split('/')[0] || 'grahame-white';
    const repo = process.env.GITHUB_REPOSITORY?.split('/')[1] || 'ai_msp430_emulator';
    const tasksFile = process.argv[2] || '../../MSP430_EMULATOR_TASKS.md';
    const dryRun = process.argv.includes('--dry-run');

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

        const synchronizer = new GitHubIssuesSynchronizer(effectiveToken, owner, repo);
        if (dryRun) {
            synchronizer.enableDryRun();
            console.log('🔍 Running in DRY RUN mode - no actual changes will be made\n');
            if (!token) {
                console.log('ℹ️  No GITHUB_TOKEN provided - running in offline preview mode\n');
            }
        }

        const results = await synchronizer.synchronize(tasksFile);

        // Exit with error code if there were errors (but ignore API errors in dry-run mode without token)
        if (results.errors.length > 0) {
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
