export type ProductType = 'Tour' | 'Hotel' | 'Transport';

export interface Product {
  id: string;
  supplierId: string;
  name: string;
  type: ProductType;
  metadata: string;
  createdAt: string;
}

export interface CreateProductRequest {
  supplierId: string;
  name: string;
  type: ProductType;
  metadata: string;
}
