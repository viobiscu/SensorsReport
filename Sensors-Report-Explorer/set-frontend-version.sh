#!/bin/bash
# set-frontend-version.sh: Set the frontend version for Docker build and deployment
# Usage: ./set-frontend-version.sh

set -e

VERSION_FILE="version.txt"

# Always use version.txt, increment if numeric, or set to 1 if missing/invalid
if [ -f "$VERSION_FILE" ]; then
  CURRENT=$(cat "$VERSION_FILE")
  if [[ "$CURRENT" =~ ^[0-9]+$ ]]; then
    VERSION=$((CURRENT + 1))
  else
    VERSION=1
  fi
else
  VERSION=1
fi
echo "$VERSION" > "$VERSION_FILE"

echo "Setting frontend version to: $VERSION"

# Build Docker image with version tag and pass as build-arg/env
DOCKER_IMAGE="viobiscu/sensors-report-explorer-frontend:$VERSION"
docker build -f Dockerfile.frontend -t "$DOCKER_IMAGE" --build-arg FRONTEND_IMAGE_VERSION="$VERSION" .

echo "Built image: $DOCKER_IMAGE"

echo "You can now push the image and update your deployment YAML with:"
echo "  - name: FRONTEND_IMAGE_VERSION"
echo "    value: \"$VERSION\""
echo "  image: $DOCKER_IMAGE"
