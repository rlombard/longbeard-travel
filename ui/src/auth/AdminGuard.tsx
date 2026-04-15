import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { getAccessToken, isAdminUser } from './keycloak';

interface Props {
  children: ReactNode;
}

export const AdminGuard = ({ children }: Props) => {
  if (!getAccessToken()) {
    return <Navigate to="/" replace />;
  }

  if (!isAdminUser()) {
    return (
      <div className="rounded-lg border border-rose-200 bg-rose-50 p-6 shadow-sm">
        <p className="text-sm font-semibold text-rose-700">Admin access required</p>
        <p className="mt-2 text-sm text-rose-600">This area uses Keycloak admin-backed actions. Ask an admin for the `admin` role.</p>
      </div>
    );
  }

  return <>{children}</>;
};
