"use client";

import { X } from "lucide-react";
import { type ReactNode, useEffect, useRef } from "react";

type ManagementModalProps = {
  eyebrow: string;
  title: string;
  description: string;
  children: ReactNode;
  onClose: () => void;
  size?: "default" | "wide";
};

export function ManagementModal({
  eyebrow,
  title,
  description,
  children,
  onClose,
  size = "default",
}: Readonly<ManagementModalProps>) {
  const dialogRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (dialog && !dialog.open) dialog.showModal();

    return () => {
      if (dialog?.open) dialog.close();
    };
  }, []);

  return (
    <dialog
      ref={dialogRef}
      aria-labelledby="management-modal-title"
      className={`m-auto ${size === "wide" ? "w-[min(760px,calc(100%-32px))]" : "w-[min(520px,calc(100%-32px))]"} rounded-2xl bg-white p-0 text-slate-900 shadow-2xl backdrop:bg-slate-950/55 backdrop:backdrop-blur-[2px]`}
      onCancel={onClose}
      onClick={(event) => {
        if (event.target === event.currentTarget) onClose();
      }}
    >
      <div className="p-6 sm:p-7">
        <header className="flex items-start justify-between gap-4">
          <div>
            <p className="text-[11px] font-black tracking-[.13em] text-cyan-700">{eyebrow}</p>
            <h2 id="management-modal-title" className="mt-2 text-2xl font-black tracking-[-.035em] text-slate-950">
              {title}
            </h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">{description}</p>
          </div>
          <button
            type="button"
            aria-label="Đóng"
            className="grid size-10 shrink-0 place-items-center rounded-xl text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-700"
            onClick={onClose}
          >
            <X className="size-5" />
          </button>
        </header>

        {children}
      </div>
    </dialog>
  );
}
