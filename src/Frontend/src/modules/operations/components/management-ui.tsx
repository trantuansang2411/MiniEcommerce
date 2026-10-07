import { AlertCircle, Inbox } from "lucide-react";
import { formatStatus } from "@/shared/utils/status";

export function StatusBadge({ value }: Readonly<{ value: string }>) {
  const positive = ["Active", "Paid", "Completed", "Delivered"].includes(value);
  const warning = ["AwaitingPayment", "Pending", "Picking", "Packed", "Shipped", "InTransit", "Draft"].includes(value);
  return <span className={`inline-flex rounded-full px-2.5 py-1 text-[11px] font-bold ${positive ? "bg-emerald-50 text-emerald-700" : warning ? "bg-amber-50 text-amber-700" : "bg-slate-100 text-slate-600"}`}>{formatStatus(value)}</span>;
}

export function DataState({ loading, error, empty, children }: Readonly<{ loading: boolean; error?: string | null; empty: boolean; children: React.ReactNode }>) {
  if (loading) return <div className="rounded-2xl bg-white p-10 text-center text-sm font-semibold text-slate-400 shadow-[0_0_0_1px_rgba(15,23,42,.05)]">Đang tải dữ liệu...</div>;
  if (error) return <div className="flex items-center gap-3 rounded-2xl bg-rose-50 p-5 text-sm font-semibold text-rose-700"><AlertCircle className="size-5" />{error}</div>;
  if (empty) return <div className="grid place-items-center rounded-2xl bg-white p-12 text-center shadow-[0_0_0_1px_rgba(15,23,42,.05)]"><Inbox className="mb-3 size-8 text-slate-300" /><p className="text-sm font-semibold text-slate-500">Chưa có dữ liệu phù hợp.</p></div>;
  return children;
}

export function shortId(id: string) { return `${id.slice(0, 8)}…`; }

export const fieldClass = "h-10 w-full rounded-lg border border-slate-200 bg-slate-50 px-3 text-sm font-normal text-slate-800 outline-none transition focus:border-cyan-500 focus:bg-white focus:ring-2 focus:ring-cyan-500/10";
export const tableWrapClass = "overflow-x-auto rounded-2xl bg-white shadow-[0_0_0_1px_rgba(15,23,42,.05),0_8px_28px_rgba(15,23,42,.04)]";
export const tableClass = "w-full min-w-[720px] border-collapse text-left text-sm";
export const thClass = "border-b border-slate-100 bg-slate-50/70 px-5 py-3 text-[11px] font-black uppercase tracking-[.08em] text-slate-500";
export const tdClass = "border-b border-slate-100 px-5 py-4 align-middle text-slate-600 last:border-b-0";

