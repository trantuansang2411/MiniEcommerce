using ApplicationCore.Entities.Catalog;
using ApplicationCore.Entities.Orders;

namespace ApplicationCore.Entities.Fulfillment;

public class ShipmentItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ShipmentId { get; private set; }
    public Shipment Shipment { get; private set; } = null!;
    public Guid OrderItemId { get; private set; }
    public OrderItem OrderItem { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    private ShipmentItem() { }
    public ShipmentItem(Guid shipmentId, Guid orderItemId, Guid productId, int quantity)
    {
        if (shipmentId == Guid.Empty || orderItemId == Guid.Empty || productId == Guid.Empty || quantity <= 0)
            throw new ArgumentException("Shipment item data is invalid.");
        ShipmentId = shipmentId; OrderItemId = orderItemId; ProductId = productId; Quantity = quantity;
    }
}
