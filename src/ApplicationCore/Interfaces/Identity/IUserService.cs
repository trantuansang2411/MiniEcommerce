using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces;
public interface IUserService
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User> RegisterAsync(string email, string password);
    Task<User> VerifyEmailAsync(string email, string code);
    Task SendEmailVerificationAsync(string email);
    Task<User> LoginAsync(string email, string password);
    Task ChangePasswordAsync(Guid id, string oldPassword, string newPassword);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(string email, string code, string newPassword);
}

