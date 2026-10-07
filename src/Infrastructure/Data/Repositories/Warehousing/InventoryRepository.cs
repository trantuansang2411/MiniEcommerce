using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Interfaces.Warehousing;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.Warehousing;

public sealed class InventoryRepository : EfRepository<Inventory>, IInventoryRepository
{
    public InventoryRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Inventory?> GetByWarehouseAndProductAsync(Guid warehouseId, Guid productId)
    {
        return _context.Inventories
            .SingleOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ProductId == productId);
    }

    public Task<Inventory?> GetByWarehouseAndProductForReadAsync(Guid warehouseId, Guid productId)
    {
        return _context.Inventories
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Product)
            .SingleOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ProductId == productId);
    }

    public async Task<IReadOnlyList<Inventory>> GetAllAsync(Guid? warehouseId)
    {
        var query = _context.Inventories
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Product)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(x => x.WarehouseId == warehouseId.Value);

        return await query
            .OrderBy(x => x.Warehouse.Name)
            .ThenBy(x => x.Product.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<InventoryTransaction>> GetTransactionsAsync(Guid inventoryId)
    {
        return await _context.Set<InventoryTransaction>()
            .AsNoTracking()
            .Where(x => x.InventoryId == inventoryId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public Task<Inventory?> GetAvailableForProductAsync(Guid productId, int quantity)
    {
        return _context.Inventories
            .Include(x => x.Warehouse)
            .Where(x => x.ProductId == productId && x.Warehouse.Status == WarehouseStatus.Active && x.OnHand - x.Reserved >= quantity)
            .OrderBy(x => x.Warehouse.Code)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetTotalAvailableForProductAsync(Guid productId)
    {
        return await _context.Inventories
            .Where(x => x.ProductId == productId && x.Warehouse.Status == WarehouseStatus.Active)
            .Select(x => (int?)(x.OnHand - x.Reserved))
            .SumAsync() ?? 0;
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetTotalAvailableForProductsAsync(IEnumerable<Guid> productIds)
    {
        var ids = productIds.Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<Guid, int>();

        return await _context.Inventories
            .Where(x => ids.Contains(x.ProductId) && x.Warehouse.Status == WarehouseStatus.Active)
            .GroupBy(x => x.ProductId)
            .Select(group => new { ProductId = group.Key, AvailableStock = group.Sum(x => x.OnHand - x.Reserved) })
            .ToDictionaryAsync(x => x.ProductId, x => x.AvailableStock);
    }
}
