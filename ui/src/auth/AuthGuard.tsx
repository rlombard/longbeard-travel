import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { BootSplash } from '../components/BootSplash';
import { useAuth } from './AuthContext';

interface Props {
  children: ReactNode;
}

export const AuthGuard = ({ children }: Props) => {
  const auth = useAuth();

  if (!auth.isReady) {
    return <BootSplash eyebrow="Checking Session" title="Verifying your login" detail="We are checking existing authentication before opening the workspace." />;
  }

  if (!auth.isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};
