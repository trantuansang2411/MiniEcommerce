using ApplicationCore.Entities.Orders;

namespace ApplicationCore.Entities.Fulfillment;

public class Shipment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public Guid WarehouseId { get; private set; }
    public ShipmentStatus Status { get; private set; } = ShipmentStatus.Pending;
    public string? ShippingProvider { get; private set; }
    public string? TrackingNumber { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ShippedAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public ICollection<ShipmentItem> Items { get; private set; } = new List<ShipmentItem>();

    private Shipment() { }

    public Shipment(Guid orderId, Guid warehouseId)
    {
        if (orderId == Guid.Empty || warehouseId == Guid.Empty)
            throw new ArgumentException("OrderId and WarehouseId are required.");
        OrderId = orderId;
        WarehouseId = warehouseId;
    }

    public void AddItem(Guid orderItemId, Guid productId, int quantity) => Items.Add(new ShipmentItem(Id, orderItemId, productId, quantity));

    public void MoveTo(ShipmentStatus nextStatus, string? shippingProvider = null, string? trackingNumber = null)
    {
        var valid = (Status, nextStatus) switch
        {
            (ShipmentStatus.Pending, ShipmentStatus.Picking) => true,
            (ShipmentStatus.Picking, ShipmentStatus.Packed) => true,
            (ShipmentStatus.Packed, ShipmentStatus.Shipped) => true,
            (ShipmentStatus.Shipped, ShipmentStatus.InTransit) => true,
            (ShipmentStatus.InTransit, ShipmentStatus.Delivered) => true,
            _ => false
        };
        if (!valid) throw new InvalidOperationException("Invalid shipment status transition.");
        if (nextStatus == ShipmentStatus.Shipped && (string.IsNullOrWhiteSpace(shippingProvider) || string.IsNullOrWhiteSpace(trackingNumber)))
            throw new InvalidOperationException("ShippingProvider and TrackingNumber are required when shipping.");
        if (nextStatus == ShipmentStatus.Shipped) { ShippingProvider = shippingProvider!.Trim(); TrackingNumber = trackingNumber!.Trim(); ShippedAt = DateTimeOffset.UtcNow; }
        if (nextStatus == ShipmentStatus.Delivered) DeliveredAt = DateTimeOffset.UtcNow;
        Status = nextStatus;
    }
}

public enum ShipmentStatus { Pending = 1, Picking = 2, Packed = 3, Shipped = 4, InTransit = 5, Delivered = 6, Cancelled = 7 }
