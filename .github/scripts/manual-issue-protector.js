#!/usr/bin/env node

/**
 * Manual Issue Protector
 *
 * Protects manually created GitHub issues from being modified or deleted
 * by the automation system. Identifies and marks manual issues.
 */

const { Octokit } = require('@octokit/rest');
const { isPermissionError } = require('./github-utils.js');

class ManualIssueProtector {
    constructor(token, owner, repo) {
        this.octokit = new Octokit({
            auth: token,
            userAgent: 'MSP430-Emulator-Issues-Bot v1.0.0'
        });
        this.owner = owner;
        this.repo = repo;
        this.dryRun = false;
    }

    /**
     * Enable dry run mode
     */
    enableDryRun() {
        this.dryRun = true;
    }

    /**
     * Scan and protect manual issues
     */
    async protectManualIssues() {
        const results = {
            analyzed: 0,
            manual: [],
            automated: [],
            protected: [],
            errors: []
        };

        try {
            console.log('🛡️  Scanning for manual issues to protect...');

            // Get all issues in the repository
            const allIssues = await this.getAllIssues();
            results.analyzed = allIssues.length;

            console.log(`Found ${allIssues.length} total issues to analyze`);

            // Analyze each issue
            for (const issue of allIssues) {
                try {
                    const analysis = this.analyzeIssue(issue);

                    if (analysis.isManual) {
                        results.manual.push({
                            number: issue.number,
                            title: issue.title,
                            reason: analysis.reason,
                            confidence: analysis.confidence
                        });

                        // Protect if not already protected
                        if (!analysis.isProtected) {
                            if (this.dryRun) {
                                console.log(`[DRY RUN] Would protect manual issue #${issue.number}: ${issue.title}`);
                                results.protected.push({
                                    number: issue.number,
                                    dryRun: true
                                });
                            } else {
                                await this.protectIssue(issue, analysis.reason);
                                results.protected.push({
                                    number: issue.number
                                });

                                // Small delay to respect rate limits
                                await this.delay(300);
                            }
                        }
                    } else {
                        results.automated.push({
                            number: issue.number,
                            title: issue.title,
                            confidence: analysis.confidence
                        });
                    }

                } catch (error) {
                    results.errors.push({
                        issue: issue.number,
                        error: error.message
                    });
                }
            }

            this.printProtectionSummary(results);
            return results;

        } catch (error) {
            console.error('❌ Protection scan failed:', error.message);
            results.errors.push({ error: error.message });
            return results;
        }
    }

    /**
     * Get all issues from the repository
     */
    async getAllIssues() {
        try {
            const allIssues = [];
            let page = 1;
            const perPage = 100;

            // eslint-disable-next-line no-constant-condition
            while (true) {
                const response = await this.octokit.rest.issues.listForRepo({
                    owner: this.owner,
                    repo: this.repo,
                    state: 'all',
                    page: page,
                    per_page: perPage,
                    sort: 'created',
                    direction: 'desc'
                });

                if (response.data.length === 0) {
                    break;
                }

                allIssues.push(...response.data);
                page++;

                // Avoid infinite loops
                if (page > 50) {
                    console.warn('Warning: Stopped fetching after 50 pages');
                    break;
                }
            }

            return allIssues;
        } catch (error) {
            throw new Error(`Failed to fetch issues: ${error.message}`);
        }
    }

    /**
     * Analyze an issue to determine if it's manual or automated
     */
    analyzeIssue(issue) {
        const analysis = {
            isManual: false,
            isProtected: false,
            reason: '',
            confidence: 0
        };

        // Check for automation markers
        const automationMarkers = [
            '🤖 Managed by GitHub Issues Automation',
            'Automatically generated from MSP430_EMULATOR_TASKS.md',
            'This issue was automatically generated',
            'Automatically created by'
        ];

        const hasAutomationMarker = automationMarkers.some(marker =>
            issue.body && issue.body.includes(marker)
        );

        // Check for protection markers
        const protectionMarkers = [
            'manual-issue-protected',
            '🛡️ Protected from automation',
            'MANUAL_ISSUE_DO_NOT_MODIFY'
        ];

        const hasProtectionMarker = protectionMarkers.some(marker =>
            (issue.body && issue.body.includes(marker)) ||
            (issue.labels && issue.labels.some(label =>
                (typeof label === 'string' ? label : label.name) === marker
            ))
        );

        analysis.isProtected = hasProtectionMarker;

        // Determine if manual based on various criteria
        if (hasAutomationMarker && !hasProtectionMarker) {
            // Clearly automated
            analysis.isManual = false;
            analysis.confidence = 0.95;
        } else if (hasProtectionMarker) {
            // Already protected
            analysis.isManual = true;
            analysis.isProtected = true;
            analysis.reason = 'Already marked as protected';
            analysis.confidence = 1.0;
        } else {
            // Check other indicators
            const indicators = this.getManualIndicators(issue);

            if (indicators.score >= 0.7) {
                analysis.isManual = true;
                analysis.reason = indicators.reasons.join(', ');
                analysis.confidence = indicators.score;
            } else {
                analysis.isManual = false;
                analysis.confidence = 1 - indicators.score;
            }
        }

        return analysis;
    }

    /**
     * Get indicators that suggest an issue is manually created
     */
    getManualIndicators(issue) {
        const indicators = {
            score: 0,
            reasons: []
        };

        // Check title format
        if (!issue.title.match(/^Task \d+\.\d+:/)) {
            indicators.score += 0.4;
            indicators.reasons.push('non-standard title format');
        }

        // Check creation date vs. automation start
        const createdDate = new Date(issue.created_at);
        const automationStartDate = new Date('2024-01-01'); // Adjust based on when automation started

        if (createdDate < automationStartDate) {
            indicators.score += 0.3;
            indicators.reasons.push('created before automation');
        }

        // Check author patterns
        if (issue.user && issue.user.type === 'User') {
            indicators.score += 0.2;
            indicators.reasons.push('created by human user');
        }

        // Check body structure
        if (issue.body) {
            const hasStructuredSections = issue.body.includes('## Acceptance Criteria') ||
                                        issue.body.includes('## Files to Create') ||
                                        issue.body.includes('## Testing Strategy');

            if (!hasStructuredSections) {
                indicators.score += 0.3;
                indicators.reasons.push('lacks structured sections');
            }

            // Check for typical manual content
            const manualPatterns = [
                'bug report',
                'feature request',
                'enhancement',
                'please',
                'could we',
                'would be nice',
                'I think',
                'suggestion'
            ];

            if (manualPatterns.some(pattern =>
                issue.body.toLowerCase().includes(pattern.toLowerCase())
            )) {
                indicators.score += 0.2;
                indicators.reasons.push('contains manual language patterns');
            }
        }

        // Check labels
        if (issue.labels) {
            const manualLabels = ['bug', 'enhancement', 'feature', 'question', 'help wanted'];
            const hasManualLabels = issue.labels.some(label => {
                const labelName = typeof label === 'string' ? label : label.name;
                return manualLabels.includes(labelName.toLowerCase());
            });

            if (hasManualLabels) {
                indicators.score += 0.2;
                indicators.reasons.push('has manual-typical labels');
            }

            const hasTaskLabel = issue.labels.some(label => {
                const labelName = typeof label === 'string' ? label : label.name;
                return labelName === 'task';
            });

            if (!hasTaskLabel) {
                indicators.score += 0.3;
                indicators.reasons.push('missing task label');
            }
        }

        return indicators;
    }

    /**
     * Protect a manual issue from automation
     */
    async protectIssue(issue, reason) {
        let labelSuccess = false;
        let commentSuccess = false;

        try {
            // Add protection label
            try {
                await this.octokit.rest.issues.addLabels({
                    owner: this.owner,
                    repo: this.repo,
                    issue_number: issue.number,
                    labels: ['manual-issue-protected']
                });
                labelSuccess = true;
            } catch (labelError) {
                if (isPermissionError(labelError)) {
                    console.warn(`⚠️  Cannot add protection label to issue #${issue.number}: Insufficient permissions`);
                } else {
                    console.warn(`⚠️  Failed to add protection label to issue #${issue.number}: ${labelError.message}`);
                }
            }

            // Add protection comment
            try {
                const protectionComment = `🛡️ **Manual Issue Protection Activated**\n\nThis issue has been identified as manually created and is now protected from automated modifications.\n\n**Detection Reason:** ${reason}\n\n**Protection Details:**\n- This issue will not be modified by GitHub Issues Automation\n- The automation system will preserve this issue's content and state\n- If you need to remove this protection, remove the \`manual-issue-protected\` label\n\n*Automatically protected by Manual Issue Protector*`;

                await this.octokit.rest.issues.createComment({
                    owner: this.owner,
                    repo: this.repo,
                    issue_number: issue.number,
                    body: protectionComment
                });
                commentSuccess = true;
            } catch (commentError) {
                if (isPermissionError(commentError)) {
                    console.warn(`⚠️  Cannot add protection comment to issue #${issue.number}: Insufficient permissions`);
                } else {
                    console.warn(`⚠️  Failed to add protection comment to issue #${issue.number}: ${commentError.message}`);
                }
            }

            // Report overall success
            if (labelSuccess || commentSuccess) {
                const actions = [];
                if (labelSuccess) {
                    actions.push('labeled');
                }
                if (commentSuccess) {
                    actions.push('commented');
                }
                console.log(`✅ Protected manual issue #${issue.number} (${actions.join(' + ')}): ${issue.title}`);
            } else {
                console.log(`⚠️  Issue #${issue.number} identified as manual but could not be fully protected due to permissions: ${issue.title}`);
            }

        } catch (error) {
            if (isPermissionError(error)) {
                console.warn(`⚠️  Cannot protect issue #${issue.number} due to insufficient permissions: ${issue.title}`);
            } else {
                throw new Error(`Failed to protect issue #${issue.number}: ${error.message}`);
            }
        }
    }

    /**
     * Remove protection from an issue (if needed)
     */
    async unprotectIssue(issueNumber) {
        try {
            // Remove protection label
            await this.octokit.rest.issues.removeLabel({
                owner: this.owner,
                repo: this.repo,
                issue_number: issueNumber,
                name: 'manual-issue-protected'
            });

            // Add unprotection comment
            const unprotectionComment = '🔓 **Manual Issue Protection Removed**\n\nThis issue is no longer protected from automated modifications.\n\n*Protection removed by Manual Issue Protector*';

            await this.octokit.rest.issues.createComment({
                owner: this.owner,
                repo: this.repo,
                issue_number: issueNumber,
                body: unprotectionComment
            });

            console.log(`Removed protection from issue #${issueNumber}`);

        } catch (error) {
            throw new Error(`Failed to unprotect issue #${issueNumber}: ${error.message}`);
        }
    }

    /**
     * Check if a specific issue is protected
     */
    async isIssueProtected(issueNumber) {
        try {
            const issue = await this.octokit.rest.issues.get({
                owner: this.owner,
                repo: this.repo,
                issue_number: issueNumber
            });

            return this.analyzeIssue(issue.data).isProtected;
        } catch (error) {
            throw new Error(`Failed to check protection status: ${error.message}`);
        }
    }

    /**
     * Ensure protection label exists
     */
    async ensureProtectionLabelExists() {
        try {
            await this.octokit.rest.issues.getLabel({
                owner: this.owner,
                repo: this.repo,
                name: 'manual-issue-protected'
            });
        } catch (error) {
            if (error.status === 404) {
                // Label doesn't exist, create it
                if (!this.dryRun) {
                    try {
                        await this.octokit.rest.issues.createLabel({
                            owner: this.owner,
                            repo: this.repo,
                            name: 'manual-issue-protected',
                            color: 'ff6b6b',
                            description: 'Issue is manually created and protected from automation'
                        });
                        console.log('✅ Created protection label: manual-issue-protected');
                    } catch (createError) {
                        if (isPermissionError(createError)) {
                            console.warn('⚠️  Cannot create protection label due to insufficient permissions');
                        } else {
                            console.warn(`⚠️  Could not create protection label: ${createError.message}`);
                        }
                    }
                } else {
                    console.log('[DRY RUN] Would create protection label: manual-issue-protected');
                }
            } else if (isPermissionError(error)) {
                console.warn('⚠️  Cannot access label information due to insufficient permissions');
            } else {
                console.warn(`⚠️  Error checking protection label: ${error.message}`);
            }
        }
    }

    /**
     * Print protection summary
     */
    printProtectionSummary(results) {
        console.log('\n📊 Protection Summary:');
        console.log(`   Issues analyzed: ${results.analyzed}`);
        console.log(`   Manual issues found: ${results.manual.length}`);
        console.log(`   Automated issues found: ${results.automated.length}`);
        console.log(`   Issues protected: ${results.protected.length}`);
        console.log(`   Errors: ${results.errors.length}`);

        if (results.manual.length > 0) {
            console.log('\n📝 Manual Issues:');
            results.manual.forEach((issue, index) => {
                console.log(`   ${index + 1}. #${issue.number}: ${issue.title}`);
                console.log(`      Reason: ${issue.reason}`);
                console.log(`      Confidence: ${(issue.confidence * 100).toFixed(1)}%`);
            });
        }

        if (results.errors.length > 0) {
            console.log('\n❌ Errors encountered:');
            results.errors.forEach((error, index) => {
                console.log(`   ${index + 1}. ${error.issue ? `Issue #${error.issue}: ` : ''}${error.error}`);
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
module.exports = { ManualIssueProtector };

// Main function for CLI usage
async function main() {
    const token = process.env.GITHUB_TOKEN;
    const owner = process.env.GITHUB_REPOSITORY?.split('/')[0] || 'grahame-white';
    const repo = process.env.GITHUB_REPOSITORY?.split('/')[1] || 'ai_msp430_emulator';
    const dryRun = process.argv.includes('--dry-run');
    const unprotectIssue = process.argv.find(arg => arg.startsWith('--unprotect='));
    const checkIssue = process.argv.find(arg => arg.startsWith('--check='));

    if (!token) {
        console.error('Error: GITHUB_TOKEN environment variable is required');
        process.exit(1);
    }

    try {
        const protector = new ManualIssueProtector(token, owner, repo);

        if (dryRun) {
            protector.enableDryRun();
            console.log('🔍 Running in DRY RUN mode - no actual changes will be made\n');
        }

        // Ensure protection label exists
        await protector.ensureProtectionLabelExists();

        if (unprotectIssue) {
            // Unprotect specific issue
            const issueNumber = parseInt(unprotectIssue.split('=')[1]);
            await protector.unprotectIssue(issueNumber);
        } else if (checkIssue) {
            // Check protection status of specific issue
            const issueNumber = parseInt(checkIssue.split('=')[1]);
            const isProtected = await protector.isIssueProtected(issueNumber);
            console.log(`Issue #${issueNumber} is ${isProtected ? 'protected' : 'not protected'}`);
        } else {
            // Scan and protect all manual issues
            const results = await protector.protectManualIssues();

            // Exit with error code if there were errors
            if (results.errors.length > 0) {
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
