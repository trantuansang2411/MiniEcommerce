using ApplicationCore.Entities.Orders;

namespace ApplicationCore.Entities.Warehousing;

public class InventoryReservation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid InventoryId { get; private set; }
    public Inventory Inventory { get; private set; } = null!;
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public Guid OrderItemId { get; private set; }
    public OrderItem OrderItem { get; private set; } = null!;
    public int Quantity { get; private set; }
    public InventoryReservationStatus Status { get; private set; } = InventoryReservationStatus.Active;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ReleasedAt { get; private set; }

    private InventoryReservation() { }

    public InventoryReservation(Guid inventoryId, Guid orderId, Guid orderItemId, int quantity, DateTimeOffset expiresAt)
    {
        if (inventoryId == Guid.Empty || orderId == Guid.Empty || orderItemId == Guid.Empty)
            throw new ArgumentException("Reservation references are required.");
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        InventoryId = inventoryId;
        OrderId = orderId;
        OrderItemId = orderItemId;
        Quantity = quantity;
        ExpiresAt = expiresAt;
    }

    public void Consume() { if (Status != InventoryReservationStatus.Active) throw new InvalidOperationException("Only an active reservation can be consumed."); Status = InventoryReservationStatus.Consumed; ReleasedAt = DateTimeOffset.UtcNow; }

    public void Release()
    {
        if (Status != InventoryReservationStatus.Active)
            throw new InvalidOperationException("Only an active reservation can be released.");

        Status = InventoryReservationStatus.Released;
        ReleasedAt = DateTimeOffset.UtcNow;
    }
}

public enum InventoryReservationStatus
{
    Active = 1,
    Expired = 2,
    Released = 3,
    Consumed = 4
}
