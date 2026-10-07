"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { CircleCheck, Clock3, MapPin, PackageCheck, Truck } from "lucide-react";
import { useEffect } from "react";
import { useAuth } from "@/modules/auth/components/auth-provider";
import { useMyOrder, useMyOrderShipments } from "@/modules/orders/hooks/use-orders";
import { formatCurrency } from "@/shared/utils/format";
import { formatStatus } from "@/shared/utils/status";

export default function OrderTrackingPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const { session, isReady } = useAuth();
  const order = useMyOrder(params.id, Boolean(session) && Boolean(params.id));
  const shipments = useMyOrderShipments(params.id, Boolean(session) && Boolean(params.id));

  useEffect(() => {
    if (isReady && !session) router.replace("/login");
  }, [isReady, router, session]);

  if (isReady && !session) return <p>Đang chuyển đến trang đăng nhập...</p>;
  if (!isReady || order.isLoading || shipments.isLoading) return <p>Đang tải hành trình đơn hàng...</p>;
  if (order.error || !order.data) return <p className="error-message">{order.error?.message ?? "Không tìm thấy đơn hàng."}</p>;

  const isAwaitingPayment = order.data.status === "AwaitingPayment";
  const shipmentStatuses = shipments.data?.map((shipment) => shipment.status) ?? [];
  const trackingStep = shipmentStatuses.length === 0
    ? 1
    : shipmentStatuses.every((status) => status === "Delivered")
      ? 3
      : shipmentStatuses.some((status) => status === "Shipped" || status === "InTransit")
        ? 2
        : 1;
  const progressClass = (step: number) => step < trackingStep ? "is-complete" : step === trackingStep ? "is-current" : "";
  return <section className="tracking-page">
    <header className="tracking-heading"><p className="eyebrow">ĐƠN HÀNG / THEO DÕI</p><h1>Đơn #{order.data.id.slice(0, 8).toUpperCase()}</h1><p>Đặt lúc {new Date(order.data.createdAt).toLocaleString("vi-VN")} · {formatCurrency(order.data.totalAmount)}</p></header>
    {isAwaitingPayment ? <div className="tracking-payment-reminder"><Clock3 /><div><strong>Đơn đang chờ thanh toán</strong><span>Hoàn tất thanh toán để cửa hàng bắt đầu chuẩn bị hàng.</span></div><Link className="button" href={`/orders/${order.data.id}/payment`}>Thanh toán ngay</Link></div> : <div className="tracking-card"><div className="tracking-progress"><span className="is-complete"><CircleCheck />Đã thanh toán</span><span className={progressClass(1)}><PackageCheck />Đang chuẩn bị</span><span className={progressClass(2)}><Truck />Đang giao</span><span className={progressClass(3)}><MapPin />Đã giao</span></div>{shipments.error ? <p className="error-message">{shipments.error.message}</p> : shipments.data?.length ? <div className="shipment-list">{shipments.data.map((shipment) => <article key={shipment.id} className="shipment-card"><Truck /><div><strong>Kiện hàng #{shipment.id.slice(0, 8).toUpperCase()}</strong><span>Trạng thái: {formatStatus(shipment.status)}</span>{shipment.trackingNumber && <small>Mã vận đơn: {shipment.trackingNumber}</small>}</div></article>)}</div> : <div className="shipment-pending"><PackageCheck /><div><strong>Đơn hàng đang được chuẩn bị</strong><span>Cửa hàng sẽ cập nhật hành trình giao hàng ngay khi bàn giao cho đơn vị vận chuyển.</span></div></div>}</div>}
  </section>;
}
