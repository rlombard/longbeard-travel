import Keycloak from 'keycloak-js';
import { appEnv } from '../config/env';

export const keycloak = new Keycloak({
  url: appEnv.keycloakUrl,
  realm: appEnv.keycloakRealm,
  clientId: appEnv.keycloakClientId
});

let accessToken: string | null = null;

export const getAccessToken = () => accessToken;
export const getCurrentUserId = () => keycloak.tokenParsed?.sub ?? null;

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
