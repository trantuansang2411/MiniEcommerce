using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public sealed class OtpVerificationRepository : EfRepository<OtpVerification>, IOtpVerificationRepository
{
    public OtpVerificationRepository(AppDbContext context) : base(context)
    {
    }

    public Task<OtpVerification?> GetLatestAsync(Guid userId, OtpPurpose purpose)
    {
        return _context.OtpVerifications
            .Where(x => x.UserId == userId && x.Purpose == purpose)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
