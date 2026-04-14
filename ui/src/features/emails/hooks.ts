import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { addEmailMessage, analyzeEmailThread, approveEmailDraft, createEmailDraft, createEmailThread, draftReply, generateTasksFromEmailThread, getBookingEmailThreads, getEmailThread, getEmailThreads, sendEmailDraft, updateEmailDraft } from '../../services/emailApi';
import { AddEmailMessageRequest, CreateEmailDraftRequest, CreateEmailThreadRequest, UpdateEmailDraftRequest } from '../../types/email';

export const useEmailThreads = (bookingId?: string) =>
  useQuery({
    queryKey: ['emailThreads', bookingId],
    queryFn: () => (bookingId ? getBookingEmailThreads(bookingId) : getEmailThreads()),
    enabled: bookingId === undefined || Boolean(bookingId)
  });

export const useEmailThread = (threadId?: string) =>
  useQuery({
    queryKey: ['emailThread', threadId],
    queryFn: () => getEmailThread(threadId!),
    enabled: Boolean(threadId)
  });

const invalidateEmailQueries = (queryClient: ReturnType<typeof useQueryClient>, bookingId?: string) => {
  queryClient.invalidateQueries({ queryKey: ['emailThreads'] });
  queryClient.invalidateQueries({ queryKey: ['emailThreads', bookingId] });
  queryClient.invalidateQueries({ queryKey: ['emailThread'] });
  queryClient.invalidateQueries({ queryKey: ['taskSuggestions'] });
};

export const useCreateEmailThread = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ bookingId, payload }: { bookingId: string; payload: CreateEmailThreadRequest }) => createEmailThread(bookingId, payload),
    onSuccess: (thread) => invalidateEmailQueries(queryClient, thread.relatedBookingId ?? thread.bookingId ?? undefined)
  });
};

export const useAddEmailMessage = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ threadId, payload }: { threadId: string; payload: AddEmailMessageRequest }) => addEmailMessage(threadId, payload),
    onSuccess: () => invalidateEmailQueries(queryClient)
  });
};

export const useAnalyzeEmailThread = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (threadId: string) => analyzeEmailThread(threadId),
    onSuccess: () => invalidateEmailQueries(queryClient)
  });
};

export const useGenerateTasksFromEmailThread = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (threadId: string) => generateTasksFromEmailThread(threadId),
    onSuccess: () => invalidateEmailQueries(queryClient)
  });
};

export const useDraftReply = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (threadId: string) => draftReply(threadId),
    onSuccess: () => invalidateEmailQueries(queryClient)
  });
};

export const useCreateEmailDraft = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateEmailDraftRequest) => createEmailDraft(payload),
    onSuccess: () => invalidateEmailQueries(queryClient)
  });
};

export const useUpdateEmailDraft = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateEmailDraftRequest }) => updateEmailDraft(id, payload),
    onSuccess: () => invalidateEmailQueries(queryClient)
  });
};

export const useApproveEmailDraft = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (draftId: string) => approveEmailDraft(draftId),
    onSuccess: () => invalidateEmailQueries(queryClient)
  });
};

export const useSendEmailDraft = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (draftId: string) => sendEmailDraft(draftId),
    onSuccess: () => invalidateEmailQueries(queryClient)
  });
};
