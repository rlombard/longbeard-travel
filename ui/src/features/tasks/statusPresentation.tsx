import { OperationalTaskStatus } from '../../types/task';

const statusClasses: Record<OperationalTaskStatus, string> = {
  ToDo: 'bg-slate-200 text-slate-800',
  Waiting: 'bg-orange-100 text-orange-800',
  FollowUp: 'bg-sky-100 text-sky-800',
  Blocked: 'bg-red-100 text-red-800',
  Done: 'bg-emerald-100 text-emerald-800'
};

const statusLabels: Record<OperationalTaskStatus, string> = {
  ToDo: 'To Do',
  Waiting: 'Waiting',
  FollowUp: 'Follow Up',
  Blocked: 'Blocked',
  Done: 'Done'
};

export const taskStatusOptions: { value: OperationalTaskStatus; label: string }[] = [
  { value: 'ToDo', label: 'To Do' },
  { value: 'Waiting', label: 'Waiting' },
  { value: 'FollowUp', label: 'Follow Up' },
  { value: 'Blocked', label: 'Blocked' },
  { value: 'Done', label: 'Done' }
];

export const TaskStatusBadge = ({ status }: { status: OperationalTaskStatus }) => (
  <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${statusClasses[status]}`}>
    {statusLabels[status]}
  </span>
);

export const getTaskStatusLabel = (status: OperationalTaskStatus) => statusLabels[status];
