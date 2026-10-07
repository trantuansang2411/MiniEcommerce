export const queryKeys = {
  products: ["products"] as const,
  productSearch: (search?: string, categoryId?: string, page?: number, pageSize?: number) => ["products", "search", search ?? "", categoryId ?? "", page ?? 1, pageSize ?? 20] as const,
  categories: ["categories"] as const,
  cart: ["cart"] as const,
  myOrders: ["my-orders"] as const,
  myOrder: (orderId: string) => ["my-order", orderId] as const,
  orderShipments: (orderId: string) => ["order-shipments", orderId] as const,
};
