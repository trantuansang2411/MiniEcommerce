"use client";

import Link from "next/link";
import { ShoppingCart } from "lucide-react";
import { useRouter } from "next/navigation";
import type { Product } from "@/modules/catalog/types/catalog.type";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { useCartActions } from "@/modules/shopping/hooks/use-cart";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent } from "@/shared/components/ui/card";
import { useToast } from "@/shared/components/ui/toast";
import { formatCurrency, productImageUrl } from "@/shared/utils/format";

type ProductCardProps = Readonly<{
  product: Product;
  variant?: "default" | "catalog";
}>;

export function ProductCard({ product, variant = "default" }: ProductCardProps) {
  const router = useRouter();
  const { session } = useAuth();
  const { addItem } = useCartActions();
  const toast = useToast();
  const imageUrl = product.thumbnailUrl || product.imageUrl;
  const isCatalogCard = variant === "catalog";
  const isOutOfStock = product.availableStock <= 0;

  async function addToCart() {
    if (isOutOfStock) return;
    if (!session) { router.push("/login"); return; }
    try {
      await addItem.mutateAsync({ productId: product.id, quantity: 1 });
      toast.success(`Đã thêm “${product.name}” vào giỏ hàng.`);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Không thể thêm sản phẩm vào giỏ hàng.");
    }
  }

  return <Card className="group flex min-w-0 flex-col overflow-hidden border-slate-200/80 transition-[transform,box-shadow] duration-200 hover:-translate-y-1 hover:shadow-[0_16px_32px_rgba(15,32,55,.10)]">
    <Link href={`/products/${product.id}`} className={`grid place-items-center bg-white px-4 pb-2 pt-4 ${isCatalogCard ? "h-60 sm:h-64" : "h-48"}`}>
      {imageUrl ? <>
        {/* Backend serves configured local uploads, so this URL cannot use Next's static Image optimizer. */}
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={productImageUrl(imageUrl)} alt={product.name} className="size-full object-contain transition-transform duration-300 group-hover:scale-[1.04]" />
      </> : <span className="text-xs text-slate-400">Chưa có ảnh</span>}
    </Link>
    <CardContent className={`flex flex-1 flex-col gap-3 px-4 pb-4 pt-3 ${isCatalogCard ? "sm:px-5 sm:pb-5 sm:pt-4" : ""}`}>
      <Link href={`/products/${product.id}`} className={`line-clamp-2 min-h-9 font-bold leading-5 text-slate-700 transition-colors hover:text-cyan-700 ${isCatalogCard ? "text-[15px] sm:text-base" : "text-sm"}`}>{product.name}</Link>
      <p className={`truncate text-slate-500 ${isCatalogCard ? "text-sm" : "text-xs"}`}>{product.specifications || "Chưa có thông số"}</p>
      <div className="mt-0.5 flex flex-wrap items-baseline gap-x-2 gap-y-0.5 tabular-nums">
        <span className="text-xs text-slate-400 line-through">{formatCurrency(product.originalPrice)}</span>
        <strong className={`${isCatalogCard ? "text-lg" : "text-base"} font-extrabold text-orange-500`}>{formatCurrency(product.sellingPrice)}</strong>
      </div>
      <p className={`text-xs font-semibold ${isOutOfStock ? "text-rose-500" : "text-emerald-600"}`}>{isOutOfStock ? "Tạm hết hàng" : `Còn ${product.availableStock} sản phẩm`}</p>
      <Button variant="outline" size="sm" className={`mt-auto w-full ${isOutOfStock ? "cursor-not-allowed border-slate-200 bg-slate-100 text-slate-400 hover:bg-slate-100 hover:text-slate-400" : "border-cyan-100 bg-cyan-50 text-cyan-700 hover:bg-cyan-500 hover:text-white"}`} disabled={isOutOfStock || addItem.isPending} onClick={() => void addToCart()}><ShoppingCart className="size-4" />{isOutOfStock ? "Tạm hết hàng" : addItem.isPending ? "Đang thêm..." : "Thêm vào giỏ"}</Button>
    </CardContent>
  </Card>;
}
