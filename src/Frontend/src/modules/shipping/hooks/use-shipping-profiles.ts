"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { shippingProfileService } from "@/modules/shipping/api/shipping-profile.service";
import type { ShippingProfileInput } from "@/modules/shipping/api/shipping-profile.service";

export function useShippingProfiles(enabled: boolean) {
  return useQuery({
    queryKey: ["shipping-profiles"],
    queryFn: shippingProfileService.getMine,
    enabled,
  });
}

export function useShippingProfileActions() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["shipping-profiles"] });

  return {
    create: useMutation({ mutationFn: (input: ShippingProfileInput) => shippingProfileService.create(input), onSuccess: invalidate }),
    update: useMutation({ mutationFn: ({ id, input }: { id: string; input: ShippingProfileInput }) => shippingProfileService.update(id, input), onSuccess: invalidate }),
    setDefault: useMutation({ mutationFn: shippingProfileService.setDefault, onSuccess: invalidate }),
    remove: useMutation({ mutationFn: shippingProfileService.remove, onSuccess: invalidate }),
  };
}
