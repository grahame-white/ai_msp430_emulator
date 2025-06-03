#!/usr/bin/env node

/**
 * GitHub Issues Dry Run Preview
 *
 * Generates a preview of what changes would be made during synchronization
 * without actually making any API calls or modifications.
 */

const { TaskParser } = require('./parse-tasks.js');
const { GitHubIssuesSynchronizer } = require('./sync-tasks.js');

class DryRunPreview {
    constructor() {
        this.changes = {
            newIssues: [],
            updatedIssues: [],
            closedIssues: [],
            dependencies: [],
            obsoleteIssues: [],
            milestones: [],
            labels: []
        };

        // Tasks to exclude from preview (already implemented or actively being developed)
        this.excludedTasks = ['1.1', '1.2', '1.3', '1.4', '1.5'];
    }

    /**
     * Filter out excluded tasks
     */
    filterIncludedTasks(tasks) {
        return tasks.filter(task => !this.excludedTasks.includes(task.id));
    }

    /**
     * Generate comprehensive dry run preview
     */
    async generatePreview(tasksFile, token, owner, repo) {
        try {
            console.log('🔍 Generating Dry Run Preview...\n');

            // Parse tasks
            const parser = new TaskParser(tasksFile);
            const allTasks = await parser.parse();
            const tasks = this.filterIncludedTasks(allTasks);
            console.log(`Found ${allTasks.length} tasks (${allTasks.length - tasks.length} excluded, ${tasks.length} included)\n`);

            // Generate preview sections
            await this.previewTaskAnalysis(tasks);
            await this.previewLabelsAndMilestones(tasks);

            if (token) {
                // If token provided, use actual API for more accurate preview
                await this.previewWithApi(tasks, token, owner, repo);
            } else {
                // Generate basic preview without API
                await this.previewWithoutApi(tasks);
            }

            this.printPreviewSummary();
            this.generateHumanReadableReport();

            return this.changes;

        } catch (error) {
            console.error('❌ Preview generation failed:', error.message);
            throw error;
        }
    }

    /**
     * Preview task analysis
     */
    async previewTaskAnalysis(tasks) {
        console.log('📊 Task Analysis:');

        const completedTasks = tasks.filter(task => task.completed);
        const incompleteTasks = tasks.filter(task => !task.completed);
        const tasksByPhase = {};

        // Group by phase
        for (const task of tasks) {
            if (!tasksByPhase[task.phase]) {
                tasksByPhase[task.phase] = { total: 0, completed: 0 };
            }
            tasksByPhase[task.phase].total++;
            if (task.completed) {
                tasksByPhase[task.phase].completed++;
            }
        }

        console.log(`   Total tasks: ${tasks.length}`);
        console.log(`   Completed: ${completedTasks.length}`);
        console.log(`   Incomplete: ${incompleteTasks.length}`);
        console.log('\n   Phase breakdown:');

        for (const [phase, stats] of Object.entries(tasksByPhase)) {
            const percentage = Math.round((stats.completed / stats.total) * 100);
            console.log(`     ${phase}: ${stats.completed}/${stats.total} (${percentage}%)`);
        }
        console.log('');
    }

    /**
     * Preview labels and milestones that would be created
     */
    async previewLabelsAndMilestones(tasks) {
        console.log('🏷️  Labels and Milestones:');

        // Generate unique labels that would be created
        const allLabels = new Set();
        const phases = new Set();

        for (const task of tasks) {
            phases.add(task.phase);

            // Phase label
            allLabels.add(task.phase.toLowerCase().replace(' ', '-'));

            // Priority label
            allLabels.add(`priority-${task.priority.toLowerCase()}`);

            // Effort label
            if (task.effort.includes('hours')) {
                const hours = task.effort.match(/(\d+)-?(\d+)?/);
                if (hours) {
                    const minHours = parseInt(hours[1]);
                    if (minHours <= 2) {
                        allLabels.add('effort-small');
                    } else if (minHours <= 4) {
                        allLabels.add('effort-medium');
                    } else {
                        allLabels.add('effort-large');
                    }
                }
            }

            // Status label
            allLabels.add(task.completed ? 'status-completed' : 'status-pending');
        }

        this.changes.labels = Array.from(allLabels);
        this.changes.milestones = Array.from(phases);

        console.log(`   Labels to ensure exist: ${this.changes.labels.length}`);
        console.log(`     ${this.changes.labels.slice(0, 5).join(', ')}${this.changes.labels.length > 5 ? '...' : ''}`);
        console.log(`   Milestones to create: ${this.changes.milestones.length}`);
        console.log(`     ${this.changes.milestones.join(', ')}`);
        console.log('');
    }

    /**
     * Preview with API access for accurate comparison
     */
    async previewWithApi(tasks, token, owner, repo) {
        console.log('🔗 Analyzing with GitHub API...');

        const synchronizer = new GitHubIssuesSynchronizer(token, owner, repo);
        synchronizer.enableDryRun();

        const results = await synchronizer.synchronize('./MSP430_EMULATOR_TASKS.md');

        this.changes.newIssues = results.created.map(r => ({
            taskId: r.task.id,
            title: r.task.title,
            priority: r.task.priority
        }));

        this.changes.updatedIssues = results.updated.map(r => ({
            taskId: r.task.id,
            issueNumber: r.issueNumber,
            changes: r.changes
        }));

        this.changes.closedIssues = results.closed.map(r => ({
            taskId: r.task.id,
            issueNumber: r.issueNumber
        }));

        this.changes.dependencies = results.linked.map(r => ({
            taskId: r.task,
            dependsOn: r.dependency
        }));

        this.changes.obsoleteIssues = results.cleaned.map(r => ({
            issueNumber: r.issueNumber,
            taskId: r.taskId
        }));
    }

    /**
     * Preview without API access (basic analysis)
     */
    async previewWithoutApi(tasks) {
        console.log('📝 Basic Analysis (no API access):');

        const incompleteTasks = tasks.filter(task => !task.completed);

        this.changes.newIssues = incompleteTasks.map(task => ({
            taskId: task.id,
            title: task.title,
            priority: task.priority
        }));

        // Analyze dependencies
        for (const task of tasks) {
            if (task.dependencies && task.dependencies.length > 0) {
                for (const depId of task.dependencies) {
                    this.changes.dependencies.push({
                        taskId: task.id,
                        dependsOn: depId
                    });
                }
            }
        }

        console.log(`   Would potentially create: ${this.changes.newIssues.length} issues`);
        console.log(`   Dependencies to link: ${this.changes.dependencies.length} relationships`);
        console.log('   Note: Use with GITHUB_TOKEN for accurate existing issue comparison');
        console.log('');
    }

    /**
     * Print preview summary
     */
    printPreviewSummary() {
        console.log('📋 Preview Summary:');
        console.log(`   New issues to create: ${this.changes.newIssues.length}`);
        console.log(`   Existing issues to update: ${this.changes.updatedIssues.length}`);
        console.log(`   Issues to close: ${this.changes.closedIssues.length}`);
        console.log(`   Dependencies to link: ${this.changes.dependencies.length}`);
        console.log(`   Obsolete issues to clean: ${this.changes.obsoleteIssues.length}`);
        console.log('');
    }

    /**
     * Generate human-readable report
     */
    generateHumanReadableReport() {
        console.log('📄 Detailed Preview Report:');
        console.log('-'.repeat(60));

        // New issues section
        if (this.changes.newIssues.length > 0) {
            console.log('\n🆕 NEW ISSUES TO CREATE:');
            this.changes.newIssues.forEach((issue, index) => {
                console.log(`   ${index + 1}. Task ${issue.taskId}: ${issue.title}`);
                console.log(`      Priority: ${issue.priority}`);
            });
        }

        // Updated issues section
        if (this.changes.updatedIssues.length > 0) {
            console.log('\n🔄 ISSUES TO UPDATE:');
            this.changes.updatedIssues.forEach((issue, index) => {
                console.log(`   ${index + 1}. Issue #${issue.issueNumber} (Task ${issue.taskId})`);
                if (issue.changes) {
                    issue.changes.forEach(change => {
                        console.log(`      - ${change.description}`);
                    });
                }
            });
        }

        // Closed issues section
        if (this.changes.closedIssues.length > 0) {
            console.log('\n✅ ISSUES TO CLOSE:');
            this.changes.closedIssues.forEach((issue, index) => {
                console.log(`   ${index + 1}. Issue #${issue.issueNumber} (Task ${issue.taskId}) - Completed`);
            });
        }

        // Dependencies section
        if (this.changes.dependencies.length > 0) {
            console.log('\n🔗 DEPENDENCIES TO LINK:');
            this.changes.dependencies.forEach((dep, index) => {
                console.log(`   ${index + 1}. Task ${dep.taskId} depends on Task ${dep.dependsOn}`);
            });
        }

        // Obsolete issues section
        if (this.changes.obsoleteIssues.length > 0) {
            console.log('\n🗑️  OBSOLETE ISSUES TO CLEAN:');
            this.changes.obsoleteIssues.forEach((issue, index) => {
                console.log(`   ${index + 1}. Issue #${issue.issueNumber} (Task ${issue.taskId}) - No longer exists`);
            });
        }

        // Risk assessment
        console.log('\n⚠️  RISK ASSESSMENT:');
        const totalChanges = this.changes.newIssues.length +
                           this.changes.updatedIssues.length +
                           this.changes.closedIssues.length +
                           this.changes.obsoleteIssues.length;

        if (totalChanges === 0) {
            console.log('   ✅ No changes detected - safe to run');
        } else if (totalChanges <= 5) {
            console.log('   🟡 Low risk - few changes detected');
        } else if (totalChanges <= 20) {
            console.log('   🟠 Medium risk - moderate number of changes');
        } else {
            console.log('   🔴 High risk - many changes detected, review carefully');
        }

        console.log('\n💡 NEXT STEPS:');
        console.log('   1. Review the changes above carefully');
        console.log('   2. If satisfied, run the sync script without --dry-run');
        console.log('   3. Monitor the GitHub repository for proper issue creation');
        console.log('   4. Verify dependency links and milestone assignments');

        console.log('\n' + '-'.repeat(60));
    }

    /**
     * Generate JSON output for programmatic usage
     */
    generateJsonOutput() {
        return JSON.stringify(this.changes, null, 2);
    }
}

// Export for use as module
module.exports = { DryRunPreview };

// Main function for CLI usage
async function main() {
    const token = process.env.GITHUB_TOKEN;
    const owner = process.env.GITHUB_REPOSITORY?.split('/')[0] || 'grahame-white';
    const repo = process.env.GITHUB_REPOSITORY?.split('/')[1] || 'ai_msp430_emulator';
    const tasksFile = process.argv[2] || './MSP430_EMULATOR_TASKS.md';
    const jsonOutput = process.argv.includes('--json');

    try {
        const preview = new DryRunPreview();
        await preview.generatePreview(tasksFile, token, owner, repo);

        if (jsonOutput) {
            console.log(preview.generateJsonOutput());
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
