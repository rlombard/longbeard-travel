import { apiClient } from './apiClient';
import { Supplier, SupplierListItem, SupplierRequest } from '../types/supplier';

export const getSuppliers = async (): Promise<SupplierListItem[]> => {
  const { data } = await apiClient.get<SupplierListItem[]>('/suppliers');
  return Array.isArray(data) ? data : [];
};

export const getSupplier = async (id: string): Promise<Supplier> => {
  const { data } = await apiClient.get<Supplier>(`/suppliers/${id}`);
  return data;
};

export const createSupplier = async (payload: SupplierRequest): Promise<Supplier> => {
  const { data } = await apiClient.post<Supplier>('/suppliers', payload);
  return data;
};

export const updateSupplier = async (id: string, payload: SupplierRequest): Promise<Supplier> => {
  const { data } = await apiClient.put<Supplier>(`/suppliers/${id}`, payload);
  return data;
};
