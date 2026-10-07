namespace ApplicationCore.Entities;

public class OtpVerification
{
	public Guid Id { get; private set; } = Guid.NewGuid();

	public Guid UserId { get; private set; }

	public User User { get; private set; } = null!;

	public string OtpHash { get; private set; } = string.Empty;

	public OtpPurpose Purpose { get; private set; }

	public int AttemptCount { get; private set; }

	public DateTimeOffset CreatedAt { get; private set; }
		= DateTimeOffset.UtcNow;

	public DateTimeOffset ExpiresAt { get; private set; }

	public DateTimeOffset? UsedAt { get; private set; }

	private OtpVerification() { }

	public OtpVerification(
		Guid userId,
		string otpHash,
		OtpPurpose purpose,
		DateTimeOffset expiresAt)
	{
		if (userId == Guid.Empty)
			throw new ArgumentException("UserId is required.", nameof(userId));

		if (string.IsNullOrWhiteSpace(otpHash))
			throw new ArgumentException("OTP hash is required.", nameof(otpHash));

		if (expiresAt <= DateTimeOffset.UtcNow)
			throw new ArgumentException(
				"OTP expiration must be in the future.",
				nameof(expiresAt));

		UserId = userId;
		OtpHash = otpHash;
		Purpose = purpose;
		ExpiresAt = expiresAt;
	}

	public bool IsActive =>
		UsedAt is null &&
		ExpiresAt > DateTimeOffset.UtcNow;

	public void MarkUsed()
	{
		if (!IsActive)
			throw new InvalidOperationException("OTP is no longer active.");

		UsedAt = DateTimeOffset.UtcNow;
	}

	public void IncreaseAttempt()
	{
		if (!IsActive)
			throw new InvalidOperationException("OTP is no longer active.");

		AttemptCount++;
	}
}

public enum OtpPurpose
{
	EmailVerification = 1,
	PasswordReset = 2
}
