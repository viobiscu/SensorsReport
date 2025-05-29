#!/bin/sh
# docker-entrypoint.sh: Generate config.js from BACKEND_BASE_URL env and start nginx

set -e

# Default to service name if not set
: "${BACKEND_BASE_URL:=http://sensors-report-explorer-backend:5000}"

# Generate config.js from template
if [ -f /usr/share/nginx/html/config.js.template ]; then
  envsubst < /usr/share/nginx/html/config.js.template > /usr/share/nginx/html/config.js
fi

exec nginx -g 'daemon off;'
