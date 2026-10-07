using ApplicationCore.Entities.Shipping;

namespace ApplicationCore.Interfaces.Shipping;

public interface IShippingProfileService
{
    Task<IReadOnlyList<ShippingProfile>> GetMyProfilesAsync(Guid userId);
    Task<ShippingProfile> CreateAsync(Guid userId, string recipientName, string recipientPhone, string shippingAddress, bool isDefault);
    Task<ShippingProfile> UpdateAsync(Guid userId, Guid profileId, string recipientName, string recipientPhone, string shippingAddress, bool isDefault);
    Task SetDefaultAsync(Guid userId, Guid profileId);
    Task DeleteAsync(Guid userId, Guid profileId);
}
