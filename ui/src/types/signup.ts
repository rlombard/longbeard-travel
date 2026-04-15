export type LicenseSignupKind = 'Hidden' | 'Free' | 'Trial' | 'Paid';
export type SignupSessionStatus =
  | 'Draft'
  | 'EmailPending'
  | 'EmailVerified'
  | 'PlanSelected'
  | 'PaymentPending'
  | 'PaymentConfirmed'
  | 'TenantProvisioning'
  | 'IdentityProvisioning'
  | 'AdminBootstrap'
  | 'ConfigSeeded'
  | 'Active'
  | 'Failed'
  | 'Cancelled'
  | 'Expired';
export type SignupBillingStatus = 'NotRequired' | 'Pending' | 'Confirmed' | 'Failed' | 'Cancelled' | 'RequiresManualReview';

export interface SignupBootstrap {
  enabled: boolean;
  disabledReason: string;
  allowTestPaymentConfirmation: boolean;
  supportEmail: string;
}

export interface SignupPlan {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  signupKind: LicenseSignupKind;
  trialDays: number;
  monthlyPrice: number;
  currency: string;
  requiresTermsAcceptance: boolean;
  includedFeatures: string[];
  limits: Record<string, number>;
}

export interface SignupSession {
  id: string;
  status: SignupSessionStatus;
  currentStep: string;
  email?: string | null;
  emailVerified: boolean;
  emailVerifiedAt?: string | null;
  verificationSentAt?: string | null;
  verificationExpiresAt?: string | null;
  selectedPlanId?: string | null;
  selectedPlanCode?: string | null;
  billingStatus: SignupBillingStatus;
  billingIntentId?: string | null;
  tenantId?: string | null;
  termsAccepted: boolean;
  organizationName?: string | null;
  organizationLegalName?: string | null;
  tenantSlug?: string | null;
  billingEmail?: string | null;
  defaultCurrency?: string | null;
  timeZone?: string | null;
  adminEmail?: string | null;
  adminFirstName?: string | null;
  adminLastName?: string | null;
  adminUsername?: string | null;
  temporaryPassword?: string | null;
  launchPath?: string | null;
  lastError?: string | null;
  expiresAt: string;
  createdAt: string;
  updatedAt: string;
}

export interface SignupSessionEnvelope {
  session: SignupSession;
  accessToken?: string | null;
  debugVerificationToken?: string | null;
}

export interface SignupSessionSummary {
  id: string;
  status: SignupSessionStatus;
  currentStep: string;
  email?: string | null;
  organizationName?: string | null;
  tenantSlug?: string | null;
  selectedPlanCode?: string | null;
  billingStatus: SignupBillingStatus;
  tenantId?: string | null;
  lastError?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface SignupEmailUpdateRequest {
  email: string;
}

export interface SignupVerifyEmailRequest {
  token: string;
}

export interface SignupOrganizationRequest {
  organizationName: string;
  organizationLegalName?: string;
  tenantSlug: string;
  billingEmail: string;
  defaultCurrency: string;
  timeZone: string;
}

export interface SignupPlanSelectionRequest {
  planCode: string;
}

export interface SignupTermsAcceptanceRequest {
  accepted: boolean;
}

export interface SignupAdminSetupRequest {
  email: string;
  firstName: string;
  lastName: string;
  username: string;
}
