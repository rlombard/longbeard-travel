import { apiClient } from './apiClient';
import { OperationalTask, OperationalTaskStatus, TaskRequest, UpdateTaskDetailsRequest } from '../types/task';

export const getTasks = async (bookingId?: string) => {
  const { data } = await apiClient.get<OperationalTask[]>('/tasks', {
    params: bookingId ? { bookingId } : undefined
  });

  return data;
};

export const getMyTasks = async () => {
  const { data } = await apiClient.get<OperationalTask[]>('/tasks/my');
  return data;
};

export const createTask = async (payload: TaskRequest) => {
  const { data } = await apiClient.post<OperationalTask>('/tasks', payload);
  return data;
};

export const updateTaskStatus = async (id: string, status: OperationalTaskStatus) => {
  const { data } = await apiClient.patch<OperationalTask>(`/tasks/${id}/status`, { status });
  return data;
};

export const updateTaskDetails = async (id: string, payload: UpdateTaskDetailsRequest) => {
  const { data } = await apiClient.patch<OperationalTask>(`/tasks/${id}`, payload);
  return data;
};

export const assignTask = async (id: string, userId: string) => {
  const { data } = await apiClient.patch<OperationalTask>(`/tasks/${id}/assign`, { userId });
  return data;
};

export const deleteTask = async (id: string) => {
  await apiClient.delete(`/tasks/${id}`);
};
