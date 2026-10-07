"use client";

import Link from "next/link";
import { CategoryIcon } from "@/modules/catalog/components/category-icon";
import { useCategories } from "@/modules/catalog/hooks/use-catalog";

export function CategoryRail() {
  const { data: categories } = useCategories();

  if (!categories?.length) return null;

  return (
    <div className="category-rail">
      <div className="category-rail-inner">
        {categories.slice(0, 8).map((category) => (
          <Link key={category.id} href={`/?category=${category.id}`} className="category-nav-item">
            <span><CategoryIcon iconKey={category.iconKey} className="size-5" /></span>
            {category.name}
          </Link>
        ))}
      </div>
    </div>
  );
}
