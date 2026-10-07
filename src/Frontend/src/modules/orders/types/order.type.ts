export type OrderItem = {
  productId: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
};

export type Order = {
  id: string;
  totalAmount: number;
  status: "AwaitingPayment" | "Paid" | "Completed" | "Cancelled" | "Expired";
  createdAt: string;
  expiresAt: string;
  recipientName?: string | null;
  recipientPhone?: string | null;
  shippingAddress?: string | null;
  deliveryNote?: string | null;
  items: OrderItem[];
};

export type Shipment = {
  id: string;
  orderId: string;
  warehouseId: string;
  status: string;
  shippingProvider?: string | null;
  trackingNumber?: string | null;
  items: { productId: string; quantity: number }[];
};
