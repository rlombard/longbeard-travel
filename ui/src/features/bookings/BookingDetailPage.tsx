import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getCurrentUserId } from '../../auth/keycloak';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { useBooking, useUpdateBookingItemNote, useUpdateBookingItemStatus, useUpdateBookingStatus } from './hooks';
import { BookingItemStatus, BookingStatus } from '../../types/booking';
import { bookingItemStatusOptions, bookingStatusOptions, StatusBadge } from './statusPresentation';
import { TaskForm } from '../tasks/TaskForm';
import { useAssignTask, useCreateTask, useDeleteTask, useTasks, useUpdateTaskStatus } from '../tasks/hooks';
import { OperationalTaskStatus } from '../../types/task';
import { TaskStatusBadge, taskStatusOptions } from '../tasks/statusPresentation';
import { useAcceptTaskSuggestion, useGenerateTaskSuggestions, useRejectTaskSuggestion, useRegenerateTaskSuggestions, useTaskSuggestions } from './aiHooks';
import { useAddEmailMessage, useAnalyzeEmailThread, useApproveEmailDraft, useCreateEmailDraft, useCreateEmailThread, useDraftReply, useEmailThreads, useGenerateTasksFromEmailThread, useSendEmailDraft, useUpdateEmailDraft } from '../emails/hooks';
import { AddEmailMessageRequest, EmailClassificationType, EmailDirection } from '../../types/email';

const formatDate = (value: string) => new Date(value).toLocaleString();
const toIsoDateInput = (value?: string | null) => (value ? new Date(value).toISOString().slice(0, 16) : '');
const shortId = (value?: string | null) => (value ? `${value.slice(0, 8)}...` : 'n/a');
const formatDateMaybe = (value?: string | null) => (value ? formatDate(value) : 'No date set');
const toPercent = (value: number) => `${Math.round(value * 100)}%`;

const classificationColors: Record<EmailClassificationType, string> = {
  ConfirmationReceived: 'bg-emerald-100 text-emerald-700',
  PartialConfirmation: 'bg-sky-100 text-sky-700',
  NeedsMoreInformation: 'bg-amber-100 text-amber-700',
  PricingChanged: 'bg-rose-100 text-rose-700',
  AvailabilityIssue: 'bg-red-100 text-red-700',
  NoActionNeeded: 'bg-slate-100 text-slate-700',
  HumanDecisionRequired: 'bg-violet-100 text-violet-700',
  Unclear: 'bg-stone-100 text-stone-700'
};

const draftStatusColors: Record<string, string> = {
  Draft: 'bg-slate-100 text-slate-700',
  Approved: 'bg-sky-100 text-sky-700',
  Sent: 'bg-emerald-100 text-emerald-700',
  Rejected: 'bg-rose-100 text-rose-700'
};

interface DraftEditorState {
  subject: string;
  body: string;
}

interface MessageComposerState {
  direction: EmailDirection;
  subject: string;
  bodyText: string;
  sender: string;
  recipients: string;
}

export const BookingDetailPage = () => {
  const { bookingId } = useParams();
  const currentUserId = getCurrentUserId();
  const { data: booking, isLoading, isError, error } = useBooking(bookingId);
  const { data: tasks = [], isLoading: tasksLoading, isError: tasksError, error: tasksQueryError } = useTasks(bookingId);
  const { data: suggestions = [] } = useTaskSuggestions(bookingId);
  const { data: emailThreads = [] } = useEmailThreads(bookingId);

  const updateBookingStatusMutation = useUpdateBookingStatus();
  const updateBookingItemStatusMutation = useUpdateBookingItemStatus();
  const updateBookingItemNoteMutation = useUpdateBookingItemNote();
  const createTaskMutation = useCreateTask();
  const updateTaskStatusMutation = useUpdateTaskStatus();
  const assignTaskMutation = useAssignTask();
  const deleteTaskMutation = useDeleteTask();
  const generateTaskSuggestionsMutation = useGenerateTaskSuggestions();
  const regenerateTaskSuggestionsMutation = useRegenerateTaskSuggestions();
  const acceptTaskSuggestionMutation = useAcceptTaskSuggestion();
  const rejectTaskSuggestionMutation = useRejectTaskSuggestion();
  const createEmailThreadMutation = useCreateEmailThread();
  const addEmailMessageMutation = useAddEmailMessage();
  const analyzeEmailThreadMutation = useAnalyzeEmailThread();
  const generateEmailTasksMutation = useGenerateTasksFromEmailThread();
  const draftReplyMutation = useDraftReply();
  const createEmailDraftMutation = useCreateEmailDraft();
  const updateEmailDraftMutation = useUpdateEmailDraft();
  const approveEmailDraftMutation = useApproveEmailDraft();
  const sendEmailDraftMutation = useSendEmailDraft();

  const [noteDrafts, setNoteDrafts] = useState<Record<string, string>>({});
  const [taskContext, setTaskContext] = useState<{ bookingId?: string; bookingItemId?: string; label: string } | null>(null);
  const [createThreadState, setCreateThreadState] = useState({ subject: '', supplierEmail: '', bookingItemId: '' });
  const [manualDraftState, setManualDraftState] = useState({ emailThreadId: '', subject: '', body: '' });
  const [messageDrafts, setMessageDrafts] = useState<Record<string, MessageComposerState>>({});
  const [draftEdits, setDraftEdits] = useState<Record<string, DraftEditorState>>({});

  useEffect(() => {
    if (!booking) {
      return;
    }

    setNoteDrafts(
      booking.items.reduce<Record<string, string>>((accumulator, item) => {
        accumulator[item.id] = item.notes ?? '';
        return accumulator;
      }, {})
    );

    if (!createThreadState.supplierEmail && booking.items[0]?.supplierName) {
      setCreateThreadState((current) => ({ ...current, bookingItemId: booking.items[0]?.id ?? '' }));
    }
  }, [booking]);

  useEffect(() => {
    setDraftEdits(
      emailThreads.flatMap((thread) => thread.drafts).reduce<Record<string, DraftEditorState>>((accumulator, draft) => {
        accumulator[draft.id] = { subject: draft.subject, body: draft.body };
        return accumulator;
      }, {})
    );

    setMessageDrafts(
      emailThreads.reduce<Record<string, MessageComposerState>>((accumulator, thread) => {
        accumulator[thread.id] = {
          direction: 'Inbound',
          subject: thread.subject,
          bodyText: '',
          sender: thread.supplierEmail,
          recipients: 'ops@tourops.local'
        };
        return accumulator;
      }, {})
    );
  }, [emailThreads]);

  const itemSavingState = useMemo(
    () => ({
      statusId: updateBookingItemStatusMutation.variables?.id,
      noteId: updateBookingItemNoteMutation.variables?.id
    }),
    [updateBookingItemStatusMutation.variables, updateBookingItemNoteMutation.variables]
  );

  const taskMutationId = updateTaskStatusMutation.variables?.id ?? assignTaskMutation.variables?.id ?? deleteTaskMutation.variables;
  const pendingSuggestions = suggestions.filter((suggestion) => suggestion.state === 'PendingReview');

  if (isLoading) {
    return <Card title="Booking Detail"><p className="text-sm text-slate-500">Loading booking...</p></Card>;
  }

  if (isError || !booking) {
    return <Card title="Booking Detail"><p className="text-sm text-red-600">{(error as Error)?.message ?? 'Booking not found.'}</p></Card>;
  }

  const saveManualDraft = async () => {
    await createEmailDraftMutation.mutateAsync({
      bookingId: booking.id,
      emailThreadId: manualDraftState.emailThreadId || undefined,
      subject: manualDraftState.subject,
      body: manualDraftState.body
    });

    setManualDraftState({ emailThreadId: '', subject: '', body: '' });
  };

  return (
    <div className="space-y-6">
      <Card title="Booking Detail">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div className="space-y-2">
            <Link className="text-sm text-slate-500 underline decoration-amber-300 underline-offset-4" to="/app/bookings">
              Back to bookings
            </Link>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.3em] text-amber-700">Booking</p>
              <h2 className="font-mono text-sm text-slate-900">{booking.id}</h2>
            </div>
            <div className="flex flex-wrap gap-3 text-sm text-slate-600">
              <span>Quote: <span className="font-mono text-xs">{booking.quoteId}</span></span>
              <span>Created: {formatDate(booking.createdAt)}</span>
            </div>
          </div>

          <div className="grid gap-3 md:min-w-72">
            <div className="flex items-center gap-3">
              <StatusBadge status={booking.status} />
              <select
                className="rounded border border-slate-300 px-3 py-2 text-sm"
                value={booking.status}
                onChange={(event) => {
                  void updateBookingStatusMutation.mutateAsync({
                    id: booking.id,
                    status: event.target.value as BookingStatus
                  });
                }}
                disabled={updateBookingStatusMutation.isPending}
              >
                {bookingStatusOptions.map((option) => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </select>
            </div>
            {updateBookingStatusMutation.isError ? <p className="text-sm text-red-600">{(updateBookingStatusMutation.error as Error).message}</p> : null}
          </div>
        </div>
      </Card>

      <Card title="Suggested Tasks">
        <div className="mb-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <p className="text-sm text-slate-500">AI can draft operational follow-ups from booking state and supplier communication, but every suggestion still needs human review.</p>
          <div className="flex flex-wrap gap-2">
            <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => bookingId && void generateTaskSuggestionsMutation.mutateAsync(bookingId)} isLoading={generateTaskSuggestionsMutation.isPending}>
              Generate Suggestions
            </Button>
            <Button type="button" onClick={() => bookingId && void regenerateTaskSuggestionsMutation.mutateAsync(bookingId)} isLoading={regenerateTaskSuggestionsMutation.isPending}>
              Regenerate
            </Button>
          </div>
        </div>

        {pendingSuggestions.length === 0 ? <p className="text-sm text-slate-500">No pending AI suggestions yet for this booking.</p> : null}
        <div className="space-y-3">
          {pendingSuggestions.map((suggestion) => (
            <div key={suggestion.id} className="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                <div className="space-y-2">
                  <div className="flex flex-wrap items-center gap-2">
                    <p className="text-sm font-semibold text-slate-900">{suggestion.title}</p>
                    <TaskStatusBadge status={suggestion.suggestedStatus} />
                    <span className="rounded-full bg-amber-100 px-2 py-1 text-xs font-medium text-amber-700">Review required</span>
                  </div>
                  <p className="text-sm text-slate-600">{suggestion.description}</p>
                  <div className="flex flex-wrap gap-3 text-xs text-slate-500">
                    <span>Reason: {suggestion.reason}</span>
                    <span>Confidence: {toPercent(suggestion.confidence)}</span>
                    <span>Due: {formatDateMaybe(suggestion.suggestedDueDate)}</span>
                    {suggestion.productName ? <span>Product: {suggestion.productName}</span> : null}
                    {suggestion.supplierName ? <span>Supplier: {suggestion.supplierName}</span> : null}
                  </div>
                </div>
                <div className="flex flex-wrap gap-2">
                  <Button
                    type="button"
                    className="bg-emerald-600 hover:bg-emerald-500"
                    disabled={!currentUserId || acceptTaskSuggestionMutation.isPending}
                    onClick={() => currentUserId && void acceptTaskSuggestionMutation.mutateAsync({ id: suggestion.id, assignedToUserId: currentUserId })}
                  >
                    Accept
                  </Button>
                  <Button
                    type="button"
                    className="bg-red-600 hover:bg-red-500"
                    disabled={rejectTaskSuggestionMutation.isPending}
                    onClick={() => void rejectTaskSuggestionMutation.mutateAsync(suggestion.id)}
                  >
                    Reject
                  </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
        {generateTaskSuggestionsMutation.isError ? <p className="mt-3 text-sm text-red-600">{(generateTaskSuggestionsMutation.error as Error).message}</p> : null}
        {regenerateTaskSuggestionsMutation.isError ? <p className="mt-3 text-sm text-red-600">{(regenerateTaskSuggestionsMutation.error as Error).message}</p> : null}
        {acceptTaskSuggestionMutation.isError ? <p className="mt-3 text-sm text-red-600">{(acceptTaskSuggestionMutation.error as Error).message}</p> : null}
        {rejectTaskSuggestionMutation.isError ? <p className="mt-3 text-sm text-red-600">{(rejectTaskSuggestionMutation.error as Error).message}</p> : null}
      </Card>

      <Card title="Booking Items">
        <div className="overflow-auto rounded border border-slate-200">
          <table className="min-w-full bg-white text-sm">
            <thead className="bg-slate-100 text-left text-slate-700">
              <tr>
                <th className="px-3 py-2 font-medium">Product</th>
                <th className="px-3 py-2 font-medium">Supplier</th>
                <th className="px-3 py-2 font-medium">Status</th>
                <th className="px-3 py-2 font-medium">Notes</th>
                <th className="px-3 py-2 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {booking.items.map((item) => {
                const draftValue = noteDrafts[item.id] ?? '';
                const noteChanged = draftValue !== (item.notes ?? '');

                return (
                  <tr key={item.id} className="border-t border-slate-200 align-top">
                    <td className="px-3 py-3">
                      <div className="font-medium text-slate-900">{item.productName || item.productId}</div>
                      <div className="font-mono text-xs text-slate-500">{item.productId}</div>
                    </td>
                    <td className="px-3 py-3">
                      <div className="font-medium text-slate-900">{item.supplierName || item.supplierId}</div>
                      <div className="font-mono text-xs text-slate-500">{item.supplierId}</div>
                    </td>
                    <td className="px-3 py-3">
                      <div className="space-y-2">
                        <StatusBadge status={item.status} />
                        <select
                          className="w-full rounded border border-slate-300 px-3 py-2 text-sm"
                          value={item.status}
                          onChange={(event) => {
                            void updateBookingItemStatusMutation.mutateAsync({
                              id: item.id,
                              status: event.target.value as BookingItemStatus
                            });
                          }}
                          disabled={updateBookingItemStatusMutation.isPending && itemSavingState.statusId === item.id}
                        >
                          {bookingItemStatusOptions.map((option) => (
                            <option key={option.value} value={option.value}>{option.label}</option>
                          ))}
                        </select>
                      </div>
                    </td>
                    <td className="px-3 py-3">
                      <div className="space-y-2">
                        <textarea
                          rows={3}
                          className="w-full rounded border border-slate-300 px-3 py-2 text-sm focus:border-slate-500 focus:outline-none focus:ring-2 focus:ring-amber-200"
                          value={draftValue}
                          onChange={(event) => setNoteDrafts((current) => ({ ...current, [item.id]: event.target.value }))}
                          placeholder="Add supplier operations notes"
                        />
                        <div className="flex items-center gap-2">
                          <Button
                            type="button"
                            className="bg-slate-200 text-slate-800 hover:bg-slate-300"
                            disabled={!noteChanged}
                            isLoading={updateBookingItemNoteMutation.isPending && itemSavingState.noteId === item.id}
                            onClick={() => {
                              void updateBookingItemNoteMutation.mutateAsync({ id: item.id, note: draftValue });
                            }}
                          >
                            Save note
                          </Button>
                          <span className="text-xs text-slate-500">Created {formatDate(item.createdAt)}</span>
                        </div>
                      </div>
                    </td>
                    <td className="px-3 py-3">
                      <Button
                        type="button"
                        className="bg-sky-600 hover:bg-sky-500"
                        onClick={() => setTaskContext({
                          bookingItemId: item.id,
                          label: `Booking item task for ${item.productName || item.productId}`
                        })}
                      >
                        + Task
                      </Button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        {updateBookingItemStatusMutation.isError ? <p className="mt-3 text-sm text-red-600">{(updateBookingItemStatusMutation.error as Error).message}</p> : null}
        {updateBookingItemNoteMutation.isError ? <p className="mt-3 text-sm text-red-600">{(updateBookingItemNoteMutation.error as Error).message}</p> : null}
      </Card>

      <Card title="Email Threads">
        <div className="grid gap-4 lg:grid-cols-[1.1fr_0.9fr]">
          <div className="space-y-4 rounded-xl border border-slate-200 bg-slate-50 p-4">
            <h3 className="text-base font-semibold text-slate-900">Create Thread</h3>
            <div className="grid gap-3 md:grid-cols-2">
              <label className="block text-sm">
                <span className="mb-1 block font-medium text-slate-700">Subject</span>
                <input className="w-full rounded border border-slate-300 px-3 py-2" value={createThreadState.subject} onChange={(event) => setCreateThreadState((current) => ({ ...current, subject: event.target.value }))} />
              </label>
              <label className="block text-sm">
                <span className="mb-1 block font-medium text-slate-700">Supplier Email</span>
                <input className="w-full rounded border border-slate-300 px-3 py-2" value={createThreadState.supplierEmail} onChange={(event) => setCreateThreadState((current) => ({ ...current, supplierEmail: event.target.value }))} />
              </label>
            </div>
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-slate-700">Booking Item Context</span>
              <select className="w-full rounded border border-slate-300 px-3 py-2" value={createThreadState.bookingItemId} onChange={(event) => setCreateThreadState((current) => ({ ...current, bookingItemId: event.target.value }))}>
                <option value="">Booking-level thread</option>
                {booking.items.map((item) => (
                  <option key={item.id} value={item.id}>{item.productName} • {item.supplierName}</option>
                ))}
              </select>
            </label>
            <Button
              type="button"
              isLoading={createEmailThreadMutation.isPending}
              onClick={() => void createEmailThreadMutation.mutateAsync({
                bookingId: booking.id,
                payload: {
                  bookingItemId: createThreadState.bookingItemId || undefined,
                  subject: createThreadState.subject,
                  supplierEmail: createThreadState.supplierEmail
                }
              }).then(() => setCreateThreadState({ subject: '', supplierEmail: '', bookingItemId: '' }))}
            >
              Create Thread
            </Button>
            {createEmailThreadMutation.isError ? <p className="text-sm text-red-600">{(createEmailThreadMutation.error as Error).message}</p> : null}
          </div>

          <div className="space-y-4 rounded-xl border border-slate-200 bg-slate-50 p-4">
            <h3 className="text-base font-semibold text-slate-900">Manual Draft</h3>
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-slate-700">Thread</span>
              <select className="w-full rounded border border-slate-300 px-3 py-2" value={manualDraftState.emailThreadId} onChange={(event) => setManualDraftState((current) => ({ ...current, emailThreadId: event.target.value }))}>
                <option value="">Booking-level draft</option>
                {emailThreads.map((thread) => (
                  <option key={thread.id} value={thread.id}>{thread.subject} • {thread.supplierEmail}</option>
                ))}
              </select>
            </label>
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-slate-700">Subject</span>
              <input className="w-full rounded border border-slate-300 px-3 py-2" value={manualDraftState.subject} onChange={(event) => setManualDraftState((current) => ({ ...current, subject: event.target.value }))} />
            </label>
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-slate-700">Body</span>
              <textarea className="w-full rounded border border-slate-300 px-3 py-2" rows={5} value={manualDraftState.body} onChange={(event) => setManualDraftState((current) => ({ ...current, body: event.target.value }))} />
            </label>
            <Button type="button" isLoading={createEmailDraftMutation.isPending} onClick={() => void saveManualDraft()}>
              Save Draft
            </Button>
            {createEmailDraftMutation.isError ? <p className="text-sm text-red-600">{(createEmailDraftMutation.error as Error).message}</p> : null}
          </div>
        </div>

        <div className="mt-4 space-y-4">
          {emailThreads.length === 0 ? <p className="text-sm text-slate-500">No email threads linked to this booking yet.</p> : null}
          {emailThreads.map((thread) => {
            const composer = messageDrafts[thread.id] ?? {
              direction: 'Inbound' as EmailDirection,
              subject: thread.subject,
              bodyText: '',
              sender: thread.supplierEmail,
              recipients: 'ops@tourops.local'
            };

            return (
              <div key={thread.id} className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                <div className="mb-4 flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                  <div>
                    <p className="text-sm font-semibold text-slate-900">{thread.subject}</p>
                    <p className="text-xs text-slate-500">{thread.supplierEmail} • Last message {formatDateMaybe(thread.lastMessageAt)}</p>
                    <p className="text-xs text-slate-500">Context: {thread.bookingItemId ? `Booking item ${shortId(thread.bookingItemId)}` : `Booking ${shortId(thread.relatedBookingId ?? thread.bookingId)}`}</p>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => void analyzeEmailThreadMutation.mutateAsync(thread.id)} isLoading={analyzeEmailThreadMutation.isPending}>
                      Analyze with AI
                    </Button>
                    <Button type="button" className="bg-sky-600 hover:bg-sky-500" onClick={() => void generateEmailTasksMutation.mutateAsync(thread.id)} isLoading={generateEmailTasksMutation.isPending}>
                      Generate Tasks
                    </Button>
                    <Button type="button" onClick={() => void draftReplyMutation.mutateAsync(thread.id)} isLoading={draftReplyMutation.isPending}>
                      Suggest Reply
                    </Button>
                  </div>
                </div>

                <div className="grid gap-4 xl:grid-cols-[1.05fr_0.95fr]">
                  <div className="space-y-3 rounded-xl border border-slate-200 bg-slate-50 p-4">
                    <h4 className="text-sm font-semibold text-slate-900">Thread Timeline</h4>
                    {thread.messages.length === 0 ? <p className="text-sm text-slate-500">No messages yet.</p> : null}
                    {thread.messages.map((message) => (
                      <div key={message.id} className="rounded-lg border border-slate-200 bg-white p-3">
                        <div className="flex flex-wrap items-center gap-2">
                          <span className={`rounded-full px-2 py-1 text-xs font-medium ${message.direction === 'Inbound' ? 'bg-amber-100 text-amber-700' : 'bg-slate-100 text-slate-700'}`}>{message.direction}</span>
                          {message.aiClassification ? <span className={`rounded-full px-2 py-1 text-xs font-medium ${classificationColors[message.aiClassification]}`}>{message.aiClassification}</span> : null}
                          <span className="text-xs text-slate-500">{formatDate(message.sentAt)}</span>
                        </div>
                        <p className="mt-2 text-sm font-medium text-slate-900">{message.subject}</p>
                        <p className="mt-1 whitespace-pre-wrap text-sm text-slate-600">{message.bodyText}</p>
                        {message.aiSummary ? <p className="mt-2 rounded bg-slate-50 px-2 py-2 text-xs text-slate-600"><span className="font-semibold text-slate-700">AI summary:</span> {message.aiSummary}</p> : null}
                      </div>
                    ))}
                  </div>

                  <div className="space-y-3 rounded-xl border border-slate-200 bg-slate-50 p-4">
                    <h4 className="text-sm font-semibold text-slate-900">Add Message</h4>
                    <div className="grid gap-3 md:grid-cols-2">
                      <label className="block text-sm">
                        <span className="mb-1 block font-medium text-slate-700">Direction</span>
                        <select className="w-full rounded border border-slate-300 px-3 py-2" value={composer.direction} onChange={(event) => setMessageDrafts((current) => ({ ...current, [thread.id]: { ...composer, direction: event.target.value as EmailDirection } }))}>
                          <option value="Inbound">Inbound</option>
                          <option value="Outbound">Outbound</option>
                        </select>
                      </label>
                      <label className="block text-sm">
                        <span className="mb-1 block font-medium text-slate-700">Subject</span>
                        <input className="w-full rounded border border-slate-300 px-3 py-2" value={composer.subject} onChange={(event) => setMessageDrafts((current) => ({ ...current, [thread.id]: { ...composer, subject: event.target.value } }))} />
                      </label>
                    </div>
                    <div className="grid gap-3 md:grid-cols-2">
                      <label className="block text-sm">
                        <span className="mb-1 block font-medium text-slate-700">Sender</span>
                        <input className="w-full rounded border border-slate-300 px-3 py-2" value={composer.sender} onChange={(event) => setMessageDrafts((current) => ({ ...current, [thread.id]: { ...composer, sender: event.target.value } }))} />
                      </label>
                      <label className="block text-sm">
                        <span className="mb-1 block font-medium text-slate-700">Recipients</span>
                        <input className="w-full rounded border border-slate-300 px-3 py-2" value={composer.recipients} onChange={(event) => setMessageDrafts((current) => ({ ...current, [thread.id]: { ...composer, recipients: event.target.value } }))} />
                      </label>
                    </div>
                    <label className="block text-sm">
                      <span className="mb-1 block font-medium text-slate-700">Body</span>
                      <textarea className="w-full rounded border border-slate-300 px-3 py-2" rows={5} value={composer.bodyText} onChange={(event) => setMessageDrafts((current) => ({ ...current, [thread.id]: { ...composer, bodyText: event.target.value } }))} />
                    </label>
                    <Button
                      type="button"
                      isLoading={addEmailMessageMutation.isPending}
                      onClick={() => void addEmailMessageMutation.mutateAsync({
                        threadId: thread.id,
                        payload: {
                          direction: composer.direction,
                          subject: composer.subject,
                          bodyText: composer.bodyText,
                          sender: composer.sender,
                          recipients: composer.recipients,
                          sentAt: new Date().toISOString()
                        } as AddEmailMessageRequest
                      }).then(() => setMessageDrafts((current) => ({ ...current, [thread.id]: { ...composer, bodyText: '' } })))}
                    >
                      Save Message
                    </Button>
                  </div>
                </div>

                <div className="mt-4 space-y-3">
                  <h4 className="text-sm font-semibold text-slate-900">Draft Replies</h4>
                  {thread.drafts.length === 0 ? <p className="text-sm text-slate-500">No drafts yet for this thread.</p> : null}
                  {thread.drafts.map((draft) => {
                    const draftEdit = draftEdits[draft.id] ?? { subject: draft.subject, body: draft.body };
                    return (
                      <div key={draft.id} className="rounded-xl border border-slate-200 bg-slate-50 p-4">
                        <div className="mb-3 flex flex-wrap items-center gap-2">
                          <span className={`rounded-full px-2 py-1 text-xs font-medium ${draftStatusColors[draft.status]}`}>{draft.status}</span>
                          {draft.generatedByAi ? <span className="rounded-full bg-indigo-100 px-2 py-1 text-xs font-medium text-indigo-700">Generated by AI</span> : null}
                          {draft.llmProvider ? <span className="text-xs text-slate-500">{draft.llmProvider} / {draft.llmModel}</span> : null}
                        </div>
                        <label className="block text-sm">
                          <span className="mb-1 block font-medium text-slate-700">Subject</span>
                          <input className="w-full rounded border border-slate-300 px-3 py-2" value={draftEdit.subject} onChange={(event) => setDraftEdits((current) => ({ ...current, [draft.id]: { ...draftEdit, subject: event.target.value } }))} />
                        </label>
                        <label className="mt-3 block text-sm">
                          <span className="mb-1 block font-medium text-slate-700">Body</span>
                          <textarea className="w-full rounded border border-slate-300 px-3 py-2" rows={6} value={draftEdit.body} onChange={(event) => setDraftEdits((current) => ({ ...current, [draft.id]: { ...draftEdit, body: event.target.value } }))} />
                        </label>
                        <div className="mt-3 flex flex-wrap gap-2">
                          <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => void updateEmailDraftMutation.mutateAsync({ id: draft.id, payload: draftEdit })}>
                            Save Draft
                          </Button>
                          <Button type="button" className="bg-sky-600 hover:bg-sky-500" disabled={draft.status !== 'Draft'} onClick={() => void approveEmailDraftMutation.mutateAsync(draft.id)}>
                            Approve Draft
                          </Button>
                          <Button type="button" className="bg-emerald-600 hover:bg-emerald-500" disabled={draft.status !== 'Approved'} onClick={() => void sendEmailDraftMutation.mutateAsync(draft.id)}>
                            Send Draft
                          </Button>
                        </div>
                        <p className="mt-2 text-xs text-slate-500">Updated {formatDate(draft.updatedAt)}</p>
                      </div>
                    );
                  })}
                </div>
              </div>
            );
          })}
        </div>
        {addEmailMessageMutation.isError ? <p className="mt-3 text-sm text-red-600">{(addEmailMessageMutation.error as Error).message}</p> : null}
        {analyzeEmailThreadMutation.isError ? <p className="mt-3 text-sm text-red-600">{(analyzeEmailThreadMutation.error as Error).message}</p> : null}
        {generateEmailTasksMutation.isError ? <p className="mt-3 text-sm text-red-600">{(generateEmailTasksMutation.error as Error).message}</p> : null}
        {draftReplyMutation.isError ? <p className="mt-3 text-sm text-red-600">{(draftReplyMutation.error as Error).message}</p> : null}
        {updateEmailDraftMutation.isError ? <p className="mt-3 text-sm text-red-600">{(updateEmailDraftMutation.error as Error).message}</p> : null}
        {approveEmailDraftMutation.isError ? <p className="mt-3 text-sm text-red-600">{(approveEmailDraftMutation.error as Error).message}</p> : null}
        {sendEmailDraftMutation.isError ? <p className="mt-3 text-sm text-red-600">{(sendEmailDraftMutation.error as Error).message}</p> : null}
      </Card>

      <Card title="Tasks">
        <div className="mb-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <p className="text-sm text-slate-500">Create and hand over operational follow-ups directly against this booking or one of its supplier booking items.</p>
          <Button
            type="button"
            onClick={() => setTaskContext({
              bookingId: booking.id,
              label: `Booking-level task for ${shortId(booking.id)}`
            })}
          >
            + Booking Task
          </Button>
        </div>

        {taskContext ? (
          <div className="mb-4">
            <TaskForm
              heading="Create Task"
              submitLabel="Create Task"
              initialValues={{
                bookingId: taskContext.bookingId ?? '',
                bookingItemId: taskContext.bookingItemId ?? '',
                title: '',
                description: '',
                dueDate: '',
                assignedToUserId: currentUserId ?? ''
              }}
              isSubmitting={createTaskMutation.isPending}
              contextLabel={taskContext.label}
              onSubmit={async (values) => {
                await createTaskMutation.mutateAsync({
                  bookingId: values.bookingId || undefined,
                  bookingItemId: values.bookingItemId || undefined,
                  title: values.title,
                  description: values.description || undefined,
                  dueDate: values.dueDate ? new Date(values.dueDate).toISOString() : undefined,
                  assignedToUserId: values.assignedToUserId
                });

                setTaskContext(null);
              }}
              onCancel={() => setTaskContext(null)}
            />
          </div>
        ) : null}

        {tasksLoading ? <p className="text-sm text-slate-500">Loading tasks...</p> : null}
        {tasksError ? <p className="text-sm text-red-600">{(tasksQueryError as Error).message}</p> : null}
        {!tasksLoading && tasks.length === 0 ? <p className="text-sm text-slate-500">No operational tasks yet for this booking.</p> : null}

        <div className="space-y-3">
          {tasks.map((task) => (
            <div key={task.id} className="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                <div className="space-y-2">
                  <div className="flex items-center gap-3">
                    <p className="text-sm font-semibold text-slate-900">{task.title}</p>
                    <TaskStatusBadge status={task.status} />
                  </div>
                  {task.description ? <p className="text-sm text-slate-600">{task.description}</p> : null}
                  <div className="flex flex-wrap gap-3 text-xs text-slate-500">
                    <span>Assigned: <span className="font-mono text-slate-700">{task.assignedToUserId}</span></span>
                    <span>Due: {task.dueDate ? formatDate(task.dueDate) : 'No due date'}</span>
                    <span>{task.productName ? `Product: ${task.productName}` : `Booking ${shortId(task.relatedBookingId ?? task.bookingId)}`}</span>
                    {task.supplierName ? <span>Supplier: {task.supplierName}</span> : null}
                  </div>
                </div>

                <div className="grid gap-2 md:min-w-80">
                  <select
                    className="rounded border border-slate-300 px-3 py-2 text-sm"
                    value={task.status}
                    onChange={(event) => {
                      void updateTaskStatusMutation.mutateAsync({
                        id: task.id,
                        status: event.target.value as OperationalTaskStatus
                      });
                    }}
                    disabled={taskMutationId === task.id}
                  >
                    {taskStatusOptions.map((option) => (
                      <option key={option.value} value={option.value}>{option.label}</option>
                    ))}
                  </select>
                  <div className="flex flex-wrap gap-2">
                    <Button
                      type="button"
                      className="bg-sky-600 hover:bg-sky-500"
                      disabled={!currentUserId || task.assignedToUserId === currentUserId || taskMutationId === task.id}
                      onClick={() => {
                        if (!currentUserId) {
                          return;
                        }

                        void assignTaskMutation.mutateAsync({ id: task.id, userId: currentUserId });
                      }}
                    >
                      Assign to Me
                    </Button>
                    <Button
                      type="button"
                      className="bg-red-600 hover:bg-red-500"
                      disabled={taskMutationId === task.id}
                      onClick={() => {
                        void deleteTaskMutation.mutateAsync(task.id);
                      }}
                    >
                      Delete
                    </Button>
                  </div>
                  <p className="text-xs text-slate-500">Updated {formatDate(task.updatedAt)}</p>
                </div>
              </div>
            </div>
          ))}
        </div>

        {createTaskMutation.isError ? <p className="mt-3 text-sm text-red-600">{(createTaskMutation.error as Error).message}</p> : null}
        {updateTaskStatusMutation.isError ? <p className="mt-3 text-sm text-red-600">{(updateTaskStatusMutation.error as Error).message}</p> : null}
        {assignTaskMutation.isError ? <p className="mt-3 text-sm text-red-600">{(assignTaskMutation.error as Error).message}</p> : null}
        {deleteTaskMutation.isError ? <p className="mt-3 text-sm text-red-600">{(deleteTaskMutation.error as Error).message}</p> : null}
      </Card>
    </div>
  );
};
