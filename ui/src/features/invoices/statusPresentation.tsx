import { Badge } from '../../components/Badge';
import { InvoiceStatus } from '../../types/invoice';

const toneByStatus: Record<InvoiceStatus, 'neutral' | 'info' | 'success' | 'warning' | 'danger'> = {
  Draft: 'neutral',
  Received: 'info',
  Matched: 'info',
  Unmatched: 'warning',
  PendingReview: 'warning',
  Approved: 'success',
  Rejected: 'danger',
  Unpaid: 'warning',
  PartiallyPaid: 'info',
  Paid: 'success',
  Overdue: 'danger',
  RebatePending: 'warning',
  RebateApplied: 'success',
  Cancelled: 'neutral'
};

export const invoiceStatusOptions: { label: InvoiceStatus | 'All'; value: InvoiceStatus | '' }[] = [
  { label: 'All', value: '' },
  { label: 'Draft', value: 'Draft' },
  { label: 'Received', value: 'Received' },
  { label: 'Matched', value: 'Matched' },
  { label: 'Unmatched', value: 'Unmatched' },
  { label: 'PendingReview', value: 'PendingReview' },
  { label: 'Approved', value: 'Approved' },
  { label: 'Rejected', value: 'Rejected' },
  { label: 'Unpaid', value: 'Unpaid' },
  { label: 'PartiallyPaid', value: 'PartiallyPaid' },
  { label: 'Paid', value: 'Paid' },
  { label: 'Overdue', value: 'Overdue' },
  { label: 'RebatePending', value: 'RebatePending' },
  { label: 'RebateApplied', value: 'RebateApplied' },
  { label: 'Cancelled', value: 'Cancelled' }
];

export const InvoiceStatusBadge = ({ status }: { status: InvoiceStatus }) => <Badge tone={toneByStatus[status]}>{status}</Badge>;
