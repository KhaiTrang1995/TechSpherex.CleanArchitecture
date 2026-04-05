#!/bin/bash

# Creates a clean zip of the template for distribution (no git files, no build artifacts)

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
VERSION=$(cat "$REPO_ROOT/VERSION")
OUTPUT_DIR="$REPO_ROOT/dist"
ZIP_NAME="CleanArchitectureTemplate.zip"

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

cd "$REPO_ROOT" || exit 1

# Clean build artifacts
dotnet clean src/TechSpherex.CleanArchitecture.slnx -c Debug --verbosity quiet 2>/dev/null
dotnet clean src/TechSpherex.CleanArchitecture.slnx -c Release --verbosity quiet 2>/dev/null
find . -type d \( -name bin -o -name obj \) -not -path './.git/*' -exec rm -rf {} + 2>/dev/null

# Convert paths for PowerShell on Windows (Git Bash uses /c/ but pwsh needs C:\)
to_windows_path() {
    echo "$1" | sed 's|^/\([a-zA-Z]\)/|\1:\\|' | sed 's|/|\\|g'
}

if command -v pwsh &>/dev/null; then
    WIN_OUTPUT="$(to_windows_path "$OUTPUT_DIR/$ZIP_NAME")"
    WIN_REPO="$(to_windows_path "$REPO_ROOT")"
    pwsh -Command "
        \$items = Get-ChildItem -Path '$WIN_REPO' -Exclude '.git','dist'
        Compress-Archive -Path \$items.FullName -DestinationPath '$WIN_OUTPUT' -Force
    "
elif command -v zip &>/dev/null; then
    zip -r "$OUTPUT_DIR/$ZIP_NAME" . \
        -x ".git/*" -x ".git" \
        -x "dist/*" -x "dist" \
        -x "**/bin/*" -x "**/obj/*" \
        -x "**/.vs/*" -x "**/.idea/*" -x "**/.vscode/*"
else
    echo "Error: Neither pwsh nor zip found. Install one of them."
    exit 1
fi

echo ""
echo "Created: $OUTPUT_DIR/$ZIP_NAME"
echo "Size: $(wc -c < "$OUTPUT_DIR/$ZIP_NAME" | awk '{printf "%.0fKB", $1/1024}')"
