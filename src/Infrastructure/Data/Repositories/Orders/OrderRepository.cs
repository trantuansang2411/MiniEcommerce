using ApplicationCore.Entities.Orders;
using ApplicationCore.Interfaces.Orders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.Orders;

public sealed class OrderRepository : EfRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public Task<Order?> GetByIdForUserAsync(Guid orderId, Guid userId)
    {
        return _context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == orderId && x.UserId == userId);
    }

    public Task<Order?> GetForPaymentByIdForUserAsync(Guid orderId, Guid userId)
    {
        return _context.Orders
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(x => x.Id == orderId && x.UserId == userId);
    }

    public Task<Order?> GetForUpdateByIdForUserAsync(Guid orderId, Guid userId) =>
        _context.Orders.SingleOrDefaultAsync(x => x.Id == orderId && x.UserId == userId);

    public async Task<IReadOnlyList<Order>> GetAllForManagementAsync(OrderStatus? status)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public Task<Order?> GetManagementDetailAsync(Guid orderId) =>
        _context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .Include(x => x.Payments)
            .Include(x => x.Shipments)
                .ThenInclude(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == orderId);
}
