"use client";

import { Pencil, Plus, Power, Trash2, X } from "lucide-react";
import dynamic from "next/dynamic";
import { useCallback, useEffect, useRef, useState } from "react";
import type { Category } from "@/modules/catalog/types/catalog.type";
import { managementService } from "@/modules/operations/api/management.service";
import { DashboardPageHeader } from "@/modules/operations/components/dashboard-shell";
import {
  DataState,
  StatusBadge,
  fieldClass,
  tableClass,
  tableWrapClass,
  tdClass,
  thClass,
} from "@/modules/operations/components/management-ui";
import { Button } from "@/shared/components/ui/button";
import { useToast } from "@/shared/components/ui/toast";

const CategoryIconPicker = dynamic(
  () => import("@/modules/operations/components/category-icon-picker").then((module) => module.CategoryIconPicker),
  { ssr: false, loading: () => <div className="h-72 animate-pulse rounded-xl bg-slate-100" /> },
);

type CategoryModalProps = {
  category?: Category;
  busy: boolean;
  onClose: () => void;
  onSave: (event: React.FormEvent<HTMLFormElement>) => void;
};

function CategoryModal({ category, busy, onClose, onSave }: Readonly<CategoryModalProps>) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const isEditing = Boolean(category);

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
      aria-labelledby="category-modal-title"
      className="m-auto w-[min(520px,calc(100%-32px))] rounded-2xl bg-white p-0 text-slate-900 shadow-2xl backdrop:bg-slate-950/55 backdrop:backdrop-blur-[2px]"
      onCancel={onClose}
      onClick={(event) => {
        if (event.target === event.currentTarget) onClose();
      }}
    >
      <form onSubmit={onSave} className="p-6 sm:p-7">
        <header className="flex items-start justify-between gap-4">
          <div>
            <p className="text-[11px] font-black tracking-[.13em] text-cyan-700">ADMIN / CATALOG</p>
            <h2 id="category-modal-title" className="mt-2 text-2xl font-black tracking-[-.035em]">
              {isEditing ? "Sửa danh mục" : "Thêm danh mục"}
            </h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              {isEditing
                ? "Cập nhật thông tin hiển thị của danh mục trên cửa hàng."
                : "Tạo một nhóm sản phẩm mới cho catalog MiniStore."}
            </p>
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

        <div className="mt-6 grid gap-4">
          <label className="grid gap-2 text-sm font-bold text-slate-700">
            Tên danh mục
            <input
              autoFocus
              className={fieldClass}
              name="name"
              defaultValue={category?.name ?? ""}
              placeholder="Ví dụ: Laptop"
              required
            />
          </label>
          <label className="grid gap-2 text-sm font-bold text-slate-700">
            Mô tả
            <textarea
              className={`${fieldClass} min-h-24 resize-y py-3`}
              name="description"
              defaultValue={category?.description ?? ""}
              placeholder="Mô tả ngắn về danh mục"
              maxLength={500}
            />
          </label>
          <CategoryIconPicker initialIconKey={category?.iconKey} />
          {category && (
            <label className="flex items-center gap-3 rounded-xl bg-slate-50 px-4 py-3 text-sm font-bold text-slate-700">
              <input
                type="checkbox"
                name="isActive"
                defaultChecked={category.isActive}
                className="size-4 accent-cyan-700"
              />
              Hiển thị danh mục trên cửa hàng
            </label>
          )}
        </div>

        <footer className="mt-7 flex justify-end gap-3 border-t border-slate-100 pt-5">
          <Button
            type="button"
            variant="secondary"
            className="bg-slate-100 text-slate-700 hover:bg-slate-200 hover:text-slate-900"
            onClick={onClose}
          >
            Hủy
          </Button>
          <Button type="submit" disabled={busy}>
            {busy ? "Đang lưu..." : isEditing ? "Lưu thay đổi" : "Tạo danh mục"}
          </Button>
        </footer>
      </form>
    </dialog>
  );
}

export function CategoryManagement() {
  const toast = useToast();
  const [items, setItems] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editingCategory, setEditingCategory] = useState<Category | null | undefined>(undefined);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setItems(await managementService.getCategories());
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Không tải được danh mục.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  async function save(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const payload = {
      name: String(data.get("name") ?? "").trim(),
      description: String(data.get("description") ?? "").trim(),
      iconKey: String(data.get("iconKey") ?? "generic").trim(),
    };

    setBusy(true);
    setError(null);
    try {
      if (editingCategory) {
        await managementService.updateCategory(editingCategory.id, {
          ...payload,
          isActive: data.get("isActive") === "on",
        });
      } else {
        await managementService.createCategory(payload);
      }

      setEditingCategory(undefined);
      await load();
      toast.success(editingCategory ? "Đã cập nhật danh mục." : "Đã tạo danh mục.");
    } catch (requestError) {
      const message = requestError instanceof Error ? requestError.message : "Không thể lưu danh mục.";
      setError(message);
      toast.error(message);
    } finally {
      setBusy(false);
    }
  }

  async function toggle(item: Category) {
    try {
      await managementService.updateCategory(item.id, { isActive: !item.isActive });
      await load();
      toast.success(item.isActive ? "Đã ẩn danh mục." : "Đã hiển thị danh mục.");
    } catch (requestError) {
      const message = requestError instanceof Error ? requestError.message : "Không thể đổi trạng thái danh mục.";
      setError(message);
      toast.error(message);
    }
  }

  async function remove(id: string) {
    if (!window.confirm("Xóa danh mục này?")) return;
    try {
      await managementService.deleteCategory(id);
      await load();
      toast.success("Đã xóa danh mục.");
    } catch (requestError) {
      const message = requestError instanceof Error ? requestError.message : "Không thể xóa danh mục.";
      setError(message);
      toast.error(message);
    }
  }

  return (
    <>
      <DashboardPageHeader
        eyebrow="ADMIN / CATALOG"
        title="Danh mục sản phẩm"
        description="Tạo cấu trúc danh mục và kiểm soát nội dung hiển thị tại cửa hàng."
        action={(
          <Button onClick={() => setEditingCategory(null)}>
            <Plus className="size-4" />
            Thêm danh mục
          </Button>
        )}
      />

      <DataState loading={loading} error={error} empty={!items.length}>
        <div className={tableWrapClass}>
          <table className={tableClass}>
            <thead>
              <tr>
                <th className={thClass}>Danh mục</th>
                <th className={thClass}>Mô tả</th>
                <th className={thClass}>Icon</th>
                <th className={thClass}>Trạng thái</th>
                <th className={thClass}>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td className={`${tdClass} font-bold text-slate-900`}>{item.name}</td>
                  <td className={tdClass}>{item.description || "—"}</td>
                  <td className={tdClass}>{item.iconKey}</td>
                  <td className={tdClass}>
                    <StatusBadge value={item.isActive ? "Active" : "Inactive"} />
                  </td>
                  <td className={tdClass}>
                    <div className="flex gap-2">
                      <Button size="sm" variant="outline" onClick={() => void toggle(item)}>
                        <Power className="size-4" />
                        {item.isActive ? "Ẩn" : "Bật"}
                      </Button>
                      <Button
                        size="icon"
                        variant="ghost"
                        aria-label={`Sửa danh mục ${item.name}`}
                        onClick={() => setEditingCategory(item)}
                      >
                        <Pencil className="size-4 text-cyan-700" />
                      </Button>
                      <Button
                        size="icon"
                        variant="ghost"
                        aria-label={`Xóa danh mục ${item.name}`}
                        onClick={() => void remove(item.id)}
                      >
                        <Trash2 className="size-4 text-rose-600" />
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </DataState>

      {editingCategory !== undefined && (
        <CategoryModal
          key={editingCategory?.id ?? "new-category"}
          category={editingCategory ?? undefined}
          busy={busy}
          onClose={() => setEditingCategory(undefined)}
          onSave={(event) => void save(event)}
        />
      )}
    </>
  );
}
