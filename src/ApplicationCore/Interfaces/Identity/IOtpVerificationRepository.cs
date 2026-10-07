using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces;

public interface IOtpVerificationRepository : IRepository<OtpVerification>
{
    Task<OtpVerification?> GetLatestAsync(Guid userId, OtpPurpose purpose);
}
