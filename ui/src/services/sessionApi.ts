import { apiClient } from './apiClient';
import { DiscoverTenantRequest, SessionBootstrap, TenantLoginDiscovery } from '../types/session';

export const getSessionBootstrap = async () => {
  const { data } = await apiClient.get<SessionBootstrap>('/session/bootstrap');
  return data;
};

export const discoverTenant = async (payload: DiscoverTenantRequest) => {
  const { data } = await apiClient.post<TenantLoginDiscovery>('/session/discover-tenant', payload);
  return data;
};
