export type ShippingProfile = {
  id: string;
  recipientName: string;
  recipientPhone: string;
  shippingAddress: string;
  isDefault: boolean;
  createdAt: string;
  updatedAt: string;
};

export type CheckoutShippingDetails = {
  shippingProfileId?: string;
  recipientName?: string;
  recipientPhone?: string;
  shippingAddress?: string;
  deliveryNote?: string;
  saveAsProfile: boolean;
  setAsDefault: boolean;
};
