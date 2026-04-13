#!/bin/sh
set -eu

: "${API_BASE_URL:=/api}"
: "${KEYCLOAK_URL:=http://localhost:8080}"
: "${KEYCLOAK_REALM:=tourops}"
: "${KEYCLOAK_CLIENT_ID:=frontend}"

envsubst '${API_BASE_URL} ${KEYCLOAK_URL} ${KEYCLOAK_REALM} ${KEYCLOAK_CLIENT_ID}' \
  < /usr/share/nginx/html/env.template.js > /usr/share/nginx/html/env.js
