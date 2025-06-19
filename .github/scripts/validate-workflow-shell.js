#!/usr/bin/env node

/**
 * Validates shell script snippets in GitHub Actions workflow files
 * Checks for common issues like 'local' outside functions
 */

const fs = require('fs');
const path = require('path');

function validateWorkflowShell(workflowPath) {
    const content = fs.readFileSync(workflowPath, 'utf8');
    const issues = [];

    // Split into lines for line-by-line analysis
    const lines = content.split('\n');

    for (let i = 0; i < lines.length; i++) {
        const line = lines[i];
        const lineNum = i + 1;

        // Check for 'local' keyword outside function context
        if (line.includes('local ') && !isInFunction(lines, i)) {
            issues.push({
                line: lineNum,
                issue: 'local keyword used outside function',
                detail: "'local' is only valid inside bash functions",
                suggestion: 'Remove "local" keyword for top-level variables'
            });
        }

        // Check for other common shell issues in workflows
        if (line.includes('#!/bin/bash') && !isInRunBlock(lines, i)) {
            issues.push({
                line: lineNum,
                issue: 'shebang in workflow file',
                detail: 'Shebang lines are not needed in GitHub Actions run blocks',
                suggestion: 'Remove shebang line'
            });
        }
    }

    return issues;
}

function isInFunction(lines, currentIndex) {
    // Look backwards for function definition
    for (let i = currentIndex; i >= 0; i--) {
        const line = lines[i].trim();
        if (line.includes('function ') || line.match(/^\w+\(\s*\)\s*{/)) {
            return true;
        }
        // If we hit a run: block, we're not in a function
        if (line.includes('run:') || line.includes('run: |')) {
            return false;
        }
    }
    return false;
}

function isInRunBlock(lines, currentIndex) {
    // Look backwards for run: block
    for (let i = currentIndex; i >= 0; i--) {
        const line = lines[i].trim();
        if (line.includes('run:') || line.includes('run: |')) {
            return true;
        }
        // If we hit a step boundary, we're not in a run block
        if (line.includes('- name:') && i < currentIndex) {
            return false;
        }
    }
    return false;
}

function main() {
    const workflowsDir = path.join(__dirname, '..', 'workflows');

    if (!fs.existsSync(workflowsDir)) {
        console.log('No workflows directory found');
        return;
    }

    const workflowFiles = fs
        .readdirSync(workflowsDir)
        .filter(file => file.endsWith('.yml') || file.endsWith('.yaml'))
        .map(file => path.join(workflowsDir, file));

    let totalIssues = 0;

    for (const workflowFile of workflowFiles) {
        const issues = validateWorkflowShell(workflowFile);

        if (issues.length > 0) {
            console.log(`\n❌ Issues found in ${path.basename(workflowFile)}:`);

            for (const issue of issues) {
                console.log(`  Line ${issue.line}: ${issue.issue}`);
                console.log(`    Detail: ${issue.detail}`);
                console.log(`    Suggestion: ${issue.suggestion}`);
                totalIssues++;
            }
        }
    }

    if (totalIssues === 0) {
        console.log('✅ No workflow shell issues found');
    } else {
        console.log(`\n❌ Found ${totalIssues} workflow shell issues`);
        process.exit(1);
    }
}

if (require.main === module) {
    main();
}

module.exports = { validateWorkflowShell };
