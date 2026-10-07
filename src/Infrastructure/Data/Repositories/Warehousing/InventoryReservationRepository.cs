using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Interfaces.Warehousing;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.Warehousing;

public sealed class InventoryReservationRepository : EfRepository<InventoryReservation>, IInventoryReservationRepository
{
    public InventoryReservationRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<InventoryReservation>> GetActiveByOrderIdAsync(Guid orderId) =>
        await _context.Set<InventoryReservation>()
            .Include(x => x.Inventory)
            .Where(x => x.OrderId == orderId && x.Status == InventoryReservationStatus.Active)
            .ToListAsync();
}
