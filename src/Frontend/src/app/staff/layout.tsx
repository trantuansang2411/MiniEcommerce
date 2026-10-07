import { DashboardShell } from "@/modules/operations/components/dashboard-shell";
export default function StaffLayout({ children }: Readonly<{ children: React.ReactNode }>) { return <DashboardShell role="Staff">{children}</DashboardShell>; }
