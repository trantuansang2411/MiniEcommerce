using ApplicationCore.Entities.Payments;

namespace ApplicationCore.Interfaces.Payments;

public interface IPaymentService
{
    Task<Payment> PayOrderManuallyAsync(Guid userId, Guid orderId);
}
