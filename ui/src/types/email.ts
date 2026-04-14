export type EmailDirection = 'Inbound' | 'Outbound';
export type EmailDraftStatus = 'Draft' | 'Approved' | 'Sent' | 'Rejected';
export type EmailDraftGeneratedBy = 'Human' | 'AI';
export type EmailClassificationType = 'ConfirmationReceived' | 'PartialConfirmation' | 'NeedsMoreInformation' | 'PricingChanged' | 'AvailabilityIssue' | 'NoActionNeeded' | 'HumanDecisionRequired' | 'Unclear';

export interface EmailMessage {
  id: string;
  emailThreadId: string;
  direction: EmailDirection;
  subject: string;
  bodyText: string;
  bodyHtml?: string | null;
  sender: string;
  recipients: string;
  sentAt: string;
  requiresHumanReview: boolean;
  aiSummary?: string | null;
  aiClassification?: EmailClassificationType | null;
  aiConfidence?: number | null;
  createdAt: string;
}

export interface EmailDraft {
  id: string;
  bookingId?: string | null;
  bookingItemId?: string | null;
  emailThreadId?: string | null;
  subject: string;
  body: string;
  status: EmailDraftStatus;
  generatedBy: EmailDraftGeneratedBy;
  generatedByAi: boolean;
  approvedByUserId?: string | null;
  approvedAt?: string | null;
  sentAt?: string | null;
  llmProvider?: string | null;
  llmModel?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface EmailThreadAiAnalysis {
  emailThreadId: string;
  summary: string;
  classification: EmailClassificationType;
  reason: string;
  confidence: number;
  requiresHumanReview: boolean;
  recommendedActions: string[];
  missingInformationItems: string[];
}

export interface EmailThread {
  id: string;
  bookingId?: string | null;
  bookingItemId?: string | null;
  relatedBookingId?: string | null;
  subject: string;
  supplierEmail: string;
  lastMessageAt?: string | null;
  createdAt: string;
  messages: EmailMessage[];
  drafts: EmailDraft[];
}

export interface CreateEmailThreadRequest {
  bookingItemId?: string;
  subject: string;
  supplierEmail: string;
  externalThreadId?: string;
}

export interface AddEmailMessageRequest {
  direction: EmailDirection;
  subject: string;
  bodyText: string;
  bodyHtml?: string;
  sender: string;
  recipients: string;
  sentAt: string;
}

export interface CreateEmailDraftRequest {
  bookingId?: string;
  bookingItemId?: string;
  emailThreadId?: string;
  subject: string;
  body: string;
}

export interface UpdateEmailDraftRequest {
  subject: string;
  body: string;
}
