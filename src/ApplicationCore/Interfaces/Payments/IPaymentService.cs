using ApplicationCore.Entities.Payments;

namespace ApplicationCore.Interfaces.Payments;

public interface IPaymentService
{
    Task<Payment> PayOrderManuallyAsync(Guid userId, Guid orderId);
    Task<VnPayPaymentSession> CreateVnPayPaymentAsync(Guid userId, Guid orderId, string clientIp);
    Task<VnPayIpnResponse> ProcessVnPayIpnAsync(IReadOnlyDictionary<string, string> parameters);
    Task<Payment> GetPaymentForUserAsync(Guid userId, Guid paymentId);
}
