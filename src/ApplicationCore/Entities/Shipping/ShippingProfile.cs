namespace ApplicationCore.Entities.Shipping;

public class ShippingProfile
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public string RecipientName { get; private set; } = string.Empty;

    public string RecipientPhone { get; private set; } = string.Empty;

    public string ShippingAddress { get; private set; } = string.Empty;

    public bool IsDefault { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private ShippingProfile() { }

    public ShippingProfile(Guid userId, string recipientName, string recipientPhone, string shippingAddress, bool isDefault = false)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        UserId = userId;
        ApplyDetails(recipientName, recipientPhone, shippingAddress);
        IsDefault = isDefault;
    }

    public void Update(string recipientName, string recipientPhone, string shippingAddress, bool isDefault)
    {
        ApplyDetails(recipientName, recipientPhone, shippingAddress);
        IsDefault = isDefault;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void ApplyDetails(string recipientName, string recipientPhone, string shippingAddress)
    {
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new ArgumentException("Recipient name is required.", nameof(recipientName));
        if (string.IsNullOrWhiteSpace(recipientPhone))
            throw new ArgumentException("Recipient phone is required.", nameof(recipientPhone));
        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address is required.", nameof(shippingAddress));

        RecipientName = recipientName.Trim();
        RecipientPhone = recipientPhone.Trim();
        ShippingAddress = shippingAddress.Trim();
    }
}
