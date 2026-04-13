import axios from 'axios';
import { getAccessToken } from '../auth/keycloak';
import { appEnv } from '../config/env';

export const apiClient = axios.create({
  baseURL: appEnv.apiBaseUrl
});

apiClient.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
