"use client";

import { useState } from "react";
import { Check, MapPin, PackagePlus, Pencil, Phone, Plus, Star, Trash2, X } from "lucide-react";
import { useShippingProfileActions, useShippingProfiles } from "@/modules/shipping/hooks/use-shipping-profiles";
import type { ShippingProfile } from "@/modules/shipping/types/shipping-profile.type";
import type { ShippingProfileInput } from "@/modules/shipping/api/shipping-profile.service";
import { Button } from "@/shared/components/ui/button";
import { useToast } from "@/shared/components/ui/toast";

const blankForm: ShippingProfileInput = { recipientName: "", recipientPhone: "", shippingAddress: "", isDefault: false };

export function ShippingProfileManager({ enabled }: Readonly<{ enabled: boolean }>) {
  const profiles = useShippingProfiles(enabled);
  const actions = useShippingProfileActions();
  const toast = useToast();
  const [editingProfile, setEditingProfile] = useState<ShippingProfile | null>(null);
  const [form, setForm] = useState<ShippingProfileInput>(blankForm);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const isSaving = actions.create.isPending || actions.update.isPending;

  function openCreate() {
    setEditingProfile(null);
    setForm({ ...blankForm, isDefault: profiles.data?.length === 0 });
    setIsFormOpen(true);
  }

  function openEdit(profile: ShippingProfile) {
    setEditingProfile(profile);
    setForm({ recipientName: profile.recipientName, recipientPhone: profile.recipientPhone, shippingAddress: profile.shippingAddress, isDefault: profile.isDefault });
    setIsFormOpen(true);
  }

  function closeForm() {
    if (isSaving) return;
    setIsFormOpen(false);
    setEditingProfile(null);
  }

  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    try {
      if (editingProfile) {
        await actions.update.mutateAsync({ id: editingProfile.id, input: form });
        toast.success("Đã cập nhật địa chỉ giao hàng.");
      } else {
        await actions.create.mutateAsync(form);
        toast.success("Đã lưu địa chỉ giao hàng.");
      }
      closeForm();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Không thể lưu địa chỉ. Vui lòng thử lại.");
    }
  }

  async function makeDefault(profile: ShippingProfile) {
    try {
      await actions.setDefault.mutateAsync(profile.id);
      toast.success("Đã đặt làm địa chỉ mặc định.");
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Không thể cập nhật địa chỉ mặc định.");
    }
  }

  async function remove(profile: ShippingProfile) {
    if (!window.confirm(`Xóa địa chỉ của ${profile.recipientName}?`)) return;
    try {
      await actions.remove.mutateAsync(profile.id);
      toast.success("Đã xóa địa chỉ giao hàng.");
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Không thể xóa địa chỉ.");
    }
  }

  return <section className="shipping-profile-manager" aria-labelledby="shipping-profile-title">
    <header className="shipping-profile-heading">
      <div><p className="eyebrow">ĐỊA CHỈ NHẬN HÀNG</p><h2 id="shipping-profile-title">Thông tin giao hàng</h2><p>Lưu nhiều địa chỉ để đặt hàng cho bạn hoặc người thân nhanh hơn.</p></div>
      <Button type="button" onClick={openCreate}><Plus className="size-4" />Thêm địa chỉ</Button>
    </header>

    {isFormOpen && <form className="shipping-profile-form" onSubmit={submit}>
      <div className="shipping-profile-form-heading"><span><PackagePlus /><strong>{editingProfile ? "Cập nhật địa chỉ" : "Thêm địa chỉ mới"}</strong></span><button type="button" aria-label="Đóng form" onClick={closeForm} disabled={isSaving}><X /></button></div>
      <div className="shipping-profile-fields">
        <label>Họ và tên<input required value={form.recipientName} onChange={(event) => setForm((current) => ({ ...current, recipientName: event.target.value }))} placeholder="Tên người nhận" /></label>
        <label>Số điện thoại<input required inputMode="tel" value={form.recipientPhone} onChange={(event) => setForm((current) => ({ ...current, recipientPhone: event.target.value }))} placeholder="Ví dụ: 0901 234 567" /></label>
        <label className="shipping-profile-field-full">Địa chỉ nhận hàng<textarea required rows={3} value={form.shippingAddress} onChange={(event) => setForm((current) => ({ ...current, shippingAddress: event.target.value }))} placeholder="Số nhà, tên đường, phường/xã, quận/huyện, tỉnh/thành" /></label>
        <label className="shipping-profile-default-toggle"><input type="checkbox" checked={form.isDefault} onChange={(event) => setForm((current) => ({ ...current, isDefault: event.target.checked }))} />Đặt làm địa chỉ mặc định</label>
      </div>
      <div className="shipping-profile-form-actions"><Button type="button" variant="outline" onClick={closeForm} disabled={isSaving}>Hủy</Button><Button type="submit" disabled={isSaving}>{isSaving ? "Đang lưu..." : <><Check className="size-4" />Lưu địa chỉ</>}</Button></div>
    </form>}

    {profiles.isLoading ? <div className="shipping-profile-loading">Đang tải địa chỉ đã lưu...</div> : profiles.error ? <div className="shipping-profile-error">{profiles.error.message}</div> : profiles.data?.length ? <div className="shipping-profile-list">{profiles.data.map((profile) => <article className={`shipping-profile-card ${profile.isDefault ? "is-default" : ""}`} key={profile.id}>
      <div className="shipping-profile-marker"><MapPin /></div>
      <div className="shipping-profile-copy"><div><h3>{profile.recipientName}</h3>{profile.isDefault && <span>Địa chỉ mặc định</span>}</div><p><Phone />{profile.recipientPhone}</p><p className="shipping-profile-address">{profile.shippingAddress}</p></div>
      <div className="shipping-profile-actions"><Button type="button" variant="ghost" size="sm" onClick={() => openEdit(profile)}><Pencil />Sửa</Button>{!profile.isDefault && <Button type="button" variant="outline" size="sm" disabled={actions.setDefault.isPending} onClick={() => void makeDefault(profile)}><Star />Mặc định</Button>}<Button type="button" variant="ghost" size="sm" className="shipping-profile-delete" disabled={actions.remove.isPending} onClick={() => void remove(profile)}><Trash2 /><span className="sr-only">Xóa địa chỉ</span></Button></div>
    </article>)}</div> : <div className="shipping-profile-empty"><span><MapPin /></span><h3>Chưa có địa chỉ giao hàng</h3><p>Thêm địa chỉ đầu tiên để checkout nhanh hơn trong lần mua tới.</p><Button type="button" variant="outline" onClick={openCreate}><Plus className="size-4" />Thêm địa chỉ đầu tiên</Button></div>}
  </section>;
}
