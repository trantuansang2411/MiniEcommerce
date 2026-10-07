export const statusLabels: Record<string, string> = {
  Active: "Đang hoạt động",
  Inactive: "Tạm ngưng",
  Closed: "Đã đóng",
  Draft: "Bản nháp",
  AwaitingPayment: "Chờ thanh toán",
  Paid: "Đã thanh toán",
  Completed: "Hoàn thành",
  Cancelled: "Đã hủy",
  Expired: "Đã hết hạn",
  Pending: "Chờ lấy hàng",
  Picking: "Đang lấy hàng",
  Packed: "Đã đóng gói",
  Shipped: "Đã bàn giao vận chuyển",
  InTransit: "Đang giao hàng",
  Delivered: "Đã giao hàng",
};

export function formatStatus(value: string | null | undefined) {
  return value ? statusLabels[value] ?? value : "—";
}

export const orderStatusOptions = [
  { value: "", label: "Tất cả trạng thái" },
  { value: "1", label: statusLabels.AwaitingPayment },
  { value: "2", label: statusLabels.Paid },
  { value: "3", label: statusLabels.Completed },
  { value: "4", label: statusLabels.Cancelled },
  { value: "5", label: statusLabels.Expired },
] as const;

export const shipmentStatusOptions = [
  { value: "", label: "Tất cả trạng thái" },
  { value: "1", label: statusLabels.Pending },
  { value: "2", label: statusLabels.Picking },
  { value: "3", label: statusLabels.Packed },
  { value: "4", label: statusLabels.Shipped },
  { value: "5", label: statusLabels.InTransit },
  { value: "6", label: statusLabels.Delivered },
  { value: "7", label: statusLabels.Cancelled },
] as const;
