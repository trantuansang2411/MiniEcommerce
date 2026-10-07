using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashWithUserAndRoleAsync(string tokenHash);
}
