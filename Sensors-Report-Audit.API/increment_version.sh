#!/bin/bash
# increment_version.sh: Increment integer version for Docker build/tagging
VERSION_FILE="version.txt"
if [ -f "$VERSION_FILE" ]; then
  CURRENT=$(cat "$VERSION_FILE" | tr -d '\r\n' | tr -d ' ')
  if [[ "$CURRENT" =~ ^[0-9]+$ ]]; then
    VERSION=$((CURRENT + 1))
  else
    VERSION=1
  fi
else
  VERSION=1
fi
echo "$VERSION" > "$VERSION_FILE"
echo "Setting API version to: $VERSION"
DOCKER_IMAGE="viobiscu/sensors-report-audit-api:$VERSION"
docker build -f Dockerfile -t "$DOCKER_IMAGE" .
echo "Built image: $DOCKER_IMAGE"
echo "You can now push the image and update your deployment YAML with:"
echo "  image: $DOCKER_IMAGE"
