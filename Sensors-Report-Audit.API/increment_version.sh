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

# if parameter has `-ver` then skip docker build
if [[ "$1" == "-ver" || "$1" == "-v" ]]; then
  echo "Skipping build docker."
  exit 0
fi

DOCKER_IMAGE="viobiscu/sensors-report-audit-api:$VERSION"
DOCKER_LATEST="viobiscu/sensors-report-audit-api:latest"
cd ..
docker build -f Sensors-Report-Audit.API/Dockerfile -t "$DOCKER_IMAGE" .
docker build -f Sensors-Report-Audit.API/Dockerfile -t "$DOCKER_LATEST" .
echo "Built image: $DOCKER_IMAGE"
echo "You can now push the image and update your deployment YAML with:"
echo "  image: $DOCKER_IMAGE"

# confirm to push the image
read -p "Do you want to push the image $DOCKER_IMAGE to Docker Hub? (Y/n): " confirm
confirm=${confirm:-y}
if [[ "$confirm" == "y" || "$confirm" == "Y" ]]; then
  docker push "$DOCKER_IMAGE"
  echo "Pushed images: $DOCKER_IMAGE"
fi

read -p "Do you want to push the image $DOCKER_LATEST to Docker Hub? (Y/n): " confirm
confirm=${confirm:-y}
if [[ "$confirm" == "y" || "$confirm" == "Y" ]]; then
  docker push "$DOCKER_LATEST"
  echo "Pushed images: $DOCKER_LATEST"
fi
