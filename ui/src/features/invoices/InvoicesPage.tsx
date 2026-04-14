import { FormEvent, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { MultiValueEditor } from '../../components/MultiValueEditor';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import { TextAreaField } from '../../components/TextAreaField';
import { useIngestInvoice, useInvoices } from './hooks';
import { InvoiceIngestionRequest, InvoiceLineItemRequest, InvoiceStatus } from '../../types/invoice';
import { invoiceStatusOptions, InvoiceStatusBadge } from './statusPresentation';
import { formatCurrency, formatDateOnly } from '../../utils/formatters';
import { Badge } from '../../components/Badge';

const emptyInvoice: InvoiceIngestionRequest = {
  sourceSystem: 'AI Forged',
  invoiceNumber: '',
  supplierName: '',
  bookingReference: '',
  invoiceDate: '',
  dueDate: '',
  currency: 'USD',
  subtotalAmount: 0,
  taxAmount: 0,
  totalAmount: 0,
  notes: '',
  rawExtractionPayloadJson: '',
  sourceSnapshotJson: '',
  extractionConfidence: 0.9,
  extractionIssues: [],
  unresolvedFields: [],
  lineItems: [],
  attachments: []
};

const emptyLineItem: InvoiceLineItemRequest = {
  description: '',
  quantity: 1,
  unitPrice: 0,
  taxAmount: 0,
  totalAmount: 0
};

export const InvoicesPage = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [status, setStatus] = useState<InvoiceStatus | ''>('');
  const [dueBefore, setDueBefore] = useState('');
  const [currency, setCurrency] = useState('');
  const [unpaidOnly, setUnpaidOnly] = useState(false);
  const [draft, setDraft] = useState<InvoiceIngestionRequest>(emptyInvoice);

  const invoicesQuery = useInvoices({
    status: status || undefined,
    dueBefore: dueBefore || undefined,
    unpaidOnly
  });
  const ingestMutation = useIngestInvoice();

  const filteredInvoices = useMemo(() => {
    return (invoicesQuery.data ?? []).filter((invoice) => {
      const haystack = [invoice.invoiceNumber, invoice.supplierName, invoice.bookingId, invoice.bookingItemId].join(' ').toLowerCase();
      const matchesSearch = !searchTerm || haystack.includes(searchTerm.toLowerCase());
      const matchesCurrency = !currency || invoice.currency.toLowerCase() === currency.toLowerCase();
      return matchesSearch && matchesCurrency;
    });
  }, [currency, invoicesQuery.data, searchTerm]);

  const actionCount = filteredInvoices.filter((invoice) => invoice.requiresHumanReview || invoice.status === 'Unmatched' || invoice.status === 'Overdue').length;

  const updateLineItem = (index: number, updates: Partial<InvoiceLineItemRequest>) => {
    setDraft((current) => ({
      ...current,
      lineItems: current.lineItems.map((item, itemIndex) => (itemIndex === index ? { ...item, ...updates } : item))
    }));
  };

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();
    await ingestMutation.mutateAsync(draft);
    setDraft(emptyInvoice);
  };

  return (
    <div className="space-y-6">
      <Card title="Invoices">
        <div className="mb-5 grid gap-4 lg:grid-cols-[1.25fr_0.75fr]">
          <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
            <div className="mb-3 flex flex-wrap items-center gap-2">
              <Badge tone="warning">{actionCount} need action</Badge>
              <Badge tone="info">{filteredInvoices.length} shown</Badge>
            </div>
            <div className="grid gap-3 md:grid-cols-5">
              <FormInput label="Search" value={searchTerm} onChange={(event) => setSearchTerm(event.target.value)} />
              <SelectDropdown label="Status" value={status} options={invoiceStatusOptions} onChange={(event) => setStatus(event.target.value as InvoiceStatus | '')} />
              <FormInput label="Due Before" type="date" value={dueBefore} onChange={(event) => setDueBefore(event.target.value)} />
              <FormInput label="Currency" value={currency} onChange={(event) => setCurrency(event.target.value)} />
              <label className="flex items-end gap-2 text-sm font-medium text-slate-700">
                <input type="checkbox" checked={unpaidOnly} onChange={(event) => setUnpaidOnly(event.target.checked)} />
                Unpaid only
              </label>
            </div>
            <p className="mt-3 text-xs text-slate-500">Search matches invoice number, supplier name, booking id, and booking item id. Server-side bulk search can tighten this later.</p>
          </div>

          <form className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm" onSubmit={onSubmit}>
            <div className="mb-3 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-slate-900">Manual intake / test ingest</h3>
              <Badge tone="ai">AI Forged push shape</Badge>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              <FormInput label="Source System" value={draft.sourceSystem} onChange={(event) => setDraft((current) => ({ ...current, sourceSystem: event.target.value }))} />
              <FormInput label="Invoice Number" value={draft.invoiceNumber ?? ''} onChange={(event) => setDraft((current) => ({ ...current, invoiceNumber: event.target.value }))} />
              <FormInput label="Supplier Name" value={draft.supplierName ?? ''} onChange={(event) => setDraft((current) => ({ ...current, supplierName: event.target.value }))} />
              <FormInput label="Booking Reference" value={draft.bookingReference ?? ''} onChange={(event) => setDraft((current) => ({ ...current, bookingReference: event.target.value }))} />
              <FormInput label="Invoice Date" type="date" required value={draft.invoiceDate} onChange={(event) => setDraft((current) => ({ ...current, invoiceDate: event.target.value }))} />
              <FormInput label="Due Date" type="date" value={draft.dueDate ?? ''} onChange={(event) => setDraft((current) => ({ ...current, dueDate: event.target.value }))} />
              <FormInput label="Currency" required value={draft.currency} onChange={(event) => setDraft((current) => ({ ...current, currency: event.target.value.toUpperCase() }))} />
              <FormInput label="Extraction Confidence" type="number" min={0} max={1} step={0.01} value={draft.extractionConfidence} onChange={(event) => setDraft((current) => ({ ...current, extractionConfidence: Number(event.target.value) || 0 }))} />
              <FormInput label="Subtotal" type="number" step={0.01} value={draft.subtotalAmount} onChange={(event) => setDraft((current) => ({ ...current, subtotalAmount: Number(event.target.value) || 0 }))} />
              <FormInput label="Tax" type="number" step={0.01} value={draft.taxAmount} onChange={(event) => setDraft((current) => ({ ...current, taxAmount: Number(event.target.value) || 0 }))} />
              <FormInput label="Total" type="number" step={0.01} value={draft.totalAmount} onChange={(event) => setDraft((current) => ({ ...current, totalAmount: Number(event.target.value) || 0 }))} />
              <FormInput label="Rebate Amount" type="number" step={0.01} value={draft.rebateAmount ?? ''} onChange={(event) => setDraft((current) => ({ ...current, rebateAmount: event.target.value ? Number(event.target.value) : null }))} />
            </div>

            <div className="mt-3 space-y-3">
              <TextAreaField label="Notes" rows={2} value={draft.notes ?? ''} onChange={(event) => setDraft((current) => ({ ...current, notes: event.target.value }))} />
              <MultiValueEditor label="Extraction Issues" values={draft.extractionIssues} onChange={(values) => setDraft((current) => ({ ...current, extractionIssues: values }))} placeholder="low confidence supplier, date unreadable" />
              <MultiValueEditor label="Unresolved Fields" values={draft.unresolvedFields} onChange={(values) => setDraft((current) => ({ ...current, unresolvedFields: values }))} placeholder="bookingReference, dueDate" />
            </div>

            <div className="mt-4 rounded-xl border border-slate-200 bg-slate-50 p-3">
              <div className="mb-2 flex items-center justify-between">
                <p className="text-sm font-medium text-slate-900">Line items</p>
                <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setDraft((current) => ({ ...current, lineItems: [...current.lineItems, emptyLineItem] }))}>
                  Add line item
                </Button>
              </div>
              <div className="space-y-3">
                {draft.lineItems.map((item, index) => (
                  <div key={`${item.description}-${index}`} className="grid gap-3 md:grid-cols-5">
                    <FormInput label="Description" value={item.description} onChange={(event) => updateLineItem(index, { description: event.target.value })} />
                    <FormInput label="Qty" type="number" min={0} step={0.01} value={item.quantity} onChange={(event) => updateLineItem(index, { quantity: Number(event.target.value) || 0 })} />
                    <FormInput label="Unit Price" type="number" step={0.01} value={item.unitPrice} onChange={(event) => updateLineItem(index, { unitPrice: Number(event.target.value) || 0 })} />
                    <FormInput label="Tax" type="number" step={0.01} value={item.taxAmount} onChange={(event) => updateLineItem(index, { taxAmount: Number(event.target.value) || 0 })} />
                    <FormInput label="Total" type="number" step={0.01} value={item.totalAmount} onChange={(event) => updateLineItem(index, { totalAmount: Number(event.target.value) || 0 })} />
                  </div>
                ))}
              </div>
            </div>

            <div className="mt-4 flex items-center gap-3">
              <Button isLoading={ingestMutation.isPending}>Ingest invoice</Button>
              {ingestMutation.isSuccess ? <span className="text-sm text-emerald-700">Created {ingestMutation.data.invoiceId}</span> : null}
            </div>
            {ingestMutation.isError ? <p className="mt-3 text-sm text-rose-600">{(ingestMutation.error as Error).message}</p> : null}
          </form>
        </div>

        {invoicesQuery.isLoading ? <p className="text-sm text-slate-500">Loading invoices...</p> : null}
        {invoicesQuery.isError ? <p className="text-sm text-rose-600">{(invoicesQuery.error as Error).message}</p> : null}
        {!invoicesQuery.isLoading && filteredInvoices.length === 0 ? <p className="text-sm text-slate-500">No invoices match the current filters.</p> : null}
        {filteredInvoices.length > 0 ? (
          <Table headers={['Invoice', 'Supplier', 'Status', 'Invoice Date', 'Due', 'Amount', 'Action']}>
            {filteredInvoices.map((invoice) => (
              <tr key={invoice.id} className="border-t border-slate-200">
                <td className="px-3 py-3">
                  <p className="font-medium text-slate-900">{invoice.invoiceNumber ?? 'No invoice number'}</p>
                  <p className="font-mono text-xs text-slate-500">{invoice.id}</p>
                </td>
                <td className="px-3 py-3">
                  <p className="text-slate-700">{invoice.supplierName}</p>
                  {invoice.requiresHumanReview ? <Badge tone="warning">Review required</Badge> : null}
                </td>
                <td className="px-3 py-3"><InvoiceStatusBadge status={invoice.status} /></td>
                <td className="px-3 py-3 text-slate-700">{formatDateOnly(invoice.invoiceDate)}</td>
                <td className="px-3 py-3 text-slate-700">{formatDateOnly(invoice.dueDate)}</td>
                <td className="px-3 py-3">
                  <p className="text-slate-900">{formatCurrency(invoice.totalAmount, invoice.currency)}</p>
                  <p className="text-xs text-slate-500">Outstanding {formatCurrency(invoice.outstandingAmount, invoice.currency)}</p>
                </td>
                <td className="px-3 py-3">
                  <Link className="text-sm font-medium text-slate-900 underline decoration-amber-300 underline-offset-4" to={`/invoices/${invoice.id}`}>
                    Review invoice
                  </Link>
                </td>
              </tr>
            ))}
          </Table>
        ) : null}
      </Card>
    </div>
  );
};
