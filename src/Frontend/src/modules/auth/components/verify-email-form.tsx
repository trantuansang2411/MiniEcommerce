"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { authService } from "@/modules/auth/api/auth.service";
import { AuthShell } from "@/modules/auth/components/auth-shell";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useToast } from "@/shared/components/ui/toast";

export function VerifyEmailForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { setSession } = useAuth();
  const toast = useToast();
  const [email, setEmail] = useState(searchParams.get("email") ?? "");
  const [code, setCode] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isResending, setIsResending] = useState(false);

  async function verify(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage(null); setIsSubmitting(true);
    try {
      const session = await authService.verifyEmail({ email, code });
      setSession(session); toast.success("Xác thực email thành công."); router.push(session.role === "Admin" ? "/admin" : session.role === "Manager" ? "/manager" : session.role === "Staff" ? "/staff" : "/");
    } catch (error) {
      const message = error instanceof Error ? error.message : "Không thể xác thực mã. Vui lòng thử lại.";
      setMessage(message); toast.error(message);
    } finally { setIsSubmitting(false); }
  }

  async function resendCode() {
    setMessage(null); setIsResending(true);
    try {
      await authService.resendVerification({ email });
      const message = "Mã xác thực mới đã được gửi. Hãy kiểm tra hộp thư của bạn.";
      setMessage(message); toast.success(message);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Không thể gửi lại mã lúc này.";
      setMessage(message); toast.error(message);
    } finally { setIsResending(false); }
  }

  return <AuthShell eyebrow="BƯỚC CUỐI CÙNG" title="Xác thực email" description="Nhập mã gồm 6 chữ số MiniStore vừa gửi để kích hoạt tài khoản.">
    <form onSubmit={verify} className="auth-form-stack">
      <label className="auth-field"><span>Email</span><Input type="email" value={email} onChange={(event) => setEmail(event.target.value)} autoComplete="email" required /></label>
      <label className="auth-field"><span>Mã xác thực</span><Input className="auth-otp-input" inputMode="numeric" pattern="[0-9]{6}" maxLength={6} value={code} onChange={(event) => setCode(event.target.value.replace(/\D/g, ""))} placeholder="000000" autoComplete="one-time-code" required /></label>
      {message && <p className="auth-message" role="status">{message}</p>}<Button type="submit" className="auth-submit" disabled={isSubmitting}>{isSubmitting ? "Đang xác thực..." : "Xác thực email"}</Button>
    </form>
    <div className="auth-resend-row"><span>Chưa nhận được mã?</span><Button variant="ghost" size="sm" type="button" onClick={resendCode} disabled={!email || isResending}>{isResending ? "Đang gửi..." : "Gửi lại mã"}</Button></div>
    <p className="auth-switch">Đã có tài khoản? <Link href="/login">Đăng nhập</Link></p>
  </AuthShell>;
}
