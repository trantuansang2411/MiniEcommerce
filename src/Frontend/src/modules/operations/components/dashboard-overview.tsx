"use client";

import Link from "next/link";
import { ArrowRight, Boxes, ClipboardList, FolderTree, PackageSearch, Truck, Warehouse } from "lucide-react";
import { useEffect, useState } from "react";
import { managementService } from "@/modules/operations/api/management.service";
import { DashboardPageHeader } from "@/modules/operations/components/dashboard-shell";
import type { DashboardRole } from "@/modules/operations/types/operations.type";

type Metric = { label: string; value: number; helper: string; href: string; icon: typeof Boxes };

export function DashboardOverview({ role }: Readonly<{ role: DashboardRole }>) {
  const [metrics, setMetrics] = useState<Metric[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        if (role === "Admin") {
          const [categories, products, warehouses, orders, shipments] = await Promise.all([managementService.getCategories(), managementService.getProducts(), managementService.getWarehouses(), managementService.getOrders(), managementService.getShipments()]);
          setMetrics([
            { label: "Danh mục", value: categories.length, helper: "Cấu trúc cửa hàng", href: "/admin/categories", icon: FolderTree },
            { label: "Sản phẩm", value: products.length, helper: "Toàn bộ catalog", href: "/admin/products", icon: PackageSearch },
            { label: "Kho hàng", value: warehouses.length, helper: "Điểm lưu trữ", href: "/admin/warehouses", icon: Warehouse },
            { label: "Đơn hàng", value: orders.length, helper: "Tất cả trạng thái", href: "/admin/orders", icon: ClipboardList },
            { label: "Shipment", value: shipments.length, helper: "Đang được theo dõi", href: "/admin/shipments", icon: Truck },
          ]);
        } else if (role === "Manager") {
          const [inventories, orders, shipments] = await Promise.all([managementService.getInventories(), managementService.getOrders(), managementService.getShipments()]);
          setMetrics([
            { label: "Mặt hàng tồn", value: inventories.length, helper: "Theo kho và sản phẩm", href: "/manager/inventory", icon: Boxes },
            { label: "Đơn hàng", value: orders.length, helper: "Theo dõi toàn hệ thống", href: "/manager/orders", icon: ClipboardList },
            { label: "Shipment", value: shipments.length, helper: "Tiến độ giao nhận", href: "/manager/shipments", icon: Truck },
          ]);
        } else {
          const shipments = await managementService.getShipments();
          setMetrics([{ label: "Shipment cần xử lý", value: shipments.filter((item) => !["Delivered", "Cancelled"].includes(item.status)).length, helper: "Cập nhật theo đúng quy trình", href: "/staff/shipments", icon: Truck }]);
        }
      } finally { setLoading(false); }
    };
    void load();
  }, [role]);

  return <><DashboardPageHeader eyebrow="MINISTORE OPERATIONS" title={`Chào ${role}`} description={role === "Admin" ? "Điều hành catalog, kho và toàn bộ luồng đơn hàng từ một nơi." : role === "Manager" ? "Nắm nhanh tồn kho, đơn hàng và tiến độ vận chuyển cần theo dõi." : "Tập trung vào các shipment cần được xử lý và cập nhật trong ca làm việc."} />
    <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">{loading ? [1, 2, 3].map((item) => <div key={item} className="h-40 animate-pulse rounded-2xl bg-white" />) : metrics.map((metric) => { const Icon = metric.icon; return <Link key={metric.label} href={metric.href} className="group rounded-2xl bg-white p-5 shadow-[0_0_0_1px_rgba(15,23,42,.05),0_8px_28px_rgba(15,23,42,.04)] transition-transform hover:-translate-y-0.5"><div className="flex items-start justify-between"><span className="grid size-10 place-items-center rounded-xl bg-cyan-50 text-cyan-700"><Icon className="size-5" /></span><ArrowRight className="size-4 text-slate-300 transition-transform group-hover:translate-x-1 group-hover:text-cyan-600" /></div><strong className="mt-6 block text-3xl font-black tabular-nums text-slate-950">{metric.value}</strong><span className="mt-1 block text-sm font-bold text-slate-700">{metric.label}</span><span className="mt-1 block text-xs text-slate-400">{metric.helper}</span></Link>; })}</section>
  </>;
}

