import { apiClient } from './apiClient';
import {
  DisconnectEmailConnectionResponse,
  EmailConnectionTestResponse,
  EmailOAuthStartResponse,
  EmailProviderConnectionListItem,
  EmailSyncResponse,
  StartEmailProviderOAuthRequest
} from '../types/emailIntegration';

export const getEmailConnections = async () => {
  const { data } = await apiClient.get<EmailProviderConnectionListItem[]>('/email-integrations/connections');
  return data;
};

export const startEmailOAuth = async (payload: StartEmailProviderOAuthRequest) => {
  const { data } = await apiClient.post<EmailOAuthStartResponse>('/email-integrations/oauth/start', payload);
  return data;
};

export const testEmailConnection = async (connectionId: string) => {
  const { data } = await apiClient.post<EmailConnectionTestResponse>(`/email-integrations/connections/${connectionId}/test`);
  return data;
};

export const syncEmailConnection = async (connectionId: string) => {
  const { data } = await apiClient.post<EmailSyncResponse>(`/email-integrations/connections/${connectionId}/sync`);
  return data;
};

export const disconnectEmailConnection = async (connectionId: string) => {
  const { data } = await apiClient.delete<DisconnectEmailConnectionResponse>(`/email-integrations/connections/${connectionId}`);
  return data;
};
