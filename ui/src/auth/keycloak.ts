import Keycloak from 'keycloak-js';
import { appEnv } from '../config/env';

export const keycloak = new Keycloak({
  url: appEnv.keycloakUrl,
  realm: appEnv.keycloakRealm,
  clientId: appEnv.keycloakClientId
});

let accessToken: string | null = null;

type ParsedToken = {
  sub?: string;
  realm_access?: {
    roles?: string[];
  };
  resource_access?: Record<string, { roles?: string[] }>;
};

export const getAccessToken = () => accessToken;
export const getCurrentUserId = () => keycloak.tokenParsed?.sub ?? null;
export const getRealmRoles = () => ((keycloak.tokenParsed as ParsedToken | undefined)?.realm_access?.roles ?? []).filter(Boolean);
export const getClientRoles = (clientId?: string) => {
  const resourceAccess = (keycloak.tokenParsed as ParsedToken | undefined)?.resource_access ?? {};

  if (clientId) {
    return (resourceAccess[clientId]?.roles ?? []).filter(Boolean);
  }

  return Object.values(resourceAccess).flatMap((entry) => (entry.roles ?? []).filter(Boolean));
};
export const hasRole = (roleName: string) =>
  getRealmRoles().some((role) => role.toLowerCase() === roleName.toLowerCase())
  || getClientRoles().some((role) => role.toLowerCase() === roleName.toLowerCase());
export const isAdminUser = () => hasRole('admin');

export const setAccessToken = (token: string | null) => {
  accessToken = token;
};

export const initAuth = async () => {
  const authenticated = await keycloak.init({
    onLoad: 'login-required',
    checkLoginIframe: false,
    pkceMethod: 'S256'
  });

  if (!authenticated || !keycloak.token) {
    throw new Error('Authentication failed.');
  }

  setAccessToken(keycloak.token);

  keycloak.onTokenExpired = async () => {
    try {
      await keycloak.updateToken(30);
      setAccessToken(keycloak.token ?? null);
    } catch {
      await keycloak.login();
    }
  };

  return keycloak;
};
