import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  acceptSignupTerms,
  cancelSignupSession,
  confirmPlatformSignupBilling,
  confirmSignupTestPayment,
  createSignupBillingIntent,
  getPlatformSignupSessions,
  getSignupBootstrap,
  getSignupPlans,
  getSignupSession,
  provisionSignupSession,
  resendSignupVerification,
  retryPlatformSignupSession,
  saveSignupAdmin,
  saveSignupEmail,
  saveSignupOrganization,
  selectSignupPlan,
  startSignupSession,
  verifySignupEmail
} from '../../services/signupApi';
import {
  SignupAdminSetupRequest,
  SignupEmailUpdateRequest,
  SignupOrganizationRequest,
  SignupPlanSelectionRequest,
  SignupTermsAcceptanceRequest,
  SignupVerifyEmailRequest
} from '../../types/signup';

export const useSignupBootstrap = () =>
  useQuery({
    queryKey: ['signup-bootstrap'],
    queryFn: getSignupBootstrap
  });

export const useSignupPlans = () =>
  useQuery({
    queryKey: ['signup-plans'],
    queryFn: getSignupPlans
  });

export const useSignupSession = (sessionId?: string, accessToken?: string) =>
  useQuery({
    queryKey: ['signup-session', sessionId],
    queryFn: () => getSignupSession(sessionId!, accessToken!),
    enabled: Boolean(sessionId && accessToken)
  });

export const useStartSignupSession = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: startSignupSession,
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useSaveSignupEmail = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, accessToken, payload }: { sessionId: string; accessToken: string; payload: SignupEmailUpdateRequest }) =>
      saveSignupEmail(sessionId, accessToken, payload),
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useResendSignupVerification = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, accessToken }: { sessionId: string; accessToken: string }) =>
      resendSignupVerification(sessionId, accessToken),
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useVerifySignupEmail = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, payload }: { sessionId: string; payload: SignupVerifyEmailRequest }) =>
      verifySignupEmail(sessionId, payload),
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useSaveSignupOrganization = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, accessToken, payload }: { sessionId: string; accessToken: string; payload: SignupOrganizationRequest }) =>
      saveSignupOrganization(sessionId, accessToken, payload),
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useSelectSignupPlan = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, accessToken, payload }: { sessionId: string; accessToken: string; payload: SignupPlanSelectionRequest }) =>
      selectSignupPlan(sessionId, accessToken, payload),
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useAcceptSignupTerms = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, accessToken, payload }: { sessionId: string; accessToken: string; payload: SignupTermsAcceptanceRequest }) =>
      acceptSignupTerms(sessionId, accessToken, payload),
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useCreateSignupBillingIntent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, accessToken }: { sessionId: string; accessToken: string }) =>
      createSignupBillingIntent(sessionId, accessToken),
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useConfirmSignupTestPayment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, accessToken }: { sessionId: string; accessToken: string }) =>
      confirmSignupTestPayment(sessionId, accessToken),
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useSaveSignupAdmin = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, accessToken, payload }: { sessionId: string; accessToken: string; payload: SignupAdminSetupRequest }) =>
      saveSignupAdmin(sessionId, accessToken, payload),
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useProvisionSignupSession = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, accessToken }: { sessionId: string; accessToken: string }) =>
      provisionSignupSession(sessionId, accessToken),
    onSuccess: (result) => {
      queryClient.setQueryData(['signup-session', result.session.id], result);
    }
  });
};

export const useCancelSignupSession = () =>
  useMutation({
    mutationFn: ({ sessionId, accessToken }: { sessionId: string; accessToken: string }) =>
      cancelSignupSession(sessionId, accessToken)
  });

export const usePlatformSignupSessions = () =>
  useQuery({
    queryKey: ['platform-signup-sessions'],
    queryFn: getPlatformSignupSessions
  });

export const useRetryPlatformSignupSession = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (sessionId: string) => retryPlatformSignupSession(sessionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platform-signup-sessions'] });
    }
  });
};

export const useConfirmPlatformSignupBilling = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (sessionId: string) => confirmPlatformSignupBilling(sessionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platform-signup-sessions'] });
    }
  });
};
