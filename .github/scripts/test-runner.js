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
    runner.assertExists('eslint.config.js');
    // For flat config, we just verify the file exists and is loadable as JavaScript
    const eslintConfigContent = fs.readFileSync('eslint.config.js', 'utf8');
    runner.assert(
        eslintConfigContent.includes('module.exports'),
        'ESLint config should export a configuration'
    );
    runner.assert(eslintConfigContent.includes('rules'), 'ESLint rules configuration missing');
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

// Test: Review and comply with acceptance criteria parsing
runner.test('TaskParser can parse "Review and comply with" acceptance criteria', () => {
    const { TaskParser } = require('./parse-tasks.js');
    const parser = new TaskParser();

    const mockTaskWithCompliance = `### Task 1.1: Test Task
**Priority**: High
**Estimated Effort**: 2 hours
**Dependencies**: None

This is a test task description.

**Acceptance Criteria**:
- [ ] Review and comply with [AI Developer Guidelines](.github/copilot-instructions.md) for comprehensive development guidance
- [ ] Review and comply with [CONTRIBUTING.md](CONTRIBUTING.md) (entire document)
- [ ] First regular criterion
- [x] Second regular criterion

**Files to Create**:
- test.js`;

    const criteria = parser.extractAcceptanceCriteria(mockTaskWithCompliance);
    runner.assertEqual(criteria.length, 4, 'Should extract 4 acceptance criteria');

    // Check compliance criteria
    runner.assert(
        criteria[0].text.includes('Review and comply with') &&
            criteria[0].text.includes('AI Developer Guidelines'),
        'First criterion should be compliance with AI Developer Guidelines'
    );
    runner.assert(
        criteria[1].text.includes('Review and comply with') &&
            criteria[1].text.includes('CONTRIBUTING.md'),
        'Second criterion should be compliance with CONTRIBUTING.md'
    );

    // Check regular criteria
    runner.assert(
        criteria[2].text === 'First regular criterion',
        'Third criterion should be regular criterion'
    );
    runner.assert(
        criteria[3].text === 'Second regular criterion',
        'Fourth criterion should be regular criterion'
    );

    // Check completion status
    runner.assertEqual(
        criteria[0].completed,
        false,
        'First compliance criterion should be incomplete'
    );
    runner.assertEqual(
        criteria[1].completed,
        false,
        'Second compliance criterion should be incomplete'
    );
    runner.assertEqual(
        criteria[2].completed,
        false,
        'First regular criterion should be incomplete'
    );
    runner.assertEqual(criteria[3].completed, true, 'Second regular criterion should be complete');
});

// Test: Task parsing no longer includes requiredReading field
runner.test('TaskParser no longer includes requiredReading field', () => {
    const { TaskParser } = require('./parse-tasks.js');
    const parser = new TaskParser();

    const mockTaskContent = `### Task 1.1: Test Task
**Priority**: High
**Estimated Effort**: 2 hours
**Dependencies**: None

This is a test task description.

**Acceptance Criteria**:
- [ ] Review and comply with [AI Developer Guidelines](.github/copilot-instructions.md)
- [ ] First criterion

**Files to Create**:
- test.js`;

    const task = parser.parseTaskSection('1.1', 'Test Task', mockTaskContent);
    runner.assert(
        !Object.prototype.hasOwnProperty.call(task, 'requiredReading'),
        'Task object should not contain requiredReading field'
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
    runner.assert(
        content.includes('createImpactAnalysis'),
        'createImpactAnalysis method not found'
    );
    runner.assert(
        content.includes('generateImpactAnalysisBody'),
        'generateImpactAnalysisBody method not found'
    );
    runner.assert(content.includes('module.exports'), 'module.exports not found');
});

// Test: Impact analysis functionality
runner.test('GitHubIssuesUpdater impact analysis functionality works', () => {
    const { GitHubIssuesUpdater } = require('./update-issues.js');

    // Create updater instance with dummy token for testing
    const updater = new GitHubIssuesUpdater('dummy-token', 'test-owner', 'test-repo');
    updater.enableDryRun();

    // Test impact analysis body generation
    const mockIssue = {
        number: 123,
        title: 'Task 1.1: Test Task'
    };

    const mockTask = {
        id: '1.1',
        title: 'Test Task',
        priority: 'High',
        effort: '2 hours',
        phase: 'Core Framework',
        dependencies: [],
        description: 'Test task description',
        acceptanceCriteria: [
            { text: 'First criterion', completed: false },
            { text: 'Second criterion', completed: true }
        ],
        completed: false
    };

    const analysisBody = updater.generateImpactAnalysisBody(mockIssue, mockTask);

    runner.assert(
        analysisBody.includes('Task Requirements Changed'),
        'Impact analysis body should include requirements change header'
    );
    runner.assert(
        analysisBody.includes('**Original Issue**: #123'),
        'Impact analysis body should reference original issue'
    );
    runner.assert(
        analysisBody.includes('**Task**: 1.1'),
        'Impact analysis body should include task ID'
    );
    runner.assert(
        analysisBody.includes('Completed → Incomplete'),
        'Impact analysis body should show status change'
    );
    runner.assert(
        analysisBody.includes('Impact Analysis Checklist'),
        'Impact analysis body should include checklist'
    );
    runner.assert(
        analysisBody.includes('Review changes to task requirements'),
        'Impact analysis body should include review step'
    );
});

// Test: Dry-run mode works without GitHub token
runner.test('update-issues.js dry-run mode works without GitHub token', async () => {
    // Store original token
    const originalToken = process.env.GITHUB_TOKEN;

    try {
        // Temporarily remove GitHub token
        delete process.env.GITHUB_TOKEN;

        const { GitHubIssuesUpdater } = require('./update-issues.js');
        const updater = new GitHubIssuesUpdater('dummy-token', 'test-owner', 'test-repo');
        updater.enableDryRun();

        // Test that getAllTaskIssues works in dry-run mode
        const issues = await updater.getAllTaskIssues();
        runner.assert(
            Array.isArray(issues),
            'getAllTaskIssues should return an array in dry-run mode'
        );
        runner.assertEqual(
            issues.length,
            0,
            'getAllTaskIssues should return empty array in dry-run mode without token'
        );
    } finally {
        // Restore original token
        if (originalToken) {
            process.env.GITHUB_TOKEN = originalToken;
        }
    }
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

// Test: TASK_ID_PATTERNS regex patterns work correctly
runner.test('TASK_ID_PATTERNS supports both 2-part and 3-part task IDs', () => {
    const { TASK_ID_PATTERNS } = require('./config.js');

    // Test 2-part task IDs
    const twoPartTests = [
        { input: 'Task 1.1: Some Title', expected: '1.1' },
        { input: 'Task 5.5: Another Title', expected: '5.5' },
        { input: 'Task 10.15: Complex Title', expected: '10.15' }
    ];

    for (const { input, expected } of twoPartTests) {
        const match = input.match(TASK_ID_PATTERNS.ISSUE_TITLE);
        runner.assert(match, `Should match 2-part task ID in: ${input}`);
        runner.assertEqual(
            match[1],
            expected,
            `Should extract correct 2-part task ID from: ${input}`
        );
    }

    // Test 3-part task IDs (the main fix)
    const threePartTests = [
        { input: 'Task 5.5.2: Some Title', expected: '5.5.2' },
        { input: 'Task 5.5.3: Another Title', expected: '5.5.3' },
        { input: 'Task 1.2.10: Complex Title', expected: '1.2.10' }
    ];

    for (const { input, expected } of threePartTests) {
        const match = input.match(TASK_ID_PATTERNS.ISSUE_TITLE);
        runner.assert(match, `Should match 3-part task ID in: ${input}`);
        runner.assertEqual(
            match[1],
            expected,
            `Should extract correct 3-part task ID from: ${input}`
        );
    }

    // Test strict validation
    runner.assert(
        TASK_ID_PATTERNS.ISSUE_TITLE_STRICT.test('Task 1.1: Title'),
        'Should validate 2-part task title'
    );
    runner.assert(
        TASK_ID_PATTERNS.ISSUE_TITLE_STRICT.test('Task 5.5.2: Title'),
        'Should validate 3-part task title'
    );
    runner.assert(
        !TASK_ID_PATTERNS.ISSUE_TITLE_STRICT.test('Not a task title'),
        'Should reject non-task titles'
    );
});

// Test: TASK_UTILS utility functions work correctly
runner.test('TASK_UTILS functions handle both 2-part and 3-part task IDs', () => {
    const { TASK_UTILS } = require('./config.js');

    // Test extractTaskIdFromTitle
    runner.assertEqual(
        TASK_UTILS.extractTaskIdFromTitle('Task 1.1: Some Title'),
        '1.1',
        'Should extract 2-part task ID'
    );
    runner.assertEqual(
        TASK_UTILS.extractTaskIdFromTitle('Task 5.5.2: Some Title'),
        '5.5.2',
        'Should extract 3-part task ID'
    );
    runner.assertEqual(
        TASK_UTILS.extractTaskIdFromTitle('Not a task title'),
        null,
        'Should return null for non-task titles'
    );

    // Test isValidTaskId
    runner.assert(TASK_UTILS.isValidTaskId('1.1'), 'Should validate 2-part task ID');
    runner.assert(TASK_UTILS.isValidTaskId('5.5.2'), 'Should validate 3-part task ID');
    runner.assert(!TASK_UTILS.isValidTaskId('1'), 'Should reject single number');
    runner.assert(!TASK_UTILS.isValidTaskId('1.1.1.1'), 'Should reject 4-part task ID');
    runner.assert(!TASK_UTILS.isValidTaskId('abc.def'), 'Should reject non-numeric task ID');

    // Test isTaskIssueTitle
    runner.assert(
        TASK_UTILS.isTaskIssueTitle('Task 1.1: Title'),
        'Should recognize 2-part task issue title'
    );
    runner.assert(
        TASK_UTILS.isTaskIssueTitle('Task 5.5.2: Title'),
        'Should recognize 3-part task issue title'
    );
    runner.assert(
        !TASK_UTILS.isTaskIssueTitle('Some other title'),
        'Should reject non-task titles'
    );

    // Test formatIssueTitle
    runner.assertEqual(
        TASK_UTILS.formatIssueTitle('1.1', 'Test Task'),
        'Task 1.1: Test Task',
        'Should format 2-part task issue title'
    );
    runner.assertEqual(
        TASK_UTILS.formatIssueTitle('5.5.2', 'Test Task'),
        'Task 5.5.2: Test Task',
        'Should format 3-part task issue title'
    );
});

// Test: TASK_UTILS issueMatchesTaskId function (core duplicate prevention logic)
runner.test('TASK_UTILS.issueMatchesTaskId prevents duplicate issues', () => {
    const { TASK_UTILS } = require('./config.js');

    // Mock GitHub issue objects
    const issue2Part = { title: 'Task 1.1: Some Task' };
    const issue3Part = { title: 'Task 5.5.2: Some Task' };
    const nonTaskIssue = { title: 'Bug: Some Bug Report' };

    // Test matching
    runner.assert(TASK_UTILS.issueMatchesTaskId(issue2Part, '1.1'), 'Should match 2-part task ID');
    runner.assert(
        TASK_UTILS.issueMatchesTaskId(issue3Part, '5.5.2'),
        'Should match 3-part task ID'
    );

    // Test non-matching
    runner.assert(
        !TASK_UTILS.issueMatchesTaskId(issue2Part, '1.2'),
        'Should not match different 2-part task ID'
    );
    runner.assert(
        !TASK_UTILS.issueMatchesTaskId(issue3Part, '5.5.3'),
        'Should not match different 3-part task ID'
    );
    runner.assert(
        !TASK_UTILS.issueMatchesTaskId(nonTaskIssue, '1.1'),
        'Should not match non-task issues'
    );

    // Test the specific case from the bug report
    runner.assert(
        TASK_UTILS.issueMatchesTaskId({ title: 'Task 5.5.2: Implement X' }, '5.5.2'),
        'Should match the problematic 3-part task ID 5.5.2'
    );
    runner.assert(
        TASK_UTILS.issueMatchesTaskId({ title: 'Task 5.5.3: Implement Y' }, '5.5.3'),
        'Should match the problematic 3-part task ID 5.5.3'
    );
});

// Test: TASK_UTILS parseTaskId function handles complex parsing
runner.test('TASK_UTILS.parseTaskId correctly parses and validates task IDs', () => {
    const { TASK_UTILS } = require('./config.js');

    // Test 2-part task ID parsing
    const parsed2Part = TASK_UTILS.parseTaskId('1.1');
    runner.assert(parsed2Part.valid, '2-part task ID should be valid');
    runner.assertEqual(parsed2Part.major, 1, 'Should parse major version correctly');
    runner.assertEqual(parsed2Part.minor, 1, 'Should parse minor version correctly');
    runner.assertEqual(parsed2Part.patch, null, 'Should have null patch for 2-part ID');
    runner.assert(!parsed2Part.is3Part, 'Should not be marked as 3-part');

    // Test 3-part task ID parsing
    const parsed3Part = TASK_UTILS.parseTaskId('5.5.2');
    runner.assert(parsed3Part.valid, '3-part task ID should be valid');
    runner.assertEqual(parsed3Part.major, 5, 'Should parse major version correctly');
    runner.assertEqual(parsed3Part.minor, 5, 'Should parse minor version correctly');
    runner.assertEqual(parsed3Part.patch, 2, 'Should parse patch version correctly');
    runner.assert(parsed3Part.is3Part, 'Should be marked as 3-part');

    // Test invalid task ID
    const parsedInvalid = TASK_UTILS.parseTaskId('invalid');
    runner.assert(!parsedInvalid.valid, 'Invalid task ID should be marked as invalid');
    runner.assertEqual(parsedInvalid.major, null, 'Invalid ID should have null major');
    runner.assertEqual(parsedInvalid.minor, null, 'Invalid ID should have null minor');
    runner.assertEqual(parsedInvalid.patch, null, 'Invalid ID should have null patch');
});

// Test: TASK_UTILS compareTaskIds function for sorting
runner.test('TASK_UTILS.compareTaskIds correctly sorts task IDs', () => {
    const { TASK_UTILS } = require('./config.js');

    // Test 2-part comparisons
    runner.assert(TASK_UTILS.compareTaskIds('1.1', '1.2') < 0, '1.1 should come before 1.2');
    runner.assert(TASK_UTILS.compareTaskIds('1.2', '1.1') > 0, '1.2 should come after 1.1');
    runner.assert(TASK_UTILS.compareTaskIds('1.1', '1.1') === 0, '1.1 should equal 1.1');

    // Test 3-part comparisons
    runner.assert(
        TASK_UTILS.compareTaskIds('5.5.2', '5.5.3') < 0,
        '5.5.2 should come before 5.5.3'
    );
    runner.assert(TASK_UTILS.compareTaskIds('5.5.3', '5.5.2') > 0, '5.5.3 should come after 5.5.2');

    // Test mixed 2-part and 3-part (treating missing patch as 0)
    runner.assert(TASK_UTILS.compareTaskIds('5.5', '5.5.1') < 0, '5.5 should come before 5.5.1');
    runner.assert(TASK_UTILS.compareTaskIds('5.5.1', '5.5') > 0, '5.5.1 should come after 5.5');
    runner.assert(TASK_UTILS.compareTaskIds('5.5', '5.5.0') === 0, '5.5 should equal 5.5.0');
});

// Test: TASK_UTILS extractTaskIdsFromIssues function
runner.test('TASK_UTILS.extractTaskIdsFromIssues extracts all task IDs', () => {
    const { TASK_UTILS } = require('./config.js');

    const mockIssues = [
        { title: 'Task 1.1: First Task' },
        { title: 'Task 5.5.2: Second Task' },
        { title: 'Bug: Not a task' },
        { title: 'Task 5.5.3: Third Task' },
        { title: 'Feature request: Also not a task' }
    ];

    const extractedIds = TASK_UTILS.extractTaskIdsFromIssues(mockIssues);
    runner.assertEqual(extractedIds.length, 3, 'Should extract 3 task IDs');
    runner.assert(extractedIds.includes('1.1'), 'Should include 2-part task ID');
    runner.assert(extractedIds.includes('5.5.2'), 'Should include first 3-part task ID');
    runner.assert(extractedIds.includes('5.5.3'), 'Should include second 3-part task ID');
    runner.assert(!extractedIds.includes(null), 'Should not include null values');
});

// Test: Edge cases and error conditions for regex patterns
runner.test('TASK_ID_PATTERNS handles edge cases correctly', () => {
    const { TASK_ID_PATTERNS } = require('./config.js');

    // Test edge cases that should NOT match with ISSUE_TITLE
    const negativeTests = [
        'Task 1: Missing minor version',
        'Task 1.1.1.1: Too many parts',
        'Task a.b: Non-numeric',
        'task 1.1: Lowercase task (case sensitive)', // Note: ISSUE_TITLE is case sensitive
        'Task 1.1 Missing colon'
    ];

    for (const input of negativeTests) {
        const match = input.match(TASK_ID_PATTERNS.ISSUE_TITLE);
        runner.assert(!match, `Should NOT match edge case: "${input}"`);
    }

    // Test boundary conditions that SHOULD match with ISSUE_TITLE
    const positiveTests = [
        { input: 'Task 0.0: Zero values', expected: '0.0' },
        { input: 'Task 99.99: Large numbers', expected: '99.99' },
        { input: 'Task 1.1.0: Zero patch', expected: '1.1.0' },
        { input: 'Task 10.20.30: Multiple digits', expected: '10.20.30' },
        { input: 'Pre Task 1.1: Prefix', expected: '1.1' } // ISSUE_TITLE matches anywhere
    ];

    for (const { input, expected } of positiveTests) {
        const match = input.match(TASK_ID_PATTERNS.ISSUE_TITLE);
        runner.assert(match, `Should match valid case: "${input}"`);
        runner.assertEqual(match[1], expected, `Should extract correct ID from: "${input}"`);
    }

    // Test ISSUE_TITLE_STRICT for validation (should only match at start)
    runner.assert(
        TASK_ID_PATTERNS.ISSUE_TITLE_STRICT.test('Task 1.1: Valid'),
        'STRICT should match at start of line'
    );
    runner.assert(
        !TASK_ID_PATTERNS.ISSUE_TITLE_STRICT.test('Pre Task 1.1: Invalid'),
        'STRICT should NOT match with prefix'
    );
    runner.assert(
        !TASK_ID_PATTERNS.ISSUE_TITLE_STRICT.test(''),
        'STRICT should NOT match empty string'
    );
});

// Test: Scenario-based testing for the original duplicate issue problem
runner.test('Duplicate issue prevention scenario (original bug reproduction)', () => {
    const { TASK_UTILS } = require('./config.js');

    // Simulate the original problem scenario
    // Before fix: Task 5.5.2 would create multiple issues #150, #147, #140
    // After fix: Should correctly identify existing issues

    const existingIssues = [
        { number: 150, title: 'Task 5.5.2: Implement Feature X' },
        { number: 147, title: 'Task 5.5.2: Implement Feature X' }, // duplicate
        { number: 140, title: 'Task 5.5.2: Implement Feature X' }, // duplicate
        { number: 151, title: 'Task 5.5.3: Implement Feature Y' },
        { number: 148, title: 'Task 5.5.3: Implement Feature Y' }, // duplicate
        { number: 141, title: 'Task 5.5.3: Implement Feature Y' }, // duplicate
        { number: 100, title: 'Task 1.1: Basic 2-part task' },
        { number: 99, title: 'Bug: Not a task issue' }
    ];

    // Test that we can correctly identify all existing issues for task 5.5.2
    const task552Issues = existingIssues.filter(issue =>
        TASK_UTILS.issueMatchesTaskId(issue, '5.5.2')
    );
    runner.assertEqual(task552Issues.length, 3, 'Should find all 3 existing issues for task 5.5.2');

    // Test that we can correctly identify all existing issues for task 5.5.3
    const task553Issues = existingIssues.filter(issue =>
        TASK_UTILS.issueMatchesTaskId(issue, '5.5.3')
    );
    runner.assertEqual(task553Issues.length, 3, 'Should find all 3 existing issues for task 5.5.3');

    // Test that 2-part tasks still work
    const task11Issues = existingIssues.filter(issue =>
        TASK_UTILS.issueMatchesTaskId(issue, '1.1')
    );
    runner.assertEqual(task11Issues.length, 1, 'Should find 1 existing issue for task 1.1');

    // Test that non-existent tasks return empty
    const noIssues = existingIssues.filter(issue => TASK_UTILS.issueMatchesTaskId(issue, '99.99'));
    runner.assertEqual(noIssues.length, 0, 'Should find no issues for non-existent task');
});

// Test: lint-with-annotations.sh script structure and functionality
runner.test('lint-with-annotations.sh has required structure', () => {
    const scriptPath = path.resolve('./lint-with-annotations.sh');
    runner.assertExists(scriptPath);
    
    const content = fs.readFileSync(scriptPath, 'utf8');
    
    // Test that it sources github-annotations helper
    runner.assert(
        content.includes('source "../../script/github-annotations"'),
        'Should source github-annotations helper script'
    );
    
    // Test that it has automation-specific processor functions
    runner.assert(
        content.includes('process_automation_eslint_annotations'),
        'Should have process_automation_eslint_annotations function'
    );
    runner.assert(
        content.includes('process_automation_prettier_annotations'),
        'Should have process_automation_prettier_annotations function'
    );
    
    // Test that it checks for GitHub Actions environment
    runner.assert(
        content.includes('is_github_actions'),
        'Should check GitHub Actions environment'
    );
    
    // Test that it handles npm commands properly
    runner.assert(
        content.includes('npm run lint:json'),
        'Should run ESLint JSON output'
    );
    runner.assert(
        content.includes('npm run format:check'),
        'Should run Prettier format check'
    );
    runner.assert(
        content.includes('npm run lint:yaml'),
        'Should run YAML linting'
    );
});

// Test: Annotation script path handling
runner.test('lint-with-annotations.sh handles path processing correctly', () => {
    const scriptPath = path.resolve('./lint-with-annotations.sh');
    const content = fs.readFileSync(scriptPath, 'utf8');
    
    // Test that it converts to relative paths with .github/scripts prefix
    runner.assert(
        content.includes('.github/scripts/$relative_file'),
        'Should add .github/scripts prefix to relative file paths'
    );
    
    // Test that it removes path prefixes correctly
    runner.assert(
        content.includes('relative_file="${relative_file#../../}"'),
        'Should remove relative path prefix'
    );
});

// Test: Annotation test script exists and is executable
runner.test('Annotation test script exists and is functional', () => {
    const testScriptPath = path.resolve('../../script/test-annotations');
    runner.assertExists(testScriptPath, 'Annotation test script should exist');
    
    // Check if it's executable (on Unix systems)
    try {
        const stats = fs.statSync(testScriptPath);
        // Check if the owner has execute permission (simplified check)
        runner.assert(
            (stats.mode & parseInt('100', 8)) !== 0,
            'Annotation test script should be executable'
        );
    } catch (error) {
        // If we can't check permissions, at least verify the file exists
        runner.assertExists(testScriptPath);
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
