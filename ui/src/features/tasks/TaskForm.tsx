import { FormEvent, useEffect, useMemo, useState } from 'react';
import { Button } from '../../components/Button';
import { FormInput } from '../../components/FormInput';

export interface TaskFormValues {
  bookingId?: string;
  bookingItemId?: string;
  title: string;
  description: string;
  dueDate: string;
  assignedToUserId: string;
}

interface Props {
  heading: string;
  submitLabel: string;
  initialValues: TaskFormValues;
  isSubmitting?: boolean;
  allowContextEditing?: boolean;
  contextLabel?: string;
  onSubmit: (values: TaskFormValues) => Promise<void> | void;
  onCancel?: () => void;
}

const inferLinkMode = (values: TaskFormValues) => (values.bookingItemId ? 'bookingItem' : 'booking');

export const TaskForm = ({
  heading,
  submitLabel,
  initialValues,
  isSubmitting,
  allowContextEditing = false,
  contextLabel,
  onSubmit,
  onCancel
}: Props) => {
  const [values, setValues] = useState<TaskFormValues>(initialValues);
  const [linkMode, setLinkMode] = useState<'booking' | 'bookingItem'>(inferLinkMode(initialValues));

  useEffect(() => {
    setValues(initialValues);
    setLinkMode(inferLinkMode(initialValues));
  }, [initialValues]);

  const contextSummary = useMemo(() => {
    if (contextLabel) {
      return contextLabel;
    }

    if (values.bookingItemId) {
      return `Booking item ${values.bookingItemId}`;
    }

    if (values.bookingId) {
      return `Booking ${values.bookingId}`;
    }

    return 'No booking context selected.';
  }, [contextLabel, values.bookingId, values.bookingItemId]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const payload: TaskFormValues = {
      ...values,
      bookingId: allowContextEditing && linkMode === 'bookingItem' ? undefined : values.bookingId || undefined,
      bookingItemId: allowContextEditing && linkMode === 'booking' ? undefined : values.bookingItemId || undefined,
      title: values.title.trim(),
      description: values.description.trim(),
      assignedToUserId: values.assignedToUserId.trim()
    };

    await onSubmit(payload);
  };

  return (
    <form className="space-y-4 rounded-xl border border-slate-200 bg-slate-50 p-4" onSubmit={(event) => void handleSubmit(event)}>
      <div className="flex items-center justify-between gap-3">
        <div>
          <h3 className="text-base font-semibold text-slate-900">{heading}</h3>
          <p className="text-sm text-slate-500">{contextSummary}</p>
        </div>
        {onCancel ? (
          <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={onCancel}>
            Cancel
          </Button>
        ) : null}
      </div>

      {allowContextEditing ? (
        <div className="space-y-3 rounded-lg border border-dashed border-slate-300 bg-white p-3">
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              className={`rounded-full px-3 py-1 text-sm font-medium ${linkMode === 'booking' ? 'bg-slate-900 text-white' : 'bg-slate-200 text-slate-700'}`}
              onClick={() => {
                setLinkMode('booking');
                setValues((current) => ({ ...current, bookingItemId: '' }));
              }}
            >
              Link to booking
            </button>
            <button
              type="button"
              className={`rounded-full px-3 py-1 text-sm font-medium ${linkMode === 'bookingItem' ? 'bg-slate-900 text-white' : 'bg-slate-200 text-slate-700'}`}
              onClick={() => {
                setLinkMode('bookingItem');
                setValues((current) => ({ ...current, bookingId: '' }));
              }}
            >
              Link to booking item
            </button>
          </div>

          {linkMode === 'booking' ? (
            <FormInput
              label="Booking ID"
              value={values.bookingId ?? ''}
              onChange={(event) => setValues((current) => ({ ...current, bookingId: event.target.value }))}
              required
            />
          ) : (
            <FormInput
              label="Booking Item ID"
              value={values.bookingItemId ?? ''}
              onChange={(event) => setValues((current) => ({ ...current, bookingItemId: event.target.value }))}
              required
            />
          )}
        </div>
      ) : null}

      <div className="grid gap-4 md:grid-cols-2">
        <FormInput
          label="Title"
          value={values.title}
          required
          onChange={(event) => setValues((current) => ({ ...current, title: event.target.value }))}
        />
        <FormInput
          label="Assigned User ID"
          value={values.assignedToUserId}
          required
          onChange={(event) => setValues((current) => ({ ...current, assignedToUserId: event.target.value }))}
        />
      </div>

      <label className="block text-sm">
        <span className="mb-1 block font-medium text-slate-700">Description</span>
        <textarea
          rows={4}
          className="w-full rounded border border-slate-300 px-3 py-2 text-sm focus:border-slate-500 focus:outline-none focus:ring-2 focus:ring-amber-200"
          value={values.description}
          onChange={(event) => setValues((current) => ({ ...current, description: event.target.value }))}
          placeholder="Add the supplier follow-up, handover, or blocker detail"
        />
      </label>

      <div className="grid gap-4 md:grid-cols-2">
        <FormInput
          label="Due Date"
          type="datetime-local"
          value={values.dueDate}
          onChange={(event) => setValues((current) => ({ ...current, dueDate: event.target.value }))}
        />
      </div>

      <div className="flex items-center gap-3">
        <Button type="submit" isLoading={isSubmitting}>{submitLabel}</Button>
        <span className="text-xs text-slate-500">Task ownership can be reassigned later without reloading the page.</span>
      </div>
    </form>
  );
};
