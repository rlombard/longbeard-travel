export type EmailIntegrationProviderType = 'Microsoft365' | 'Gmail' | 'Mailcow' | 'GenericImapSmtp' | 'SmtpDirect' | 'SendGrid';
export type EmailIntegrationAuthMethod = 'Basic' | 'OAuth2' | 'ApiKey';
export type EmailIntegrationStatus = 'Pending' | 'Active' | 'Error' | 'Disconnected';

export interface EmailConnectionSettingsRequest {
  incomingHost?: string;
  incomingPort?: number;
  incomingUseSsl?: boolean;
  incomingUsername?: string;
  incomingFolder?: string;
  outgoingHost?: string;
  outgoingPort?: number;
  outgoingUseSsl?: boolean;
  outgoingUsername?: string;
  fromAddressOverride?: string;
  replyToAddress?: string;
  microsoftTenantId?: string;
  sendGridFromAddress?: string;
  syncIntervalMinutes?: number;
}

export interface StartEmailProviderOAuthRequest {
  connectionName: string;
  providerType: EmailIntegrationProviderType;
  mailboxAddress: string;
  displayName?: string;
  allowSend: boolean;
  allowSync: boolean;
  isDefaultConnection: boolean;
  returnUrl?: string;
  settings: EmailConnectionSettingsRequest;
}

export interface EmailProviderConnectionListItem {
  id: string;
  connectionName: string;
  providerType: EmailIntegrationProviderType;
  authMethod: EmailIntegrationAuthMethod;
  status: EmailIntegrationStatus;
  mailboxAddress: string;
  displayName?: string | null;
  allowSend: boolean;
  allowSync: boolean;
  isDefaultConnection: boolean;
  lastSyncedAt?: string | null;
  lastSuccessfulSendAt?: string | null;
  lastTestedAt?: string | null;
  lastError?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface EmailOAuthStartResponse {
  connectionId: string;
  authorizationUrl: string;
  state: string;
}

export interface EmailConnectionTestResponse {
  connectionId: string;
  success: boolean;
  message: string;
  testedAt: string;
}

export interface EmailSyncResponse {
  connectionId: string;
  messagesProcessed: number;
  messagesImported: number;
  messagesSkipped: number;
  syncedAt: string;
  cursorPreview?: string | null;
}

export interface DisconnectEmailConnectionResponse {
  connectionId: string;
  status: EmailIntegrationStatus;
}
