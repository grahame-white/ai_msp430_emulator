/**
 * Shared configuration constants for GitHub automation scripts
 * 
 * This file provides centralized configuration for all GitHub issues automation,
 * including task ID patterns, utility functions, and shared constants.
 * 
 * IMPORTANT: All task ID regex patterns are centralized here to ensure consistency
 * across all automation scripts. When modifying task ID formats, only update
 * the patterns in this file.
 */

// Bot configuration
const BOT_USER_AGENT = 'MSP430-Emulator-Issues-Bot v1.0.0';

// Tasks to exclude from automation (already implemented or actively being developed)
const EXCLUDED_TASKS = ['1.1', '1.2', '1.3', '1.4', '1.5'];

// Task ID regex patterns - centralized for consistency across all scripts
const TASK_ID_PATTERNS = {
    // Matches task IDs in format X.Y or X.Y.Z (e.g., "1.1", "5.5.2")
    TASK_ID: /(\d+\.\d+(?:\.\d+)?)/,
    
    // Matches full task headers in markdown (e.g., "### Task 1.1: Some Title")
    TASK_HEADER: /### Task (\d+\.\d+(?:\.\d+)?): (.+?)(?=### Task|\n## |$)/gs,
    
    // Matches task titles in GitHub issues (e.g., "Task 1.1: Some Title")
    ISSUE_TITLE: /Task (\d+\.\d+(?:\.\d+)?): /,
    
    // Matches task titles at start of line (for validation)
    ISSUE_TITLE_STRICT: /^Task \d+\.\d+(?:\.\d+)?:/,
    
    // Matches any task reference in text (e.g., "Task 1.1" or "task 5.5.2")
    TASK_REFERENCE: /Task (\d+\.\d+(?:\.\d+)?)/i
};

/**
 * Utility functions for working with task IDs
 */
const TASK_UTILS = {
    /**
     * Extract task ID from a GitHub issue title
     * @param {string} title - The GitHub issue title
     * @returns {string|null} The task ID or null if not found
     */
    extractTaskIdFromTitle(title) {
        const match = title.match(TASK_ID_PATTERNS.ISSUE_TITLE);
        return match ? match[1] : null;
    },

    /**
     * Check if a string contains a valid task ID format
     * @param {string} str - String to check
     * @returns {boolean} True if contains valid task ID
     */
    isValidTaskId(str) {
        // Ensure the entire string matches the pattern (2-part or 3-part only)
        return /^\d+\.\d+(?:\.\d+)?$/.test(str);
    },

    /**
     * Check if a GitHub issue title is a valid task issue
     * @param {string} title - The GitHub issue title
     * @returns {boolean} True if it's a valid task issue title
     */
    isTaskIssueTitle(title) {
        return TASK_ID_PATTERNS.ISSUE_TITLE_STRICT.test(title);
    },

    /**
     * Format a task ID and title into a GitHub issue title
     * @param {string} taskId - The task ID (e.g., "1.1" or "5.5.2")
     * @param {string} title - The task title
     * @returns {string} Formatted GitHub issue title
     */
    formatIssueTitle(taskId, title) {
        return `Task ${taskId}: ${title}`;
    },

    /**
     * Check if an issue matches a specific task ID
     * @param {Object} issue - GitHub issue object
     * @param {string} taskId - Task ID to match against
     * @returns {boolean} True if issue matches the task ID
     */
    issueMatchesTaskId(issue, taskId) {
        const extractedId = TASK_UTILS.extractTaskIdFromTitle(issue.title);
        return extractedId === taskId;
    },

    /**
     * Parse and validate task ID components
     * @param {string} taskId - The task ID to parse
     * @returns {Object} Parsed components with validation
     */
    parseTaskId(taskId) {
        if (!this.isValidTaskId(taskId)) {
            return { valid: false, major: null, minor: null, patch: null };
        }

        const parts = taskId.split('.');
        return {
            valid: true,
            major: parseInt(parts[0], 10),
            minor: parseInt(parts[1], 10),
            patch: parts[2] ? parseInt(parts[2], 10) : null,
            is3Part: parts.length === 3
        };
    },

    /**
     * Compare two task IDs for sorting
     * @param {string} a - First task ID
     * @param {string} b - Second task ID
     * @returns {number} Comparison result (-1, 0, 1)
     */
    compareTaskIds(a, b) {
        const parsedA = TASK_UTILS.parseTaskId(a);
        const parsedB = TASK_UTILS.parseTaskId(b);

        if (!parsedA.valid || !parsedB.valid) {
            return 0; // Can't compare invalid IDs
        }

        // Compare major version
        if (parsedA.major !== parsedB.major) {
            return parsedA.major - parsedB.major;
        }

        // Compare minor version
        if (parsedA.minor !== parsedB.minor) {
            return parsedA.minor - parsedB.minor;
        }

        // Compare patch version (null treated as 0)
        const patchA = parsedA.patch || 0;
        const patchB = parsedB.patch || 0;
        return patchA - patchB;
    },

    /**
     * Get all task IDs from a list of issues
     * @param {Array} issues - Array of GitHub issue objects
     * @returns {Array} Array of task IDs found in the issues
     */
    extractTaskIdsFromIssues(issues) {
        return issues
            .map(issue => TASK_UTILS.extractTaskIdFromTitle(issue.title))
            .filter(taskId => taskId !== null);
    }
};

module.exports = {
    BOT_USER_AGENT,
    EXCLUDED_TASKS,
    TASK_ID_PATTERNS,
    TASK_UTILS
};
