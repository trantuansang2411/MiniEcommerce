import { StaffShipmentDetail } from "@/modules/operations/components/staff-shipment-detail";

export default async function StaffShipmentDetailPage({ params }: Readonly<{ params: Promise<{ id: string }> }>) {
  const { id } = await params;
  return <StaffShipmentDetail shipmentId={id} />;
}
