import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createAdminUser,
  getAdminAccessCatalog,
  getAdminUser,
  resetAdminUserPassword,
  searchAdminUsers,
  updateAdminUser,
  updateAdminUserRoles
} from '../../services/adminUsersApi';
import {
  AdminResetPasswordRequest,
  AdminUserCreateRequest,
  AdminUserRoleUpdateRequest,
  AdminUserSearchQuery,
  AdminUserUpdateRequest
} from '../../types/adminUser';

export const useAdminUsers = (query: AdminUserSearchQuery) =>
  useQuery({
    queryKey: ['admin-users', query],
    queryFn: () => searchAdminUsers(query)
  });

export const useAdminUser = (userId?: string) =>
  useQuery({
    queryKey: ['admin-user', userId],
    queryFn: () => getAdminUser(userId!),
    enabled: Boolean(userId)
  });

export const useAdminAccessCatalog = () =>
  useQuery({
    queryKey: ['admin-user-access-catalog'],
    queryFn: getAdminAccessCatalog
  });

export const useCreateAdminUser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: AdminUserCreateRequest) => createAdminUser(payload),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      queryClient.setQueryData(['admin-user', result.user.id], result.user);
    }
  });
};

export const useUpdateAdminUser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, payload }: { userId: string; payload: AdminUserUpdateRequest }) => updateAdminUser(userId, payload),
    onSuccess: (user) => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      queryClient.setQueryData(['admin-user', user.id], user);
    }
  });
};

export const useUpdateAdminUserRoles = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, payload }: { userId: string; payload: AdminUserRoleUpdateRequest }) => updateAdminUserRoles(userId, payload),
    onSuccess: (user) => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      queryClient.setQueryData(['admin-user', user.id], user);
    }
  });
};

export const useResetAdminUserPassword = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, payload }: { userId: string; payload: AdminResetPasswordRequest }) => resetAdminUserPassword(userId, payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['admin-user', variables.userId] });
    }
  });
};
