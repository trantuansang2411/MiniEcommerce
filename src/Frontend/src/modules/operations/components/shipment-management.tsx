"use client";

import { Truck } from "lucide-react";
import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { managementService } from "@/modules/operations/api/management.service";
import { DashboardPageHeader } from "@/modules/operations/components/dashboard-shell";
import { ManagementModal } from "@/modules/operations/components/management-modal";
import { DataState, StatusBadge, fieldClass, shortId, tableClass, tableWrapClass, tdClass, thClass } from "@/modules/operations/components/management-ui";
import type { DashboardRole, Shipment } from "@/modules/operations/types/operations.type";
import { Button } from "@/shared/components/ui/button";
import { useToast } from "@/shared/components/ui/toast";
import { formatStatus, shipmentStatusOptions } from "@/shared/utils/status";

const statuses = shipmentStatusOptions;
const nextStatus: Partial<Record<Shipment["status"], { value: number; label: string }>> = { Pending: { value: 2, label: "Bắt đầu lấy hàng" }, Picking: { value: 3, label: "Xác nhận đóng gói" }, Packed: { value: 4, label: "Bàn giao vận chuyển" }, Shipped: { value: 5, label: "Đang giao" }, InTransit: { value: 6, label: "Đã giao" } };

type ShipmentModalProps = {
  shipment: Shipment;
  busy: boolean;
  onClose: () => void;
  onSave: (event: React.FormEvent<HTMLFormElement>) => void;
};

function ShipmentModal({ shipment, busy, onClose, onSave }: Readonly<ShipmentModalProps>) {
  const transition = nextStatus[shipment.status];
  if (!transition) return null;

  return (
    <ManagementModal
      eyebrow="STAFF / FULFILLMENT"
      title={transition.label}
      description={`Shipment ${shortId(shipment.id)} sẽ chuyển từ ${formatStatus(shipment.status)} sang bước kế tiếp.`}
      onClose={onClose}
    >
      <form onSubmit={onSave} className="mt-6 grid gap-4">
        {shipment.status === "Packed" && (
          <>
            <label className="grid gap-2 text-sm font-bold text-slate-700">
              Đơn vị vận chuyển
              <input name="shippingProvider" className={fieldClass} placeholder="Ví dụ: Giao Hàng Nhanh" required />
            </label>
            <label className="grid gap-2 text-sm font-bold text-slate-700">
              Mã vận đơn
              <input name="trackingNumber" className={fieldClass} placeholder="Nhập mã vận đơn" required />
            </label>
          </>
        )}
        <footer className="mt-3 flex justify-end gap-3 border-t border-slate-100 pt-5">
          <Button type="button" variant="secondary" className="bg-slate-100 text-slate-700 hover:bg-slate-200 hover:text-slate-900" onClick={onClose}>Hủy</Button>
          <Button type="submit" disabled={busy}>{busy ? "Đang cập nhật..." : "Xác nhận"}</Button>
        </footer>
      </form>
    </ManagementModal>
  );
}

export function ShipmentManagement({ role }: Readonly<{ role: DashboardRole }>) {
  const toast = useToast();
  const [items, setItems] = useState<Shipment[]>([]);
  const [status, setStatus] = useState("");
  const [selected, setSelected] = useState<Shipment | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setItems(await managementService.getShipments(status ? Number(status) : undefined));
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Không tải được shipment.");
    } finally {
      setLoading(false);
    }
  }, [status]);

  useEffect(() => {
    void load();
  }, [load]);

  async function update(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selected) return;
    const transition = nextStatus[selected.status];
    if (!transition) return;

    const data = new FormData(event.currentTarget);
    setBusy(true);
    setError(null);
    try {
      await managementService.updateShipment(selected.id, {
        status: transition.value,
        shippingProvider: String(data.get("shippingProvider") ?? "") || undefined,
        trackingNumber: String(data.get("trackingNumber") ?? "") || undefined,
      });
      setSelected(null);
      await load();
      toast.success("Đã cập nhật trạng thái vận chuyển.");
    } catch (requestError) {
      const message = requestError instanceof Error ? requestError.message : "Không thể cập nhật shipment.";
      setError(message);
      toast.error(message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <DashboardPageHeader
        eyebrow={`${role.toUpperCase()} / FULFILLMENT`}
        title="Vận chuyển"
        description={role === "Staff" ? "Cập nhật shipment tuần tự từ lấy hàng đến giao thành công." : "Theo dõi tiến độ xử lý và giao nhận của tất cả shipment."}
      />
      <div className="mb-5 max-w-xs">
        <select className={fieldClass} value={status} onChange={(event) => setStatus(event.target.value)}>
          {statuses.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}
        </select>
      </div>
      <DataState loading={loading} error={error} empty={!items.length}>
        <div className={tableWrapClass}>
          <table className={tableClass}>
            <thead>
              <tr><th className={thClass}>Shipment</th><th className={thClass}>Order</th><th className={thClass}>Số mặt hàng</th><th className={thClass}>Đơn vị vận chuyển</th><th className={thClass}>Trạng thái</th>{role === "Staff" && <th className={thClass}>Thao tác</th>}</tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td className={`${tdClass} font-mono font-bold text-slate-900`} title={item.id}>{shortId(item.id)}</td>
                  <td className={`${tdClass} font-mono`} title={item.orderId}>{shortId(item.orderId)}</td>
                  <td className={`${tdClass} tabular-nums`}>{item.items.reduce((sum, product) => sum + product.quantity, 0)}</td>
                  <td className={tdClass}>{item.shippingProvider ? `${item.shippingProvider} · ${item.trackingNumber}` : "—"}</td>
                  <td className={tdClass}><StatusBadge value={item.status} /></td>
                  {role === "Staff" && (
                    <td className={`${tdClass} whitespace-nowrap`}>
                      <div className="flex flex-wrap gap-2">
                        <Button asChild size="sm" variant="outline"><Link href={`/staff/shipments/${item.id}`}>Chi tiết</Link></Button>
                        {nextStatus[item.status]
                          ? <Button size="sm" className="bg-cyan-600 text-white shadow-[0_6px_14px_rgba(8,145,178,.24)] hover:bg-cyan-700" onClick={() => setSelected(item)}><Truck className="size-4" />Cập nhật</Button>
                          : <span className="self-center text-xs text-slate-400">Đã kết thúc</span>}
                      </div>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </DataState>
      {selected && <ShipmentModal shipment={selected} busy={busy} onClose={() => setSelected(null)} onSave={(event) => void update(event)} />}
    </>
  );
}
