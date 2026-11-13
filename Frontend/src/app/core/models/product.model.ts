export interface Product {
  id: string;
  reference: string;
  name: string;
  description: string;
  unitPriceHT: number;
  stockQuantity: number;
}

export interface CreateProductPayload {
  reference: string;
  name: string;
  description: string;
  unitPriceHT: number;
  initialStock: number;
}

export type UpdateProductPayload = Omit<CreateProductPayload, 'initialStock'>;
