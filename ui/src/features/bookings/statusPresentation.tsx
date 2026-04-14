import { BookingItemStatus, BookingStatus } from '../../types/booking';

type Status = BookingStatus | BookingItemStatus;

const statusClasses: Record<Status, string> = {
  Draft: 'bg-slate-100 text-slate-700',
  Pending: 'bg-slate-100 text-slate-700',
  Requested: 'bg-orange-100 text-orange-700',
  Confirmed: 'bg-emerald-100 text-emerald-700',
  Cancelled: 'bg-rose-100 text-rose-700'
};

export const StatusBadge = ({ status }: { status: Status }) => (
  <span className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${statusClasses[status]}`}>{status}</span>
);

export const bookingStatusOptions = [
  { label: 'Draft', value: 'Draft' },
  { label: 'Confirmed', value: 'Confirmed' },
  { label: 'Cancelled', value: 'Cancelled' }
] as const;

export const bookingItemStatusOptions = [
  { label: 'Pending', value: 'Pending' },
  { label: 'Requested', value: 'Requested' },
  { label: 'Confirmed', value: 'Confirmed' },
  { label: 'Cancelled', value: 'Cancelled' }
] as const;
