import { OperationalTaskStatus } from './task';

export type TaskSuggestionState = 'PendingReview' | 'Accepted' | 'Rejected';

export interface TaskSuggestion {
  id: string;
  bookingId: string;
  bookingItemId?: string | null;
  title: string;
  description: string;
  suggestedStatus: OperationalTaskStatus;
  suggestedDueDate?: string | null;
  reason: string;
  confidence: number;
  requiresHumanReview: boolean;
  state: TaskSuggestionState;
  source?: string | null;
  acceptedTaskId?: string | null;
  reviewedByUserId?: string | null;
  reviewedAt?: string | null;
  createdAt: string;
  productName?: string | null;
  supplierName?: string | null;
}
