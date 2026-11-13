export enum OrderStatus {
  Draft = 'DRAFT',
  Validated = 'VALIDATED',
  Cancelled = 'CANCELLED'
}

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  [OrderStatus.Draft]: 'Brouillon',
  [OrderStatus.Validated]: 'Validée',
  [OrderStatus.Cancelled]: 'Annulée'
};

export interface OrderSummary {
  id: string;
  orderNumber: string;
  clientName: string;
  orderDate: string;
  status: OrderStatus;
  totalHT: number;
  totalTTC: number;
}

export interface OrderLine {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderDetail {
  id: string;
  orderNumber: string;
  clientId: string;
  clientName: string;
  orderDate: string;
  orderStatus: OrderStatus;
  totalHt: number;
  totalTTC: number;
  lines: OrderLine[];
}

export interface OrderLineInput {
  productId: string;
  quantity: number;
}

export interface OrderPayload {
  clientId: string;
  lines: OrderLineInput[];
}

export interface ActionMessage {
  message: string;
}
