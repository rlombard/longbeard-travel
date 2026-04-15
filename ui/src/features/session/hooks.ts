import { useMutation, useQuery } from '@tanstack/react-query';
import { discoverTenant, getSessionBootstrap } from '../../services/sessionApi';

export const useSessionBootstrap = () =>
  useQuery({
    queryKey: ['session-bootstrap'],
    queryFn: getSessionBootstrap,
    staleTime: 30000
  });

export const useTenantDiscovery = () =>
  useMutation({
    mutationFn: discoverTenant
  });
