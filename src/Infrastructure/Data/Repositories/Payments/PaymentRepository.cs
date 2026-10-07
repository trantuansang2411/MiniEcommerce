using ApplicationCore.Entities.Payments;
using ApplicationCore.Interfaces.Payments;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.Payments;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context) => _context = context;

    public Task<Payment?> GetWithOrderAsync(Guid paymentId) =>
        _context.Payments.Include(x => x.Order).SingleOrDefaultAsync(x => x.Id == paymentId);

    public Task<Payment?> GetWithOrderForUserAsync(Guid paymentId, Guid userId) =>
        _context.Payments.AsNoTracking().Include(x => x.Order)
            .SingleOrDefaultAsync(x => x.Id == paymentId && x.Order.UserId == userId);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
