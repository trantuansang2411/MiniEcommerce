using ApplicationCore.Entities.Shipping;
using ApplicationCore.Interfaces.Shipping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.Shipping;

public sealed class ShippingProfileRepository : EfRepository<ShippingProfile>, IShippingProfileRepository
{
    public ShippingProfileRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ShippingProfile>> GetByUserIdAsync(Guid userId) =>
        await _context.ShippingProfiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .OrderByDescending(profile => profile.IsDefault)
            .ThenByDescending(profile => profile.UpdatedAt)
            .ToListAsync();

    public Task<ShippingProfile?> GetByIdForUserAsync(Guid id, Guid userId) =>
        _context.ShippingProfiles.SingleOrDefaultAsync(profile => profile.Id == id && profile.UserId == userId);

    public async Task ClearDefaultForUserAsync(Guid userId, Guid? exceptProfileId = null)
    {
        var profiles = await _context.ShippingProfiles
            .Where(profile => profile.UserId == userId && profile.IsDefault && profile.Id != exceptProfileId)
            .ToListAsync();

        foreach (var profile in profiles)
            profile.SetDefault(false);
    }
}
