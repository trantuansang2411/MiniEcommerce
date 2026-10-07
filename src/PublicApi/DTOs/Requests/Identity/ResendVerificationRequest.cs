using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests;

public sealed class ResendVerificationRequest
{
    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; init; } = string.Empty;
}
