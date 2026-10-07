using ApplicationCore.Entities.Shipping;

namespace ApplicationCore.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    public string Email { get; private set; } = string.Empty;

    public string? PasswordHash { get; private set; }

    public string? GoogleId { get; private set; }

    public bool IsEmailVerified { get; private set; }

    public UserStatus Status { get; private set; } = UserStatus.PendingVerification;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public ICollection<RefreshToken> RefreshTokens { get; private set; }
        = new List<RefreshToken>();

    public ICollection<OtpVerification> OtpVerifications { get; private set; }
        = new List<OtpVerification>();

    public ICollection<ShippingProfile> ShippingProfiles { get; private set; }
        = new List<ShippingProfile>();

    private User() { }

    public User(
        Role role,
        string email,
        string? passwordHash = null,
        string? googleId = null)
    {
        ArgumentNullException.ThrowIfNull(role);

        if (role.Id == Guid.Empty)
            throw new ArgumentException("RoleId is required.", nameof(role));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        Role = role;
        RoleId = role.Id;
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        GoogleId = googleId;
    }
    public void ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Mật khẩu không hợp lệ");
        PasswordHash = passwordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        Status = UserStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public enum UserStatus
{
    PendingVerification = 1,
    Active = 2,
    Locked = 3,
    Disabled = 4
}
