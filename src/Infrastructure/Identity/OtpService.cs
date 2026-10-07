using ApplicationCore.Entities;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace Infrastructure.Identity;

public sealed class OtpService : IOtpService
{
    private const int MaxAttempts = 5;
    private readonly IOtpVerificationRepository _otpVerificationRepository;
    private readonly IHashPassword _hashPassword;
    private readonly IEmailSender _emailSender;
    private readonly TimeSpan _lifetime;

    public OtpService(
        IOtpVerificationRepository otpVerificationRepository,
        IHashPassword hashPassword,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _otpVerificationRepository = otpVerificationRepository;
        _hashPassword = hashPassword;
        _emailSender = emailSender;

        var expirationMinutes = configuration.GetValue<int?>("Otp:ExpirationMinutes") ?? 5;
        if (expirationMinutes <= 0)
            throw new InvalidOperationException("OTP expiration must be greater than zero.");

        _lifetime = TimeSpan.FromMinutes(expirationMinutes);
    }

    public async Task SendAsync(User user, OtpPurpose purpose)
    {
        ArgumentNullException.ThrowIfNull(user);

        var rawOtp = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        var otp = new OtpVerification(
            user.Id,
            _hashPassword.Hash(rawOtp),
            purpose,
            DateTimeOffset.UtcNow.Add(_lifetime));

        _otpVerificationRepository.Add(otp);
        await _otpVerificationRepository.SaveChangesAsync();

        var (subject, htmlBody) = CreateEmailContent(purpose, rawOtp);
        await _emailSender.SendAsync(user.Email, subject, htmlBody);
    }

    public async Task VerifyAsync(Guid userId, string code, OtpPurpose purpose)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User Id is required.", nameof(userId));

        var otp = await _otpVerificationRepository.GetLatestAsync(userId, purpose);
        if (otp is null || !otp.IsActive || otp.AttemptCount >= MaxAttempts)
            throw new BadRequestException("Verification code is invalid or expired.");

        if (!_hashPassword.Verify(code, otp.OtpHash))
        {
            otp.IncreaseAttempt();
            _otpVerificationRepository.Update(otp);
            await _otpVerificationRepository.SaveChangesAsync();
            throw new BadRequestException("Verification code is invalid or expired.");
        }

        otp.MarkUsed();
        _otpVerificationRepository.Update(otp);
        await _otpVerificationRepository.SaveChangesAsync();
    }

    private (string Subject, string HtmlBody) CreateEmailContent(OtpPurpose purpose, string rawOtp)
    {
        var action = purpose switch
        {
            OtpPurpose.EmailVerification => "verify your email address",
            OtpPurpose.PasswordReset => "reset your password",
            _ => throw new ArgumentOutOfRangeException(nameof(purpose))
        };

        return (
            $"MiniProject: {action}",
            $"<p>Your verification code is <strong>{rawOtp}</strong>.</p><p>This code expires in {(int)_lifetime.TotalMinutes} minutes.</p>");
    }
}
