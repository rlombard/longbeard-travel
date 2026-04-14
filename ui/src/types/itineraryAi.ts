import type { Itinerary } from './itinerary';
import type { ProductType } from './product';

export type ItineraryDraftStatus = 'Draft' | 'Approved' | 'Rejected';

export interface ProductAssistRequest {
  destination?: string;
  region?: string;
  startDate?: string;
  endDate?: string;
  season?: string;
  travellerCount?: number;
  budgetLevel?: string;
  preferredCurrency?: string;
  travelStyle?: string;
  interests: string[];
  accommodationPreference?: string;
  specialConstraints: string[];
  productTypes: ProductType[];
  customerBrief?: string;
  maxResults: number;
}

export interface ProductRecommendation {
  productId: string;
  productName: string;
  supplierName: string;
  productType: ProductType;
  matchScore: number;
  reason: string;
  warnings: string[];
  assumptionFlags: string[];
  missingData: string[];
}

export interface ProductAssistResponse {
  candidateCount: number;
  returnedCount: number;
  assumptions: string[];
  recommendations: ProductRecommendation[];
}

export interface GenerateItineraryDraftRequest {
  destination?: string;
  region?: string;
  startDate?: string;
  endDate?: string;
  duration?: number;
  season?: string;
  travellerCount?: number;
  budgetLevel?: string;
  preferredCurrency?: string;
  travelStyle?: string;
  interests: string[];
  accommodationPreference?: string;
  specialConstraints: string[];
  customerBrief?: string;
}

export interface ItineraryDraftItem {
  id: string;
  dayNumber: number;
  sequence: number;
  title: string;
  productId?: string | null;
  productName?: string | null;
  supplierName?: string | null;
  quantity: number;
  notes?: string | null;
  confidence: number;
  reason: string;
  isUnresolved: boolean;
  warnings: string[];
  missingData: string[];
}

export interface ItineraryDraft {
  id: string;
  status: ItineraryDraftStatus;
  proposedStartDate?: string | null;
  duration: number;
  customerBrief?: string | null;
  llmProvider?: string | null;
  llmModel?: string | null;
  persistedItineraryId?: string | null;
  createdAt: string;
  updatedAt: string;
  approvedAt?: string | null;
  assumptions: string[];
  caveats: string[];
  dataGaps: string[];
  items: ItineraryDraftItem[];
}

export interface ApproveItineraryDraftItemRequest {
  dayNumber: number;
  productId: string;
  quantity: number;
  notes?: string | null;
}

export interface ApproveItineraryDraftRequest {
  startDate?: string;
  duration?: number;
  decisionNotes?: string;
  items: ApproveItineraryDraftItemRequest[];
}

export interface ItineraryDraftApprovalResponse {
  draftId: string;
  approvalRequestId: string;
  approvedAt: string;
  itinerary: Itinerary;
}
