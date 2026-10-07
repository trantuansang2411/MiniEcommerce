import Link from "next/link";
import { ArrowLeft, Boxes, SearchX } from "lucide-react";

export default function NotFound() {
  return <section className="not-found-page"><div className="not-found-code">404</div><div className="not-found-orbit"><SearchX /></div><p className="eyebrow">MINISTORE / KHÔNG TÌM THẤY</p><h1>Trang này đã đi lạc<br /><strong>giữa thế giới công nghệ.</strong></h1><p>Liên kết có thể đã thay đổi hoặc trang bạn tìm không còn tồn tại. Hãy quay lại để tiếp tục khám phá.</p><div><Link className="button" href="/"><Boxes />Về trang chủ</Link><Link className="not-found-secondary" href="/products"><ArrowLeft />Xem sản phẩm</Link></div></section>;
}
