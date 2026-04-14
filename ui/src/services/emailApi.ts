import { apiClient } from './apiClient';
import { AddEmailMessageRequest, CreateEmailDraftRequest, CreateEmailThreadRequest, EmailDraft, EmailMessage, EmailThread, EmailThreadAiAnalysis, UpdateEmailDraftRequest } from '../types/email';
import { TaskSuggestion } from '../types/taskSuggestion';

export const getEmailThreads = async (bookingId?: string) => {
  const { data } = await apiClient.get<EmailThread[]>('/email-threads', {
    params: bookingId ? { bookingId } : undefined
  });
  return data;
};

export const getBookingEmailThreads = async (bookingId: string) => {
  const { data } = await apiClient.get<EmailThread[]>(`/bookings/${bookingId}/email-threads`);
  return data;
};

export const getEmailThread = async (threadId: string) => {
  const { data } = await apiClient.get<EmailThread>(`/email-threads/${threadId}`);
  return data;
};

export const createEmailThread = async (bookingId: string, payload: CreateEmailThreadRequest) => {
  const { data } = await apiClient.post<EmailThread>(`/bookings/${bookingId}/email-threads`, payload);
  return data;
};

export const addEmailMessage = async (threadId: string, payload: AddEmailMessageRequest) => {
  const { data } = await apiClient.post<EmailMessage>(`/email-threads/${threadId}/messages`, payload);
  return data;
};

export const analyzeEmailThread = async (threadId: string) => {
  const { data } = await apiClient.post<EmailThreadAiAnalysis>(`/email-threads/${threadId}/analyze`);
  return data;
};

export const generateTasksFromEmailThread = async (threadId: string) => {
  const { data } = await apiClient.post<TaskSuggestion[]>(`/email-threads/${threadId}/generate-tasks`);
  return data;
};

export const draftReply = async (threadId: string) => {
  const { data } = await apiClient.post<EmailDraft>(`/email-threads/${threadId}/suggest-reply`);
  return data;
};

export const createEmailDraft = async (payload: CreateEmailDraftRequest) => {
  const { data } = await apiClient.post<EmailDraft>('/email-drafts', payload);
  return data;
};

export const updateEmailDraft = async (draftId: string, payload: UpdateEmailDraftRequest) => {
  const { data } = await apiClient.patch<EmailDraft>(`/email-drafts/${draftId}`, payload);
  return data;
};

export const approveEmailDraft = async (draftId: string) => {
  const { data } = await apiClient.post<EmailDraft>(`/email-drafts/${draftId}/approve`);
  return data;
};

export const sendEmailDraft = async (draftId: string) => {
  const { data } = await apiClient.post<EmailDraft>(`/email-drafts/${draftId}/send`);
  return data;
};
