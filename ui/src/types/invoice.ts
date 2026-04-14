export type InvoiceStatus =
  | 'Draft'
  | 'Received'
  | 'Matched'
  | 'Unmatched'
  | 'PendingReview'
  | 'Approved'
  | 'Rejected'
  | 'Unpaid'
  | 'PartiallyPaid'
  | 'Paid'
  | 'Overdue'
  | 'RebatePending'
  | 'RebateApplied'
  | 'Cancelled';

export interface InvoiceAttachmentRequest {
  externalFileReference?: string | null;
  fileName: string;
  contentType?: string | null;
  sourceUrl?: string | null;
  metadataJson?: string | null;
}

export interface InvoiceLineItemRequest {
  externalLineReference?: string | null;
  bookingItemId?: string | null;
  bookingItemReference?: string | null;
  description: string;
  serviceDate?: string | null;
  quantity: number;
  unitPrice: number;
  taxAmount: number;
  totalAmount: number;
  notes?: string | null;
}

export interface InvoiceIngestionRequest {
  sourceSystem: string;
  externalSourceReference?: string | null;
  invoiceNumber?: string | null;
  supplierId?: string | null;
  supplierReference?: string | null;
  supplierName?: string | null;
  bookingId?: string | null;
  bookingReference?: string | null;
  bookingItemId?: string | null;
  bookingItemReference?: string | null;
  quoteId?: string | null;
  quoteReference?: string | null;
  emailThreadId?: string | null;
  invoiceDate: string;
  dueDate?: string | null;
  currency: string;
  subtotalAmount: number;
  taxAmount: number;
  totalAmount: number;
  rebateAmount?: number | null;
  notes?: string | null;
  rawExtractionPayloadJson?: string | null;
  sourceSnapshotJson?: string | null;
  extractionConfidence: number;
  extractionIssues: string[];
  unresolvedFields: string[];
  lineItems: InvoiceLineItemRequest[];
  attachments: InvoiceAttachmentRequest[];
}

export interface InvoiceIngestionResponse {
  invoiceId: string;
  wasExisting: boolean;
  supplierId?: string | null;
  bookingId?: string | null;
  bookingItemId?: string | null;
  quoteId?: string | null;
  emailThreadId?: string | null;
  reviewTaskId?: string | null;
  finalStatus: InvoiceStatus;
  unresolvedFields: string[];
  warnings: string[];
}

export interface InvoiceListItem {
  id: string;
  invoiceNumber?: string | null;
  supplierName: string;
  supplierId?: string | null;
  bookingId?: string | null;
  bookingItemId?: string | null;
  invoiceDate: string;
  dueDate?: string | null;
  currency: string;
  totalAmount: number;
  amountPaid: number;
  outstandingAmount: number;
  requiresHumanReview: boolean;
  status: InvoiceStatus;
}

export interface InvoiceLineItem {
  id: string;
  bookingItemId?: string | null;
  description: string;
  serviceDate?: string | null;
  quantity: number;
  unitPrice: number;
  taxAmount: number;
  totalAmount: number;
  notes?: string | null;
}

export interface InvoiceAttachment {
  id: string;
  externalFileReference?: string | null;
  fileName: string;
  contentType?: string | null;
  sourceUrl?: string | null;
  createdAt: string;
}

export interface PaymentRecord {
  id: string;
  externalPaymentReference?: string | null;
  amount: number;
  currency: string;
  paidAt: string;
  paymentMethod?: string | null;
  notes?: string | null;
  recordedByUserId: string;
  createdAt: string;
}

export interface Invoice {
  id: string;
  sourceSystem: string;
  externalSourceReference?: string | null;
  invoiceNumber?: string | null;
  supplierId?: string | null;
  supplierName: string;
  bookingId?: string | null;
  bookingItemId?: string | null;
  quoteId?: string | null;
  emailThreadId?: string | null;
  reviewTaskId?: string | null;
  invoiceDate: string;
  dueDate?: string | null;
  currency: string;
  subtotalAmount: number;
  taxAmount: number;
  totalAmount: number;
  rebateAmount?: number | null;
  amountPaid: number;
  outstandingAmount: number;
  notes?: string | null;
  extractionConfidence: number;
  extractionIssues: string[];
  unresolvedFields: string[];
  requiresHumanReview: boolean;
  status: InvoiceStatus;
  receivedAt: string;
  createdAt: string;
  updatedAt: string;
  lineItems: InvoiceLineItem[];
  attachments: InvoiceAttachment[];
  paymentRecords: PaymentRecord[];
}

export interface InvoiceListQuery {
  supplierId?: string;
  bookingId?: string;
  bookingItemId?: string;
  quoteId?: string;
  status?: InvoiceStatus;
  dueBefore?: string;
  unpaidOnly?: boolean;
}

export interface UpdateInvoiceStatusRequest {
  status: InvoiceStatus;
  notes?: string;
}

export interface RelinkInvoiceRequest {
  supplierId?: string | null;
  supplierName?: string | null;
  bookingId?: string | null;
  bookingItemId?: string | null;
  quoteId?: string | null;
  emailThreadId?: string | null;
  notes?: string | null;
}

export interface RecordInvoicePaymentRequest {
  externalPaymentReference?: string | null;
  amount: number;
  currency: string;
  paidAt: string;
  paymentMethod?: string | null;
  notes?: string | null;
  metadataJson?: string | null;
}

export interface ApplyInvoiceRebateRequest {
  notes?: string | null;
}
