"use client";

import { Check, CircleCheck, MapPin, Plus, ShoppingBag, X } from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { formatCurrency } from "@/shared/utils/format";
import type { CheckoutShippingDetails, ShippingProfile } from "@/modules/shipping/types/shipping-profile.type";

type CheckoutConfirmationDialogProps = {
  isOpen: boolean;
  itemCount: number;
  totalAmount: number;
  isSubmitting: boolean;
  error?: string;
  profiles: ShippingProfile[];
  profilesAreLoading: boolean;
  onClose: () => void;
  onConfirm: (shipping: CheckoutShippingDetails) => void;
};

const emptyShippingDetails = { recipientName: "", recipientPhone: "", shippingAddress: "", deliveryNote: "", saveAsProfile: true, setAsDefault: false };

export function CheckoutConfirmationDialog({ isOpen, itemCount, totalAmount, isSubmitting, error, profiles, profilesAreLoading, onClose, onConfirm }: Readonly<CheckoutConfirmationDialogProps>) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const defaultProfileId = useMemo(() => profiles.find((profile) => profile.isDefault)?.id ?? profiles[0]?.id ?? "manual", [profiles]);
  const [selection, setSelection] = useState(defaultProfileId);
  const [shipping, setShipping] = useState(emptyShippingDetails);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) return;
    if (isOpen && !dialog.open) dialog.showModal();
    if (!isOpen && dialog.open) dialog.close();
  }, [isOpen]);

  useEffect(() => {
    if (isOpen) setSelection(defaultProfileId);
  }, [defaultProfileId, isOpen]);

  function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (selection === "manual") {
      onConfirm(shipping);
      return;
    }

    onConfirm({ shippingProfileId: selection, deliveryNote: shipping.deliveryNote, saveAsProfile: false, setAsDefault: false });
  }

  return <dialog ref={dialogRef} className="checkout-confirm-dialog" aria-labelledby="checkout-confirmation-title" onCancel={onClose} onClick={(event) => { if (event.target === event.currentTarget) onClose(); }}>
    <div className="checkout-confirm-icon"><ShoppingBag /></div>
    <button className="checkout-confirm-close" type="button" aria-label="Đóng" onClick={onClose} disabled={isSubmitting}><X /></button>
    <p className="checkout-confirm-kicker">SẴN SÀNG THANH TOÁN</p>
    <h2 id="checkout-confirmation-title">Xác nhận tạo đơn hàng?</h2>
    <p className="checkout-confirm-copy">Chọn nơi nhận hàng. Thông tin này sẽ được lưu cố định cho đơn hàng của bạn.</p>
    <div className="checkout-confirm-summary"><span>{itemCount} sản phẩm</span><strong>{formatCurrency(totalAmount)}</strong></div>
    <form className="checkout-shipping-form" onSubmit={submit}>
      <fieldset disabled={isSubmitting}>
        <legend><MapPin />Thông tin nhận hàng</legend>
        {profilesAreLoading ? <p className="checkout-profile-loading">Đang tải địa chỉ đã lưu...</p> : profiles.map((profile) => (
          <label className={`checkout-profile ${selection === profile.id ? "is-selected" : ""}`} key={profile.id}>
            <input type="radio" name="shipping-profile" checked={selection === profile.id} onChange={() => setSelection(profile.id)} />
            <span className="checkout-profile-check"><Check /></span>
            <span><strong>{profile.recipientName}{profile.isDefault && <em>Mặc định</em>}</strong><small>{profile.recipientPhone}</small><small>{profile.shippingAddress}</small></span>
          </label>
        ))}
        <label className={`checkout-profile checkout-profile-manual ${selection === "manual" ? "is-selected" : ""}`}>
          <input type="radio" name="shipping-profile" checked={selection === "manual"} onChange={() => setSelection("manual")} />
          <span className="checkout-profile-check"><Plus /></span>
          <span><strong>Dùng địa chỉ khác</strong><small>Nhập thông tin giao hàng mới cho đơn này.</small></span>
        </label>
        {selection === "manual" && <div className="checkout-shipping-fields">
          <label>Họ và tên<input required value={shipping.recipientName} onChange={(event) => setShipping((current) => ({ ...current, recipientName: event.target.value }))} placeholder="Người nhận hàng" /></label>
          <label>Số điện thoại<input required inputMode="tel" value={shipping.recipientPhone} onChange={(event) => setShipping((current) => ({ ...current, recipientPhone: event.target.value }))} placeholder="Ví dụ: 0901 234 567" /></label>
          <label className="checkout-shipping-full">Địa chỉ nhận hàng<textarea required rows={2} value={shipping.shippingAddress} onChange={(event) => setShipping((current) => ({ ...current, shippingAddress: event.target.value }))} placeholder="Số nhà, tên đường, phường/xã, quận/huyện, tỉnh/thành" /></label>
          <label className="checkout-shipping-full">Ghi chú giao hàng <span>(không bắt buộc)</span><textarea rows={2} value={shipping.deliveryNote} onChange={(event) => setShipping((current) => ({ ...current, deliveryNote: event.target.value }))} placeholder="Ví dụ: Gọi trước khi giao" /></label>
          <label className="checkout-save-profile"><input type="checkbox" checked={shipping.saveAsProfile} onChange={(event) => setShipping((current) => ({ ...current, saveAsProfile: event.target.checked, setAsDefault: event.target.checked ? current.setAsDefault : false }))} />Lưu địa chỉ này cho lần sau</label>
          {shipping.saveAsProfile && <label className="checkout-save-profile"><input type="checkbox" checked={shipping.setAsDefault} onChange={(event) => setShipping((current) => ({ ...current, setAsDefault: event.target.checked }))} />Đặt làm địa chỉ mặc định</label>}
        </div>}
        {selection !== "manual" && <label className="checkout-note">Ghi chú giao hàng <span>(không bắt buộc)</span><textarea rows={2} value={shipping.deliveryNote} onChange={(event) => setShipping((current) => ({ ...current, deliveryNote: event.target.value }))} placeholder="Ví dụ: Gọi trước khi giao" /></label>}
      </fieldset>
      {error && <p className="checkout-confirm-error" role="alert">{error}</p>}
      <div className="checkout-confirm-actions"><Button type="button" variant="outline" onClick={onClose} disabled={isSubmitting}>Xem lại giỏ</Button><Button type="submit" disabled={isSubmitting}>{isSubmitting ? "Đang tạo đơn..." : <><CircleCheck />Xác nhận</>}</Button></div>
    </form>
  </dialog>;
}
