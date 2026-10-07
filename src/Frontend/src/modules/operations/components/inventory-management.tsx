"use client";

import { History, PackagePlus, SlidersHorizontal } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import type { Product } from "@/modules/catalog/types/catalog.type";
import { managementService } from "@/modules/operations/api/management.service";
import { DashboardPageHeader } from "@/modules/operations/components/dashboard-shell";
import { ManagementModal } from "@/modules/operations/components/management-modal";
import { DataState, fieldClass, tableClass, tableWrapClass, tdClass, thClass } from "@/modules/operations/components/management-ui";
import type { DashboardRole, Inventory, InventoryTransaction, Warehouse } from "@/modules/operations/types/operations.type";
import { Button } from "@/shared/components/ui/button";
import { useToast } from "@/shared/components/ui/toast";

const reasons = [{ value: 1, label: "Hư hỏng" }, { value: 2, label: "Thất lạc" }, { value: 3, label: "Lệch kiểm kê" }, { value: 4, label: "Hết hạn" }, { value: 5, label: "Trả nhà cung cấp" }, { value: 6, label: "Điều chỉnh" }];

type InventoryModalProps = {
  mode: "stock" | "adjust";
  warehouses: Warehouse[];
  products: Product[];
  busy: boolean;
  onClose: () => void;
  onSave: (event: React.FormEvent<HTMLFormElement>) => void;
};

function InventoryModal({ mode, warehouses, products, busy, onClose, onSave }: Readonly<InventoryModalProps>) {
  const isStockIn = mode === "stock";

  return (
    <ManagementModal
      eyebrow="MANAGER / WAREHOUSING"
      title={isStockIn ? "Nhập hàng vào kho" : "Điều chỉnh tồn kho"}
      description={isStockIn ? "Ghi nhận số lượng hàng mới nhận tại kho được chọn." : "Ghi nhận chênh lệch tồn kho và lý do điều chỉnh để lưu lịch sử."}
      onClose={onClose}
    >
      <form onSubmit={onSave} className="mt-6 grid gap-4">
        <label className="grid gap-2 text-sm font-bold text-slate-700">
          Kho
          <select name="warehouseId" className={fieldClass} required defaultValue="">
            <option value="" disabled>Chọn kho</option>
            {warehouses.map((item) => <option key={item.id} value={item.id}>{item.code} — {item.name}</option>)}
          </select>
        </label>
        <label className="grid gap-2 text-sm font-bold text-slate-700">
          Sản phẩm
          <select name="productId" className={fieldClass} required defaultValue="">
            <option value="" disabled>Chọn sản phẩm</option>
            {products.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}
          </select>
        </label>
        {isStockIn ? (
          <label className="grid gap-2 text-sm font-bold text-slate-700">
            Số lượng nhập
            <input name="quantity" className={fieldClass} type="number" min="1" placeholder="Ví dụ: 20" required />
          </label>
        ) : (
          <>
            <label className="grid gap-2 text-sm font-bold text-slate-700">
              Số lượng thay đổi
              <input name="quantityChange" className={fieldClass} type="number" placeholder="Ví dụ: -2 hoặc 5" required />
            </label>
            <label className="grid gap-2 text-sm font-bold text-slate-700">
              Lý do
              <select name="reason" className={fieldClass} required>
                {reasons.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}
              </select>
            </label>
          </>
        )}
        <footer className="mt-3 flex justify-end gap-3 border-t border-slate-100 pt-5">
          <Button type="button" variant="secondary" className="bg-slate-100 text-slate-700 hover:bg-slate-200 hover:text-slate-900" onClick={onClose}>Hủy</Button>
          <Button type="submit" disabled={busy}>{busy ? "Đang lưu..." : isStockIn ? "Xác nhận nhập" : "Xác nhận điều chỉnh"}</Button>
        </footer>
      </form>
    </ManagementModal>
  );
}

export function InventoryManagement({ role }: Readonly<{ role: Extract<DashboardRole, "Admin" | "Manager"> }>) {
  const toast = useToast();
  const [items, setItems] = useState<Inventory[]>([]); const [warehouses, setWarehouses] = useState<Warehouse[]>([]); const [products, setProducts] = useState<Product[]>([]); const [warehouseId, setWarehouseId] = useState(""); const [mode, setMode] = useState<"stock" | "adjust" | null>(null); const [transactions, setTransactions] = useState<InventoryTransaction[] | null>(null); const [loading, setLoading] = useState(true); const [busy, setBusy] = useState(false); const [error, setError] = useState<string | null>(null);
  const load = useCallback(async () => { setLoading(true); setError(null); try { const [inventoryItems, warehouseItems] = await Promise.all([managementService.getInventories(warehouseId || undefined), managementService.getWarehouses()]); setItems(inventoryItems); setWarehouses(warehouseItems); if (role === "Manager") setProducts(await managementService.getCatalogProducts()); } catch (e) { setError(e instanceof Error ? e.message : "Không tải được tồn kho."); } finally { setLoading(false); } }, [role, warehouseId]);
  useEffect(() => { void load(); }, [load]);
  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);
    setBusy(true);
    try {
      const payload = { warehouseId: String(data.get("warehouseId")), productId: String(data.get("productId")) };
      if (mode === "stock") await managementService.stockIn({ ...payload, quantity: Number(data.get("quantity")) });
      else await managementService.adjustInventory({ ...payload, quantityChange: Number(data.get("quantityChange")), reason: Number(data.get("reason")) });
      form.reset();
      setMode(null);
      await load();
      toast.success(mode === "stock" ? "Đã nhập hàng vào kho." : "Đã điều chỉnh tồn kho.");
    } catch (error) {
      const message = error instanceof Error ? error.message : "Không thể cập nhật tồn kho.";
      setError(message);
      toast.error(message);
    } finally {
      setBusy(false);
    }
  }

  async function showHistory(id: string) {
    try {
      setTransactions(await managementService.getInventoryTransactions(id));
    } catch (error) {
      const message = error instanceof Error ? error.message : "Không tải được lịch sử.";
      setError(message);
      toast.error(message);
    }
  }
  return <><DashboardPageHeader eyebrow={`${role.toUpperCase()} / WAREHOUSING`} title="Tồn kho" description={role === "Manager" ? "Nhập hàng, điều chỉnh chênh lệch và theo dõi số lượng khả dụng." : "Theo dõi tồn vật lý, lượng đã giữ và số lượng có thể bán tại từng kho."} action={role === "Manager" ? <div className="flex gap-2"><Button variant="secondary" onClick={() => setMode("adjust")}><SlidersHorizontal className="size-4" />Điều chỉnh</Button><Button onClick={() => setMode("stock")}><PackagePlus className="size-4" />Nhập hàng</Button></div> : undefined} />
    <div className="mb-5 flex max-w-xs"><select className={fieldClass} value={warehouseId} onChange={(event) => setWarehouseId(event.target.value)}><option value="">Tất cả kho</option>{warehouses.map((item) => <option key={item.id} value={item.id}>{item.code} — {item.name}</option>)}</select></div>
    {mode && <InventoryModal mode={mode} warehouses={warehouses} products={products} busy={busy} onClose={() => setMode(null)} onSave={(event) => void submit(event)} />}
    <DataState loading={loading} error={error} empty={!items.length}><div className={tableWrapClass}><table className={tableClass}><thead><tr><th className={thClass}>Kho</th><th className={thClass}>Sản phẩm</th><th className={thClass}>On hand</th><th className={thClass}>Reserved</th><th className={thClass}>Available</th><th className={thClass}>Lịch sử</th></tr></thead><tbody>{items.map((item) => <tr key={item.id}><td className={`${tdClass} font-black text-cyan-700`}>{item.warehouseCode}</td><td className={`${tdClass} font-bold text-slate-900`}>{item.productName}</td><td className={`${tdClass} tabular-nums`}>{item.onHand}</td><td className={`${tdClass} tabular-nums`}>{item.reserved}</td><td className={`${tdClass} font-black tabular-nums text-emerald-700`}>{item.available}</td><td className={tdClass}><Button size="sm" variant="ghost" onClick={() => void showHistory(item.id)}><History className="size-4" />Xem</Button></td></tr>)}</tbody></table></div></DataState>
    {transactions && <section className="mt-6 rounded-2xl bg-[#07111f] p-5 text-slate-100"><div className="mb-4 flex items-center justify-between"><h2 className="font-black">Lịch sử biến động</h2><button className="text-xs text-slate-400 hover:text-white" onClick={() => setTransactions(null)}>Đóng</button></div><div className="grid gap-2">{transactions.length ? transactions.map((item) => <div key={item.id} className="flex flex-wrap items-center justify-between gap-2 rounded-xl bg-white/5 px-4 py-3 text-sm"><span>{item.type}{item.reason ? ` · ${item.reason}` : ""}</span><strong className={item.quantityChange >= 0 ? "text-emerald-300" : "text-rose-300"}>{item.quantityChange > 0 ? "+" : ""}{item.quantityChange}</strong><time className="text-xs text-slate-500">{new Date(item.createdAt).toLocaleString("vi-VN")}</time></div>) : <p className="text-sm text-slate-400">Chưa có giao dịch.</p>}</div></section>}
  </>;
}

