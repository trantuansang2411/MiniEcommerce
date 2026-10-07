import { DashboardShell } from "@/modules/operations/components/dashboard-shell";
export default function ManagerLayout({ children }: Readonly<{ children: React.ReactNode }>) { return <DashboardShell role="Manager">{children}</DashboardShell>; }
