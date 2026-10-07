using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests;
public sealed class ChangePasswordReq
{
    [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống.")]
    public string OldPassword { get; init; } = string.Empty;
    [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])\S{8,}$",
        ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ thường, chữ hoa, số và ký tự đặc biệt."
    )]
    public string NewPassword { get; init; } = string.Empty;
}
