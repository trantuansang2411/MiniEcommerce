"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { ArrowRight, BadgeCheck, CircleHelp, Headphones, PackageCheck, ShieldCheck, ShoppingCart, Trash2, Truck } from "lucide-react";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { useOrderActions } from "@/modules/orders/hooks/use-orders";
import { CheckoutConfirmationDialog } from "@/modules/shopping/components/checkout-confirmation-dialog";
import { useCart, useCartActions } from "@/modules/shopping/hooks/use-cart";
import { useShippingProfiles } from "@/modules/shipping/hooks/use-shipping-profiles";
import type { CheckoutShippingDetails } from "@/modules/shipping/types/shipping-profile.type";
import { useToast } from "@/shared/components/ui/toast";
import { formatCurrency, productImageUrl } from "@/shared/utils/format";

export default function CartPage() {
  const router = useRouter();
  const { session, isReady } = useAuth();
  const cart = useCart(Boolean(session));
  const { updateItem, removeItem } = useCartActions();
  const { checkout } = useOrderActions();
  const shippingProfiles = useShippingProfiles(Boolean(session));
  const toast = useToast();
  const [selectedProductIds, setSelectedProductIds] = useState<string[]>([]);
  const [isCheckoutConfirmationOpen, setIsCheckoutConfirmationOpen] = useState(false);
  const [checkoutError, setCheckoutError] = useState<string | undefined>();
  const items = useMemo(() => cart.data?.items ?? [], [cart.data?.items]);

  const selectedCount = selectedProductIds.length;
  const isAllSelected = items.length > 0 && selectedCount === items.length;
  const selectedItems = useMemo(() => items.filter((item) => selectedProductIds.includes(item.productId)), [items, selectedProductIds]);

  useEffect(() => {
    if (isReady && !session) router.replace("/login");
  }, [isReady, router, session]);

  useEffect(() => {
    setSelectedProductIds((current) => {
      const remaining = current.filter((productId) => items.some((item) => item.productId === productId));
      return remaining.length === current.length ? current : remaining;
    });
  }, [items]);

  function toggleItem(productId: string) {
    setSelectedProductIds((current) => current.includes(productId) ? current.filter((id) => id !== productId) : [...current, productId]);
  }

  function toggleAll() {
    setSelectedProductIds(isAllSelected ? [] : items.map((item) => item.productId));
  }

  async function removeSelectedItems() {
    if (!selectedItems.length) return;

    try {
      await Promise.all(selectedItems.map((item) => removeItem.mutateAsync(item.productId)));
      toast.success(`Đã xóa ${selectedItems.length} sản phẩm đã chọn.`);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Không thể xóa một số sản phẩm đã chọn.");
    }
  }

  async function checkoutCart(shipping: CheckoutShippingDetails) {
    setCheckoutError(undefined);
    try {
      const order = await checkout.mutateAsync(shipping);
      setIsCheckoutConfirmationOpen(false);
      router.replace(`/orders/${order.id}/payment`);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Không thể tạo đơn hàng. Vui lòng thử lại.";
      setCheckoutError(message);
      toast.error(message);
    }
  }

  if (isReady && !session) {
    return <p>Đang chuyển đến trang đăng nhập...</p>;
  }

  if (!isReady || cart.isLoading) return <p>Đang tải giỏ hàng...</p>;
  if (cart.error) return <p className="error-message">{cart.error.message}</p>;
  if (!cart.data?.items.length) {
    return (
      <section className="cart-empty-experience" aria-labelledby="cart-empty-title">
        <div className="cart-empty-illustration" aria-hidden="true">
          <div className="cart-empty-orbit" />
          <div className="cart-empty-icon"><ShoppingCart /></div>
          <span>0</span>
        </div>
        <p className="cart-empty-kicker">GIỎ HÀNG MINISTORE</p>
        <h1 id="cart-empty-title">Giỏ hàng của bạn<br /><strong>đang chờ được lấp đầy.</strong></h1>
        <p className="cart-empty-copy">Khám phá những thiết bị phù hợp với bạn và lưu chúng ở đây để mua sắm thuận tiện hơn.</p>
        <Link className="cart-empty-action" href="/products">Khám phá sản phẩm <ArrowRight /></Link>
        <div className="cart-empty-benefits" aria-label="Lợi ích khi mua sắm tại MiniStore">
          <span><Truck />Giao hàng toàn quốc</span>
          <span><BadgeCheck />Sản phẩm chính hãng</span>
          <span><Headphones />Hỗ trợ tận tâm</span>
        </div>
      </section>
    );
  }

  return (
    <section className="cart-page">
      <header className="cart-page-heading">
        <div><h1>Giỏ hàng của bạn</h1><p>Có {items.length} sản phẩm trong giỏ hàng</p></div>
      </header>

      <div className="cart-layout">
        <div className="cart-items">
          <div className="cart-table-head">
            <label className="cart-select-all"><input type="checkbox" checked={isAllSelected} onChange={toggleAll} aria-label="Chọn tất cả sản phẩm" /><span>Chọn tất cả ({items.length} sản phẩm)</span></label>
            <span>Đơn giá</span><span>Số lượng</span><span>Thành tiền</span><span className="cart-table-action">Thao tác</span>
          </div>

          <div className="cart-list">
            {items.map((item) => {
              const isSelected = selectedProductIds.includes(item.productId);
              return <article className="cart-item" key={item.id}>
                <label className="cart-item-select"><input type="checkbox" checked={isSelected} onChange={() => toggleItem(item.productId)} aria-label={`Chọn ${item.productName}`} /></label>
                {item.imageUrl ? (
                  // Backend serves user-uploaded files from a configurable local address, so native img is intentional here.
                  // eslint-disable-next-line @next/next/no-img-element
                  <img src={productImageUrl(item.imageUrl)} alt={item.productName} />
                ) : <div className="image-placeholder" />}
                <div className="cart-product-details"><h2>{item.productName}</h2><span className={item.isAvailable ? "cart-stock-badge" : "cart-stock-badge is-unavailable"}>{item.isAvailable ? "Còn hàng" : "Tạm hết hàng"}</span></div>
                <p className="cart-unit-price">{formatCurrency(item.unitPrice)}</p>
                <div className="quantity-control">
                  <button aria-label={`Giảm số lượng ${item.productName}`} disabled={item.quantity <= 1 || updateItem.isPending} onClick={() => updateItem.mutate({ productId: item.productId, quantity: item.quantity - 1 })}>−</button>
                  <span>{item.quantity}</span>
                  <button aria-label={`Tăng số lượng ${item.productName}`} disabled={updateItem.isPending} onClick={() => updateItem.mutate({ productId: item.productId, quantity: item.quantity + 1 })}>+</button>
                </div>
                <strong className="cart-item-total">{formatCurrency(item.lineTotal)}</strong>
                <button className="cart-remove-item" type="button" aria-label={`Xóa ${item.productName}`} disabled={removeItem.isPending} onClick={() => removeItem.mutate(item.productId)}><Trash2 /></button>
              </article>;
            })}
          </div>

          <footer className="cart-selection-footer"><span>{selectedCount ? `Đã chọn ${selectedCount} sản phẩm` : "Chọn sản phẩm cần xóa"}</span><button type="button" onClick={() => void removeSelectedItems()} disabled={!selectedCount || removeItem.isPending}><Trash2 />Xóa sản phẩm đã chọn</button></footer>
        </div>

        <aside className="cart-sidebar">
          <section className="order-summary">
            <h2>Tổng đơn hàng</h2>
            <p><span>Tạm tính ({cart.data.totalQuantity} sản phẩm)</span><strong>{formatCurrency(cart.data.totalAmount)}</strong></p>
            <p><span className="shipping-label">Phí vận chuyển <CircleHelp aria-label="Miễn phí vận chuyển" /></span><strong className="shipping-free">Miễn phí</strong></p>
            <div className="order-summary-total"><span>Tổng tiền</span><strong>{formatCurrency(cart.data.totalAmount)}</strong></div>
            <button className="cart-checkout-button" type="button" onClick={() => { setCheckoutError(undefined); setIsCheckoutConfirmationOpen(true); }}>Tiến hành thanh toán <ArrowRight /></button>
          </section>
          <ul className="cart-assurances">
            <li><Truck /><span><strong>Miễn phí vận chuyển</strong><small>Áp dụng cho mọi đơn hàng</small></span></li>
            <li><ShieldCheck /><span><strong>Thanh toán an toàn</strong><small>Nhiều phương thức thanh toán</small></span></li>
            <li><PackageCheck /><span><strong>Đổi trả dễ dàng</strong><small>Trong 7 ngày</small></span></li>
          </ul>
        </aside>
      </div>
      <CheckoutConfirmationDialog isOpen={isCheckoutConfirmationOpen} itemCount={cart.data.totalQuantity} totalAmount={cart.data.totalAmount} isSubmitting={checkout.isPending} error={checkoutError} profiles={shippingProfiles.data ?? []} profilesAreLoading={shippingProfiles.isLoading} onClose={() => setIsCheckoutConfirmationOpen(false)} onConfirm={(shipping) => void checkoutCart(shipping)} />
    </section>
  );
}
