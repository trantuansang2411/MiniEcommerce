using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests.Orders;

public sealed class CheckoutRequest
{
    public Guid? ShippingProfileId { get; init; }

    [StringLength(100, ErrorMessage = "Recipient name cannot exceed 100 characters.")]
    public string? RecipientName { get; init; }

    [StringLength(30, ErrorMessage = "Recipient phone cannot exceed 30 characters.")]
    public string? RecipientPhone { get; init; }

    [StringLength(500, ErrorMessage = "Shipping address cannot exceed 500 characters.")]
    public string? ShippingAddress { get; init; }

    [StringLength(500, ErrorMessage = "Delivery note cannot exceed 500 characters.")]
    public string? DeliveryNote { get; init; }

    public bool SaveAsProfile { get; init; }
    public bool SetAsDefault { get; init; }
}
