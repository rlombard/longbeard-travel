import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactNode, useMemo } from 'react';
import { AuthProvider } from '../auth/AuthContext';

interface Props {
  children: ReactNode;
}

export const AppProviders = ({ children }: Props) => {
  const queryClient = useMemo(() => new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30000,
        gcTime: 5 * 60 * 1000,
        refetchOnWindowFocus: false,
        retry: 1
      }
    }
  }), []);

  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>{children}</AuthProvider>
    </QueryClientProvider>
  );
};
