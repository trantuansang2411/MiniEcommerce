import Image from "next/image";
import { Check, Headphones, ShieldCheck, Sparkles } from "lucide-react";
import { Card, CardContent } from "@/shared/components/ui/card";

type AuthShellProps = { eyebrow: string; title: string; description: string; children: React.ReactNode };

export function AuthShell({ eyebrow, title, description, children }: Readonly<AuthShellProps>) {
  return <section className="auth-experience">
    <aside className="auth-showcase" aria-label="Giới thiệu MiniStore">
      <div className="auth-showcase-orbit auth-showcase-orbit-one" /><div className="auth-showcase-orbit auth-showcase-orbit-two" />
      <div className="auth-showcase-top"><Image src="/brand/AnhEcommerc.png" alt="MiniStore" width={48} height={48} /><span>MINI<strong>STORE</strong></span></div>
      <div className="auth-showcase-copy"><p>MINISTORE / CONNECTED LIFE</p><h2>Công nghệ phù hợp.<br /><strong>Trải nghiệm trọn vẹn.</strong></h2><span>Khám phá thiết bị chính hãng, lưu đơn hàng và mua sắm dễ dàng theo cách của bạn.</span></div>
      <div className="auth-trust-list"><span><ShieldCheck />Bảo mật tài khoản</span><span><Headphones />Hỗ trợ tận tâm</span><span><Sparkles />Ưu đãi dành riêng</span></div>
    </aside>
    <Card className="auth-panel"><CardContent className="auth-panel-content"><div className="auth-panel-heading"><p>{eyebrow}</p><h1>{title}</h1><span>{description}</span></div>{children}<div className="auth-secure-note"><Check />Thông tin của bạn luôn được bảo vệ an toàn.</div></CardContent></Card>
  </section>;
}
