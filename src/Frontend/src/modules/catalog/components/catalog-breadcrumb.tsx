import Link from "next/link";
import { ChevronRight } from "lucide-react";

type BreadcrumbItem = {
  label: string;
  href?: string;
};

export function CatalogBreadcrumb({ items }: Readonly<{ items: BreadcrumbItem[] }>) {
  return <nav aria-label="Breadcrumb" className="mb-5 overflow-hidden">
    <ol className="flex min-w-0 items-center gap-1.5 text-sm">
      {items.map((item, index) => {
        const isCurrent = index === items.length - 1;
        return <li key={`${item.label}-${index}`} className="flex min-w-0 items-center gap-1.5">
          {index > 0 && <ChevronRight aria-hidden="true" className="size-3.5 shrink-0 text-slate-300" />}
          {item.href && !isCurrent
            ? <Link href={item.href} className="shrink-0 font-medium text-slate-500 transition-colors hover:text-cyan-700">{item.label}</Link>
            : <span aria-current={isCurrent ? "page" : undefined} className={`truncate ${isCurrent ? "font-semibold text-slate-800" : "text-slate-500"}`}>{item.label}</span>}
        </li>;
      })}
    </ol>
  </nav>;
}
