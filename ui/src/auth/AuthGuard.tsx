import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { getAccessToken } from './keycloak';

interface Props {
  children: ReactNode;
}

export const AuthGuard = ({ children }: Props) => {
  if (!getAccessToken()) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};
