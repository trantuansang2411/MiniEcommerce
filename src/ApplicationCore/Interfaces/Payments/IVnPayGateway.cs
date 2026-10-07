namespace ApplicationCore.Interfaces.Payments;

public interface IVnPayGateway
{
    string CreatePaymentUrl(VnPayPaymentRequest request);
    bool IsValidSignature(IReadOnlyDictionary<string, string> parameters);
}

public sealed record VnPayPaymentRequest(Guid PaymentId, Guid OrderId, decimal Amount, DateTimeOffset ExpiresAt, string ClientIp);
public sealed record VnPayPaymentSession(Guid PaymentId, string PaymentUrl);
public sealed record VnPayIpnResponse(string RspCode, string Message);
