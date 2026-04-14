import { useMemo, useState } from 'react';
import { Card } from '../../components/Card';
import { Button } from '../../components/Button';
import { useBookings } from '../bookings/hooks';
import { TaskForm, TaskFormValues } from './TaskForm';
import { useAssignTask, useCreateTask, useDeleteTask, useMyTasks, useTasks, useUpdateTaskDetails, useUpdateTaskStatus } from './hooks';
import { OperationalTask, OperationalTaskStatus } from '../../types/task';
import { getCurrentUserId } from '../../auth/keycloak';
import { TaskStatusBadge, getTaskStatusLabel, taskStatusOptions } from './statusPresentation';

const viewStorageKey = 'tourops.operations.view';
const formatDate = (value?: string | null) => (value ? new Date(value).toLocaleString() : 'No due date');
const toDateInput = (value?: string | null) => (value ? new Date(value).toISOString().slice(0, 16) : '');
const shortId = (value?: string | null) => (value ? `${value.slice(0, 8)}...` : 'n/a');

const buildTaskContext = (task: OperationalTask) => {
  if (task.bookingItemId) {
    const productLabel = task.productName || task.bookingItemId;
    const supplierLabel = task.supplierName ? `Supplier: ${task.supplierName}` : `Booking Item: ${shortId(task.bookingItemId)}`;
    return { primary: productLabel, secondary: supplierLabel, bookingLabel: `Booking ${shortId(task.relatedBookingId)}` };
  }

  return {
    primary: `Booking ${shortId(task.relatedBookingId ?? task.bookingId)}`,
    secondary: 'Booking-level operational task',
    bookingLabel: `Booking ${shortId(task.relatedBookingId ?? task.bookingId)}`
  };
};

const defaultTaskValues = (currentUserId: string | null): TaskFormValues => ({
  bookingId: '',
  bookingItemId: '',
  title: '',
  description: '',
  dueDate: '',
  assignedToUserId: currentUserId ?? ''
});

const TaskCard = ({
  task,
  currentUserId,
  onStatusChange,
  onDelete,
  onSave,
  onAssign,
  isBusy
}: {
  task: OperationalTask;
  currentUserId: string | null;
  onStatusChange: (taskId: string, status: OperationalTaskStatus) => void;
  onDelete: (taskId: string) => void;
  onSave: (task: OperationalTask, values: TaskFormValues) => Promise<void>;
  onAssign: (taskId: string, userId: string) => void;
  isBusy: boolean;
}) => {
  const [isEditing, setIsEditing] = useState(false);
  const context = buildTaskContext(task);

  return (
    <div
      draggable={!isBusy}
      onDragStart={(event) => {
        event.dataTransfer.setData('text/task-id', task.id);
        event.dataTransfer.effectAllowed = 'move';
      }}
      className="space-y-3 rounded-xl border border-slate-200 bg-white p-4 shadow-sm"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="space-y-1">
          <p className="text-sm font-semibold text-slate-900">{task.title}</p>
          <p className="text-xs text-slate-500">{context.primary}</p>
          <p className="text-xs text-slate-500">{context.secondary}</p>
        </div>
        <TaskStatusBadge status={task.status} />
      </div>

      {task.description ? <p className="text-sm text-slate-600">{task.description}</p> : null}

      <div className="grid gap-2 text-xs text-slate-500">
        <span>Assigned: <span className="font-mono text-slate-700">{task.assignedToUserId}</span></span>
        <span>Due: <span className="text-slate-700">{formatDate(task.dueDate)}</span></span>
        <span>{context.bookingLabel}</span>
      </div>

      <div className="flex flex-wrap gap-2">
        <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setIsEditing((current) => !current)}>
          {isEditing ? 'Close' : 'Edit'}
        </Button>
        <Button type="button" className="bg-emerald-600 hover:bg-emerald-500" onClick={() => onStatusChange(task.id, 'Done')} disabled={task.status === 'Done' || isBusy}>
          Mark Done
        </Button>
        <Button type="button" className="bg-sky-600 hover:bg-sky-500" onClick={() => currentUserId && onAssign(task.id, currentUserId)} disabled={!currentUserId || task.assignedToUserId === currentUserId || isBusy}>
          Assign to Me
        </Button>
        <Button type="button" className="bg-red-600 hover:bg-red-500" onClick={() => onDelete(task.id)} disabled={isBusy}>
          Delete
        </Button>
      </div>

      {isEditing ? (
        <TaskForm
          heading="Edit Task"
          submitLabel="Save Task"
          initialValues={{
            bookingId: task.bookingId ?? '',
            bookingItemId: task.bookingItemId ?? '',
            title: task.title,
            description: task.description ?? '',
            dueDate: toDateInput(task.dueDate),
            assignedToUserId: task.assignedToUserId
          }}
          isSubmitting={isBusy}
          contextLabel={context.bookingLabel}
          onSubmit={async (values) => {
            await onSave(task, values);
            setIsEditing(false);
          }}
          onCancel={() => setIsEditing(false)}
        />
      ) : null}
    </div>
  );
};

const TaskListRow = ({
  task,
  currentUserId,
  onStatusChange,
  onDelete,
  onSave,
  onAssign,
  isBusy
}: {
  task: OperationalTask;
  currentUserId: string | null;
  onStatusChange: (taskId: string, status: OperationalTaskStatus) => void;
  onDelete: (taskId: string) => void;
  onSave: (task: OperationalTask, values: TaskFormValues) => Promise<void>;
  onAssign: (taskId: string, userId: string) => void;
  isBusy: boolean;
}) => {
  const [isEditing, setIsEditing] = useState(false);
  const context = buildTaskContext(task);

  return (
    <div className="space-y-3 rounded-xl border border-slate-200 bg-white p-4">
      <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
        <div className="flex items-start gap-3">
          <input
            type="checkbox"
            checked={task.status === 'Done'}
            className="mt-1 h-4 w-4 rounded border-slate-300 text-slate-900 focus:ring-amber-300"
            disabled={task.status === 'Done'}
            onChange={(event) => {
              if (event.target.checked) {
                onStatusChange(task.id, 'Done');
              }
            }}
          />
          <div className="space-y-1">
            <p className="text-sm font-semibold text-slate-900">{task.title}</p>
            <p className="text-xs text-slate-500">{context.primary} • {context.secondary}</p>
            <p className="text-xs text-slate-500">Assigned: <span className="font-mono">{task.assignedToUserId}</span></p>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <TaskStatusBadge status={task.status} />
          <span className="text-xs text-slate-500">{formatDate(task.dueDate)}</span>
          <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setIsEditing((current) => !current)}>
            {isEditing ? 'Close' : 'Edit'}
          </Button>
          <Button type="button" className="bg-sky-600 hover:bg-sky-500" onClick={() => currentUserId && onAssign(task.id, currentUserId)} disabled={!currentUserId || task.assignedToUserId === currentUserId || isBusy}>
            Assign to Me
          </Button>
          <Button type="button" className="bg-red-600 hover:bg-red-500" onClick={() => onDelete(task.id)} disabled={isBusy}>
            Delete
          </Button>
        </div>
      </div>

      {isEditing ? (
        <TaskForm
          heading="Edit Task"
          submitLabel="Save Task"
          initialValues={{
            bookingId: task.bookingId ?? '',
            bookingItemId: task.bookingItemId ?? '',
            title: task.title,
            description: task.description ?? '',
            dueDate: toDateInput(task.dueDate),
            assignedToUserId: task.assignedToUserId
          }}
          isSubmitting={isBusy}
          contextLabel={context.bookingLabel}
          onSubmit={async (values) => {
            await onSave(task, values);
            setIsEditing(false);
          }}
          onCancel={() => setIsEditing(false)}
        />
      ) : null}
    </div>
  );
};

export const OperationsPage = () => {
  const currentUserId = getCurrentUserId();
  const [view, setView] = useState<'kanban' | 'list'>(() => {
    const stored = typeof window !== 'undefined' ? window.localStorage.getItem(viewStorageKey) : null;
    return stored === 'list' ? 'list' : 'kanban';
  });
  const [statusFilter, setStatusFilter] = useState<'All' | OperationalTaskStatus>('All');
  const [selectedBookingId, setSelectedBookingId] = useState<string>('');
  const [assignedToMeOnly, setAssignedToMeOnly] = useState(false);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [showCompleted, setShowCompleted] = useState(false);

  const { data: bookings = [] } = useBookings();
  const { data: tasks = [], isLoading, isError, error } = useTasks(selectedBookingId || undefined);
  const { data: myTasks = [] } = useMyTasks();
  const createTaskMutation = useCreateTask();
  const updateTaskStatusMutation = useUpdateTaskStatus();
  const updateTaskDetailsMutation = useUpdateTaskDetails();
  const assignTaskMutation = useAssignTask();
  const deleteTaskMutation = useDeleteTask();

  const mutationTaskId = updateTaskStatusMutation.variables?.id
    ?? updateTaskDetailsMutation.variables?.id
    ?? assignTaskMutation.variables?.id
    ?? deleteTaskMutation.variables;

  const handleViewChange = (nextView: 'kanban' | 'list') => {
    setView(nextView);
    window.localStorage.setItem(viewStorageKey, nextView);
  };

  const filteredTasks = useMemo(() => {
    return tasks.filter((task) => {
      if (statusFilter !== 'All' && task.status !== statusFilter) {
        return false;
      }

      if (assignedToMeOnly && currentUserId && task.assignedToUserId !== currentUserId) {
        return false;
      }

      return true;
    });
  }, [assignedToMeOnly, currentUserId, statusFilter, tasks]);

  const filteredMyTasks = useMemo(() => {
    return myTasks.filter((task) => {
      if (selectedBookingId && task.relatedBookingId !== selectedBookingId && task.bookingId !== selectedBookingId) {
        return false;
      }

      if (statusFilter !== 'All' && task.status !== statusFilter) {
        return false;
      }

      return true;
    });
  }, [myTasks, selectedBookingId, statusFilter]);

  const listSourceTasks = assignedToMeOnly ? filteredMyTasks : filteredTasks;
  const overdueTasks = listSourceTasks.filter((task) => task.status !== 'Done' && task.dueDate && new Date(task.dueDate).getTime() < Date.now());
  const completedTasks = listSourceTasks.filter((task) => task.status === 'Done');
  const activeTasks = listSourceTasks.filter((task) => task.status !== 'Done' && !overdueTasks.some((overdue) => overdue.id === task.id));

  const handleTaskSave = async (task: OperationalTask, values: TaskFormValues) => {
    await updateTaskDetailsMutation.mutateAsync({
      id: task.id,
      payload: {
        title: values.title,
        description: values.description || undefined,
        dueDate: values.dueDate || undefined
      }
    });

    if (values.assignedToUserId && values.assignedToUserId !== task.assignedToUserId) {
      await assignTaskMutation.mutateAsync({ id: task.id, userId: values.assignedToUserId });
    }
  };

  const createInitialValues = useMemo(() => defaultTaskValues(currentUserId), [currentUserId]);

  return (
    <div className="space-y-6">
      <Card title="Operations Board">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
          <div className="grid gap-3 md:grid-cols-3 xl:min-w-[720px] xl:flex-1">
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-slate-700">Status</span>
              <select className="w-full rounded border border-slate-300 px-3 py-2 text-sm" value={statusFilter} onChange={(event) => setStatusFilter(event.target.value as 'All' | OperationalTaskStatus)}>
                <option value="All">All statuses</option>
                {taskStatusOptions.map((option) => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </select>
            </label>

            <label className="block text-sm">
              <span className="mb-1 block font-medium text-slate-700">Booking</span>
              <select className="w-full rounded border border-slate-300 px-3 py-2 text-sm" value={selectedBookingId} onChange={(event) => setSelectedBookingId(event.target.value)}>
                <option value="">All bookings</option>
                {bookings.map((booking) => (
                  <option key={booking.id} value={booking.id}>
                    {shortId(booking.id)} • {booking.status}
                  </option>
                ))}
              </select>
            </label>

            <label className="flex items-center gap-3 rounded border border-slate-200 bg-slate-50 px-3 py-2 text-sm font-medium text-slate-700">
              <input type="checkbox" checked={assignedToMeOnly} onChange={(event) => setAssignedToMeOnly(event.target.checked)} />
              Assigned to me
            </label>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <div className="inline-flex rounded-full border border-slate-200 bg-slate-100 p-1">
              <button type="button" className={`rounded-full px-4 py-2 text-sm font-medium ${view === 'kanban' ? 'bg-slate-900 text-white' : 'text-slate-700'}`} onClick={() => handleViewChange('kanban')}>
                Kanban
              </button>
              <button type="button" className={`rounded-full px-4 py-2 text-sm font-medium ${view === 'list' ? 'bg-slate-900 text-white' : 'text-slate-700'}`} onClick={() => handleViewChange('list')}>
                List
              </button>
            </div>

            <Button type="button" onClick={() => setShowCreateForm((current) => !current)}>
              {showCreateForm ? 'Close Task Form' : 'New Task'}
            </Button>
          </div>
        </div>

        {showCreateForm ? (
          <div className="mt-4">
            <TaskForm
              heading="Create Operational Task"
              submitLabel="Create Task"
              initialValues={createInitialValues}
              isSubmitting={createTaskMutation.isPending}
              allowContextEditing
              onSubmit={async (values) => {
                await createTaskMutation.mutateAsync({
                  bookingId: values.bookingId || undefined,
                  bookingItemId: values.bookingItemId || undefined,
                  title: values.title,
                  description: values.description || undefined,
                  dueDate: values.dueDate ? new Date(values.dueDate).toISOString() : undefined,
                  assignedToUserId: values.assignedToUserId
                });

                setShowCreateForm(false);
              }}
            />
          </div>
        ) : null}

        {createTaskMutation.isError ? <p className="mt-3 text-sm text-red-600">{(createTaskMutation.error as Error).message}</p> : null}
      </Card>

      {isLoading ? <Card title="Operational Tasks"><p className="text-sm text-slate-500">Loading tasks...</p></Card> : null}
      {isError ? <Card title="Operational Tasks"><p className="text-sm text-red-600">{(error as Error).message}</p></Card> : null}

      {!isLoading && !isError ? (
        view === 'kanban' ? (
          <div className="grid gap-4 xl:grid-cols-5">
            {taskStatusOptions.map((column) => {
              const columnTasks = filteredTasks.filter((task) => task.status === column.value);

              return (
                <section
                  key={column.value}
                  className="rounded-2xl border border-slate-200 bg-white/80 p-4 shadow-sm"
                  onDragOver={(event) => event.preventDefault()}
                  onDrop={(event) => {
                    event.preventDefault();
                    const taskId = event.dataTransfer.getData('text/task-id');
                    if (taskId) {
                      updateTaskStatusMutation.mutate({ id: taskId, status: column.value });
                    }
                  }}
                >
                  <div className="mb-4 flex items-center justify-between gap-3">
                    <div>
                      <p className="text-sm font-semibold text-slate-900">{column.label}</p>
                      <p className="text-xs text-slate-500">{columnTasks.length} tasks</p>
                    </div>
                  </div>

                  <div className="space-y-3">
                    {columnTasks.map((task) => (
                      <TaskCard
                        key={task.id}
                        task={task}
                        currentUserId={currentUserId}
                        isBusy={mutationTaskId === task.id}
                        onStatusChange={(taskId, status) => updateTaskStatusMutation.mutate({ id: taskId, status })}
                        onDelete={(taskId) => deleteTaskMutation.mutate(taskId)}
                        onAssign={(taskId, userId) => assignTaskMutation.mutate({ id: taskId, userId })}
                        onSave={handleTaskSave}
                      />
                    ))}

                    {columnTasks.length === 0 ? <p className="rounded-xl border border-dashed border-slate-200 bg-slate-50 px-3 py-4 text-sm text-slate-400">Drop a task here to move it to {getTaskStatusLabel(column.value)}.</p> : null}
                  </div>
                </section>
              );
            })}
          </div>
        ) : (
          <div className="space-y-6">
            <Card title="My Tasks">
              <div className="space-y-3">
                {activeTasks.length === 0 ? <p className="text-sm text-slate-500">No active tasks match the current filters.</p> : null}
                {activeTasks.map((task) => (
                  <TaskListRow
                    key={task.id}
                    task={task}
                    currentUserId={currentUserId}
                    isBusy={mutationTaskId === task.id}
                    onStatusChange={(taskId, status) => updateTaskStatusMutation.mutate({ id: taskId, status })}
                    onDelete={(taskId) => deleteTaskMutation.mutate(taskId)}
                    onAssign={(taskId, userId) => assignTaskMutation.mutate({ id: taskId, userId })}
                    onSave={handleTaskSave}
                  />
                ))}
              </div>
            </Card>

            <Card title="Overdue Tasks">
              <div className="space-y-3">
                {overdueTasks.length === 0 ? <p className="text-sm text-slate-500">No overdue tasks right now.</p> : null}
                {overdueTasks.map((task) => (
                  <TaskListRow
                    key={task.id}
                    task={task}
                    currentUserId={currentUserId}
                    isBusy={mutationTaskId === task.id}
                    onStatusChange={(taskId, status) => updateTaskStatusMutation.mutate({ id: taskId, status })}
                    onDelete={(taskId) => deleteTaskMutation.mutate(taskId)}
                    onAssign={(taskId, userId) => assignTaskMutation.mutate({ id: taskId, userId })}
                    onSave={handleTaskSave}
                  />
                ))}
              </div>
            </Card>

            <Card title="Completed">
              <div className="mb-3 flex justify-end">
                <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setShowCompleted((current) => !current)}>
                  {showCompleted ? 'Hide Completed' : 'Show Completed'}
                </Button>
              </div>

              {showCompleted ? (
                <div className="space-y-3">
                  {completedTasks.length === 0 ? <p className="text-sm text-slate-500">No completed tasks for the current filters.</p> : null}
                  {completedTasks.map((task) => (
                    <TaskListRow
                      key={task.id}
                      task={task}
                      currentUserId={currentUserId}
                      isBusy={mutationTaskId === task.id}
                      onStatusChange={(taskId, status) => updateTaskStatusMutation.mutate({ id: taskId, status })}
                      onDelete={(taskId) => deleteTaskMutation.mutate(taskId)}
                      onAssign={(taskId, userId) => assignTaskMutation.mutate({ id: taskId, userId })}
                      onSave={handleTaskSave}
                    />
                  ))}
                </div>
              ) : (
                <p className="text-sm text-slate-500">Completed tasks stay tucked away until you need them.</p>
              )}
            </Card>
          </div>
        )
      ) : null}
    </div>
  );
};
