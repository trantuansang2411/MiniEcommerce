"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { cartService } from "@/modules/shopping/api/cart.service";
import { queryKeys } from "@/shared/constants/query-keys";

export function useCart(enabled: boolean) {
  return useQuery({ queryKey: queryKeys.cart, queryFn: cartService.getMyCart, enabled });
}

export function useCartActions() {
  const queryClient = useQueryClient();
  const invalidateCart = () => queryClient.invalidateQueries({ queryKey: queryKeys.cart });

  return {
    addItem: useMutation({ mutationFn: ({ productId, quantity }: { productId: string; quantity: number }) => cartService.addItem(productId, quantity), onSuccess: invalidateCart }),
    updateItem: useMutation({ mutationFn: ({ productId, quantity }: { productId: string; quantity: number }) => cartService.updateItem(productId, quantity), onSuccess: invalidateCart }),
    removeItem: useMutation({ mutationFn: cartService.removeItem, onSuccess: invalidateCart }),
  };
}
