export type AuthTarget = {
  keycloakUrl: string;
  realm: string;
  clientId: string;
};

export type SessionTenantMembership = {
  tenantId: string;
  tenantSlug: string;
  tenantName: string;
  role: 'PlatformAdmin' | 'TenantAdmin' | 'Operator';
  realmName: string;
};

export type SessionActor = {
  isAuthenticated: boolean;
  isPlatformAdmin: boolean;
  userId?: string | null;
  displayName?: string | null;
  email?: string | null;
  currentTenantId?: string | null;
  currentTenantSlug?: string | null;
  currentTenantName?: string | null;
  homeArea: string;
  memberships: SessionTenantMembership[];
};

export type SessionBootstrap = {
  deploymentMode: 'Standalone' | 'SaaS';
  platformManagementEnabled: boolean;
  publicSignupEnabled: boolean;
  publicSignupDisabledReason: string;
  managementAuth: AuthTarget;
  standaloneTenantAuth?: AuthTarget | null;
  session?: SessionActor | null;
};

export type DiscoverTenantRequest = {
  email?: string;
  tenantSlug?: string;
};

export type TenantLoginDiscovery = {
  found: boolean;
  tenantId?: string | null;
  tenantSlug?: string | null;
  tenantName?: string | null;
  resolutionSource: string;
  auth?: AuthTarget | null;
};
