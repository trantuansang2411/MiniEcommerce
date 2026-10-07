using ApplicationCore.Entities;

namespace ApplicationCore.Entities.Warehousing;

public class InventoryTransaction
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid InventoryId { get; private set; }
    public Inventory Inventory { get; private set; } = null!;
    public InventoryTransactionType Type { get; private set; }
    public int QuantityChange { get; private set; }
    public InventoryAdjustmentReason? Reason { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private InventoryTransaction() { }

    public InventoryTransaction(
        Guid inventoryId,
        InventoryTransactionType type,
        int quantityChange,
        Guid createdByUserId,
        InventoryAdjustmentReason? reason = null)
    {
        if (inventoryId == Guid.Empty)
            throw new ArgumentException("InventoryId is required.", nameof(inventoryId));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId is required.", nameof(createdByUserId));

        if (quantityChange == 0)
            throw new ArgumentOutOfRangeException(nameof(quantityChange));

        if (type == InventoryTransactionType.StockIn && quantityChange < 0)
            throw new ArgumentException("Stock-in quantity must be positive.", nameof(quantityChange));

        if (type == InventoryTransactionType.Adjustment && reason is null)
            throw new ArgumentException("Adjustment reason is required.", nameof(reason));

        InventoryId = inventoryId;
        Type = type;
        QuantityChange = quantityChange;
        CreatedByUserId = createdByUserId;
        Reason = reason;
    }
}

public enum InventoryTransactionType
{
    StockIn = 1,
    Adjustment = 2,
    Reservation = 3,
    ReservationRelease = 4,
    StockOut = 5
}

public enum InventoryAdjustmentReason
{
    Damaged = 1,
    Lost = 2,
    CountMismatch = 3,
    Expired = 4,
    ReturnToSupplier = 5,
    Correction = 6
}
