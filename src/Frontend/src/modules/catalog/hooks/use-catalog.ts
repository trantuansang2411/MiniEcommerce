"use client";

import { useQuery } from "@tanstack/react-query";
import { catalogService } from "@/modules/catalog/api/catalog.service";
import { queryKeys } from "@/shared/constants/query-keys";

export function useProducts() {
  return useQuery({ queryKey: queryKeys.products, queryFn: catalogService.getProducts });
}

export function useProductSearch(search?: string, categoryId?: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: queryKeys.productSearch(search, categoryId, page, pageSize),
    queryFn: () => catalogService.searchProducts({ search, categoryId, page, pageSize }),
  });
}

export function useCategories() {
  return useQuery({ queryKey: queryKeys.categories, queryFn: catalogService.getCategories });
}
