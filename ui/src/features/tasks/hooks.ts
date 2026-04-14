import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { assignTask, createTask, deleteTask, getMyTasks, getTasks, updateTaskDetails, updateTaskStatus } from '../../services/tasksApi';
import { OperationalTaskStatus, TaskRequest, UpdateTaskDetailsRequest } from '../../types/task';

export const useTasks = (bookingId?: string) =>
  useQuery({
    queryKey: ['tasks', bookingId ?? 'all'],
    queryFn: () => getTasks(bookingId)
  });

export const useMyTasks = () =>
  useQuery({
    queryKey: ['myTasks'],
    queryFn: getMyTasks
  });

const invalidateTaskQueries = (queryClient: ReturnType<typeof useQueryClient>) => {
  queryClient.invalidateQueries({ queryKey: ['tasks'] });
  queryClient.invalidateQueries({ queryKey: ['myTasks'] });
};

export const useCreateTask = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: TaskRequest) => createTask(payload),
    onSuccess: () => invalidateTaskQueries(queryClient)
  });
};

export const useUpdateTaskStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: OperationalTaskStatus }) => updateTaskStatus(id, status),
    onSuccess: () => invalidateTaskQueries(queryClient)
  });
};

export const useUpdateTaskDetails = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateTaskDetailsRequest }) => updateTaskDetails(id, payload),
    onSuccess: () => invalidateTaskQueries(queryClient)
  });
};

export const useAssignTask = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, userId }: { id: string; userId: string }) => assignTask(id, userId),
    onSuccess: () => invalidateTaskQueries(queryClient)
  });
};

export const useDeleteTask = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteTask(id),
    onSuccess: () => invalidateTaskQueries(queryClient)
  });
};
