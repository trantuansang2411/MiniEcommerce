using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Identity;

public sealed class JwtTokenService : IToken
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration) => _configuration = configuration;

    public string CreateAccessToken(User user)
    {
        var settings = _configuration.GetSection("Jwt");
        var signingKey = settings["SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");
        var issuer = settings["Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");
        var audience = settings["Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(
            settings.GetValue<int?>("ExpirationMinutes") ?? 60);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            DateTime.UtcNow,
            expiresAt.UtcDateTime,
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
