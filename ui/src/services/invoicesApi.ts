import { apiClient } from './apiClient';
import {
  ApplyInvoiceRebateRequest,
  Invoice,
  InvoiceIngestionRequest,
  InvoiceIngestionResponse,
  InvoiceListItem,
  InvoiceListQuery,
  RecordInvoicePaymentRequest,
  RelinkInvoiceRequest,
  UpdateInvoiceStatusRequest
} from '../types/invoice';

const buildParams = (query: InvoiceListQuery) => {
  const params = new URLSearchParams();

  Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') {
      return;
    }

    params.set(key, String(value));
  });

  return params;
};

export const ingestInvoice = async (payload: InvoiceIngestionRequest): Promise<InvoiceIngestionResponse> => {
  const { data } = await apiClient.post<InvoiceIngestionResponse>('/invoices/ingest', payload);
  return data;
};

export const getInvoices = async (query: InvoiceListQuery = {}): Promise<InvoiceListItem[]> => {
  const { data } = await apiClient.get<InvoiceListItem[]>('/invoices', { params: buildParams(query) });
  return Array.isArray(data) ? data : [];
};

export const getInvoice = async (invoiceId: string): Promise<Invoice> => {
  const { data } = await apiClient.get<Invoice>(`/invoices/${invoiceId}`);
  return data;
};

export const updateInvoiceStatus = async (invoiceId: string, payload: UpdateInvoiceStatusRequest): Promise<Invoice> => {
  const { data } = await apiClient.patch<Invoice>(`/invoices/${invoiceId}/status`, payload);
  return data;
};

export const relinkInvoice = async (invoiceId: string, payload: RelinkInvoiceRequest): Promise<Invoice> => {
  const { data } = await apiClient.patch<Invoice>(`/invoices/${invoiceId}/links`, payload);
  return data;
};

export const recordInvoicePayment = async (invoiceId: string, payload: RecordInvoicePaymentRequest): Promise<Invoice> => {
  const { data } = await apiClient.post<Invoice>(`/invoices/${invoiceId}/payments`, payload);
  return data;
};

export const applyInvoiceRebate = async (invoiceId: string, payload: ApplyInvoiceRebateRequest): Promise<Invoice> => {
  const { data } = await apiClient.post<Invoice>(`/invoices/${invoiceId}/rebate/apply`, payload);
  return data;
};
