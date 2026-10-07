using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Interfaces.Warehousing;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.Warehousing;

public sealed class WarehouseRepository : EfRepository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Warehouse?> GetByCodeAsync(string code)
    {
        return _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code);
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync()
    {
        return await _context.Warehouses
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}
