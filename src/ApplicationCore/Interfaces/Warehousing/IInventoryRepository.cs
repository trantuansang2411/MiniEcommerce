using ApplicationCore.Entities.Warehousing;

namespace ApplicationCore.Interfaces.Warehousing;

public interface IInventoryRepository : IRepository<Inventory>
{
    Task<Inventory?> GetByWarehouseAndProductAsync(Guid warehouseId, Guid productId);
    Task<Inventory?> GetByWarehouseAndProductForReadAsync(Guid warehouseId, Guid productId);
    Task<IReadOnlyList<Inventory>> GetAllAsync(Guid? warehouseId);
    Task<IReadOnlyList<InventoryTransaction>> GetTransactionsAsync(Guid inventoryId);
    Task<Inventory?> GetAvailableForProductAsync(Guid productId, int quantity);
    Task<int> GetTotalAvailableForProductAsync(Guid productId);
    Task<IReadOnlyDictionary<Guid, int>> GetTotalAvailableForProductsAsync(IEnumerable<Guid> productIds);
}
