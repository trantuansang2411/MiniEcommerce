"use client";

import { Search, icons } from "lucide-react";
import { useMemo, useState } from "react";
import { CategoryIcon } from "@/modules/catalog/components/category-icon";
import { fieldClass } from "@/modules/operations/components/management-ui";

type CategoryIconPickerProps = {
  initialIconKey?: string;
};

function toIconKey(name: string) {
  return name
    .replace(/([a-z0-9])([A-Z])/g, "$1-$2")
    .replace(/([A-Z])([A-Z][a-z])/g, "$1-$2")
    .toLowerCase();
}

const categoryIconOptions = Object.entries(icons).map(([name, Icon]) => ({
  name,
  key: toIconKey(name),
  Icon,
}));

export function CategoryIconPicker({ initialIconKey }: Readonly<CategoryIconPickerProps>) {
  const [query, setQuery] = useState("");
  const [selectedIconKey, setSelectedIconKey] = useState(() => {
    const candidate = (initialIconKey || "boxes").trim().toLowerCase();
    return categoryIconOptions.some((item) => item.key === candidate) ? candidate : "boxes";
  });
  const matchingIcons = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();
    if (!normalizedQuery) return categoryIconOptions;
    return categoryIconOptions.filter((item) => item.name.toLowerCase().includes(normalizedQuery));
  }, [query]);
  const selectedIcon = categoryIconOptions.find((item) => item.key === selectedIconKey);

  return (
    <section className="grid gap-3" aria-labelledby="category-icon-picker-label">
      <input type="hidden" name="iconKey" value={selectedIconKey} />
      <div className="flex items-center justify-between gap-3">
        <div>
          <p id="category-icon-picker-label" className="text-sm font-bold text-slate-700">Icon danh mục</p>
          <p className="mt-1 text-xs text-slate-400">Chọn trực tiếp từ toàn bộ thư viện Lucide.</p>
        </div>
        <span className="inline-flex items-center gap-2 rounded-lg bg-cyan-50 px-3 py-2 text-xs font-bold text-cyan-800">
          <CategoryIcon iconKey={selectedIconKey} className="size-4" />
          {selectedIcon?.name ?? "Boxes"}
        </span>
      </div>
      <label className="relative block">
        <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
        <input
          className={`${fieldClass} pl-10 font-normal`}
          value={query}
          placeholder="Tìm icon, ví dụ: laptop, phone, camera..."
          onChange={(event) => setQuery(event.target.value)}
        />
      </label>
      <div className="grid max-h-60 grid-cols-6 gap-2 overflow-y-auto rounded-xl border border-slate-200 bg-slate-50 p-3 sm:grid-cols-8" aria-label="Danh sách icon Lucide">
        {matchingIcons.map(({ name, key, Icon }) => {
          const selected = key === selectedIconKey;
          return (
            <button
              key={name}
              type="button"
              title={name}
              aria-label={`Chọn icon ${name}`}
              aria-pressed={selected}
              className={`grid size-10 place-items-center rounded-lg transition-[background-color,color,box-shadow,transform] hover:-translate-y-px ${selected ? "bg-cyan-500 text-slate-950 shadow-[0_4px_12px_rgba(6,182,212,.25)]" : "bg-white text-slate-600 shadow-[0_0_0_1px_rgba(15,23,42,.06)] hover:bg-cyan-50 hover:text-cyan-700"}`}
              onClick={() => setSelectedIconKey(key)}
            >
              <Icon className="size-5" aria-hidden="true" />
            </button>
          );
        })}
      </div>
      <p className="text-xs text-slate-400">{matchingIcons.length.toLocaleString("vi-VN")} icon khả dụng.</p>
    </section>
  );
}
