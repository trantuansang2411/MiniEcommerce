using ApplicationCore.Entities.Fulfillment;
using ApplicationCore.Interfaces.Fulfillment;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.Fulfillment;

public sealed class ShipmentRepository : EfRepository<Shipment>, IShipmentRepository
{
    public ShipmentRepository(AppDbContext context) : base(context) { }

    private IQueryable<Shipment> Detail() => _context.Shipments.Include(x => x.Items);

    private IQueryable<Shipment> DetailWithOrderAndProducts() => _context.Shipments
        .Include(x => x.Order)
        .Include(x => x.Items)
            .ThenInclude(x => x.Product);

    public async Task<IReadOnlyList<Shipment>> GetAllAsync(ShipmentStatus? status) =>
        await Detail().AsNoTracking().Where(x => !status.HasValue || x.Status == status).OrderBy(x => x.Status).ThenBy(x => x.CreatedAt).ToListAsync();

    public Task<Shipment?> GetDetailAsync(Guid id) =>
        DetailWithOrderAndProducts().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

    public async Task<IReadOnlyList<Shipment>> GetByOrderForUserAsync(Guid orderId, Guid userId) =>
        await Detail().AsNoTracking().Where(x => x.OrderId == orderId && x.Order.UserId == userId).ToListAsync();

    public Task<Shipment?> GetForUpdateAsync(Guid id) =>
        Detail().Include(x => x.Order).SingleOrDefaultAsync(x => x.Id == id);

    public Task<bool> HasOtherUndeliveredAsync(Guid orderId, Guid shipmentId) =>
        _context.Shipments.AnyAsync(x => x.OrderId == orderId && x.Id != shipmentId && x.Status != ShipmentStatus.Delivered);
}
