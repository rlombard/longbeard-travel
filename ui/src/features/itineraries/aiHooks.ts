import { useMutation, useQueryClient } from '@tanstack/react-query';
import { approveItineraryDraft, generateItineraryDraft, getProductAssist } from '../../services/itineraryAiApi';
import { ApproveItineraryDraftRequest, GenerateItineraryDraftRequest, ProductAssistRequest } from '../../types/itineraryAi';

export const useProductAssist = () =>
  useMutation({
    mutationFn: (payload: ProductAssistRequest) => getProductAssist(payload)
  });

export const useGenerateItineraryDraft = () =>
  useMutation({
    mutationFn: (payload: GenerateItineraryDraftRequest) => generateItineraryDraft(payload)
  });

export const useApproveItineraryDraft = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ draftId, payload }: { draftId: string; payload: ApproveItineraryDraftRequest }) => approveItineraryDraft(draftId, payload),
    onSuccess: (result) => {
      queryClient.setQueryData(['itinerary', result.itinerary.id], result.itinerary);
    }
  });
};
