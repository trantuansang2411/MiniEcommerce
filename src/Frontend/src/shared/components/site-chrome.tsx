"use client";

import { usePathname, useRouter } from "next/navigation";
import { useEffect } from "react";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { SiteHeader } from "@/shared/components/site-header";

export function SiteChrome({ children }: Readonly<{ children: React.ReactNode }>) {
  const pathname = usePathname();
  const router = useRouter();
  const { session, isReady } = useAuth();
  const isDashboard = /^\/(admin|manager|staff)(\/|$)/.test(pathname);
  const dashboardPath = session?.role === "Admin"
    ? "/admin"
    : session?.role === "Manager"
      ? "/manager"
      : session?.role === "Staff"
        ? "/staff"
        : null;

  useEffect(() => {
    if (isReady && dashboardPath && !isDashboard) {
      router.replace(dashboardPath);
    }
  }, [dashboardPath, isDashboard, isReady, router]);

  if (!isReady || (dashboardPath && !isDashboard)) {
    return <div className="grid min-h-screen place-items-center bg-slate-50 text-sm font-semibold text-slate-500">Đang chuyển đến khu vực làm việc...</div>;
  }

  if (isDashboard) return children;

  return <><SiteHeader /><main className="page-container">{children}</main></>;
}

