"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { Eye, EyeOff, LockKeyhole, Mail } from "lucide-react";
import { useState } from "react";
import { authService } from "@/modules/auth/api/auth.service";
import { AuthShell } from "@/modules/auth/components/auth-shell";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { useToast } from "@/shared/components/ui/toast";

export function AuthForm({ mode }: Readonly<{ mode: "login" | "register" }>) {
  const router = useRouter();
  const { setSession } = useAuth();
  const toast = useToast();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isPasswordVisible, setIsPasswordVisible] = useState(false);
  const isLogin = mode === "login";

  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage(null);
    setIsSubmitting(true);

    try {
      if (isLogin) {
        const session = await authService.login({ email, password });
        setSession(session);
        toast.success("Đăng nhập thành công.");
        router.push(session.role === "Admin" ? "/admin" : session.role === "Manager" ? "/manager" : session.role === "Staff" ? "/staff" : "/");
        return;
      }

      if (password !== confirmPassword) {
        const message = "Mật khẩu xác nhận chưa khớp.";
        setMessage(message);
        toast.error(message);
        return;
      }

      await authService.register({ email, password });
      toast.success("Đăng ký thành công. Hãy kiểm tra email để xác thực tài khoản.");
      router.push(`/verify-email?email=${encodeURIComponent(email.trim())}`);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Có lỗi xảy ra. Vui lòng thử lại.";
      setMessage(message);
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  return <AuthShell
    eyebrow={isLogin ? "CHÀO MỪNG TRỞ LẠI" : "BẮT ĐẦU CÙNG MINISTORE"}
    title={isLogin ? "Đăng nhập tài khoản" : "Tạo tài khoản mới"}
    description={isLogin ? "Đăng nhập để lưu giỏ hàng, theo dõi đơn và mua sắm nhanh hơn." : "Chỉ mất một phút để bắt đầu hành trình mua sắm công nghệ của bạn."}
  >
    <form onSubmit={submit} className="auth-form-stack">
      <label className="auth-field"><span>Email</span><div className="auth-input-wrap"><Mail /><Input type="email" value={email} onChange={(event) => setEmail(event.target.value)} placeholder="you@example.com" autoComplete="email" required /></div></label>
      <label className="auth-field"><span>Mật khẩu</span><div className="auth-input-wrap"><LockKeyhole /><Input type={isPasswordVisible ? "text" : "password"} minLength={isLogin ? 6 : 8} value={password} onChange={(event) => setPassword(event.target.value)} placeholder={isLogin ? "Nhập mật khẩu" : "Tối thiểu 8 ký tự"} autoComplete={isLogin ? "current-password" : "new-password"} required /><button type="button" className="password-visibility" aria-label={isPasswordVisible ? "Ẩn mật khẩu" : "Hiện mật khẩu"} onClick={() => setIsPasswordVisible((visible) => !visible)}>{isPasswordVisible ? <EyeOff /> : <Eye />}</button></div></label>
      {!isLogin && <label className="auth-field"><span>Xác nhận mật khẩu</span><div className="auth-input-wrap"><LockKeyhole /><Input type={isPasswordVisible ? "text" : "password"} minLength={8} value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} placeholder="Nhập lại mật khẩu" autoComplete="new-password" required /><button type="button" className="password-visibility" aria-label={isPasswordVisible ? "Ẩn mật khẩu" : "Hiện mật khẩu"} onClick={() => setIsPasswordVisible((visible) => !visible)}>{isPasswordVisible ? <EyeOff /> : <Eye />}</button></div></label>}
      {!isLogin && <p className="auth-password-hint">Dùng ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.</p>}
      {isLogin && <Link className="auth-inline-link auth-forgot-link" href="/forgot-password">Quên mật khẩu?</Link>}
      {message && <p className="auth-message" role="alert">{message}</p>}
      <Button type="submit" className="auth-submit" disabled={isSubmitting}>{isSubmitting ? "Đang xử lý..." : isLogin ? "Đăng nhập" : "Tạo tài khoản"}</Button>
    </form>
    <p className="auth-switch">{isLogin ? "Chưa có tài khoản?" : "Đã có tài khoản?"} <Link href={isLogin ? "/register" : "/login"}>{isLogin ? "Đăng ký ngay" : "Đăng nhập"}</Link></p>
  </AuthShell>;
}
