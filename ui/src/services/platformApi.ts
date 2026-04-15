import { apiClient } from './apiClient';
import {
  AssignTenantUserRequest,
  CreateTenantRequest,
  TenantConfigEntry,
  TenantDetail,
  TenantOnboarding,
  TenantSummary,
  TenantUserMembership,
  UpdateTenantOnboardingRequest,
  UpsertTenantConfigRequest
} from '../types/platform';

export const getTenants = async () => {
  const { data } = await apiClient.get<TenantSummary[]>('/platform/tenants');
  return data;
};

export const getTenant = async (tenantId: string) => {
  const { data } = await apiClient.get<TenantDetail>(`/platform/tenants/${tenantId}`);
  return data;
};

export const createTenant = async (payload: CreateTenantRequest) => {
  const { data } = await apiClient.post<TenantDetail>('/platform/tenants', payload);
  return data;
};

export const updateTenantOnboarding = async (tenantId: string, payload: UpdateTenantOnboardingRequest) => {
  const { data } = await apiClient.patch<TenantOnboarding>(`/platform/tenants/${tenantId}/onboarding`, payload);
  return data;
};

export const upsertTenantConfig = async (tenantId: string, payload: UpsertTenantConfigRequest) => {
  const { data } = await apiClient.put<TenantConfigEntry>(`/platform/tenants/${tenantId}/config`, payload);
  return data;
};

export const assignTenantUser = async (tenantId: string, payload: AssignTenantUserRequest) => {
  const { data } = await apiClient.post<TenantUserMembership>(`/platform/tenants/${tenantId}/users`, payload);
  return data;
};
