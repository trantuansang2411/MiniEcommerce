"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { PackageSearch, ReceiptText } from "lucide-react";
import { useEffect } from "react";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { useMyOrders, useOrderActions } from "@/modules/orders/hooks/use-orders";
import { formatCurrency } from "@/shared/utils/format";
import { formatStatus } from "@/shared/utils/status";

export default function OrdersPage() {
  const router = useRouter();
  const { session, isReady } = useAuth();
  const orders = useMyOrders(Boolean(session));
  const { cancel } = useOrderActions();

  useEffect(() => {
    if (isReady && !session) router.replace("/login");
  }, [isReady, router, session]);

  if (isReady && !session) return <p>Đang chuyển đến trang đăng nhập...</p>;
  if (!isReady || orders.isLoading) return <p>Đang tải đơn hàng...</p>;
  if (orders.error) return <p className="error-message">{orders.error.message}</p>;

  return <section className="customer-orders-page">
    <header className="customer-orders-heading"><p className="eyebrow">TÀI KHOẢN / ĐƠN HÀNG</p><h1>Đơn của tôi</h1><p>Theo dõi trạng thái, thanh toán lại đơn đang chờ và kiểm tra hành trình giao hàng.</p></header>
    <div className="customer-order-list">
      {orders.data?.map((order) => <article key={order.id} className="customer-order-card">
        <div className="customer-order-meta"><div className="customer-order-icon"><ReceiptText /></div><div><strong>Đơn hàng #{order.id.slice(0, 8).toUpperCase()}</strong><span>{new Date(order.createdAt).toLocaleString("vi-VN")}</span></div></div>
        <span className={`customer-order-status status-${order.status}`}>{formatStatus(order.status)}</span>
        <strong className="customer-order-total">{formatCurrency(order.totalAmount)}</strong>
        <div className="customer-order-actions">
          {order.status === "AwaitingPayment" && <><Link className="button" href={`/orders/${order.id}/payment`}>Thanh toán</Link><button className="button button-secondary" disabled={cancel.isPending} onClick={() => cancel.mutate(order.id)}>Hủy đơn</button></>}
          {["Paid", "Completed"].includes(order.status) && <Link className="button button-secondary" href={`/orders/${order.id}`}><PackageSearch />Theo dõi đơn</Link>}
        </div>
      </article>)}
      {!orders.data?.length && <div className="customer-orders-empty"><ReceiptText /><h2>Bạn chưa có đơn hàng nào</h2><p>Hãy chọn sản phẩm yêu thích để bắt đầu mua sắm.</p><Link className="button" href="/products">Khám phá sản phẩm</Link></div>}
    </div>
  </section>;
}
