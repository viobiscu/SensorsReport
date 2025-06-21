#!/bin/bash
VERSION_FILE="version.txt"
if [ -f "$VERSION_FILE" ]; then
    OLD_VERSION=$(cat "$VERSION_FILE" | tr -d '\r\n' | tr -d ' ')
    MAJOR=$(echo "$OLD_VERSION" | cut -d. -f1)
    MINOR=$(echo "$OLD_VERSION" | cut -d. -f2)
    PATCH=$(echo "$OLD_VERSION" | cut -d. -f3)
    PATCH=$((10#$PATCH + 1))
    echo "$MAJOR.$MINOR.$PATCH" > "$VERSION_FILE"
else
    echo "1.0.0.1" > "$VERSION_FILE"
fi
