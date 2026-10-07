using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces;

public interface IRefreshTokenService
{
    Task<string> CreateAsync(Guid userId);

    Task<(User User, string RefreshToken)> RotateAsync(string refreshToken);

    Task RevokeAsync(string refreshToken);
}
