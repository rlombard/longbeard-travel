import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { assignTenantUser, createTenant, getTenant, getTenants, updateTenantOnboarding, upsertTenantConfig } from '../../services/platformApi';
import { AssignTenantUserRequest, CreateTenantRequest, UpdateTenantOnboardingRequest, UpsertTenantConfigRequest } from '../../types/platform';

export const useTenants = () =>
  useQuery({
    queryKey: ['platform-tenants'],
    queryFn: getTenants
  });

export const useTenant = (tenantId?: string) =>
  useQuery({
    queryKey: ['platform-tenant', tenantId],
    queryFn: () => getTenant(tenantId!),
    enabled: Boolean(tenantId)
  });

export const useCreateTenant = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateTenantRequest) => createTenant(payload),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['platform-tenants'] });
      queryClient.setQueryData(['platform-tenant', result.tenant.id], result);
    }
  });
};

export const useUpdateTenantOnboarding = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ tenantId, payload }: { tenantId: string; payload: UpdateTenantOnboardingRequest }) => updateTenantOnboarding(tenantId, payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['platform-tenant', variables.tenantId] });
      queryClient.invalidateQueries({ queryKey: ['platform-tenants'] });
    }
  });
};

export const useUpsertTenantConfig = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ tenantId, payload }: { tenantId: string; payload: UpsertTenantConfigRequest }) => upsertTenantConfig(tenantId, payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['platform-tenant', variables.tenantId] });
    }
  });
};

export const useAssignTenantUser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ tenantId, payload }: { tenantId: string; payload: AssignTenantUserRequest }) => assignTenantUser(tenantId, payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['platform-tenant', variables.tenantId] });
      queryClient.invalidateQueries({ queryKey: ['platform-tenants'] });
    }
  });
};
