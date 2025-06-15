#!/usr/bin/env bash
# .github/scripts/lint-with-annotations.sh
# Lint automation scripts with GitHub Actions annotations support

set -e

cd "$(dirname "$0")"

# Check if we're in GitHub Actions
is_github_actions() {
    [ -n "${GITHUB_ACTIONS}" ]
}

# Generate GitHub Actions annotation
github_annotation() {
    local type="$1"
    local file="$2"
    local line="$3"
    local col="$4"
    local message="$5"
    
    if is_github_actions; then
        if [ -n "$file" ] && [ -n "$line" ]; then
            if [ -n "$col" ]; then
                echo "::${type} file=${file},line=${line},col=${col}::${message}"
            else
                echo "::${type} file=${file},line=${line}::${message}"
            fi
        else
            echo "::${type}::${message}"
        fi
    else
        # Non-GitHub Actions environment - output human-readable format
        case "$type" in
            "error")   echo "❌ ${file}:${line}:${col}: ${message}" ;;
            "warning") echo "⚠️  ${file}:${line}:${col}: ${message}" ;;
            "notice")  echo "ℹ️  ${file}:${line}:${col}: ${message}" ;;
            *)         echo "${file}:${line}:${col}: ${message}" ;;
        esac
    fi
}

# Process ESLint JSON output into annotations  
process_eslint_annotations() {
    local json_file="$1"
    
    if [ ! -f "$json_file" ]; then
        return 0
    fi
    
    # Extract JSON from the output (skip npm header lines)
    local json_content
    json_content=$(grep -A 9999 '^\[{' "$json_file" | head -n -0)
    
    if [ -z "$json_content" ]; then
        return 0
    fi
    
    # Parse ESLint JSON output
    if command -v jq >/dev/null 2>&1; then
        echo "$json_content" | jq -r '.[] | 
            .filePath as $file | 
            .messages[] | 
            "\($file)|\(.line)|\(.column)|\(.severity)|\(.message)"' | \
        while IFS='|' read -r file line col severity message; do
            local annotation_type="notice"
            case "$severity" in
                "1") annotation_type="warning" ;;
                "2") annotation_type="error" ;;
            esac
            
            # Convert to relative path
            local relative_file="${file#${PWD}/}"
            relative_file="${relative_file#../../}"  # Remove relative path prefix
            
            github_annotation "$annotation_type" ".github/scripts/$relative_file" "$line" "$col" "$message"
        done
    fi
}

# Process Prettier check output into annotations
process_prettier_annotations() {
    local output_file="$1"
    
    if [ ! -f "$output_file" ]; then
        return 0
    fi
    
    # Prettier outputs files that need formatting
    while IFS= read -r line; do
        if [[ "$line" =~ ^.+\.(js|json|md|yml|yaml)$ ]]; then
            # Convert to relative path
            local relative_file="${line#${PWD}/}"
            relative_file="${relative_file#../../}"  # Remove relative path prefix
            
            github_annotation "error" ".github/scripts/$relative_file" "" "" "File needs prettier formatting"
        fi
    done < "$output_file"
}

echo "Installing dependencies..."
npm install

echo "Running ESLint..."
if is_github_actions; then
    ESLINT_OUTPUT=$(mktemp)
    # Redirect npm output to stderr, capture only eslint JSON output
    if ! npm run lint:json 2>/dev/null > "$ESLINT_OUTPUT"; then
        echo "❌ ESLint issues detected!"
        process_eslint_annotations "$ESLINT_OUTPUT"
        # Also show regular output
        npm run lint || true
        rm -f "$ESLINT_OUTPUT"
        exit 1
    fi
    rm -f "$ESLINT_OUTPUT"
else
    npm run lint
fi

echo "Running Prettier check..."
if is_github_actions; then
    PRETTIER_OUTPUT=$(mktemp)
    if ! npm run format:check > "$PRETTIER_OUTPUT" 2>&1; then
        echo "❌ Prettier formatting issues detected!"
        process_prettier_annotations "$PRETTIER_OUTPUT"
        cat "$PRETTIER_OUTPUT"
        rm -f "$PRETTIER_OUTPUT" 
        exit 1
    fi
    rm -f "$PRETTIER_OUTPUT"
else
    npm run format:check
fi

echo "✅ All automation script checks passed!"

echo "Running YAML linting..."
if is_github_actions; then
    YAML_OUTPUT=$(mktemp)
    if ! npm run lint:yaml > "$YAML_OUTPUT" 2>&1; then
        echo "❌ YAML linting issues detected!"
        # yaml-lint doesn't provide structured output, so just show the output
        cat "$YAML_OUTPUT"
        rm -f "$YAML_OUTPUT"
        exit 1
    fi
    rm -f "$YAML_OUTPUT"
else
    npm run lint:yaml
fi

echo "✅ YAML linting passed!"