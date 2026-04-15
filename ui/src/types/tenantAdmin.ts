import { EmailProviderConnectionListItem } from './emailIntegration';
import { TenantConfigEntry, TenantUserRole, TenantUserStatus } from './platform';

export interface TenantWorkspaceUser {
  membershipId: string;
  userId: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  role: TenantUserRole;
  status: TenantUserStatus;
  enabled: boolean;
  invitedAt: string;
  lastSeenAt?: string | null;
}

export interface TenantAdminWorkspace {
  tenantId: string;
  tenantSlug: string;
  tenantName: string;
  legalName?: string | null;
  defaultCurrency: string;
  timeZone: string;
  billingEmail?: string | null;
  notes?: string | null;
  realmName: string;
  configEntries: TenantConfigEntry[];
  users: TenantWorkspaceUser[];
  emailConnections: EmailProviderConnectionListItem[];
}

export interface UpdateTenantWorkspaceProfileRequest {
  tenantName: string;
  legalName?: string | null;
  defaultCurrency: string;
  timeZone: string;
  billingEmail?: string | null;
  notes?: string | null;
}

export interface CreateTenantWorkspaceUserRequest {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: TenantUserRole;
  temporaryPassword?: string;
}

export interface TenantWorkspaceUserCreateResponse {
  user: TenantWorkspaceUser;
  temporaryPassword: string;
}

export interface UpdateTenantWorkspaceUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  role: TenantUserRole;
  enabled: boolean;
}

export interface ResetTenantWorkspaceUserPasswordRequest {
  temporaryPassword?: string;
}

export interface TenantWorkspaceUserPasswordResetResponse {
  userId: string;
  temporaryPassword: string;
}
