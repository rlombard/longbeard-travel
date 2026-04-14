import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { useAddEmailMessage, useAnalyzeEmailThread, useApproveEmailDraft, useDraftReply, useEmailThread, useEmailThreads, useGenerateTasksFromEmailThread, useSendEmailDraft, useUpdateEmailDraft } from './hooks';
import { AddEmailMessageRequest, EmailClassificationType, EmailDirection } from '../../types/email';

const formatDate = (value?: string | null) => (value ? new Date(value).toLocaleString() : 'No date');
const shortId = (value?: string | null) => (value ? `${value.slice(0, 8)}...` : 'n/a');

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

export const EmailsPage = () => {
  const { data: threads = [], isLoading, isError, error } = useEmailThreads();
  const [selectedThreadId, setSelectedThreadId] = useState<string | null>(null);
  const { data: selectedThread } = useEmailThread(selectedThreadId ?? undefined);

  const analyzeMutation = useAnalyzeEmailThread();
  const generateTasksMutation = useGenerateTasksFromEmailThread();
  const draftReplyMutation = useDraftReply();
  const addMessageMutation = useAddEmailMessage();
  const updateEmailDraftMutation = useUpdateEmailDraft();
  const approveDraftMutation = useApproveEmailDraft();
  const sendDraftMutation = useSendEmailDraft();

  const [draftEdits, setDraftEdits] = useState<Record<string, DraftEditorState>>({});
  const [composer, setComposer] = useState<MessageComposerState>({
    direction: 'Outbound',
    subject: '',
    bodyText: '',
    sender: 'ops@tourops.local',
    recipients: ''
  });

  useEffect(() => {
    if (!selectedThreadId && threads[0]) {
      setSelectedThreadId(threads[0].id);
    }
  }, [threads, selectedThreadId]);

  useEffect(() => {
    if (!selectedThread) {
      return;
    }

    setComposer({
      direction: 'Outbound',
      subject: selectedThread.subject,
      bodyText: '',
      sender: 'ops@tourops.local',
      recipients: selectedThread.supplierEmail
    });

    setDraftEdits(
      selectedThread.drafts.reduce<Record<string, DraftEditorState>>((accumulator, draft) => {
        accumulator[draft.id] = { subject: draft.subject, body: draft.body };
        return accumulator;
      }, {})
    );
  }, [selectedThread]);

  const latestInbound = useMemo(
    () => selectedThread?.messages.find((message) => message.direction === 'Inbound' && message.aiClassification) ?? selectedThread?.messages.find((message) => message.aiClassification),
    [selectedThread]
  );

  if (isLoading) {
    return <Card title="Emails"><p className="text-sm text-slate-500">Loading email workspace...</p></Card>;
  }

  if (isError) {
    return <Card title="Emails"><p className="text-sm text-red-600">{(error as Error).message}</p></Card>;
  }

  return (
    <div className="grid gap-6 lg:grid-cols-[320px_minmax(0,1fr)]">
      <Card title="Threads">
        <div className="space-y-3">
          {threads.length === 0 ? <p className="text-sm text-slate-500">No email threads yet. Start them from a booking and they will appear here.</p> : null}
          {threads.map((thread) => (
            <button
              key={thread.id}
              type="button"
              onClick={() => setSelectedThreadId(thread.id)}
              className={`w-full rounded-2xl border px-4 py-3 text-left transition ${selectedThreadId === thread.id ? 'border-slate-900 bg-slate-900 text-white shadow-lg shadow-slate-900/20' : 'border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50'}`}
            >
              <p className="text-sm font-semibold">{thread.subject}</p>
              <p className={`mt-1 text-xs ${selectedThreadId === thread.id ? 'text-slate-200' : 'text-slate-500'}`}>{thread.supplierEmail}</p>
              <p className={`mt-2 text-xs ${selectedThreadId === thread.id ? 'text-slate-300' : 'text-slate-500'}`}>Booking {shortId(thread.relatedBookingId ?? thread.bookingId)} • Last activity {formatDate(thread.lastMessageAt ?? thread.createdAt)}</p>
            </button>
          ))}
        </div>
      </Card>

      <div className="space-y-6">
        {!selectedThread ? (
          <Card title="Email Workspace">
            <p className="text-sm text-slate-500">Select a thread to review supplier communication.</p>
          </Card>
        ) : (
          <>
            <Card title="Email Workspace">
              <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                <div>
                  <p className="text-lg font-semibold text-slate-900">{selectedThread.subject}</p>
                  <p className="text-sm text-slate-500">{selectedThread.supplierEmail}</p>
                  <p className="mt-2 text-xs text-slate-500">Linked booking <Link className="underline decoration-amber-300 underline-offset-4" to={`/bookings/${selectedThread.relatedBookingId ?? selectedThread.bookingId}`}>{shortId(selectedThread.relatedBookingId ?? selectedThread.bookingId)}</Link></p>
                </div>
                <div className="flex flex-wrap gap-2">
                  <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" isLoading={analyzeMutation.isPending} onClick={() => void analyzeMutation.mutateAsync(selectedThread.id)}>
                    Analyze with AI
                  </Button>
                  <Button type="button" className="bg-sky-600 hover:bg-sky-500" isLoading={generateTasksMutation.isPending} onClick={() => void generateTasksMutation.mutateAsync(selectedThread.id)}>
                    Generate Tasks
                  </Button>
                  <Button type="button" isLoading={draftReplyMutation.isPending} onClick={() => void draftReplyMutation.mutateAsync(selectedThread.id)}>
                    Suggest Reply
                  </Button>
                </div>
              </div>
              {latestInbound ? (
                <div className="mt-4 rounded-2xl border border-slate-200 bg-slate-50 p-4">
                  <div className="flex flex-wrap items-center gap-2">
                    {latestInbound.aiClassification ? <span className={`rounded-full px-2 py-1 text-xs font-medium ${classificationColors[latestInbound.aiClassification]}`}>{latestInbound.aiClassification}</span> : null}
                    <span className="text-xs text-slate-500">Confidence {Math.round((latestInbound.aiConfidence ?? 0) * 100)}%</span>
                    {latestInbound.requiresHumanReview ? <span className="rounded-full bg-amber-100 px-2 py-1 text-xs font-medium text-amber-700">Human review required</span> : null}
                  </div>
                  {latestInbound.aiSummary ? <p className="mt-2 text-sm text-slate-600">{latestInbound.aiSummary}</p> : null}
                </div>
              ) : null}
              {analyzeMutation.isError ? <p className="mt-3 text-sm text-red-600">{(analyzeMutation.error as Error).message}</p> : null}
              {generateTasksMutation.isError ? <p className="mt-3 text-sm text-red-600">{(generateTasksMutation.error as Error).message}</p> : null}
              {draftReplyMutation.isError ? <p className="mt-3 text-sm text-red-600">{(draftReplyMutation.error as Error).message}</p> : null}
            </Card>

            <div className="grid gap-6 xl:grid-cols-[1.05fr_0.95fr]">
              <Card title="Thread Timeline">
                <div className="space-y-3">
                  {selectedThread.messages.length === 0 ? <p className="text-sm text-slate-500">No messages yet.</p> : null}
                  {selectedThread.messages.map((message) => (
                    <div key={message.id} className="rounded-xl border border-slate-200 bg-slate-50 p-4">
                      <div className="flex flex-wrap items-center gap-2">
                        <span className={`rounded-full px-2 py-1 text-xs font-medium ${message.direction === 'Inbound' ? 'bg-amber-100 text-amber-700' : 'bg-slate-100 text-slate-700'}`}>{message.direction}</span>
                        {message.aiClassification ? <span className={`rounded-full px-2 py-1 text-xs font-medium ${classificationColors[message.aiClassification]}`}>{message.aiClassification}</span> : null}
                        <span className="text-xs text-slate-500">{formatDate(message.sentAt)}</span>
                      </div>
                      <p className="mt-2 text-sm font-semibold text-slate-900">{message.subject}</p>
                      <p className="mt-2 whitespace-pre-wrap text-sm text-slate-600">{message.bodyText}</p>
                      {message.aiSummary ? <p className="mt-3 rounded-lg bg-white px-3 py-2 text-xs text-slate-600"><span className="font-semibold text-slate-700">AI summary:</span> {message.aiSummary}</p> : null}
                    </div>
                  ))}
                </div>
              </Card>

              <Card title="Compose / Reply">
                <div className="space-y-3">
                  <div className="grid gap-3 md:grid-cols-2">
                    <label className="block text-sm">
                      <span className="mb-1 block font-medium text-slate-700">Direction</span>
                      <select className="w-full rounded border border-slate-300 px-3 py-2" value={composer.direction} onChange={(event) => setComposer((current) => ({ ...current, direction: event.target.value as EmailDirection }))}>
                        <option value="Outbound">Outbound</option>
                        <option value="Inbound">Inbound</option>
                      </select>
                    </label>
                    <label className="block text-sm">
                      <span className="mb-1 block font-medium text-slate-700">Subject</span>
                      <input className="w-full rounded border border-slate-300 px-3 py-2" value={composer.subject} onChange={(event) => setComposer((current) => ({ ...current, subject: event.target.value }))} />
                    </label>
                  </div>
                  <div className="grid gap-3 md:grid-cols-2">
                    <label className="block text-sm">
                      <span className="mb-1 block font-medium text-slate-700">Sender</span>
                      <input className="w-full rounded border border-slate-300 px-3 py-2" value={composer.sender} onChange={(event) => setComposer((current) => ({ ...current, sender: event.target.value }))} />
                    </label>
                    <label className="block text-sm">
                      <span className="mb-1 block font-medium text-slate-700">Recipients</span>
                      <input className="w-full rounded border border-slate-300 px-3 py-2" value={composer.recipients} onChange={(event) => setComposer((current) => ({ ...current, recipients: event.target.value }))} />
                    </label>
                  </div>
                  <label className="block text-sm">
                    <span className="mb-1 block font-medium text-slate-700">Body</span>
                    <textarea className="w-full rounded border border-slate-300 px-3 py-2" rows={6} value={composer.bodyText} onChange={(event) => setComposer((current) => ({ ...current, bodyText: event.target.value }))} />
                  </label>
                  <Button
                    type="button"
                    isLoading={addMessageMutation.isPending}
                    onClick={() => void addMessageMutation.mutateAsync({
                      threadId: selectedThread.id,
                      payload: {
                        direction: composer.direction,
                        subject: composer.subject,
                        bodyText: composer.bodyText,
                        sender: composer.sender,
                        recipients: composer.recipients,
                        sentAt: new Date().toISOString()
                      } as AddEmailMessageRequest
                    }).then(() => setComposer((current) => ({ ...current, bodyText: '' })))}
                  >
                    Save Message
                  </Button>
                  {addMessageMutation.isError ? <p className="text-sm text-red-600">{(addMessageMutation.error as Error).message}</p> : null}
                </div>
              </Card>
            </div>

            <Card title="Draft Replies">
              <div className="space-y-4">
                {selectedThread.drafts.length === 0 ? <p className="text-sm text-slate-500">No drafts yet for this thread.</p> : null}
                {selectedThread.drafts.map((draft) => {
                  const draftEdit = draftEdits[draft.id] ?? { subject: draft.subject, body: draft.body };
                  return (
                    <div key={draft.id} className="rounded-xl border border-slate-200 bg-slate-50 p-4">
                      <div className="mb-3 flex flex-wrap items-center gap-2">
                        <span className={`rounded-full px-2 py-1 text-xs font-medium ${draftStatusColors[draft.status]}`}>{draft.status}</span>
                        {draft.generatedByAi ? <span className="rounded-full bg-indigo-100 px-2 py-1 text-xs font-medium text-indigo-700">Generated by AI Forged</span> : null}
                        {draft.llmProvider ? <span className="text-xs text-slate-500">{draft.llmProvider} / {draft.llmModel}</span> : null}
                      </div>
                      <label className="block text-sm">
                        <span className="mb-1 block font-medium text-slate-700">Subject</span>
                        <input className="w-full rounded border border-slate-300 px-3 py-2" value={draftEdit.subject} onChange={(event) => setDraftEdits((current) => ({ ...current, [draft.id]: { ...draftEdit, subject: event.target.value } }))} />
                      </label>
                      <label className="mt-3 block text-sm">
                        <span className="mb-1 block font-medium text-slate-700">Body</span>
                        <textarea className="w-full rounded border border-slate-300 px-3 py-2" rows={7} value={draftEdit.body} onChange={(event) => setDraftEdits((current) => ({ ...current, [draft.id]: { ...draftEdit, body: event.target.value } }))} />
                      </label>
                      <div className="mt-3 flex flex-wrap gap-2">
                        <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => void updateEmailDraftMutation.mutateAsync({ id: draft.id, payload: draftEdit })}>
                          Save Draft
                        </Button>
                        <Button type="button" className="bg-sky-600 hover:bg-sky-500" disabled={draft.status !== 'Draft'} onClick={() => void approveDraftMutation.mutateAsync(draft.id)}>
                          Approve &amp; Send Prep
                        </Button>
                        <Button type="button" className="bg-emerald-600 hover:bg-emerald-500" disabled={draft.status !== 'Approved'} onClick={() => void sendDraftMutation.mutateAsync(draft.id)}>
                          Send Draft
                        </Button>
                      </div>
                    </div>
                  );
                })}
              </div>
              {updateEmailDraftMutation.isError ? <p className="mt-3 text-sm text-red-600">{(updateEmailDraftMutation.error as Error).message}</p> : null}
              {approveDraftMutation.isError ? <p className="mt-3 text-sm text-red-600">{(approveDraftMutation.error as Error).message}</p> : null}
              {sendDraftMutation.isError ? <p className="mt-3 text-sm text-red-600">{(sendDraftMutation.error as Error).message}</p> : null}
            </Card>
          </>
        )}
      </div>
    </div>
  );
};
