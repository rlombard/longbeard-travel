import { apiClient } from './apiClient';
import { CreateProductRequest, Product } from '../types/product';

export const getProducts = async (): Promise<Product[]> => {
  const { data } = await apiClient.get<Product[]>('/products');
  return data;
};

export const getProduct = async (id: string): Promise<Product> => {
  const { data } = await apiClient.get<Product>(`/products/${id}`);
  return data;
};

export const createProduct = async (payload: CreateProductRequest): Promise<Product> => {
  const { data } = await apiClient.post<Product>('/products', payload);
  return data;
};
