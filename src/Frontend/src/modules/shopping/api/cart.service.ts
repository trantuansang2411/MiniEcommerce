import { apiClient } from "@/shared/api/api-client";
import type { Cart } from "@/modules/shopping/types/cart.type";

export const cartService = {
  getMyCart: () => apiClient<Cart>("/cart", { authenticated: true }),
  addItem: (productId: string, quantity: number) =>
    apiClient<Cart>("/cart/items", { method: "POST", authenticated: true, body: { productId, quantity } }),
  updateItem: (productId: string, quantity: number) =>
    apiClient<Cart>(`/cart/items/${productId}`, { method: "PATCH", authenticated: true, body: { quantity } }),
  removeItem: (productId: string) =>
    apiClient<null>(`/cart/items/${productId}`, { method: "DELETE", authenticated: true }),
};
