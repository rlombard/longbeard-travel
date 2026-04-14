import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  applyInvoiceRebate,
  getInvoice,
  getInvoices,
  ingestInvoice,
  recordInvoicePayment,
  relinkInvoice,
  updateInvoiceStatus
} from '../../services/invoicesApi';
import {
  ApplyInvoiceRebateRequest,
  InvoiceIngestionRequest,
  InvoiceListQuery,
  RecordInvoicePaymentRequest,
  RelinkInvoiceRequest,
  UpdateInvoiceStatusRequest
} from '../../types/invoice';

export const useInvoices = (query: InvoiceListQuery) =>
  useQuery({
    queryKey: ['invoices', query],
    queryFn: () => getInvoices(query)
  });

export const useInvoice = (invoiceId?: string) =>
  useQuery({
    queryKey: ['invoice', invoiceId],
    queryFn: () => getInvoice(invoiceId!),
    enabled: Boolean(invoiceId)
  });

export const useIngestInvoice = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: InvoiceIngestionRequest) => ingestInvoice(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
    }
  });
};

export const useUpdateInvoiceStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ invoiceId, payload }: { invoiceId: string; payload: UpdateInvoiceStatusRequest }) => updateInvoiceStatus(invoiceId, payload),
    onSuccess: (invoice) => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      queryClient.setQueryData(['invoice', invoice.id], invoice);
    }
  });
};

export const useRelinkInvoice = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ invoiceId, payload }: { invoiceId: string; payload: RelinkInvoiceRequest }) => relinkInvoice(invoiceId, payload),
    onSuccess: (invoice) => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      queryClient.setQueryData(['invoice', invoice.id], invoice);
    }
  });
};

export const useRecordInvoicePayment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ invoiceId, payload }: { invoiceId: string; payload: RecordInvoicePaymentRequest }) => recordInvoicePayment(invoiceId, payload),
    onSuccess: (invoice) => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      queryClient.setQueryData(['invoice', invoice.id], invoice);
    }
  });
};

export const useApplyInvoiceRebate = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ invoiceId, payload }: { invoiceId: string; payload: ApplyInvoiceRebateRequest }) => applyInvoiceRebate(invoiceId, payload),
    onSuccess: (invoice) => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      queryClient.setQueryData(['invoice', invoice.id], invoice);
    }
  });
};
