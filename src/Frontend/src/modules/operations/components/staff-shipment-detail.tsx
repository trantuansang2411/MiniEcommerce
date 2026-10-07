"use client";

import Image from "next/image";
import Link from "next/link";
import { ArrowLeft, ClipboardList, ExternalLink, MapPin, PackageCheck, Phone, Truck } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { managementService } from "@/modules/operations/api/management.service";
import { DataState, fieldClass, StatusBadge } from "@/modules/operations/components/management-ui";
import type { ShipmentDetail } from "@/modules/operations/types/operations.type";
import { Button } from "@/shared/components/ui/button";
import { productImageUrl } from "@/shared/utils/format";
import { useToast } from "@/shared/components/ui/toast";
import { formatStatus } from "@/shared/utils/status";

const stages = ["Pending", "Picking", "Packed", "Shipped", "InTransit", "Delivered"] as const;
const nextStatus: Partial<Record<ShipmentDetail["status"], { value: number; label: string }>> = { Pending: { value: 2, label: "Bắt đầu lấy hàng" }, Picking: { value: 3, label: "Xác nhận đóng gói" }, Packed: { value: 4, label: "Bàn giao vận chuyển" }, Shipped: { value: 5, label: "Xác nhận đang giao" }, InTransit: { value: 6, label: "Xác nhận đã giao" } };
const stageIndex = (status: ShipmentDetail["status"]) => stages.indexOf(status as (typeof stages)[number]);
const time = (value?: string | null) => value ? new Intl.DateTimeFormat("vi-VN", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value)) : "Chưa ghi nhận";
const shortId = (id: string) => id.slice(0, 8).toUpperCase();

export function StaffShipmentDetail({ shipmentId }: Readonly<{ shipmentId: string }>) {
  const toast = useToast();
  const [shipment, setShipment] = useState<ShipmentDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [updating, setUpdating] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try { setShipment(await managementService.getShipmentDetail(shipmentId)); }
    catch (requestError) { setError(requestError instanceof Error ? requestError.message : "Không tải được chi tiết shipment."); }
    finally { setLoading(false); }
  }, [shipmentId]);

  useEffect(() => { void load(); }, [load]);

  async function update(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!shipment || !nextStatus[shipment.status]) return;
    const data = new FormData(event.currentTarget);
    setUpdating(true);
    try {
      await managementService.updateShipment(shipment.id, { status: nextStatus[shipment.status]!.value, shippingProvider: String(data.get("shippingProvider") ?? "") || undefined, trackingNumber: String(data.get("trackingNumber") ?? "") || undefined });
      toast.success("Đã cập nhật trạng thái shipment.");
      await load();
    } catch (requestError) {
      const message = requestError instanceof Error ? requestError.message : "Không thể cập nhật shipment.";
      setError(message);
      toast.error(message);
    } finally { setUpdating(false); }
  }

  return <DataState loading={loading} error={error} empty={!shipment}>
    {shipment && <ShipmentView shipment={shipment} updating={updating} onUpdate={update} />}
  </DataState>;
}

function ShipmentView({ shipment, updating, onUpdate }: Readonly<{ shipment: ShipmentDetail; updating: boolean; onUpdate: (event: React.FormEvent<HTMLFormElement>) => void }>) {
  const current = stageIndex(shipment.status);
  const final = shipment.status === "Cancelled";

  return <div className="mx-auto max-w-6xl">
    <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
      <Button asChild variant="ghost" className="-ml-3 text-slate-600"><Link href="/staff/shipments"><ArrowLeft className="size-4" />Quay lại danh sách</Link></Button>
      <p className="text-xs font-semibold text-slate-400">Order <span className="font-mono text-slate-600">#{shortId(shipment.orderId)}</span></p>
    </div>

    <header className="relative overflow-hidden rounded-2xl bg-[#07111f] px-6 py-7 text-white shadow-[0_18px_45px_rgba(7,17,31,.18)] sm:px-8">
      <div className="absolute -right-16 -top-24 size-64 rounded-full border border-cyan-200/15" /><div className="absolute right-16 top-8 size-36 rounded-full border border-cyan-200/10" />
      <div className="relative flex flex-col gap-5 sm:flex-row sm:items-end sm:justify-between"><div><p className="text-[11px] font-black tracking-[.16em] text-cyan-300">STAFF / FULFILLMENT</p><h1 className="mt-2 text-3xl font-black tracking-[-.045em]">Shipment <span className="font-mono text-cyan-300">#{shortId(shipment.id)}</span></h1><p className="mt-2 text-sm text-slate-300">Xác nhận hàng, địa chỉ và tiến độ trước khi chuyển bước xử lý.</p></div><StatusBadge value={shipment.status} /></div>
    </header>

    <div className="mt-6 grid gap-6 lg:grid-cols-[minmax(0,1.35fr)_minmax(300px,.85fr)]">
      <div className="grid gap-6">
        <section className="rounded-2xl border border-cyan-100 bg-gradient-to-br from-cyan-50 to-white p-5 shadow-[0_10px_28px_rgba(8,145,178,.07)] sm:p-6"><div className="flex gap-3"><span className="grid size-10 shrink-0 place-items-center rounded-xl rounded-bl-md bg-cyan-600 text-white"><MapPin className="size-5" /></span><div className="min-w-0"><p className="text-[11px] font-black tracking-[.13em] text-cyan-700">THÔNG TIN NGƯỜI NHẬN</p><h2 className="mt-1 text-lg font-black text-slate-900">{shipment.delivery.recipientName ?? "Chưa có tên người nhận"}</h2><p className="mt-1 flex items-center gap-2 text-sm font-medium text-slate-600"><Phone className="size-4 text-cyan-700" />{shipment.delivery.recipientPhone ?? "Chưa có số điện thoại"}</p><p className="mt-4 text-sm leading-6 text-slate-700">{shipment.delivery.shippingAddress ?? "Chưa có địa chỉ giao hàng"}</p>{shipment.delivery.deliveryNote && <p className="mt-3 rounded-lg border border-cyan-100 bg-white/80 px-3 py-2 text-sm text-slate-600"><span className="font-bold text-slate-700">Ghi chú: </span>{shipment.delivery.deliveryNote}</p>}</div></div></section>
        <section className="rounded-2xl border border-slate-200 bg-white shadow-[0_10px_28px_rgba(15,32,55,.05)]"><div className="flex items-center justify-between border-b border-slate-100 px-5 py-4 sm:px-6"><div><p className="text-[11px] font-black tracking-[.13em] text-cyan-700">HÀNG CẦN XỬ LÝ</p><h2 className="mt-1 font-black text-slate-900">{shipment.items.length} sản phẩm trong kiện</h2></div><PackageCheck className="size-6 text-cyan-600" /></div><div className="divide-y divide-slate-100">{shipment.items.map((item) => <div key={item.productId} className="flex gap-4 px-5 py-4 sm:px-6"><Image src={item.thumbnailUrl ? productImageUrl(item.thumbnailUrl) : "/brand/AnhEcommerc.png"} alt="" width={56} height={56} unoptimized className="size-14 rounded-xl border border-slate-100 object-cover" /><div className="min-w-0 flex-1"><h3 className="truncate text-sm font-bold text-slate-800">{item.productName}</h3><p className="mt-1 text-xs text-slate-400">Mã SP: <span className="font-mono">{shortId(item.productId)}</span></p></div><span className="self-center rounded-lg bg-slate-100 px-3 py-1.5 text-sm font-black tabular-nums text-slate-700">× {item.quantity}</span></div>)}</div></section>
      </div>
      <div className="grid content-start gap-6">
        <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-[0_10px_28px_rgba(15,32,55,.05)]"><div className="flex items-center gap-3"><span className="grid size-10 place-items-center rounded-xl rounded-bl-md bg-slate-900 text-cyan-300"><Truck className="size-5" /></span><div><p className="text-[11px] font-black tracking-[.13em] text-cyan-700">VẬN CHUYỂN</p><h2 className="font-black text-slate-900">{shipment.shippingProvider ?? "Chưa bàn giao"}</h2></div></div><div className="mt-5 grid gap-3 rounded-xl bg-slate-50 p-4 text-sm"><p className="text-slate-500">Mã vận đơn</p><p className="flex items-center justify-between gap-3 font-mono font-bold text-slate-800">{shipment.trackingNumber ?? "—"}{shipment.trackingNumber && <ExternalLink className="size-4 text-cyan-700" />}</p></div></section>
        {nextStatus[shipment.status] && <section className="rounded-2xl border border-cyan-200 bg-cyan-50/60 p-5 shadow-[0_10px_28px_rgba(8,145,178,.07)]"><p className="text-[11px] font-black tracking-[.13em] text-cyan-700">BƯỚC TIẾP THEO</p><h2 className="mt-1 font-black text-slate-900">{nextStatus[shipment.status]!.label}</h2><form className="mt-4 grid gap-3" onSubmit={onUpdate}>{shipment.status === "Packed" && <><label className="grid gap-1.5 text-xs font-bold text-slate-600">Đơn vị vận chuyển<input name="shippingProvider" required className={fieldClass} placeholder="Ví dụ: Giao Hàng Nhanh" /></label><label className="grid gap-1.5 text-xs font-bold text-slate-600">Mã vận đơn<input name="trackingNumber" required className={fieldClass} placeholder="Nhập mã vận đơn" /></label></>}<Button type="submit" disabled={updating} className="mt-1 w-full bg-cyan-600 text-white hover:bg-cyan-700"><Truck className="size-4" />{updating ? "Đang cập nhật..." : "Xác nhận cập nhật"}</Button></form></section>}
        <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-[0_10px_28px_rgba(15,32,55,.05)]"><div className="flex items-center gap-3"><ClipboardList className="size-5 text-cyan-700" /><h2 className="font-black text-slate-900">Tiến trình xử lý</h2></div><ol className="mt-5 grid gap-0">{stages.map((stage, index) => { const done = !final && index <= current; const active = stage === shipment.status; const timestamp = stage === "Pending" ? shipment.createdAt : stage === "Shipped" ? shipment.shippedAt : stage === "Delivered" ? shipment.deliveredAt : null; return <li key={stage} className="relative grid grid-cols-[24px_minmax(0,1fr)] gap-3 pb-4 last:pb-0"><span className={`relative z-10 mt-0.5 grid size-6 place-items-center rounded-full border-2 text-[10px] font-black ${done ? "border-cyan-500 bg-cyan-500 text-white" : "border-slate-200 bg-white text-slate-400"}`}>{index + 1}</span>{index < stages.length - 1 && <span className={`absolute left-[11px] top-7 h-[calc(100%-10px)] w-px ${done && index < current ? "bg-cyan-400" : "bg-slate-200"}`} />}<div><p className={`text-sm font-bold ${active ? "text-cyan-700" : done ? "text-slate-700" : "text-slate-400"}`}>{formatStatus(stage)}</p><p className="mt-0.5 text-xs text-slate-400">{timestamp ? time(timestamp) : done ? "Đã hoàn tất" : "Chờ xử lý"}</p></div></li>; })}</ol></section>
      </div>
    </div>
  </div>;
}
