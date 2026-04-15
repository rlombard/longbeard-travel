import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { BootSplash } from '../components/BootSplash';
import { useSessionBootstrap } from '../features/session/hooks';
import { getActiveTenantId } from '../services/tenantScope';
import { useAuth } from './AuthContext';

interface Props {
  children: ReactNode;
}

export const TenantAdminGuard = ({ children }: Props) => {
  const auth = useAuth();
  const bootstrapQuery = useSessionBootstrap();

  if (!auth.isReady || bootstrapQuery.isLoading) {
    return <BootSplash eyebrow="Opening Settings" title="Preparing tenant admin" detail="Loading tenant role, config scope, and workspace settings." />;
  }

  if (!auth.isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const session = bootstrapQuery.data?.session;
  const activeTenantId = getActiveTenantId() ?? session?.currentTenantId ?? undefined;
  const membership = session?.memberships.find((item) => item.tenantId === activeTenantId);
  const canManageTenant = session?.isPlatformAdmin || membership?.role === 'TenantAdmin';

  if (!canManageTenant) {
    return (
      <div className="rounded-lg border border-rose-200 bg-rose-50 p-6 shadow-sm">
        <p className="text-sm font-semibold text-rose-700">Tenant admin access required</p>
        <p className="mt-2 text-sm text-rose-600">This area is for tenant setup, email system, and tenant user management.</p>
      </div>
    );
  }

  return <>{children}</>;
};
