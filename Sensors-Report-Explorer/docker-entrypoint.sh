#!/bin/sh
# docker-entrypoint.sh: Start nginx (no config.js generation needed)

set -e

exec nginx -g 'daemon off;'
