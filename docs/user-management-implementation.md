# User Management Implementation

## What added

- admin-only backend user-management API backed by Keycloak Admin API
- Keycloak role claim enrichment and `AdminOnly` policy
- admin UI for listing, creating, editing, disabling, resetting passwords, and assigning roles
- service-account client wiring for local Keycloak realm and docker compose

## Backend endpoints

- `GET /api/admin/users`
- `GET /api/admin/users/{userId}`
- `POST /api/admin/users`
- `PATCH /api/admin/users/{userId}`
- `POST /api/admin/users/{userId}/reset-password`
- `PUT /api/admin/users/{userId}/roles`
- `GET /api/admin/roles`

## UI routes

- `/admin/users`

## Key decisions

- roles and permissions stay in Keycloak only
- browser calls TourOps API only
- backend uses confidential client credentials for Keycloak admin operations
- create and reset-password flows always set temporary credentials and `UPDATE_PASSWORD`
- frontend admin checks are UX only; backend policy is real enforcement

## Config

- `KeycloakAdmin__BaseUrl`
- `KeycloakAdmin__Realm`
- `KeycloakAdmin__ClientId`
- `KeycloakAdmin__ClientSecret`

## Local realm changes

- added confidential client `tourops-admin-api`
- added service-account mapping for user and role management scopes

## Follow-up

- add group management if Keycloak groups become part of operator workflow
- add session revocation endpoint if admins need forced sign-out
- align EF/Npgsql package versions to remove existing restore warning noise
