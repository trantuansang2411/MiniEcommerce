using ApplicationCore.Entities.Warehousing;

namespace ApplicationCore.Interfaces.Warehousing;

public interface IInventoryService
{
    Task<IReadOnlyList<Inventory>> GetAllAsync(Guid? warehouseId);
    Task<IReadOnlyList<InventoryTransaction>> GetTransactionsAsync(Guid inventoryId);
    Task<Inventory> StockInAsync(Guid managerId, Guid warehouseId, Guid productId, int quantity);
    Task<Inventory> AdjustAsync(Guid managerId, Guid warehouseId, Guid productId, int quantityChange, InventoryAdjustmentReason reason);
}
