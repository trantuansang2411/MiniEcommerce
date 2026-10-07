namespace Infrastructure.Payments;

public sealed class VnPayOptions
{
    public const string SectionName = "VnPay";
    public string PaymentUrl { get; init; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    public string TmnCode { get; init; } = string.Empty;
    public string HashSecret { get; init; } = string.Empty;
    public string ReturnUrl { get; init; } = "http://localhost:5114/api/payments/vnpay/return";
    public string FrontendUrl { get; init; } = "http://localhost:3000";
}
