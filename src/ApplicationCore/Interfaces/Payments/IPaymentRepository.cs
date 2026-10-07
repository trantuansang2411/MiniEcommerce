using ApplicationCore.Entities.Payments;

namespace ApplicationCore.Interfaces.Payments;

public interface IPaymentRepository
{
    Task<Payment?> GetWithOrderAsync(Guid paymentId);
    Task<Payment?> GetWithOrderForUserAsync(Guid paymentId, Guid userId);
    Task<int> SaveChangesAsync();
}
