"use client";

import Image from "next/image";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Boxes, ClipboardList, FolderTree, House, LogOut, PackageSearch, PanelsTopLeft, Truck, Warehouse as WarehouseIcon } from "lucide-react";
import { useEffect } from "react";
import { useAuth } from "@/modules/auth/components/auth-provider";
import type { DashboardRole } from "@/modules/operations/types/operations.type";
import { Button } from "@/shared/components/ui/button";
import { cn } from "@/shared/utils/cn";

const menus = {
  Admin: [
    { href: "/admin", label: "Tổng quan", icon: PanelsTopLeft },
    { href: "/admin/categories", label: "Danh mục", icon: FolderTree },
    { href: "/admin/products", label: "Sản phẩm", icon: PackageSearch },
    { href: "/admin/warehouses", label: "Kho hàng", icon: WarehouseIcon },
    { href: "/admin/inventory", label: "Tồn kho", icon: Boxes },
    { href: "/admin/orders", label: "Đơn hàng", icon: ClipboardList },
    { href: "/admin/shipments", label: "Vận chuyển", icon: Truck },
  ],
  Manager: [
    { href: "/manager", label: "Tổng quan", icon: PanelsTopLeft },
    { href: "/manager/inventory", label: "Điều hành tồn kho", icon: Boxes },
    { href: "/manager/orders", label: "Đơn hàng", icon: ClipboardList },
    { href: "/manager/shipments", label: "Vận chuyển", icon: Truck },
  ],
  Staff: [
    { href: "/staff", label: "Tổng quan", icon: PanelsTopLeft },
    { href: "/staff/shipments", label: "Xử lý vận chuyển", icon: Truck },
  ],
} satisfies Record<DashboardRole, { href: string; label: string; icon: typeof House }[]>;

const roleLabels: Record<DashboardRole, string> = { Admin: "Quản trị hệ thống", Manager: "Quản lý vận hành", Staff: "Nhân viên xử lý" };

export function DashboardShell({ role, children }: Readonly<{ role: DashboardRole; children: React.ReactNode }>) {
  const pathname = usePathname();
  const router = useRouter();
  const { session, isReady, logout } = useAuth();

  useEffect(() => {
    if (!isReady) return;
    if (!session) router.replace("/login");
    else if (session.role !== role) router.replace(session.role === "User" ? "/" : `/${session.role?.toLowerCase()}`);
  }, [isReady, role, router, session]);

  if (!isReady || !session || session.role !== role) return <div className="grid min-h-screen place-items-center bg-slate-50 text-sm font-semibold text-slate-500">Đang kiểm tra quyền truy cập...</div>;

  return <div className="min-h-screen bg-[#f5f7fa] text-slate-900 lg:grid lg:grid-cols-[264px_minmax(0,1fr)]">
    <aside className="border-b border-white/10 bg-[#07111f] text-slate-100 lg:sticky lg:top-0 lg:h-screen lg:border-b-0 lg:border-r">
      <div className="flex h-full flex-col px-4 py-5">
        <Link href={`/${role.toLowerCase()}`} className="flex items-center gap-3 px-2"><Image src="/brand/AnhEcommerc.png" alt="MiniStore" width={44} height={44} className="size-11 object-contain" /><div><strong className="block tracking-[.06em]">MINI<span className="text-cyan-300">STORE</span></strong><span className="text-[11px] text-slate-400">{roleLabels[role]}</span></div></Link>
        <div className="my-5 h-px bg-white/10" />
        <nav className="flex gap-2 overflow-x-auto pb-1 lg:flex-1 lg:flex-col lg:overflow-visible" aria-label={`Điều hướng ${role}`}>
          {menus[role].map((item) => { const Icon = item.icon; const active = pathname === item.href || (item.href !== `/${role.toLowerCase()}` && pathname.startsWith(`${item.href}/`)); return <Link key={item.href} href={item.href} className={cn("flex min-h-11 shrink-0 items-center gap-3 rounded-xl px-3 text-sm font-semibold text-slate-400 transition-colors hover:bg-white/8 hover:text-white", active && "bg-cyan-700 text-white shadow-[inset_0_0_0_1px_rgba(103,232,249,.18)] hover:bg-cyan-600 hover:text-white")}><Icon className="size-[18px]" /><span>{item.label}</span></Link>; })}
        </nav>
        <div className="mt-5 hidden rounded-xl border border-white/10 bg-white/5 p-3 lg:block"><p className="truncate text-xs font-semibold text-slate-200">{session.email}</p><p className="mt-1 text-[11px] text-slate-500">{roleLabels[role]}</p></div>
        <div className="mt-3"><Button variant="ghost" className="w-full justify-start text-slate-300 hover:bg-white/10 hover:text-white" onClick={() => { logout(); router.push("/login"); }}><LogOut className="size-4" />Đăng xuất</Button></div>
      </div>
    </aside>
    <main className="min-w-0 p-5 sm:p-7 lg:p-9">{children}</main>
  </div>;
}

export function DashboardPageHeader({ eyebrow, title, description, action }: Readonly<{ eyebrow: string; title: string; description: string; action?: React.ReactNode }>) {
  return <header className="mb-7 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between"><div><p className="mb-2 text-[11px] font-black tracking-[.14em] text-cyan-600">{eyebrow}</p><h1 className="text-3xl font-black tracking-[-.04em] text-slate-950">{title}</h1><p className="mt-2 max-w-2xl text-sm leading-6 text-slate-500">{description}</p></div>{action}</header>;
}

