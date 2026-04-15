import { createContext, ReactNode, useContext, useEffect, useMemo, useState } from 'react';
import { AuthTarget, initAuth, loginWithTarget, logout as keycloakLogout } from './keycloak';
import { setActiveTenantId } from '../services/tenantScope';

type AuthContextValue = {
  isReady: boolean;
  isAuthenticated: boolean;
  error: string | null;
  authTarget: AuthTarget | null;
  beginLogin: (target: AuthTarget) => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [isReady, setIsReady] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [authTarget, setAuthTarget] = useState<AuthTarget | null>(null);

  useEffect(() => {
    initAuth()
      .then((result) => {
        setIsAuthenticated(result.authenticated);
        setAuthTarget(result.target);
        setIsReady(true);
      })
      .catch((err: Error) => {
        setError(err.message);
        setIsReady(true);
      });
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    isReady,
    isAuthenticated,
    error,
    authTarget,
    beginLogin: async (target) => {
      setAuthTarget(target);
      await loginWithTarget(target);
    },
    logout: async () => {
      setIsAuthenticated(false);
      setAuthTarget(null);
      setActiveTenantId(null);
      await keycloakLogout();
    }
  }), [authTarget, error, isAuthenticated, isReady]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const value = useContext(AuthContext);
  if (!value) {
    throw new Error('AuthContext is not available.');
  }

  return value;
};
