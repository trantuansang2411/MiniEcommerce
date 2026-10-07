using ApplicationCore.Entities.Shipping;

namespace ApplicationCore.Interfaces.Shipping;

public interface IShippingProfileRepository : IRepository<ShippingProfile>
{
    Task<IReadOnlyList<ShippingProfile>> GetByUserIdAsync(Guid userId);
    Task<ShippingProfile?> GetByIdForUserAsync(Guid id, Guid userId);
    Task ClearDefaultForUserAsync(Guid userId, Guid? exceptProfileId = null);
}
