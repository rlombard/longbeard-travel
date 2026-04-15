#!/bin/sh
set -eu

: "${BFF_BASE_URL:=${API_BASE_URL:-/bff/api}}"
: "${KEYCLOAK_URL:=http://localhost:8080}"
: "${KEYCLOAK_REALM:=tourops}"
: "${KEYCLOAK_CLIENT_ID:=frontend}"

envsubst '${BFF_BASE_URL} ${KEYCLOAK_URL} ${KEYCLOAK_REALM} ${KEYCLOAK_CLIENT_ID}' \
  < /usr/share/nginx/html/env.template.js > /usr/share/nginx/html/env.js
