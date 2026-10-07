"use client";

import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import { ChevronLeft, ChevronRight, Headphones, Pause, Play, ShieldCheck, Truck } from "lucide-react";
import { CategoryIcon } from "@/modules/catalog/components/category-icon";
import { ProductCard } from "@/modules/catalog/components/product-card";
import { useCategories, useProducts } from "@/modules/catalog/hooks/use-catalog";

const pageSize = 42;
const heroSlides = [
  { image: "/images/ministore-tech-hero.png", eyebrow: "MINISTORE / TECH DROP", titleWhite: "Thiết bị mới.", titleBlue: "Nhịp sống mới.", description: "Chọn những món công nghệ thật hợp với cách bạn học, làm việc và giải trí.", action: "Khám phá sản phẩm" },
  { image: "/images/tech-workspace.jpg", eyebrow: "MINISTORE / WORKSPACE EDITION", titleWhite: "Góc làm việc mới.", titleBlue: "Cảm hứng mới.", description: "Từ laptop, màn hình đến phụ kiện — chọn bộ thiết bị phù hợp với bạn.", action: "Xem bộ sưu tập" },
  { image: "/images/tech-desk.jpg", eyebrow: "MINISTORE / SMARTER EVERYDAY", titleWhite: "Công nghệ gọn gàng.", titleBlue: "Trải nghiệm rộng mở.", description: "Khám phá các thiết bị giúp mọi khoảnh khắc trở nên đơn giản hơn.", action: "Mua sắm ngay" },
];

export function Storefront() {
  const searchParams = useSearchParams();
  const { data: products, isLoading, error } = useProducts();
  const { data: categories } = useCategories();
  const [activeCategory, setActiveCategory] = useState(searchParams.get("category") ?? "all");
  const [search, setSearch] = useState(searchParams.get("q") ?? "");
  const [page, setPage] = useState(1);
  const [activeSlide, setActiveSlide] = useState(0);
  const [isCarouselPlaying, setIsCarouselPlaying] = useState(true);

  useEffect(() => {
    setActiveCategory(searchParams.get("category") ?? "all");
    setSearch(searchParams.get("q") ?? "");
  }, [searchParams]);

  useEffect(() => {
    if (!isCarouselPlaying) return;
    const interval = window.setInterval(() => setActiveSlide((current) => (current + 1) % heroSlides.length), 5500);
    return () => window.clearInterval(interval);
  }, [isCarouselPlaying]);

  const filteredProducts = useMemo(() => (products ?? []).filter((product) => {
    const matchesCategory = activeCategory === "all" || product.categoryId === activeCategory;
    const matchesSearch = product.name.toLocaleLowerCase("vi-VN").includes(search.toLocaleLowerCase("vi-VN"));
    return matchesCategory && matchesSearch;
  }), [activeCategory, products, search]);

  const totalPages = Math.max(1, Math.ceil(filteredProducts.length / pageSize));
  const visibleProducts = filteredProducts.slice((page - 1) * pageSize, page * pageSize);
  const selectedCategory = categories?.find((category) => category.id === activeCategory);
  const selectCategory = (categoryId: string) => { setActiveCategory(categoryId); setPage(1); };

  return <div className="storefront">
    <section className="tech-carousel" aria-roledescription="carousel" aria-label="Ưu đãi công nghệ">
      <div className="carousel-track" style={{ transform: `translateX(-${activeSlide * 100}%)` }}>
        {heroSlides.map((slide, index) => <article className="carousel-slide" key={slide.image} aria-hidden={activeSlide !== index}>
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img src={slide.image} alt="" className="tech-hero-image" />
          <div className="tech-hero-overlay" />
          <div className="tech-hero-copy">
            <p className="hero-kicker">{slide.eyebrow}</p>
            <h1><span>{slide.titleWhite}</span><strong>{slide.titleBlue}</strong></h1>
            <p>{slide.description}</p>
            <a href="#products" className="hero-action">{slide.action} <span>→</span></a>
            <div className="hero-benefits">
              <span><Truck />Giao hàng nhanh<small>Toàn quốc</small></span>
              <span><ShieldCheck />Sản phẩm chính hãng<small>100% đảm bảo</small></span>
              <span><Headphones />Hỗ trợ tận tâm<small>Luôn đồng hành</small></span>
            </div>
          </div>
        </article>)}
      </div>
      <button className="carousel-arrow carousel-arrow-previous" aria-label="Banner trước" onClick={() => setActiveSlide((current) => (current - 1 + heroSlides.length) % heroSlides.length)}><ChevronLeft aria-hidden="true" /></button>
      <button className="carousel-arrow carousel-arrow-next" aria-label="Banner tiếp theo" onClick={() => setActiveSlide((current) => (current + 1) % heroSlides.length)}><ChevronRight aria-hidden="true" /></button>
      <div className="carousel-controls">
        <div className="carousel-dots">{heroSlides.map((slide, index) => <button key={slide.image} className={activeSlide === index ? "is-active" : ""} aria-label={`Chuyển đến slide ${index + 1}`} onClick={() => setActiveSlide(index)} />)}</div>
        <button className="carousel-playback" aria-label={isCarouselPlaying ? "Dừng carousel" : "Phát carousel"} aria-pressed={!isCarouselPlaying} onClick={() => setIsCarouselPlaying((current) => !current)}>{isCarouselPlaying ? <Pause aria-hidden="true" /> : <Play aria-hidden="true" />}</button>
      </div>
    </section>

    <section className="category-section" aria-labelledby="category-heading">
      <div className="section-intro"><p className="section-eyebrow">BẮT ĐẦU TỪ ĐÂY</p><h2 id="category-heading">Khám phá theo danh mục</h2></div>
      <div className="category-grid">
        <button className={`category-tile ${activeCategory === "all" ? "is-active" : ""}`} onClick={() => selectCategory("all")}><span><CategoryIcon iconKey="all" className="size-7" /></span><strong>Tất cả</strong></button>
        {categories?.map((category) => <button key={category.id} className={`category-tile ${activeCategory === category.id ? "is-active" : ""}`} onClick={() => selectCategory(category.id)}><span><CategoryIcon iconKey={category.iconKey} className="size-7" /></span><strong>{category.name}</strong></button>)}
      </div>
    </section>

    <section className="product-section" id="products" aria-labelledby="product-heading">
      {selectedCategory && <div className="catalog-toolbar"><div><p className="section-eyebrow">{selectedCategory.name}</p><h2 id="product-heading">{selectedCategory.description || selectedCategory.name}</h2></div></div>}
      {isLoading ? <p className="store-status">Đang tải sản phẩm...</p> : error ? <p className="store-status error-message">{error.message}</p> : visibleProducts.length ? <div className="store-product-grid">{visibleProducts.map((product) => <ProductCard product={product} key={product.id} />)}</div> : <div className="catalog-empty"><span>⌁</span><h3>Chưa tìm thấy sản phẩm phù hợp</h3><p>Hãy thử một từ khóa khác hoặc chọn lại danh mục.</p><button onClick={() => { setSearch(""); selectCategory("all"); }}>Xem toàn bộ sản phẩm</button></div>}
      {filteredProducts.length > pageSize && <nav className="pagination" aria-label="Phân trang sản phẩm"><button disabled={page === 1} onClick={() => setPage((current) => current - 1)}>←</button>{Array.from({ length: totalPages }, (_, index) => index + 1).map((pageNumber) => <button key={pageNumber} className={page === pageNumber ? "is-current" : ""} onClick={() => setPage(pageNumber)}>{pageNumber}</button>)}<button disabled={page === totalPages} onClick={() => setPage((current) => current + 1)}>→</button></nav>}
    </section>
  </div>;
}
