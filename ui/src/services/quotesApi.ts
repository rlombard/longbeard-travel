import { apiClient } from './apiClient';
import { GenerateQuoteRequest, Quote } from '../types/quote';

export const generateQuote = async (payload: GenerateQuoteRequest): Promise<Quote> => {
  const { data } = await apiClient.post<Quote>('/quotes/generate', payload);
  return data;
};

export const getQuote = async (id: string): Promise<Quote> => {
  const { data } = await apiClient.get<Quote>(`/quotes/${id}`);
  return data;
};
