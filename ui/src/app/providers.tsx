import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactNode, useEffect, useMemo, useState } from 'react';
import { initAuth } from '../auth/keycloak';

interface Props {
  children: ReactNode;
}

export const AppProviders = ({ children }: Props) => {
  const [isReady, setIsReady] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const queryClient = useMemo(() => new QueryClient(), []);

  useEffect(() => {
    initAuth()
      .then(() => setIsReady(true))
      .catch((err: Error) => setError(err.message));
  }, []);

  if (error) {
    return <div className="p-8 text-red-600">Authentication error: {error}</div>;
  }

  if (!isReady) {
    return <div className="p-8">Loading authentication...</div>;
  }

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
};
