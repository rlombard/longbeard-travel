import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createSupplier, getSupplier, getSuppliers, updateSupplier } from '../../services/suppliersApi';
import { SupplierRequest } from '../../types/supplier';

export const useSuppliers = () =>
  useQuery({
    queryKey: ['suppliers'],
    queryFn: getSuppliers
  });

export const useSupplier = (supplierId?: string) =>
  useQuery({
    queryKey: ['supplier', supplierId],
    queryFn: () => getSupplier(supplierId!),
    enabled: Boolean(supplierId)
  });

export const useCreateSupplier = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: SupplierRequest) => createSupplier(payload),
    onSuccess: (supplier) => {
      queryClient.invalidateQueries({ queryKey: ['suppliers'] });
      queryClient.setQueryData(['supplier', supplier.id], supplier);
    }
  });
};

export const useUpdateSupplier = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: SupplierRequest }) => updateSupplier(id, payload),
    onSuccess: (supplier) => {
      queryClient.invalidateQueries({ queryKey: ['suppliers'] });
      queryClient.setQueryData(['supplier', supplier.id], supplier);
    }
  });
};
