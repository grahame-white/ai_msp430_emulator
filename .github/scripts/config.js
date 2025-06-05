/**
 * Shared configuration constants for GitHub automation scripts
 */

// Bot configuration
const BOT_USER_AGENT = 'MSP430-Emulator-Issues-Bot v1.0.0';

// Tasks to exclude from automation (already implemented or actively being developed)
const EXCLUDED_TASKS = ['1.1', '1.2', '1.3', '1.4', '1.5'];

module.exports = {
    BOT_USER_AGENT,
    EXCLUDED_TASKS
};
