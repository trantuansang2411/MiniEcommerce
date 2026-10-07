using ApplicationCore.Entities.Warehousing;

namespace ApplicationCore.Interfaces.Warehousing;

public interface IWarehouseService
{
    Task<IReadOnlyList<Warehouse>> GetAllAsync();
    Task<Warehouse> GetByIdAsync(Guid id);
    Task<Warehouse> CreateAsync(string code, string name, string address);
    Task<Warehouse> UpdateAsync(Guid id, string? name, string? address, WarehouseStatus? status);
}
