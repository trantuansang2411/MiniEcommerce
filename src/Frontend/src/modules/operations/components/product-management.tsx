"use client";

import Image from "next/image";
import { ImagePlus, Images, Pencil, Plus, Trash2 } from "lucide-react";
import { useCallback, useEffect, useId, useMemo, useState } from "react";
import type { Category, Product } from "@/modules/catalog/types/catalog.type";
import { managementService } from "@/modules/operations/api/management.service";
import { DashboardPageHeader } from "@/modules/operations/components/dashboard-shell";
import { ManagementModal } from "@/modules/operations/components/management-modal";
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
import { formatCurrency, productImageUrl } from "@/shared/utils/format";

type ProductModalProps = {
  product?: Product;
  categories: Category[];
  busy: boolean;
  onClose: () => void;
  onSave: (event: React.FormEvent<HTMLFormElement>) => void;
};

type FilePickerProps = {
  name: string;
  label: string;
  hint?: string;
  required?: boolean;
  multiple?: boolean;
  existingImageUrls?: string[];
  removedImageUrls?: string[];
  onToggleExistingImage?: (imageUrl: string) => void;
};

function formatVietnameseCurrency(value: string) {
  const digits = value.replace(/\D/g, "").replace(/^0+(?=\d)/, "");
  return digits ? digits.replace(/\B(?=(\d{3})+(?!\d))/g, ".") : "";
}

function CurrencyInput({ name, defaultValue }: Readonly<{ name: string; defaultValue?: number }>) {
  const [amount, setAmount] = useState(defaultValue ? String(defaultValue) : "");

  return (
    <>
      <input name={name} type="hidden" value={amount} />
      <div className="relative">
        <input
          className={`${fieldClass} pr-10 font-normal tabular-nums`}
          inputMode="numeric"
          type="text"
          value={formatVietnameseCurrency(amount)}
          placeholder="0"
          required
          onChange={(event) => setAmount(event.target.value.replace(/\D/g, ""))}
        />
        <span aria-hidden="true" className="pointer-events-none absolute inset-y-0 right-4 flex items-center text-sm font-medium text-slate-500">đ</span>
      </div>
    </>
  );
}

function FilePicker({
  name,
  label,
  hint,
  required = false,
  multiple = false,
  existingImageUrls = [],
  removedImageUrls = [],
  onToggleExistingImage,
}: Readonly<FilePickerProps>) {
  const inputId = useId();
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const newImagePreviews = useMemo(
    () => selectedFiles.map((file) => ({ name: file.name, url: URL.createObjectURL(file) })),
    [selectedFiles],
  );
  useEffect(() => () => newImagePreviews.forEach((image) => URL.revokeObjectURL(image.url)), [newImagePreviews]);

  const Icon = multiple ? Images : ImagePlus;
  const summary = selectedFiles.length === 0
    ? "Chưa chọn ảnh"
    : multiple && selectedFiles.length > 1
      ? `${selectedFiles.length} ảnh đã chọn`
      : selectedFiles[0].name;
  const hasPreview = existingImageUrls.length > 0 || newImagePreviews.length > 0;

  return (
    <div className="grid gap-2 text-sm text-slate-700">
      <div className="flex items-baseline justify-between gap-3">
        <label htmlFor={inputId} className="font-medium">{label}</label>
        {hint && <span className="text-xs text-slate-400">{hint}</span>}
      </div>
      <input
        id={inputId}
        className="sr-only"
        name={name}
        type="file"
        accept="image/*"
        multiple={multiple}
        required={required}
        onChange={(event) => setSelectedFiles(Array.from(event.target.files ?? []))}
      />
      <label
        htmlFor={inputId}
        className="group flex h-12 cursor-pointer items-center gap-3 rounded-lg border border-dashed border-slate-300 bg-slate-50 px-3 text-slate-500 transition-[border-color,background-color,color] hover:border-cyan-400 hover:bg-cyan-50/60 hover:text-slate-700"
      >
        <span className="grid size-8 shrink-0 place-items-center rounded-md bg-white text-cyan-700 shadow-[0_0_0_1px_rgba(15,23,42,.06)] group-hover:bg-cyan-100">
          <Icon className="size-4" />
        </span>
        <span className="min-w-0 flex-1 truncate text-sm font-normal">{summary}</span>
        <span className="rounded-md bg-white px-2.5 py-1 text-xs font-medium text-slate-700 shadow-[0_0_0_1px_rgba(15,23,42,.06)]">Chọn ảnh</span>
      </label>
      {hasPreview && (
        <div className="rounded-xl border border-slate-200 bg-slate-50/70 p-2.5">
          <p className="mb-2 text-[11px] font-semibold uppercase tracking-[.08em] text-slate-400">Xem trước</p>
          <div className="flex flex-wrap gap-2.5">
            {existingImageUrls.map((imageUrl) => {
              const markedForRemoval = removedImageUrls.includes(imageUrl);
              const preview = (
                <>
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img src={productImageUrl(imageUrl)} alt="Ảnh hiện tại" className="size-full object-cover" />
                  <span className={`absolute inset-x-1 bottom-1 rounded px-1.5 py-0.5 text-center text-[10px] font-semibold ${markedForRemoval ? "bg-rose-600 text-white" : "bg-slate-950/75 text-white"}`}>
                    {markedForRemoval ? "Đã xoá" : "Đang dùng"}
                  </span>
                </>
              );

              return onToggleExistingImage ? (
                <button
                  key={imageUrl}
                  type="button"
                  aria-pressed={markedForRemoval}
                  aria-label={markedForRemoval ? "Giữ lại ảnh" : "Đánh dấu xóa ảnh"}
                  className={`relative size-[72px] overflow-hidden rounded-lg outline-none ring-offset-2 transition focus-visible:ring-2 focus-visible:ring-cyan-500 ${markedForRemoval ? "opacity-45 ring-2 ring-rose-400" : "ring-1 ring-slate-200 hover:ring-cyan-400"}`}
                  onClick={() => onToggleExistingImage(imageUrl)}
                >
                  {preview}
                </button>
              ) : (
                <div key={imageUrl} className="relative size-[72px] overflow-hidden rounded-lg ring-1 ring-slate-200">
                  {preview}
                </div>
              );
            })}
            {newImagePreviews.map((image) => (
              <div key={image.url} className="relative size-[72px] overflow-hidden rounded-lg ring-2 ring-cyan-300 ring-offset-1">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img src={image.url} alt={`Xem trước ${image.name}`} className="size-full object-cover" />
                <span className="absolute inset-x-1 bottom-1 rounded bg-cyan-600 px-1.5 py-0.5 text-center text-[10px] font-semibold text-white">Mới</span>
              </div>
            ))}
          </div>
          {onToggleExistingImage && existingImageUrls.length > 0 && <p className="mt-2 text-xs text-slate-500">Bấm ảnh đang dùng để đánh dấu xóa.</p>}
        </div>
      )}
      {removedImageUrls.map((imageUrl) => <input key={imageUrl} type="hidden" name="removeImageUrls" value={imageUrl} />)}
    </div>
  );
}

function ProductModal({ product, categories, busy, onClose, onSave }: Readonly<ProductModalProps>) {
  const [removedImages, setRemovedImages] = useState<string[]>([]);
  const isEditing = Boolean(product);

  function toggleImageRemoval(imageUrl: string) {
    setRemovedImages((current) => current.includes(imageUrl) ? current.filter((item) => item !== imageUrl) : [...current, imageUrl]);
  }

  return (
    <ManagementModal
      eyebrow="ADMIN / CATALOG"
      title={isEditing ? "Sửa sản phẩm" : "Thêm sản phẩm"}
      description={isEditing ? "Cập nhật thông tin bán hàng, hình ảnh và trạng thái hiển thị của sản phẩm." : "Tạo sản phẩm mới cùng ảnh đại diện và bộ sưu tập ảnh chi tiết."}
      onClose={onClose}
      size="wide"
    >
      <form onSubmit={onSave} className="mt-6 grid gap-4 sm:grid-cols-2">
        <label className="grid gap-2 text-sm font-medium text-slate-700">
          Danh mục
          <select className={fieldClass} name="categoryId" defaultValue={product?.categoryId ?? ""} required>
            <option value="" disabled>Chọn danh mục</option>
            {categories.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}
          </select>
        </label>
        <label className="grid gap-2 text-sm font-medium text-slate-700">
          Tên sản phẩm
          <input autoFocus className={fieldClass} name="name" defaultValue={product?.name ?? ""} placeholder="Ví dụ: MacBook Air M4" required />
        </label>
        <label className="grid gap-2 text-sm font-medium text-slate-700">
          Giá gốc
          <CurrencyInput name="originalPrice" defaultValue={product?.originalPrice} />
        </label>
        <label className="grid gap-2 text-sm font-medium text-slate-700">
          Giá bán
          <CurrencyInput name="sellingPrice" defaultValue={product?.sellingPrice} />
        </label>
        <label className="grid gap-2 text-sm font-medium text-slate-700 sm:col-span-2">
          Thông số kỹ thuật
          <textarea
            className={`${fieldClass} min-h-28 resize-y py-3`}
            name="specifications"
            defaultValue={product?.specifications ?? ""}
            placeholder="Mô tả, cấu hình hoặc thông số kỹ thuật của sản phẩm"
          />
        </label>
        <FilePicker name="thumbnail" label="Ảnh thumbnail" hint={isEditing ? "Để trống để giữ ảnh cũ" : "Bắt buộc"} required={!isEditing} existingImageUrls={product?.thumbnailUrl ? [product.thumbnailUrl] : []} />
        <FilePicker name="images" label="Ảnh chi tiết" hint="Có thể chọn nhiều ảnh" multiple existingImageUrls={product?.imageUrls ?? []} removedImageUrls={removedImages} onToggleExistingImage={toggleImageRemoval} />
        <label className="grid gap-2 text-sm font-medium text-slate-700">
          Trạng thái
          <select className={fieldClass} name="status" defaultValue={String(product?.status ?? 1)}>
            <option value="1">Bản nháp</option>
            <option value="2">Đang hoạt động</option>
            <option value="3">Tạm ngưng</option>
          </select>
        </label>

        <footer className="mt-3 flex justify-end gap-3 border-t border-slate-100 pt-5 sm:col-span-2">
          <Button type="button" variant="secondary" className="bg-slate-100 text-slate-700 hover:bg-slate-200 hover:text-slate-900" onClick={onClose}>
            Hủy
          </Button>
          <Button type="submit" disabled={busy}>{busy ? "Đang lưu..." : isEditing ? "Lưu thay đổi" : "Tạo sản phẩm"}</Button>
        </footer>
      </form>
    </ManagementModal>
  );
}

export function ProductManagement() {
  const toast = useToast();
  const [items, setItems] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingProduct, setEditingProduct] = useState<Product | null | undefined>(undefined);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [products, categoryItems] = await Promise.all([managementService.getProducts(), managementService.getCategories()]);
      setItems(products);
      setCategories(categoryItems);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Không tải được sản phẩm.");
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
    setBusy(true);
    setError(null);
    try {
      if (editingProduct) await managementService.updateProduct(editingProduct.id, data);
      else await managementService.createProduct(data);

      setEditingProduct(undefined);
      await load();
      toast.success(editingProduct ? "Đã cập nhật sản phẩm." : "Đã tạo sản phẩm.");
    } catch (requestError) {
      const message = requestError instanceof Error ? requestError.message : "Không thể lưu sản phẩm.";
      setError(message);
      toast.error(message);
    } finally {
      setBusy(false);
    }
  }

  async function remove(id: string) {
    if (!window.confirm("Xóa sản phẩm này?")) return;
    try {
      await managementService.deleteProduct(id);
      await load();
      toast.success("Đã xóa sản phẩm.");
    } catch (requestError) {
      const message = requestError instanceof Error ? requestError.message : "Không thể xóa sản phẩm.";
      setError(message);
      toast.error(message);
    }
  }

  return (
    <>
      <DashboardPageHeader
        eyebrow="ADMIN / CATALOG"
        title="Sản phẩm"
        description="Quản lý sản phẩm, giá bán, ảnh đại diện và bộ sưu tập ảnh."
        action={<Button onClick={() => setEditingProduct(null)}><Plus className="size-4" />Thêm sản phẩm</Button>}
      />

      <DataState loading={loading} error={error} empty={!items.length}>
        <div className={tableWrapClass}>
          <table className={tableClass}>
            <thead>
              <tr><th className={thClass}>Sản phẩm</th><th className={thClass}>Giá gốc</th><th className={thClass}>Giá bán</th><th className={thClass}>Trạng thái</th><th className={thClass}>Thao tác</th></tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td className={tdClass}>
                    <div className="flex items-center gap-3">
                      <Image src={item.thumbnailUrl || item.imageUrl ? productImageUrl(item.thumbnailUrl || item.imageUrl || "") : "/brand/AnhEcommerc.png"} alt="" width={44} height={44} className="size-11 rounded-lg object-cover" unoptimized />
                      <span className="font-bold text-slate-900">{item.name}</span>
                    </div>
                  </td>
                  <td className={tdClass}>{formatCurrency(item.originalPrice)}</td>
                  <td className={`${tdClass} font-bold text-slate-900`}>{formatCurrency(item.sellingPrice)}</td>
                  <td className={tdClass}><StatusBadge value={item.status === 1 ? "Draft" : item.status === 2 ? "Active" : "Inactive"} /></td>
                  <td className={tdClass}>
                    <div className="flex items-center gap-1">
                      <Button size="icon" variant="ghost" aria-label={`Sửa sản phẩm ${item.name}`} onClick={() => setEditingProduct(item)}>
                        <Pencil className="size-4 text-cyan-700" />
                      </Button>
                      <Button size="icon" variant="ghost" aria-label={`Xóa sản phẩm ${item.name}`} onClick={() => void remove(item.id)}>
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

      {editingProduct !== undefined && (
        <ProductModal
          key={editingProduct?.id ?? "new-product"}
          product={editingProduct ?? undefined}
          categories={categories}
          busy={busy}
          onClose={() => setEditingProduct(undefined)}
          onSave={(event) => void save(event)}
        />
      )}
    </>
  );
}
