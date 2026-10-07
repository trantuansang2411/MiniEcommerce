import { apiClient } from "@/shared/api/api-client";
import type { Order, Shipment } from "@/modules/orders/types/order.type";
import type { CheckoutShippingDetails } from "@/modules/shipping/types/shipping-profile.type";

export const orderService = {
  checkout: (shipping: CheckoutShippingDetails) => apiClient<Order>("/orders/checkout", { method: "POST", authenticated: true, body: shipping }),
  getMyOrders: () => apiClient<Order[]>("/orders/my-orders", { authenticated: true }),
  getMyOrder: (orderId: string) => apiClient<Order>(`/orders/${orderId}`, { authenticated: true }),
  getMyOrderShipments: (orderId: string) => apiClient<Shipment[]>(`/orders/${orderId}/shipments`, { authenticated: true }),
  payManually: (orderId: string) => apiClient<unknown>(`/orders/${orderId}/payments/manual`, { method: "POST", authenticated: true }),
  createVnPayPayment: (orderId: string) => apiClient<{ paymentId: string; paymentUrl: string }>(`/orders/${orderId}/payments/vnpay`, { method: "POST", authenticated: true }),
  getPayment: (orderId: string, paymentId: string) => apiClient<{ id: string; orderId: string; status: string }>(`/orders/${orderId}/payments/${paymentId}`, { authenticated: true }),
  cancel: (orderId: string) => apiClient<null>(`/orders/${orderId}/cancel`, { method: "POST", authenticated: true }),
};
