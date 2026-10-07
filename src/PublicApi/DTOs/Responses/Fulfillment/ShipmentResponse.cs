using ApplicationCore.Entities.Fulfillment;
namespace PublicApi.DTOs.Responses.Fulfillment;
public sealed class ShipmentResponse { public Guid Id{get;init;} public Guid OrderId{get;init;} public Guid WarehouseId{get;init;} public string Status{get;init;}=string.Empty; public string? ShippingProvider{get;init;} public string? TrackingNumber{get;init;} public IReadOnlyList<ShipmentItemResponse> Items{get;init;}=Array.Empty<ShipmentItemResponse>(); }
public sealed class ShipmentItemResponse { public Guid ProductId{get;init;} public int Quantity{get;init;} }
public sealed class ShipmentDetailResponse
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ShippingProvider { get; init; }
    public string? TrackingNumber { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ShippedAt { get; init; }
    public DateTimeOffset? DeliveredAt { get; init; }
    public ShipmentDeliveryResponse Delivery { get; init; } = new();
    public IReadOnlyList<ShipmentDetailItemResponse> Items { get; init; } = Array.Empty<ShipmentDetailItemResponse>();
}
public sealed class ShipmentDeliveryResponse { public string? RecipientName { get; init; } public string? RecipientPhone { get; init; } public string? ShippingAddress { get; init; } public string? DeliveryNote { get; init; } }
public sealed class ShipmentDetailItemResponse { public Guid ProductId { get; init; } public string ProductName { get; init; } = string.Empty; public string ThumbnailUrl { get; init; } = string.Empty; public int Quantity { get; init; } }
