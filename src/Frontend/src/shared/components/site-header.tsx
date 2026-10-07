"use client";

import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ChevronDown, ClipboardList, LogOut, Search, ShoppingCart, UserRound } from "lucide-react";
import { useState } from "react";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { ProductMenu } from "@/modules/catalog/components/product-menu";
import { useCart } from "@/modules/shopping/hooks/use-cart";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Popover, PopoverClose, PopoverContent, PopoverTrigger } from "@/shared/components/ui/popover";

export function SiteHeader() {
  const router = useRouter();
  const { session, isReady, logout } = useAuth();
  const cart = useCart(Boolean(session));
  const [search, setSearch] = useState("");
  const isAdmin = session?.role === "Admin";
  const canManageInventory = session?.role === "Admin" || session?.role === "Manager";
  const isStaff = session?.role === "Staff";

  function submitSearch(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const query = search.trim();
    router.push(query ? `/products?q=${encodeURIComponent(query)}` : "/products");
  }

  return <header className="sticky top-0 z-40 border-b border-cyan-200/10 bg-[#07111f]/95 text-slate-100 backdrop-blur-xl">
    <nav className="mx-auto flex min-h-[68px] w-[min(1440px,calc(100%-32px))] items-center gap-4 lg:grid lg:min-h-[88px] lg:grid-cols-[minmax(0,1fr)_minmax(0,720px)_minmax(0,1fr)] lg:gap-8">
      <Link className="flex shrink-0 items-center gap-2.5 text-lg font-black tracking-[.06em] text-white" href="/"><Image src="/brand/AnhEcommerc.png" alt="MiniStore" width={48} height={48} priority className="size-11 object-contain" /><span>MINI<span className="text-cyan-300">STORE</span></span></Link>
      <div className="hidden min-w-0 items-center gap-3 lg:flex">
        <form className="min-w-0 flex-1" onSubmit={submitSearch}>
          <div className="relative w-full"><Search className="pointer-events-none absolute left-4 top-1/2 size-5 -translate-y-1/2 text-slate-300" /><Input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Tìm thiết bị, phụ kiện..." aria-label="Tìm sản phẩm" className="h-12 border-cyan-100/20 bg-white/10 pl-12 text-base text-slate-100 placeholder:text-slate-400 focus-visible:border-cyan-300 focus-visible:ring-cyan-300/20" /><Button size="icon" type="submit" aria-label="Tìm kiếm" className="absolute right-0 top-0 size-12 rounded-l-none"><Search className="size-5" /></Button></div>
        </form>
        <ProductMenu />
        {isAdmin && <Link className="rounded-lg px-2.5 py-2 text-sm font-semibold text-slate-300 transition-colors hover:text-cyan-300" href="/admin/products">Quản trị</Link>}{canManageInventory && <Link className="rounded-lg px-2.5 py-2 text-sm font-semibold text-slate-300 transition-colors hover:text-cyan-300" href="/manager/inventory">Kho</Link>}{isStaff && <Link className="rounded-lg px-2.5 py-2 text-sm font-semibold text-slate-300 transition-colors hover:text-cyan-300" href="/staff/shipments">Giao hàng</Link>}
      </div>
      <div className="ml-auto flex shrink-0 items-center gap-2 lg:ml-0 lg:justify-self-end">
        {session && <Link href="/cart" aria-label="Giỏ hàng" className="relative grid size-10 place-items-center rounded-lg text-slate-100 transition-colors hover:bg-white/10"><ShoppingCart className="size-5" /><span className="absolute right-0.5 top-0.5 grid min-w-4 place-items-center rounded-full bg-cyan-300 px-1 text-[10px] font-black text-slate-950">{cart.data?.totalQuantity ?? 0}</span></Link>}
        {!isReady ? null : session ? <Popover><PopoverTrigger asChild><button type="button" aria-label="Mở menu tài khoản" className="flex max-w-[min(18rem,calc(100vw-6rem))] items-center gap-2 rounded-xl px-2 py-1.5 text-left text-slate-100 outline-none transition-colors hover:bg-white/10 focus-visible:ring-2 focus-visible:ring-cyan-300/70"><span className="grid size-8 shrink-0 place-items-center rounded-lg bg-cyan-300/15 text-cyan-300"><UserRound className="size-4" /></span><span className="hidden max-w-44 truncate text-sm font-semibold sm:block">{session.email}</span><ChevronDown className="hidden size-4 shrink-0 text-slate-400 sm:block" /></button></PopoverTrigger><PopoverContent align="end" sideOffset={12} className="w-56 p-2"><div className="grid gap-1"><PopoverClose asChild><Link href="/account" className="flex min-h-11 items-center gap-3 rounded-lg px-3 text-sm font-semibold text-slate-700 transition-colors hover:bg-cyan-50 hover:text-cyan-800"><UserRound className="size-4 text-cyan-700" />Tài khoản</Link></PopoverClose><PopoverClose asChild><Link href="/orders" className="flex min-h-11 items-center gap-3 rounded-lg px-3 text-sm font-semibold text-slate-700 transition-colors hover:bg-cyan-50 hover:text-cyan-800"><ClipboardList className="size-4 text-cyan-700" />Đơn của tôi</Link></PopoverClose><div className="my-1 border-t border-slate-100" /><button type="button" className="flex min-h-11 items-center gap-3 rounded-lg px-3 text-left text-sm font-semibold text-rose-600 transition-colors hover:bg-rose-50" onClick={() => { logout(); router.push("/"); }}><LogOut className="size-4" />Đăng xuất</button></div></PopoverContent></Popover> : <><Link className="hidden items-center gap-2 px-2 py-2 text-sm font-bold text-slate-100 transition-colors hover:text-cyan-300 sm:inline-flex" href="/login"><UserRound className="size-5 text-cyan-300" />Đăng nhập</Link><Button asChild size="sm"><Link href="/register">Đăng ký</Link></Button></>}
      </div>
    </nav>
    <div className="mx-auto flex w-[min(760px,calc(100%-20px))] items-center gap-3 pb-3 lg:hidden"><form className="relative min-w-0 flex-1" onSubmit={submitSearch}><Search className="pointer-events-none absolute left-4 top-1/2 size-5 -translate-y-1/2 text-slate-300" /><Input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Tìm thiết bị, phụ kiện..." aria-label="Tìm sản phẩm" className="h-12 border-cyan-100/20 bg-white/10 pl-12 text-base text-slate-100 placeholder:text-slate-400 focus-visible:border-cyan-300 focus-visible:ring-cyan-300/20" /><Button size="icon" type="submit" aria-label="Tìm kiếm" className="absolute right-0 top-0 size-12 rounded-l-none"><Search className="size-5" /></Button></form><ProductMenu /></div>
  </header>;
}
