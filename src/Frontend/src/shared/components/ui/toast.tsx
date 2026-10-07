"use client";

import { CheckCircle2, CircleAlert, X } from "lucide-react";
import { createContext, type ReactNode, useCallback, useContext, useRef, useState } from "react";

type ToastVariant = "success" | "error";

type ToastItem = {
  id: number;
  message: string;
  variant: ToastVariant;
};

type ToastContextValue = {
  success: (message: string) => void;
  error: (message: string) => void;
};

const ToastContext = createContext<ToastContextValue | null>(null);

export function ToastProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const nextId = useRef(0);

  const dismiss = useCallback((id: number) => {
    setToasts((current) => current.filter((toast) => toast.id !== id));
  }, []);

  const show = useCallback((message: string, variant: ToastVariant) => {
    const id = nextId.current++;
    setToasts((current) => [...current.slice(-3), { id, message, variant }]);
    window.setTimeout(() => dismiss(id), 4500);
  }, [dismiss]);

  return (
    <ToastContext.Provider value={{ success: (message) => show(message, "success"), error: (message) => show(message, "error") }}>
      {children}
      <div aria-live="polite" className="pointer-events-none fixed inset-x-4 top-4 z-[100] flex flex-col items-end gap-3 sm:left-auto sm:right-5 sm:w-[min(390px,calc(100vw-40px))]">
        {toasts.map((toast) => {
          const isSuccess = toast.variant === "success";
          const Icon = isSuccess ? CheckCircle2 : CircleAlert;

          return (
            <div
              key={toast.id}
              role={isSuccess ? "status" : "alert"}
              className={`pointer-events-auto flex w-full items-start gap-3 rounded-2xl border bg-white px-4 py-3.5 shadow-xl shadow-slate-950/10 ${isSuccess ? "border-emerald-200" : "border-rose-200"}`}
            >
              <Icon className={`mt-0.5 size-5 shrink-0 ${isSuccess ? "text-emerald-600" : "text-rose-600"}`} />
              <p className="flex-1 text-sm font-medium leading-5 text-slate-700">{toast.message}</p>
              <button
                type="button"
                aria-label="Đóng thông báo"
                className="-mr-1 -mt-1 grid size-8 shrink-0 place-items-center rounded-lg text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-700"
                onClick={() => dismiss(toast.id)}
              >
                <X className="size-4" />
              </button>
            </div>
          );
        })}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const context = useContext(ToastContext);
  if (!context) throw new Error("useToast must be used within ToastProvider.");
  return context;
}
