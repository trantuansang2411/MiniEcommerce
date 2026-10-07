using ApplicationCore.Entities;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services;

public sealed class UserService : IUserService
{
    private const string DefaultRoleName = "User";
    private readonly IUserRepository _userRepository;
    private readonly IHashPassword _hashPassword;
    private readonly IRoleRepository _roleRepository;
    private readonly IOtpService _otpService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IHashPassword hashPassword,
        IRoleRepository roleRepository,
        IOtpService otpService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _hashPassword = hashPassword;
        _roleRepository = roleRepository;
        _otpService = otpService;
        _logger = logger;
    }

    public Task<User?> GetByIdAsync(Guid id) => _userRepository.GetByIdAsync(id);

    public async Task<User> RegisterAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await _userRepository.GetByEmailAsync(normalizedEmail) is not null)
            throw new ConflictException("Email is already registered.");

        var role = await _roleRepository.GetRoleByNameAsync(DefaultRoleName)
        ?? throw new InvalidOperationException("Default role User is not configured.");
        var user = new User(role, normalizedEmail, _hashPassword.Hash(password));
        _userRepository.Add(user);
        await _userRepository.SaveChangesAsync();
        await _otpService.SendAsync(user, OtpPurpose.EmailVerification);
        _logger.LogInformation("User {UserId} registered successfully", user.Id);
        return user;
    }

    public async Task<User> VerifyEmailAsync(string email, string code)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailWithRoleAsync(normalizedEmail)
            ?? throw new BadRequestException("Invalid email or verification code.");

        if (user.IsEmailVerified)
            throw new BadRequestException("Email has already been verified.");

        await _otpService.VerifyAsync(user.Id, code, OtpPurpose.EmailVerification);
        user.VerifyEmail();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return user;
    }

    public async Task SendEmailVerificationAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        // Do not reveal whether an email address is registered.
        if (user is null || user.IsEmailVerified)
            return;

        await _otpService.SendAsync(user, OtpPurpose.EmailVerification);
    }

    public async Task<User> LoginAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailWithRoleAsync(normalizedEmail);
        if (user?.PasswordHash is null || !_hashPassword.Verify(password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsEmailVerified || user.Status != UserStatus.Active)
            throw new ForbiddenException("Email verification is required before logging in.");

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);
        return user;
    }
    // TODO: Require OTP verification before allowing password reset.
    public async Task ChangePasswordAsync(Guid id, string oldPassword,string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("User not found.");
        if(user?.PasswordHash is null || !_hashPassword.Verify(oldPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid password");
        }
        user.ChangePassword(_hashPassword.Hash(newPassword));
        await _userRepository.SaveChangesAsync();
        _logger.LogInformation("User {UserId} changed password successfully",id);
    }
    public async Task RequestPasswordResetAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        // Do not reveal whether an email address is registered.
        if (user is null)
            return;

        await _otpService.SendAsync(user, OtpPurpose.PasswordReset);
    }

    public async Task ResetPasswordAsync(string email, string code, string newPassword)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail)
            ?? throw new BadRequestException("Invalid email or verification code.");

        await _otpService.VerifyAsync(user.Id, code, OtpPurpose.PasswordReset);
        user.ChangePassword(_hashPassword.Hash(newPassword));
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("User {UserId} reset password successfully", user.Id);
    }
}
