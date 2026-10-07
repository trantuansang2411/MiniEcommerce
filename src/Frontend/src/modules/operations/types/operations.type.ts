export type DashboardRole = "Admin" | "Manager" | "Staff";

export type Warehouse = {
  id: string;
  code: string;
  name: string;
  address: string;
  status: "Active" | "Inactive" | "Closed";
  createdAt: string;
  updatedAt: string;
};

export type Inventory = {
  id: string;
  warehouseId: string;
  warehouseCode: string;
  productId: string;
  productName: string;
  onHand: number;
  reserved: number;
  available: number;
  updatedAt: string;
};

export type InventoryTransaction = {
  id: string;
  inventoryId: string;
  type: string;
  quantityChange: number;
  reason?: string;
  createdByUserId: string;
  createdAt: string;
};

export type ShipmentItem = { productId: string; quantity: number };

export type Shipment = {
  id: string;
  orderId: string;
  warehouseId: string;
  status: "Pending" | "Picking" | "Packed" | "Shipped" | "InTransit" | "Delivered" | "Cancelled";
  shippingProvider?: string;
  trackingNumber?: string;
  items: ShipmentItem[];
};

export type ShipmentDetail = {
  id: string;
  orderId: string;
  status: Shipment["status"];
  shippingProvider?: string | null;
  trackingNumber?: string | null;
  createdAt: string;
  shippedAt?: string | null;
  deliveredAt?: string | null;
  delivery: {
    recipientName?: string | null;
    recipientPhone?: string | null;
    shippingAddress?: string | null;
    deliveryNote?: string | null;
  };
  items: { productId: string; productName: string; thumbnailUrl: string; quantity: number }[];
};

export type OrderManagementDetail = {
  id: string;
  status: string;
  totalAmount: number;
  createdAt: string;
  expiresAt: string;
  delivery: { recipientName?: string | null; recipientPhone?: string | null; shippingAddress?: string | null; deliveryNote?: string | null };
  items: { productId: string; productName: string; thumbnailUrl: string; quantity: number; unitPrice: number; lineTotal: number }[];
  payments: { id: string; method: string; status: string; transactionId?: string | null; amount: number; createdAt: string; paidAt?: string | null }[];
  shipments: { id: string; status: string; shippingProvider?: string | null; trackingNumber?: string | null; itemCount: number; createdAt: string; shippedAt?: string | null; deliveredAt?: string | null }[];
};
