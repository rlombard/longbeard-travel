import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createTenantWorkspaceUser,
  getTenantAdminWorkspace,
  resetTenantWorkspaceUserPassword,
  updateTenantAdminWorkspace,
  updateTenantWorkspaceUser,
  upsertTenantAdminConfig
} from '../../services/tenantAdminApi';
import { disconnectEmailConnection, startEmailOAuth, syncEmailConnection, testEmailConnection } from '../../services/emailIntegrationApi';
import {
  CreateTenantWorkspaceUserRequest,
  ResetTenantWorkspaceUserPasswordRequest,
  UpdateTenantWorkspaceProfileRequest,
  UpdateTenantWorkspaceUserRequest
} from '../../types/tenantAdmin';
import { StartEmailProviderOAuthRequest } from '../../types/emailIntegration';
import { UpsertTenantConfigRequest } from '../../types/platform';

export const useTenantAdminWorkspace = () =>
  useQuery({
    queryKey: ['tenant-admin-workspace'],
    queryFn: getTenantAdminWorkspace
  });

export const useUpdateTenantAdminWorkspace = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: UpdateTenantWorkspaceProfileRequest) => updateTenantAdminWorkspace(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant-admin-workspace'] });
    }
  });
};

export const useUpsertTenantAdminConfig = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: UpsertTenantConfigRequest) => upsertTenantAdminConfig(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant-admin-workspace'] });
    }
  });
};

export const useCreateTenantWorkspaceUser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateTenantWorkspaceUserRequest) => createTenantWorkspaceUser(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant-admin-workspace'] });
    }
  });
};

export const useUpdateTenantWorkspaceUser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, payload }: { userId: string; payload: UpdateTenantWorkspaceUserRequest }) => updateTenantWorkspaceUser(userId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant-admin-workspace'] });
    }
  });
};

export const useResetTenantWorkspaceUserPassword = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, payload }: { userId: string; payload: ResetTenantWorkspaceUserPasswordRequest }) => resetTenantWorkspaceUserPassword(userId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant-admin-workspace'] });
    }
  });
};

export const useStartTenantEmailOAuth = () =>
  useMutation({
    mutationFn: (payload: StartEmailProviderOAuthRequest) => startEmailOAuth(payload)
  });

export const useTestTenantEmailConnection = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (connectionId: string) => testEmailConnection(connectionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant-admin-workspace'] });
    }
  });
};

export const useSyncTenantEmailConnection = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (connectionId: string) => syncEmailConnection(connectionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant-admin-workspace'] });
    }
  });
};

export const useDisconnectTenantEmailConnection = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (connectionId: string) => disconnectEmailConnection(connectionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant-admin-workspace'] });
    }
  });
};
