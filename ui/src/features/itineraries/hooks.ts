import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createItinerary } from '../../services/itinerariesApi';
import { CreateItineraryRequest } from '../../types/itinerary';
import { getItinerary } from '../../services/itinerariesApi';

export const useItinerary = (itineraryId?: string) =>
  useQuery({
    queryKey: ['itinerary', itineraryId],
    queryFn: () => getItinerary(itineraryId!),
    enabled: Boolean(itineraryId)
  });

export const useCreateItinerary = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateItineraryRequest) => createItinerary(payload),
    onSuccess: (itinerary) => {
      queryClient.setQueryData(['itinerary', itinerary.id], itinerary);
    }
  });
};
