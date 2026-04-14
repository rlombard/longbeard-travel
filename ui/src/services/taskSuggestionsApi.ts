import { apiClient } from './apiClient';
import { OperationalTask } from '../types/task';
import { TaskSuggestion } from '../types/taskSuggestion';

export const getTaskSuggestions = async (bookingId: string) => {
  const { data } = await apiClient.get<TaskSuggestion[]>(`/bookings/${bookingId}/task-suggestions`);
  return data;
};

export const generateTaskSuggestions = async (bookingId: string) => {
  const { data } = await apiClient.post<TaskSuggestion[]>(`/bookings/${bookingId}/task-suggestions/generate`);
  return data;
};

export const regenerateTaskSuggestions = async (bookingId: string) => {
  const { data } = await apiClient.post<TaskSuggestion[]>(`/bookings/${bookingId}/task-suggestions/regenerate`);
  return data;
};

export const acceptTaskSuggestion = async (id: string, assignedToUserId: string) => {
  const { data } = await apiClient.post<OperationalTask>(`/task-suggestions/${id}/accept`, { assignedToUserId });
  return data;
};

export const rejectTaskSuggestion = async (id: string) => {
  const { data } = await apiClient.post<TaskSuggestion>(`/task-suggestions/${id}/reject`);
  return data;
};
