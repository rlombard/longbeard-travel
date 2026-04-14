import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { acceptTaskSuggestion, generateTaskSuggestions, getTaskSuggestions, regenerateTaskSuggestions, rejectTaskSuggestion } from '../../services/taskSuggestionsApi';

export const useTaskSuggestions = (bookingId?: string) =>
  useQuery({
    queryKey: ['taskSuggestions', bookingId],
    queryFn: () => getTaskSuggestions(bookingId!),
    enabled: Boolean(bookingId)
  });

const invalidateBookingSuggestionQueries = (queryClient: ReturnType<typeof useQueryClient>, bookingId?: string) => {
  queryClient.invalidateQueries({ queryKey: ['taskSuggestions', bookingId] });
  queryClient.invalidateQueries({ queryKey: ['tasks'] });
};

export const useGenerateTaskSuggestions = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (bookingId: string) => generateTaskSuggestions(bookingId),
    onSuccess: (_, bookingId) => invalidateBookingSuggestionQueries(queryClient, bookingId)
  });
};

export const useRegenerateTaskSuggestions = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (bookingId: string) => regenerateTaskSuggestions(bookingId),
    onSuccess: (_, bookingId) => invalidateBookingSuggestionQueries(queryClient, bookingId)
  });
};

export const useAcceptTaskSuggestion = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, assignedToUserId }: { id: string; assignedToUserId: string }) => acceptTaskSuggestion(id, assignedToUserId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taskSuggestions'] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['myTasks'] });
    }
  });
};

export const useRejectTaskSuggestion = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => rejectTaskSuggestion(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['taskSuggestions'] })
  });
};
