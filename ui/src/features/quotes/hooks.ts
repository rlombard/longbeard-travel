import { useMutation, useQuery } from '@tanstack/react-query';
import { generateQuote, getQuote } from '../../services/quotesApi';
import { GenerateQuoteRequest } from '../../types/quote';

export const useGenerateQuote = () =>
  useMutation({
    mutationFn: (payload: GenerateQuoteRequest) => generateQuote(payload)
  });

export const useQuote = (quoteId?: string) =>
  useQuery({
    queryKey: ['quote', quoteId],
    queryFn: () => getQuote(quoteId!),
    enabled: Boolean(quoteId)
  });
