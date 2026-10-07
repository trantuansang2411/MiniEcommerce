import { apiClient } from "@/shared/api/api-client";
import type { Category, Product } from "@/modules/catalog/types/catalog.type";
import type { Order } from "@/modules/orders/types/order.type";
import type { Inventory, InventoryTransaction, OrderManagementDetail, Shipment, ShipmentDetail, Warehouse } from "@/modules/operations/types/operations.type";

type CategoryPayload = { name: string; description?: string; iconKey?: string };
type WarehousePayload = { code: string; name: string; address: string };

export const managementService = {
  getCategories: () => apiClient<Category[]>("/category"),
  createCategory: (body: CategoryPayload) => apiClient<Category>("/category", { method: "POST", body, authenticated: true }),
  updateCategory: (id: string, body: Partial<CategoryPayload> & { isActive?: boolean }) => apiClient<Category>(`/category/${id}`, { method: "PATCH", body, authenticated: true }),
  deleteCategory: (id: string) => apiClient<null>(`/category/${id}`, { method: "DELETE", authenticated: true }),

  getProducts: () => apiClient<Product[]>("/product/management", { authenticated: true }),
  getCatalogProducts: () => apiClient<Product[]>("/product"),
  createProduct: (body: FormData) => apiClient<Product>("/product", { method: "POST", body, authenticated: true }),
  updateProduct: (id: string, body: FormData) => apiClient<Product>(`/product/${id}`, { method: "PATCH", body, authenticated: true }),
  deleteProduct: (id: string) => apiClient<null>(`/product/${id}`, { method: "DELETE", authenticated: true }),

  getWarehouses: () => apiClient<Warehouse[]>("/warehouses", { authenticated: true }),
  createWarehouse: (body: WarehousePayload) => apiClient<Warehouse>("/warehouses", { method: "POST", body, authenticated: true }),
  updateWarehouse: (id: string, body: Partial<WarehousePayload> & { status?: number }) => apiClient<Warehouse>(`/warehouses/${id}`, { method: "PATCH", body, authenticated: true }),

  getInventories: (warehouseId?: string) => apiClient<Inventory[]>(`/inventories${warehouseId ? `?warehouseId=${warehouseId}` : ""}`, { authenticated: true }),
  getInventoryTransactions: (inventoryId: string) => apiClient<InventoryTransaction[]>(`/inventories/${inventoryId}/transactions`, { authenticated: true }),
  stockIn: (body: { warehouseId: string; productId: string; quantity: number }) => apiClient<Inventory>("/inventories/stock-in", { method: "POST", body, authenticated: true }),
  adjustInventory: (body: { warehouseId: string; productId: string; quantityChange: number; reason: number }) => apiClient<Inventory>("/inventories/adjustments", { method: "POST", body, authenticated: true }),

  getOrders: (status?: number) => apiClient<Order[]>(`/orders/management${status ? `?status=${status}` : ""}`, { authenticated: true }),
  getOrderDetail: (id: string) => apiClient<OrderManagementDetail>(`/orders/management/${id}`, { authenticated: true }),
  getShipments: (status?: number) => apiClient<Shipment[]>(`/shipments${status ? `?status=${status}` : ""}`, { authenticated: true }),
  getShipmentDetail: (id: string) => apiClient<ShipmentDetail>(`/shipments/${id}`, { authenticated: true }),
  updateShipment: (id: string, body: { status: number; shippingProvider?: string; trackingNumber?: string }) => apiClient<Shipment>(`/shipments/${id}/status`, { method: "PATCH", body, authenticated: true }),
};
