using ApplicationCore.Entities.Shipping;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Shipping;

namespace ApplicationCore.Services.Shipping;

public sealed class ShippingProfileService : IShippingProfileService
{
    private readonly IShippingProfileRepository _shippingProfileRepository;

    public ShippingProfileService(IShippingProfileRepository shippingProfileRepository)
    {
        _shippingProfileRepository = shippingProfileRepository;
    }

    public Task<IReadOnlyList<ShippingProfile>> GetMyProfilesAsync(Guid userId)
    {
        EnsureUserId(userId);
        return _shippingProfileRepository.GetByUserIdAsync(userId);
    }

    public async Task<ShippingProfile> CreateAsync(Guid userId, string recipientName, string recipientPhone, string shippingAddress, bool isDefault)
    {
        EnsureUserId(userId);
        var profile = new ShippingProfile(userId, recipientName, recipientPhone, shippingAddress, isDefault);
        if (isDefault)
            await _shippingProfileRepository.ClearDefaultForUserAsync(userId);

        _shippingProfileRepository.Add(profile);
        await _shippingProfileRepository.SaveChangesAsync();
        return profile;
    }

    public async Task<ShippingProfile> UpdateAsync(Guid userId, Guid profileId, string recipientName, string recipientPhone, string shippingAddress, bool isDefault)
    {
        var profile = await GetRequiredAsync(userId, profileId);
        if (isDefault)
            await _shippingProfileRepository.ClearDefaultForUserAsync(userId, profileId);

        profile.Update(recipientName, recipientPhone, shippingAddress, isDefault);
        await _shippingProfileRepository.SaveChangesAsync();
        return profile;
    }

    public async Task SetDefaultAsync(Guid userId, Guid profileId)
    {
        var profile = await GetRequiredAsync(userId, profileId);
        await _shippingProfileRepository.ClearDefaultForUserAsync(userId, profileId);
        profile.SetDefault(true);
        await _shippingProfileRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId, Guid profileId)
    {
        var profile = await GetRequiredAsync(userId, profileId);
        _shippingProfileRepository.Delete(profile);
        await _shippingProfileRepository.SaveChangesAsync();
    }

    private async Task<ShippingProfile> GetRequiredAsync(Guid userId, Guid profileId)
    {
        EnsureUserId(userId);
        if (profileId == Guid.Empty)
            throw new BadRequestException("Shipping profile id is required.");

        return await _shippingProfileRepository.GetByIdForUserAsync(profileId, userId)
            ?? throw new NotFoundException("Shipping profile not found.");
    }

    private static void EnsureUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("User is not authenticated.");
    }
}
