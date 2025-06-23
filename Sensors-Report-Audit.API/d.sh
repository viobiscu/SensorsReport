#!/bin/bash
# This script builds the Docker image for the Sensors Report Audit API
# With using the Dockerfile in the current directory and docker context of parent directory.
# Context directory is important to use project dependencies.

cd ..
docker build -f Sensors-Report-Audit.API/Dockerfile -t viobiscu/sensors-report-audit .