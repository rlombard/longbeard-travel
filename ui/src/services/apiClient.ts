import axios from 'axios';
import { getAccessToken } from '../auth/keycloak';
import { appEnv } from '../config/env';
import { getActiveTenantId } from './tenantScope';

export const apiClient = axios.create({
  baseURL: appEnv.bffBaseUrl
});

apiClient.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  const tenantId = getActiveTenantId();
  if (tenantId) {
    config.headers['X-Tenant-Id'] = tenantId;
  }

  return config;
});
