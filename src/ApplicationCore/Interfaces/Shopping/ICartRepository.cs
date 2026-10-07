using ApplicationCore.Entities.Shopping;

namespace ApplicationCore.Interfaces.Shopping;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdWithItemsAsync(Guid userId);
}
