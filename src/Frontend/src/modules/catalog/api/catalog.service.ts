import { apiClient } from "@/shared/api/api-client";
import type { Category, PagedProducts, Product, ProductDetail } from "@/modules/catalog/types/catalog.type";

export const catalogService = {
  getProducts: () => apiClient<Product[]>("/product"),
  searchProducts: ({ search, categoryId, page = 1, pageSize = 20 }: { search?: string; categoryId?: string; page?: number; pageSize?: number }) => {
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (search?.trim()) params.set("search", search.trim());
    if (categoryId) params.set("categoryId", categoryId);
    return apiClient<PagedProducts>(`/product/search?${params.toString()}`);
  },
  getProduct: (id: string) => apiClient<ProductDetail>(`/product/${id}`),
  getCategories: () => apiClient<Category[]>("/category"),
  getProductsByCategory: (categoryId: string) => apiClient<Product[]>(`/product/category/${categoryId}`),
};
