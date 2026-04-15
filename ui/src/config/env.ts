const runtime = window.__APP_ENV__ ?? {};

export const appEnv = {
  bffBaseUrl: runtime.BFF_BASE_URL ?? runtime.API_BASE_URL ?? import.meta.env.VITE_BFF_BASE_URL ?? import.meta.env.VITE_API_BASE_URL ?? '/bff/api',
  keycloakUrl: runtime.KEYCLOAK_URL ?? import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8080',
  keycloakRealm: runtime.KEYCLOAK_REALM ?? import.meta.env.VITE_KEYCLOAK_REALM ?? 'tourops',
  keycloakClientId: runtime.KEYCLOAK_CLIENT_ID ?? import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'frontend'
};
