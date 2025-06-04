#!/usr/bin/env node

/**
 * Simple test runner for MSP430 Issues automation scripts
 * Validates core functionality without requiring external dependencies
 */

const fs = require('fs');
const path = require('path');

class TestRunner {
    constructor() {
        this.tests = [];
        this.passed = 0;
        this.failed = 0;
    }

    test(name, fn) {
        this.tests.push({ name, fn });
    }

    async run() {
        console.log('🧪 Running MSP430 Issues Automation Tests\n');

        for (const { name, fn } of this.tests) {
            try {
                await fn();
                console.log(`✅ ${name}`);
                this.passed++;
            } catch (error) {
                console.log(`❌ ${name}`);
                console.log(`   Error: ${error.message}`);
                this.failed++;
            }
        }

        console.log(`\n📊 Test Results: ${this.passed} passed, ${this.failed} failed`);
        return this.failed === 0;
    }

    assert(condition, message) {
        if (!condition) {
            throw new Error(message || 'Assertion failed');
        }
    }

    assertEqual(actual, expected, message) {
        if (actual !== expected) {
            throw new Error(message || `Expected ${expected}, got ${actual}`);
        }
    }

    assertExists(filePath, message) {
        if (!fs.existsSync(filePath)) {
            throw new Error(message || `File does not exist: ${filePath}`);
        }
    }
}

// Initialize test runner
const runner = new TestRunner();

// Test: Validate script files exist
runner.test('All automation scripts exist', () => {
    const requiredScripts = [
        'parse-tasks.js',
        'create-issues.js',
        'update-issues.js',
        'sync-tasks.js',
        'dry-run.js',
        'disaster-recovery.js',
        'manual-issue-protector.js'
    ];

    for (const script of requiredScripts) {
        runner.assertExists(script, `Missing required script: ${script}`);
    }
});

// Test: Validate package.json structure
runner.test('Package.json has required configuration', () => {
    runner.assertExists('package.json');
    const pkg = JSON.parse(fs.readFileSync('package.json', 'utf8'));

    runner.assert(pkg.dependencies['@octokit/rest'], 'Missing @octokit/rest dependency');
    runner.assert(pkg.devDependencies['eslint'], 'Missing eslint dev dependency');
    runner.assert(pkg.scripts.lint, 'Missing lint script');
    runner.assert(pkg.scripts.parse, 'Missing parse script');
});

// Test: Validate TaskParser class structure
runner.test('TaskParser class is properly structured', () => {
    const parseTasksPath = path.resolve('./parse-tasks.js');
    runner.assertExists(parseTasksPath);

    const content = fs.readFileSync(parseTasksPath, 'utf8');
    runner.assert(content.includes('class TaskParser'), 'TaskParser class not found');
    runner.assert(content.includes('extractDescription'), 'extractDescription method not found');
    runner.assert(content.includes('extractDependencies'), 'extractDependencies method not found');
    runner.assert(
        content.includes('extractAcceptanceCriteria'),
        'extractAcceptanceCriteria method not found'
    );
});

// Test: Validate ESLint configuration
runner.test('ESLint configuration is valid', () => {
    runner.assertExists('.eslintrc.json');
    const eslintConfig = JSON.parse(fs.readFileSync('.eslintrc.json', 'utf8'));

    runner.assert(eslintConfig.env, 'ESLint env configuration missing');
    runner.assert(eslintConfig.rules, 'ESLint rules configuration missing');
});

// Test: Validate tasks file path resolution
runner.test('Tasks file path can be resolved', () => {
    const tasksPath = path.resolve('../../MSP430_EMULATOR_TASKS.md');
    runner.assertExists(tasksPath, 'MSP430_EMULATOR_TASKS.md not found at expected location');

    const content = fs.readFileSync(tasksPath, 'utf8');
    runner.assert(
        content.includes('MSP430 Emulator Development Task List'),
        'Tasks file content invalid'
    );
    runner.assert(content.includes('### Task'), 'No tasks found in tasks file');
});

// Test: Basic TaskParser functionality (without GitHub API)
runner.test('TaskParser can parse basic task structure', () => {
    const { TaskParser } = require('./parse-tasks.js');
    const parser = new TaskParser();

    const mockTaskContent = `### Task 1.1: Test Task
**Priority**: High
**Estimated Effort**: 2 hours
**Dependencies**: None

This is a test task description.

**Acceptance Criteria**:
- [ ] First criterion
- [x] Second criterion

**Files to Create**:
- test.js`;

    const description = parser.extractDescription(mockTaskContent);
    runner.assert(
        description.includes('This is a test task description'),
        'Description extraction failed'
    );

    const criteria = parser.extractAcceptanceCriteria(mockTaskContent);
    runner.assertEqual(criteria.length, 2, 'Should extract 2 acceptance criteria');
    runner.assertEqual(criteria[0].completed, false, 'First criterion should be incomplete');
    runner.assertEqual(criteria[1].completed, true, 'Second criterion should be complete');
});

// Test: Enhanced description extraction with fallback
runner.test('TaskParser fallback description extraction works', () => {
    const { TaskParser } = require('./parse-tasks.js');
    const parser = new TaskParser();

    // Test case without Dependencies section
    const mockTaskWithoutDeps = `### Task 1.1: Test Task
**Priority**: High
**Estimated Effort**: 2 hours

This is a description without dependencies section.

**Acceptance Criteria**:
- [ ] Test criterion`;

    const description = parser.extractDescription(mockTaskWithoutDeps);
    runner.assert(
        description.includes('This is a description without dependencies section'),
        'Fallback description extraction failed'
    );
});

// Test: Sync-tasks module structure
runner.test('sync-tasks.js has required exports', () => {
    const syncTasksPath = path.resolve('./sync-tasks.js');
    runner.assertExists(syncTasksPath);

    const content = fs.readFileSync(syncTasksPath, 'utf8');
    runner.assert(
        content.includes('class GitHubIssuesSynchronizer'),
        'GitHubIssuesSynchronizer class not found'
    );
    runner.assert(content.includes('synchronize'), 'synchronize method not found');
    runner.assert(content.includes('module.exports'), 'module.exports not found');
});

// Test: Create-issues module structure
runner.test('create-issues.js has required exports', () => {
    const createIssuesPath = path.resolve('./create-issues.js');
    runner.assertExists(createIssuesPath);

    const content = fs.readFileSync(createIssuesPath, 'utf8');
    runner.assert(
        content.includes('class GitHubIssuesCreator'),
        'GitHubIssuesCreator class not found'
    );
    runner.assert(content.includes('createIssue'), 'createIssue method not found');
    runner.assert(content.includes('module.exports'), 'module.exports not found');
});

// Test: Update-issues module structure
runner.test('update-issues.js has required exports', () => {
    const updateIssuesPath = path.resolve('./update-issues.js');
    runner.assertExists(updateIssuesPath);

    const content = fs.readFileSync(updateIssuesPath, 'utf8');
    runner.assert(
        content.includes('class GitHubIssuesUpdater'),
        'GitHubIssuesUpdater class not found'
    );
    runner.assert(content.includes('updateIssue'), 'updateIssue method not found');
    runner.assert(content.includes('module.exports'), 'module.exports not found');
});

// Test: Manual-issue-protector module structure
runner.test('manual-issue-protector.js has required exports', () => {
    const protectorPath = path.resolve('./manual-issue-protector.js');
    runner.assertExists(protectorPath);

    const content = fs.readFileSync(protectorPath, 'utf8');
    runner.assert(
        content.includes('class ManualIssueProtector'),
        'ManualIssueProtector class not found'
    );
    runner.assert(content.includes('protectManualIssues'), 'protectManualIssues method not found');
    runner.assert(content.includes('module.exports'), 'module.exports not found');
});

// Test: Disaster-recovery module structure
runner.test('disaster-recovery.js has required exports', () => {
    const recoveryPath = path.resolve('./disaster-recovery.js');
    runner.assertExists(recoveryPath);

    const content = fs.readFileSync(recoveryPath, 'utf8');
    runner.assert(content.includes('class DisasterRecovery'), 'DisasterRecovery class not found');
    runner.assert(content.includes('recover'), 'recover method not found');
    runner.assert(content.includes('module.exports'), 'module.exports not found');
});

// Test: Dry-run module structure
runner.test('dry-run.js has required exports', () => {
    const dryRunPath = path.resolve('./dry-run.js');
    runner.assertExists(dryRunPath);

    const content = fs.readFileSync(dryRunPath, 'utf8');
    runner.assert(content.includes('class DryRunPreview'), 'DryRunPreview class not found');
    runner.assert(content.includes('generatePreview'), 'generatePreview method not found');
    runner.assert(content.includes('module.exports'), 'module.exports not found');
});

// Test: Module exports are properly structured (without loading dependencies)
runner.test('All automation modules have proper exports', () => {
    const moduleFiles = [
        { name: 'parse-tasks.js', export: 'TaskParser' },
        { name: 'sync-tasks.js', export: 'GitHubIssuesSynchronizer' },
        { name: 'create-issues.js', export: 'GitHubIssuesCreator' },
        { name: 'update-issues.js', export: 'GitHubIssuesUpdater' },
        { name: 'manual-issue-protector.js', export: 'ManualIssueProtector' },
        { name: 'disaster-recovery.js', export: 'DisasterRecovery' },
        { name: 'dry-run.js', export: 'DryRunPreview' }
    ];

    for (const { name, export: exportName } of moduleFiles) {
        const content = fs.readFileSync(name, 'utf8');
        runner.assert(content.includes('module.exports'), `${name} missing module.exports`);
        runner.assert(
            content.includes(`class ${exportName}`),
            `${name} missing ${exportName} class`
        );
    }
});

// Test: Core TaskParser functionality works independently
runner.test('TaskParser can be imported and instantiated', () => {
    try {
        const { TaskParser } = require('./parse-tasks.js');
        runner.assert(
            typeof TaskParser === 'function',
            'TaskParser should be a constructor function'
        );

        const parser = new TaskParser();
        runner.assert(
            typeof parser.extractDescription === 'function',
            'extractDescription method should exist'
        );
        runner.assert(
            typeof parser.extractAcceptanceCriteria === 'function',
            'extractAcceptanceCriteria method should exist'
        );
        runner.assert(
            typeof parser.extractDependencies === 'function',
            'extractDependencies method should exist'
        );
    } catch (error) {
        throw new Error(`TaskParser import/instantiation failed: ${error.message}`);
    }
});

// Run all tests
async function main() {
    const success = await runner.run();
    process.exit(success ? 0 : 1);
}

if (require.main === module) {
    main().catch(console.error);
}

module.exports = TestRunner;
