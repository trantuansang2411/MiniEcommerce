"use client";

import Link from "next/link";
import { ChevronRight, LayoutGrid } from "lucide-react";
import { CategoryIcon } from "@/modules/catalog/components/category-icon";
import { useCategories } from "@/modules/catalog/hooks/use-catalog";
import { Button } from "@/shared/components/ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "@/shared/components/ui/popover";

export function ProductMenu() {
  const { data: categories } = useCategories();

  return <Popover>
    <PopoverTrigger asChild>
      <Button variant="secondary" size="sm" className="h-12 shrink-0 rounded-xl px-4 text-[15px] text-slate-100 hover:bg-white/10 hover:text-cyan-300"><LayoutGrid className="size-5 text-cyan-300" />Sản phẩm</Button>
    </PopoverTrigger>
    <PopoverContent className="w-80 p-3">
      <p className="mb-2 px-2 text-[11px] font-black tracking-[.13em] text-slate-500">DANH MỤC SẢN PHẨM</p>
      {categories?.length ? <div className="grid grid-cols-2 gap-1">
        {categories.map((category) => <Link href={`/products?category=${category.id}`} key={category.id} className="group flex items-center gap-2 rounded-lg px-2.5 py-2.5 text-sm font-semibold text-slate-700 transition-colors hover:bg-cyan-50 hover:text-cyan-700"><CategoryIcon iconKey={category.iconKey} className="size-4 shrink-0 text-cyan-500" />{category.name}<ChevronRight className="ml-auto size-3.5 opacity-0 transition-opacity group-hover:opacity-100" /></Link>)}
      </div> : <div className="rounded-lg bg-slate-50 px-3 py-3 text-sm leading-5 text-slate-500">Danh mục sẽ hiển thị khi Admin tạo dữ liệu.</div>}
    </PopoverContent>
  </Popover>;
}
