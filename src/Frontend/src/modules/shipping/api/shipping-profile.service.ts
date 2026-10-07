import { apiClient } from "@/shared/api/api-client";
import type { ShippingProfile } from "@/modules/shipping/types/shipping-profile.type";

export type ShippingProfileInput = Pick<ShippingProfile, "recipientName" | "recipientPhone" | "shippingAddress" | "isDefault">;

export const shippingProfileService = {
  getMine: () => apiClient<ShippingProfile[]>("/shipping-profiles", { authenticated: true }),
  create: (input: ShippingProfileInput) => apiClient<ShippingProfile>("/shipping-profiles", { method: "POST", authenticated: true, body: input }),
  update: (id: string, input: ShippingProfileInput) => apiClient<ShippingProfile>(`/shipping-profiles/${id}`, { method: "PUT", authenticated: true, body: input }),
  setDefault: (id: string) => apiClient<null>(`/shipping-profiles/${id}/default`, { method: "PATCH", authenticated: true }),
  remove: (id: string) => apiClient<null>(`/shipping-profiles/${id}`, { method: "DELETE", authenticated: true }),
};
