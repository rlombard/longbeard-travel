import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createProduct, getProducts } from '../../services/productsApi';
import { CreateProductRequest } from '../../types/product';

export const useProducts = () =>
  useQuery({
    queryKey: ['products'],
    queryFn: getProducts
  });

export const useCreateProduct = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateProductRequest) => createProduct(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
    }
  });
};
