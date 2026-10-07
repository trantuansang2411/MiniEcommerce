namespace ApplicationCore.Services.Orders;

public sealed record CheckoutShippingInfo(
    Guid? ShippingProfileId,
    string? RecipientName,
    string? RecipientPhone,
    string? ShippingAddress,
    string? DeliveryNote,
    bool SaveAsProfile,
    bool SetAsDefault);
