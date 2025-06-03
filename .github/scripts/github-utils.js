/**
 * GitHub API Utilities
 *
 * Shared utility functions for GitHub API operations
 */

/**
 * Check if an error is a permission-related error
 * @param {Error} error - The error object to check
 * @returns {boolean} - True if the error indicates a permission issue
 */
function isPermissionError(error) {
    const permissionIndicators = [
        'Resource not accessible by integration',
        'Bad credentials',
        'Forbidden',
        'insufficient permissions',
        'requires authentication',
        'token does not have'
    ];

    const errorMessage = error.message || '';
    const errorStatus = error.status || 0;

    // Check status codes that clearly indicate permission issues
    if (errorStatus === 403 || errorStatus === 401) {
        return true;
    }

    // For 404, only treat as permission error if message contains permission indicators
    if (errorStatus === 404) {
        return permissionIndicators.some(indicator =>
            errorMessage.toLowerCase().includes(indicator.toLowerCase())
        );
    }

    // Check message content for permission indicators
    return permissionIndicators.some(indicator =>
        errorMessage.toLowerCase().includes(indicator.toLowerCase())
    );
}

/**
 * Execute an async operation with standardized permission error handling
 * @param {Function} operation - Async function to execute
 * @param {string} operationDescription - Description for logging (e.g., "add protection label")
 * @param {string} context - Context information for logging (e.g., "issue #123")
 * @returns {Promise<{success: boolean, error?: Error}>} - Result object
 */
async function executeWithPermissionHandling(operation, operationDescription, context) {
    try {
        await operation();
        return { success: true };
    } catch (error) {
        if (isPermissionError(error)) {
            console.warn(`⚠️  Cannot ${operationDescription} for ${context}: Insufficient permissions`);
        } else {
            console.warn(`⚠️  Failed to ${operationDescription} for ${context}: ${error.message}`);
        }
        return { success: false, error };
    }
}

module.exports = {
    isPermissionError,
    executeWithPermissionHandling
};
