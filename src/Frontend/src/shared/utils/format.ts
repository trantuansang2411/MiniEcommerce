export function formatCurrency(value: number) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(value);
}

export function productImageUrl(imageUrl: string) {
  if (!imageUrl) return "";
  if (imageUrl.startsWith("http")) return imageUrl;

  const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5114/api";
  return `${apiUrl.replace(/\/api$/, "")}${imageUrl}`;
}
