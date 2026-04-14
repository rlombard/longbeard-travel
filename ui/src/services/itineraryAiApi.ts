import { apiClient } from './apiClient';
import {
  ApproveItineraryDraftRequest,
  ItineraryDraft,
  ItineraryDraftApprovalResponse,
  ProductAssistRequest,
  ProductAssistResponse,
  GenerateItineraryDraftRequest
} from '../types/itineraryAi';

export const getProductAssist = async (payload: ProductAssistRequest): Promise<ProductAssistResponse> => {
  const { data } = await apiClient.post<ProductAssistResponse>('/itineraries/ai/product-assist', payload);
  return data;
};

export const generateItineraryDraft = async (payload: GenerateItineraryDraftRequest): Promise<ItineraryDraft> => {
  const { data } = await apiClient.post<ItineraryDraft>('/itineraries/ai/draft', payload);
  return data;
};

export const approveItineraryDraft = async (draftId: string, payload: ApproveItineraryDraftRequest): Promise<ItineraryDraftApprovalResponse> => {
  const { data } = await apiClient.post<ItineraryDraftApprovalResponse>(`/itineraries/ai/drafts/${draftId}/approve`, payload);
  return data;
};
