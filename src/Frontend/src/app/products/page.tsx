"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { ProductCard } from "@/modules/catalog/components/product-card";
import { CatalogBreadcrumb } from "@/modules/catalog/components/catalog-breadcrumb";
import { useCategories, useProductSearch } from "@/modules/catalog/hooks/use-catalog";

export default function ProductsPage() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const search = searchParams.get("q")?.trim() || undefined;
  const categoryId = searchParams.get("category");
  const parsedPage = Number.parseInt(searchParams.get("page") ?? "1", 10);
  const page = Number.isFinite(parsedPage) && parsedPage > 0 ? parsedPage : 1;
  const pageSize = 20;
  const { data: categories, isLoading: isLoadingCategories } = useCategories();
  const { data: result, isLoading, error } = useProductSearch(search, categoryId ?? undefined, page, pageSize);
  const category = categories?.find((item) => item.id === categoryId);

  function goToPage(nextPage: number) {
    const params = new URLSearchParams();
    if (search) params.set("q", search);
    if (categoryId) params.set("category", categoryId);
    if (nextPage > 1) params.set("page", String(nextPage));
    router.push(`/products${params.size ? `?${params.toString()}` : ""}`);
  }

  if (isLoading || isLoadingCategories) return <p className="store-status">Đang tải sản phẩm...</p>;
  if (error) return <p className="error-message">{error.message}</p>;

  return (
    <section className="mx-auto w-full max-w-[1440px] px-1 pb-12 pt-3 sm:px-3 sm:pt-7">
      <div className="mb-8 border-b border-slate-200 pb-7 sm:mb-10">
        <CatalogBreadcrumb items={category
          ? [{ label: "Trang chủ", href: "/" }, { label: category.name }]
          : [{ label: "Trang chủ", href: "/" }, { label: search ? "Tìm kiếm" : "Sản phẩm" }]} />
        <h1 className="text-3xl font-black tracking-[-.04em] text-slate-900 sm:text-4xl">{search ? `Kết quả cho “${search}”` : category?.name ?? "Sản phẩm"}</h1>
        <p className="mt-3 max-w-2xl text-base leading-7 text-slate-500">{search ? `Tìm thấy ${result?.totalCount ?? 0} sản phẩm phù hợp.` : category?.description || "Khám phá những thiết bị công nghệ phù hợp với nhu cầu của bạn."}</p>
      </div>
      {categoryId && !category ? <p className="catalog-empty">Danh mục này không còn tồn tại.</p> : result?.items.length ? <>
        <div className="grid grid-cols-1 gap-4 min-[580px]:grid-cols-2 md:grid-cols-3 xl:grid-cols-5 xl:gap-5">{result.items.map((product) => <ProductCard key={product.id} product={product} variant="catalog" />)}</div>
        {result.totalPages > 1 && <nav className="pagination" aria-label="Phân trang sản phẩm"><button disabled={result.page === 1} onClick={() => goToPage(result.page - 1)}>←</button>{Array.from({ length: result.totalPages }, (_, index) => index + 1).map((pageNumber) => <button key={pageNumber} className={pageNumber === result.page ? "is-current" : ""} onClick={() => goToPage(pageNumber)}>{pageNumber}</button>)}<button disabled={result.page === result.totalPages} onClick={() => goToPage(result.page + 1)}>→</button></nav>}
      </> : <p className="catalog-empty">Chưa có sản phẩm nào trong danh mục này.</p>}
    </section>
  );
}
