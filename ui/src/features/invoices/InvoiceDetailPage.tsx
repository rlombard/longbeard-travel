import { FormEvent, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import { TextAreaField } from '../../components/TextAreaField';
import { useBookings, useBooking } from '../bookings/hooks';
import { useSuppliers } from '../suppliers/hooks';
import { useApplyInvoiceRebate, useInvoice, useRecordInvoicePayment, useRelinkInvoice, useUpdateInvoiceStatus } from './hooks';
import { invoiceStatusOptions, InvoiceStatusBadge } from './statusPresentation';
import { InvoiceStatus, RecordInvoicePaymentRequest, RelinkInvoiceRequest } from '../../types/invoice';
import { formatCurrency, formatDateOnly, formatDateTime } from '../../utils/formatters';
import { Badge } from '../../components/Badge';

const emptyPayment: RecordInvoicePaymentRequest = {
  amount: 0,
  currency: 'USD',
  paidAt: new Date().toISOString().slice(0, 16),
  paymentMethod: '',
  notes: '',
  externalPaymentReference: '',
  metadataJson: ''
};

export const InvoiceDetailPage = () => {
  const { invoiceId } = useParams();
  const navigate = useNavigate();
  const invoiceQuery = useInvoice(invoiceId);
  const suppliersQuery = useSuppliers();
  const bookingsQuery = useBookings();
  const selectedBookingQuery = useBooking(invoiceQuery.data?.bookingId ?? undefined);
  const updateStatusMutation = useUpdateInvoiceStatus();
  const relinkMutation = useRelinkInvoice();
  const paymentMutation = useRecordInvoicePayment();
  const rebateMutation = useApplyInvoiceRebate();

  const [status, setStatus] = useState<InvoiceStatus>('PendingReview');
  const [statusNotes, setStatusNotes] = useState('');
  const [linkDraft, setLinkDraft] = useState<RelinkInvoiceRequest>({});
  const [paymentDraft, setPaymentDraft] = useState<RecordInvoicePaymentRequest>(emptyPayment);
  const [rebateNotes, setRebateNotes] = useState('');

  useEffect(() => {
    if (!invoiceQuery.data) {
      return;
    }

    setStatus(invoiceQuery.data.status);
    setLinkDraft({
      supplierId: invoiceQuery.data.supplierId ?? null,
      supplierName: invoiceQuery.data.supplierName ?? null,
      bookingId: invoiceQuery.data.bookingId ?? null,
      bookingItemId: invoiceQuery.data.bookingItemId ?? null,
      quoteId: invoiceQuery.data.quoteId ?? null,
      emailThreadId: invoiceQuery.data.emailThreadId ?? null,
      notes: ''
    });
    setPaymentDraft((current) => ({
      ...current,
      currency: invoiceQuery.data.currency,
      amount: invoiceQuery.data.outstandingAmount
    }));
  }, [invoiceQuery.data]);

  const bookingItemOptions = useMemo(() => {
    if (!selectedBookingQuery.data) {
      return [{ label: 'No booking item', value: '' }];
    }

    return [
      { label: 'No booking item', value: '' },
      ...selectedBookingQuery.data.items.map((item) => ({ label: `${item.productName} (${item.id.slice(0, 8)})`, value: item.id }))
    ];
  }, [selectedBookingQuery.data]);

  const supplierOptions = useMemo(
    () => [{ label: 'No supplier link', value: '' }, ...(suppliersQuery.data ?? []).map((supplier) => ({ label: supplier.name, value: supplier.id }))],
    [suppliersQuery.data]
  );
  const bookingOptions = useMemo(
    () => [{ label: 'No booking link', value: '' }, ...(bookingsQuery.data ?? []).map((booking) => ({ label: `${booking.id.slice(0, 8)} ${booking.leadCustomerName ?? ''}`.trim(), value: booking.id }))],
    [bookingsQuery.data]
  );

  const onStatusSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!invoiceId) {
      return;
    }

    await updateStatusMutation.mutateAsync({
      invoiceId,
      payload: {
        status,
        notes: statusNotes || undefined
      }
    });
    setStatusNotes('');
  };

  const onRelinkSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!invoiceId) {
      return;
    }

    await relinkMutation.mutateAsync({
      invoiceId,
      payload: linkDraft
    });
  };

  const onPaymentSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!invoiceId) {
      return;
    }

    await paymentMutation.mutateAsync({
      invoiceId,
      payload: paymentDraft
    });
    setPaymentDraft((current) => ({
      ...current,
      amount: 0,
      externalPaymentReference: '',
      notes: '',
      metadataJson: ''
    }));
  };

  if (invoiceQuery.isLoading) {
    return <Card title="Invoice Review"><p className="text-sm text-slate-500">Loading invoice...</p></Card>;
  }

  if (invoiceQuery.isError || !invoiceQuery.data) {
    return (
      <Card title="Invoice Review">
        <p className="text-sm text-rose-600">{invoiceQuery.isError ? (invoiceQuery.error as Error).message : 'Invoice not found.'}</p>
        <div className="mt-4">
          <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => navigate('/invoices')}>
            Back to invoices
          </Button>
        </div>
      </Card>
    );
  }

  const invoice = invoiceQuery.data;

  return (
    <div className="space-y-6">
      <Card title="Invoice Detail">
        <div className="mb-5 flex flex-wrap items-center gap-2">
          <InvoiceStatusBadge status={invoice.status} />
          {invoice.requiresHumanReview ? <Badge tone="warning">Human review required</Badge> : null}
          <Badge tone="info">Confidence {Math.round(invoice.extractionConfidence * 100)}%</Badge>
          {invoice.reviewTaskId ? <Badge tone="warning">Task {invoice.reviewTaskId.slice(0, 8)}</Badge> : null}
        </div>

        <div className="grid gap-6 xl:grid-cols-[1fr_1fr]">
          <div className="space-y-4">
            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
              <h3 className="mb-3 text-sm font-semibold text-slate-900">Summary</h3>
              <div className="grid gap-3 md:grid-cols-2">
                <p className="text-sm text-slate-700"><strong>Invoice:</strong> {invoice.invoiceNumber ?? 'No invoice number'}</p>
                <p className="text-sm text-slate-700"><strong>Source:</strong> {invoice.sourceSystem}</p>
                <p className="text-sm text-slate-700"><strong>Supplier:</strong> {invoice.supplierName}</p>
                <p className="text-sm text-slate-700"><strong>Invoice Date:</strong> {formatDateOnly(invoice.invoiceDate)}</p>
                <p className="text-sm text-slate-700"><strong>Due:</strong> {formatDateOnly(invoice.dueDate)}</p>
                <p className="text-sm text-slate-700"><strong>Received:</strong> {formatDateTime(invoice.receivedAt)}</p>
                <p className="text-sm text-slate-700"><strong>Total:</strong> {formatCurrency(invoice.totalAmount, invoice.currency)}</p>
                <p className="text-sm text-slate-700"><strong>Outstanding:</strong> {formatCurrency(invoice.outstandingAmount, invoice.currency)}</p>
                <p className="text-sm text-slate-700"><strong>Booking:</strong> <span className="font-mono text-xs">{invoice.bookingId ?? '-'}</span></p>
                <p className="text-sm text-slate-700"><strong>Booking Item:</strong> <span className="font-mono text-xs">{invoice.bookingItemId ?? '-'}</span></p>
                <p className="text-sm text-slate-700"><strong>Quote:</strong> <span className="font-mono text-xs">{invoice.quoteId ?? '-'}</span></p>
                <p className="text-sm text-slate-700"><strong>Email Thread:</strong> <span className="font-mono text-xs">{invoice.emailThreadId ?? '-'}</span></p>
              </div>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
              <h3 className="mb-3 text-sm font-semibold text-slate-900">Line items</h3>
              {invoice.lineItems.length === 0 ? <p className="text-sm text-slate-500">No line items stored.</p> : null}
              {invoice.lineItems.length > 0 ? (
                <Table headers={['Description', 'Service Date', 'Qty', 'Unit Price', 'Total']}>
                  {invoice.lineItems.map((item) => (
                    <tr key={item.id} className="border-t border-slate-200">
                      <td className="px-3 py-2 text-slate-700">{item.description}</td>
                      <td className="px-3 py-2 text-slate-700">{formatDateOnly(item.serviceDate)}</td>
                      <td className="px-3 py-2 text-slate-700">{item.quantity}</td>
                      <td className="px-3 py-2 text-slate-700">{formatCurrency(item.unitPrice, invoice.currency)}</td>
                      <td className="px-3 py-2 text-slate-700">{formatCurrency(item.totalAmount, invoice.currency)}</td>
                    </tr>
                  ))}
                </Table>
              ) : null}
            </div>

            <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
              <h3 className="mb-3 text-sm font-semibold text-slate-900">Extraction review</h3>
              <div className="space-y-3">
                <div>
                  <p className="mb-2 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Issues</p>
                  <div className="flex flex-wrap gap-2">
                    {invoice.extractionIssues.length === 0 ? <Badge tone="success">No extraction issues flagged</Badge> : invoice.extractionIssues.map((issue) => <Badge key={issue} tone="warning">{issue}</Badge>)}
                  </div>
                </div>
                <div>
                  <p className="mb-2 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Unresolved fields</p>
                  <div className="flex flex-wrap gap-2">
                    {invoice.unresolvedFields.length === 0 ? <Badge tone="success">No unresolved fields</Badge> : invoice.unresolvedFields.map((field) => <Badge key={field} tone="danger">{field}</Badge>)}
                  </div>
                </div>
                <p className="text-xs text-slate-500">Raw extraction payload and source snapshot are stored backend-side but not yet exposed on this UI contract.</p>
              </div>
            </div>
          </div>

          <div className="space-y-4">
            <form className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm" onSubmit={onStatusSubmit}>
              <h3 className="mb-3 text-sm font-semibold text-slate-900">Workflow status</h3>
              <div className="grid gap-3 md:grid-cols-2">
                <SelectDropdown label="Status" value={status} options={invoiceStatusOptions.filter((option) => option.value).map((option) => ({ label: option.label, value: option.value }))} onChange={(event) => setStatus(event.target.value as InvoiceStatus)} />
                <TextAreaField label="Notes" rows={2} value={statusNotes} onChange={(event) => setStatusNotes(event.target.value)} />
              </div>
              <div className="mt-3 flex gap-3">
                <Button isLoading={updateStatusMutation.isPending}>Update status</Button>
                <Button
                  type="button"
                  className="bg-amber-600 hover:bg-amber-500"
                  isLoading={rebateMutation.isPending}
                  onClick={() => void rebateMutation.mutateAsync({ invoiceId: invoice.id, payload: { notes: rebateNotes || undefined } })}
                >
                  Apply rebate
                </Button>
              </div>
              <TextAreaField label="Rebate Notes" rows={2} value={rebateNotes} onChange={(event) => setRebateNotes(event.target.value)} />
            </form>

            <form className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm" onSubmit={onRelinkSubmit}>
              <h3 className="mb-3 text-sm font-semibold text-slate-900">Manual review and relink</h3>
              <div className="grid gap-3 md:grid-cols-2">
                <SelectDropdown label="Supplier" value={linkDraft.supplierId ?? ''} options={supplierOptions} onChange={(event) => {
                  const supplier = (suppliersQuery.data ?? []).find((entry) => entry.id === event.target.value);
                  setLinkDraft((current) => ({ ...current, supplierId: event.target.value || null, supplierName: supplier?.name ?? current.supplierName }));
                }} />
                <SelectDropdown label="Booking" value={linkDraft.bookingId ?? ''} options={bookingOptions} onChange={(event) => setLinkDraft((current) => ({ ...current, bookingId: event.target.value || null, bookingItemId: null }))} />
                <SelectDropdown label="Booking Item" value={linkDraft.bookingItemId ?? ''} options={bookingItemOptions} onChange={(event) => setLinkDraft((current) => ({ ...current, bookingItemId: event.target.value || null }))} />
                <FormInput label="Quote ID" value={linkDraft.quoteId ?? ''} onChange={(event) => setLinkDraft((current) => ({ ...current, quoteId: event.target.value || null }))} />
                <FormInput label="Email Thread ID" value={linkDraft.emailThreadId ?? ''} onChange={(event) => setLinkDraft((current) => ({ ...current, emailThreadId: event.target.value || null }))} />
                <TextAreaField label="Review Notes" rows={2} value={linkDraft.notes ?? ''} onChange={(event) => setLinkDraft((current) => ({ ...current, notes: event.target.value }))} />
              </div>
              <div className="mt-3 flex items-center gap-3">
                <Button isLoading={relinkMutation.isPending}>Save links</Button>
                <p className="text-xs text-slate-500">Use this when matching was ambiguous or incomplete.</p>
              </div>
            </form>

            <form className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm" onSubmit={onPaymentSubmit}>
              <h3 className="mb-3 text-sm font-semibold text-slate-900">Payment prep</h3>
              <div className="grid gap-3 md:grid-cols-2">
                <FormInput label="Amount" type="number" step={0.01} value={paymentDraft.amount} onChange={(event) => setPaymentDraft((current) => ({ ...current, amount: Number(event.target.value) || 0 }))} />
                <FormInput label="Currency" value={paymentDraft.currency} onChange={(event) => setPaymentDraft((current) => ({ ...current, currency: event.target.value.toUpperCase() }))} />
                <FormInput label="Paid At" type="datetime-local" value={paymentDraft.paidAt} onChange={(event) => setPaymentDraft((current) => ({ ...current, paidAt: event.target.value }))} />
                <FormInput label="Payment Method" value={paymentDraft.paymentMethod ?? ''} onChange={(event) => setPaymentDraft((current) => ({ ...current, paymentMethod: event.target.value }))} />
                <FormInput label="Payment Reference" value={paymentDraft.externalPaymentReference ?? ''} onChange={(event) => setPaymentDraft((current) => ({ ...current, externalPaymentReference: event.target.value }))} />
                <FormInput label="Metadata JSON" value={paymentDraft.metadataJson ?? ''} onChange={(event) => setPaymentDraft((current) => ({ ...current, metadataJson: event.target.value }))} />
              </div>
              <TextAreaField label="Notes" rows={2} value={paymentDraft.notes ?? ''} onChange={(event) => setPaymentDraft((current) => ({ ...current, notes: event.target.value }))} />
              <div className="mt-3 flex items-center gap-3">
                <Button isLoading={paymentMutation.isPending}>Record payment</Button>
                <p className="text-xs text-slate-500">No gateway yet. This records operator payment prep data only.</p>
              </div>
            </form>

            <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
              <h3 className="mb-3 text-sm font-semibold text-slate-900">Payment history</h3>
              {invoice.paymentRecords.length === 0 ? <p className="text-sm text-slate-500">No payment records yet.</p> : null}
              {invoice.paymentRecords.length > 0 ? (
                <Table headers={['Paid At', 'Amount', 'Method', 'Reference', 'Recorded By']}>
                  {invoice.paymentRecords.map((record) => (
                    <tr key={record.id} className="border-t border-slate-200">
                      <td className="px-3 py-2 text-slate-700">{formatDateTime(record.paidAt)}</td>
                      <td className="px-3 py-2 text-slate-700">{formatCurrency(record.amount, record.currency)}</td>
                      <td className="px-3 py-2 text-slate-700">{record.paymentMethod ?? '-'}</td>
                      <td className="px-3 py-2 text-slate-700">{record.externalPaymentReference ?? '-'}</td>
                      <td className="px-3 py-2 font-mono text-xs text-slate-500">{record.recordedByUserId}</td>
                    </tr>
                  ))}
                </Table>
              ) : null}
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
};
