using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests.Shipping;

public sealed class UpsertShippingProfileRequest
{
    [Required(ErrorMessage = "Recipient name is required.")]
    [StringLength(100, ErrorMessage = "Recipient name cannot exceed 100 characters.")]
    public string RecipientName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Recipient phone is required.")]
    [StringLength(30, ErrorMessage = "Recipient phone cannot exceed 30 characters.")]
    public string RecipientPhone { get; init; } = string.Empty;

    [Required(ErrorMessage = "Shipping address is required.")]
    [StringLength(500, ErrorMessage = "Shipping address cannot exceed 500 characters.")]
    public string ShippingAddress { get; init; } = string.Empty;

    public bool IsDefault { get; init; }
}
