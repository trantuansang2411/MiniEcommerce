using ApplicationCore.Entities.Warehousing;

namespace ApplicationCore.Interfaces.Warehousing;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse?> GetByCodeAsync(string code);
    Task<IReadOnlyList<Warehouse>> GetAllAsync();
}
