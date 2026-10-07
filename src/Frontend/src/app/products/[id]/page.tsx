"use client";

import { useQuery } from "@tanstack/react-query";
import { Check, ChevronLeft, ChevronRight, Minus, PackageX, Plus, ShoppingCart, Truck } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { catalogService } from "@/modules/catalog/api/catalog.service";
import { CatalogBreadcrumb } from "@/modules/catalog/components/catalog-breadcrumb";
import { useCategories } from "@/modules/catalog/hooks/use-catalog";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { useCartActions } from "@/modules/shopping/hooks/use-cart";
import { Button } from "@/shared/components/ui/button";
import { useToast } from "@/shared/components/ui/toast";
import { formatCurrency, productImageUrl } from "@/shared/utils/format";

export default function ProductDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const toast = useToast();
  const { session } = useAuth();
  const product = useQuery({ queryKey: ["product", params.id], queryFn: () => catalogService.getProduct(params.id) });
  const { data: categories } = useCategories();
  const { addItem } = useCartActions();
  const [activeImage, setActiveImage] = useState(0);
  const [quantity, setQuantity] = useState(1);

  const images = useMemo(() => {
    if (!product.data) return [];
    return [...new Set([product.data.thumbnailUrl, ...product.data.imageUrls].filter(Boolean))];
  }, [product.data]);
  const availableStock = product.data?.availableStock ?? 0;
  const isInStock = availableStock > 0;
  const category = categories?.find((item) => item.id === product.data?.categoryId);

  useEffect(() => {
    setActiveImage(0);
    setQuantity(1);
  }, [product.data?.id]);

  useEffect(() => {
    if (isInStock) setQuantity((current) => Math.min(Math.max(current, 1), availableStock));
  }, [availableStock, isInStock]);

  async function addToCart() {
    if (!session) { router.push("/login"); return; }
    try {
      await addItem.mutateAsync({ productId: params.id, quantity });
      toast.success("Đã thêm sản phẩm vào giỏ hàng.");
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Không thể thêm sản phẩm vào giỏ hàng.");
    }
  }

  if (product.isLoading) return <p className="store-status">Đang tải sản phẩm...</p>;
  if (product.error) return <p className="error-message">{product.error.message}</p>;
  if (!product.data) return null;

  const currentImage = images[activeImage];
  const showPrevious = () => setActiveImage((current) => (current - 1 + images.length) % images.length);
  const showNext = () => setActiveImage((current) => (current + 1) % images.length);

  return <section className="mx-auto w-full max-w-[1360px] px-1 py-4 sm:px-3 sm:py-8">
    <CatalogBreadcrumb items={[
      { label: "Trang chủ", href: "/" },
      { label: category?.name ?? "Sản phẩm", href: category ? `/products?category=${category.id}` : "/products" },
      { label: product.data.name },
    ]} />
    <div className="grid gap-8 xl:grid-cols-[minmax(0,1.14fr)_minmax(360px,.86fr)] xl:gap-12">
      <div className="min-w-0">
        <div className="relative grid min-h-[360px] place-items-center overflow-hidden rounded-[24px] border border-slate-200/80 bg-white p-6 shadow-[0_18px_45px_rgba(15,32,55,.06)] sm:min-h-[540px] sm:p-10">
          {currentImage ? <>
            {/* Product uploads are hosted by the API, so the Next image optimizer is intentionally not used. */}
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={productImageUrl(currentImage)} alt={product.data.name} className="h-full max-h-[470px] w-full object-contain" />
          </> : <PackageX className="size-14 text-slate-300" />}
          {images.length > 1 && <>
            <button type="button" aria-label="Ảnh trước" onClick={showPrevious} className="absolute left-4 top-1/2 grid size-11 -translate-y-1/2 place-items-center rounded-full border border-slate-200 bg-white/90 text-slate-600 shadow-sm transition hover:border-cyan-300 hover:text-cyan-700"><ChevronLeft className="size-5" /></button>
            <button type="button" aria-label="Ảnh tiếp theo" onClick={showNext} className="absolute right-4 top-1/2 grid size-11 -translate-y-1/2 place-items-center rounded-full border border-slate-200 bg-white/90 text-slate-600 shadow-sm transition hover:border-cyan-300 hover:text-cyan-700"><ChevronRight className="size-5" /></button>
          </>}
        </div>
        {images.length > 1 && <div className="mt-4 flex gap-3 overflow-x-auto pb-1" aria-label="Danh sách ảnh sản phẩm">
          {images.map((image, index) => <button type="button" key={image} aria-label={`Xem ảnh ${index + 1}`} aria-pressed={activeImage === index} onClick={() => setActiveImage(index)} className={`grid size-[76px] shrink-0 place-items-center overflow-hidden rounded-xl border bg-white p-1.5 transition ${activeImage === index ? "border-cyan-400 ring-2 ring-cyan-100" : "border-slate-200 hover:border-cyan-200"}`}>
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={productImageUrl(image)} alt="" className="size-full object-contain" />
          </button>)}
        </div>}
        <article className="mt-8 rounded-[22px] border border-slate-200/80 bg-white px-5 py-6 sm:px-7">
          <p className="text-[11px] font-black uppercase tracking-[.15em] text-cyan-600">Thông số kỹ thuật</p>
          <p className="mt-4 whitespace-pre-line text-[15px] leading-7 text-slate-600">{product.data.specifications || "Thông tin sản phẩm đang được cập nhật."}</p>
        </article>
      </div>

      <aside className="h-fit rounded-[24px] border border-slate-200/80 bg-white p-6 shadow-[0_18px_45px_rgba(15,32,55,.07)] sm:p-8 xl:sticky xl:top-24">
        <h1 className="text-2xl font-black leading-tight tracking-[-.035em] text-slate-900 sm:text-3xl">{product.data.name}</h1>
        <div className="mt-6 rounded-2xl bg-slate-50 px-5 py-4">
          <p className="text-sm text-slate-500">Giá bán</p>
          <div className="mt-1 flex flex-wrap items-end gap-x-3 gap-y-1">
            <strong className="text-3xl font-black tracking-[-.04em] text-orange-500">{formatCurrency(product.data.sellingPrice)}</strong>
            {product.data.originalPrice > product.data.sellingPrice && <span className="pb-1 text-sm text-slate-400 line-through">{formatCurrency(product.data.originalPrice)}</span>}
          </div>
        </div>

        <div className={`mt-5 flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-bold ${isInStock ? "bg-emerald-50 text-emerald-700" : "bg-rose-50 text-rose-700"}`}>
          <span className={`size-2.5 rounded-full ${isInStock ? "bg-emerald-500" : "bg-rose-500"}`} />
          {isInStock ? `Còn ${availableStock} sản phẩm` : "Hết hàng"}
        </div>

        <div className="mt-7 border-t border-slate-100 pt-6">
          <p className="text-sm font-bold text-slate-800">Số lượng</p>
          <div className="mt-3 flex items-center gap-4">
            <div className="flex h-11 items-center rounded-xl border border-slate-200 bg-slate-50">
              <button type="button" aria-label="Giảm số lượng" disabled={!isInStock || quantity <= 1} onClick={() => setQuantity((current) => Math.max(1, current - 1))} className="grid size-11 place-items-center text-slate-500 transition hover:text-cyan-700 disabled:cursor-not-allowed disabled:opacity-35"><Minus className="size-4" /></button>
              <span className="min-w-10 text-center text-sm font-black tabular-nums text-slate-900">{quantity}</span>
              <button type="button" aria-label="Tăng số lượng" disabled={!isInStock || quantity >= availableStock} onClick={() => setQuantity((current) => Math.min(availableStock, current + 1))} className="grid size-11 place-items-center text-slate-500 transition hover:text-cyan-700 disabled:cursor-not-allowed disabled:opacity-35"><Plus className="size-4" /></button>
            </div>
            {isInStock && <span className="text-xs text-slate-500">Tối đa {availableStock} sản phẩm</span>}
          </div>
        </div>

        <Button className="mt-7 h-12 w-full text-base" disabled={!isInStock || addItem.isPending} onClick={() => void addToCart()}><ShoppingCart className="size-5" />{addItem.isPending ? "Đang thêm..." : isInStock ? "Thêm vào giỏ hàng" : "Tạm hết hàng"}</Button>
        <div className="mt-6 grid gap-3 border-t border-slate-100 pt-5 text-sm text-slate-600">
          <span className="flex items-center gap-3"><Truck className="size-5 text-cyan-600" />Giao hàng nhanh, đóng gói cẩn thận</span>
          <span className="flex items-center gap-3"><Check className="size-5 text-cyan-600" />Sản phẩm chính hãng, hỗ trợ tận tâm</span>
        </div>
      </aside>
    </div>
  </section>;
}
