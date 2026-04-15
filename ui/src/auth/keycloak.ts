import Keycloak from 'keycloak-js';

export type AuthTarget = {
  keycloakUrl: string;
  realm: string;
  clientId: string;
  kind: 'management' | 'tenant';
  tenantId?: string | null;
  tenantSlug?: string | null;
};

type ParsedToken = {
  sub?: string;
  realm_access?: {
    roles?: string[];
  };
  resource_access?: Record<string, { roles?: string[] }>;
};

const authTargetStorageKey = 'tourops.auth.target';

let accessToken: string | null = null;
let currentTarget: AuthTarget | null = null;
let keycloak: Keycloak | null = null;

export const getAccessToken = () => accessToken;
export const getCurrentAuthTarget = () => currentTarget;
export const getCurrentUserId = () => keycloak?.tokenParsed?.sub ?? null;

export const getRealmRoles = () => ((keycloak?.tokenParsed as ParsedToken | undefined)?.realm_access?.roles ?? []).filter(Boolean);

export const getClientRoles = (clientId?: string) => {
  const resourceAccess = (keycloak?.tokenParsed as ParsedToken | undefined)?.resource_access ?? {};

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

export const getStoredAuthTarget = (): AuthTarget | null => {
  const raw = window.sessionStorage.getItem(authTargetStorageKey);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AuthTarget;
  } catch {
    window.sessionStorage.removeItem(authTargetStorageKey);
    return null;
  }
};

export const storeAuthTarget = (target: AuthTarget) => {
  currentTarget = target;
  window.sessionStorage.setItem(authTargetStorageKey, JSON.stringify(target));
};

export const clearStoredAuthTarget = () => {
  currentTarget = null;
  window.sessionStorage.removeItem(authTargetStorageKey);
};

const ensureClient = (target: AuthTarget) => {
  if (
    !keycloak
    || !currentTarget
    || currentTarget.realm !== target.realm
    || currentTarget.clientId !== target.clientId
    || currentTarget.keycloakUrl !== target.keycloakUrl
  ) {
    keycloak = new Keycloak({
      url: target.keycloakUrl,
      realm: target.realm,
      clientId: target.clientId
    });
  }

  currentTarget = target;
  return keycloak;
};

export const initAuth = async () => {
  const target = getStoredAuthTarget();
  if (!target) {
    setAccessToken(null);
    return {
      authenticated: false,
      target: null as AuthTarget | null
    };
  }

  const client = ensureClient(target);
  const authenticated = await client.init({
    onLoad: 'check-sso',
    checkLoginIframe: false,
    pkceMethod: 'S256'
  });

  if (!authenticated || !client.token) {
    setAccessToken(null);
    return {
      authenticated: false,
      target
    };
  }

  setAccessToken(client.token);
  client.onTokenExpired = async () => {
    try {
      await client.updateToken(30);
      setAccessToken(client.token ?? null);
    } catch {
      await client.login({ redirectUri: `${window.location.origin}/auth/callback` });
    }
  };

  return {
    authenticated: true,
    target
  };
};

export const loginWithTarget = async (target: AuthTarget) => {
  storeAuthTarget(target);
  const client = ensureClient(target);
  await client.login({
    redirectUri: `${window.location.origin}/auth/callback`
  });
};

export const logout = async () => {
  const client = keycloak;
  setAccessToken(null);
  clearStoredAuthTarget();

  if (!client) {
    window.location.assign('/');
    return;
  }

  await client.logout({
    redirectUri: `${window.location.origin}/`
  });
};
