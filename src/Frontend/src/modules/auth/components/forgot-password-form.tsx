"use client";

import Link from "next/link";
import { useState } from "react";
import { authService } from "@/modules/auth/api/auth.service";
import { AuthShell } from "@/modules/auth/components/auth-shell";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useToast } from "@/shared/components/ui/toast";

export function ForgotPasswordForm() {
  const toast = useToast();
  const [email, setEmail] = useState(""); const [code, setCode] = useState(""); const [password, setPassword] = useState(""); const [confirmPassword, setConfirmPassword] = useState("");
  const [phase, setPhase] = useState<"request" | "reset">("request"); const [message, setMessage] = useState<string | null>(null); const [isSubmitting, setIsSubmitting] = useState(false);

  async function requestCode(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault(); setMessage(null); setIsSubmitting(true);
    try { const message = "Nếu email tồn tại, mã đặt lại mật khẩu đã được gửi đến hộp thư của bạn."; await authService.forgotPassword({ email }); setPhase("reset"); setMessage(message); toast.success(message); }
    catch (error) { const message = error instanceof Error ? error.message : "Không thể gửi mã lúc này."; setMessage(message); toast.error(message); }
    finally { setIsSubmitting(false); }
  }

  async function resetPassword(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault(); setMessage(null);
    if (password !== confirmPassword) { const message = "Hai mật khẩu mới chưa khớp."; setMessage(message); toast.error(message); return; }
    setIsSubmitting(true);
    try { const message = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới."; await authService.resetPassword({ email, code, newPassword: password }); setMessage(message); toast.success(message); }
    catch (error) { const message = error instanceof Error ? error.message : "Không thể đặt lại mật khẩu."; setMessage(message); toast.error(message); }
    finally { setIsSubmitting(false); }
  }

  return <AuthShell eyebrow={phase === "request" ? "KHÔI PHỤC TÀI KHOẢN" : "XÁC NHẬN MẬT KHẨU MỚI"} title={phase === "request" ? "Quên mật khẩu?" : "Đặt lại mật khẩu"} description={phase === "request" ? "Nhập email của bạn, MiniStore sẽ gửi mã OTP để đặt lại mật khẩu." : "Nhập mã OTP và tạo mật khẩu mới an toàn hơn cho tài khoản của bạn."}>
    {phase === "request" ? <form onSubmit={requestCode} className="auth-form-stack"><label className="auth-field"><span>Email</span><Input type="email" value={email} onChange={(event) => setEmail(event.target.value)} placeholder="you@example.com" autoComplete="email" required /></label>{message && <p className="auth-message" role="status">{message}</p>}<Button type="submit" className="auth-submit" disabled={isSubmitting}>{isSubmitting ? "Đang gửi mã..." : "Gửi mã OTP"}</Button></form> : <form onSubmit={resetPassword} className="auth-form-stack"><label className="auth-field"><span>Email</span><Input type="email" value={email} disabled /></label><label className="auth-field"><span>Mã OTP</span><Input className="auth-otp-input" inputMode="numeric" pattern="[0-9]{6}" maxLength={6} value={code} onChange={(event) => setCode(event.target.value.replace(/\D/g, ""))} placeholder="000000" autoComplete="one-time-code" required /></label><label className="auth-field"><span>Mật khẩu mới</span><Input type="password" minLength={8} value={password} onChange={(event) => setPassword(event.target.value)} autoComplete="new-password" required /></label><label className="auth-field"><span>Nhập lại mật khẩu mới</span><Input type="password" minLength={8} value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} autoComplete="new-password" required /></label>{message && <p className="auth-message" role="status">{message}</p>}<Button type="submit" className="auth-submit" disabled={isSubmitting}>{isSubmitting ? "Đang cập nhật..." : "Đặt lại mật khẩu"}</Button><Button variant="ghost" type="button" onClick={() => setPhase("request")}>Dùng email khác</Button></form>}
    <p className="auth-switch">Nhớ mật khẩu rồi? <Link href="/login">Đăng nhập</Link></p>
  </AuthShell>;
}
