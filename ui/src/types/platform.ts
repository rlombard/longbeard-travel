export type TenantStatus = 'Provisioning' | 'Active' | 'Suspended' | 'Disabled';
export type LicenseStatus = 'Trial' | 'Active' | 'Suspended' | 'Expired' | 'Cancelled';
export type BillingMode = 'Standalone' | 'Trial' | 'Invoice' | 'ExternalSubscription' | 'Free';
export type OnboardingStatus = 'NotStarted' | 'InProgress' | 'Completed' | 'Blocked';
export type TenantUserRole = 'PlatformAdmin' | 'TenantAdmin' | 'Operator';
export type TenantUserStatus = 'Invited' | 'Active' | 'Disabled';
export type IdentityIsolationMode = 'SharedRealm' | 'RealmPerTenant';
export type IdentityProvisioningStatus = 'Pending' | 'Ready' | 'Failed';
export type MonetizationTransactionType = 'UsageCharge' | 'SubscriptionCharge' | 'Credit' | 'Adjustment';
export type MonetizationTransactionStatus = 'Pending' | 'Posted' | 'Failed' | 'Voided';

export interface TenantSummary {
  id: string;
  slug: string;
  name: string;
  billingEmail?: string | null;
  defaultCurrency: string;
  timeZone: string;
  status: TenantStatus;
  isStandaloneTenant: boolean;
  licensePlanCode?: string | null;
  licenseStatus?: LicenseStatus | null;
  onboardingStatus: OnboardingStatus;
  currentOnboardingStep: string;
  activeUsers: number;
  connectedEmailAccounts: number;
  createdAt: string;
  updatedAt: string;
}

export interface TenantLicense {
  id: string;
  tenantId: string;
  licensePlanId: string;
  planCode: string;
  planName: string;
  status: LicenseStatus;
  billingMode: BillingMode;
  startsAt: string;
  trialEndsAt?: string | null;
  endsAt?: string | null;
  includedFeatures: string[];
  limits: Record<string, number>;
  currentUsage: Record<string, number>;
}

export interface TenantOnboarding {
  tenantId: string;
  status: OnboardingStatus;
  currentStep: string;
  completedSteps: string[];
  lastError?: string | null;
  startedAt: string;
  completedAt?: string | null;
  updatedAt: string;
}

export interface TenantUserMembership {
  id: string;
  tenantId: string;
  userId: string;
  email: string;
  displayName: string;
  role: TenantUserRole;
  status: TenantUserStatus;
  invitedAt: string;
  joinedAt?: string | null;
  lastSeenAt?: string | null;
}

export interface TenantConfigEntry {
  id: string;
  tenantId: string;
  configDomain: string;
  configKey: string;
  jsonValue: string;
  isEncrypted: boolean;
  updatedByUserId: string;
  updatedAt: string;
}

export interface TenantIdentity {
  id: string;
  tenantId: string;
  isolationMode: IdentityIsolationMode;
  provisioningStatus: IdentityProvisioningStatus;
  realmName: string;
  clientId?: string | null;
  issuerUrl?: string | null;
  lastError?: string | null;
  updatedAt: string;
}

export interface UsageMetricSummary {
  metricKey: string;
  category: string;
  quantity: number;
  unit: string;
  isBillable: boolean;
}

export interface MonetizationTransaction {
  id: string;
  tenantId: string;
  transactionType: MonetizationTransactionType;
  status: MonetizationTransactionStatus;
  amount: number;
  currency: string;
  periodStart: string;
  periodEnd: string;
  externalReference?: string | null;
  createdAt: string;
}

export interface AuditEvent {
  id: string;
  tenantId?: string | null;
  scopeType: string;
  action: string;
  result: string;
  actorUserId?: string | null;
  actorDisplayName?: string | null;
  targetEntityType?: string | null;
  targetEntityId?: string | null;
  metadataJson?: string | null;
  createdAt: string;
}

export interface TenantDetail {
  tenant: TenantSummary;
  license?: TenantLicense | null;
  onboarding?: TenantOnboarding | null;
  users: TenantUserMembership[];
  configEntries: TenantConfigEntry[];
  identityMappings: TenantIdentity[];
  usage: UsageMetricSummary[];
  transactions: MonetizationTransaction[];
  auditEvents: AuditEvent[];
}

export interface BootstrapTenantAdminRequest {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  temporaryPassword?: string;
}

export interface CreateTenantRequest {
  slug: string;
  name: string;
  legalName?: string | null;
  billingEmail?: string | null;
  defaultCurrency: string;
  timeZone: string;
  licensePlanCode: string;
  isStandaloneTenant: boolean;
  bootstrapAdmin?: BootstrapTenantAdminRequest | null;
}

export interface UpdateTenantOnboardingRequest {
  step: string;
  markCompleted: boolean;
  completeOnboarding: boolean;
  payloadJson?: string | null;
  error?: string | null;
}

export interface UpsertTenantConfigRequest {
  configDomain: string;
  configKey: string;
  jsonValue: string;
  isEncrypted: boolean;
}

export interface AssignTenantUserRequest {
  userId: string;
  email: string;
  displayName: string;
  role: TenantUserRole;
}
