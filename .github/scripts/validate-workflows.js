#!/usr/bin/env node

/**
 * GitHub Actions Workflow Validator
 *
 * Validates GitHub Actions workflow YAML files for common issues:
 * - YAML syntax errors
 * - Invalid permission scopes
 * - Basic GitHub Actions schema validation
 */

const fs = require('fs');
const path = require('path');
const yaml = require('js-yaml');

// Valid GitHub Actions permissions
const VALID_PERMISSIONS = [
    'actions',
    'checks',
    'contents',
    'deployments',
    'id-token',
    'issues',
    'discussions',
    'packages',
    'pages',
    'pull-requests',
    'repository-projects',
    'security-events',
    'statuses'
];

// Valid permission levels
const VALID_PERMISSION_LEVELS = ['read', 'write', 'none'];

/**
 * Find all workflow files
 * @param {string} workflowDir - Path to .github/workflows directory
 * @returns {string[]} Array of workflow file paths
 */
function findWorkflowFiles(workflowDir) {
    if (!fs.existsSync(workflowDir)) {
        console.log(`Workflow directory ${workflowDir} not found`);
        return [];
    }

    const files = fs.readdirSync(workflowDir);
    return files
        .filter(file => file.endsWith('.yml') || file.endsWith('.yaml'))
        .map(file => path.join(workflowDir, file));
}

/**
 * Validate YAML syntax
 * @param {string} filePath - Path to YAML file
 * @returns {object} Validation result with success flag and errors
 */
function validateYamlSyntax(filePath) {
    try {
        const content = fs.readFileSync(filePath, 'utf8');
        const parsed = yaml.load(content);
        return { success: true, data: parsed };
    } catch (error) {
        return {
            success: false,
            error: `YAML syntax error: ${error.message}`
        };
    }
}

/**
 * Validate GitHub Actions workflow permissions
 * @param {object} workflow - Parsed workflow object
 * @returns {string[]} Array of validation errors
 */
function validatePermissions(workflow) {
    const errors = [];

    if (!workflow.permissions) {
        return errors; // Permissions are optional
    }

    // Handle permissions as object
    if (typeof workflow.permissions === 'object' && workflow.permissions !== null) {
        for (const [permission, level] of Object.entries(workflow.permissions)) {
            // Check if permission scope is valid
            if (!VALID_PERMISSIONS.includes(permission)) {
                errors.push(`Invalid permission scope: '${permission}'. Valid scopes are: ${VALID_PERMISSIONS.join(', ')}`);
            }

            // Check if permission level is valid
            if (!VALID_PERMISSION_LEVELS.includes(level)) {
                errors.push(`Invalid permission level '${level}' for '${permission}'. Valid levels are: ${VALID_PERMISSION_LEVELS.join(', ')}`);
            }
        }
    } else if (workflow.permissions !== 'read-all' && workflow.permissions !== 'write-all') {
        errors.push('Invalid permissions format. Must be an object, \'read-all\', or \'write-all\'');
    }

    return errors;
}

/**
 * Validate basic workflow structure
 * @param {object} workflow - Parsed workflow object
 * @returns {string[]} Array of validation errors
 */
function validateWorkflowStructure(workflow) {
    const errors = [];

    // Check required fields
    if (!workflow.name) {
        errors.push('Missing required field: name');
    }

    if (!workflow.on) {
        errors.push('Missing required field: on');
    }

    if (!workflow.jobs) {
        errors.push('Missing required field: jobs');
    }

    // Validate jobs structure
    if (workflow.jobs && typeof workflow.jobs === 'object') {
        for (const [jobName, job] of Object.entries(workflow.jobs)) {
            if (!job['runs-on']) {
                errors.push(`Job '${jobName}' missing required field: runs-on`);
            }
        }
    }

    return errors;
}

/**
 * Validate a single workflow file
 * @param {string} filePath - Path to workflow file
 * @returns {object} Validation result
 */
function validateWorkflow(filePath) {
    const fileName = path.basename(filePath);
    console.log(`\n🔍 Validating workflow: ${fileName}`);

    // Check YAML syntax
    const syntaxResult = validateYamlSyntax(filePath);
    if (!syntaxResult.success) {
        console.log(`❌ ${syntaxResult.error}`);
        return { success: false, errors: [syntaxResult.error] };
    }

    const workflow = syntaxResult.data;
    const errors = [];

    // Validate basic structure
    errors.push(...validateWorkflowStructure(workflow));

    // Validate permissions
    errors.push(...validatePermissions(workflow));

    // Report results
    if (errors.length === 0) {
        console.log(`✅ Workflow ${fileName} is valid`);
        return { success: true, errors: [] };
    } else {
        console.log(`❌ Workflow ${fileName} has ${errors.length} validation error(s):`);
        errors.forEach(error => console.log(`   - ${error}`));
        return { success: false, errors };
    }
}

/**
 * Main validation function
 */
function main() {
    console.log('🚀 GitHub Actions Workflow Validator');
    console.log('=====================================');

    const workflowDir = path.join(__dirname, '..', 'workflows');
    const workflowFiles = findWorkflowFiles(workflowDir);

    if (workflowFiles.length === 0) {
        console.log('No workflow files found');
        return;
    }

    console.log(`Found ${workflowFiles.length} workflow file(s)`);

    let totalErrors = 0;
    let validWorkflows = 0;

    for (const filePath of workflowFiles) {
        const result = validateWorkflow(filePath);
        if (result.success) {
            validWorkflows++;
        } else {
            totalErrors += result.errors.length;
        }
    }

    console.log('\n📊 Validation Summary');
    console.log('=====================');
    console.log(`Total workflows: ${workflowFiles.length}`);
    console.log(`Valid workflows: ${validWorkflows}`);
    console.log(`Invalid workflows: ${workflowFiles.length - validWorkflows}`);
    console.log(`Total errors: ${totalErrors}`);

    if (totalErrors > 0) {
        console.log('\n❌ Workflow validation failed');
        process.exit(1);
    } else {
        console.log('\n✅ All workflows are valid');
    }
}

// Run if called directly
if (require.main === module) {
    main();
}

module.exports = { validateWorkflow, validatePermissions, validateWorkflowStructure };
