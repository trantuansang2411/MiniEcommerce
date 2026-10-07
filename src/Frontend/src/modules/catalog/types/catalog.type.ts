export type Product = {
  id: string;
  categoryId: string;
  name: string;
  originalPrice: number;
  sellingPrice: number;
  imageUrl?: string;
  thumbnailUrl: string;
  imageUrls: string[];
  specifications: string;
  status: number;
  createdAt: string;
  availableStock: number;
};

export type PagedProducts = {
  items: Product[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type ProductDetail = Product & {
  availableStock: number;
};

export type Category = {
  id: string;
  name: string;
  description: string;
  iconKey: string;
  isActive: boolean;
  createdAt: string;
};
