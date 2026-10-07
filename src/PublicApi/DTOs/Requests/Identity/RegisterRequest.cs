using System.ComponentModel.DataAnnotations;
namespace PublicApi.DTOs.Requests;
public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Email không được để trống"), EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; init; } = string.Empty;
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])\S{8,}$",
        ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ thường, chữ hoa, số và ký tự đặc biệt."
    )]
    public string Password { get; init; } = string.Empty;

}

