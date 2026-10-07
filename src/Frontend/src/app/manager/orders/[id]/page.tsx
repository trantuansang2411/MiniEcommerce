import { OrderManagementDetailView } from "@/modules/operations/components/order-management-detail";

export default async function ManagerOrderDetailPage({ params }: Readonly<{ params: Promise<{ id: string }> }>) {
  const { id } = await params;
  return <OrderManagementDetailView role="Manager" orderId={id} />;
}
