import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { BootSplash } from '../components/BootSplash';
import { useSessionBootstrap } from '../features/session/hooks';
import { useAuth } from './AuthContext';

interface Props {
  children: ReactNode;
}

export const AdminGuard = ({ children }: Props) => {
  const auth = useAuth();
  const bootstrapQuery = useSessionBootstrap();

  if (!auth.isReady || bootstrapQuery.isLoading) {
    return <BootSplash eyebrow="Opening Platform" title="Preparing management console" detail="Loading deployment access, roles, and tenant oversight context." />;
  }

  if (!auth.isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  if (!bootstrapQuery.data?.platformManagementEnabled || !bootstrapQuery.data.session?.isPlatformAdmin) {
    return (
      <div className="rounded-lg border border-rose-200 bg-rose-50 p-6 shadow-sm">
        <p className="text-sm font-semibold text-rose-700">Admin access required</p>
        <p className="mt-2 text-sm text-rose-600">This area is for deployment owners. Tenant operators stay in the operator app.</p>
      </div>
    );
  }

  return <>{children}</>;
};
