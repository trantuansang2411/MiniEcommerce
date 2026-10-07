namespace PublicApi.DTOs.Responses.Orders;

public sealed class OrderResponse
{
    public Guid Id { get; init; }
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public string? RecipientName { get; init; }
    public string? RecipientPhone { get; init; }
    public string? ShippingAddress { get; init; }
    public string? DeliveryNote { get; init; }
    public IReadOnlyList<OrderItemResponse> Items { get; init; } = Array.Empty<OrderItemResponse>();
}

public sealed class OrderItemResponse
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

public sealed class OrderManagementDetailResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public OrderDeliveryResponse Delivery { get; init; } = new();
    public IReadOnlyList<OrderManagementItemResponse> Items { get; init; } = Array.Empty<OrderManagementItemResponse>();
    public IReadOnlyList<OrderPaymentResponse> Payments { get; init; } = Array.Empty<OrderPaymentResponse>();
    public IReadOnlyList<OrderShipmentSummaryResponse> Shipments { get; init; } = Array.Empty<OrderShipmentSummaryResponse>();
}

public sealed class OrderDeliveryResponse { public string? RecipientName { get; init; } public string? RecipientPhone { get; init; } public string? ShippingAddress { get; init; } public string? DeliveryNote { get; init; } }
public sealed class OrderManagementItemResponse { public Guid ProductId { get; init; } public string ProductName { get; init; } = string.Empty; public string ThumbnailUrl { get; init; } = string.Empty; public int Quantity { get; init; } public decimal UnitPrice { get; init; } public decimal LineTotal { get; init; } }
public sealed class OrderPaymentResponse { public Guid Id { get; init; } public string Method { get; init; } = string.Empty; public string Status { get; init; } = string.Empty; public string? TransactionId { get; init; } public decimal Amount { get; init; } public DateTimeOffset CreatedAt { get; init; } public DateTimeOffset? PaidAt { get; init; } }
public sealed class OrderShipmentSummaryResponse { public Guid Id { get; init; } public string Status { get; init; } = string.Empty; public string? ShippingProvider { get; init; } public string? TrackingNumber { get; init; } public int ItemCount { get; init; } public DateTimeOffset CreatedAt { get; init; } public DateTimeOffset? ShippedAt { get; init; } public DateTimeOffset? DeliveredAt { get; init; } }
