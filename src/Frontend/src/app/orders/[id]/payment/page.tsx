"use client";

import Link from "next/link";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { ArrowLeft, ArrowRight, CheckCircle2, CircleAlert, ClipboardList, Clock3, Copy, CreditCard, Headphones, Landmark, LockKeyhole, Package, ReceiptText, ShieldCheck, Truck } from "lucide-react";
import { useEffect, useState } from "react";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { useMyOrder, useOrderActions } from "@/modules/orders/hooks/use-orders";
import { orderService } from "@/modules/orders/api/order.service";
import { useToast } from "@/shared/components/ui/toast";
import { formatCurrency } from "@/shared/utils/format";
import { formatStatus } from "@/shared/utils/status";

export default function PaymentPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const searchParams = useSearchParams();
  const toast = useToast();
  const { session, isReady } = useAuth();
  const order = useMyOrder(params.id, Boolean(session) && Boolean(params.id));
  const { payManually } = useOrderActions();
  const [isPaymentComplete, setIsPaymentComplete] = useState(false);
  const [isVnPayRedirecting, setIsVnPayRedirecting] = useState(false);
  const [vnpayResult, setVnpayResult] = useState<"waiting" | "failed" | null>(null);
  const vnpayPaymentId = searchParams.get("vnpayPaymentId");

  useEffect(() => {
    if (isReady && !session) router.replace("/login");
  }, [isReady, router, session]);

  useEffect(() => {
    if (!isPaymentComplete) return;
    const timeout = window.setTimeout(() => router.replace("/orders?payment=success"), 1400);
    return () => window.clearTimeout(timeout);
  }, [isPaymentComplete, router]);

  useEffect(() => {
    if (!session || !vnpayPaymentId || !params.id) return;
    if (searchParams.get("verified") === "false") { setVnpayResult("failed"); return; }
    let cancelled = false;
    let timer: number | undefined;
    const poll = async () => {
      try {
        const payment = await orderService.getPayment(params.id, vnpayPaymentId);
        if (cancelled) return;
        if (payment.status === "Succeeded") { setIsPaymentComplete(true); return; }
        if (payment.status !== "Pending") { setVnpayResult("failed"); return; }
        setVnpayResult("waiting");
        timer = window.setTimeout(() => void poll(), 2000);
      } catch { if (!cancelled) setVnpayResult("failed"); }
    };
    void poll();
    return () => { cancelled = true; if (timer) window.clearTimeout(timer); };
  }, [params.id, searchParams, session, vnpayPaymentId]);

  async function payWithMock() {
    try {
      await payManually.mutateAsync(params.id);
      setIsPaymentComplete(true);
      toast.success("Thanh toán Mock thành công. Đơn hàng đang được chuẩn bị.");
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Không thể hoàn tất thanh toán.");
    }
  }

  async function payWithVnPay() {
    try {
      setIsVnPayRedirecting(true);
      const payment = await orderService.createVnPayPayment(params.id);
      window.location.assign(payment.paymentUrl);
    } catch (error) {
      setIsVnPayRedirecting(false);
      toast.error(error instanceof Error ? error.message : "Không thể khởi tạo thanh toán VNPay.");
    }
  }

  if (isReady && !session) return <p>Đang chuyển đến trang đăng nhập...</p>;
  if (!isReady || order.isLoading) return <p>Đang tải thông tin thanh toán...</p>;
  if (order.error || !order.data) return <p className="error-message">{order.error?.message ?? "Không tìm thấy đơn hàng."}</p>;
  if (isPaymentComplete) return <section className="payment-success-state"><CheckCircle2 /><p>THANH TOÁN THÀNH CÔNG</p><h1>Cảm ơn bạn đã mua sắm!</h1><span>Đang chuyển bạn đến Đơn của tôi…</span></section>;
  if (vnpayResult === "waiting") return <section className="payment-success-state"><Clock3 /><p>ĐANG XÁC NHẬN THANH TOÁN</p><h1>VNPay đang gửi kết quả giao dịch</h1><span>Vui lòng không đóng trang này. Hệ thống sẽ tự cập nhật khi nhận được xác nhận.</span></section>;
  if (vnpayResult === "failed") return <section className="payment-unavailable"><CircleAlert /><h1>Thanh toán chưa thành công</h1><p>Giao dịch bị hủy, không hợp lệ hoặc chưa được VNPay xác nhận.</p><Link className="button" href={`/orders/${params.id}/payment`}>Thử lại thanh toán</Link></section>;
  if (order.data.status !== "AwaitingPayment") return <section className="payment-unavailable"><CircleAlert /><h1>Đơn này không chờ thanh toán</h1><p>Trạng thái hiện tại: <strong>{formatStatus(order.data.status)}</strong>.</p><Link className="button" href="/orders">Về Đơn của tôi</Link></section>;

  const itemQuantity = order.data.items.reduce((total, item) => total + item.quantity, 0);
  const orderCode = `#${order.data.id.slice(0, 8).toUpperCase()}`;

  async function copyOrderCode() {
    try {
      await navigator.clipboard.writeText(orderCode);
      toast.success("Đã sao chép mã đơn hàng.");
    } catch {
      toast.error("Không thể sao chép mã đơn hàng.");
    }
  }

  return <section className="payment-page payment-workflow">
    <div className="payment-topbar"><Link href="/cart"><ArrowLeft />Quay lại giỏ hàng</Link><ol aria-label="Tiến trình thanh toán"><li className="is-complete"><span>1</span>Giỏ hàng</li><li className="is-current"><span>2</span>Thanh toán</li><li><span>3</span>Hoàn tất</li></ol></div>
    <header className="payment-heading"><p className="eyebrow">THANH TOÁN ĐƠN HÀNG</p><h1>Chọn phương thức thanh toán</h1><p className="payment-hold"><Clock3 />Đơn <strong>{orderCode}</strong> sẽ được giữ hàng đến <b>{new Date(order.data.expiresAt).toLocaleString("vi-VN")}</b>.</p></header>
    <div className="payment-layout">
      <section className="payment-method-panel"><header><span><CreditCard /></span><div><h2>Phương thức thanh toán</h2><p>Chọn phương thức phù hợp để thanh toán đơn hàng của bạn.</p></div></header><div className="payment-methods"><button type="button" className="payment-method is-selected" onClick={() => void payWithVnPay()} disabled={isVnPayRedirecting}><span className="payment-method-icon vnpay-mark"><Landmark /></span><span><strong>Thanh toán VNPay <em>Sandbox</em></strong><small>Bạn sẽ được chuyển đến cổng thanh toán VNPay để hoàn tất giao dịch.</small></span><span className="payment-radio" aria-hidden="true" /></button><button type="button" className="payment-method" onClick={() => void payWithMock()} disabled={payManually.isPending || isVnPayRedirecting}><span className="payment-method-icon"><CreditCard /></span><span><strong>Thanh toán test nội bộ</strong><small>Dùng khi kiểm thử nhanh luồng đơn hàng tại máy local.</small></span><span className="payment-radio" aria-hidden="true" /></button></div><footer className="payment-trust-row"><span><ShieldCheck /><strong>Thanh toán an toàn</strong><small>Dữ liệu được bảo vệ</small></span><span><Headphones /><strong>Hỗ trợ nhanh</strong><small>Luôn sẵn sàng hỗ trợ</small></span><span><LockKeyhole /><strong>Bảo mật giao dịch</strong><small>Thông tin được giữ kín</small></span></footer></section>
      <aside className="payment-order-summary"><header><span><ClipboardList /></span><div><h2>Tóm tắt đơn hàng</h2><p>Kiểm tra thông tin trước khi thanh toán.</p></div></header><div className="payment-order-id"><div><small>Mã đơn hàng</small><strong>{orderCode}</strong></div><button type="button" onClick={() => void copyOrderCode()} aria-label={`Sao chép mã đơn hàng ${orderCode}`} title="Sao chép mã đơn hàng"><Copy /></button></div><div className="payment-summary-rows"><p><Package /><span>Số lượng sản phẩm</span><strong>{itemQuantity} sản phẩm</strong></p><p><ReceiptText /><span>Tạm tính</span><strong>{formatCurrency(order.data.totalAmount)}</strong></p><p><Truck /><span>Phí vận chuyển</span><b>Miễn phí</b></p></div><div className="payment-total"><span>Tổng thanh toán</span><strong>{formatCurrency(order.data.totalAmount)}</strong></div><button className="payment-submit" type="button" onClick={() => void payWithVnPay()} disabled={isVnPayRedirecting}><Landmark />{isVnPayRedirecting ? "Đang chuyển sang VNPay..." : "Thanh toán qua VNPay"}<ArrowRight /></button><small className="payment-security-note"><ShieldCheck />Kết quả thanh toán chỉ được xác nhận sau khi hệ thống nhận IPN từ VNPay.</small></aside>
    </div>
  </section>;
}
