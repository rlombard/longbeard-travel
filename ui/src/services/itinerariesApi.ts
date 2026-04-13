import { apiClient } from './apiClient';
import { CreateItineraryRequest, Itinerary } from '../types/itinerary';

export const createItinerary = async (payload: CreateItineraryRequest): Promise<Itinerary> => {
  const { data } = await apiClient.post<Itinerary>('/itineraries', payload);
  return data;
};

export const getItinerary = async (id: string): Promise<Itinerary> => {
  const { data } = await apiClient.get<Itinerary>(`/itineraries/${id}`);
  return data;
};
