import { apiClient } from './apiClient';
import { Product, ProductListItem, ProductRequest } from '../types/product';

export const getProducts = async (): Promise<ProductListItem[]> => {
  const { data } = await apiClient.get<ProductListItem[]>('/products');
  return Array.isArray(data) ? data : [];
};

export const getProduct = async (id: string): Promise<Product> => {
  const { data } = await apiClient.get<Product>(`/products/${id}`);
  return data;
};

export const createProduct = async (payload: ProductRequest): Promise<Product> => {
  const { data } = await apiClient.post<Product>('/products', payload);
  return data;
};

export const updateProduct = async (id: string, payload: ProductRequest): Promise<Product> => {
  const { data } = await apiClient.put<Product>(`/products/${id}`, payload);
  return data;
};
