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

module.exports = {
    isPermissionError
};
