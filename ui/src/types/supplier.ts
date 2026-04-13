export interface SupplierListItem {
  id: string;
  name: string;
  email?: string | null;
  phone?: string | null;
}

export interface Supplier {
  id: string;
  name: string;
  email?: string | null;
  phone?: string | null;
  createdAt: string;
}

export interface SupplierRequest {
  name: string;
  email?: string | null;
  phone?: string | null;
}
