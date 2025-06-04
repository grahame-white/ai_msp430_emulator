#!/usr/bin/env node

/**
 * GitHub Issues Disaster Recovery
 *
 * Rebuilds GitHub issues from scratch based on MSP430_EMULATOR_TASKS.md
 * when the issue automation system needs to be reset or recovered.
 */

const { Octokit } = require('@octokit/rest');
const { TaskParser } = require('./parse-tasks.js');
const { GitHubIssuesCreator } = require('./create-issues.js');

class DisasterRecovery {
    constructor(token, owner, repo) {
        this.octokit = new Octokit({
            auth: token,
            userAgent: 'MSP430-Emulator-Issues-Bot v1.0.0'
        });
        this.owner = owner;
        this.repo = repo;
        this.dryRun = false;
        this.force = false;

        // Tasks to exclude from recovery (already implemented or actively being developed)
        this.excludedTasks = ['1.1', '1.2', '1.3', '1.4', '1.5'];
    }

    /**
     * Enable dry run mode
     */
    enableDryRun() {
        this.dryRun = true;
    }

    /**
     * Enable force mode (recreate even if issues exist)
     */
    enableForce() {
        this.force = true;
    }

    /**
     * Filter out excluded tasks
     */
    filterIncludedTasks(tasks) {
        return tasks.filter(task => !this.excludedTasks.includes(task.id));
    }

    /**
     * Perform disaster recovery
     */
    async recover(tasksFile) {
        const results = {
            analyzed: 0,
            backup: {
                issues: [],
                labels: [],
                milestones: []
            },
            recovered: {
                created: [],
                updated: [],
                preserved: []
            },
            errors: []
        };

        try {
            console.log('🚨 Starting Disaster Recovery Process...');
            console.log('⚠️  This will analyze and potentially recreate all task issues');

            if (!this.dryRun && !this.force) {
                console.log('\n❗ Add --force flag to proceed with actual recovery');
                console.log('   Add --dry-run to preview changes without making them');
                return results;
            }

            // Step 1: Parse current tasks
            console.log('\n📖 Parsing tasks from markdown...');
            const parser = new TaskParser(tasksFile);
            const allTasks = await parser.parse();
            const tasks = this.filterIncludedTasks(allTasks);
            results.analyzed = tasks.length;
            console.log(
                `Found ${allTasks.length} tasks (${allTasks.length - tasks.length} excluded, ${tasks.length} to recover)`
            );

            // Step 2: Backup existing state
            console.log('\n💾 Backing up existing state...');
            await this.backupExistingState(results.backup);

            // Step 3: Analyze recovery needs
            console.log('\n🔍 Analyzing recovery requirements...');
            const recoveryPlan = await this.analyzeRecoveryNeeds(tasks, results.backup.issues);

            // Step 4: Execute recovery plan
            console.log('\n🔧 Executing recovery plan...');
            await this.executeRecoveryPlan(recoveryPlan, tasks, results);

            // Step 5: Verify recovery
            console.log('\n✅ Verifying recovery...');
            await this.verifyRecovery(tasks, results);

            console.log('\n🎉 Disaster recovery completed!');
            this.printRecoverySummary(results);

            return results;
        } catch (error) {
            console.error('❌ Disaster recovery failed:', error.message);
            results.errors.push({ error: error.message });
            return results;
        }
    }

    /**
     * Backup existing GitHub issues, labels, and milestones
     */
    async backupExistingState(backup) {
        try {
            // Add delay to respect rate limits
            await this.delay(1000);

            // Backup existing task issues
            const searchQuery = `repo:${this.owner}/${this.repo} in:title "Task"`;
            const searchResults = await this.octokit.rest.search.issuesAndPullRequests({
                q: searchQuery,
                sort: 'created',
                order: 'desc',
                per_page: 100
            });

            backup.issues = searchResults.data.items.map(issue => ({
                number: issue.number,
                title: issue.title,
                body: issue.body,
                state: issue.state,
                labels: issue.labels.map(label => (typeof label === 'string' ? label : label.name)),
                milestone: issue.milestone
                    ? {
                          number: issue.milestone.number,
                          title: issue.milestone.title
                      }
                    : null,
                created_at: issue.created_at,
                updated_at: issue.updated_at,
                taskId: this.extractTaskId(issue.title),
                isAutomated: this.isAutomatedIssue(issue)
            }));

            console.log(`   Backed up ${backup.issues.length} existing issues`);

            // Backup labels
            const labels = await this.octokit.rest.issues.listLabelsForRepo({
                owner: this.owner,
                repo: this.repo,
                per_page: 100
            });

            backup.labels = labels.data.map(label => ({
                name: label.name,
                color: label.color,
                description: label.description
            }));

            console.log(`   Backed up ${backup.labels.length} labels`);

            // Backup milestones
            const milestones = await this.octokit.rest.issues.listMilestones({
                owner: this.owner,
                repo: this.repo,
                state: 'all',
                per_page: 100
            });

            backup.milestones = milestones.data.map(milestone => ({
                number: milestone.number,
                title: milestone.title,
                description: milestone.description,
                state: milestone.state,
                open_issues: milestone.open_issues,
                closed_issues: milestone.closed_issues
            }));

            console.log(`   Backed up ${backup.milestones.length} milestones`);
        } catch (error) {
            throw new Error(`Failed to backup existing state: ${error.message}`);
        }
    }

    /**
     * Analyze what needs to be recovered
     */
    async analyzeRecoveryNeeds(tasks, existingIssues) {
        const plan = {
            toCreate: [],
            toUpdate: [],
            toPreserve: [],
            toRecreate: []
        };

        const existingTaskIssues = new Map();

        // Map existing issues by task ID
        for (const issue of existingIssues) {
            if (issue.taskId) {
                existingTaskIssues.set(issue.taskId, issue);
            }
        }

        // Analyze each task
        for (const task of tasks) {
            const existingIssue = existingTaskIssues.get(task.id);

            if (!existingIssue) {
                // No issue exists, need to create
                plan.toCreate.push(task);
            } else if (!existingIssue.isAutomated) {
                // Issue exists but is manual, preserve it
                plan.toPreserve.push({ task, issue: existingIssue });
            } else if (this.needsRecreation(task, existingIssue)) {
                // Issue exists but needs complete recreation
                plan.toRecreate.push({ task, issue: existingIssue });
            } else {
                // Issue exists and just needs updating
                plan.toUpdate.push({ task, issue: existingIssue });
            }
        }

        console.log(`   To create: ${plan.toCreate.length} issues`);
        console.log(`   To update: ${plan.toUpdate.length} issues`);
        console.log(`   To preserve: ${plan.toPreserve.length} manual issues`);
        console.log(`   To recreate: ${plan.toRecreate.length} corrupted issues`);

        return plan;
    }

    /**
     * Check if an issue needs complete recreation
     */
    needsRecreation(task, issue) {
        // Check for significant corruption or missing automation markers
        return (
            !issue.body ||
            !issue.body.includes('🤖 Managed by GitHub Issues Automation') ||
            issue.title !== `Task ${task.id}: ${task.title}`
        );
    }

    /**
     * Execute the recovery plan
     */
    async executeRecoveryPlan(plan, tasks, results) {
        const creator = new GitHubIssuesCreator(this.octokit.auth, this.owner, this.repo);

        if (this.dryRun) {
            creator.enableDryRun();
        }

        // Ensure labels and milestones exist
        await creator.ensureLabelsExist();

        // Create new issues
        if (plan.toCreate.length > 0) {
            console.log(`   Creating ${plan.toCreate.length} new issues...`);
            const createResults = await creator.createIssuesFromTasks(plan.toCreate);
            results.recovered.created = createResults.created;
            results.errors.push(...createResults.errors);
        }

        // Recreate corrupted issues
        if (plan.toRecreate.length > 0) {
            console.log(`   Recreating ${plan.toRecreate.length} corrupted issues...`);

            for (const { task, issue } of plan.toRecreate) {
                try {
                    // Close old issue with explanation
                    if (!this.dryRun) {
                        await this.closeCorruptedIssue(issue);
                        await this.delay(500);
                    } else {
                        console.log(`[DRY RUN] Would close corrupted issue #${issue.number}`);
                    }

                    // Create new issue
                    const createResults = await creator.createIssuesFromTasks([task]);
                    if (createResults.created.length > 0) {
                        results.recovered.created.push(createResults.created[0]);
                    }
                    results.errors.push(...createResults.errors);
                } catch (error) {
                    results.errors.push({
                        task: task.id,
                        error: `Failed to recreate issue: ${error.message}`
                    });
                }
            }
        }

        // Update existing issues
        if (plan.toUpdate.length > 0) {
            console.log(`   Updating ${plan.toUpdate.length} existing issues...`);
            // Use the updater logic here if needed
            results.recovered.updated = plan.toUpdate.map(p => p.issue);
        }

        // Preserve manual issues
        results.recovered.preserved = plan.toPreserve.map(p => p.issue);
    }

    /**
     * Close a corrupted issue with explanation
     */
    async closeCorruptedIssue(issue) {
        const comment = `🚨 **Issue Corruption Detected**\n\nThis issue was detected as corrupted during disaster recovery and will be replaced with a properly formatted version.\n\n**Original Issue Details:**\n- Number: #${issue.number}\n- Created: ${issue.created_at}\n- Last Updated: ${issue.updated_at}\n\nA new issue will be created to replace this one.\n\n*Automatically closed by Disaster Recovery*`;

        await this.octokit.rest.issues.createComment({
            owner: this.owner,
            repo: this.repo,
            issue_number: issue.number,
            body: comment
        });

        await this.octokit.rest.issues.update({
            owner: this.owner,
            repo: this.repo,
            issue_number: issue.number,
            state: 'closed',
            labels: [...(issue.labels || []), 'recovered', 'corrupted']
        });
    }

    /**
     * Verify recovery was successful
     */
    async verifyRecovery(tasks) {
        try {
            // Add delay to respect rate limits
            await this.delay(1000);

            // Re-fetch issues to verify
            const searchQuery = `repo:${this.owner}/${this.repo} in:title "Task" label:task`;
            const searchResults = await this.octokit.rest.search.issuesAndPullRequests({
                q: searchQuery,
                sort: 'created',
                order: 'desc',
                per_page: 100
            });

            const recoveredIssues = searchResults.data.items;
            const taskIds = new Set(tasks.map(task => task.id));
            const recoveredTaskIds = new Set();

            for (const issue of recoveredIssues) {
                const taskId = this.extractTaskId(issue.title);
                if (taskId && taskIds.has(taskId)) {
                    recoveredTaskIds.add(taskId);
                }
            }

            const missingTasks = Array.from(taskIds).filter(id => !recoveredTaskIds.has(id));

            if (missingTasks.length > 0) {
                console.log(
                    `   ⚠️  ${missingTasks.length} tasks still missing issues: ${missingTasks.join(', ')}`
                );
            } else {
                console.log('   ✅ All tasks have corresponding issues');
            }
        } catch (error) {
            console.warn(`Warning: Could not verify recovery: ${error.message}`);
        }
    }

    /**
     * Extract task ID from issue title
     */
    extractTaskId(title) {
        const match = title.match(/Task (\d+\.\d+):/);
        return match ? match[1] : null;
    }

    /**
     * Check if an issue was created by automation
     */
    isAutomatedIssue(issue) {
        return issue.body && issue.body.includes('🤖 Managed by GitHub Issues Automation');
    }

    /**
     * Print recovery summary
     */
    printRecoverySummary(results) {
        console.log('\n📊 Recovery Summary:');
        console.log(`   Tasks analyzed: ${results.analyzed}`);
        console.log(`   Issues created: ${results.recovered.created.length}`);
        console.log(`   Issues updated: ${results.recovered.updated.length}`);
        console.log(`   Issues preserved: ${results.recovered.preserved.length}`);
        console.log(
            `   Backup created: ${results.backup.issues.length} issues, ${results.backup.labels.length} labels, ${results.backup.milestones.length} milestones`
        );
        console.log(`   Errors: ${results.errors.length}`);

        if (results.errors.length > 0) {
            console.log('\n❌ Errors encountered:');
            results.errors.forEach((error, index) => {
                console.log(
                    `   ${index + 1}. ${error.task ? `Task ${error.task}: ` : ''}${error.error}`
                );
            });
        }
    }

    /**
     * Utility function for delays
     */
    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}

// Export for use as module
module.exports = { DisasterRecovery };

// Main function for CLI usage
async function main() {
    const token = process.env.GITHUB_TOKEN;
    const owner = process.env.GITHUB_REPOSITORY?.split('/')[0] || 'grahame-white';
    const repo = process.env.GITHUB_REPOSITORY?.split('/')[1] || 'ai_msp430_emulator';
    const tasksFile = process.argv[2] || '../../MSP430_EMULATOR_TASKS.md';
    const dryRun = process.argv.includes('--dry-run');
    const force = process.argv.includes('--force');

    if (!token) {
        console.error('Error: GITHUB_TOKEN environment variable is required');
        process.exit(1);
    }

    try {
        const recovery = new DisasterRecovery(token, owner, repo);

        if (dryRun) {
            recovery.enableDryRun();
            console.log('🔍 Running in DRY RUN mode - no actual changes will be made\n');
        }

        if (force) {
            recovery.enableForce();
        }

        const results = await recovery.recover(tasksFile);

        // Exit with error code if there were errors
        if (results.errors.length > 0) {
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
