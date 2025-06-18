#!/usr/bin/env bash
# .github/scripts/lint-with-annotations.sh
# Lint automation scripts with GitHub Actions annotations support

set -euo pipefail

cd "$(dirname "$0")"

# Source shared annotation helper functions
source "../../script/github-annotations"

# Improved error handling with logging
log_error() {
    echo "ERROR: $*" >&2
}

log_debug() {
    if [ "${DEBUG:-}" = "1" ]; then
        echo "DEBUG: $*" >&2
    fi
}

# Process ESLint JSON output into annotations for automation scripts
process_automation_eslint_annotations() {
    local json_file="$1"
    
    if [ ! -f "$json_file" ]; then
        log_debug "ESLint output file not found: $json_file"
        return 0
    fi
    
    if [ ! -s "$json_file" ]; then
        log_debug "ESLint output file is empty: $json_file"
        return 0
    fi
    
    # More robust JSON extraction - look for JSON array anywhere in the file
    local json_content
    json_content=$(sed -n '/^\[/,/^\]/p' "$json_file" | tr -d '\0' || echo "")
    
    if [ -z "$json_content" ]; then
        log_debug "No JSON content found in ESLint output"
        return 0
    fi
    
    # Validate JSON before processing
    if ! echo "$json_content" | jq empty 2>/dev/null; then
        log_error "Invalid JSON in ESLint output"
        return 1
    fi
    
    # Parse ESLint JSON output
    if command -v jq >/dev/null 2>&1; then
        echo "$json_content" | jq -r '.[] | 
            .filePath as $file | 
            .messages[] | 
            "\($file)|\(.line)|\(.column)|\(.severity)|\(.message)"' 2>/dev/null | \
        while IFS='|' read -r file line col severity message; do
            local annotation_type="notice"
            case "$severity" in
                "1") annotation_type="warning" ;;
                "2") annotation_type="error" ;;
            esac
            
            # Convert to relative path and add .github/scripts prefix
            local relative_file="${file#${PWD}/}"
            relative_file="${relative_file#../../}"  # Remove relative path prefix
            
            github_annotation "$annotation_type" ".github/scripts/$relative_file" "$line" "$col" "$message"
        done
    else
        log_error "jq not available for JSON processing"
        return 1
    fi
}

# Process Prettier check output into annotations for automation scripts
process_automation_prettier_annotations() {
    local output_file="$1"
    
    if [ ! -f "$output_file" ]; then
        log_debug "Prettier output file not found: $output_file"
        return 0
    fi
    
    if [ ! -s "$output_file" ]; then
        log_debug "Prettier output file is empty: $output_file"
        return 0
    fi
    
    # Prettier outputs files that need formatting
    while IFS= read -r line; do
        # Skip empty lines and npm output
        if [ -z "$line" ] || [[ "$line" =~ ^(>|Checking|All\ matched) ]]; then
            continue
        fi
        
        if [[ "$line" =~ ^.+\.(js|json|md|yml|yaml)$ ]]; then
            # Convert to relative path and add .github/scripts prefix
            local relative_file="${line#${PWD}/}"
            relative_file="${relative_file#../../}"  # Remove relative path prefix
            
            github_annotation "error" ".github/scripts/$relative_file" "" "" "File needs prettier formatting"
        fi
    done < "$output_file"
}

# Cleanup function for temporary files
cleanup() {
    local exit_code=$?
    log_debug "Cleaning up temporary files..."
    
    # Remove any temporary files we might have created
    for temp_file in "${TEMP_FILES[@]}"; do
        if [ -f "$temp_file" ]; then
            rm -f "$temp_file"
            log_debug "Removed temporary file: $temp_file"
        fi
    done
    
    exit $exit_code
}

# Array to track temporary files for cleanup
TEMP_FILES=()

# Set up cleanup trap
trap cleanup EXIT INT TERM

# Function to create and track temporary files
make_temp_file() {
    local temp_file
    temp_file=$(mktemp)
    TEMP_FILES+=("$temp_file")
    echo "$temp_file"
}

echo "Installing dependencies..."
if ! npm ci; then
    log_error "Failed to install dependencies"
    exit 1
fi

echo "Running ESLint..."
if is_github_actions; then
    ESLINT_OUTPUT=$(make_temp_file)
    # Redirect npm output to stderr, capture only eslint JSON output
    if ! npm run lint:json 2>/dev/null > "$ESLINT_OUTPUT"; then
        echo "❌ ESLint issues detected!"
        if ! process_automation_eslint_annotations "$ESLINT_OUTPUT"; then
            log_error "Failed to process ESLint annotations"
        fi
        # Also show regular output
        npm run lint || true
        exit 1
    fi
else
    if ! npm run lint; then
        log_error "ESLint failed"
        exit 1
    fi
fi

echo "Running Prettier check..."
if is_github_actions; then
    PRETTIER_OUTPUT=$(make_temp_file)
    if ! npm run format:check > "$PRETTIER_OUTPUT" 2>&1; then
        echo "❌ Prettier formatting issues detected!"
        if ! process_automation_prettier_annotations "$PRETTIER_OUTPUT"; then
            log_error "Failed to process Prettier annotations"
        fi
        cat "$PRETTIER_OUTPUT"
        exit 1
    fi
else
    if ! npm run format:check; then
        log_error "Prettier check failed"
        exit 1
    fi
fi

echo "✅ All automation script checks passed!"

echo "Running YAML linting..."
if is_github_actions; then
    YAML_OUTPUT=$(make_temp_file)
    if ! npm run lint:yaml > "$YAML_OUTPUT" 2>&1; then
        echo "❌ YAML linting issues detected!"
        # yaml-lint doesn't provide structured output, so just show the output
        cat "$YAML_OUTPUT"
        exit 1
    fi
else
    if ! npm run lint:yaml; then
        log_error "YAML linting failed"
        exit 1
    fi
fi

echo "✅ YAML linting passed!"