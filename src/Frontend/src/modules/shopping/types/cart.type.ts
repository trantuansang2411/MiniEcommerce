export type CartItem = {
  id: string;
  productId: string;
  productName: string;
  imageUrl: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  isAvailable: boolean;
};

export type Cart = {
  id: string | null;
  items: CartItem[];
  totalQuantity: number;
  totalAmount: number;
  createdAt: string | null;
  updatedAt: string | null;
};
