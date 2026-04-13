import { apiClient } from './apiClient';
import { Rate, RateRequest } from '../types/rate';

export const getRatesByProduct = async (productId: string): Promise<Rate[]> => {
  const { data } = await apiClient.get<Rate[]>(`/rates/product/${productId}`);
  return Array.isArray(data) ? data : [];
};

export const createRate = async (payload: RateRequest): Promise<Rate> => {
  const { data } = await apiClient.post<Rate>('/rates', payload);
  return data;
};

export const updateRate = async (id: string, payload: RateRequest): Promise<Rate> => {
  const { data } = await apiClient.put<Rate>(`/rates/${id}`, payload);
  return data;
};
