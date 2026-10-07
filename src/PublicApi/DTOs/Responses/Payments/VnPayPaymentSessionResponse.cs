namespace PublicApi.DTOs.Responses.Payments;

public sealed class VnPayPaymentSessionResponse
{
    public Guid PaymentId { get; init; }
    public string PaymentUrl { get; init; } = string.Empty;
}
