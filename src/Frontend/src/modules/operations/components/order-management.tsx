"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import type { Order } from "@/modules/orders/types/order.type";
import { managementService } from "@/modules/operations/api/management.service";
import { DashboardPageHeader } from "@/modules/operations/components/dashboard-shell";
import { DataState, StatusBadge, fieldClass, shortId, tableClass, tableWrapClass, tdClass, thClass } from "@/modules/operations/components/management-ui";
import type { DashboardRole } from "@/modules/operations/types/operations.type";
import { Button } from "@/shared/components/ui/button";
import { formatCurrency } from "@/shared/utils/format";
import { orderStatusOptions } from "@/shared/utils/status";

const statuses = orderStatusOptions;

export function OrderManagement({ role }: Readonly<{ role: Extract<DashboardRole, "Admin" | "Manager"> }>) {
  const [items, setItems] = useState<Order[]>([]); const [status, setStatus] = useState(""); const [loading, setLoading] = useState(true); const [error, setError] = useState<string | null>(null);
  const load = useCallback(async () => { setLoading(true); setError(null); try { setItems(await managementService.getOrders(status ? Number(status) : undefined)); } catch (e) { setError(e instanceof Error ? e.message : "Không tải được đơn hàng."); } finally { setLoading(false); } }, [status]);
  useEffect(() => { void load(); }, [load]);
  return <><DashboardPageHeader eyebrow={`${role.toUpperCase()} / ORDERS`} title="Đơn hàng" description="Theo dõi tổng tiền, trạng thái thanh toán và số mặt hàng trong từng đơn." /><div className="mb-5 max-w-xs"><select className={fieldClass} value={status} onChange={(event) => setStatus(event.target.value)}>{statuses.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}</select></div><DataState loading={loading} error={error} empty={!items.length}><div className={tableWrapClass}><table className={tableClass}><thead><tr><th className={thClass}>Mã đơn</th><th className={thClass}>Thời gian</th><th className={thClass}>Số dòng</th><th className={thClass}>Tổng tiền</th><th className={thClass}>Trạng thái</th><th className={thClass}>Thao tác</th></tr></thead><tbody>{items.map((item) => <tr key={item.id}><td className={`${tdClass} font-mono font-bold text-slate-900`} title={item.id}>{shortId(item.id)}</td><td className={tdClass}>{new Date(item.createdAt).toLocaleString("vi-VN")}</td><td className={`${tdClass} tabular-nums`}>{item.items.length}</td><td className={`${tdClass} font-black tabular-nums text-slate-950`}>{formatCurrency(item.totalAmount)}</td><td className={tdClass}><StatusBadge value={item.status} /></td><td className={tdClass}><Button asChild size="sm" variant="outline"><Link href={`/${role.toLowerCase()}/orders/${item.id}`}>Chi tiết</Link></Button></td></tr>)}</tbody></table></div></DataState></>;
}
