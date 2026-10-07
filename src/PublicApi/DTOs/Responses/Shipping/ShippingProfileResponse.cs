namespace PublicApi.DTOs.Responses.Shipping;

public sealed class ShippingProfileResponse
{
    public Guid Id { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientPhone { get; init; } = string.Empty;
    public string ShippingAddress { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
