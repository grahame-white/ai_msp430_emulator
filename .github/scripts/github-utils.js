/**
 * GitHub API Utilities
 *
 * Shared utility functions for GitHub API operations, including
 * proper rate limiting following GitHub's best practices
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
            console.warn(
                `⚠️  Cannot ${operationDescription} for ${context}: Insufficient permissions`
            );
        } else {
            console.warn(`⚠️  Failed to ${operationDescription} for ${context}: ${error.message}`);
        }
        return { success: false, error };
    }
}

/**
 * Handle rate limiting according to GitHub's best practices
 * @param {Error} error - The API error
 * @param {Object} response - The response object with headers
 * @param {number} attempt - Current attempt number for exponential backoff
 * @returns {Promise<number>} - Number of milliseconds to wait, or 0 if not rate limited
 */
async function handleRateLimit(error, response, attempt = 1) {
    // Check if this is a rate limit error
    if (error && error.status === 403 && error.message.includes('rate limit')) {
        console.log('🚦 API rate limit detected');

        // Check for retry-after header first (takes priority)
        const retryAfter = response?.headers?.['retry-after'];
        if (retryAfter) {
            const waitSeconds = parseInt(retryAfter, 10);
            console.log(`⏳ Retry-After header present: waiting ${waitSeconds} seconds`);
            return waitSeconds * 1000;
        }

        // Check rate limit headers
        const remaining = response?.headers?.['x-ratelimit-remaining'];
        const resetTime = response?.headers?.['x-ratelimit-reset'];

        if (remaining === '0' && resetTime) {
            const resetTimestamp = parseInt(resetTime, 10) * 1000;
            const currentTime = Date.now();
            const waitTime = Math.max(resetTimestamp - currentTime, 0);

            if (waitTime > 0) {
                console.log(`⏳ Rate limit reset in ${Math.ceil(waitTime / 1000)} seconds`);
                return waitTime;
            }
        }

        // Default exponential backoff for secondary rate limits
        const baseDelay = 60000; // 1 minute base
        const exponentialDelay = baseDelay * Math.pow(2, attempt - 1);
        const maxDelay = 900000; // 15 minutes max
        const waitTime = Math.min(exponentialDelay, maxDelay);

        console.log(
            `⏳ Using exponential backoff: waiting ${Math.ceil(waitTime / 1000)} seconds (attempt ${attempt})`
        );
        return waitTime;
    }

    return 0;
}

/**
 * Execute a GitHub API operation with proper rate limiting
 * @param {Function} apiCall - Function that makes the API call
 * @param {string} operationName - Name of the operation for logging
 * @param {number} maxRetries - Maximum number of retries
 * @returns {Promise<any>} - Result of the API call
 */
async function executeWithRateLimit(apiCall, operationName = 'API call', maxRetries = 3) {
    let lastError = null;

    for (let attempt = 1; attempt <= maxRetries + 1; attempt++) {
        try {
            const result = await apiCall();

            // Log remaining rate limit for monitoring
            if (result?.headers?.['x-ratelimit-remaining']) {
                const remaining = result.headers['x-ratelimit-remaining'];
                const limit = result.headers['x-ratelimit-limit'];
                if (remaining && limit && parseInt(remaining) < 100) {
                    console.log(`⚠️  Rate limit warning: ${remaining}/${limit} requests remaining`);
                }
            }

            return result;
        } catch (error) {
            lastError = error;

            // Check if this is a rate limit error
            const waitTime = await handleRateLimit(error, error.response, attempt);

            if (waitTime > 0) {
                if (attempt <= maxRetries) {
                    console.log(
                        `⏳ ${operationName}: Rate limited, waiting ${Math.ceil(waitTime / 1000)}s before retry ${attempt}/${maxRetries}`
                    );
                    await delay(waitTime);
                    continue;
                } else {
                    console.error(`❌ ${operationName}: Max retries exceeded for rate limiting`);
                    throw error;
                }
            }

            // If not a rate limit error, throw immediately
            throw error;
        }
    }

    throw lastError;
}

/**
 * Smart delay that includes rate limit-aware spacing
 * @param {number} baseDelay - Base delay in milliseconds
 * @param {Object} lastResponse - Last API response to check rate limit headers
 * @returns {Promise<void>}
 */
async function smartDelay(baseDelay = 1000, lastResponse = null) {
    // Check if we're getting close to rate limits
    if (lastResponse?.headers?.['x-ratelimit-remaining']) {
        const remaining = parseInt(lastResponse.headers['x-ratelimit-remaining']);
        const limit = parseInt(lastResponse.headers['x-ratelimit-limit'] || '5000');

        // If we have less than 10% of our rate limit remaining, be more cautious
        if (remaining < limit * 0.1) {
            const cautionDelay = Math.max(baseDelay * 3, 3000);
            console.log(
                `⏳ Low rate limit (${remaining}/${limit}), using cautious delay: ${cautionDelay}ms`
            );
            await delay(cautionDelay);
            return;
        }
    }

    // Standard delay
    await delay(baseDelay);
}

/**
 * Utility function for delays
 * @param {number} ms - Milliseconds to delay
 * @returns {Promise<void>}
 */
function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

module.exports = {
    isPermissionError,
    executeWithPermissionHandling,
    handleRateLimit,
    executeWithRateLimit,
    smartDelay,
    delay
};
