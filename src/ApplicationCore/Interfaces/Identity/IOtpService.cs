using ApplicationCore.Entities;
namespace ApplicationCore.Interfaces;

public interface IOtpService
{
    Task SendAsync(User user, OtpPurpose purpose);
    Task VerifyAsync(Guid userId, string code, OtpPurpose purpose);
}
