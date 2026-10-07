using ApplicationCore.Entities.Catalog;

namespace ApplicationCore.Entities.Warehousing;

public class Inventory
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid WarehouseId { get; private set; }
    public Warehouse Warehouse { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int OnHand { get; private set; }
    public int Reserved { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public ICollection<InventoryTransaction> Transactions { get; private set; } = new List<InventoryTransaction>();

    private Inventory() { }

    public Inventory(Guid warehouseId, Guid productId)
    {
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("WarehouseId is required.", nameof(warehouseId));

        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.", nameof(productId));

        WarehouseId = warehouseId;
        ProductId = productId;
    }

    public void StockIn(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Stock-in quantity must be greater than zero.");

        OnHand += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Adjust(int quantityChange)
    {
        if (quantityChange == 0)
            throw new ArgumentOutOfRangeException(nameof(quantityChange), "Adjustment quantity cannot be zero.");

        var newOnHand = OnHand + quantityChange;
        if (newOnHand < Reserved)
            throw new InvalidOperationException("Inventory on-hand quantity cannot be less than reserved quantity.");

        OnHand = newOnHand;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        if (OnHand - Reserved < quantity)
            throw new InvalidOperationException("Insufficient available inventory.");

        Reserved += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ConsumeReservation(int quantity)
    {
        if (quantity <= 0 || Reserved < quantity || OnHand < quantity) throw new InvalidOperationException("Cannot consume inventory reservation.");
        Reserved -= quantity; OnHand -= quantity; UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0 || Reserved < quantity)
            throw new InvalidOperationException("Cannot release inventory reservation.");

        Reserved -= quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
