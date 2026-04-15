import { apiClient } from './apiClient';
import {
  AdminAccessCatalog,
  AdminResetPasswordRequest,
  AdminResetPasswordResponse,
  AdminUser,
  AdminUserCreateRequest,
  AdminUserCreateResponse,
  AdminUserListItem,
  AdminUserRoleUpdateRequest,
  AdminUserSearchQuery,
  AdminUserUpdateRequest
} from '../types/adminUser';

const buildParams = (query: AdminUserSearchQuery) => {
  const params = new URLSearchParams();

  if (query.search) {
    params.set('search', query.search);
  }

  if (typeof query.enabled === 'boolean') {
    params.set('enabled', String(query.enabled));
  }

  return params;
};

export const searchAdminUsers = async (query: AdminUserSearchQuery = {}): Promise<AdminUserListItem[]> => {
  const { data } = await apiClient.get<AdminUserListItem[]>('/admin/users', { params: buildParams(query) });
  return Array.isArray(data) ? data : [];
};

export const getAdminUser = async (userId: string): Promise<AdminUser> => {
  const { data } = await apiClient.get<AdminUser>(`/admin/users/${userId}`);
  return data;
};

export const createAdminUser = async (payload: AdminUserCreateRequest): Promise<AdminUserCreateResponse> => {
  const { data } = await apiClient.post<AdminUserCreateResponse>('/admin/users', payload);
  return data;
};

export const updateAdminUser = async (userId: string, payload: AdminUserUpdateRequest): Promise<AdminUser> => {
  const { data } = await apiClient.patch<AdminUser>(`/admin/users/${userId}`, payload);
  return data;
};

export const resetAdminUserPassword = async (userId: string, payload: AdminResetPasswordRequest): Promise<AdminResetPasswordResponse> => {
  const { data } = await apiClient.post<AdminResetPasswordResponse>(`/admin/users/${userId}/reset-password`, payload);
  return data;
};

export const updateAdminUserRoles = async (userId: string, payload: AdminUserRoleUpdateRequest): Promise<AdminUser> => {
  const { data } = await apiClient.put<AdminUser>(`/admin/users/${userId}/roles`, payload);
  return data;
};

export const getAdminAccessCatalog = async (): Promise<AdminAccessCatalog> => {
  const { data } = await apiClient.get<AdminAccessCatalog>('/admin/roles');
  return data;
};
