import { DashboardShell } from "@/modules/operations/components/dashboard-shell";
export default function AdminLayout({ children }: Readonly<{ children: React.ReactNode }>) { return <DashboardShell role="Admin">{children}</DashboardShell>; }
