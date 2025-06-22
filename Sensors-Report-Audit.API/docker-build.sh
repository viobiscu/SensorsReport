#!/bin/bash
# This script builds the Docker image for the Sensors Report Audit API
# With using the Dockerfile in the current directory and docker context of parent directory.
# Context directory is important to use project dependencies.

# Generate version tag based on current date/time
# Format: (year-24).(month).(day-hour-min)
YEAR=$(($(date +'%Y') - 2024))
VERSION="${YEAR}.$(date +'%m.%d%H%M')"
echo "Building Docker image with version: $VERSION"
echo $VERSION > version.txt

cd ..
docker build -f Sensors-Report-Audit.API/Dockerfile -t viobiscu/sensors-report-audit:$VERSION .
docker build -f Sensors-Report-Audit.API/Dockerfile -t viobiscu/sensors-report-audit:latest .

echo "Docker image built with version: $VERSION"