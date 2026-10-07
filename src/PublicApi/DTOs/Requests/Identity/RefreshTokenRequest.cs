using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests;

public sealed class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token không được để trống.")]
    public string RefreshToken { get; init; } = string.Empty;
}
