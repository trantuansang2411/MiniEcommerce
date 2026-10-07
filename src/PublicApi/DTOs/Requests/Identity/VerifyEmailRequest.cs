using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests;

public sealed class VerifyEmailRequest
{
    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mã xác thực không được để trống.")]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "Mã xác thực phải gồm đúng 6 chữ số.")]
    public string Code { get; init; } = string.Empty;
}
