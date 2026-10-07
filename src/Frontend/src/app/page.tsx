import { Suspense } from "react";
import { Storefront } from "@/modules/catalog/components/storefront";

export default function HomePage() {
  return <Suspense fallback={<p className="store-status">Đang tải cửa hàng...</p>}><Storefront /></Suspense>;
}
