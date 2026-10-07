"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { ClipboardList, Mail, UserRound } from "lucide-react";
import { useEffect } from "react";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { ShippingProfileManager } from "@/modules/shipping/components/shipping-profile-manager";
import { Button } from "@/shared/components/ui/button";

export default function AccountPage() {
  const router = useRouter();
  const { session, isReady } = useAuth();

  useEffect(() => {
    if (isReady && !session) router.replace("/login");
  }, [isReady, router, session]);

  if (!isReady || !session) {
    return <div className="grid min-h-[42vh] place-items-center text-sm font-medium text-slate-500">Đang tải tài khoản...</div>;
  }

  return (
    <section className="mx-auto max-w-5xl py-10 sm:py-14">
      <header className="mb-7"><p className="eyebrow">TÀI KHOẢN</p><h1 className="mt-2 text-3xl font-black tracking-[-.035em] text-slate-950">Thông tin tài khoản</h1><p className="mt-2 text-slate-500">Quản lý thông tin và theo dõi các đơn mua của bạn.</p></header>
      <div className="rounded-2xl bg-white p-5 shadow-[0_0_0_1px_rgba(15,23,42,.06),0_12px_32px_rgba(15,23,42,.06)] sm:p-7">
        <div className="flex items-center gap-4"><span className="grid size-14 place-items-center rounded-2xl bg-cyan-50 text-cyan-700"><UserRound className="size-7" /></span><div className="min-w-0"><p className="text-xs font-black tracking-[.12em] text-cyan-700">KHÁCH HÀNG MINISTORE</p><h2 className="mt-1 truncate text-lg font-bold text-slate-950">{session.email}</h2></div></div>
        <div className="my-6 border-t border-slate-100" />
        <div className="flex items-center gap-3 text-sm text-slate-600"><Mail className="size-4 text-slate-400" /><span className="min-w-0 truncate">{session.email}</span></div>
        <Button asChild className="mt-7"><Link href="/orders"><ClipboardList className="size-4" />Xem đơn của tôi</Link></Button>
      </div>
      <ShippingProfileManager enabled={Boolean(session)} />
    </section>
  );
}
