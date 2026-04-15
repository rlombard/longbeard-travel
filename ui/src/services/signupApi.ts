import { apiClient } from './apiClient';
import {
  SignupAdminSetupRequest,
  SignupBootstrap,
  SignupEmailUpdateRequest,
  SignupOrganizationRequest,
  SignupPlan,
  SignupPlanSelectionRequest,
  SignupSessionEnvelope,
  SignupSessionSummary,
  SignupTermsAcceptanceRequest,
  SignupVerifyEmailRequest
} from '../types/signup';

const withSignupToken = (signupAccessToken?: string | null) => ({
  headers: signupAccessToken ? { 'X-Signup-Access-Token': signupAccessToken } : undefined
});

export const getSignupBootstrap = async () => {
  const { data } = await apiClient.get<SignupBootstrap>('/signup/bootstrap');
  return data;
};

export const getSignupPlans = async () => {
  const { data } = await apiClient.get<SignupPlan[]>('/signup/plans');
  return data;
};

export const startSignupSession = async () => {
  const { data } = await apiClient.post<SignupSessionEnvelope>('/signup/sessions');
  return data;
};

export const getSignupSession = async (sessionId: string, signupAccessToken: string) => {
  const { data } = await apiClient.get<SignupSessionEnvelope>(`/signup/sessions/${sessionId}`, withSignupToken(signupAccessToken));
  return data;
};

export const saveSignupEmail = async (sessionId: string, signupAccessToken: string, payload: SignupEmailUpdateRequest) => {
  const { data } = await apiClient.put<SignupSessionEnvelope>(`/signup/sessions/${sessionId}/email`, payload, withSignupToken(signupAccessToken));
  return data;
};

export const resendSignupVerification = async (sessionId: string, signupAccessToken: string) => {
  const { data } = await apiClient.post<SignupSessionEnvelope>(`/signup/sessions/${sessionId}/email/resend`, undefined, withSignupToken(signupAccessToken));
  return data;
};

export const verifySignupEmail = async (sessionId: string, payload: SignupVerifyEmailRequest) => {
  const { data } = await apiClient.post<SignupSessionEnvelope>(`/signup/sessions/${sessionId}/email/verify`, payload);
  return data;
};

export const saveSignupOrganization = async (sessionId: string, signupAccessToken: string, payload: SignupOrganizationRequest) => {
  const { data } = await apiClient.put<SignupSessionEnvelope>(`/signup/sessions/${sessionId}/organization`, payload, withSignupToken(signupAccessToken));
  return data;
};

export const selectSignupPlan = async (sessionId: string, signupAccessToken: string, payload: SignupPlanSelectionRequest) => {
  const { data } = await apiClient.put<SignupSessionEnvelope>(`/signup/sessions/${sessionId}/plan`, payload, withSignupToken(signupAccessToken));
  return data;
};

export const acceptSignupTerms = async (sessionId: string, signupAccessToken: string, payload: SignupTermsAcceptanceRequest) => {
  const { data } = await apiClient.put<SignupSessionEnvelope>(`/signup/sessions/${sessionId}/terms`, payload, withSignupToken(signupAccessToken));
  return data;
};

export const createSignupBillingIntent = async (sessionId: string, signupAccessToken: string) => {
  const { data } = await apiClient.post<SignupSessionEnvelope>(`/signup/sessions/${sessionId}/billing-intent`, undefined, withSignupToken(signupAccessToken));
  return data;
};

export const confirmSignupTestPayment = async (sessionId: string, signupAccessToken: string) => {
  const { data } = await apiClient.post<SignupSessionEnvelope>(`/signup/sessions/${sessionId}/billing-intent/test-confirm`, undefined, withSignupToken(signupAccessToken));
  return data;
};

export const saveSignupAdmin = async (sessionId: string, signupAccessToken: string, payload: SignupAdminSetupRequest) => {
  const { data } = await apiClient.put<SignupSessionEnvelope>(`/signup/sessions/${sessionId}/admin`, payload, withSignupToken(signupAccessToken));
  return data;
};

export const provisionSignupSession = async (sessionId: string, signupAccessToken: string) => {
  const { data } = await apiClient.post<SignupSessionEnvelope>(`/signup/sessions/${sessionId}/provision`, undefined, withSignupToken(signupAccessToken));
  return data;
};

export const cancelSignupSession = async (sessionId: string, signupAccessToken: string) => {
  await apiClient.post(`/signup/sessions/${sessionId}/cancel`, undefined, withSignupToken(signupAccessToken));
};

export const getPlatformSignupSessions = async () => {
  const { data } = await apiClient.get<SignupSessionSummary[]>('/platform/signup-sessions');
  return data;
};

export const retryPlatformSignupSession = async (sessionId: string) => {
  const { data } = await apiClient.post<SignupSessionEnvelope>(`/platform/signup-sessions/${sessionId}/retry`);
  return data;
};

export const confirmPlatformSignupBilling = async (sessionId: string) => {
  const { data } = await apiClient.post<SignupSessionEnvelope>(`/platform/signup-sessions/${sessionId}/confirm-billing`);
  return data;
};
