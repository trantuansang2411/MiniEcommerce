"use client";

import { Check, ChevronDown, Pencil, Plus } from "lucide-react";
import * as Select from "@radix-ui/react-select";
import { useCallback, useEffect, useState } from "react";
import { managementService } from "@/modules/operations/api/management.service";
import { DashboardPageHeader } from "@/modules/operations/components/dashboard-shell";
import { ManagementModal } from "@/modules/operations/components/management-modal";
import {
  DataState,
  StatusBadge,
  fieldClass,
  tableClass,
  tableWrapClass,
  tdClass,
  thClass,
} from "@/modules/operations/components/management-ui";
import type { Warehouse } from "@/modules/operations/types/operations.type";
import { Button } from "@/shared/components/ui/button";
import { useToast } from "@/shared/components/ui/toast";

type WarehouseModalProps = {
  warehouse?: Warehouse;
  busy: boolean;
  onClose: () => void;
  onSave: (event: React.FormEvent<HTMLFormElement>) => void;
};

const warehouseStatuses = [
  { value: "1", label: "Hoạt động", description: "Sẵn sàng vận hành" },
  { value: "2", label: "Tạm ngưng", description: "Tạm dừng tạm thời" },
  { value: "3", label: "Đóng", description: "Ngừng hoạt động" },
] as const;

function WarehouseStatusSelect({ defaultValue }: Readonly<{ defaultValue: string }>) {
  return (
    <Select.Root name="status" defaultValue={defaultValue}>
      <Select.Trigger
        aria-label="Trạng thái kho"
        className="flex h-11 w-full items-center justify-between rounded-xl border border-slate-200 bg-slate-50 px-3.5 text-left text-sm font-medium text-slate-700 outline-none transition-[border-color,box-shadow,background-color] hover:border-slate-300 hover:bg-white focus:border-cyan-500 focus:bg-white focus:ring-4 focus:ring-cyan-500/10 data-[state=open]:border-cyan-500 data-[state=open]:bg-white"
      >
        <Select.Value />
        <Select.Icon className="grid size-6 place-items-center rounded-md text-slate-400 transition-transform data-[state=open]:rotate-180">
          <ChevronDown className="size-4" strokeWidth={2.25} />
        </Select.Icon>
      </Select.Trigger>

      <Select.Portal>
        <Select.Content
          position="popper"
          sideOffset={6}
          className="z-[100] min-w-[var(--radix-select-trigger-width)] overflow-hidden rounded-xl border border-slate-200 bg-white p-1.5 text-slate-800 shadow-[0_16px_35px_rgba(15,23,42,.14),0_2px_8px_rgba(15,23,42,.06)]"
        >
          <Select.Viewport>
            {warehouseStatuses.map((status) => (
              <Select.Item
                key={status.value}
                value={status.value}
                className="group relative flex min-h-12 cursor-pointer select-none items-center rounded-lg py-2 pl-10 pr-3 outline-none transition-colors data-[highlighted]:bg-cyan-50 data-[highlighted]:text-cyan-800 data-[state=checked]:bg-cyan-600 data-[state=checked]:text-white"
              >
                <Select.ItemIndicator className="absolute left-3 grid size-5 place-items-center">
                  <Check className="size-4" strokeWidth={2.5} />
                </Select.ItemIndicator>
                <div className="grid gap-0.5">
                  <Select.ItemText className="text-sm font-semibold" />
                  <span className="text-xs text-slate-400 group-data-[highlighted]:text-cyan-700 group-data-[state=checked]:text-cyan-100">
                    {status.description}
                  </span>
                </div>
              </Select.Item>
            ))}
          </Select.Viewport>
        </Select.Content>
      </Select.Portal>
    </Select.Root>
  );
}

function WarehouseModal({ warehouse, busy, onClose, onSave }: Readonly<WarehouseModalProps>) {
  const isEditing = Boolean(warehouse);

  return (
    <ManagementModal
      eyebrow="ADMIN / WAREHOUSING"
      title={isEditing ? "Sửa kho hàng" : "Thêm kho hàng"}
      description={isEditing ? "Cập nhật tên, địa chỉ và trạng thái vận hành của kho." : "Tạo một điểm lưu trữ mới để tiếp nhận và phân bổ hàng hóa."}
      onClose={onClose}
    >
      <form onSubmit={onSave} className="mt-6 grid gap-4">
        <label className="grid gap-2 text-sm font-bold text-slate-700">
          Mã kho
          <input
            autoFocus
            className={fieldClass}
            name="code"
            defaultValue={warehouse?.code ?? ""}
            placeholder="Ví dụ: HCM-01"
            disabled={isEditing}
            required={!isEditing}
          />
          {isEditing && <span className="text-xs font-normal text-slate-400">Mã kho được cố định sau khi tạo.</span>}
        </label>
        <label className="grid gap-2 text-sm font-bold text-slate-700">
          Tên kho
          <input className={fieldClass} name="name" defaultValue={warehouse?.name ?? ""} placeholder="Ví dụ: Kho Hồ Chí Minh" required />
        </label>
        <label className="grid gap-2 text-sm font-bold text-slate-700">
          Địa chỉ
          <textarea
            className={`${fieldClass} min-h-24 resize-y py-3`}
            name="address"
            defaultValue={warehouse?.address ?? ""}
            placeholder="Địa chỉ vận hành kho"
            required
          />
        </label>
        {warehouse && (
          <label className="grid gap-2 text-sm font-bold text-slate-700">
            Trạng thái
            <WarehouseStatusSelect defaultValue={warehouse.status === "Active" ? "1" : warehouse.status === "Inactive" ? "2" : "3"} />
          </label>
        )}
        <footer className="mt-3 flex justify-end gap-3 border-t border-slate-100 pt-5">
          <Button type="button" variant="secondary" className="bg-slate-100 text-slate-700 hover:bg-slate-200 hover:text-slate-900" onClick={onClose}>
            Hủy
          </Button>
          <Button type="submit" disabled={busy}>{busy ? "Đang lưu..." : isEditing ? "Lưu thay đổi" : "Tạo kho"}</Button>
        </footer>
      </form>
    </ManagementModal>
  );
}

export function WarehouseManagement() {
  const toast = useToast();
  const [items, setItems] = useState<Warehouse[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingWarehouse, setEditingWarehouse] = useState<Warehouse | null | undefined>(undefined);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setItems(await managementService.getWarehouses());
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Không tải được kho hàng.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  async function save(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const payload = {
      name: String(data.get("name") ?? "").trim(),
      address: String(data.get("address") ?? "").trim(),
    };

    setBusy(true);
    setError(null);
    try {
      if (editingWarehouse) {
        await managementService.updateWarehouse(editingWarehouse.id, {
          ...payload,
          status: Number(data.get("status")),
        });
      } else {
        await managementService.createWarehouse({
          ...payload,
          code: String(data.get("code") ?? "").trim(),
        });
      }

      setEditingWarehouse(undefined);
      await load();
      toast.success(editingWarehouse ? "Đã cập nhật kho hàng." : "Đã tạo kho hàng.");
    } catch (requestError) {
      const message = requestError instanceof Error ? requestError.message : "Không thể lưu kho hàng.";
      setError(message);
      toast.error(message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <DashboardPageHeader
        eyebrow="ADMIN / WAREHOUSING"
        title="Kho hàng"
        description="Tạo điểm lưu trữ và kiểm soát trạng thái hoạt động của từng kho."
        action={<Button onClick={() => setEditingWarehouse(null)}><Plus className="size-4" />Thêm kho</Button>}
      />

      <DataState loading={loading} error={error} empty={!items.length}>
        <div className={tableWrapClass}>
          <table className={tableClass}>
            <thead>
              <tr><th className={thClass}>Mã kho</th><th className={thClass}>Tên kho</th><th className={thClass}>Địa chỉ</th><th className={thClass}>Trạng thái</th><th className={thClass}>Thao tác</th></tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td className={`${tdClass} font-black text-cyan-700`}>{item.code}</td>
                  <td className={`${tdClass} font-bold text-slate-900`}>{item.name}</td>
                  <td className={tdClass}>{item.address}</td>
                  <td className={tdClass}><StatusBadge value={item.status} /></td>
                  <td className={tdClass}>
                    <Button size="icon" variant="ghost" aria-label={`Sửa kho ${item.name}`} onClick={() => setEditingWarehouse(item)}>
                      <Pencil className="size-4 text-cyan-700" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </DataState>

      {editingWarehouse !== undefined && (
        <WarehouseModal
          key={editingWarehouse?.id ?? "new-warehouse"}
          warehouse={editingWarehouse ?? undefined}
          busy={busy}
          onClose={() => setEditingWarehouse(undefined)}
          onSave={(event) => void save(event)}
        />
      )}
    </>
  );
}
