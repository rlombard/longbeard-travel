import { useMutation } from '@tanstack/react-query';
import { createItinerary } from '../../services/itinerariesApi';
import { CreateItineraryRequest } from '../../types/itinerary';

export const useCreateItinerary = () =>
  useMutation({
    mutationFn: (payload: CreateItineraryRequest) => createItinerary(payload)
  });
