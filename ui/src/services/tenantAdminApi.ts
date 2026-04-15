import { apiClient } from './apiClient';
import {
  CreateTenantWorkspaceUserRequest,
  ResetTenantWorkspaceUserPasswordRequest,
  TenantAdminWorkspace,
  UpdateTenantWorkspaceProfileRequest,
  TenantWorkspaceUser,
  TenantWorkspaceUserCreateResponse,
  TenantWorkspaceUserPasswordResetResponse,
  UpdateTenantWorkspaceUserRequest
} from '../types/tenantAdmin';
import { TenantConfigEntry, UpsertTenantConfigRequest } from '../types/platform';

export const getTenantAdminWorkspace = async () => {
  const { data } = await apiClient.get<TenantAdminWorkspace>('/tenant-admin/workspace');
  return data;
};

export const updateTenantAdminWorkspace = async (payload: UpdateTenantWorkspaceProfileRequest) => {
  const { data } = await apiClient.put<TenantAdminWorkspace>('/tenant-admin/workspace', payload);
  return data;
};

export const upsertTenantAdminConfig = async (payload: UpsertTenantConfigRequest) => {
  const { data } = await apiClient.put<TenantConfigEntry>('/tenant-admin/config', payload);
  return data;
};

export const createTenantWorkspaceUser = async (payload: CreateTenantWorkspaceUserRequest) => {
  const { data } = await apiClient.post<TenantWorkspaceUserCreateResponse>('/tenant-admin/users', payload);
  return data;
};

export const updateTenantWorkspaceUser = async (userId: string, payload: UpdateTenantWorkspaceUserRequest) => {
  const { data } = await apiClient.put<TenantWorkspaceUser>(`/tenant-admin/users/${userId}`, payload);
  return data;
};

export const resetTenantWorkspaceUserPassword = async (userId: string, payload: ResetTenantWorkspaceUserPasswordRequest) => {
  const { data } = await apiClient.post<TenantWorkspaceUserPasswordResetResponse>(`/tenant-admin/users/${userId}/reset-password`, payload);
  return data;
};
