"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { orderService } from "@/modules/orders/api/order.service";
import { queryKeys } from "@/shared/constants/query-keys";

export function useMyOrders(enabled: boolean) {
  return useQuery({ queryKey: queryKeys.myOrders, queryFn: orderService.getMyOrders, enabled });
}

export function useMyOrder(orderId: string, enabled: boolean) {
  return useQuery({ queryKey: queryKeys.myOrder(orderId), queryFn: () => orderService.getMyOrder(orderId), enabled });
}

export function useMyOrderShipments(orderId: string, enabled: boolean) {
  return useQuery({ queryKey: queryKeys.orderShipments(orderId), queryFn: () => orderService.getMyOrderShipments(orderId), enabled });
}

export function useOrderActions() {
  const client = useQueryClient();
  const invalidate = () => {
    void client.invalidateQueries({ queryKey: queryKeys.myOrders });
    void client.invalidateQueries({ queryKey: ["my-order"] });
    void client.invalidateQueries({ queryKey: ["order-shipments"] });
    void client.invalidateQueries({ queryKey: queryKeys.cart });
  };

  return {
    checkout: useMutation({ mutationFn: orderService.checkout, onSuccess: invalidate }),
    payManually: useMutation({ mutationFn: orderService.payManually, onSuccess: invalidate }),
    cancel: useMutation({ mutationFn: orderService.cancel, onSuccess: invalidate }),
  };
}
