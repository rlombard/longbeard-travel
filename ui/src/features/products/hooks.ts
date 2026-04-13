import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createProduct, getProduct, getProducts, updateProduct } from '../../services/productsApi';
import { createRate, getRatesByProduct, updateRate } from '../../services/ratesApi';
import { ProductRequest } from '../../types/product';
import { RateRequest } from '../../types/rate';

export const useProducts = () =>
  useQuery({
    queryKey: ['products'],
    queryFn: getProducts
  });

export const useProduct = (productId?: string) =>
  useQuery({
    queryKey: ['product', productId],
    queryFn: () => getProduct(productId!),
    enabled: Boolean(productId)
  });

export const useProductRates = (productId?: string) =>
  useQuery({
    queryKey: ['rates', productId],
    queryFn: () => getRatesByProduct(productId!),
    enabled: Boolean(productId)
  });

export const useCreateProduct = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: ProductRequest) => createProduct(payload),
    onSuccess: (product) => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.setQueryData(['product', product.id], product);
    }
  });
};

export const useUpdateProduct = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: ProductRequest }) => updateProduct(id, payload),
    onSuccess: (product) => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.setQueryData(['product', product.id], product);
    }
  });
};

export const useCreateRate = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: RateRequest) => createRate(payload),
    onSuccess: (rate) => {
      queryClient.invalidateQueries({ queryKey: ['rates', rate.productId] });
    }
  });
};

export const useUpdateRate = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: RateRequest }) => updateRate(id, payload),
    onSuccess: (rate) => {
      queryClient.invalidateQueries({ queryKey: ['rates', rate.productId] });
    }
  });
};
