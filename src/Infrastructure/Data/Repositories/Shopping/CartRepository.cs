using ApplicationCore.Entities.Shopping;
using ApplicationCore.Interfaces.Shopping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.Shopping;

public sealed class CartRepository : EfRepository<Cart>, ICartRepository
{
    public CartRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Cart?> GetByUserIdWithItemsAsync(Guid userId)
    {
        return _context.Carts
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.Category)
            .SingleOrDefaultAsync(x => x.UserId == userId);
    }

}
