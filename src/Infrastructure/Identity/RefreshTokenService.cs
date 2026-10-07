using ApplicationCore.Entities;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Identity;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private const int RefreshTokenSizeInBytes = 64;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly TimeSpan _lifetime;

    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration)
    {
        _refreshTokenRepository = refreshTokenRepository;

        var expirationDays = configuration.GetValue<int?>("RefreshToken:ExpirationDays") ?? 30;
        if (expirationDays <= 0)
            throw new InvalidOperationException("Refresh token expiration must be greater than zero.");

        _lifetime = TimeSpan.FromDays(expirationDays);
    }

    public async Task<string> CreateAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User Id is required.", nameof(userId));

        var rawToken = GenerateRawToken();
        var refreshToken = new RefreshToken(
            userId,
            HashToken(rawToken),
            DateTimeOffset.UtcNow.Add(_lifetime));

        _refreshTokenRepository.Add(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return rawToken;
    }

    public async Task<(User User, string RefreshToken)> RotateAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedException("Refresh token is required.");

        var tokenHash = HashToken(refreshToken);
        var currentToken = await _refreshTokenRepository
            .GetByTokenHashWithUserAndRoleAsync(tokenHash);

        if (currentToken is null || !currentToken.IsActive)
            throw new UnauthorizedException("Refresh token is invalid or expired.");

        var nextRawToken = GenerateRawToken();
        var nextToken = new RefreshToken(
            currentToken.UserId,
            HashToken(nextRawToken),
            DateTimeOffset.UtcNow.Add(_lifetime));

        currentToken.Revoke(nextToken.Id);
        _refreshTokenRepository.Update(currentToken);
        _refreshTokenRepository.Add(nextToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return (currentToken.User, nextRawToken);
    }

    public async Task RevokeAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var currentToken = await _refreshTokenRepository
            .GetByTokenHashWithUserAndRoleAsync(HashToken(refreshToken));

        if (currentToken is null || !currentToken.IsActive)
            return;

        currentToken.Revoke();
        await _refreshTokenRepository.SaveChangesAsync();
    }

    private static string GenerateRawToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(RefreshTokenSizeInBytes);
        return Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }
}
